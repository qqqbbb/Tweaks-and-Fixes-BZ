using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using static ErrorMessage;
using static GameInput;

namespace Tweaks_Fixes
{
    internal class Input_
    {
        [HarmonyPatch(typeof(GameInput))]
        internal class GameInput_
        {
            [HarmonyPatch("UpdateMoveDirection"), HarmonyPrefix]
            static bool UpdateMovePrefix()
            {
                float z = 0f;
                z += GetAnalogValueForButton(Button.MoveForward);
                z -= GetAnalogValueForButton(Button.MoveBackward);
                float x = 0f;
                x -= GetAnalogValueForButton(Button.MoveLeft);
                x += GetAnalogValueForButton(Button.MoveRight);
                float y = 0f;
                y += GetAnalogValueForButton(Button.MoveUp);
                y -= GetAnalogValueForButton(Button.MoveDown);
                //AddDebug($"UpdateMoveDirection {x} {y} {z}");
                if (autoMove && z != 0)
                {
                    autoMove = false;
                }
                if (autoMove)
                    moveDirection.Set(x, y, 1f);
                else
                    moveDirection.Set(x, y, z);

                if (!IsPrimaryDeviceGamepad())
                    return false;

                if (autoMove)
                {
                    isRunningMoveThreshold = false;
                    return false;
                }
                isRunningMoveThreshold = moveDirection.sqrMagnitude > 0.8f;
                if (!isRunningMoveThreshold)
                {
                    moveDirection /= 0.9f;
                }
                return false;
            }
            //[HarmonyPatch("UpdateMove"), HarmonyTranspiler]
            static IEnumerable<CodeInstruction> UpdateMoveTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeMatcher = new CodeMatcher(instructions);

                // Try to find stsfld for autoMove regardless of what's being stored
                codeMatcher.MatchForward(false,
                    new CodeMatch(OpCodes.Stsfld, AccessTools.Field(typeof(GameInput), "autoMove"))
                )
                .ThrowIfInvalid("Could not find autoMove field store in UpdateMove");
                // Go back one instruction to find what's being stored
                codeMatcher.Advance(-1);
                // Replace the loading instruction (ldc.i4.0 or whatever) with your delegate
                codeMatcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<bool>>(GetAutoMove));

                return codeMatcher.InstructionEnumeration();
            }

            static bool GetAutoMove()
            {
                //AddDebug("GetAutoMove");
                return true;
            }
        }

    }
}
