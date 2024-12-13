using FMOD;
using FMOD.Studio;
using FMODUnity;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Tools_Patch
    {

        //public static List<GameObject> repCannonGOs = new List<GameObject>();
        public static PlayerTool equippedTool;
        public static List<PlayerTool> fixedFish = new List<PlayerTool>();

        [HarmonyPatch(typeof(PlayerTool))]
        class PlayerTool_OnDraw_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(PlayerTool __instance)
            {
                //AddDebug("OnDraw " + __instance.name);
                //if (Util.IsEatableFish(__instance.gameObject) && !fixedFish.Contains(__instance) && !__instance.GetComponent<LiveMixin>().IsAlive())
                //{
                //    //AddDebug("OnDraw " + __instance.name);
                //    //Inventory.main.quickSlots.DeselectImmediate();
                //    fixedFish.Add(__instance);
                //    UWE.CoroutineHost.StartCoroutine(FixDeadFish());
                //    return;
                //}
                equippedTool = __instance;
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

        [HarmonyPatch(typeof(ScannerTool), "PlayScanFX")]
        class ScannerTool_PlayScanFX_Patch
        {
            static bool Prefix(ScannerTool __instance)
            {
                //AddDebug("ScannerTool PlayScanFX ");
                return ConfigToEdit.scannerFX.Value;
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


        [HarmonyPatch(typeof(VehicleInterface_MapController), "Start")]
        class VehicleInterface_MapController_Start_Patch
        {
            public static void Postfix(VehicleInterface_MapController __instance)
            {
                //AddDebug("VehicleInterface_MapController Start " + __instance.name);
                __instance.mapActive = Main.configMain.seaglideMap;
            }
        }

        [HarmonyPatch(typeof(Seaglide))]
        class Seaglide_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(Seaglide __instance)
            {
                __instance.toggleLights.SetLightsActive(Main.configMain.seaglideLights);
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnHolster")]
            public static void OnHolsterPostfix(Seaglide __instance)
            {// fires when saving
                //AddDebug("Seaglide OnHolster");
                Main.configMain.seaglideLights = __instance.toggleLights.lightsActive;
                var mc = __instance.GetComponent<VehicleInterface_MapController>();
                if (mc != null)
                    Main.configMain.seaglideMap = mc.mapActive;
            }
        }

        [HarmonyPatch(typeof(BuilderTool), "HasEnergyOrInBase")]
        class BuilderTool_HasEnergyOrInBase_Patch
        {
            static void Postfix(BuilderTool __instance, ref bool __result)
            {
                if (!ConfigToEdit.builderToolBuildsInsideWithoutPower.Value && __instance.energyMixin.charge <= 0)
                {
                    __result = false;
                }
            }
        }


    }
}
