﻿
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Exosuit), "ApplyJumpForce")]
    class Exosuit_ApplyJumpForce_Patch
    {
        static bool Prefix(Exosuit __instance)
        {
            if (__instance.timeLastJumped + 1.0 > Time.time)
                return false;

            if (__instance.onGround)
            {
                Utils.PlayFMODAsset(__instance.jumpSound, __instance.transform);
                if (__instance.IsUnderwater())
                {
                    if (Physics.Raycast(new Ray(__instance.transform.position, Vector3.down), out RaycastHit hitInfo, 10f))
                    {
                        TerrainChunkPieceCollider tcpc = hitInfo.collider.GetComponent<TerrainChunkPieceCollider>();
                        if (tcpc)
                        {
                            __instance.fxcontrol.Play(2);
                            //AddDebug("Landed on terrain ");
                        }
                        else
                            __instance.fxcontrol.Play(1);
                    }
                }

            }
            __instance.ConsumeEngineEnergy(1.2f);
            __instance.useRigidbody.AddForce(Vector3.up * (__instance.jumpJetsUpgraded ? 7f : 5f), ForceMode.VelocityChange);
            __instance.timeLastJumped = Time.time;
            __instance.timeOnGround = 0.0f;
            __instance.onGround = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Exosuit), "OnLand")]
    class Exosuit_OnLand_Patch
    { 
        static bool Prefix(Exosuit __instance)
        {
            Utils.PlayFMODAsset(__instance.landSound, __instance.bottomTransform);
            if (__instance.IsUnderwater())
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(new Ray(__instance.transform.position, Vector3.down), out hitInfo, 10f))
                {
                    //if (hitInfo.transform && hitInfo.transform.gameObject)
                    //{
                        //AddDebug("Landed on  " + hitInfo.transform.gameObject.name);
                        //VFXSurface surface = hitInfo.transform.gameObject.GetComponent<VFXSurface>();
                    TerrainChunkPieceCollider tcpc = hitInfo.collider.GetComponent<TerrainChunkPieceCollider>();
                        //if (surface)
                        //    AddDebug("surfaceType  " + surface.surfaceType);
                    if (tcpc)
                    {
                        __instance.fxcontrol.Play(4);
                        //AddDebug("Landed on terrain ");
                    }
                    else
                        __instance.fxcontrol.Play(3);
                }      
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(CollisionSound), "OnCollisionEnter")]
    class CollisionSound_OnCollisionEnter_Patch
    {
        static bool Prefix(CollisionSound __instance, Collision col)
        {
            //AddDebug("OnCollisionEnter");
            //Main.Log("OnCollisionEnter");

            Exosuit exosuit = __instance.GetComponent<Exosuit>();
            Rigidbody rb = UWE.Utils.GetRootRigidbody(col.gameObject);
            if (exosuit && !rb)
                return false;// no sounds when walking on ground

            float magnitude = col.relativeVelocity.magnitude;
            //FMODAsset asset = !rootRigidbody || rootRigidbody.mass >= 10.0 ? (magnitude <= 8.0 ? (magnitude <= 4.0 ? __instance.hitSoundSlow : __instance.hitSoundMedium) : __instance.hitSoundFast) : __instance.hitSoundSmall;
            FMODAsset asset = null;
            if (!rb || rb.mass >= 10.0f)
            {
                if (magnitude < 4f)
                    asset = __instance.hitSoundSlow;
                else if (magnitude < 8f)
                    asset = __instance.hitSoundMedium;
                else
                    asset = __instance.hitSoundFast;
            }
            else if (col.gameObject.GetComponent<Creature>())
                asset = __instance.hitSoundSmall;// fish splat sound
            else
                asset = __instance.hitSoundSlow;

            if (asset)
            {
                //AddDebug("col magnitude " + magnitude);
                float soundRadiusObsolete = Mathf.Clamp01(magnitude / 8f);
                Utils.PlayFMODAsset(asset, col.contacts[0].point, soundRadiusObsolete);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Exosuit), "Start")]
    class Exosuit_Start_Patch
    {
        static void Postfix(Exosuit __instance)
        {
            CollisionSound collisionSound = __instance.gameObject.EnsureComponent<CollisionSound>();

            FMODAsset so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/common/fishsplat";
            so.id = "{0e47f1c6-6178-41bd-93bf-40bfca179cb6}";
            collisionSound.hitSoundSmall = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_hard";
            so.id = "{ed65a390-2e80-4005-b31b-56380500df33}";
            collisionSound.hitSoundFast = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_medium";
            so.id = "{cb2927bf-3f8d-45d8-afe2-c82128f39062}";
            collisionSound.hitSoundMedium = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_soft";
            so.id = "{15dc7344-7b0a-4ffd-9b5c-c40f923e4f4d}";
            collisionSound.hitSoundSlow = so;
        }
    }

    // thrusters consumes 2x energy
    // no limit on thrusters
    [HarmonyPatch(typeof(Exosuit), "Update")]
    class Exosuit_Update_Patch
    {
        static void VehicleUpdate(Vehicle vehicle)
        {
            if (vehicle.CanPilot())
            {
                vehicle.steeringWheelYaw = Mathf.Lerp(vehicle.steeringWheelYaw, 0.0f, Time.deltaTime);
                vehicle.steeringWheelPitch = Mathf.Lerp(vehicle.steeringWheelPitch, 0.0f, Time.deltaTime);
                if (vehicle.mainAnimator)
                {
                    vehicle.mainAnimator.SetFloat("view_yaw", vehicle.steeringWheelYaw * 70f);
                    vehicle.mainAnimator.SetFloat("view_pitch", vehicle.steeringWheelPitch * 45f);
                }
            }
            if (vehicle.GetPilotingMode() && vehicle.CanPilot() && (vehicle.moveOnLand || vehicle.transform.position.y < Ocean.GetOceanLevel()))
            {
                Vector2 vector2 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetLookDelta() : Vector2.zero;
                vehicle.steeringWheelYaw = Mathf.Clamp(vehicle.steeringWheelYaw + vector2.x * vehicle.steeringReponsiveness, -1f, 1f);
                vehicle.steeringWheelPitch = Mathf.Clamp(vehicle.steeringWheelPitch + vector2.y * vehicle.steeringReponsiveness, -1f, 1f);
                if (vehicle.controlSheme == Vehicle.ControlSheme.Submersible)
                {
                    float num = 3f;
                    vehicle.useRigidbody.AddTorque(vehicle.transform.up * vector2.x * vehicle.sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
                    vehicle.useRigidbody.AddTorque(vehicle.transform.right * -vector2.y * vehicle.sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
                    vehicle.useRigidbody.AddTorque(vehicle.transform.forward * -vector2.x * vehicle.sidewaysTorque * 0.0002f * num, ForceMode.VelocityChange);
                }
                else if (vehicle.controlSheme == Vehicle.ControlSheme.Submarine || vehicle.controlSheme == Vehicle.ControlSheme.Mech)
                {
                    if (vector2.x != 0.0)
                        vehicle.useRigidbody.AddTorque(vehicle.transform.up * vector2.x * vehicle.sidewaysTorque, ForceMode.VelocityChange);
                }
                else if (vehicle.controlSheme == Vehicle.ControlSheme.Hoverbike)
                    vehicle.useRigidbody.AddRelativeTorque(new Vector3(vector2.y, 0.0f, 0.0f));
            }
            bool powered = vehicle.IsPowered();
            if (vehicle.wasPowered != powered)
            {
                vehicle.wasPowered = powered;
                vehicle.OnPoweredChanged(powered);
            }
            vehicle.ReplenishOxygen();
        }

        public static bool Prefix_OLD(Exosuit __instance)
        {
            //Vehicle vehicle = __instance as Vehicle;
            //vehicle.Update();
            if (!Main.config.exosuitMoveTweaks)
                return true;

            VehicleUpdate(__instance);

            __instance.UpdateThermalReactorCharge();
            __instance.openedFraction = !__instance.storageContainer.GetOpen() ? Mathf.Clamp01(__instance.openedFraction - Time.deltaTime * 2f) : Mathf.Clamp01(__instance.openedFraction + Time.deltaTime * 2f);
            __instance.storageFlap.localEulerAngles = new Vector3(__instance.startFlapPitch + __instance.openedFraction * 80f, 0.0f, 0.0f);
            bool pilotingMode = __instance.GetPilotingMode();
            bool onGround = __instance.onGround || Time.time - __instance.timeOnGround <= 0.5f;
            __instance.mainAnimator.SetBool("sit", !pilotingMode & onGround && !__instance.IsUnderwater());
            bool inUse = pilotingMode && !__instance.docked;
            if (pilotingMode)
            {
                Player.main.transform.localPosition = Vector3.zero;
                Player.main.transform.localRotation = Quaternion.identity;
                Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                bool thrusterOn = input.y > 0f;
                bool hasPower = __instance.IsPowered() && __instance.liveMixin.IsAlive();

                __instance.GetEnergyValues(out float charge, out float capacity);
                __instance.thrustPower = Main.NormalizeTo01range(charge, 0f, capacity);
                //Main.Message("thrustPower " + __instance.thrustPower);
                if (thrusterOn & hasPower)
                {
                    if ((__instance.onGround || Time.time - __instance.timeOnGround <= 1f) && !__instance.jetDownLastFrame)
                        __instance.ApplyJumpForce();
                    __instance.jetsActive = true;
                }
                else
                {
                    __instance.jetsActive = false;
                }
                //AddDebug("jetsActive" + __instance.jetsActive);
                __instance.jetDownLastFrame = thrusterOn;

                if (__instance.timeJetsActiveChanged + 0.3f < Time.time)
                {
                    if (__instance.jetsActive && __instance.thrustPower > 0.0f)
                    {
                        //__instance.loopingJetSound.Play();
                        __instance.fxcontrol.Play(0);
                        __instance.areFXPlaying = true;
                    }
                    else if (__instance.areFXPlaying)
                    {
                        //__instance.loopingJetSound.Stop();
                        __instance.fxcontrol.Stop(0);
                        __instance.areFXPlaying = false;
                    }
                }
                float energyCost = __instance.thrustConsumption * Time.deltaTime;
                if (thrusterOn)
                {
                    __instance.ConsumeEngineEnergy(energyCost * 2f);
                    //Main.Message("Consume Energy thrust" + energyCost * 2f);
                }
                else if (input.z != 0f)
                {
                    __instance.ConsumeEngineEnergy(energyCost);
                    //Main.Message("Consume Energy move" + energyCost);
                }
                if (__instance.jetsActive)
                    __instance.thrustIntensity += Time.deltaTime / __instance.timeForFullVirbation;
                else
                    __instance.thrustIntensity -= Time.deltaTime * 10f;

                __instance.thrustIntensity = Mathf.Clamp01(__instance.thrustIntensity);
                if (AvatarInputHandler.main.IsEnabled())
                {
                    Vector3 eulerAngles = __instance.transform.eulerAngles;
                    eulerAngles.x = MainCamera.camera.transform.eulerAngles.x;
                    Quaternion aimDirection1 = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    Quaternion aimDirection2 = aimDirection1;
                    __instance.leftArm.Update(ref aimDirection1);
                    __instance.rightArm.Update(ref aimDirection2);
                    if (inUse)
                    {
                        __instance.aimTargetLeft.transform.position = MainCamera.camera.transform.position + aimDirection1 * Vector3.forward * 100f;
                        __instance.aimTargetRight.transform.position = MainCamera.camera.transform.position + aimDirection2 * Vector3.forward * 100f;
                    }
                    __instance.UpdateUIText(__instance.rightArm is ExosuitPropulsionArm || __instance.leftArm is ExosuitPropulsionArm);
                    if (GameInput.GetButtonDown(GameInput.Button.AltTool) && !__instance.rightArm.OnAltDown())
                        __instance.leftArm.OnAltDown();
                }
                //__instance.UpdateActiveTarget(__instance.HasClaw(), __instance.HasDrill());
                __instance.UpdateSounds();
            }
            if (!inUse)
            {
                bool flag3 = false;
                bool flag4 = false;
                if (!Mathf.Approximately(__instance.aimTargetLeft.transform.localPosition.y, 0.0f))
                    __instance.aimTargetLeft.transform.localPosition = new Vector3(__instance.aimTargetLeft.transform.localPosition.x, UWE.Utils.Slerp(__instance.aimTargetLeft.transform.localPosition.y, 0.0f, Time.deltaTime * 50f), __instance.aimTargetLeft.transform.localPosition.z);
                else
                    flag3 = true;
                if (!Mathf.Approximately(__instance.aimTargetRight.transform.localPosition.y, 0.0f))
                    __instance.aimTargetRight.transform.localPosition = new Vector3(__instance.aimTargetRight.transform.localPosition.x, UWE.Utils.Slerp(__instance.aimTargetRight.transform.localPosition.y, 0.0f, Time.deltaTime * 50f), __instance.aimTargetRight.transform.localPosition.z);
                else
                    flag4 = true;
                if (flag3 & flag4)
                    __instance.SetIKEnabled(false);
            }
            __instance.UpdateAnimations();
            if (__instance.armsDirty)
                __instance.UpdateExosuitArms();

            if (!__instance.cinematicMode && __instance.rotationDirty)
            {
                Vector3 localEulerAngles = __instance.transform.localEulerAngles;
                Quaternion b = Quaternion.Euler(0.0f, localEulerAngles.y, 0.0f);
                if ((double)Mathf.Abs(localEulerAngles.x) < 1.0 / 1000.0 && (double)Mathf.Abs(localEulerAngles.z) < 1.0 / 1000.0)
                {
                    __instance.rotationDirty = false;
                    __instance.transform.localRotation = b;
                }
                else
                    __instance.transform.localRotation = Quaternion.Lerp(__instance.transform.localRotation, b, Time.deltaTime * 3f);
            }
            return false;
        }

        public static bool Prefix(Exosuit __instance)
        {
            //Vehicle vehicle = __instance as Vehicle;
            //vehicle.Update();
            if (!Main.config.exosuitMoveTweaks)
                return true;

            VehicleUpdate(__instance);

            __instance.UpdateThermalReactorCharge();
            __instance.openedFraction = !__instance.storageContainer.GetOpen() ? Mathf.Clamp01(__instance.openedFraction - Time.deltaTime * 2f) : Mathf.Clamp01(__instance.openedFraction + Time.deltaTime * 2f);
            __instance.storageFlap.localEulerAngles = new Vector3(__instance.startFlapPitch + __instance.openedFraction * 80f, 0.0f, 0.0f);
            int inUse = __instance.GetPilotingMode() ? 1 : 0;
            bool inUse_ = inUse != 0 && !__instance.docked;
            if (inUse != 0)
            {
                Player.main.transform.localPosition = Vector3.zero;
                Player.main.transform.localRotation = Quaternion.identity;
                Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                __instance.lastMoveDirection = input;
                bool thrustersOn = input.y > 0.0;
                bool sprinting = GameInput.GetButtonHeld(GameInput.Button.Sprint);
                bool isPowered = __instance.IsPowered() && __instance.liveMixin.IsAlive();
                __instance.GetEnergyValues(out float charge, out float capacity);
                float powerMult = 1f; // my
                if ((thrustersOn | sprinting) & isPowered)
                {
                    powerMult = 2f;
                    //AddDebug("thrustConsumption " + __instance.thrustConsumption);
                    //AddDebug("verticalJetConsumption " + __instance.verticalJetConsumption);
                    //AddDebug("horizontalJetConsumption " + __instance.horizontalJetConsumption);
                    //float mult = 0.0f;
                    //if (thrustersOn)
                    //    mult += __instance.verticalJetConsumption;
                    //if (sprinting)
                    //    mult += __instance.horizontalJetConsumption;
                    //__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower - Time.deltaTime * __instance.thrustConsumption * mult);
                    __instance.thrustPower = Main.NormalizeTo01range(charge, 0f, capacity);
                    if (thrustersOn && (__instance.onGround || Time.time - __instance.timeOnGround <= 1.0) && !__instance.jetDownLastFrame)
                        __instance.ApplyJumpForce();
                    __instance.jetsActive = true;
                    __instance.horizontalJetsActive = sprinting;
                    __instance.verticalJetsActive = thrustersOn;
                }
                else
                {
                    __instance.jetsActive = false;
                    __instance.horizontalJetsActive = false;
                    __instance.verticalJetsActive = false;
                    //float num2 = 0.7f;
                    //if (__instance.onGround)
                    //    num2 = 4f;
                    //else if (input.x != 0f || input.z != 0f)
                    //    num2 = -0.7f;
                    //__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower + Time.deltaTime * __instance.thrustConsumption * num2);
                    __instance.thrustPower = Main.NormalizeTo01range(charge, 0f, capacity);
                }
                __instance.jetDownLastFrame = thrustersOn;
                __instance.footStepSounds.soundsEnabled = !__instance.powersliding;
                __instance.movementEnabled = !__instance.powersliding;
                if (__instance.timeJetsActiveChanged + 0.3f <= Time.time)
                {
                    if ((__instance.jetsActive || __instance.powersliding) && (__instance.thrustPower > 0f && !__instance.areFXPlaying) && !__instance.IsUnderwater())
                    {
                        __instance.fxcontrol.Play(0);
                        __instance.areFXPlaying = true;
                    }
                    else if (__instance.areFXPlaying)
                    {
                        __instance.fxcontrol.Stop(0);
                        __instance.areFXPlaying = false;
                    }
                }

                if (__instance.powersliding)
                    __instance.loopingSlideSound.Play();
                else
                    __instance.loopingSlideSound.Stop();
                if ((thrustersOn || input.x != 0f ? 1 : (input.z != 0f ? 1 : 0)) != 0)
                    __instance.ConsumeEngineEnergy(0.08333334f * Time.deltaTime * powerMult);

                if (__instance.jetsActive)
                    __instance.thrustIntensity += Time.deltaTime / __instance.timeForFullVirbation;
                else
                    __instance.thrustIntensity -= Time.deltaTime * 10f;
                __instance.thrustIntensity = Mathf.Clamp01(__instance.thrustIntensity);
                if (AvatarInputHandler.main.IsEnabled() && !__instance.ignoreInput)
                {
                    Vector3 eulerAngles = __instance.transform.eulerAngles;
                    eulerAngles.x = MainCamera.camera.transform.eulerAngles.x;
                    Quaternion aimDirection1 = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    Quaternion aimDirection2 = aimDirection1;
                    __instance.leftArm.Update(ref aimDirection1);
                    __instance.rightArm.Update(ref aimDirection2);
                    if (inUse_)
                    {
                        Vector3 b1 = MainCamera.camera.transform.position + aimDirection1 * Vector3.forward * 100f;
                        Vector3 b2 = MainCamera.camera.transform.position + aimDirection2 * Vector3.forward * 100f;
                        __instance.aimTargetLeft.transform.position = Vector3.Lerp(__instance.aimTargetLeft.transform.position, b1, Time.deltaTime * 15f);
                        __instance.aimTargetRight.transform.position = Vector3.Lerp(__instance.aimTargetRight.transform.position, b2, Time.deltaTime * 15f);
                    }
                    __instance.UpdateUIText(__instance.rightArm is ExosuitPropulsionArm || __instance.leftArm is ExosuitPropulsionArm);
                    if (GameInput.GetButtonDown(GameInput.Button.AltTool) && !__instance.rightArm.OnAltDown())
                        __instance.leftArm.OnAltDown();
                }
                __instance.UpdateActiveTarget();
                __instance.UpdateSounds();
                if (__instance.powersliding && __instance.onGround && __instance.timeLastSlideEffect + 0.5 < Time.time)
                {
                    if (__instance.IsUnderwater())
                        __instance.fxcontrol.Play(4);
                    else
                        __instance.fxcontrol.Play(3);
                    __instance.timeLastSlideEffect = Time.time;
                }
            }
            if (!inUse_)
            {
                bool flag2 = false;
                bool flag3 = false;
                if (!Mathf.Approximately(__instance.aimTargetLeft.transform.localPosition.y, 0.0f))
                    __instance.aimTargetLeft.transform.localPosition = new Vector3(__instance.aimTargetLeft.transform.localPosition.x, Mathf.MoveTowards(__instance.aimTargetLeft.transform.localPosition.y, 0.0f, Time.deltaTime * 50f), __instance.aimTargetLeft.transform.localPosition.z);
                else
                    flag2 = true;
                if (!Mathf.Approximately(__instance.aimTargetRight.transform.localPosition.y, 0.0f))
                    __instance.aimTargetRight.transform.localPosition = new Vector3(__instance.aimTargetRight.transform.localPosition.x, Mathf.MoveTowards(__instance.aimTargetRight.transform.localPosition.y, 0.0f, Time.deltaTime * 50f), __instance.aimTargetRight.transform.localPosition.z);
                else
                    flag3 = true;
                if (flag2 & flag3)
                    __instance.SetIKEnabled(false);
            }
            __instance.UpdateAnimations();
            if (!__instance.armsDirty)
                return false;
            __instance.UpdateExosuitArms();
            return false;
        }
    }

    [HarmonyPatch(typeof(Vehicle), "ApplyPhysicsMove")]
    class Vehicle_ApplyPhysicsMove_patch
    {  // disable strafing
        public static bool Prefix(Vehicle __instance)
        {
            if (!Main.config.exosuitMoveTweaks)
                return true;

            if (!__instance.GetPilotingMode())
                return false;

            if (__instance.worldForces.IsAboveWater() != __instance.wasAboveWater)
            {
                __instance.PlaySplashSound();
                __instance.wasAboveWater = __instance.worldForces.IsAboveWater();
            }
            if (!(__instance.moveOnLand | (__instance.transform.position.y < Ocean.GetOceanLevel() && __instance.transform.position.y < __instance.worldForces.waterDepth && !__instance.forceWalkMotorMode)) || !__instance.movementEnabled)
                return false;

            Vector3 inputRaw = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
            Vector3 input = new Vector3(0f, 0f, inputRaw.z);
            //if (input.z < 0)
            //    input.z *= .5f;

            float num = (Mathf.Abs(input.x) * __instance.sidewardForce + Mathf.Max(0.0f, input.z) * __instance.forwardForce + Mathf.Max(0.0f, -input.z) * __instance.backwardForce);
            input = __instance.transform.rotation * input;
            input.y = 0.0f;
            input = Vector3.Normalize(input);
            if (__instance.onGround)
            {
                input = Vector3.ProjectOnPlane(input, __instance.surfaceNormal);
                input.y = Mathf.Clamp(input.y, -0.5f, 0.5f);
                num *= __instance.onGroundForceMultiplier;
            }
            //if (Application.isEditor)
            //    Debug.DrawLine(__instance.transform.position, __instance.transform.position + vector * 4f, Color.white);
            Vector3 vector3_4 = new Vector3(0.0f, inputRaw.y, 0.0f);
            vector3_4.y *= __instance.verticalForce * Time.deltaTime;
            Vector3 acceleration = num * input * Time.deltaTime + vector3_4;
            __instance.OverrideAcceleration(ref acceleration);
            for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                __instance.accelerationModifiers[index].ModifyAcceleration(ref acceleration);
            __instance.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);

            return false;
        }
    }

    //[HarmonyPatch(typeof(ExosuitDrillArm))]
    //[HarmonyPatch("OnHit")]
    class ExosuitDrillArm_OnHit_Patch
    { // fix not showing particles when start drilling
        public static bool Prefix(ExosuitDrillArm __instance)
        {
            //AddDebug("OnHit");
            if (!__instance.exosuit.CanPilot() || !__instance.exosuit.GetPilotingMode())
                return false;
            Vector3 zero = Vector3.zero;
            GameObject closestObj = null;
            __instance.drillTarget = null;
            UWE.Utils.TraceFPSTargetPosition(__instance.exosuit.gameObject, 5f, ref closestObj, ref zero);
            if (closestObj == null)
            {
                InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                if (component != null && component.GetMostRecent() != null)
                    closestObj = component.GetMostRecent().gameObject;
            }
            if (closestObj && __instance.drilling)
            {
                Drillable ancestor1 = closestObj.FindAncestor<Drillable>();
                __instance.loopHit.Play();
                if (ancestor1)
                {
                    GameObject hitObject;
                    ancestor1.OnDrill(__instance.fxSpawnPoint.position, __instance.exosuit, out hitObject);
                    __instance.drillTarget = hitObject;
                    //if (__instance.fxControl.emitters[0].fxPS == null || __instance.fxControl.emitters[0].fxPS.emission.enabled) 
                    //AddDebug("emission.enabled " + __instance.fxControl.emitters[0].fxPS.emission.enabled);
                    //AddDebug("IsAlive " + __instance.fxControl.emitters[0].fxPS.IsAlive());
                    if (__instance.fxControl.emitters[0].fxPS != null && (!__instance.fxControl.emitters[0].fxPS.IsAlive() || !__instance.fxControl.emitters[0].fxPS.emission.enabled))
                    {
                        __instance.fxControl.Play(0);
                    }

                }
                else
                {
                    LiveMixin ancestor2 = closestObj.FindAncestor<LiveMixin>();
                    if (ancestor2)
                    {
                        ancestor2.IsAlive();
                        ancestor2.TakeDamage(4f, zero, DamageType.Drill);
                        __instance.drillTarget = closestObj;
                    }
                    VFXSurface component = closestObj.GetComponent<VFXSurface>();
                    if (__instance.drillFXinstance == null)
                        __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(component, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                    else if (component != null && __instance.prevSurfaceType != component.surfaceType)
                    {
                        __instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
                        UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 1.6f);
                        __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(component, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                        __instance.prevSurfaceType = component.surfaceType;
                    }
                    closestObj.SendMessage("BashHit", __instance, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
                __instance.StopEffects();   

            return false;
        }

    }
}

