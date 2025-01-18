using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static ErrorMessage;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(Hoverbike))]
    class Hoverbike_Patch
    {
        static float defaultEnginePowerConsumption;
        static float forwardAccel;

        [HarmonyPostfix, HarmonyPatch("Start")]
        static void StartPostfix(Hoverbike __instance)
        {
            defaultEnginePowerConsumption = __instance.enginePowerConsumption;
            if (ConfigToEdit.hoverbikeMoveOnWater.Value)
            {
                __instance.waterDampening = 1;
                __instance.forceLandMode = true;
            }
            else
                __instance.forceLandMode = false; // this is saved

            if (ConfigToEdit.hoverbikeBoostWithoutCooldown.Value)
                __instance.boostCooldown = 0;

            //AddDebug("Start forwardBoostForce " + forwardBoostForce);
            //AddDebug("Start .forwardAccel " + __instance.forwardAccel);
            forwardAccel = __instance.forwardAccel;
        }

        [HarmonyPrefix, HarmonyPatch("PhysicsMove")]
        static void PhysicsMovePrefix(Hoverbike __instance)
        {
            if (!ConfigToEdit.fixHoverbikeAnalogMovement.Value || __instance.dockedPad || !__instance.GetPilotingCraft() || __instance.energyMixin.IsDepleted())
                return;

            Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;

            if (moveDirection == Vector3.zero)
                return;

            float x = Mathf.Abs(moveDirection.x);
            float z = Mathf.Abs(moveDirection.z);
            __instance.forwardAccel = forwardAccel;
            if (x > z)
                __instance.forwardAccel = forwardAccel * x;
            else if (x < z)
                __instance.forwardAccel = forwardAccel * z;
            //AddDebug("forwardAccel " + __instance.forwardAccel);
        }


        [HarmonyPostfix, HarmonyPatch("HoverEngines")]
        static void HoverEnginesPostfix(Hoverbike __instance)
        {
            //AddDebug("appliedThrottle " + __instance.appliedThrottle);
            if (ConfigToEdit.hoverbikeBoostWithoutCooldown.Value && !GameInput.GetButtonHeld(GameInput.Button.Sprint))
                __instance.ResetBoostCD();
        }

        [HarmonyPrefix, HarmonyPatch("HoverEngines")]
        static void HoverEnginesPrefix(Hoverbike __instance)
        {
            if (ConfigToEdit.hoverbikeMoveOnWater.Value && __instance.transform.position.y < 1f)
            {
                __instance.rb.AddForce(Vector3.up * __instance.boyancy);
                __instance.wasOnGround = true;
            }
            if (ConfigToEdit.hoverbikeBoostWithoutCooldown.Value)
                __instance.boostFuel = __instance.forwardBoostForce * Time.deltaTime;
        }

        [HarmonyPrefix, HarmonyPatch("UpdateEnergy")]
        static void UpdateEnergyPrefix(Hoverbike __instance)
        {
            if (!GameModeManager.GetOption<bool>(GameOption.TechnologyRequiresPower))
            {
                __instance.enginePowerConsumption = 0;
                return;
            }
            if (ConfigToEdit.hoverbikeBoostWithoutCooldown.Value && __instance.GetPilotingCraft())
            {
                if (__instance.appliedThrottle && GameInput.GetButtonHeld(GameInput.Button.Sprint))
                    __instance.enginePowerConsumption = defaultEnginePowerConsumption * 2;
                else
                    __instance.enginePowerConsumption = defaultEnginePowerConsumption;
            }
            __instance.enginePowerConsumption *= ConfigMenu.vehicleEnergyConsMult.Value;
            //AddDebug("enginePowerConsumption " + __instance.enginePowerConsumption);
        }

    }


}
