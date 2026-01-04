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
    internal class VehicleLightFix
    {
        static VFXVolumetricLight seamothVFXVolumetricLight;
        public static GameObject volLightBeam;
        public static float volLightAlpha = 0.098f;
        public static Color seatruckLightColor;
        public static Color exosuitLightColor;
        public static Vector3 volLightRot = new Vector3(0, 90f, 90f);
        //public static Vector3 exosuitVolLightScale = new Vector3(2f, 2f, 2f);
        public static Vector3 seatruckVolLightScale = new Vector3(3, 3, 3);

        public static void AddLightBeam(GameObject parent, Vector3 pos = default, Vector3 scale = default)
        {
            if (volLightBeam == null)
            {
                Main.logger.LogWarning("can not add vol Light to " + parent.name);
                return;
            }
            if (scale == default)
                scale = Vector3.one;

            GameObject lightBeam = UnityEngine.Object.Instantiate(volLightBeam, Vector3.zero, Quaternion.identity);
            lightBeam.transform.parent = parent.transform;
            lightBeam.transform.localPosition = pos;
            lightBeam.transform.localRotation = Quaternion.Euler(volLightRot);
            lightBeam.transform.localScale = scale;
            Light light = parent.GetComponent<Light>();
            if (light)
            {
                MeshRenderer mr = lightBeam.GetComponent<MeshRenderer>();
                mr.material.color = GetVolLightColor(light);
            }
            //VFXVolumetricLight volLight = parent.gameObject.AddComponent<VFXVolumetricLight>();
            //volLight.syncMeshWithLight = seamothVFXVolumetricLight.syncMeshWithLight;
            //volLight.angle = seamothVFXVolumetricLight.angle;
            //volLight.range = seamothVFXVolumetricLight.range;
            //volLight.intensity = seamothVFXVolumetricLight.intensity;
            //volLight.startOffset = seamothVFXVolumetricLight.startOffset;
            //volLight.startFallof = seamothVFXVolumetricLight.startFallof;
            //volLight.nearClip = seamothVFXVolumetricLight.nearClip;
            //volLight.softEdges = seamothVFXVolumetricLight.softEdges;
            //volLight.segments = seamothVFXVolumetricLight.segments;
            //volLight.lightType = seamothVFXVolumetricLight.lightType;
            //volLight.color = seamothVFXVolumetricLight.color;
            //volLight.lightIntensity = seamothVFXVolumetricLight.lightIntensity;
            //volLight.coneMat = seamothVFXVolumetricLight.coneMat;
            //volLight.sphereMat = seamothVFXVolumetricLight.sphereMat;
            //volLight.volumMesh = seamothVFXVolumetricLight.volumMesh;
            //volLight.block = seamothVFXVolumetricLight.block;
            //volLight.lightSource = parent.GetComponentInChildren<Light>();
            //volLight.volumGO = lightCone;
            //volLight.volumRenderer = lightCone.GetComponent<MeshRenderer>();
            //volLight.volumMeshFilter = lightCone.GetComponent<MeshFilter>();
        }

        public static Color GetVolLightColor(Light light)
        {
            float a = Util.NormalizeToRange(light.intensity, 0, 1, 0, .1f);
            return new Color(light.color.r * .5f + .1f, light.color.g * .5f + .1f, light.color.b * .5f + .1f, a);
        }

        private static void ToggleLights(Exosuit exosuit)
        {
            Transform lightsT = Util.GetExosuitLightsTransform(exosuit);
            if (lightsT)
            {
                //AddDebug("IngameMenu isActiveAndEnabled " + IngameMenu.main.isActiveAndEnabled);
                if (!lightsT.gameObject.activeSelf && exosuit.energyInterface.hasCharge)
                {
                    lightsT.gameObject.SetActive(true);
                    Main.configMain.DeleteExosuitLights(exosuit.gameObject);
                    if (Exosuit_Sounds.lightOnSound)
                        Utils.PlayFMODAsset(Exosuit_Sounds.lightOnSound, exosuit.gameObject.transform.position);
                }
                else if (lightsT.gameObject.activeSelf)
                {
                    lightsT.gameObject.SetActive(false);
                    Main.configMain.SaveExosuitLights(exosuit.gameObject);
                    if (Exosuit_Sounds.lightOffSound)
                        Utils.PlayFMODAsset(Exosuit_Sounds.lightOffSound, exosuit.gameObject.transform.position);
                }
                //AddDebug("lights " + lightsT.gameObject.activeSelf);
            }
        }

        private static void SetLights(Exosuit exosuit, bool on)
        {
            if (on && !exosuit.energyInterface.hasCharge)
                return;

            Util.GetExosuitLightsTransform(exosuit).gameObject.SetActive(on);
        }

        [HarmonyPatch(typeof(Exosuit))]
        class Exosuit_Patch
        {
            private static void FixExosuitLight(Exosuit exosuit)
            {
                Transform lightTransform = Util.GetExosuitLightsTransform(exosuit);
                Light[] Lights = lightTransform.GetComponentsInChildren<Light>();
                for (int i = 0; i < Lights.Length; i++)
                {
                    Light light = Lights[i];
                    //Main.logger.LogInfo("Exosuit light color " + light.color);
                    if (ConfigToEdit.exosuitLightIntensityMult.Value < 1)
                        light.intensity *= ConfigToEdit.exosuitLightIntensityMult.Value;

                    if (exosuitLightColor != default)
                        light.color = exosuitLightColor;

                    Vector3 pos;
                    if (i == 0)
                        pos = new Vector3(-0.05f, 0.2f, -0.5f);
                    else
                        pos = new Vector3(0, 0.2f, -0.5f);

                    AddLightBeam(light.gameObject, pos, seatruckVolLightScale);
                }
            }

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void AwakePostfix(Exosuit __instance)
            {
                Util.GetExosuitLightsTransform(__instance).SetParent(__instance.leftArmAttach);
                FixExosuitLight(__instance);
                if (Main.configMain.GetExosuitLights(__instance.gameObject))
                    SetLights(__instance, false);
            }

            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(Exosuit __instance)
            {
                if (Main.gameLoaded == false)
                    return;

                if (!IngameMenu.main.isActiveAndEnabled && !Player.main.pda.isInUse && Player.main.currentMountedVehicle == __instance)
                {
                    if (GameInput.GetButtonDown(GameInput.Button.MoveDown))
                        ToggleLights(__instance);
                }
            }

            [HarmonyPostfix, HarmonyPatch("EnterVehicle")]
            public static void EnterVehiclePostfix(Exosuit __instance)
            {
                CoroutineHost.StartCoroutine(DisableLightBeam(__instance));
            }

            static IEnumerator DisableLightBeam(Exosuit exosuit)
            {
                yield return new WaitUntil(() => Main.gameLoaded);
                ToggleLightBeam(exosuit, false);
            }

            private static void ToggleLightBeam(Exosuit exosuit, bool on)
            {
                Transform lightT = Util.GetExosuitLightsTransform(exosuit);
                Light[] lights = lightT.GetComponentsInChildren<Light>();
                foreach (var light in lights)
                {
                    foreach (Transform child in light.transform)
                        child.gameObject.SetActive(on);
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnPilotModeEnd")]
            public static void OnPlayerEnteredPostfix(Exosuit __instance)
            {
                ToggleLightBeam(__instance, true);
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        class VehicleDockingBay_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("OnUndockingStart")]
            public static void OnUndockingStartPostfix(VehicleDockingBay __instance)
            {
                //Exosuit exosuit = __instance.dockedObject;
                //if (exosuit)
                {
                    //AddDebug("OnUndockingStart");
                    //SetLights(exosuit, true);
                }
            }

            [HarmonyPostfix, HarmonyPatch("Dock")]
            public static void DockVehiclePostfix(VehicleDockingBay __instance, Dockable dockable)
            {
                //AddDebug("Dock");
                Exosuit exosuit = dockable.GetComponent<Exosuit>();
                if (exosuit)
                    CoroutineHost.StartCoroutine(TurnOffLightsDelay(exosuit, 2));
            }

            public static IEnumerator TurnOffLightsDelay(Exosuit exosuit, float delay)
            {
                yield return new WaitForSeconds(delay);
                SetLights(exosuit, false);
                Main.configMain.SaveExosuitLights(exosuit.gameObject);
                //AddDebug("Set Lights off");
            }
        }

        [HarmonyPatch(typeof(SeaTruckLights))]
        class SeaTruckLights_patch
        {
            //[HarmonyPrefix, HarmonyPatch("Awake")]
            public static void AwakePrefix(SeaTruckLights __instance)
            {
                //GetSeaMothVolLight(__instance);
            }

            [HarmonyPrefix, HarmonyPatch("Start")]
            public static void StartPrefix(SeaTruckLights __instance)
            {
                if (__instance.floodLight == null)
                {
                    //AddDebug("SeaTruckLights Start floodLight == null ");
                    return;
                }
                Light[] lights = __instance.floodLight.GetComponentsInChildren<Light>();
                //AddDebug("SeaTruckLights " + __instance.floodLight.name);
                for (int i = 0; i < lights.Length; i++)
                {
                    Light light = lights[i];
                    //Main.logger.LogInfo("SeaTruck Light color " + light.color);
                    if (ConfigToEdit.seatruckLightIntensityMult.Value < 1)
                        light.intensity *= ConfigToEdit.seatruckLightIntensityMult.Value;

                    if (seatruckLightColor != default)
                        light.color = seatruckLightColor;

                    Vector3 pos;
                    if (i == 0) // left
                        pos = new Vector3(-0.01f, 0.03f, -0.25f);
                    else if (i == 1) // center
                        pos = new Vector3(0, 0.1f, -0.1f);
                    else // right
                        pos = new Vector3(0.01f, 0.03f, -0.25f);

                    AddLightBeam(light.gameObject, pos, seatruckVolLightScale);
                }
            }




        }
    }
}
