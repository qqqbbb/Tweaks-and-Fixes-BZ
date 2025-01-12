using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    //[HarmonyPatch(typeof(Flare))]
    class Flare_Patch
    {
        public static float originalIntensity = -1f;
        static float originalEnergy = -1f;
        public static float halfOrigIntensity;
        static float originalRange;
        static float lowEnergy;
        public static bool intensityChanged = false;
        //static bool throwing = false;

        public static void PlayerToolOnHolster(PlayerTool tool)
        {
            tool.usingPlayer = null;
            tool.isDrawn = false;
            if (tool.firstUseAnimationStarted)
                tool.OnFirstUseAnimationStop();
            if (tool.waitForAnimDrawEvent)
                tool.SetRenderersEnabled(true);
            tool.ResetIKAim();
        }

        public static void LightFlare(Flare flare)
        {
            //AddDebug("LightFlare ");
            flare.loopingSound.Play();
            if (flare.fxControl)
            {
                flare.fxIsPlaying = true;
                flare.fxControl.Play(0);
                flare.fxControl.Play(1);
            }
            flare.capRenderer.enabled = false;
            flare.light.enabled = true;
            flare.isLightFadinfIn = true;
            //flare.isThrowing = true;
            flare.hasBeenThrown = true; // this skips "remove cao" animation
            flare.flareActivateTime = DayNightCycle.main.timePassedAsFloat;
            flare.flareActiveState = true;
            //flare.throwDuration = .125f;
            flare._isInUse = false;
        }

        public static void KillFlareLight(Flare flare)
        {
            //AddDebug("KillFlareLight ");
            if (flare.fxIsPlaying)
            {
                flare.fxControl.StopAndDestroy(1, 2f);
                flare.fxIsPlaying = false;
            }
            flare.light.enabled = false;
            flare.isLightFadinfIn = false;
            flare.hasBeenThrown = true;
            flare.loopingSound.Stop();
            flare.pickupable.isPickupable = false;
        }

        public static void PlayerToolAwake(PlayerTool tool)
        {
            tool.energyMixin = tool.GetComponent<EnergyMixin>();
            tool.savedRightHandIKTarget = tool.rightHandIKTarget;
            tool.savedLeftHandIKTarget = tool.leftHandIKTarget;
            tool.savedIkAimRightArm = tool.ikAimRightArm;
            tool.savedIkAimLeftArm = tool.ikAimLeftArm;
            tool.savedUseLeftAimTargetOnPlayer = tool.useLeftAimTargetOnPlayer;
        }

        public static void PlayerToolOnDraw(Player p, PlayerTool tool)
        {
            tool.usingPlayer = p;
            if (tool.waitForAnimDrawEvent && !p.animatedToolTracker.IsToolAnimationActive(tool))
            {
                tool.SetRenderersEnabled(false);
                tool.SetHandIKTargetsEnabled(false);
            }
            else
                tool.SetHandIKTargetsEnabled(true);
            LargeWorldEntity component = tool.GetComponent<LargeWorldEntity>();
            if (component != null && LargeWorldStreamer.main != null && LargeWorldStreamer.main.IsReady())
                LargeWorldStreamer.main.cellManager.UnregisterEntity(component);
            tool.isDrawn = true;
            tool.firstUseAnimationStarted = false;
            if (tool.hasFirstUseAnimation && tool.pickupable)
            {
                TechType techType = tool.pickupable.GetTechType();
                bool flag = tool.ShouldPlayFirstUseAnimation();
                if (flag)
                    Player.main.AddUsedTool(techType);
                Player.main.playerAnimator.SetBool("using_tool_first", flag);
                tool.firstUseAnimationStarted = flag;
            }
            if (tool.firstUseAnimationStarted && tool.firstUseSound)
            {
                tool.firstUseSound.Play();
            }
            else
            {
                FMODAsset asset = !Player.main.IsUnderwater() || tool.drawSoundUnderwater == null ? tool.drawSound : tool.drawSoundUnderwater;
                if (!asset)
                    return;
                Utils.PlayFMODAsset(asset, tool.transform);
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("Awake")]
        static bool AwakePrefix(Flare __instance)
        {
            if (__instance.energyLeft <= 0f && !__instance.GetComponentInParent<Player>())
            { // destroy only when not in inventpry
                //AddDebug("Destroy flare ");
                UnityEngine.Object.Destroy(__instance.gameObject);
                return false;
            }
            //__instance.energyLeft = 5;
            if (originalIntensity == -1f && __instance.flareActivateTime == 0f)
            {
                originalIntensity = __instance.light.intensity;
                halfOrigIntensity = originalIntensity * .5f;
                originalEnergy = __instance.energyLeft;
                //originalEnergy = 10;
                lowEnergy = originalEnergy * .1f;
                originalRange = __instance.light.range;
                __instance.light.intensity = 0f;
                //__instance.light.range = 0f;
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Tools_Patch.lightOrigIntensity[TechType.Flare] = originalIntensity;
                //Tools_Patch.lightIntensityStep[TechType.Flare] = originalIntensity * .05f;
                //AddDebug("Awake originalIntensity " + originalIntensity);
            }
            //if (Main.config.lightIntensity.ContainsKey(TechType.Flare))
            //{
            //    originalIntensity = Main.config.lightIntensity[TechType.Flare];
            //    halfOrigIntensity = originalIntensity * .5f;
            //}
            PlayerToolAwake(__instance as PlayerTool);
            __instance.originalIntensity = __instance.light.intensity;
            __instance.originalrange = __instance.light.range;
            __instance.light.intensity = 0f;
            __instance.capRenderer.enabled = __instance.flareActivateTime == 0;

            if (__instance.flareActivateTime > 0f)
            {
                if (__instance.fxControl && !__instance.fxIsPlaying)
                {
                    __instance.fxControl.Play(1);
                    __instance.fxIsPlaying = true;
                    __instance.light.enabled = true;
                }
            }
            __instance.fireSource.enabled = __instance.flareActiveState;
            WorldForces wf = __instance.GetComponent<WorldForces>();
            wf.underwaterGravity = .5f;
            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("SetFlareActiveState")]
        static bool SetFlareActiveStatePrefix(Flare __instance, bool newFlareActiveState)
        {
            //AddDebug("hasBeenThrown " + __instance.hasBeenThrown);
            //AddDebug("fxIsPlaying " + __instance.fxIsPlaying);
            if (__instance.flareActiveState == newFlareActiveState)
                return false;
            if (newFlareActiveState)
            {
                __instance.loopingSound.Play();
                if (__instance.fxControl)
                    __instance.fxControl.Play(0);
                __instance.capRenderer.enabled = false;
                __instance.light.enabled = true;
                __instance.isLightFadinfIn = true;
                __instance.hasBeenThrown = true;
                if (__instance.flareActivateTime == 0)
                    __instance.flareActivateTime = DayNightCycle.main.timePassedAsFloat;
            }
            __instance.flareActiveState = newFlareActiveState;
            __instance.fireSource.enabled = newFlareActiveState;
            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("OnDraw")]
        static bool OnDrawPrefix(Flare __instance, Player p)
        {
            //AddDebug("OnDraw originalRange " + originalRange);
            //AddDebug("OnDraw originalIntensity " + originalIntensity);
            intensityChanged = false;
            PlayerToolOnDraw(p, __instance as PlayerTool);
            if (__instance.flareActivateTime == 0)
                return false;

            __instance.throwDuration = .1f;
            __instance.isThrowing = false;
            __instance.energyLeft = originalEnergy - (DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime);
            if (__instance.energyLeft < 0)
                __instance.energyLeft = 0;

            if (__instance.energyLeft > 0 && !__instance.fxIsPlaying)
            {
                __instance.SetFlareActiveState(true);
                __instance.fxControl.Play(1);
                __instance.fxIsPlaying = true;
                p.isUnderwater.changedEvent.AddHandler(__instance, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(__instance.UpdateBubblesFx));
                __instance.UpdateBubblesFx(p.isUnderwater);
            }
            else
            {
                __instance.flareActiveState = false;
                KillFlareLight(__instance);
            }
            return false;
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("OnHolster")]
        static void OnHolsterPostfix(Flare __instance)
        {
            PlayerToolOnHolster(__instance);
            if (__instance.flareActivateTime > 0f)
                __instance.hasBeenThrown = true;

            if (!__instance.isThrowing)
            {
                __instance.loopingSound.Stop();
                if (__instance.fxIsPlaying)
                {
                    __instance.fxControl.StopAndDestroy(1, 0.0f);
                    __instance.fxIsPlaying = false;
                }
                __instance.SetFlareActiveState(false);
            }
            Player.main.isUnderwater.changedEvent.RemoveHandler(__instance, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(__instance.UpdateBubblesFx));
            //AddDebug("hasBeenThrown " + __instance.hasBeenThrown);
            //AddDebug("fxIsPlaying " + __instance.fxIsPlaying);
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("OnToolUseAnim")]
        static bool OnToolUseAnimPrefix(Flare __instance)
        {
            if (__instance.isThrowing)
                return false;

            //if (DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime > 1f)
            {
                //AddDebug("OnToolUseAnim " + __instance.throwDuration);
                __instance.isThrowing = true;
                if (__instance.flareActivateTime == 0)
                {
                    LightFlare(__instance);
                    __instance.throwDuration = .6f;
                }
                if (__instance.usingPlayer != null)
                    __instance.UpdateBubblesFx(__instance.usingPlayer.isUnderwater);

                __instance.Invoke("Throw", __instance.throwDuration);
            }
            //__instance.hasBeenThrown = true;
            //__instance.energyLeft = 0f;
            //__instance.isLightFadinfIn = false;
            //__instance.flareActiveState = true; // need this for Throw callback 
            //AddDebug("OnToolUseAnim flareActiveState " + __instance.flareActiveState);
            //KillFlareLight(__instance);
            //__instance.SetFlareActiveState(false);
            //Flare_OnDraw_Patch.Prefix(__instance, Player.main);
            //__instance.throwDuration = .1f;
            //__instance.sequence.Set(__instance.throwDuration, true, new SequenceCallback(__instance.Throw));
            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("Throw")]
        static bool ThrowPrefix(Flare __instance)
        {
            //AddDebug("Throw " + __instance.throwDuration);
            //Inventory.main.quickSlots.SelectImmediate(1);
            __instance._isInUse = false;
            __instance.pickupable.Drop(__instance.transform.position);
            //__instance.pickupable.isPickupable = false;
            //__instance.pickupable.enabled = false;
            __instance.transform.GetComponent<WorldForces>().enabled = true;
            __instance.throwSound.StartEvent();
            __instance.isThrowing = false;
            //__instance.throwDuration = DayNightCycle.main.timePassedAsFloat;
            return false;
        }

        static bool UpdateLight(Flare __instance)
        {
            //AddDebug("UpdateLight ");
            float burnTime = DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime;
            if (burnTime < 0.1f)
                return false;
            __instance.light.intensity = halfOrigIntensity + halfOrigIntensity * Mathf.PerlinNoise(Time.time * 6f, 0f);
            if (__instance.energyLeft < lowEnergy)
            {
                float f1 = __instance.energyLeft / lowEnergy;
                //AddDebug("lowEnergy " + f1.ToString("0.0"));
                __instance.light.intensity = Mathf.Lerp(0f, __instance.light.intensity, f1);
                __instance.light.range = Mathf.Lerp(0f, originalRange, f1);
            }
            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("Update")]
        static bool UpdatePrefix(Flare __instance)
        {
            //AddDebug("Flare Update energyLeft " + __instance.energyLeft);
            if (__instance.flareActiveState)
            {
                //AddDebug("UpdateLight " + __instance.energyLeft);
                //__instance.energyLeft = Mathf.Max(__instance.energyLeft - Time.deltaTime, 0f);

                __instance.energyLeft = originalEnergy - (DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime);
                if (__instance.energyLeft < 0)
                    __instance.energyLeft = 0;

                if (__instance.energyLeft > 0)
                    UpdateLight(__instance);
            }
            else
                __instance.light.intensity = 0f;

            if (__instance.fxIsPlaying && __instance.energyLeft < 3f)
            {
                __instance.fxControl.StopAndDestroy(1, 2f);
                __instance.fxControl.Play(2);
                __instance.fxIsPlaying = false;
                if (__instance.usingPlayer != null)
                    __instance.UpdateBubblesFx(__instance.usingPlayer.isUnderwater);
            }
            //if (__instance.energyLeft > 0)
            //    Main.Message("energyLeft " + (int)__instance.energyLeft);
            if (__instance.energyLeft <= 0)
                KillFlareLight(__instance);

            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("OnDrop")]
        public static bool OnDropPrefix(Flare __instance)
        {
            if (__instance.isThrowing)
            {
                __instance.GetComponent<Rigidbody>().AddForce(MainCamera.camera.transform.forward * 30f);
                __instance.GetComponent<Rigidbody>().AddTorque(MainCamera.camera.transform.forward);
                __instance.useRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                __instance.gameObject.EnsureComponent<SetRigidBodyModeOnSlowdown>().TriggerStart(__instance.useRigidbody, CollisionDetectionMode.ContinuousSpeculative, 25f);
                __instance.isThrowing = false;
                __instance.useRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                __instance.gameObject.EnsureComponent<SetRigidBodyModeOnSlowdown>().TriggerStart(__instance.useRigidbody, CollisionDetectionMode.ContinuousSpeculative, 25f);
            }
            //AddDebug("energyLeft " + __instance.energyLeft);
            if (__instance.flareActivateTime > 0f && __instance.energyLeft > 0f)
            {
                if (__instance.fxControl && !__instance.fxIsPlaying)
                {
                    __instance.fxControl.Play(1);
                    __instance.fxIsPlaying = true;
                    __instance.loopingSound.Play();
                    __instance.light.enabled = true;
                    __instance.hasBeenThrown = true;
                    __instance.flareActiveState = true;
                    __instance.UpdateBubblesFx(Player.main.isUnderwater);
                }
            }
            return false;
        }


    }
}