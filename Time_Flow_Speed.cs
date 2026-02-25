using HarmonyLib;
using Nautilus.Utility;
using Story;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(StoryGoalScheduler), "Schedule")]
    class StoryGoalScheduler_Schedule_patch
    {
        public static void Prefix(StoryGoalScheduler __instance, StoryGoal goal)
        {
            if (ConfigMenu.timeFlowSpeed.Value == 1)
                return;

            goal.delay *= ConfigMenu.timeFlowSpeed.Value;
            //AddDebug("StoryGoalScheduler Schedule " + goal.key + " delay " + goal.delay);
        }
    }

    [HarmonyPatch(typeof(DayNightCycle))]
    class DayNightCycle_Patch
    {
        static bool skipTimeMode;

        [HarmonyPostfix, HarmonyPatch("Awake")]
        static void AwakePostfix(DayNightCycle __instance)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPrefix, HarmonyPatch("Update")]
        static void UpdatePrefix(DayNightCycle __instance)
        {
            skipTimeMode = __instance.skipTimeMode;
        }
        [HarmonyPostfix, HarmonyPatch("Update")]
        static void UpdatePostfix(DayNightCycle __instance)
        {
            if (skipTimeMode && __instance.skipTimeMode == false)
            {
                skipTimeMode = false;
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
            }
        }
        [HarmonyPostfix, HarmonyPatch("Resume")]
        static void ResumePostfix(DayNightCycle __instance)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("OnConsoleCommand_night")]
        static void OnConsoleCommand_nightPostfix(DayNightCycle __instance, NotificationCenter.Notification n)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("OnConsoleCommand_day")]
        static void OnConsoleCommand_dayPostfix(DayNightCycle __instance, NotificationCenter.Notification n)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("OnConsoleCommand_daynight")]
        static void OnConsoleCommand_daynightPostfix(DayNightCycle __instance, NotificationCenter.Notification n)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("StopSkipTimeMode")]
        static void StopSkipTimeModePostfix(DayNightCycle __instance)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }

        [HarmonyPatch(typeof(WeatherSetTuning), "GetEventDuration")]
        class WeatherSetTuning_GetEventDuration_Patch
        {
            static void Postfix(WeatherSetTuning __instance, ref float __result)
            {
                __result /= DayNightCycle.main._dayNightSpeed;
                //AddDebug(__instance.weatherSet.name + " GetEventDuration " + __result);
            }
        }

    }
}
