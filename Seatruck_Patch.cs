using HarmonyLib;
using System;
using System.Collections.Generic;
using static ErrorMessage;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Seatruck_Patch
    {
        public static bool afterBurnerWasActive = false;
        public static GameObject seaTruckCab = null;
        public static GameObject seaTruckAquarium = null;
        public static int powerUpgrades = 0;
        public static List<GameObject> seatruckModules = new List<GameObject>();

        public static void DoStuff(GameObject seaTruckCab, GameObject seaTruckAquarium)
        {
            if (seaTruckCab == null || seaTruckAquarium == null)
                return;

            WaterClipProxy wcp = seaTruckCab.GetComponentInChildren<WaterClipProxy>();
            if (wcp)
            {
                Main.Log("seaTruckCab wcp " + wcp.name);
                Main.Log("seaTruckCab wcp parent " + wcp.transform.parent.name);
                MeshFilter meshFilter = wcp.gameObject.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = wcp.gameObject.GetComponent<MeshRenderer>();

                if (meshFilter && meshRenderer)
                {
                    AddDebug("add components ");
                    GameObject go = new GameObject("WaterClipProxy");
                    go.transform.SetParent(seaTruckAquarium.transform);
                    Main.CopyComponent(wcp, go);
                    Main.CopyComponent(meshFilter, go);
                    Main.CopyComponent(meshRenderer, go);
                    wcp.waterSurface = WaterSurface.Get();
                    UWE.CoroutineHost.StartCoroutine(wcp.LoadAsync());
                }
            }
        

    }

        //[HarmonyPatch(typeof(SeaTruckSegment), "OnClickHatch")]
        class SeaTruckSegment_OnClickHatch_Patch
        { // to fix: delay exit sound when exiting docking module
            public static bool Prefix(SeaTruckSegment __instance, HandTargetEventData eventData)
            {
                if (__instance.player == null && !__instance.CanEnter())
                    return false;
                if (__instance.player && __instance.player == eventData.guiHand.player)
                {
                    Vector3 exitPoint;
                    bool skipAnimations;
                    if (__instance.FindExitPoint(out exitPoint, out skipAnimations, SeaTruckAnimation.Animation.Exit))
                    {
                        AddDebug(__instance.name + " exitSound ");
                        Utils.PlayFMODAsset(__instance.exitSound, __instance.player.transform);
                        __instance.Exit(new Vector3?(exitPoint), skipAnimations);
                        if (__instance.CanAnimate() && !skipAnimations)
                            __instance.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.Exit;
                    }
                    else
                        AddError(Language.main.Get("ExitFailedNoSpace"));
                }
                else if (eventData.guiHand.player)
                {
                    bool exitPoint = __instance.FindExitPoint(out Vector3 _, out bool _);
                    if (!__instance.IsWalkable() && !SeaTruckSegment.GetHead(__instance).isMainCab)
                        AddError(Language.main.Get("EnterFailedTooSteep"));
                    else if (exitPoint)
                        __instance.EnterHatch(eventData.guiHand.player);
                    else
                        AddError(Language.main.Get("EnterFailedNoSpace"));
                }
                if (__instance.player)
                    __instance.PropagatePlayer();

                return false;
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor), "StopPiloting")]
        class SeaTruckMotor_StopPiloting_Patch
        { // dont play exit sound when not exiting cabin 
            public static bool Prefix(SeaTruckMotor __instance, ref bool __result, bool waitForDocking = false, bool forceStop = false, bool skipUnsubscribe = false)
            {
                //AddDebug("StopPiloting");
                bool flag = false;
                bool playSound = false;
                if (!skipUnsubscribe)
                    __instance.Unsubscribe();
                if (__instance.piloting)
                {
                    if (__instance.truckSegment.isMainCab && __instance.truckSegment.underCreatureAttack)
                    {
                        playSound = true;
                        __instance.truckSegment.Exit();
                        Player.main.ExitLockedMode(findNewPosition: false);
                        if (__instance.seatruckanimation && (!__instance.liveMixin || __instance.liveMixin.IsAlive()))
                            __instance.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.EjectPilot;
                        flag = true;
                    }
                    else if (__instance.truckSegment.isMainCab && (__instance.truckSegment.rearConnection && !__instance.truckSegment.rearConnection.occupied || !__instance.truckSegment.IsWalkable()))
                    {
                        Vector3 exitPoint1;
                        bool skipAnimations;
                        bool exitPoint2 = __instance.truckSegment.FindExitPoint(out exitPoint1, out skipAnimations, SeaTruckAnimation.Animation.ExitPilot);
                        skipAnimations = !__instance.truckSegment.IsWalkable() || skipAnimations;
                        if (!exitPoint2 && !forceStop)
                            AddError(Language.main.Get("ExitFailedNoSpace"));
                        else
                        {
                            Player.main.ExitLockedMode(findNewPosition: false);
                            if (!exitPoint2)
                            {
                                playSound = true;
                                AddError(Language.main.Get("ExitFailedNoSpace"));
                                __instance.truckSegment.Exit();
                            }
                            else
                            {
                                playSound = true;
                                __instance.truckSegment.Exit(new Vector3?(exitPoint1), skipAnimations);
                            }
                            if (!skipAnimations)
                                __instance.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.ExitPilot;
                            else
                            {
                                Player.main.armsController.SetTrigger("seatruck_exit");
                                __instance.animator.SetTrigger("seatruck_exit");
                            }
                            flag = true;
                        }
                    }
                    else
                    {
                        __instance.waitForDocking = waitForDocking;
                        if (!waitForDocking)
                            Player.main.ExitLockedMode(findNewPosition: false);
                        if (__instance.seatruckanimation && (!__instance.liveMixin || __instance.liveMixin.IsAlive()))
                            __instance.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.EndPilot;
                        flag = true;
                    }
                    if (flag)
                    {
                        if (!__instance.truckSegment.isMainCab)
                        {
                            playSound = true;
                            __instance.truckSegment.Exit();
                            InputHandlerStack.main.Pop(__instance.inputStackDummy);
                        }
                        __instance.piloting = false;
                        __instance.UpdateIKEnabledState();
                        Player.main.inSeatruckPilotingChair = false;
                        __instance.SendMessage("OnPilotEnd", null, SendMessageOptions.DontRequireReceiver);

                        if (playSound && __instance.stopPilotSound)
                            Utils.PlayFMODAsset(__instance.stopPilotSound, __instance.transform);
                        __instance.UpdateIKEnabledState();
                    }
                }
                __result = flag;
                return false;
            }
        }

        [HarmonyPatch(typeof(SeaTruckLights), "Start")]
        class SeaTruckLights_Start_Patch
        {
            public static void Prefix(SeaTruckLights __instance)
            {
                //if (__instance.name == "SeaTruckStorageModule(Clone)")
                //{
                    __instance.lightingController = __instance.GetComponent<LightingController>();
                //AddDebug(__instance.name + "SeaTruckStorageModule fix lights " );
                //}
                Light[] lights = Main.GetComponentsInDirectChildren<Light>(__instance, true);
                foreach (Light light in lights)
                {
                    light.enabled = false;
                    //Main.Log(light.name + " turnofflights " + Main.GetParent(__instance.gameObject));
                    //AddDebug(light.name + " turnofflights " + Main.GetParent(__instance.gameObject));
                }
            }
        }

        //[HarmonyPatch(typeof(LightingController), "LerpToState", new Type[] { typeof(int), typeof(float) })]
        class LightingController_LerpToState_Patch
        { // turn off light in teleporter and docking module
            static int prevState = -1;
            public static void Prefix(LightingController __instance, int targetState)
            {
                prevState = (int)__instance.state;
            }
            public static void Postfix(LightingController __instance, int targetState)
            {
                if (prevState == targetState || !__instance.GetComponent<SeaTruckLights>())
                    return;

                Light[] lights = Main.GetComponentsInDirectChildren<Light>(__instance, true);
                if ((LightingController.LightingState)targetState == LightingController.LightingState.Damaged)
                {
                    foreach (Light light in lights)
                        light.enabled = false;
                    //AddDebug(__instance.name + " turn off lights ");
                }
                else if ((LightingController.LightingState)targetState == LightingController.LightingState.Operational)
                {
                    foreach (Light light in lights)
                        light.enabled = true;
                    //AddDebug(__instance.name + " turn on lights ");
                }
            }
        }

        //[HarmonyPatch(typeof(SeaTruckSegment), "Start")]
        class SeaTruckSegment_Start_Patch
        {
            public static void Postfix(SeaTruckSegment __instance)
            {
                if (__instance.isMainCab)
                {
                    seaTruckCab = __instance.gameObject;
                }
                else if (Main.GetComponentsInDirectChildren<SeaTruckAquarium>(__instance).Length > 0)
                {
                    seaTruckAquarium = Main.GetParent(__instance.gameObject);
                    //if (seaTruckCab)
                    //{
   
                    //else
                    //    Main.Log("Aquarium start no seaTruckCab " + __instance.name);
                }


                //seaTruckUpgrades = __instance;
            }
        }

        public static int GetNumPowerUpgrades(SeaTruckUpgrades seaTruckUpgrades)
        {
            int count = 0;
            for (int slotID = 0; slotID < SeaTruckUpgrades.slotIDs.Length; ++slotID)
            {
                TechType tt = seaTruckUpgrades.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
                if (tt == TechType.SeaTruckUpgradeHorsePower)
                    count++;
            }
            //AddDebug("GetNumPowerUpgrades " + count);
            return count;
        }

        [HarmonyPatch(typeof(SeaTruckMotor), "GetWeight")]
        class SeaTruckMotor_GetWeight_Patch
        {
            public static bool Prefix(SeaTruckMotor __instance, ref float __result)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return true;

                __result = __instance.truckSegment.GetWeight() + __instance.truckSegment.GetAttachedWeight() * 0.8f;
                return false;
            }
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades), "IsAllowedToAdd")]
        class SeaTruckUpgrades_IsAllowedToAdd_Patch
        {
            public static bool Prefix(SeaTruckUpgrades __instance, Pickupable pickupable, ref bool __result)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return true;

                if (pickupable == null)
                    return false;
                if (__instance.modules.GetCount(pickupable.GetTechType()) == 0 || __instance.IsEquipped(pickupable))
                    __result = true;
                else if (pickupable.GetTechType() == TechType.SeaTruckUpgradeHorsePower)
                    __result = true;
                else
                {
                    __result = false;
                    AddMessage(Language.main.Get("SeaTruckErrorMultipleTechTypes"));
                }
                return false;
            }
        }

        //[HarmonyPatch(typeof(SeaTruckUpgrades), "OnUpgradeModuleChange")]
        class SeaTruckUpgrades_OnUpgradeModuleChange_Patch
        {// this somehow breaks slot extender mod
            public static void Postfix(SeaTruckUpgrades __instance, TechType techType, bool added)
            {
                //powerUpgrades = GetNumPowerUpgrades(__instance);
                //AddDebug("OnUpgradeModuleChange " + techType + " " + added);
                //AddDebug("powerUpgrades " + powerUpgrades);
            }
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades), "OnEquip")]
        class SeaTruckUpgrades_OnEquip_Patch
        {
            public static void Postfix(SeaTruckUpgrades __instance, string slot, InventoryItem item)
            {
                powerUpgrades = GetNumPowerUpgrades(__instance);
                //AddDebug("OnEquip " + item.item.name + " slot " + slot);
                //AddDebug("powerUpgrades " + powerUpgrades);
            }
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades), "TryActivateAfterBurner")]
        class SeaTruckUpgrades_TryActivateAfterBurner_Patch
        {
            public static bool Prefix(SeaTruckUpgrades __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return true;

                for (int slotID = 0; slotID < SeaTruckUpgrades.slotIDs.Length; ++slotID)
                {
                    TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
                    if (techTypeInSlot == TechType.SeaTruckUpgradeAfterburner)
                    {
                        if (!__instance.ConsumeEnergy(techTypeInSlot))
                            break;
                        __instance.OnUpgradeModuleUse(techTypeInSlot, slotID);
                        break;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor), "Update")]
        class SeaTruckMotor_Update_Patch
        {
            public static void Prefix(SeaTruckMotor __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return;

                if (__instance.afterBurnerActive)
                {
                    //AddDebug("BOOST");
                    //__instance.afterBurnerTime = Time.time + 1f;
                    afterBurnerWasActive = true;
                }
            }
            public static void Postfix(SeaTruckMotor __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return;

                if (GameInput.GetButtonHeld(GameInput.Button.Sprint))
                {
                    if (afterBurnerWasActive || __instance.afterBurnerActive)
                    {
                        //AddDebug("BOOST");
                        __instance.afterBurnerActive = true;
                    }
                    else
                    {
                        afterBurnerWasActive = false;
                        __instance.afterBurnerActive = false;
                    }
                }
                else
                {
                    afterBurnerWasActive = false;
                    __instance.afterBurnerActive = false;
                }
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor), "FixedUpdate")]
        class SeaTruckMotor_FixedUpdate_Patch
        {
            public static bool Prefix(SeaTruckMotor __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return true;

                if (__instance.transform.position.y < Ocean.GetOceanLevel() && __instance.useRigidbody != null && (__instance.IsPowered() && !__instance.IsBusyAnimating()))
                {
                    if (__instance.piloting)
                    {
                        Vector3 input = AvatarInputHandler.main.IsEnabled() || __instance.inputStackDummy.activeInHierarchy ? GameInput.GetMoveDirection() : Vector3.zero;
                        input = input.normalized;
                        input.x *= .5f;
                        input.y *= .5f;
                        if (input.z < 0)
                            input.z *= .5f;

                        Int2 int2;
                        int2.x = input.x <= 0.0 ? (input.x >= 0.0 ? 0 : -1) : 1;
                        int2.y = input.z <= 0.0 ? (input.z >= 0.0 ? 0 : -1) : 1;
                        __instance.leverDirection = int2;
                        if (__instance.afterBurnerActive)
                            input.z = 1f;

                        float powerBonus = 1 + powerUpgrades * .1f;
                        if (__instance.relay == null) // player pushing module
                            powerBonus = 1f;
                        Vector3 vector3_2 = MainCameraControl.main.rotation * input;
                        float acceleration = 1f / Mathf.Max(1f, __instance.GetWeight() * 0.35f) * __instance.acceleration * powerBonus;
                        if (__instance.afterBurnerActive)
                        {
                            //AddDebug("afterBurnerActive " + __instance.afterBurnerActive);
                            acceleration += 7f;
                        }
                        //AddDebug("__instance.relay " + __instance.relay);

                        __instance.useRigidbody.AddForce(acceleration * vector3_2, ForceMode.Acceleration);
                        if (__instance.relay && input != Vector3.zero)
                        {
                            float mult = __instance.afterBurnerActive ? 2f : 1f;
                            mult *= powerBonus;
                            __instance.relay.ConsumeEnergy(mult * Time.deltaTime * __instance.powerEfficiencyFactor * 0.12f, out float _);
                        }

                    }
                    __instance.StabilizeRoll();
                }
                if (!__instance.truckSegment.IsFront() || __instance.IsPowered() && !__instance.truckSegment.ReachingOutOfWater() && (!__instance.seatruckanimation || __instance.seatruckanimation.currentAnimation != SeaTruckAnimation.Animation.Enter))
                    return false;
                __instance.StabilizePitch();
                return false;
            }

        }


    }
}
