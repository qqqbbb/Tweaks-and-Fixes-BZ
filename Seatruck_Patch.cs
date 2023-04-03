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
        public static GameObject wcpGO = null;
        //public static GameObject seaTruckAquarium = null;
        public static int powerUpgrades = 0;
        //public static List<GameObject> seatruckModules = new List<GameObject>();
        public static HashSet<TechType> installedUpgrades = new HashSet<TechType>();
        static string uiText = string.Empty;
        static string uiTextSub = string.Empty;
        static FMODAsset stopPilotSound;

        public static void DoStuff(GameObject seaTruckAquarium)
        {
            Util.Log("DoStuff " );
            if (wcpGO == null || seaTruckAquarium == null)
                return;

            WaterClipProxy wcp = wcpGO.GetComponent<WaterClipProxy>();
            if (wcp)
            {
                Util.Log("DoStuff get WaterClipProxy " + wcp.transform.parent.name);
                AddDebug("DoStuff get WaterClipProxy parent " + wcp.transform.parent.name);
                AddDebug("DoStuff get WaterClipProxy " + wcp.name);
                MeshFilter meshFilter = wcpGO.GetComponent<MeshFilter>();
                if (!meshFilter)
                    AddDebug("no meshFilter ");
                MeshRenderer meshRenderer = wcpGO.GetComponent<MeshRenderer>();
                if (!meshRenderer)
                    AddDebug("no meshRenderer ");
                if (meshFilter && meshRenderer)
                {
                    AddDebug("add components ");
                    Util.Log("DoStuff add components " + wcp.name);
                    GameObject go = new GameObject("WaterClipProxy");
                    go.transform.SetParent(seaTruckAquarium.transform);
                    WaterClipProxy wcpCopy = Util.CopyComponent(wcp, go) as WaterClipProxy;
                    //Main.CopyComponent(meshFilter, go);
                    MeshFilter mfCopy = Util.CopyComponent(meshFilter, go) as MeshFilter;
                    //Main.CopyComponent(meshRenderer, go);
                    //MeshRenderer mrCopy = Main.CopyComponent(meshFilter, go) as MeshRenderer;
                    wcp.waterSurface = WaterSurface.Get();
                    UWE.CoroutineHost.StartCoroutine(wcp.LoadAsync());
                }
            }
        

    }

        public static void GetUpgradesNames(bool defenseSelected = false)
        {
            string lightToggle = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.RightHand);
            string defenseName = Language.main.Get(TechType.SeaTruckUpgradePerimeterDefense);
            if (Language.main.GetCurrentLanguage() == "English")
                defenseName = defenseName.Replace(" Upgrade", "");

            string exitButton = " Stop piloting (" + uGUI.FormatButton(GameInput.Button.Exit) + ")";
            uiTextSub = string.Empty;
            if (installedUpgrades.Contains(TechType.SeaTruckUpgradeAfterburner))
                uiTextSub = LanguageCache.GetButtonFormat("ExosuitBoost", GameInput.Button.Sprint) + " ";
            if (defenseSelected)
                uiText = defenseName + ". Press and hold " + uGUI.FormatButton(GameInput.Button.LeftHand
                    ) + " to charge the shot.";
            else
                uiText = string.Empty;

            //useButton = currentModuleName + ". Press and hold " + TooltipFactory.stringLeftHand + " to charge the shot.";
            uiTextSub += lightToggle + exitButton;
            //AddDebug("GetUpgradesNames activeSlot " + seaTruckUpgrades.activeSlot);
        }

        [HarmonyPatch(typeof(SeaTruckSegment))]
        class SeaTruckSegment_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SeaTruckSegment __instance)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.metal;
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnClickHatch")]
            public static bool OnClickHatchPrefix(SeaTruckSegment __instance, HandTargetEventData eventData)
            { // delay exit sound when exiting cabin or docking module
                //AddDebug("SeaTruckSegment OnClickHatch");
                if (__instance.player == null && !__instance.CanEnter())
                    return false;

                Player player = eventData.guiHand.player;
                bool playerIsOutside = __instance.PlayerIsOutside();
                if (player != null && !playerIsOutside)
                {
                    Vector3 exitPoint;
                    bool skipAnimations;
                    if (__instance.FindExitPoint(out exitPoint, out skipAnimations, SeaTruckAnimation.Animation.Exit))
                    {
                        //AddDebug("name" + __instance.name);
                        //AddDebug("parent " + __instance.transform.parent.name);
                        if (__instance.isMainCab)
                        {
                            __instance.StartCoroutine(Util.PlaySound(__instance.exitSound, .5f));
                            if (stopPilotSound)
                                __instance.StartCoroutine(Util.PlaySound(stopPilotSound, .5f));
                        }
                        else if(CraftData.GetTechType(__instance.gameObject) == TechType.SeaTruckDockingModule)
                        {
                            __instance.StartCoroutine(Util.PlaySound(__instance.exitSound, 1.5f));
                            if (stopPilotSound)
                                __instance.StartCoroutine(Util.PlaySound(stopPilotSound, 1.5f));
                        }
                        else
                        {
                            Utils.PlayFMODAsset(__instance.exitSound, player.transform);
                            if (stopPilotSound)
                                Utils.PlayFMODAsset(stopPilotSound, player.transform);
                        }
                        __instance.Exit(new Vector3?(exitPoint), skipAnimations, playerOverride: player);
                        if (__instance.CanAnimate() && !skipAnimations)
                            __instance.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.Exit;
                    }
                    else
                        AddError(Language.main.Get("ExitFailedNoSpace"));
                }
                else if (player != null & playerIsOutside)
                {
                    bool exitPoint = __instance.FindExitPoint(out Vector3 _, out bool _);
                    if (!__instance.IsWalkable() && !SeaTruckSegment.GetHead(__instance).isMainCab)
                        AddError(Language.main.Get("EnterFailedTooSteep"));
                    else if (exitPoint)
                        __instance.EnterHatch(player);
                    else
                        AddError(Language.main.Get("EnterFailedNoSpace"));
                }
                if (__instance.player)
                    __instance.PropagatePlayer();

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Exit")]
            public static void ExitPostfix(SeaTruckSegment __instance)
            {
                //AddDebug("isMainCab " + __instance.isMainCab);
                //AddDebug("isRearConnected " + __instance.isRearConnected);
                if (__instance.isMainCab && !__instance.isRearConnected)
                {
                    __instance.StartCoroutine(Util.PlaySound(__instance.exitSound, .5f));
                    if (stopPilotSound)
                        __instance.StartCoroutine(Util.PlaySound(stopPilotSound, .5f));
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("IInteriorSpace.GetInsideTemperature")]
            public static void GetInsideTemperaturePostfix(SeaTruckSegment __instance, ref float __result)
            {
                if (Main.config.useRealTempForColdMeter && !__instance.relay.IsPowered())
                    __result = Player_Patches.ambientTemperature;
                //AddDebug("SeaTruckSegment GetInsideTemperature " + __result);
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnHoverHatch")]
            public static void OnHoverHatchPostfix(SeaTruckSegment __instance)
            {
                AddDebug("SeaTruckSegment OnHoverHatch isRearConnected " + __instance.isRearConnected);
                if (Input.GetKeyDown(KeyCode.C) && stopPilotSound)
                {
                    AddDebug("stopPilotSound");
                    Utils.PlayFMODAsset(stopPilotSound, __instance.transform);
                }
                else if(Input.GetKeyDown(KeyCode.X))
                {
                    AddDebug("exitSound");
                    Utils.PlayFMODAsset(__instance.exitSound, __instance.transform);
                }
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    AddDebug("enterSound");
                    Utils.PlayFMODAsset(__instance.enterSound, __instance.transform);
                }
            }
        }


        //[HarmonyPatch(typeof(SeaTruckLights), "Start")]
        class SeaTruckLights_Start_Patch
        {
            public static void Prefix(SeaTruckLights __instance)
            {
                //if (__instance.name == "SeaTruck(Clone)")
                //{
                //    Transform transform = __instance.transform.Find("model/seatruck_anim/Seatruck_Interior_geo/");
                //    SkinnedMeshRenderer smr = transform.GetComponent<SkinnedMeshRenderer>();
                //    AddDebug("SeaTruck material name " + smr.materials[2].name);
                //    foreach (Material m in smr.materials)
                //    {
                        //m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                        //m.shaderKeywords = new string[] { "MARMO_SPECMAP", "UWE_3COLOR", "_NORMALMAP", "_ZWRITE_ON" };
                        //Main.Log("SeaTruck shaderKeywords " + item);
                    //}
                    //smr.materials[2].globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    //smr.materials[2].shaderKeywords = new string[0];

                //}
                //if (__instance.name == "SeaTruckStorageModule(Clone)")
                // fix storage module lights 
                __instance.lightingController = __instance.GetComponent<LightingController>();
                //AddDebug(__instance.name + "SeaTruckStorageModule fix lights " );
                //}
                //Light[] lights = Main.GetComponentsInDirectChildren<Light>(__instance, true);
                //foreach (Light light in lights)
                //{
                    //light.enabled = false;
                    //Main.Log(light.name + " turnofflights " + Main.GetParent(__instance.gameObject));
                    //AddDebug(light.name + " turnofflights " + Main.GetParent(__instance.gameObject));
                //}
            }
        }

        [HarmonyPatch(typeof(LightingController), "LerpToState", new Type[] { typeof(int), typeof(float) })]
        class LightingController_LerpToState_Patch
        { // turn off light in teleporter and docking module
            static int prevState = -1;
            public static void Prefix(LightingController __instance, int targetState)
            {
                prevState = (int)__instance.state;
                //AddDebug(__instance.name + " lights " + (LightingController.LightingState)targetState);
            }
            public static void Postfix(LightingController __instance, int targetState)
            {
                if (prevState == targetState || !__instance.GetComponent<SeaTruckLights>())
                    return;

                //AddDebug(__instance.name + " lights " + (LightingController.LightingState)targetState);
                Light[] lights = Util.GetComponentsInDirectChildren<Light>(__instance, true);
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

        //[HarmonyPatch(typeof(LightingController), "Update")]
        class LightingController_Update_Patch
        { 
            public static bool Prefix(LightingController __instance)
            {
       
                float deltaTime = Time.deltaTime;
                if (deltaTime <= 0F)
                    return false;
                AddDebug(__instance.name + " Update lights " + __instance.fadeDuration);
                __instance.timer.Update(deltaTime);
                int state = (int)__instance.state;
                if (__instance.prevState != state)
                {
                    AddDebug(__instance.name + " Update lights ");
                    __instance.LerpToState(state);
                    __instance.prevState = state;
                }
                __instance.UpdateIntensities();
                return false;
            }
        }

        //[HarmonyPatch(typeof(SeaTruckSegment), "Start")]
        class SeaTruckSegment_Start_Patch
        {
            public static void Postfix(SeaTruckSegment __instance)
            {
                //if (__instance.isMainCab)
                //    seaTruckCab = __instance.gameObject;
               Transform wcpTransform = __instance.transform.Find("WaterClipProxy");
                if (wcpTransform)
                {
                    //AddDebug("WaterClipProxy");
                    //Main.Log("WaterClipProxy");
                    wcpGO = wcpTransform.gameObject;
                }
                //else if (Main.GetComponentsInDirectChildren<SeaTruckAquarium>(__instance).Length > 0)
                {
                    //AddDebug("SeaTruckAquarium");
                    //Main.Log("SeaTruckAquarium");
                    //seaTruckAquarium = Main.GetParent(__instance.gameObject);
                    //if (wcpGO)
                    //    DoStuff(__instance.transform.parent.gameObject);
   
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

        [HarmonyPatch(typeof(SeaTruckUpgrades))]
        class SeaTruckUpgrades_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("IsAllowedToAdd")]
            public static bool IsAllowedToAddPrefix(SeaTruckUpgrades __instance, Pickupable pickupable, ref bool __result)
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

            [HarmonyPostfix]
            [HarmonyPatch("OnEquip")]
            public static void OnEquipPostfix(SeaTruckUpgrades __instance, string slot, InventoryItem item)
            {
                powerUpgrades = GetNumPowerUpgrades(__instance);
                //AddDebug("OnEquip " + item.item.name + " slot " + slot);
                //AddDebug("powerUpgrades " + powerUpgrades);
            }

            [HarmonyPrefix]
            [HarmonyPatch("TryActivateAfterBurner")]
            public static bool TryActivateAfterBurnerPrefix(SeaTruckUpgrades __instance)
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

            [HarmonyPostfix]
            [HarmonyPatch("OnUpgradeModuleChange")]
            public static void OnUpgradeModuleChangePostfix(SeaTruckUpgrades __instance, TechType techType, bool added)
            {// this used to somehow break slot extender mod
                //powerUpgrades = GetNumPowerUpgrades(__instance);
                //AddDebug("OnUpgradeModuleChange " + techType + " " + added);
                if (added)
                    installedUpgrades.Add(techType);
                else
                    installedUpgrades.Remove(techType);
                //AddDebug("powerUpgrades " + powerUpgrades);
                GetUpgradesNames();
            }

            [HarmonyPostfix]
            [HarmonyPatch("NotifySelectSlot")]
            public static void NotifySelectSlotPostfix(SeaTruckUpgrades __instance, int slotID)
            {
                //Main.Log("SeaTruckUpgrades NotifySelectSlot " + slotID);
                if (slotID == -1)
                    return;

                TechType tt = __instance.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
                if (tt == TechType.SeaTruckUpgradePerimeterDefense)
                    GetUpgradesNames(true);
                else
                    GetUpgradesNames();
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor))]
        class SeaTruckMotor_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SeaTruckMotor __instance)
            {
                stopPilotSound = __instance.stopPilotSound;
                __instance.stopPilotSound = null;// dont play exit sound when not exiting cabin 
            }
         
            [HarmonyPostfix]
            [HarmonyPatch("StartPiloting")]
            public static void StartPilotingPostfix(SeaTruckMotor __instance)
            {
                GetUpgradesNames();
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetWeight")]
            public static bool GetWeightPrefix(SeaTruckMotor __instance, ref float __result)
            {
                if (Main.config.seatruckMoveTweaks)
                {
                    __result = __instance.truckSegment.GetWeight() + __instance.truckSegment.GetAttachedWeight() * 0.8f;
                    return false;
                }
                return true;
            }
          
            [HarmonyPrefix]
            [HarmonyPatch( "Update")]
            public static void UpdatePrefix(SeaTruckMotor __instance)
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
        
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void UpdatePostfix(SeaTruckMotor __instance)
            {
                //AddDebug("piloting " + __instance.piloting);
                if (__instance.piloting)
                {
                    if (!string.IsNullOrEmpty(uiText))
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, uiText);
                    HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, uiTextSub);
                }
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

            [HarmonyPrefix]
            [HarmonyPatch("FixedUpdate")] // fixed
            public static bool FixedUpdatePrefix(SeaTruckMotor __instance)
            {
                //if (!Main.config.seatruckMoveTweaks)
                //    return true;

                if (!__instance.truckSegment.isMainCab && __instance.useRigidbody != null && (!__instance.useRigidbody.isKinematic && !__instance.piloting) && (__instance.useRigidbody.velocity.y > -0.3f && __instance.pilotPosition.position.y > Ocean.GetOceanLevel() - 2f))
                    __instance.useRigidbody.AddForce(new Vector3(0f, -0.3f - __instance.useRigidbody.velocity.y, 0f), ForceMode.VelocityChange);

                if (__instance.transform.position.y < Ocean.GetOceanLevel() && __instance.useRigidbody != null && (__instance.IsPowered() && !__instance.IsBusyAnimating()))
                {
                    if (__instance.piloting)
                    {
                        Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() || __instance.inputStackDummy.activeInHierarchy ? GameInput.GetMoveDirection() : Vector3.zero;
                        if (!Main.config.seatruckMoveTweaks)
                        { 
                            moveDirection = moveDirection.normalized;
                            moveDirection.x *= .5f;
                            moveDirection.y *= .5f;
                            if (moveDirection.z < 0f)
                                moveDirection.z *= .5f;
                        }
                        Int2 int2;
                        int2.x = moveDirection.x <= 0f ? (moveDirection.x >= 0 ? 0 : -1) : 1;
                        int2.y = moveDirection.z <= 0f ? (moveDirection.z >= 0 ? 0 : -1) : 1;
                        __instance.leverDirection = int2;
                        if (__instance.afterBurnerActive)
                            moveDirection.z = 1f;

                        moveDirection = moveDirection.normalized;
                        Vector3 vector3_2 = MainCameraControl.main.rotation * moveDirection;
                        float acceleration = 1f / Mathf.Max(1f, __instance.GetWeight() * 0.35f) * __instance.acceleration;
                        if (__instance.afterBurnerActive)
                            acceleration += 7f;

                        acceleration *= Main.config.seatruckSpeedMult;
                        __instance.useRigidbody.AddForce(acceleration * vector3_2, ForceMode.Acceleration);
                        if (__instance.relay && moveDirection != Vector3.zero)
                            __instance.relay.ConsumeEnergy((Time.deltaTime * __instance.powerEfficiencyFactor * 0.12f), out float _);
                    }
                    __instance.StabilizeRoll();
                }
                if (!__instance.truckSegment.IsFront() || __instance.IsPowered() && !__instance.truckSegment.ReachingOutOfWater() && (!__instance.seatruckanimation || __instance.seatruckanimation.currentAnimation != SeaTruckAnimation.Animation.Enter))
                    return false;

                __instance.StabilizePitch();
                return false;
            }
        }

        //[HarmonyPatch(typeof(GenericHandTarget), "OnHandClick")]
        class GenericHandTarget_OnHandClick_Patch
            {
            public static void OnHandClickPostfix(GenericHandTarget __instance)
            {
                AddDebug(__instance.name + " GenericHandTarget OnHandClick" + __instance.transform.parent.name);
                if (__instance.name == "hatchTrigger" && __instance.transform.parent.name == "SeaTruck(Clone)")
                {
                    AddDebug("hatchTrigger");
                    //Main.Log("WaterClipProxy");
                    //wcpGO = wcpTransform.gameObject;
                }

                //seaTruckUpgrades = __instance;
            }
        }

    }
}
