using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Base_Light
    {
        public static Color spotlightLightColor;
        public static Color vehicleDockingBayLightColor;
        static Vector3[] vehicleDockingBayLightBeamPos = new Vector3[] { new Vector3(0, 0, -0.77f), new Vector3(0, 0, -1.1f), new Vector3(0, 0, -1f), new Vector3(0, 0, -0.76f) };
        static Vector3 vehicleDockingBayLightScale = new Vector3(3f, 3f, 6f);

        [HarmonyPatch(typeof(BaseSpotLight))]
        internal class BaseSpotLight__
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            static void StartPostfix(BaseSpotLight __instance)
            {
                Light light = __instance.light.GetComponent<Light>();
                //Main.logger.LogError("BaseSpotLight light.intensity " + light.intensity);
                //Main.logger.LogError("BaseSpotLight light.a " + light.color.a);

                if (ConfigToEdit.spotlightLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.spotlightLightIntensityMult.Value;

                if (spotlightLightColor != default)
                { // no VFXVolumetricLight
                    light.color = spotlightLightColor;
                    MeshRenderer mr = light.GetComponentInChildren<MeshRenderer>();
                    mr.material.color = new Color(spotlightLightColor.r, spotlightLightColor.g, spotlightLightColor.b, mr.material.color.a);
                }
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        public class VehicleDockingBay_Patch
        {
            public static Dictionary<VehicleDockingBay, PowerSystem.Status> savedPowerStatus = new Dictionary<VehicleDockingBay, PowerSystem.Status>();

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(VehicleDockingBay __instance)
            {
                if (__instance.expansionManager != null)
                    return;

                CoroutineHost.StartCoroutine(FixVehicleDockingBayLights(__instance));
            }

            static IEnumerator FixVehicleDockingBayLights(VehicleDockingBay vehicleDockingBay)
            {
                yield return new WaitUntil(() => Main.gameLoaded);
                List<Light> lights = GetPillarLights(vehicleDockingBay);
                if (lights == null || lights.Count == 0)
                    yield return null;

                //AddDebug("FixVehicleDockingBayLights  " + lights.Count);
                for (int i = 0; i < lights.Count; i++)
                {// no VFXVolumetricLight
                    Light light = lights[i];
                    Vector3 lightBeamPos = vehicleDockingBayLightBeamPos[i];
                    VehicleLightFix.AddLightBeam(light.gameObject, lightBeamPos, vehicleDockingBayLightScale);
                    //Main.logger.LogInfo("VehicleDockingBay lightColor " + light.color);
                    if (vehicleDockingBayLightColor != default) // 0.361, 1.000, 1.000
                        light.color = vehicleDockingBayLightColor;

                    //Main.logger.LogInfo("VehicleDockingBay light intensity " + light.intensity);
                    if (ConfigToEdit.vehicleDockingBayLightIntensityMult.Value < 1) // 1.73
                        light.intensity *= ConfigToEdit.vehicleDockingBayLightIntensityMult.Value;
                }
            }

            private static List<Light> GetPillarLights(VehicleDockingBay vehicleDockingBay)
            {
                //AddDebug("GetPillarLights " + vehicleDockingBay.transform.parent.parent.name);
                Transform pillars = vehicleDockingBay.transform.parent.parent.Find("pillars");
                if (pillars == null)
                    return null;

                List<Light> lights = new List<Light>();
                foreach (Transform pillar in pillars.transform)
                {
                    //AddDebug("GetPillarLights pillar " + pillar.name + " activeSelf " + pillar.gameObject.activeSelf);
                    Transform lightT = pillar.Find("light");
                    if (lightT != null)
                        lights.Add(lightT.GetComponent<Light>());
                }
                return lights;
            }

            [HarmonyPostfix, HarmonyPatch("LateUpdate")]
            public static void LateUpdatePostfix(VehicleDockingBay __instance)
            {
                if (Main.gameLoaded == false || __instance.expansionManager != null)
                    return;

                //AddDebug("VehicleDockingBay LateUpdate  " + __instance.name);
                PowerSystem.Status currentStatus = __instance.powerRelay.powerStatus;
                if (!savedPowerStatus.ContainsKey(__instance) || currentStatus != savedPowerStatus[__instance])
                {
                    //AddDebug("VehicleDockingBay Update lights ");
                    savedPowerStatus[__instance] = currentStatus;
                    List<Light> lights = GetPillarLights(__instance);
                    if (lights == null || lights.Count == 0)
                        return;

                    bool on = currentStatus != PowerSystem.Status.Offline;
                    foreach (Light light in lights)
                    {
                        if (on == light.gameObject.activeSelf)
                            continue;

                        light.gameObject.SetActive(on);
                    }
                }
            }
        }


    }
}
