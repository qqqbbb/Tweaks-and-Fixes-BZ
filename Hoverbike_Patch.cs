using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Hoverbike_Patch
    {
        public static bool boosting = false;

        [HarmonyPatch(typeof(Hoverbike))]
        class HoverboardMotor_HoverEngines_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("HoverEngines")]
            static bool HoverEnginesPrefix(Hoverbike __instance)
            { // increase hover heigt above water, can jump and boost on water
                if (!ConfigMenu.hoverbikeMoveTweaks.Value)
                    return true;
                //AddDebug("forwardAccel " + __instance.forwardAccel);
                //AddDebug("forwardBoostForce " + __instance.forwardBoostForce);
                if (__instance.dockedPad)
                    return false;

                //if (!__instance.forceLandMode && !__instance.debugIgnoreWater && __instance.transform.position.y < __instance.waterLevelOffset)
                if (!__instance.forceLandMode && !__instance.debugIgnoreWater && __instance.transform.position.y < __instance.waterLevelOffset + 1f)
                    __instance.rb.AddForce(Vector3.up * __instance.boyancy);
                __instance.overWater = false;
                if (!__instance.forceLandMode && !__instance.debugIgnoreWater && __instance.transform.position.y < __instance.waterLevelOffset + 3f)
                    __instance.overWater = true;
                bool jumping = false;
                bool boosting = false;
                if (__instance.isPiloting)
                    //if (__instance.isPiloting && !__instance.overWater)
                {
                    jumping = GameInput.GetButtonHeld(GameInput.Button.Jump);
                    //if (jumping && __instance.jumpReset && (__instance.wasOnGround && __instance.jumpEnabled))
                    if (jumping && __instance.jumpReset && __instance.jumpEnabled && (__instance.wasOnGround || __instance.overWater))
                    {
                        //AddDebug("jump " + jumping);
                        //AddDebug("enginePowerConsumption " + __instance.enginePowerConsumption);
                        //AddDebug("enginePowerConsumption per frame" + Time.deltaTime * __instance.enginePowerConsumption);
                        __instance.energyMixin.ConsumeEnergy(__instance.enginePowerConsumption * .5f);
                        __instance.jumpReset = false;
                        __instance.wasOnGround = false;
                        __instance.jumpFuel = __instance.verticalBoostForce;
                        if (MiscSettings.flashes)
                            __instance.jumpFxControl.Play();

                        __instance.sfx_jump.Play();
                        Player.main.playerAnimator.SetTrigger("hovercraft_button_1");
                        __instance.animator.SetBool("hovercraft_jump", true);
                        Player.main.playerAnimator.SetBool("hovercraft_jump", true);
                        if (__instance.jumpCooldown > 0f)
                            __instance.Invoke("ResetJumpCD", __instance.jumpCooldown);
                    }
                    boosting = GameInput.GetButtonHeld(GameInput.Button.Sprint);
                    if (boosting && __instance.boostReset && (__instance.wasOnGround || __instance.overWater))
                    {
                        Hoverbike_Patch.boosting = true;
                        //__instance.enginePowerConsumption *= 2f;
                        //AddDebug("boost " + boosting);
                        __instance.boostReset = false;
                        __instance.boostFuel = __instance.forwardBoostForce;
                        if (MiscSettings.flashes)
                            __instance.boostFxControl.Play();

                        __instance.sfx_boost.Play();
                        __instance.SetBoostButtonState(false);
                        Player.main.playerAnimator.SetTrigger("hovercraft_button_3");
                        //if (__instance.boostCooldown > 0.0)
                        //    __instance.Invoke("ResetBoostCD", __instance.boostCooldown);
                    }
                    if (!boosting)
                    {
                        Hoverbike_Patch.boosting = false;
                        //AddDebug("stop boost " );
                        __instance.ResetBoostCD();
                    }
                    if (!boosting && !jumping)
                        __instance.boostFxControl.Stop();
                }
                if (boosting)
                {
                    __instance.rb.AddForce(__instance.transform.forward * Time.deltaTime * __instance.boostFuel);
                    //__instance.boostFuel = Mathf.MoveTowards(__instance.boostFuel, 0f, Time.deltaTime * __instance.boostDecay);
                }
                if (jumping)
                {
                    foreach (Transform hoverPoint in __instance.hoverPoints)
                        __instance.rb.AddForceAtPosition(Vector3.up * __instance.jumpFuel, hoverPoint.position);
                    __instance.jumpFuel = Mathf.MoveTowards(__instance.jumpFuel, 0.0f, Time.deltaTime * __instance.jumpDecay);
                }
                bool flag3 = true;
                RaycastHit[] raycastHitArray = new RaycastHit[__instance.hoverPoints.childCount];
                int layerMask = 1073741825;
                int index1 = 0;
                float t = __instance.jumpFuel / __instance.verticalBoostForce;
                float maxDistance = jumping ? Mathf.Lerp(__instance.hoverDist, __instance.hoverDist / 2f, t) : (__instance.isPiloting ? __instance.hoverDist : __instance.hoverDist / __instance.emptyVehicleHeight);
                foreach (Transform hoverPoint in __instance.hoverPoints)
                {
                    if (Physics.Raycast(hoverPoint.position, -Vector3.up, out raycastHitArray[index1], maxDistance, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        ++index1;
                    }
                    else
                    {
                        flag3 = false;
                        break;
                    }
                }
                if (flag3 && !__instance.playerTool.isEquipped)
                {
                    float num1 = __instance.energyMixin.IsDepleted() ? 0.0f : __instance.hoverForce;
                    float num2;
                    if (__instance.fictionalPowerState == Hoverbike.PowerDownStates.PowerDown)
                    {
                        __instance.emptyHoverPower = Mathf.MoveTowards(__instance.emptyHoverPower, 0.0f, Time.deltaTime * 1500f);
                        num2 = __instance.emptyHoverPower;
                        __instance.animator.SetBool("powered_down", true);
                        __instance.powerDownCollider.SetActive(true);
                        if (__instance.sfx_engineIdle.playing)
                            __instance.sfx_engineIdle.Stop();
                    }
                    else
                    {
                        if (__instance.fictionalPowerState == Hoverbike.PowerDownStates.PowerUp)
                        {
                            __instance.animator.SetBool("powered_down", false);
                            __instance.fictionalPowerState = Hoverbike.PowerDownStates.Idle;
                            __instance.sfx_engineIdle.Play();
                        }
                        if (__instance.fictionalPowerState == Hoverbike.PowerDownStates.Idle)
                        {
                            __instance.emptyHoverPower = Mathf.MoveTowards(__instance.emptyHoverPower, __instance.hoverForce, Time.deltaTime * 2500f);
                            num1 = __instance.emptyHoverPower;
                        }
                        num2 = num1 + Mathf.Sin(Time.time) * __instance.hoverEngineMicroVariance;
                    }
                    int index2 = 0;
                    bool flag4 = false;
                    foreach (RaycastHit raycastHit in raycastHitArray)
                    {
                        if (raycastHit.distance < __instance.impactCompensatorHeight)
                            flag4 = true;
                        __instance.rb.AddForceAtPosition(Vector3.up * Mathf.Max(num2 / __instance.constantForceDampening, num2 * Mathf.Clamp01((1f - raycastHit.distance / maxDistance))), __instance.hoverPoints.GetChild(index2).position);
                        ++index2;
                    }
                    if (__instance.isPiloting & flag4)
                        __instance.rb.AddForce(Vector3.up * __instance.hoverForce * 4f);
                    if (__instance.jumpCooldown <= 0.0)
                        __instance.jumpReset = true;
                    __instance.animator.SetBool("hovercraft_jump", false);
                    Player.main.playerAnimator.SetBool("hovercraft_jump", false);
                    if (!__instance.wasOnGround)
                        __instance.sfx_land.Play();
                    __instance.wasOnGround = true;
                }
                if (!__instance.energyMixin.IsDepleted() && __instance.fictionalPowerState != Hoverbike.PowerDownStates.PowerDown && !__instance.debugDisableVerticalStabilizer)
                {
                    Vector3 rhs = Vector3.up;
                    RaycastHit hitInfo;
                    if (Physics.Raycast(__instance.transform.position, -Vector3.up, out hitInfo, 15f, layerMask, QueryTriggerInteraction.Ignore))
                        rhs = hitInfo.normal;
                    float num = Vector3.Dot(__instance.transform.up, rhs);
                    Vector3 vector3 = Vector3.Cross(__instance.transform.up, rhs);
                    if (!jumping && (flag3 && num < __instance.stability || !flag3))
                        __instance.rb.AddTorque(vector3 * __instance.stabilitySpeed * __instance.stabilitySpeed);
                    if (1.0 - Vector3.Dot(__instance.transform.up, Vector3.up) > __instance.sfxCreakThreshold)
                    {
                        if (__instance.creakReset)
                            __instance.sfx_creak.Play();
                        __instance.creakReset = false;
                        if (!__instance.IsInvoking("CreakReset"))
                            __instance.Invoke("CreakReset", UnityEngine.Random.Range(0.3f, 0.5f));
                    }
                }
                //__instance.sfx_wind.SetParameterValue("velocity", Mathf.Clamp01(__instance.rb.velocity.magnitude / __instance.sfxWindThreshold));
                __instance.rb.AddForce(new Vector3(0.0f, -__instance.gravity * Time.deltaTime, 0.0f), ForceMode.VelocityChange);
                __instance.animator.SetBool("on_land", flag3);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("UpdateEnergy")]
            static bool UpdateEnergyPrefix(Hoverbike __instance)
            {
                //AddDebug("maxEnergy " + __instance.energyMixin.maxEnergy);
                //AddDebug("capacity " + __instance.energyMixin.capacity);
                //AddDebug("charge " + __instance.energyMixin.charge);
                if (!__instance.appliedThrottle)
                    return false;

                if (boosting)
                    __instance.energyMixin.ConsumeEnergy(Time.deltaTime * __instance.enginePowerConsumption * 2f);
                else
                    __instance.energyMixin.ConsumeEnergy(Time.deltaTime * __instance.enginePowerConsumption);
                return false;
            }

            [HarmonyPrefix] // fixed
            [HarmonyPatch("PhysicsMove")]
            static bool PhysicsMovePrefix(Hoverbike __instance)
            {// move on water, halve strafe and backward speed
                __instance.isPiloting = __instance.GetPilotingCraft();
                if (__instance.dockedPad || !__instance.isPiloting)
                    return false;

                __instance.moveDirection = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                Vector3 vector3 = new Vector3(__instance.moveDirection.x, 0f, __instance.moveDirection.z);
                Vector2 overflowInput = __instance.overflowInput;
                if (__instance.energyMixin.IsDepleted())
                    __instance.moveDirection = Vector3.zero;
                __instance.appliedThrottle = __instance.moveDirection != Vector3.zero;
                float horizontalDampening = __instance.horizontalDampening;
                if (__instance.overWater && !ConfigMenu.hoverbikeMoveTweaks.Value)
                    horizontalDampening = __instance.horizontalDampening / __instance.waterDampening;

                __instance.rb.AddTorque(__instance.transform.right * -overflowInput.x * __instance.sidewaysTorque * __instance.verticalDampening, ForceMode.VelocityChange);
                __instance.rb.AddTorque(__instance.transform.up * overflowInput.y * __instance.sidewaysTorque * horizontalDampening, ForceMode.VelocityChange);
                //Vector3 velocity = __instance.rb.velocity;
                Vector3 moveDirection = __instance.moveDirection;
                //double num2 = Mathf.Min(1f, moveDirection.magnitude);
                moveDirection.y = 0f;
                moveDirection.Normalize();
                if (ConfigMenu.hoverbikeMoveTweaks.Value)
                { 
                    moveDirection.x *= .5f;
                    if (moveDirection.z < 0f)
                        moveDirection.z *= .5f;
                }
                __instance.horizMoveDir = MainCamera.camera.transform.rotation * moveDirection;
                Vector3 accel = __instance.horizMoveDir * __instance.forwardAccel;
                if (__instance.overWater && !ConfigMenu.hoverbikeMoveTweaks.Value)
                    accel = __instance.horizMoveDir * (__instance.forwardAccel / __instance.waterDampening);

                accel *= ConfigMenu.hoverbikeSpeedMult.Value;
                 __instance.rb.AddForce(accel);
                return false;
            }

        }


    }
}
