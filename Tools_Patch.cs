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

        public static void SaveSeaglideState(Seaglide seaglide)
        {
            var seaglideMap = seaglide.GetComponent<VehicleInterface_MapController>();
            if (seaglideMap && seaglideMap.miniWorld)
            {
                if (seaglideMap.miniWorld.active)
                    Main.configMain.DeleteSeaglideMap(seaglide.gameObject);
                else
                    Main.configMain.SaveSeaglideMap(seaglide.gameObject);
            }
            if (seaglide.toggleLights)
            {
                if (seaglide.toggleLights.lightsActive)
                    Main.configMain.SaveSeaglideLights(seaglide.gameObject);
                else
                    Main.configMain.DeleteSeaglideLights(seaglide.gameObject);
            }
        }

        [HarmonyPatch(typeof(BeaconLabel))]
        class BeaconLabel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(BeaconLabel __instance)
            {
                if (ConfigToEdit.beaconTweaks.Value)
                {
                    Collider collider = __instance.GetComponent<Collider>();
                    if (collider)
                        UnityEngine.Object.Destroy(collider);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnPickedUp")]
            static bool OnPickedUpPrefix(BeaconLabel __instance)
            {
                return !ConfigToEdit.beaconTweaks.Value;
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnDropped")]
            static bool OnDroppedPrefix(BeaconLabel __instance)
            {
                return !ConfigToEdit.beaconTweaks.Value;
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


        //[HarmonyPatch(typeof(VehicleInterface_MapController), "Start")]
        class VehicleInterface_MapController_Start_Patch
        {
            public static void Postfix(VehicleInterface_MapController __instance)
            {
                //AddDebug("VehicleInterface_MapController Start " + __instance.name);
                //__instance.mapActive = Main.configMain.seaglideMap;
            }
        }

        [HarmonyPatch(typeof(Seaglide))]
        class Seaglide_Patch
        {
            public static IEnumerator LoadSeaglideState(Seaglide seaglide)
            {
                if (seaglide == null)
                    yield break;

                if (seaglide.toggleLights == null)
                    yield return null;

                bool lightOn = Main.configMain.GetSeaglideLights(seaglide.gameObject);
                //AddDebug("Seaglide saved light " + lightOn);
                seaglide.toggleLights.SetLightsActive(lightOn);
                //AddDebug("Seaglide GetLightsActive " + seaglide.toggleLights.GetLightsActive());
                var map = seaglide.GetComponent<VehicleInterface_MapController>();
                if (map == null)
                    yield break;

                if (map.miniWorld == null)
                    yield return null;

                bool mapOn = Main.configMain.GetSeaglideMap(seaglide.gameObject);
                //AddDebug("Seaglide map " + mapOn);
                map.miniWorld.active = mapOn;
                map.mapActive = mapOn;
            }

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(Seaglide __instance)
            {
                //__instance.toggleLights.SetLightsActive(Main.configMain.GetSeaglideLights(__instance.gameObject));
                CoroutineHost.StartCoroutine(LoadSeaglideState(__instance));
            }
            [HarmonyPrefix, HarmonyPatch("OnHolster")]
            public static void OnHolsterPrefix(Seaglide __instance)
            { // fires when saving, after nautilus SaveEvent
                //AddDebug("Seaglide OnHolster lightsActive " + __instance.toggleLights.lightsActive);
                SaveSeaglideState(__instance);
            }
        }

        [HarmonyPatch(typeof(BuilderTool))]
        class BuilderTool_Patch
        {
            [HarmonyPostfix, HarmonyPatch("HasEnergyOrInBase")]
            static void HasEnergyOrInBasePostfix(BuilderTool __instance, ref bool __result)
            {
                if (!ConfigToEdit.builderToolBuildsInsideWithoutPower.Value && __instance.energyMixin.charge <= 0)
                {
                    __result = false;
                }
            }
            [HarmonyPostfix, HarmonyPatch("HandleInput")]
            public static void HandleInputPostfix(BuilderTool __instance)
            {
                if (Builder.isPlacing && GameInput.GetButtonDown(GameInput.Button.Exit))
                {
                    //AddDebug("BuilderTool HandleInput Exit");
                    //__instance.OnHolster();
                    Inventory.main.quickSlots.Deselect();
                }
            }
        }

        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Construct_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("NotifyConstructedChanged")]
            public static void Postfix(Constructable __instance, bool constructed)
            {
                if (!constructed || !Main.gameLoaded)
                    return;

                //AddDebug(" NotifyConstructedChanged " + __instance.techType);
                //AddDebug(" NotifyConstructedChanged isPlacing " + Builder.isPlacing);
                if (!ConfigToEdit.builderPlacingWhenFinishedBuilding.Value)
                    Player.main.StartCoroutine(BuilderEnd(2));
            }
        }

        static IEnumerator BuilderEnd(int waitFrames)
        {
            //AddDebug("BuilderEnd start ");
            //yield return new WaitForSeconds(waitTime);
            while (waitFrames > 0)
            {
                waitFrames--;
                yield return null;
            }
            Builder.End();
            //AddDebug("BuilderEnd end ");
        }


    }
}
