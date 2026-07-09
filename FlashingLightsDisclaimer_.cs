using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tweaks_Fixes

{
    internal class FlashingLightsDisclaimer_
    {
        [HarmonyPatch(typeof(FlashingLightsDisclaimer))]
        class FlashingLightsDisclaimer_CanShow_Patch
        {
            //[HarmonyPrefix, HarmonyPatch("TryToShow")]
            static bool TryToShowPrefix(FlashingLightsDisclaimer __instance)
            {
                bool noText = String.IsNullOrEmpty(ConfigToEdit.gameStartWarningText.Value);
                //Main.logger.LogDebug("FlashingLightsDisclaimer TryToShow noText " + noText);
                return !noText;
            }
            [HarmonyPostfix, HarmonyPatch("SetText")]
            static void SetTextPostfix(FlashingLightsDisclaimer __instance)
            {
                __instance.text.text = Language.main.Get(ConfigToEdit.gameStartWarningText.Value);
                //Main.logger.LogDebug("FlashingLightsDisclaimer SetText " + ConfigToEdit.gameStartWarningText.Value);
            }
        }
    }
}
