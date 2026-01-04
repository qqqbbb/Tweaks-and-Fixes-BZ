using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Battery_
    {

        static Dictionary<string, float> defaultBatteryCharge = new Dictionary<string, float>();

        [HarmonyPatch(typeof(Battery), "OnAfterDeserialize")]
        class Battery_OnAfterDeserialize_Patch
        {
            static void Postfix(Battery __instance)
            {
                if (ConfigMenu.batteryChargeMult.Value == 1f || __instance.name.IsNullOrWhiteSpace())
                    return;

                //AddDebug(__instance.name + " Battery OnAfterDeserialize " + __instance._capacity);
                if (!defaultBatteryCharge.ContainsKey(__instance.name))
                {
                    defaultBatteryCharge[__instance.name] = __instance._capacity;
                }
                if (defaultBatteryCharge.ContainsKey(__instance.name))
                {
                    __instance._capacity = defaultBatteryCharge[__instance.name] * ConfigMenu.batteryChargeMult.Value;
                    if (__instance.charge > __instance._capacity)
                        __instance.charge = __instance._capacity;
                }
            }
        }


    }
}
