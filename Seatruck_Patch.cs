using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Seatruck_Patch
    {
        public static GameObject wcpGO = null;
        //public static GameObject seaTruckAquarium = null;
        //public static List<GameObject> seatruckModules = new List<GameObject>();
        public static HashSet<TechType> installedUpgrades = new HashSet<TechType>();
        static string uiText = string.Empty;
        static string uiTextSub = string.Empty;
        static FMODAsset stopPilotSound;

        public static void GetUpgradesNames(bool defenseSelected = false)
        {
            string lightToggle = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.RightHand);
            string defenseName = Language.main.Get(TechType.SeaTruckUpgradePerimeterDefense);
            if (Language.main.GetCurrentLanguage() == "English")
                defenseName = defenseName.Replace(" Upgrade", "");

            string exitButton = " Stop piloting (" + uGUI.FormatButton(GameInput.Button.Exit) + ")";
            uiTextSub = string.Empty;
            if (installedUpgrades.Contains(TechType.SeaTruckUpgradeAfterburner))
                uiTextSub = LanguageCache.GetButtonFormat("ExosuitBoost", GameInput.Button.Sprint) + " ";
            if (defenseSelected)
                uiText = defenseName + ". Press and hold " + uGUI.FormatButton(GameInput.Button.LeftHand
                    ) + " to charge the shot.";
            else
                uiText = string.Empty;

            //useButton = currentModuleName + ". Press and hold " + TooltipFactory.stringLeftHand + " to charge the shot.";
            uiTextSub += lightToggle + exitButton;
            //AddDebug("GetUpgradesNames activeSlot " + seaTruckUpgrades.activeSlot);
        }

        [HarmonyPatch(typeof(SeaTruckSegment))]
        class SeaTruckSegment_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SeaTruckSegment __instance)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.metal;
                DealDamageOnImpact ddoi = __instance.GetComponent<DealDamageOnImpact>();
                if (ddoi && ddoi.impactSound)
                {
                    SoundOnDamage sod = __instance.gameObject.EnsureComponent<SoundOnDamage>();
                    sod.sound = ddoi.impactSound.asset;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnClickHatch")]
            public static bool OnClickHatchPrefix(SeaTruckSegment __instance, HandTargetEventData eventData)
            { // delay exit sound when exiting cabin or docking module
                //AddDebug("SeaTruckSegment OnClickHatch");
                if (__instance.player == null && !__instance.CanEnter())
                    return false;

                Player player = eventData.guiHand.player;
                bool playerIsOutside = __instance.PlayerIsOutside();
                if (player != null && !playerIsOutside)
                {
                    Vector3 exitPoint;
                    bool skipAnimations;
                    if (__instance.FindExitPoint(out exitPoint, out skipAnimations, SeaTruckAnimation.Animation.Exit))
                    {
                        //AddDebug("name" + __instance.name);
                        //AddDebug("parent " + __instance.transform.parent.name);
                        if (__instance.isMainCab)
                        {
                            __instance.StartCoroutine(Util.PlaySound(__instance.exitSound, .5f));
                            if (stopPilotSound)
                                __instance.StartCoroutine(Util.PlaySound(stopPilotSound, .5f));
                        }
                        else if (CraftData.GetTechType(__instance.gameObject) == TechType.SeaTruckDockingModule)
                        {
                            __instance.StartCoroutine(Util.PlaySound(__instance.exitSound, 1.5f));
                            if (stopPilotSound)
                                __instance.StartCoroutine(Util.PlaySound(stopPilotSound, 1.5f));
                        }
                        else
                        {
                            Utils.PlayFMODAsset(__instance.exitSound, player.transform);
                            if (stopPilotSound)
                                Utils.PlayFMODAsset(stopPilotSound, player.transform);
                        }
                        __instance.Exit(new Vector3?(exitPoint), skipAnimations, playerOverride: player);
                        if (__instance.CanAnimate() && !skipAnimations)
                            __instance.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.Exit;
                    }
                }
                else if (player != null & playerIsOutside)
                {
                    bool exitPoint = __instance.FindExitPoint(out Vector3 _, out bool _);
                    //if (!__instance.IsWalkable() && !SeaTruckSegment.GetHead(__instance).isMainCab)
                    //    AddError(Language.main.Get("EnterFailedTooSteep"));
                    if (exitPoint)
                        __instance.EnterHatch(player);
                    //else
                    //    AddError(Language.main.Get("EnterFailedNoSpace"));
                }
                if (__instance.player)
                    __instance.PropagatePlayer();

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Exit")]
            public static void ExitPostfix(SeaTruckSegment __instance)
            {
                //AddDebug("isMainCab " + __instance.isMainCab);
                //AddDebug("isRearConnected " + __instance.isRearConnected);
                if (__instance.isMainCab && !__instance.isRearConnected)
                {
                    __instance.StartCoroutine(Util.PlaySound(__instance.exitSound, .5f));
                    if (stopPilotSound)
                        __instance.StartCoroutine(Util.PlaySound(stopPilotSound, .5f));
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("IInteriorSpace.GetInsideTemperature")]
            public static void GetInsideTemperaturePostfix(SeaTruckSegment __instance, ref float __result)
            {
                if (ConfigMenu.useRealTempForPlayerTemp.Value && !__instance.relay.IsPowered())
                    __result = WaterTemperatureSimulation.main.GetTemperature(__instance.transform.position);
                else
                    __result = ConfigToEdit.insideBaseTemp.Value;
                //AddDebug("SeaTruckSegment GetInsideTemperature " + __result);
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnHoverHatch")]
            public static void OnHoverHatchPostfix(SeaTruckSegment __instance)
            {
                AddDebug("SeaTruckSegment OnHoverHatch isRearConnected " + __instance.isRearConnected);
                if (Input.GetKeyDown(KeyCode.C) && stopPilotSound)
                {
                    AddDebug("stopPilotSound");
                    Utils.PlayFMODAsset(stopPilotSound, __instance.transform);
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    AddDebug("exitSound");
                    Utils.PlayFMODAsset(__instance.exitSound, __instance.transform);
                }
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    AddDebug("enterSound");
                    Utils.PlayFMODAsset(__instance.enterSound, __instance.transform);
                }
            }

            //[HarmonyPostfix, HarmonyPatch("UpdatePowerRelay")]
            public static void UpdatePowerRelayPostfix(SeaTruckSegment __instance)
            {
                AddDebug("SeaTruckSegment UpdatePowerRelay ");

            }
        }


        //[HarmonyPatch(typeof(SeaTruckLights), "Start")]
        class SeaTruckLights_Start_Patch
        {
            public static void Prefix(SeaTruckLights __instance)
            {
                //if (__instance.name == "SeaTruck(Clone)")
                //{
                //    Transform transform = __instance.transform.Find("model/seatruck_anim/Seatruck_Interior_geo/");
                //    SkinnedMeshRenderer smr = transform.GetComponent<SkinnedMeshRenderer>();
                //    AddDebug("SeaTruck material name " + smr.materials[2].name);
                //    foreach (Material m in smr.materials)
                //    {
                //m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                //m.shaderKeywords = new string[] { "MARMO_SPECMAP", "UWE_3COLOR", "_NORMALMAP", "_ZWRITE_ON" };
                //Main.Log("SeaTruck shaderKeywords " + item);
                //}
                //smr.materials[2].globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                //smr.materials[2].shaderKeywords = new string[0];

                //}
                //if (__instance.name == "SeaTruckStorageModule(Clone)")
                // fix storage module lights 
                __instance.lightingController = __instance.GetComponent<LightingController>();
                //AddDebug(__instance.name + "SeaTruckStorageModule fix lights " );
                //}
                //Light[] lights = Main.GetComponentsInDirectChildren<Light>(__instance, true);
                //foreach (Light light in lights)
                //{
                //light.enabled = false;
                //Main.Log(light.name + " turnofflights " + Main.GetParent(__instance.gameObject));
                //AddDebug(light.name + " turnofflights " + Main.GetParent(__instance.gameObject));
                //}
            }
        }

        [HarmonyPatch(typeof(LightingController), "LerpToState", new Type[] { typeof(int), typeof(float) })]
        class LightingController_LerpToState_Patch
        { // turn off light in teleporter and docking module
            static int prevState = -1;
            public static void Prefix(LightingController __instance, int targetState)
            {
                prevState = (int)__instance.state;
            }
            public static void Postfix(LightingController __instance, int targetState)
            {
                if (prevState == targetState || !__instance.GetComponent<SeaTruckSegment>())
                    return;

                //AddDebug(__instance.name + " lights " + (LightingController.LightingState)targetState);
                bool off = (LightingController.LightingState)targetState == LightingController.LightingState.Damaged;
                ToggleLights(__instance.gameObject, !off);
            }

            private static void ToggleLights(GameObject go, bool on)
            {
                Light[] lights = Util.GetComponentsInDirectChildren<Light>(go, true);
                foreach (Light light in lights)
                    light.enabled = on;
                //AddDebug(go.name + " ToggleLights ");
                Transform jukebox = go.transform.Find("Jukebox/UI");
                if (jukebox)
                    jukebox.gameObject.SetActive(on);
            }
        }

        //[HarmonyPatch(typeof(LightingController), "Update")]
        class LightingController_Update_Patch
        {
            public static bool Prefix(LightingController __instance)
            {

                float deltaTime = Time.deltaTime;
                if (deltaTime <= 0F)
                    return false;
                //AddDebug(__instance.name + " Update lights " + __instance.fadeDuration);
                __instance.timer.Update(deltaTime);
                int state = (int)__instance.state;
                if (__instance.prevState != state)
                {
                    //AddDebug(__instance.name + " Update lights ");
                    __instance.LerpToState(state);
                    __instance.prevState = state;
                }
                __instance.UpdateIntensities();
                return false;
            }
        }

        //[HarmonyPatch(typeof(SeaTruckSegment), "Start")]
        class SeaTruckSegment_Start_Patch
        {
            public static void Postfix(SeaTruckSegment __instance)
            {
                //if (__instance.isMainCab)
                //    seaTruckCab = __instance.gameObject;
                Transform wcpTransform = __instance.transform.Find("WaterClipProxy");
                if (wcpTransform)
                {
                    //AddDebug("WaterClipProxy");
                    //Main.Log("WaterClipProxy");
                    wcpGO = wcpTransform.gameObject;
                }
                //else if (Main.GetComponentsInDirectChildren<SeaTruckAquarium>(__instance).Length > 0)
                {
                    //AddDebug("SeaTruckAquarium");
                    //Main.Log("SeaTruckAquarium");
                    //seaTruckAquarium = Main.GetParent(__instance.gameObject);
                    //if (wcpGO)
                    //    DoStuff(__instance.transform.parent.gameObject);

                    //else
                    //    Main.Log("Aquarium start no seaTruckCab " + __instance.name);
                }


                //seaTruckUpgrades = __instance;
            }
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades))]
        class SeaTruckUpgrades_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnUpgradeModuleChange")]
            public static void OnUpgradeModuleChangePostfix(SeaTruckUpgrades __instance, TechType techType, bool added)
            {// this used to somehow break slot extender mod
                //powerUpgrades = GetNumPowerUpgrades(__instance);
                //AddDebug("OnUpgradeModuleChange " + techType + " " + added);
                if (added)
                    installedUpgrades.Add(techType);
                else
                    installedUpgrades.Remove(techType);
                //AddDebug("powerUpgrades " + powerUpgrades);
                GetUpgradesNames();
            }

            [HarmonyPostfix]
            [HarmonyPatch("NotifySelectSlot")]
            public static void NotifySelectSlotPostfix(SeaTruckUpgrades __instance, int slotID)
            {
                //Main.Log("SeaTruckUpgrades NotifySelectSlot " + slotID);
                if (slotID == -1)
                    return;

                TechType tt = __instance.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
                if (tt == TechType.SeaTruckUpgradePerimeterDefense)
                    GetUpgradesNames(true);
                else
                    GetUpgradesNames();
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor))]
        class SeaTruckMotor_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SeaTruckMotor __instance)
            {
                stopPilotSound = __instance.stopPilotSound;
                __instance.stopPilotSound = null;// dont play exit sound when not exiting cabin 
            }

            [HarmonyPostfix]
            [HarmonyPatch("StartPiloting")]
            public static void StartPilotingPostfix(SeaTruckMotor __instance)
            {
                GetUpgradesNames();
            }

        }


    }
}
