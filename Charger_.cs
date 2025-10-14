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
    [HarmonyPatch(typeof(Charger))]
    internal class Charger_
    {
        public static HashSet<TechType> notRechargableBatteries = new HashSet<TechType>();

        public static IEnumerator CloseUIafterAnimationFinished(Charger charger)
        {
            //AddDebug("WaitForAnimationToFinish " + charger.animTimeOpen);
            yield return new WaitForSeconds(charger.animTimeOpen);
            //AddDebug("WaitForAnimationToFinish !");
            charger.ui.SetActive(false);
        }

        [HarmonyPrefix, HarmonyPatch("ToggleUI")]
        public static bool ToggleUIPrefix(Charger __instance, bool active)
        {
            //AddDebug($"ToggleUI {active}");
            if (active == false)
            {
                CoroutineHost.StartCoroutine(CloseUIafterAnimationFinished(__instance));
                return false;
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch("ToggleUIPowered")]
        public static void ToggleUIPoweredPostfix(Charger __instance, bool powered)
        {
            //AddDebug($"ToggleUIPowered {powered} ui.activeSelf {__instance.ui.activeSelf}");
            bool powered_ = __instance.powerConsumer.IsPowered();
            if (__instance.ui.activeSelf && powered_ == false)
                __instance.ui.SetActive(false);
            else if (__instance.ui.activeSelf == false && powered_)
                __instance.ui.SetActive(true);
        }

        [HarmonyPrefix, HarmonyPatch("OnHandClick")]
        public static bool OnHandClickPrefix(Charger __instance)
        {
            //AddDebug($"nextChargeAttemptTimer {__instance.nextChargeAttemptTimer}");
            if (__instance.opened == false && __instance.powerConsumer.IsPowered() == false)
                return false;

            bool animPlaying = Util.IsAnimationPlaying(__instance.animator);
            //AddDebug($"OnHandClick {animPlaying}");
            return animPlaying == false;
        }

        [HarmonyPrefix, HarmonyPatch("OnCloseCallback")]
        public static bool OnCloseCallbackPreix(Charger __instance)
        {
            if (__instance.enabled && __instance.opened)
            { // dont play animation when unpowered
                if (__instance.powerConsumer.IsPowered() == false)
                    return false;
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(Charger __instance)
        {
            //AddDebug(__instance.name + " Charger Start");
            if (__instance.allowedTech == null)
                return;

            foreach (TechType tt in notRechargableBatteries)
            {
                if (__instance.allowedTech.Contains(tt))
                {
                    __instance.allowedTech.Remove(tt);
                    //AddDebug("remove " + tt + " from " + __instance.name);
                }
            }
            //Main.logger.LogMessage(__instance.name + " Charger Start");
            //foreach (var tt in __instance.allowedTech)
            //    Main.logger.LogMessage(__instance.name + " allowedTech " + tt);
        }


    }
}
