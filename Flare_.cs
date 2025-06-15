using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Flare))]
    class Flare_
    {
        public static void LightFlare(Flare flare)
        {
            //AddDebug($"LightFlare flareActiveState {flare.flareActiveState}");
            //if (flare.flareActiveState)
            //    return;

            flare.loopingSound.Play();
            flare.capRenderer.enabled = false;
            flare.light.enabled = true;
            flare.isLightFadinfIn = true;
            flare.hasBeenThrown = true; // removing cap animation will not play when throwing
            flare.fireSource.enabled = true;
            flare.flareActiveState = true;
            //if (flare.fxControl && !flare.fxIsPlaying)
            if (flare.fxControl)
            {
                flare.fxControl.Play(1);
                flare.fxIsPlaying = true;
                if (flare.flareActivateTime == 0)
                    flare.fxControl.Play(0);
            }
            if (flare.flareActivateTime == 0)
                flare.flareActivateTime = DayNightCycle.main.timePassedAsFloat;

            Player.main.isUnderwater.changedEvent.AddHandler(flare, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(flare.UpdateBubblesFx));
            flare.UpdateBubblesFx(Player.main.isUnderwater);
        }

        public static float GetFlickerInterval()
        {
            return Flare.flickerInterval * DayNightCycle.main._dayNightSpeed;
        }

        [HarmonyPatch("Awake"), HarmonyPostfix]
        static void AwakePostfix(Flare __instance)
        {
            if (ConfigToEdit.flareTweaks.Value)
            {
                __instance.throwDuration = .4f;
            }
        }

        [HarmonyPatch("UpdateLight")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions)
         .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, Flare.flickerInterval))
         .ThrowIfInvalid("Could not find Ldc_R4 flickerInterval in Flare.UpdateLight")
         .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<float>>(GetFlickerInterval))
         .InstructionEnumeration();
            return codeMatcher;
        }

        [HarmonyPatch("UpdateLight"), HarmonyPostfix]
        static void UpdateLightPostfix(Flare __instance)
        {
            //if (Input.GetKeyDown(KeyCode.LeftShift))
            //    AddDebug($"energy {__instance.energyLeft.ToString("0.0")} intensity {__instance.light.intensity.ToString("0.0")} range {__instance.light.range.ToString("0.0")}");

            if (ConfigToEdit.flareFlicker.Value)
                return;

            __instance.light.intensity = __instance.originalIntensity;
            __instance.light.range = __instance.originalrange;
        }

        [HarmonyPatch("OnDraw"), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnDrawTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (ConfigToEdit.flareTweaks.Value == false)
                return codes;
            // Find the index right after the base.OnDraw call
            int returnIndex = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call &&
                    codes[i].operand.ToString().Contains("OnDraw(Player)"))
                {
                    returnIndex = i + 1;
                    break;
                }
            }
            if (returnIndex != -1)
            {
                // Insert return at the found position
                codes.Insert(returnIndex, new CodeInstruction(OpCodes.Ret));
                // Remove all remaining instructions after our return
                if (codes.Count > returnIndex + 1)
                    codes.RemoveRange(returnIndex + 1, codes.Count - (returnIndex + 1));
            }
            //Util.PrintOpcodes(codes, "OnDrawTranspiler");
            return codes;
        }

        [HarmonyPatch("Update"), HarmonyPostfix]
        static void UpdatePostfix(Flare __instance)
        {
            if (ConfigToEdit.flareTweaks.Value)
            {
                if (IsFlareLit(__instance) == false && GameInput.GetButtonDown(GameInput.Button.AltTool))
                {
                    __instance.throwDuration = 0f;
                    LightFlare(__instance);
                }
            }
        }

        private static bool IsFlareLit(Flare flare)
        {
            return flare.flareActivateTime > 0;
        }

        [HarmonyPatch("OnDraw"), HarmonyPostfix]
        static void OnDrawPostfix(Flare __instance)
        {
            if (ConfigToEdit.flareTweaks.Value && IsFlareLit(__instance))
                LightFlare(__instance);
        }

        //[HarmonyPatch("SetFlareActiveState"), HarmonyPostfix]
        static void SetFlareActiveStatePostfix(Flare __instance, bool newFlareActiveState)
        {
            //AddDebug($"SetFlareActiveState {newFlareActiveState}");
        }

        [HarmonyPatch("OnDrop"), HarmonyPrefix]
        static bool OnDropPrefix(Flare __instance)
        {
            //AddDebug("OnDrop");
            if (ConfigToEdit.flareTweaks.Value == false)
                return true;

            __instance.useRigidbody.AddTorque(__instance.transform.right * 1f);
            if (__instance.isThrowing)
            {
                __instance.useRigidbody.AddForce(MainCamera.camera.transform.forward * 60f);
                __instance.useRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                __instance.gameObject.EnsureComponent<SetRigidBodyModeOnSlowdown>().TriggerStart(__instance.useRigidbody, CollisionDetectionMode.ContinuousSpeculative, 25f);
            }
            if (IsFlareLit(__instance))
                LightFlare(__instance);

            __instance.isThrowing = false;
            return false;
        }

        [HarmonyPatch("OnToolUseAnim"), HarmonyPrefix]
        static bool OnToolUseAnimPrefix(Flare __instance)
        {
            //AddDebug("OnToolUseAnim");
            if (ConfigToEdit.flareTweaks.Value == false)
                return true;

            if (__instance.isThrowing)
                return false;

            if (IsFlareLit(__instance) == false)
                LightFlare(__instance);

            __instance.sequence.Set(__instance.throwDuration, true, new SequenceCallback(__instance.Throw));
            __instance.isThrowing = true;
            return false;
        }

    }

    [HarmonyPatch(typeof(Pickupable), "Pickup")]
    class Pickupable_Pickup_Postfix_Patch
    {
        static void Postfix(Pickupable __instance)
        {
            //AddDebug("Pickup Postfix");
            if (ConfigToEdit.flareTweaks.Value)
            {
                Flare flare = __instance.GetComponent<Flare>();
                if (flare)
                    flare.SetFlareActiveState(false);
            }
        }
    }


}