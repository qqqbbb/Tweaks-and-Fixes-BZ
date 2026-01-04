using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Seaglide))]
    internal class Seaglide_
    {
        public static Color lightColor;
        public static Vector3 volLightScale = new Vector3(2, 2, 4);
        public static Vector3 volLightPos = new Vector3(0.015f, 0.003f, -0.2f);


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
            //AddDebug("Seaglide Start");
            FixLight(__instance);
        }

        private static void FixLight(Seaglide __instance)
        {
            CoroutineHost.StartCoroutine(LoadSeaglideState(__instance));
            //if (lightColor == default && ConfigToEdit.seaglideLightIntensityMult.Value == 1)
            //return;

            Transform t = __instance.transform.Find("lights_parent/Light");
            Light light = t.GetComponent<Light>();
            if (lightColor != default)
            {
                light.color = lightColor;
                //MeshRenderer mr = light.GetComponentInChildren<MeshRenderer>();
                //mr.material.color = VehicleLightFix.GetVolLightColor(light);
            }
            //Main.logger.LogInfo("Seaglide light color " + light.color);
            if (ConfigToEdit.seaglideLightIntensityMult.Value < 1)
                light.intensity *= ConfigToEdit.seaglideLightIntensityMult.Value;

            VehicleLightFix.AddLightBeam(light.gameObject, volLightPos, volLightScale);
        }

        [HarmonyPrefix, HarmonyPatch("OnHolster")]
        public static void OnHolsterPrefix(Seaglide __instance)
        { // fires when saving, after nautilus SaveEvent
          //AddDebug("Seaglide OnHolster lightsActive " + __instance.toggleLights.lightsActive);
            SaveSeaglideState(__instance);
            ToggleVolLight(__instance, true);
        }

        [HarmonyPrefix, HarmonyPatch("OnDraw")]
        public static void OnDrawPrefix(Seaglide __instance)
        {
            CoroutineHost.StartCoroutine(DisableVolLight(__instance));
        }

        public static IEnumerator DisableVolLight(Seaglide seaglide)
        {
            bool thisFrame = true;
            if (thisFrame)
            {
                thisFrame = false;
                yield return null;
            }
            ToggleVolLight(seaglide, false);
        }

        private static void ToggleVolLight(Seaglide seaglide, bool enabled)
        {
            Transform t = seaglide.transform.Find("lights_parent/Light/x_flashlightCone(Clone)");
            if (t && t.gameObject.activeSelf != enabled)
                t.gameObject.SetActive(enabled);
        }



    }
}
