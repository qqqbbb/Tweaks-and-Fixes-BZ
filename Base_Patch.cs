using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public static class Base_Patch
    {

        public static Dictionary<BaseHullStrength, SubRoot> baseHullStrengths = new Dictionary<BaseHullStrength, SubRoot>();

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

        public static void ToggleBaseLight(SubRoot subRoot)
        {
            if (subRoot.powerRelay == null || subRoot.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                return;

            //AddDebug("ToggleBaseLight " + lightOn);
            subRoot.subLightsOn = !subRoot.subLightsOn;
            if (subRoot.subLightsOn)
                Main.configMain.DeleteBaseLights(subRoot.transform.position);
            else
                Main.configMain.SaveBaseLights(subRoot.transform.position);

            BaseCellLighting[] bcls = subRoot.GetComponentsInChildren<BaseCellLighting>();
            foreach (BaseCellLighting bcl in bcls)
            {
                //AddDebug("currentIntensity " + bcl.currentIntensity);
                //AddDebug("PowerLossValue " + bcl.GetPowerLossValue());
                //AddDebug("appliedIntensity " + bcl.appliedIntensity);
                bcl.ApplyCurrentIntensity();
            }
        }

        [HarmonyPatch(typeof(BaseHullStrength))]
        class BaseHullStrength_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnPostRebuildGeometry")]
            static bool OnPostRebuildGeometryPrefix(BaseHullStrength __instance)
            {
                if (ConfigMenu.baseHullStrengthMult.Value == 1)
                    return true;

                if (!GameModeManager.GetOption<bool>(GameOption.BaseWaterPressureDamage))
                    return false;

                float strength = BaseHullStrength.InitialStrength * ConfigMenu.baseHullStrengthMult.Value;
                __instance.victims.Clear();
                foreach (Int3 cell in __instance.baseComp.AllCells)
                {
                    if (__instance.baseComp.GridToWorld(cell).y < 0)
                    {
                        Transform cellObject = __instance.baseComp.GetCellObject(cell);
                        if (cellObject != null)
                        {
                            __instance.victims.Add(cellObject.GetComponent<LiveMixin>());
                            strength += __instance.baseComp.GetHullStrength(cell);
                        }
                    }
                }
                if (Main.gameLoaded && GameModeManager.GetOption<bool>(GameOption.BaseWaterPressureDamage) && !Mathf.Approximately(strength, __instance.totalStrength))
                    AddMessage(Language.main.GetFormat("BaseHullStrChanged", strength - __instance.totalStrength, strength));

                __instance.totalStrength = strength;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("CrushDamageUpdate")]
            static bool CrushDamageUpdatePrefix(BaseHullStrength __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                if (!GameModeManager.GetOption<bool>(GameOption.BaseWaterPressureDamage) || __instance.totalStrength >= 0 || __instance.victims.Count <= 0)
                    return false;

                LiveMixin random = __instance.victims.GetRandom();
                random.TakeDamage(BaseHullStrength.damagePerCrush, random.transform.position, DamageType.Pressure);
                int index = 0;
                if (__instance.totalStrength <= -3.0)
                    index = 2;
                else if (__instance.totalStrength <= -2.0)
                    index = 1;

                if (!baseHullStrengths.ContainsKey(__instance))
                {
                    baseHullStrengths[__instance] = __instance.GetComponent<SubRoot>();
                }
                else if (baseHullStrengths[__instance] == Player.main.currentSub)
                {
                    //AddDebug("Player inside");
                    if (__instance.crushSounds[index] != null)
                        Utils.PlayFMODAsset(__instance.crushSounds[index], random.transform);

                    AddMessage(Language.main.GetFormat("BaseHullStrDamageDetected", __instance.totalStrength));
                }
                return false;
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
                if (__instance.isCyclops)
                    return;
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
                if (Main.configMain.baseLights.ContainsKey(currentSlot) && Main.configMain.baseLights[currentSlot].Contains(key))
                {
                    ToggleBaseLight(__instance);
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
                            //bcl.ApplyCurrentIntensity();
                            //ApplyIntensity(bcl, lightOn);
                        }
                    }
                    //togglingLight = false;
                    //__instance.subLightsOn = Main.config.baseLights[currentSlot][key];
                    //AddDebug(" BaseLight " + key + " " + __instance.subLightsOn);
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


            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void UpdatePostfix(SubRoot __instance)
            { // fix temp updating only when player is in
                if (!Main.gameLoaded)
                    return;

                if (__instance.LOD.IsMinimal() || !ConfigMenu.useRealTempForPlayerTemp.Value)
                    return;

                if (Player.main.currentSub == null || Player.main.currentSub != __instance)
                    __instance.UpdateInternalTemperature(Time.deltaTime);
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetInsideTemperature")]
            public static void GetInsideTemperaturePostfix(SubRoot __instance, ref float __result)
            {
                //if (Main.config.useRealTempForColdMeter)
                __result = ConfigToEdit.insideBaseTemp.Value;
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
                    newIntensity = Main.configMain.GetBaseLights() ? 0 : 1;

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

        [HarmonyPatch(typeof(SolarPanel))]
        public static class SolarPanel_Patch
        { // dont show hand icon
            [HarmonyPrefix]
            [HarmonyPatch("OnHandHover")]
            static bool OnHandHoverPrefix(SolarPanel __instance, GUIHand hand)
            {
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (!c || !c.constructed)
                    return false;
                HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<int, int, int>("SolarPanelStatus", Mathf.RoundToInt(__instance.GetRechargeScalar() * 100f), Mathf.RoundToInt(__instance.powerSource.GetPower()), Mathf.RoundToInt(__instance.powerSource.GetMaxPower())), false);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                //HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            static void StartPrefix(SolarPanel __instance)
            {
                __instance.maxDepth = ConfigToEdit.solarPanelMaxDepth.Value;
            }
        }


        [HarmonyPatch(typeof(FMOD_CustomEmitter), "Awake")]
        class FMOD_CustomEmitter_Awake_Patch
        {
            static void Postfix(FMOD_CustomEmitter __instance)
            {
                if (ConfigToEdit.silentReactor.Value && __instance.asset && __instance.asset.path == "event:/sub/base/nuke_gen_loop")
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
            }
        }

        [HarmonyPatch(typeof(Bench))]
        class Bench_Patch
        {
            private static float chairRotSpeed = 70f;

            [HarmonyPostfix, HarmonyPatch("Update")]
            static void UpdatePostfix(Bench __instance)
            {
                if (__instance.currentPlayer == null || __instance.isSitting == false || __instance.currentPlayer.GetPDA().isInUse)
                    return;

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
        }

        [HarmonyPatch(typeof(ToggleOnClick), "SwitchOn")]
        internal class ToggleOnClick_Patch
        {
            public static void Postfix(ToggleOnClick __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //AddDebug("SwitchOn " + tt);
                if (tt == TechType.SmallStove)
                {
                    PlayerTool heldTool = Inventory.main.GetHeldTool();
                    if (heldTool)
                    {
                        GameObject go = heldTool.gameObject;
                        if (Util.IsCreatureAlive(go) && Util.IsEatableFish(go))
                            Util.CookFish(go);
                    }
                }
            }
        }







    }
}
