using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;
using System.Collections;
using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace Tweaks_Fixes
{
    class Tools_Patch
    {
        public static bool releasingGrabbedObject = false;
        public static List<GameObject> repCannonGOs = new List<GameObject>();
        public static PlayerTool equippedTool;
        public static List<PlayerTool> fixedFish = new List<PlayerTool>();

        public static IEnumerator FixDeadFish()
        {
            while (!uGUI.main.hud.active)
                yield return null;

            yield return new WaitForSeconds(0.5f);
            int activeSlot = Inventory.main.quickSlots.activeSlot;
            //AddDebug("DeselectImmediate " + Inventory.main.quickSlots.activeSlot);
            Inventory.main.quickSlots.DeselectImmediate();
            Inventory.main.quickSlots.Select(activeSlot);
        }

        [HarmonyPatch(typeof(Knife), "OnToolUseAnim")]
        class Knife_OnToolUseAnim_Prefix_Patch
        {
            public static bool Prefix(Knife __instance, GUIHand hand)
            {
                Vector3 position = new Vector3();
                GameObject closestObj = null;
                Vector3 normal;
                UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref closestObj, ref position, out normal);
                if (closestObj == null)
                {
                    InteractionVolumeUser ivu = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                    if (ivu != null && ivu.GetMostRecent() != null)
                        closestObj = ivu.GetMostRecent().gameObject;
                }
                if (closestObj)
                {
                    GameObject root = null;
                    LargeWorldEntity lwe = closestObj.GetComponentInParent<LargeWorldEntity>();
                    if (lwe)
                        root = lwe.gameObject;

                    //AddDebug("closestObj " + closestObj.name);
                    //AddDebug("root " + root.name);

                    LiveMixin lm = closestObj.FindAncestor<LiveMixin>();

                    if (lm && Knife.IsValidTarget(lm))
                    {
                        bool wasAlive = lm.IsAlive();
                        lm.TakeDamage(__instance.damage, position, __instance.damageType, Utils.GetLocalPlayer());
                        __instance.GiveResourceOnDamage(closestObj, lm.IsAlive(), wasAlive);
                    }
                    VFXSurface surface = closestObj.GetComponent<VFXSurface>();
                    if (surface == null && root != null)
                        surface = root.GetComponent<VFXSurface>();

                    Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                    //if (surface)
                    //    AddDebug("surface " + surface.surfaceType);

                    VFXSurfaceTypeManager.main.Play(surface, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);

                    VFXSurfaceTypes vfxSurfaceType = VFXSurfaceTypes.none;
                    if (surface)
                        vfxSurfaceType = surface.surfaceType;
                    else
                        vfxSurfaceType = Utils.GetTerrainSurfaceType(position, normal, VFXSurfaceTypes.sand);

                    FMOD.Studio.EventInstance fmodEvent = Utils.GetFMODEvent(__instance.hitSound, __instance.transform.position);
                    fmodEvent.setParameterValueByIndex(__instance.surfaceParamIndex, (int)vfxSurfaceType);
                    fmodEvent.start();
                    fmodEvent.release();
                }
                Utils.PlayFMODAsset(Player.main.IsUnderwater() ? __instance.swingWaterSound : __instance.swingSound, __instance.transform.position);
                return false;

            }
              
            public static void Postfix(Knife __instance)
            {
                if (!Player.main.guiHand.activeTarget)
                    return;

                BreakableResource breakableResource = Player.main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //AddDebug("BreakableResource");
                }
                Pickupable pickupable = Player.main.guiHand.activeTarget.GetComponent<Pickupable>();
                if (pickupable)
                {
                    TechType techType = pickupable.GetTechType();
                    if (Main.config.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Player.main.guiHand);
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(PlayerTool))]
        class PlayerTool_OnDraw_Patch
        {
            static float knifeRangeDefault = 0f;
            static float knifeDamageDefault = 0f;

            [HarmonyPostfix]
            [HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(PlayerTool __instance)
            {
                //AddDebug("OnDraw " + __instance.name);
                if (Main.IsEatableFish(__instance.gameObject) && !fixedFish.Contains(__instance) && !__instance.GetComponent<LiveMixin>().IsAlive())
                {
                    //AddDebug("OnDraw " + __instance.name);
                    //Inventory.main.quickSlots.DeselectImmediate();
                    fixedFish.Add(__instance);
                    UWE.CoroutineHost.StartCoroutine(FixDeadFish());
                }
                equippedTool = __instance;
                Knife knife = __instance as Knife;
                if (knife)
                {
                    if (knifeRangeDefault == 0f)
                        knifeRangeDefault = knife.attackDist;
                    if (knifeDamageDefault == 0f)
                        knifeDamageDefault = knife.damage;

                    knife.attackDist = knifeRangeDefault * Main.config.knifeRangeMult;
                    knife.damage = knifeDamageDefault * Main.config.knifeDamageMult;
                    //AddDebug(" attackDist  " + knife.attackDist);
                    //AddDebug(" damage  " + knife.damage);
                }

            }

            [HarmonyPostfix]
            [HarmonyPatch("OnHolster")]
            public static void OnHolsterPostfix(PlayerTool __instance)
            {
                if (__instance is Seaglide)
                {
                    VehicleInterface_MapController mc = __instance.GetComponent<VehicleInterface_MapController>();
                    Main.config.seaGlideMap = mc.mapActive;
                }
            }

        }

        [HarmonyPatch(typeof(BeaconLabel))]
        class BeaconLabel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(BeaconLabel __instance)
            {
                Collider collider = __instance.GetComponent<Collider>();
                if (collider)
                    UnityEngine.Object.Destroy(collider);
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnPickedUp")]
            static bool OnPickedUpPrefix(BeaconLabel __instance)
            {
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnDropped")]
            static bool OnDroppedPrefix(BeaconLabel __instance)
            {
                return false;
            }
        }

        //[HarmonyPatch(typeof(PlayerTool), "OnHolster")]
        class PlayerTool_OnHolster_Patch
        {
            public static void Postfix(PlayerTool __instance)
            {
                AddDebug("OnHolster " + __instance.name);
                //equippedTool = null;
            }
        }

         //[HarmonyPatch(typeof(Pickupable), "Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
            class Pickupable_Drop_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                AddDebug("Drop " + __instance.name);

            }
        }

        //[HarmonyPatch(typeof(ScannerTool), "Update")]
        class ScannerTool_Update_Patch
        {// SHOW power when equipped
            private static bool Prefix(ScannerTool __instance)
            {
                //PlayerTool playerTool = 
                //bool isDrawn = (bool)PlayerTool_get_isDrawn.Invoke(__instance, new object[] { });
                if (__instance.isDrawn)
                {
                    //float idleTimer = (float)ScannerTool_idleTimer.GetValue(__instance);
                    //AddDebug("useText1 " + HandReticle.main.useText1);
                    //AddDebug("useText2 " + HandReticle.main.useText2);
                    if (__instance.idleTimer > 0f)
                    {
                        __instance.idleTimer = Mathf.Max(0f, __instance.idleTimer - Time.deltaTime);
                        //string buttonFormat = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
                        //               HandReticle.main.SetUseTextRaw(buttonFormat, null);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Constructor), "OnEnable")]
        class Constructor_OnEnable_Patch
        {
            static void Postfix(Constructor __instance)
            {
                ImmuneToPropulsioncannon itpc = __instance.GetComponent<ImmuneToPropulsioncannon>();
                if (itpc)
                {
                    //AddDebug("OnEnable Constructor ");
                    UnityEngine.Object.Destroy(itpc);
                }
                //itpc.enabled = false;
            }
        }

        [HarmonyPatch(typeof(FlashLight), "Start")]
        public class FlashLight_Start_Patch
        {
            public static void Prefix(FlashLight __instance)
            {
                Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                //AddDebug("FlashLight lights " + lights.Length);
                for (int i = lights.Length - 1; i >= 0; i--)
                {
                    if (lights[i].type == LightType.Point)
                        lights[i].enabled = false;
                }
            }
        }

        [HarmonyPatch(typeof(PropulsionCannon))]
        class PropulsionCannon_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnShoot")]
            static void OnShootPrefix(PropulsionCannon __instance)
            {
                if (__instance.grabbedObject == null)
                    return;
                //AddDebug("OnShoot " + __instance.grabbedObject.name);
                releasingGrabbedObject = true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("ReleaseGrabbedObject")]
            static void ReleaseGrabbedObjectPrefix(PropulsionCannon __instance)
            {
                if (__instance.grabbedObject == null)
                    return;
                //AddDebug("ReleaseGrabbedObject " + __instance.grabbedObject.name);
                releasingGrabbedObject = true;
            }

        }

        [HarmonyPatch(typeof(VehicleInterface_MapController), "Start")]
        class VehicleInterface_MapController_Start_Patch
        {
            public static void Postfix(VehicleInterface_MapController __instance)
            {
                //AddDebug("VehicleInterface_MapController Start " + __instance.name);
                __instance.mapActive = Main.config.seaGlideMap;
            }
        }

        //[HarmonyPatch(typeof(Welder), "CanWeldTarget")]
        class Welder_CanWeldTarget_Patch
        {
            static void Postfix(Welder __instance, LiveMixin activeWeldTarget, ref bool __result)
            {
                //if (Main.config.cantRepairVehicleInWater && Player.main.isUnderwater.value && activeWeldTarget)
                {
                    if (activeWeldTarget.GetComponent<SeaTruckSegment>() != null || activeWeldTarget.GetComponent<Exosuit>() != null)
                    {
                        //AddDebug("CanWeldTarget SeaTruckSegment ");
                        __result = false;
                    }
                }
            }
        }


    }
}
