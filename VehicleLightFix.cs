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
        public static Vector3 seatruckVolLightScale = new Vector3(3, 3, 3);
        static WaitUntil volLightBeamNotNull = new WaitUntil(() => volLightBeam != null);
        public static bool fixed_;
        public static FMODAsset lightOnSound;
        public static FMODAsset lightOffSound;

        private static void CreateExosuitSounds(GameObject exosuit)
        {
            lightOnSound = ScriptableObject.CreateInstance<FMODAsset>();
            lightOnSound.path = "event:/sub/seamoth/seaglide_light_on";
            lightOnSound.id = "{fe76457f-0c94-4245-a080-8a5b2f8853c4}";
            lightOffSound = ScriptableObject.CreateInstance<FMODAsset>();
            lightOffSound.path = "event:/sub/seamoth/seaglide_light_off";
            lightOffSound.id = "{b52592a9-19f5-45d1-ad56-7d355fc3dcc3}";
            CollisionSound collisionSound = exosuit.EnsureComponent<CollisionSound>();
            FMODAsset so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/common/fishsplat";
            so.id = "{0e47f1c6-6178-41bd-93bf-40bfca179cb6}";
            collisionSound.hitSoundSmall = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_hard";
            so.id = "{ed65a390-2e80-4005-b31b-56380500df33}";
            collisionSound.hitSoundFast = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_medium";
            so.id = "{cb2927bf-3f8d-45d8-afe2-c82128f39062}";
            collisionSound.hitSoundMedium = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_soft";
            so.id = "{15dc7344-7b0a-4ffd-9b5c-c40f923e4f4d}";
            collisionSound.hitSoundSlow = so;
        }

        public IEnumerator GetLightBeamPrefab()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Flashlight);
            yield return request;
            GameObject prefab = request.GetResult();
            Transform cone = prefab.transform.Find("lights_parent/x_flashlightCone");
            volLightBeam = cone.gameObject;
        }

        public static IEnumerator AddLightBeam(GameObject parent, Vector3 pos = default, Vector3 scale = default)
        {
            yield return volLightBeamNotNull;
            GameObject lightBeam = UnityEngine.Object.Instantiate(volLightBeam, Vector3.zero, Quaternion.identity);
            lightBeam.transform.parent = parent.transform;
            lightBeam.transform.localPosition = pos;
            lightBeam.transform.localRotation = Quaternion.Euler(volLightRot);
            if (scale != default)
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

        private static void ToggleExosuitLights(Exosuit exosuit)
        {
            Transform lightParent = Util.GetExosuitLightsTransform(exosuit);
            //AddDebug("IngameMenu isActiveAndEnabled " + IngameMenu.main.isActiveAndEnabled);
            if (!lightParent.gameObject.activeSelf && exosuit.energyInterface.hasCharge)
            {
                lightParent.gameObject.SetActive(true);
                Main.configMain.DeleteExosuitLights(exosuit.gameObject);
                if (lightOnSound)
                    Utils.PlayFMODAsset(lightOnSound, exosuit.gameObject.transform.position);
            }
            else if (lightParent.gameObject.activeSelf)
            {
                lightParent.gameObject.SetActive(false);
                Main.configMain.SaveExosuitLights(exosuit.gameObject);
                if (lightOffSound)
                    Utils.PlayFMODAsset(lightOffSound, exosuit.gameObject.transform.position);
            }
            //AddDebug("lights " + lightsT.gameObject.activeSelf);
        }

        private static void SetExosuitLights(Exosuit exosuit, bool on)
        {
            Util.GetExosuitLightsTransform(exosuit).gameObject.SetActive(on);
        }

        public IEnumerator FixExosuit()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Exosuit);
            yield return request;
            GameObject prefab = request.GetResult();
            Transform lights_parent = prefab.transform.Find("lights_parent");
            Exosuit exosuit = prefab.GetComponent<Exosuit>();
            lights_parent.SetParent(exosuit.leftArmAttach);
            FixExosuitLight(exosuit);
            CreateExosuitSounds(prefab);
            EnergyEffect energyEffect = prefab.GetComponent<EnergyEffect>();
            // it turns off lights when left battery is removed
            UnityEngine.Object.Destroy(energyEffect);
        }

        public IEnumerator FixSeaTruckLight()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.SeaTruck);
            yield return request;
            GameObject prefab = request.GetResult();
            SeaTruckLights seaTruckLights = prefab.GetComponent<SeaTruckLights>();
            Light[] lights = seaTruckLights.floodLight.GetComponentsInChildren<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
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

                UWE.CoroutineHost.StartCoroutine(AddLightBeam(light.gameObject, pos, seatruckVolLightScale));
            }
        }

        private void FixExosuitLight(Exosuit exosuit)
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
                    pos = new Vector3(-0.05f, -0.2f, -0.5f);
                else
                    pos = new Vector3(0, -0.2f, -0.5f);

                UWE.CoroutineHost.StartCoroutine(AddLightBeam(light.gameObject, pos, seatruckVolLightScale));
            }
            lightTransform.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(Vehicle))]
        class Vehicle_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnPoweredChanged")]
            public static void OnPoweredChangedPostfix(Vehicle __instance, bool powered)
            {
                //AddDebug("Vehicle OnPoweredChanged " + powered);
                Exosuit exosuit = __instance as Exosuit;
                if (exosuit)
                {
                    Transform lightParent = Util.GetExosuitLightsTransform(exosuit);
                    lightParent.gameObject.SetActive(powered);
                }
            }
        }

        [HarmonyPatch(typeof(Exosuit))]
        class Exosuit_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void Startostfix(Exosuit __instance)
            {
                if (Main.gameLoaded == false)
                {
                    bool off = Main.configMain.GetExosuitLights(__instance.gameObject);
                    SetExosuitLights(__instance, !off);
                }
            }

            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(Exosuit __instance)
            {
                if (Main.gameLoaded == false)
                    return;

                if (!IngameMenu.main.isActiveAndEnabled && !Player.main.pda.isInUse && Player.main.currentMountedVehicle == __instance)
                {
                    if (GameInput.GetButtonDown(GameInput.Button.MoveDown))
                        ToggleExosuitLights(__instance);
                }
            }

            [HarmonyPostfix, HarmonyPatch("EnterVehicle")]
            public static void EnterVehiclePostfix(Exosuit __instance)
            {
                CoroutineHost.StartCoroutine(DisableLightBeam(__instance));
            }

            static IEnumerator DisableLightBeam(Exosuit exosuit)
            {
                yield return Main.waitUntilGameLoaded;
                ToggleLightBeam(exosuit, false);
            }

            private static void ToggleLightBeam(Exosuit exosuit, bool on)
            {
                Transform lightT = Util.GetExosuitLightsTransform(exosuit);
                Light[] lights = lightT.GetComponentsInChildren<Light>();
                foreach (var light in lights)
                {
                    foreach (Transform beam in light.transform)
                        beam.gameObject.SetActive(on);
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
                SetExosuitLights(exosuit, false);
                Main.configMain.SaveExosuitLights(exosuit.gameObject);
                //AddDebug("Set Lights off");
            }
        }

        //[HarmonyPatch(typeof(SeaTruckLights))]
        class SeaTruckLights_patch
        {
            //[HarmonyPrefix, HarmonyPatch("Awake")]
            public static void AwakePrefix(SeaTruckLights __instance)
            {
                //GetSeaMothVolLight(__instance);
            }

            //[HarmonyPrefix, HarmonyPatch("Start")]
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

                    UWE.CoroutineHost.StartCoroutine(AddLightBeam(light.gameObject, pos, seatruckVolLightScale));
                }
            }





        }
    }
}
