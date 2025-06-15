using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class UGUI_meters
    {
        //[HarmonyPatch(typeof(uGUI_BodyHeatMeter), "LateUpdate")]
        class uGUI_BodyHeatMeter_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (!ConfigToEdit.alwaysShowHealthFoodNunbers.Value)
                    return null;

                var matcher = new CodeMatcher(instructions);
                matcher.MatchForward(true,    // Match the line "this.showNumbers = false;"
                   new CodeMatch(OpCodes.Ldarg_0),
                   new CodeMatch(OpCodes.Ldc_I4_0),
                   new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(uGUI_BodyHeatMeter), "showNumbers")));

                if (matcher.IsValid)
                {   // Insert "this.showNumbers = true;" after the matched line
                    matcher.Advance(1)
                           .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                           .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
                           .InsertAndAdvance(new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(uGUI_BodyHeatMeter), "showNumbers")));
                }
                return matcher.InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(uGUI_HealthBar), "LateUpdate")]
        class uGUI_HealthBar_LateUpdate_Patch
        {
            public static void Postfix(uGUI_HealthBar __instance)
            {
                if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value && __instance.icon.localRotation.y != 180f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        [HarmonyPatch(typeof(uGUI_FoodBar), "LateUpdate")]
        class uGUI_FoodBar_LateUpdate_Patch
        {
            public static void Postfix(uGUI_FoodBar __instance)
            {
                if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value && __instance.icon.localRotation.y != 180f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        [HarmonyPatch(typeof(uGUI_WaterBar), "LateUpdate")]
        class uGUI_WaterBar_LateUpdate_Patch
        {
            public static void Postfix(uGUI_WaterBar __instance)
            {
                if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value && __instance.icon.localRotation.y != 180f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        [HarmonyPatch(typeof(uGUI_BodyHeatMeter), "LateUpdate")]
        class uGUI_BodyHeatMeter_LateUpdatPrefixe_Patch
        {
            public static void Postfix(uGUI_BodyHeatMeter __instance)
            {
                if (!ConfigToEdit.alwaysShowHealthFoodNunbers.Value)
                    return;

                if (__instance.stateMaximize.normalizedTime > 0.5f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 0f, 0f);
                else if (__instance.icon.localRotation.y != 180f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

    }
}
