using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;
using System.Text;

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

        [HarmonyPatch(typeof(SubRoot), "Awake")]
        public static class SubRoot_Awake_Patch
        {
            static void Postfix(SubRoot __instance)
            {
                //Light[] lights = __instance.GetComponentsInChildren<Light>();
                if (__instance.isBase)
                {
                    //bool canToggle = __instance.powerRelay && __instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Normal;
                    //AddDebug("SubRoot Awake canToggle " + canToggle);
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
                        //AddDebug("saved Lights " + lightOn);
                        //AddDebug("saved BaseCellLighting " + bcls.Length);
                        foreach (BaseCellLighting bcl in bcls)
                        {
                            //AddDebug("currentIntensity " + bcl.currentIntensity);
                            //AddDebug("PowerLossValue " + bcl.GetPowerLossValue());
                            //AddDebug("appliedIntensity " + bcl.appliedIntensity);
                            if (bcl.GetPowerLossValue() == 0f)
                            {
                                bcl.ApplyCurrentIntensity();
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

        [HarmonyPatch(typeof(BaseUpgradeConsoleGeometry), "GetDockedInfo")]
        public class BaseUpgradeConsoleGeometry_GetDockedInfo_Patch
        {
            static bool Prefix(BaseUpgradeConsoleGeometry __instance, Dockable dockable, ref string __result)
            {
                if (dockable == null)
                {
                    __result = "";
                    return false;
                }
                NamePlate namePlate = dockable.GetComponent<NamePlate>();
                string str = namePlate ? namePlate._name : string.Empty;
                StringBuilder stringBuilder = new StringBuilder(str);
                stringBuilder.Append(' ');
                stringBuilder.Append(Language.main.Get("SubmersibleDocked"));
                if (dockable.UsesEnergy())
                {
                    float energyScalar = dockable.GetEnergyScalar();
                    stringBuilder.Append('\n');
                    stringBuilder.Append("<size=30>");
                    if (energyScalar == 1f)
                    {
                        stringBuilder.Append(Language.main.Get("SubmersibleFullyCharged"));
                    }
                    else
                    {
                        stringBuilder.Append(Language.main.Get("SubmersibleCharging"));
                        stringBuilder.Append(' ');
                        stringBuilder.Append(Mathf.RoundToInt(energyScalar * 100f).ToString());
                        stringBuilder.Append('%');
                    }
                    stringBuilder.Append("</size>");
                }
                __result = stringBuilder.ToString();
                return false;
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

        [HarmonyPatch(typeof(SolarPanel), "OnHandHover")]
        public static class SolarPanel_OnHandHover_Patch
        {
            static bool Prefix(SolarPanel __instance, GUIHand hand)
            {
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (!c || !c.constructed)
                    return false;
                HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<int, int, int>("SolarPanelStatus", Mathf.RoundToInt(__instance.GetRechargeScalar() * 100f), Mathf.RoundToInt(__instance.powerSource.GetPower()), Mathf.RoundToInt(__instance.powerSource.GetMaxPower())), false);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                //HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
        }

        [HarmonyPatch(typeof(CoffeeVendingMachine), "Start")]
        public static class CoffeeVendingMachine_Start_Patch
        {
            static bool Prefix(CoffeeVendingMachine __instance)
            {
                //AddDebug("CoffeeVendingMachine Start");
                if (Main.loadingDone)
                    __instance.idleSound.Play();
                return false;
            }
        }

        [HarmonyPatch(typeof(FMOD_CustomEmitter), "Awake")]
        class FMOD_CustomEmitter_Awake_Patch
        {
            static void Postfix(FMOD_CustomEmitter __instance)
            {
                if (Main.config.silentReactor && __instance.asset && __instance.asset.path == "event:/sub/base/nuke_gen_loop")
                {
                    //AddDebug(__instance.name + " FMOD_CustomEmitter Awake ");
                    __instance.SetAsset(null);
                }
            }
        }


    }
}
