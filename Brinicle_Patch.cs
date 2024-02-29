using System;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{   //  47 -14 -28
    [HarmonyPatch(typeof(Brinicle))]
    public class Brinicle_Patch 
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetState", new Type[] { typeof(Brinicle.State), typeof(float) })]
        public static bool SetStatePrefix(Brinicle __instance, Brinicle.State newState, float changedTime)
        {
            __instance.timeStateCanged = changedTime;
            __instance.state = newState;
            __instance.model.gameObject.SetActive((uint)__instance.state > 0U);
            if (__instance.fxController != null)
            {
                if (__instance.state == Brinicle.State.Enabled || __instance.state == Brinicle.State.Grow)
                    __instance.fxController.Play(0);
                else
                    __instance.fxController.Stop(0);
            }
            switch (__instance.state)
            {
                case Brinicle.State.Disabled:
                    __instance.timeNextState = __instance.timeStateCanged + Mathf.Lerp(__instance.minSpawnInterval, __instance.maxSpawnInterval, UnityEngine.Random.value);
                    __instance.UnfreezeAll();
                    break;
                case Brinicle.State.Grow:
                    if (Main.config.brinicleDaysToGrow == 0)
                        __instance.timeNextState = __instance.timeStateCanged + Mathf.Lerp(__instance.minGrowTime, __instance.maxGrowTime, UnityEngine.Random.value);
                    else if(Main.config.brinicleDaysToGrow > 0)
                        __instance.timeNextState = __instance.timeStateCanged + Main.config.brinicleDaysToGrow * 1200f / DayNightCycle.main._dayNightSpeed;
                    //__instance.timeNextState = __instance.timeStateCanged + Mathf.Lerp(__instance.minGrowTime, __instance.maxGrowTime, Random.value);
                    __instance.currentSize = __instance.growthSpeed.Evaluate(Mathf.InverseLerp(__instance.timeStateCanged, __instance.timeStateCanged + __instance.timeNextState, Time.time));
                    __instance.fullScale = Vector3.Lerp(__instance.minFullScale, __instance.maxFullScale, UnityEngine.Random.value);
                    __instance.model.localScale = Vector3.Lerp(__instance.zeroScale, __instance.fullScale, __instance.currentSize);
                    break;
                case Brinicle.State.Enabled:
                    __instance.timeNextState = __instance.timeStateCanged + Mathf.Lerp(__instance.minLifeTime, __instance.maxLifeTime, UnityEngine.Random.value);
                    __instance.currentSize = 1f;
                    __instance.model.localScale = __instance.fullScale;
                    break;
                case Brinicle.State.FadeOut:
                    __instance.timeNextState = __instance.timeStateCanged + __instance.fadeOutTime;
                    break;
            }
            return false;
        }
           
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(Brinicle __instance)
        {
            float currentTime = Time.time;
            switch (__instance.state)
            {
                case Brinicle.State.Disabled:
                    if (currentTime <= __instance.timeNextState)
                        break;
                    if (__instance.randomizeRotation)
                        __instance.RandomizeRotation();
                    __instance.liveMixin.health = __instance.liveMixin.data.maxHealth;
                    __instance.SetState(Brinicle.State.Grow);
                    break;
                case Brinicle.State.Grow:
                    __instance.UpdatePlayerDamage(currentTime);
                    float time2 = Mathf.InverseLerp(__instance.timeStateCanged, __instance.timeNextState, currentTime);
                    __instance.currentSize = __instance.growthSpeed.Evaluate(time2);
                    __instance.model.localScale = Vector3.Lerp(__instance.zeroScale, __instance.fullScale, __instance.currentSize);
                    if (time2 < 1f)
                        break;
                    __instance.SetState(Brinicle.State.Enabled);
                    break;
                case Brinicle.State.Enabled:
                    __instance.UpdatePlayerDamage(currentTime);
                    //if (currentTime <= __instance.timeNextState)
                    //    break;
                    //__instance.SetState(Brinicle.State.FadeOut);
                    break;
                case Brinicle.State.FadeOut:
                    __instance.UpdatePlayerDamage(currentTime);
                    float fadeAmount = Mathf.InverseLerp(__instance.timeStateCanged, __instance.timeNextState, currentTime);
                    foreach (Renderer fadeRenderer in __instance.fadeRenderers)
                        fadeRenderer.fadeAmount = fadeAmount;
                    if (fadeAmount != 1f)
                        break;
                    if (__instance.breakAfterFadeOut && __instance.fxController != null)
                        __instance.fxController.Play(1);
                    __instance.SetState(Brinicle.State.Disabled);
                    break;
            }
            return false;
        }
     }

}