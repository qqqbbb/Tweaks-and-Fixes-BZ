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
        public static Color flashlightLightColor;

        private static void FixFlashLight(GameObject go)
        {
            Transform lightParentTransform = go.transform.Find("lights_parent");
            Transform cone = lightParentTransform.Find("x_flashlightCone");
            VehicleLightFix.volLightBeam = cone.gameObject;
            Light[] lights = lightParentTransform.GetComponentsInChildren<Light>(true);
            foreach (var light in lights)
            {
                if (light.type == LightType.Point)
                {
                    light.enabled = false;
                    return;
                }
                if (ConfigToEdit.flashlightLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.flashlightLightIntensityMult.Value;

                if (flashlightLightColor != default)
                {
                    light.color = flashlightLightColor;
                    MeshRenderer mr = cone.GetComponent<MeshRenderer>();
                    //Main.logger.LogInfo("flashLight vol light color " + mr.material.color);
                    mr.material.color = VehicleLightFix.GetVolLightColor(light);
                    //Main.logger.LogInfo("flashLight vol light color ! " + mr.material.color);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerTool))]
        class PlayerTool_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void AwakePostfix(PlayerTool __instance)
            {
                if (__instance is FlashLight)
                {
                    FixFlashLight(__instance.gameObject);
                }
            }
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
                //AddDebug("OnHolster " + __instance.name);
                //equippedTool = null;
            }
        }

        //[HarmonyPatch(typeof(Pickupable), "Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
        class Pickupable_Drop_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                //AddDebug("Drop " + __instance.name);

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

        //[HarmonyPatch(typeof(FlashLight), "Start")]
        public class FlashLight_Start_Patch
        {
            public static void Prefix(FlashLight __instance)
            {
                //AddDebug("FlashLight Start");
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
