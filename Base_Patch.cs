using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;
using System.Text;
using System.Collections;

namespace Tweaks_Fixes
{
    public static class Base_Patch
    {
        static bool lightOn
        {
            get
            {
                bool on = true;
                SubRoot currentSub = Player.main.currentSub;
                if (currentSub)
                {
                    //BaseCellLighting[] bcls = subRoot.GetComponentsInChildren<BaseCellLighting>();
                    Vector3 pos = currentSub.transform.position;
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
                SubRoot currentSub = Player.main.currentSub;
                if (currentSub)
                {
                    Vector3 pos = currentSub.transform.position;
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

        //public static Dictionary<SubRoot, bool> baseBuilt = new Dictionary<SubRoot, bool>();

        [HarmonyPatch(typeof(BaseUpgradeConsoleGeometry), "GetDockedInfo")]
        public class BaseUpgradeConsoleGeometry_GetDockedInfo_Patch
        { // fix: vehicle name was not shown on upgrade console wall
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

        [HarmonyPatch(typeof(SubRoot))]
        internal class Subroot_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void PostfixAwake(SubRoot __instance)
            {
                //Light[] lights = __instance.GetComponentsInChildren<Light>();
                if (__instance.isBase)
                {
                    //if (!Main.loadingDone)
                    //    baseBuilt[__instance] = true;
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

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SubRoot __instance)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.metal;
                //if (uGUI.isLoading && __instance.powerRelay)
                //{
                //    AddDebug("SubRoot Start powerRelay " + __instance.powerRelay.isPowered);
                //    AddDebug("SubRoot Start IsHeatOnline " + __instance.IsHeatOnline());
                //}
                //if (uGUI.isLoading && __instance.powerRelay && __instance.powerRelay.isPowered)
                //{
                //    __instance.internalTemperature = __instance.heatedIndoorTargetTemperature;
                //    AddDebug("SubRoot Start " + __instance.internalTemperature);
                //}
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("Update")]
            public static bool UpdatePrefix(SubRoot __instance)
            { // fix temp updating only when player is in
                if (!Main.config.useRealTempForColdMeter)
                    return true;

                if (__instance.LOD.IsMinimal())
                    return false;

                //else if (Player.main.IsInSub() && Player.main.currentSub == __instance)
                    __instance.UpdateInternalTemperature(Time.deltaTime);
                __instance.UpdateDamageSettings();
                __instance.UpdateLighting();
                __instance.UpdateThermalReactorCharge();
                if (__instance.waterPlane != null)
                {
                    if (Player.main.IsInSub())
                        __instance.waterPlane.GetComponent<SubWaterPlane>().leakAmount = __instance.floodFraction;
                    else
                        __instance.waterPlane.GetComponent<SubWaterPlane>().leakAmount = 0f;
                }
                if (__instance.shieldFX != null && __instance.shieldFX.gameObject.activeSelf)
                {
                    __instance.shieldImpactIntensity = Mathf.MoveTowards(__instance.shieldImpactIntensity, 0f, Time.deltaTime * .25f);
                    __instance.shieldIntensity = Mathf.MoveTowards(__instance.shieldIntensity, __instance.shieldGoToIntensity, Time.deltaTime *.5f);
                    __instance.shieldFX.material.SetFloat(ShaderPropertyID._Intensity, __instance.shieldIntensity);
                    __instance.shieldFX.material.SetFloat(ShaderPropertyID._ImpactIntensity, __instance.shieldImpactIntensity);
                    if (Mathf.Approximately(__instance.shieldIntensity, 0f) && __instance.shieldGoToIntensity == 0f)
                        __instance.shieldFX.gameObject.SetActive(false);
                }
                __instance.UpdateSubModules();

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void UpdatePostfix(SubRoot __instance)
            { // fix temp updating only when player is in
                if (__instance.LOD.IsMinimal() || !Main.config.useRealTempForColdMeter)
                    return;

                if (Player.main.currentSub == null || !Player.main.currentSub.Equals(__instance))
                    __instance.UpdateInternalTemperature(Time.deltaTime);
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
                    if (renderer != null)
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
        { // dont show hand cursor
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
                if (!uGUI.isLoading)
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

        [HarmonyPatch(typeof(Aquarium), "IsAllowedToAdd")]
        class Aquarium_IsAllowedToAdd_Patch
        {
            static void Postfix(Aquarium __instance, ref bool __result, Pickupable pickupable)
            {
                //AddDebug(" Aquarium IsAllowedToAdd " + pickupable.GetTechType() + " " + __result);
                LiveMixin liveMixin = pickupable.GetComponent<LiveMixin>();
                if (liveMixin && !liveMixin.IsAlive())
                    __result = false;

                //TechType tt = CraftData.GetTechType(pickupable.gameObject);
                //if (tt == TechType.NootFish)
                //{
                //    AddDebug(" NootFish ");
                //    __result = true;
                //}
 
            }

        }


        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Construct_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("NotifyConstructedChanged")]
            public static void Postfix(Constructable __instance, bool constructed)
            {
                if (!constructed || uGUI.isLoading)
                    return;
                //Main.config.builderPlacingWhenFinishedBuilding = false;
                //AddDebug(" NotifyConstructedChanged " + __instance.techType);
                //AddDebug(" NotifyConstructedChanged isPlacing " + Builder.isPlacing);
                if (!Main.config.builderPlacingWhenFinishedBuilding)
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


        [HarmonyPatch(typeof(Bench))]
        class Bench_Patch
        {
            private static float chairRotSpeed = 70f;

            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            static bool Prefix(Bench __instance)
            {
                if (__instance.currentPlayer == null)
                    return false;

                if (__instance.isSitting)
                {
                    if (__instance.currentPlayer.GetPDA().isInUse)
                        return false;

                    if (GameInput.GetButtonDown(GameInput.Button.Exit))
                        __instance.ExitSittingMode(__instance.currentPlayer);

                    HandReticle.main.SetText(HandReticle.TextType.Use, "StandUp", true, GameInput.Button.Exit);
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    if (tt == TechType.StarshipChair)
                    {
                        HandReticle.main.SetText(HandReticle.TextType.UseSubscript, UI_Patches.swivelText, false);
                        if (GameInput.GetButtonHeld(GameInput.Button.MoveRight))
                            __instance.transform.Rotate(Vector3.up * chairRotSpeed * Time.deltaTime);
                        else if (GameInput.GetButtonHeld(GameInput.Button.MoveLeft))
                            __instance.transform.Rotate(-Vector3.up * chairRotSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    __instance.Subscribe(__instance.currentPlayer, false);
                    __instance.currentPlayer = null;
                }
                return false;
            }


        }




    }
}
