using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(MapRoomCamera))]
    internal class MapRoomCamera_
    {
        static Vector3 beamPos = new Vector3(-0.05f, -0.05f, -0.31f);
        static Vector3 beamScale = new Vector3(2, 2, 2);
        public static Color lightColor;

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(MapRoomCamera __instance)
        {
            Light[] lights = __instance.lightsParent.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                if (ConfigToEdit.cameraLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.cameraLightIntensityMult.Value;

                if (lightColor != default)
                    light.color = lightColor;
            }
            VehicleLightFix.AddLightBeam(__instance.lightsParent, beamPos, beamScale);
        }
    }
}
