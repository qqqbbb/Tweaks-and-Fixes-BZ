
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Weather_Patch
    {
        [HarmonyPatch(typeof(WeatherSetTuning), "GetEventDuration")]
        class WeatherSetTuning_GetEventDuration_Patch
        {
            static void Postfix(WeatherSetTuning __instance, ref float __result)
            {
                __result /= DayNightCycle.main._dayNightSpeed;
                //AddDebug(__instance.weatherSet.name + " GetEventDuration " + __result);
            }
        }

        //[HarmonyPatch(typeof(WeatherManager), "ExtendWeatherTimeline")]
        class WeatherManager_ExtendWeatherTimeline_Patch
        {
            static bool Prefix(WeatherManager __instance)
            { // prevent debug logging 
                 //AddDebug(" ExtendWeatherTimeline " );
                if (__instance.currentWeatherProfile != null)
                {
                    float timeSeconds = __instance.minPredictedWeatherTime - __instance.weatherTimeline.GetCurrentDuration();
                    __instance.weatherTimeline.Populate(__instance.currentWeatherProfile, __instance.currentWeatherEvent == null || !(__instance.currentWeatherEvent.weatherSet != null) ? WeatherDangerLevel.None : __instance.currentWeatherEvent.weatherSet.dangerLevel, timeSeconds);
                }
                return false;
            }
        }


    }
}
