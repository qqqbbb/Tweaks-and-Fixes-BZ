using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public static class Base_Patch
    {
        static bool lightOn
        {
            get
            {
                bool on = true;
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot)
                {
                    //BaseCellLighting[] bcls = subRoot.GetComponentsInChildren<BaseCellLighting>();
                    Vector3 pos = subRoot.transform.position;
                    int x = (int)pos.x;
                    int y = (int)pos.y;
                    int z = (int)pos.z;
                    string key = x + "_" + y + "_" + z;
                    string currentSlot = SaveLoadManager.main.currentSlot;
                    if (Main.config.baseLights.ContainsKey(currentSlot) && Main.config.baseLights[currentSlot].ContainsKey(key))
                        return Main.config.baseLights[currentSlot][key];
                }
                return on;
            }
            set
            {
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot)
                {
                    Vector3 pos = subRoot.transform.position;
                    int x = (int)pos.x;
                    int y = (int)pos.y;
                    int z = (int)pos.z;
                    string key = x + "_" + y + "_" + z;
                    string currentSlot = SaveLoadManager.main.currentSlot;
                    if (Main.config.baseLights.ContainsKey(currentSlot))
                        Main.config.baseLights[currentSlot][key] = value;
                    else
                    {
                        Main.config.baseLights[currentSlot] = new Dictionary<string, bool>();
                        Main.config.baseLights[currentSlot][key] = value;
                    }
                }
            }
        }

        public static bool isLightOn(SubRoot subRoot)
        {
            BaseCellLighting[] bcls = subRoot.GetComponentsInChildren<BaseCellLighting>();
            Vector3 pos = subRoot.transform.position;
            int x = (int)pos.x;
            int y = (int)pos.y;
            int z = (int)pos.z;
            string key = x + "_" + y + "_" + z;
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (Main.config.baseLights.ContainsKey(currentSlot) && Main.config.baseLights[currentSlot].ContainsKey(key))
                return Main.config.baseLights[currentSlot][key];

            return true;
        }
         
        public static void ToggleBaseLight(SubRoot subRoot)
        {
            if (subRoot.powerRelay && subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline)
            {
                BaseCellLighting[] bcls = subRoot.GetComponentsInChildren<BaseCellLighting>();
                //AddDebug("ToggleBaseLight " + lightOn);
                lightOn = !lightOn;
                foreach (BaseCellLighting bcl in bcls)
                {
                    //AddDebug("currentIntensity " + bcl.currentIntensity);
                    //AddDebug("PowerLossValue " + bcl.GetPowerLossValue());
                    //AddDebug("appliedIntensity " + bcl.appliedIntensity);
                    bcl.ApplyCurrentIntensity();
                }
            }
        }

        //[HarmonyPatch(typeof(SubRoot), "Awake")]
        public static class SubRoot_Awake_Patch
        {
            static void Postfix(SubRoot __instance)
            {
                //Light[] lights = __instance.GetComponentsInChildren<Light>();
                if (__instance.isBase)
                {
                    bool canToggle = __instance.powerRelay && __instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Normal;
                    AddDebug("SubRoot Awake canToggle " + canToggle);
                    //if (!canToggle)
                    //    return;

                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    string key = x + "_" + y + "_" + z;
                    string currentSlot = SaveLoadManager.main.currentSlot;
                    if (Main.config.baseLights.ContainsKey(currentSlot) && Main.config.baseLights[currentSlot].ContainsKey(key))
                    {
                        lightOn = Main.config.baseLights[currentSlot][key];
                        BaseCellLighting[] bcls = __instance.GetComponentsInChildren<BaseCellLighting>();
                        //togglingLight = true;
                        AddDebug("SubRoot Awake BaseCellLighting " + bcls.Length);
                        foreach (BaseCellLighting bcl in bcls)
                        {
                            //AddDebug("currentIntensity " + bcl.currentIntensity);
                            //AddDebug("PowerLossValue " + bcl.GetPowerLossValue());
                            //AddDebug("appliedIntensity " + bcl.appliedIntensity);
                            if (bcl.GetPowerLossValue() == 0f)
                            {
                                //bcl.ApplyCurrentIntensity();
                                //ApplyIntensity(bcl, lightOn);
                            }
                        }
                        //togglingLight = false;
                        //__instance.subLightsOn = Main.config.baseLights[currentSlot][key];
                        //AddDebug(" BaseLight " + key + " " + __instance.subLightsOn);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(BaseCellLighting), "Start")]
        public class BaseCellLighting_Start_Patch : MonoBehaviour
        {
            static void Postfix(BaseCellLighting __instance)
            {
                Vector3 pos = __instance.transform.parent.position;
                int x = (int)pos.x;
                int y = (int)pos.y;
                int z = (int)pos.z;
                string key = x + "_" + y + "_" + z;
                string currentSlot = SaveLoadManager.main.currentSlot;
                if (Main.config.baseLights.ContainsKey(currentSlot) && Main.config.baseLights[currentSlot].ContainsKey(key))
                    lightOn = Main.config.baseLights[currentSlot][key];

                //ApplyIntensity(__instance, lightOn);
            }
        }

        [HarmonyPatch(typeof(BaseCellLighting), "ApplyCurrentIntensity")]
        public class BaseCellLighting_ApplyCurrentIntensity_Patch : MonoBehaviour
        {
            static bool Prefix(BaseCellLighting __instance)
            {
                if (__instance.block == null)
                    __instance.block = new MaterialPropertyBlock();

                float powerLoss = __instance.GetPowerLossValue();
                float newIntensity = powerLoss;
                if (powerLoss == 0)
                    newIntensity = lightOn ? 0 : 1;

                //AddDebug("BaseCellLighting ApplyCurrentIntensity " + newIntensity);
                if (__instance.appliedIntensity == newIntensity && !__instance.geometryChanged)
                    return false;
                __instance.appliedIntensity = newIntensity;
                __instance.interiorSky.MasterIntensity = __instance.interiorMasterIntensity.Lerp(1f - newIntensity);
                __instance.interiorSky.DiffIntensity = __instance.interiorDiffuseIntensity.Lerp(1f - newIntensity);
                __instance.interiorSky.SpecIntensity = __instance.interiorSpecIntensity.Lerp(1f - newIntensity);
                __instance.glassSky.MasterIntensity = __instance.glassMasterIntensity.Lerp(1f - newIntensity);
                __instance.glassSky.DiffIntensity = __instance.glassDiffuseIntensity.Lerp(1f - newIntensity);
                __instance.glassSky.SpecIntensity = __instance.glassSpecIntensity.Lerp(1f - newIntensity);
                foreach (Renderer renderer in __instance.interior)
                {
                    if (!(renderer == null))
                    {
                        __instance.block.Clear();
                        renderer.GetPropertyBlock(__instance.block);
                        __instance.block.SetFloat(ShaderPropertyID._UwePowerLoss, newIntensity);
                        __instance.interiorSky.ApplyToBlock(ref __instance.block, 0);
                        renderer.SetPropertyBlock(__instance.block);
                    }
                }
                foreach (Renderer renderer in __instance.glass)
                {
                    __instance.block.Clear();
                    renderer.GetPropertyBlock(__instance.block);
                    __instance.block.SetFloat(ShaderPropertyID._UwePowerLoss, newIntensity);
                    __instance.glassSky.ApplyToBlock(ref __instance.block, 0);
                    renderer.SetPropertyBlock(__instance.block);
                }
                __instance.geometryChanged = false;
                return false;
            }
        }

    }
}
