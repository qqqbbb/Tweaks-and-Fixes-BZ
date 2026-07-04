using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(ThermalPlant))]

    internal class ThermalPlant_
    {
        [HarmonyPostfix, HarmonyPatch("UpdateUI")]
        public static void UpdateUIPostfix(ThermalPlant __instance)
        {
            //AddDebug("ThermalPlant UpdateUI");
            if (!Main.gameLoaded)
                return;

            if (ConfigToEdit.showTempFahrenhiet.Value)
            {
                __instance.temperatureText.text = (int)Util.CelciusToFahrenhiet(__instance.temperature) + "°F";
            }
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(ThermalPlant __instance)
        {
            //AddDebug("ThermalPlant Start");
            if (Main.gameLoaded)
                CoroutineHost.StartCoroutine(FixTempDisplay(__instance.gameObject));
        }

        [HarmonyPrefix, HarmonyPatch("OnHandHover")]
        public static bool OnHandHoverPrefix(ThermalPlant __instance, GUIHand hand)
        {
            if (!__instance.constructable.constructed)
                return false;

            HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<int, int>("ThermalPlantStatus", Mathf.RoundToInt(__instance.powerSource.GetPower()), Mathf.RoundToInt(__instance.powerSource.GetMaxPower())), false);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
            //HandReticle.main.SetIcon(HandReticle.IconType.Interact);
            return false;
        }

        public static IEnumerator FixTempDisplay(GameObject go)
        {// fix disappearing temp display
            yield return new WaitForSeconds(2);
            Transform model = go.transform.Find("model");
            model.gameObject.SetActive(false);
            model.gameObject.SetActive(true);
        }

    }
}
