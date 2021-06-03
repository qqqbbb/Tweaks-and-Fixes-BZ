using System;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using HarmonyLib;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Player_Movement
    {
        static float oceanLevel;
        //static int invSize;
        static Equipment equipment;
        static Survival survival;
        static float swimMaxAllowedY = .6f; // .6

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Postfix(Player __instance)
            {
                oceanLevel = Ocean.GetOceanLevel();
                //invSize = Inventory.main.container.sizeX * Inventory.main.container.sizeY;
                equipment = Inventory.main.equipment;
                survival = Player.main.GetComponent<Survival>();
            }
        }
        // AlterMaxSpeed AdjustGroundSpeed
        public static float GetInvMult()
        {
            float massTotal = 0f;
            foreach (InventoryItem inventoryItem in Inventory.main.container)
            {
                Rigidbody rb = inventoryItem.item.GetComponent<Rigidbody>();
                if (rb)
                    massTotal += rb.mass;
            }

            float mult;
            if (Player.main.IsSwimming())
                mult = 100f - massTotal * Main.config.invMultWater;
            else
                mult = 100f - massTotal * Main.config.invMultLand;

            //float mult = massTotal * Main.config.InvMult;
            mult = Mathf.Clamp(mult, 0f, 100f);
            return mult * .01f;
        }

        [HarmonyPatch(typeof(Seaglide), "UpdateActiveState")]
        internal class Seaglide_UpdateActiveState_Patch
        { // seaglide works only if moving forward
            public static bool Prefix(Seaglide __instance)
            {
                if (!Main.config.playerMoveTweaks)
                    return true;

                int wasActive = __instance.activeState ? 1 : 0;
                __instance.activeState = false;
                if (__instance.energyMixin.charge > 0f)
                {
                    if (__instance.screenEffectModel != null)
                        __instance.screenEffectModel.SetActive(__instance.usingPlayer != null);
                    if (__instance.usingPlayer != null && __instance.usingPlayer.IsSwimming())
                    {
                        Vector3 moveDirection = GameInput.GetMoveDirection();
                        __instance.activeState = moveDirection.z > 0f;
                    }
                    if (__instance.powerGlideActive)
                        __instance.activeState = true;
                }
                int num2 = __instance.activeState ? 1 : 0;
                if (wasActive == num2)
                    return false;
                __instance.SetVFXActive(__instance.activeState);
                return false;
            }
        }

        [HarmonyPatch(typeof(UnderwaterMotor), "AlterMaxSpeed")]
        internal class AlterMaxSpeedPatch
        {
            public static bool Prefix(UnderwaterMotor __instance, float inMaxSpeed, ref float __result)
            {
                if (!Main.config.playerMoveTweaks)
                    return true;
                //AddDebug("AlterMaxSpeed");
                __result = inMaxSpeed;

                TechType suit = equipment.GetTechTypeInSlot("Body");
                if (suit != TechType.None)
                    __result *= 0.9f;
                //!!!
                //if (Player.main.motorMode != Player.MotorMode.Seaglide)
                //    Utils.AdjustSpeedScalarFromWeakness(ref __result);

                TechType fins = equipment.GetTechTypeInSlot("Foots");
                if (fins == TechType.Fins)
                    __result *= 1.2f;
                else if (fins == TechType.UltraGlideFins)
                    __result *= 1.3f;

                TechType tank = equipment.GetTechTypeInSlot("Tank");
                if (tank == TechType.PlasteelTank)
                    __result *= 0.97f;
                else if (tank != TechType.None)
                    __result *= 0.9f;


                if (Main.pda.isInUse)
                    __result *= 0.5f;
                else
                {
                    if (Player.main.motorMode != Player.MotorMode.Seaglide)
                    {
                        PlayerTool tool = Inventory.main.GetHeldTool();
                        if (tool)
                            __result *= 0.7f;
                    }
                }
                if (Player.main.gameObject.transform.position.y > oceanLevel)
                    __result *= 1.3f;

                //float ms = (float)System.Math.Round(Player.main.movementSpeed * 10f) / 10f;
                //Main.Message("movementSpeed  " + ms);
                __instance.currentPlayerSpeedMultipler = Mathf.MoveTowards(__instance.currentPlayerSpeedMultipler, __instance.playerSpeedModifier.Value, 0.3f * Time.deltaTime);
                __result *= __instance.currentPlayerSpeedMultipler;
                return false;
            }

            public static void Postfix(float inMaxSpeed, ref float __result)
            {
                if (Main.config.newHungerSystem)
                {
                    Seaglide seaglide = Inventory.main.GetHeldTool() as Seaglide;
                    //PlayerTool tool = Inventory.main.GetHeldTool() ;
                    //__instance = seaglide;
                    if (seaglide && seaglide.activeState)
                    { }
                    else
                    {
                        float foodMult = 1f;
                        float waterMult = 1f;
                        if (Main.survival.food < 0f)
                        {
                            foodMult = Mathf.Abs(Main.survival.food / 100f);
                            foodMult = 1f - foodMult;
                        }
                        if (Main.survival.water < 0f)
                        {
                            waterMult = Mathf.Abs(Main.survival.water / 100f);
                            waterMult = 1f - waterMult;
                        }
                        __result = (foodMult + waterMult) * .5f;
                    }
                }
                if (Main.config.invMultWater > 0f)
                    __result *= GetInvMult();
                //__instance.movementSpeed = __instance.playerController.velocity.magnitude / 5f;
                //float ms = (float)System.Math.Round(Player.main.movementSpeed * 10f) / 10f;
                //ms = Player.main.rigidBody.velocity.magnitude;
                //Main.Message("movementSpeed  " + ms);
            }
        }

        ////[HarmonyPatch(typeof(PlayerController), "SetMotorMode")]
        class PlayerController_SetMotorMode_Patch
        {
            static void Postfix(PlayerController __instance)
            {
                if (Main.config.playerMoveTweaks)
                { // underWaterController ignores these
                    //AddDebug("SetMotorMode");
                    __instance.underWaterController.backwardMaxSpeed = __instance.underWaterController.forwardMaxSpeed * .5f;
                    __instance.underWaterController.strafeMaxSpeed = __instance.underWaterController.backwardMaxSpeed;
                    __instance.underWaterController.verticalMaxSpeed = __instance.underWaterController.backwardMaxSpeed;

                    __instance.groundController.backwardMaxSpeed = __instance.groundController.forwardMaxSpeed * .5f;
                    __instance.groundController.strafeMaxSpeed = __instance.groundController.backwardMaxSpeed;
                    __instance.groundController.verticalMaxSpeed = __instance.groundController.backwardMaxSpeed;
                    //AddDebug("backwardMaxSpeed " + __instance.underWaterController.backwardMaxSpeed);
                    //Main.Log("forwardMaxSpeed " + __instance.forwardMaxSpeed);
                    //Main.Log("backwardMaxSpeed " + __instance.backwardMaxSpeed);
                }
            }
        }


        [HarmonyPatch(typeof(MainCameraControl), "GetCameraBob")]
        class MainCameraControl_GetCameraBob_Patch
        {
            static bool Prefix(MainCameraControl __instance, ref bool __result)
            {
                if (Main.config.playerMoveTweaks)
                {
                    __result = false;
                    return false;
                }
                Seaglide seaglide = Inventory.main.GetHeldTool() as Seaglide;
                bool seagliding = seaglide && seaglide.activeState;
                __result = Player.main.GetMode() == Player.Mode.Normal && __instance.swimCameraAnimation > 0f && !UWEXR.XRSettings.enabled && MiscSettings.cameraBobbing && !seagliding;
                //AddDebug(" seagliding " + seagliding);
                //AddDebug(" GetCameraBob " + __result);
                return false;
            }
        }

        [HarmonyPatch(typeof(UnderwaterMotor), "UpdateMove")]
        internal class UnderwaterMotor_UpdateMove_Patch
        { // strafe, backward, vertival speed halved
            public static bool Prefix(UnderwaterMotor __instance, ref Vector3 __result)
            {
                if (!Main.config.playerMoveTweaks)
                    return true;

                Rigidbody rb = __instance.rb;
                if (__instance.playerController == null || __instance.playerController.forwardReference == null)
                {
                    __result = rb.velocity;
                    return false;
                }
                __instance.fastSwimMode = Player.main.debugFastSwimAllowed && GameInput.GetButtonHeld(GameInput.Button.Sprint) && Inventory.main.equipment.GetTechTypeInSlot("Tank") != TechType.SuitBoosterTank;
                Vector3 velocity = rb.velocity;
                Vector3 input = __instance.movementInputDirection;
                input.Normalize();
                input.y *= .5f;
                input.x *= .5f;
                if (input.z < 0f)
                    input.z *= .5f;

                float y = input.y;
                float num1 = Mathf.Min(1f, input.magnitude);
                input.y = 0f;
                input.Normalize();
                float maxSpeed = 0f;
                if (input.z > 0f)
                    maxSpeed = __instance.forwardMaxSpeed;
                else if (input.z < 0f)
                    maxSpeed = __instance.backwardMaxSpeed;
                if (input.x != 0f)
                    maxSpeed = Mathf.Max(maxSpeed, __instance.strafeMaxSpeed);

                maxSpeed = __instance.AlterMaxSpeed(Mathf.Max(maxSpeed, __instance.verticalMaxSpeed)) * __instance.playerController.player.mesmerizedSpeedMultiplier;
                if (__instance.fastSwimMode)
                    maxSpeed *= 1000f;
                maxSpeed = maxSpeed * __instance.debugSpeedMult;
                maxSpeed = Mathf.Max(velocity.magnitude, maxSpeed);
                Vector3 vector3_2 = __instance.playerController.forwardReference.rotation * input;
                input = vector3_2;
                input.y += y;
                input.Normalize();
                if (!__instance.canSwim)
                {
                    input.y = 0f;
                    input.Normalize();
                }
                float acceleration = __instance.airAcceleration;
                if (__instance.grounded)
                    acceleration = __instance.groundAcceleration;
                else if (__instance.underWater)
                    acceleration = __instance.waterAcceleration * __instance.playerSpeedModifier.Value;
                //float num5 = acceleration;
                float num6 = (num1 * acceleration) * Time.deltaTime;
                if (num6 > 0f)
                {
                    Vector3 lhs = velocity + input * num6;
                    if (lhs.magnitude > maxSpeed)
                    {
                        lhs.Normalize();
                        lhs *= maxSpeed;
                    }

                    float num7 = Vector3.Dot(lhs, __instance.surfaceNormal);
                    if (!__instance.canSwim)
                        lhs -= num7 * __instance.surfaceNormal;
                    bool flag1 = y < 0f;
                    bool flag2 = vector3_2.y < -0.3f;
                    float num8 = 0.14f;
                    float num9 = -0.5f;
                    if (__instance.transform.position.y >= num9 && !flag1 && !flag2)
                    {
                        float num10 = Mathf.Pow(Mathf.Clamp01(((num8 - __instance.transform.position.y) / (num8 - num9))), 0.3f);
                        lhs.y *= num10;
                    }
                    Vector3 deltaMove = new Vector3(input.x, 0f, input.z) * 0.3f;
                    RaycastHit hit;
                    if (__instance.transform.position.y >= 0f && (__instance.recentlyCollided || __instance.TestWillCollide(deltaMove)) && !Player.main.playerController.Trace(__instance.transform.position, __instance.transform.position + Vector3.up * 1.85f, out hit))
                    {
                        Vector3 position = __instance.transform.position;
                        Vector3 from = __instance.transform.position + Vector3.up * 1.85f;
                        if (!Player.main.playerController.Trace(from, from + deltaMove, out hit) && Player.main.playerController.Trace(from + deltaMove, from + deltaMove + Vector3.down * 1.85f, out hit) && PlayerMotor.IsWalkable(hit.normal))
                        {
                            __instance.transform.position = from + deltaMove + Vector3.down * Mathf.Max(0f, hit.distance - 0.1f);
                            lhs.y = 0.2f;
                            MainCameraControl.main.stepAmount += __instance.transform.position.y - position.y;
                        }
                    }
                    rb.velocity = lhs;
                    __instance.desiredVelocity = lhs;
                }
                else
                    __instance.desiredVelocity = rb.velocity;

                float gravity = __instance.underWater ? __instance.underWaterGravity : __instance.gravity;
                if (gravity != 0f)
                {
                    rb.AddForce(new Vector3(0f, -gravity * Time.deltaTime, 0f), ForceMode.VelocityChange);
                    __instance.usingGravity = true;
                }
                else
                    __instance.usingGravity = false;

                float drag = __instance.airDrag;
                if (__instance.underWater)
                    drag = __instance.swimDrag;
                else if (__instance.grounded)
                    drag = __instance.groundDrag;
                rb.drag = drag;
                if (__instance.fastSwimMode)
                    rb.drag = 0.0f;
                __instance.grounded = false;
                __instance.vel = rb.velocity;
                __instance.recentlyCollided = false;

                __result = __instance.vel;
                //__result.Normalize();
                return false;
            }
        }

        private static float AdjustGroundSpeed(float maxSpeed)
        {
            TechType suit = equipment.GetTechTypeInSlot("Body");
            if (suit != TechType.None)
                maxSpeed *= 0.9f;
            TechType fins = equipment.GetTechTypeInSlot("Foots");
            if (fins != TechType.None)
                maxSpeed *= 0.9f;

            TechType tank = equipment.GetTechTypeInSlot("Tank");
            if (tank == TechType.PlasteelTank)
                maxSpeed *= 0.97f;
            else if (tank != TechType.None)
                maxSpeed *= 0.9f;

            //AddDebug("AdjustGroundSpeed " + maxSpeed);
            return maxSpeed;
        }

        [HarmonyPatch(typeof(GroundMotor), "ApplyInputVelocityChange")]
        internal class GroundMotor_ApplyInputVelocityChange_Patch
        {// can sprint only if moving forward, sideways and backward speed halved 
            public static bool Prefix(GroundMotor __instance, ref Vector3 __result, Vector3 velocity)
            {
                //if (!Main.config.playerMoveSpeedTweaks)
                //    return true;

                if (__instance.playerController == null || __instance.playerController.forwardReference == null)
                {
                    __result = Vector3.zero;
                    return false;
                }

                Quaternion quaternion = !__instance.underWater || !__instance.canSwim ? Quaternion.Euler(0.0f, __instance.playerController.forwardReference.rotation.eulerAngles.y, 0.0f) : __instance.playerController.forwardReference.rotation;
                Vector3 input = __instance.movementInputDirection;
                if (Main.config.playerMoveTweaks)
                {
                    input.Normalize();
                    input.x *= .5f;
                    if (input.z < 0f)
                        input.z *= .5f;
                }
                float inputMagn = Mathf.Min(1f, input.magnitude);
                float y = !__instance.underWater || !__instance.canSwim ? 0.0f : input.y;
                input.y = 0.0f;
                input = quaternion * input;
                input.y += y;
                input.Normalize();
                Vector3 hVelocity;
                if (__instance.grounded && !__instance.underWater && (__instance.TooSteep() && __instance.sliding.enabled))
                {
                    Vector3 slidingDirection = __instance.GetSlidingDirection();
                    Vector3 vector3_3 = Vector3.Project(__instance.movementInputDirection, slidingDirection);
                    hVelocity = (slidingDirection + vector3_3 * __instance.sliding.speedControl + (__instance.movementInputDirection - vector3_3) * __instance.sliding.sidewaysControl) * __instance.sliding.slidingSpeed;
                }
                else
                {
                    float mult = 1f;
                    if (Main.config.playerMoveTweaks)
                        mult = AdjustGroundSpeed(mult);

                    if (Main.config.invMultLand > 0f)
                        mult *= GetInvMult();

                    if (!__instance.underWater && __instance.sprintPressed && __instance.grounded)
                    {
                        float z = __instance.movementInputDirection.z;
                        if (z > 0f)
                            mult *= __instance.forwardSprintModifier;
                        else if (!Main.config.playerMoveTweaks && z == 0f)
                            mult *= __instance.strafeSprintModifier;
                        __instance.sprinting = true;
                    }
                    hVelocity = input * __instance.forwardMaxSpeed * mult * inputMagn;
                }
                if (!__instance.underWater && UWEXR.XRSettings.enabled)
                    hVelocity *= VROptions.groundMoveScale;
                if (!__instance.underWater && __instance.movingPlatform.enabled && __instance.movingPlatform.movementTransfer == GroundMotor.MovementTransferOnJump.PermaTransfer)
                {
                    hVelocity += __instance.movement.frameVelocity;
                    hVelocity.y = 0.0f;
                }
                if (!__instance.underWater)
                {
                    if (__instance.grounded)
                        hVelocity = __instance.AdjustGroundVelocityToNormal(hVelocity, __instance.groundNormal);
                    else
                        velocity.y = 0.0f;
                }
                float num4 = __instance.GetMaxAcceleration(__instance.grounded) * Time.deltaTime;

                Vector3 vector3_5 = hVelocity - velocity;
                if (vector3_5.sqrMagnitude > num4 * num4)
                    vector3_5 = vector3_5.normalized * num4;
                if (__instance.grounded || __instance.canControl)
                    velocity += vector3_5;
                if (__instance.grounded && !__instance.underWater)
                    velocity.y = Mathf.Min(velocity.y, 0.0f);
                __result = velocity;
                return false;
            }
        }


        //[HarmonyPatch(typeof(GroundMotor), "ApplyInputVelocityChange")]
        internal class GroundMotor_ApplyInputVelocityChange_Postfix_Patch
        {
            public static void Postfix(GroundMotor __instance, ref Vector3 __result, Vector3 velocity)
            {
                //if (Main.config.invMultLand > 0f)
                    __result *= AdjustGroundSpeed(1f);
            }

        }
    }
}
