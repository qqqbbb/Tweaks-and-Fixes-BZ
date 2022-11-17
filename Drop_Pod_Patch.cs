using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Drop_Pod_Patch
    {
        static Light podLight;
        public static PowerSource podPowerSource;
        static GhostCrafter podGhostCrafter;

        class RegenerateSunPowerSource : MonoBehaviour
        {
            //float regenerationThreshhold = 25f;
            float regenerationInterval = 10f;
            float regenerationAmount = 1f;
            float lightPowerCost = .025f;

            void Start()
            {
                //AddDebug("RegenerateSunPowerSource Start");
                //regenerationThreshhold = Main.config.dropPodMaxPower;
                this.InvokeRepeating("Regenerate", 0f, regenerationInterval);
            }

            void Regenerate()
            {
                float amount = podPowerSource.power - lightPowerCost + regenerationAmount * DayNightCycle.main.GetLocalLightScalar();
                podPowerSource.SetPower(amount);
                //AddDebug("RegenerateSunPowerSource Regenerate " + amount);
                if (podLight)
                    podLight.enabled = podPowerSource.power > 0f;
            }
        }

        [HarmonyPatch(typeof(Fabricator), "Start")]
        class Fabricator_Start_Patch
        {
            static void Postfix(Fabricator __instance)
            {
                if (Main.config.dropPodMaxPower == 0)
                    return;

                GameObject dropPod = __instance.transform.parent.gameObject;
                if (dropPod.GetComponent<LifepodDrop>())
                {
                    podGhostCrafter = __instance;
                    podLight = dropPod.GetComponentInChildren<Light>();
                    __instance.needsPower = true;
                    //AddDebug("Fabricator Start needsPower " + __instance.needsPower);
                    podPowerSource = __instance.gameObject.EnsureComponent<PowerSource>();
                    podPowerSource.maxPower = Main.config.dropPodMaxPower;
                    __instance.gameObject.AddComponent<RegenerateSunPowerSource>();
                    if (Main.config.podPower.ContainsKey(SaveLoadManager.main.currentSlot))
                        podPowerSource.power = Main.config.podPower[SaveLoadManager.main.currentSlot];
                }
            }
        }

        [HarmonyPatch(typeof(GhostCrafter), "OnHandHover")]
        class GhostCrafter_OnHandHover_Patch
        {
            static bool Prefix(GhostCrafter __instance, GUIHand hand)
            {
                if (Main.config.dropPodMaxPower == 0 || __instance != podGhostCrafter)
                    return true;
                //AddDebug("GhostCrafter OnHandHover " + __instance.powerRelay.GetPower());
                if (!__instance.enabled || __instance.logic == null)
                    return false;

                string text1 = __instance.handOverText;
                string text2 = string.Empty;
                if (__instance.logic.inProgress)
                {
                    text1 = __instance.logic.craftingTechType.AsString();
                    HandReticle.main.SetProgress(__instance.logic.progress);
                    HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1.5f);
                }
                else if (__instance.HasCraftedItem())
                {
                    text1 = __instance.logic.currentTechType.AsString();
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                }
                else
                {
                    if (__instance.HasEnoughPower())
                        text2 = Language.main.GetFormat<int, int>("PowerCellStatus", Mathf.FloorToInt(__instance.powerRelay.GetPower()), Mathf.FloorToInt(__instance.powerRelay.GetMaxPower()));
                    else
                        text2 = "unpowered";

                    HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                }
                HandReticle.main.SetText(HandReticle.TextType.Hand, text1, true, GameInput.Button.LeftHand);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, true);
                return false;
            }
        }

        //[HarmonyPatch(typeof(LifepodDrop), "Start")]
        class LifepodDrop_Start_Patch
        {
            static void Postfix(LifepodDrop __instance)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.metal;
            }
        }

    }
}
