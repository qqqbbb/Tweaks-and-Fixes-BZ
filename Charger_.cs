using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Charger))]
    internal class Charger_
    {
        public static HashSet<TechType> notRechargableBatteries = new HashSet<TechType>();

        public static void TrimUnpoweredNotifyStrings(Charger charger)
        {
            string s = Language.main.Get("ChargerInsufficientPower");
            s = RemoveAfterNewLine(s);
            //AddDebug(s);
            for (int i = 0; i <= Charger.chargeAttemptInterval; i++)
                charger.unpoweredNotifyStrings[i] = s;
        }

        public static string RemoveAfterNewLine(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            int newLineIndex = input.IndexOfAny(new char[] { '\n', '\r' });

            if (newLineIndex == -1)
                return input;

            return input.Substring(0, newLineIndex);
        }


        [HarmonyPrefix, HarmonyPatch("ToggleUIPowered")]
        public static void ToggleUIPoweredPreix(Charger __instance, ref bool powered)
        {
            //AddDebug($"Charger ToggleUIPowered {powered} {__instance.powerConsumer.IsPowered()}");
            powered = __instance.powerConsumer.IsPowered();
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(Charger __instance)
        {
            //AddDebug(__instance.name + " Charger Start");
            __instance.chargeSpeed *= ConfigToEdit.batteryChargeSpeedMult.Value;

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
            if (ConfigToEdit.hints.Value == false)
            {
                TrimUnpoweredNotifyStrings(__instance);
                if (__instance is PowerCellCharger)
                {
                    Transform unpoweredUI = __instance.transform.Find("UI/Unpowered/Text");
                    TextMeshProUGUI textMeshProUGUI = unpoweredUI.GetComponent<TextMeshProUGUI>();
                    textMeshProUGUI.fontSizeMax = 110;
                }
            }
        }


    }
}
