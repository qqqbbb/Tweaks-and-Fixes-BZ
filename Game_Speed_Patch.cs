﻿using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Game_Speed_Patch
    { 
        [HarmonyPatch(typeof(DayNightCycle), "Awake")]
        class DayNightCycle_Awake_Patch
        {
            static void Postfix(DayNightCycle __instance)
            {
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
            }
        }
        
        [HarmonyPatch(typeof(DayNightCycle), "Update")]
        class DayNightCycle_Update_Patch
        {
            static bool Prefix(DayNightCycle __instance)
            {
                if (__instance.debugFreeze)
                    return false;
                __instance.timePassedAsDouble += __instance.deltaTime;
                if (__instance.skipTimeMode && __instance.timePassed >= __instance.skipModeEndTime)
                {
                    __instance.skipTimeMode = false;
                    __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
                }
                __instance.UpdateAtmosphere();
                __instance.UpdateDayNightMessage();
                //AddDebug();
                return false;
            }
        }

        [HarmonyPatch(typeof(DayNightCycle), "Resume")]
        class DayNightCycle_Resume_Patch
        {
            static bool Prefix(DayNightCycle __instance)
            {
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
                return false;
            }
        }

        [HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_night")]
        class DayNightCycle_OnConsoleCommand_night_Patch
        {
            static bool Prefix(DayNightCycle __instance, NotificationCenter.Notification n)
            {
                int num = __instance.IsDay() ? 1 : 0;
                AddDebug("Night cheat activated");
                __instance.timePassedAsDouble += Main.dayLengthSeconds - __instance.timePassed % Main.dayLengthSeconds;
                __instance.skipTimeMode = false;
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
                __instance.UpdateAtmosphere();
                if (num == 0)
                    return false;
                __instance.dayNightCycleChangedEvent.Trigger(false);
                return false;
            }
        }

        [HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_day")]
        class DayNightCycle_OnConsoleCommand_day_Patch
        {
            static bool Prefix(DayNightCycle __instance, NotificationCenter.Notification n)
            {
                int num = __instance.IsDay() ? 1 : 0;
                AddDebug("Day cheat activated");
                __instance.timePassedAsDouble += Main.dayLengthSeconds - __instance.timePassed % Main.dayLengthSeconds + Main.dayLengthSeconds * .5f;
                __instance.skipTimeMode = false;
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
                __instance.UpdateAtmosphere();
                if (num != 0)
                    return false;
                __instance.dayNightCycleChangedEvent.Trigger(true);
                return false;
            }
        }

        [HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_daynight")]
        class DayNightCycle_OnConsoleCommand_daynight_Patch
        {
            static bool Prefix(DayNightCycle __instance, NotificationCenter.Notification n)
            {
                bool flag = __instance.IsDay();
                float num1;
                if (DevConsole.ParseFloat(n, 0, out num1))
                {
                    float num2 = Mathf.Clamp01(num1);
                    AddDebug("Setting day/night scalar to " + num2 + ".");
                    __instance.timePassedAsDouble += Main.dayLengthSeconds - __instance.timePassedAsDouble % Main.dayLengthSeconds + num2 * Main.dayLengthSeconds;
                }
                __instance.skipTimeMode = false;
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
                __instance.UpdateAtmosphere();
                bool parms = __instance.IsDay();
                if (parms == flag)
                    return false;
                __instance.dayNightCycleChangedEvent.Trigger(parms);
                return false;
            }
        }

        [HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_daynightspeed")]
        class DayNightCycle_OnConsoleCommand_daynightspeed_Patch
        {
            static bool Prefix(DayNightCycle __instance, NotificationCenter.Notification n)
            {
                float newSpeed;
                if (DevConsole.ParseFloat(n, 0, out newSpeed))
                {
                    float num2 = Mathf.Clamp(newSpeed, 0.0f, 100f);
                    AddDebug("Setting day/night speed to " + num2 + ".");
                    __instance._dayNightSpeed = num2;
                    ConfigMenu.timeFlowSpeed.Value = num2;
                    __instance.skipTimeMode = false;
                }
                else
                    AddDebug("Must specify value from 0 to 100.");
                return false;
            }
        }

        [HarmonyPatch(typeof(DayNightCycle), "SkipTime")]
        class DayNightCycle_OSkipTime_Patch
        {
            static bool Prefix(DayNightCycle __instance, float timeAmount, float skipDuration, ref bool __result)
            {
                if (__instance.skipTimeMode || timeAmount <= 0f || skipDuration <= 0f)
                {
                    __result = false;
                    return false;
                }
                __instance.skipTimeMode = true;
                __instance.skipModeEndTime = __instance.timePassed + timeAmount;
                __instance._dayNightSpeed = timeAmount / skipDuration;
                __result = true;

                return false;
            }
        }

        [HarmonyPatch(typeof(DayNightCycle), "StopSkipTimeMode")]
        class DayNightCycle_StopSkipTimeMode_Patch
        {
            static bool Prefix(DayNightCycle __instance)
            {
                __instance.skipTimeMode = false;
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;

                return false;
            }
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
