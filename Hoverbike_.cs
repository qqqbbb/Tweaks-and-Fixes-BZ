using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(Hoverbike))]
    class Hoverbike_
    {
        static float defaultEnginePowerConsumption;
        static float forwardAccel;
        static bool boosting;
        public static Color lightColor;

        [HarmonyPrefix, HarmonyPatch("Start")]
        static void StartPrefix(Hoverbike __instance)
        {
            Light light = null;
            if (ConfigToEdit.hoverbikeLightIntensityMult.Value < 1)
            {
                light = __instance.toggleLights.lightsParent.GetComponentInChildren<Light>(true);
                light.intensity *= ConfigToEdit.hoverbikeLightIntensityMult.Value;
            }
            if (lightColor != default)
            {
                if (light == null)
                    light = __instance.toggleLights.lightsParent.GetComponentInChildren<Light>(true);

                light.color = lightColor;
            }
        }
        [HarmonyPostfix, HarmonyPatch("Start")]
        static void StartPostfix(Hoverbike __instance)
        {
            //defaultEnginePowerConsumption = __instance.enginePowerConsumption;
            //AddDebug("Hoverbike Start enginePowerConsumption " + __instance.enginePowerConsumption);
            if (ConfigToEdit.hoverbikeMoveOnWater.Value)
            {
                __instance.waterDampening = 1;
                __instance.forceLandMode = true;
            }
            else
                __instance.forceLandMode = false; // this is saved

            if (ConfigToEdit.hoverbikeBoostWithoutCooldown.Value)
                __instance.boostCooldown = 0;

            forwardAccel = __instance.forwardAccel;
        }

        [HarmonyPrefix, HarmonyPatch("PhysicsMove")]
        static void PhysicsMovePrefix(Hoverbike __instance)
        {
            if (__instance.dockedPad || !__instance.GetPilotingCraft())
                return;

            Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;

            if (moveDirection == Vector3.zero)
                return;

            __instance.forwardAccel = forwardAccel * ConfigMenu.hoverbikeSpeedMult.Value;
            if (ConfigToEdit.fixHoverbikeAnalogMovement.Value)
            {
                float x = Mathf.Abs(moveDirection.x);
                float z = Mathf.Abs(moveDirection.z);
                if (x > z)
                    __instance.forwardAccel *= x;
                else
                    __instance.forwardAccel *= z;
            }
            //AddDebug("forwardAccel " + __instance.forwardAccel);
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
            {
                __instance.boostFuel = __instance.forwardBoostForce * Time.deltaTime;
                if (!boosting && GameInput.GetButtonHeld(GameInput.Button.Sprint))
                {
                    boosting = true;
                    //AddDebug("start boosting");
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch("HoverEngines")]
        static void HoverEnginesPostfix(Hoverbike __instance)
        {
            if (ConfigToEdit.hoverbikeBoostWithoutCooldown.Value)
            {
                Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : default;

                if (boosting)
                {
                    if (moveDirection == default)
                    {
                        boosting = false;
                        __instance.ResetBoostCD();
                        //AddDebug("stop boosting");
                    }
                    else
                        __instance.rb.AddForce(__instance.transform.forward * __instance.boostFuel);
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch("UpdateEnergy")]
        static void UpdateEnergyPrefix(Hoverbike __instance)
        {
            if (!GameModeManager.GetOption<bool>(GameOption.TechnologyRequiresPower))
            {
                __instance.enginePowerConsumption = 0;
                return;
            }
            defaultEnginePowerConsumption = __instance.enginePowerConsumption;
            if (ConfigToEdit.hoverbikeBoostWithoutCooldown.Value && __instance.GetPilotingCraft())
            {
                if (__instance.appliedThrottle && boosting)
                    __instance.enginePowerConsumption = defaultEnginePowerConsumption * 2;
            }
            __instance.enginePowerConsumption *= ConfigMenu.vehicleEnergyConsMult.Value;
            //AddDebug("enginePowerConsumption " + __instance.enginePowerConsumption);
        }
        [HarmonyPostfix, HarmonyPatch("UpdateEnergy")]
        static void UpdateEnergyPostfix(Hoverbike __instance)
        {
            __instance.enginePowerConsumption = defaultEnginePowerConsumption;
            //AddDebug("enginePowerConsumption Postfix " + __instance.enginePowerConsumption);
        }

    }


}
