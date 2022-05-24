
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Exosuit))]
    class Exosuit_Patch
    {
        public static string exosuitName;
        public static string leftArm;
        public static string rightArm;

        public static bool armNamesChanged = false;
        public static bool exosuitStarted = false;

        static string GetTorpedoName(Exosuit exosuit, int slot)
        {
            //AddDebug("GetTorpedoName " + slot);
            ItemsContainer container = exosuit.GetStorageInSlot(slot, TechType.ExosuitTorpedoArmModule);
            TorpedoType[] torpedoTypes = exosuit.torpedoTypes;
            for (int index = 0; index < torpedoTypes.Length; ++index)
            {
                TechType torpedoType = torpedoTypes[index].techType;
                if (container.Contains(torpedoType))
                    return Language.main.Get(torpedoType) + " x" + container.GetCount(torpedoType);
            }
            string name = Language.main.Get(TechType.ExosuitTorpedoArmModule);
            name = name.Replace(exosuitName, "");
            name = name.TrimStart();
            name = name[0].ToString().ToUpper() + name.Substring(1);
            //AddDebug("GetTorpedoName " + name);
            return name;
        }

        static public void GetArmNames(Exosuit exosuit)
        {
            //AddDebug("GetNames " + exosuit.name);
            if (exosuit.currentLeftArmType == TechType.ExosuitTorpedoArmModule)
                leftArm = GetTorpedoName(exosuit, 0);
            else
            {
                //AddDebug("GetNames TooltipFactory.stringLeftHand " + uGUI.FormatButton(GameInput.Button.LeftHand));
                leftArm = Language.main.Get(exosuit.currentLeftArmType);
                leftArm = leftArm.Replace(exosuitName, "");
                leftArm = leftArm.TrimStart();
                leftArm = leftArm[0].ToString().ToUpper() + leftArm.Substring(1);
                //AddDebug("GetArmNames leftArm " + leftArm);
            }
            //AddDebug("leftArm " + leftArm);
            if (exosuit.currentRightArmType == TechType.ExosuitTorpedoArmModule)
                rightArm = GetTorpedoName(exosuit, 1);
            else
            {
                rightArm = Language.main.Get(exosuit.currentRightArmType);
                rightArm = rightArm.Replace(exosuitName, "");
                rightArm = rightArm.TrimStart();
                rightArm = rightArm[0].ToString().ToUpper() + rightArm.Substring(1);
                //AddDebug("GetArmNames rightArm " + rightArm);
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPostfix(Exosuit __instance)
        {
            exosuitName = Language.main.Get("Exosuit");
            //rightButton = uGUI.FormatButton(GameInput.Button.RightHand);
            //leftButton = uGUI.FormatButton(GameInput.Button.LeftHand);
            GetArmNames(__instance);
            armNamesChanged = true;

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
            exosuitStarted = true;
        }
     
        [HarmonyPatch("ApplyJumpForce")]
        [HarmonyPrefix]
        static bool ApplyJumpForcePrefix(Exosuit __instance)
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
      
        [HarmonyPatch("OnLand")]
        [HarmonyPrefix]
        static bool OnLandPrefix(Exosuit __instance)
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
        // thrusters consumes 2x energy
        // no limit on thrusters
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool UpdatePrefix(Exosuit __instance)
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
      
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void UpdatePostfix(Exosuit __instance)
        {
            if (!IngameMenu.main.isActiveAndEnabled && !Main.pda.isInUse && Player.main.currentMountedVehicle == __instance && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
            {
                Transform lightsT = __instance.transform.Find("lights_parent");
                if (lightsT)
                {
                    //AddDebug("IngameMenu isActiveAndEnabled " + IngameMenu.main.isActiveAndEnabled);
                        if (!lightsT.gameObject.activeSelf && __instance.energyInterface.hasCharge)
                            lightsT.gameObject.SetActive(true);
                        else if (lightsT.gameObject.activeSelf)
                            lightsT.gameObject.SetActive(false);
                    //AddDebug("lights " + lightsT.gameObject.activeSelf);
                }
            }
        }
     
        [HarmonyPatch("UpdateUIText")]
        [HarmonyPrefix]
        public static bool UpdateUITextPrefix(Exosuit __instance, bool hasPropCannon)
        {
            if (armNamesChanged || !__instance.hasInitStrings || __instance.lastHasPropCannon != hasPropCannon)
            {
                string buttonFormat1 = LanguageCache.GetButtonFormat("ExosuitBoost", GameInput.Button.Sprint);
                string buttonFormat2 = LanguageCache.GetButtonFormat("ExosuitJump", GameInput.Button.MoveUp);
                string buttonFormat3 = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
                string lightsButton = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.Deconstruct);
                __instance.sb.Length = 0;
                __instance.sb.AppendLine(Language.main.GetFormat<string, string, string>("ExosuitBoostJumpExitFormat", buttonFormat1, buttonFormat2, buttonFormat3));
                __instance.sb.Append(leftArm);
                __instance.sb.Append(" ");
                __instance.sb.Append(UI_Patches.leftHandButton);
                __instance.sb.Append("  ");
                __instance.sb.Append(rightArm);
                __instance.sb.Append(" ");
                __instance.sb.Append(UI_Patches.rightHandButton);
                __instance.sb.Append("  ");
                __instance.sb.Append(lightsButton);
                if (hasPropCannon)
                    __instance.sb.AppendLine(LanguageCache.GetButtonFormat("PropulsionCannonToRelease", GameInput.Button.AltTool));
                __instance.lastHasPropCannon = hasPropCannon;
                __instance.uiStringPrimary = __instance.sb.ToString();
                armNamesChanged = false;
            }
            //HandReticle.main.SetTextRaw(HandReticle.TextType.Use, __instance.uiStringPrimary);
            //HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, string.Empty);
            HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, __instance.uiStringPrimary);
            __instance.hasInitStrings = true;
            return false;
        }

        [HarmonyPatch("OnUpgradeModuleChange")]
        [HarmonyPostfix]
        public static void OnUpgradeModuleChangePostfix(Exosuit __instance, int slotID, TechType techType, bool added)
        { // runs before Exosuit.Start
            //AddDebug("OnUpgradeModuleChange " + techType + " " + added + " " + slotID);
            if (!exosuitStarted)
                return;

            if (!added)
            {
                if (slotID == 0)
                {
                    leftArm = Language.main.Get(TechType.ExosuitClawArmModule);
                    leftArm = leftArm.Replace(exosuitName, "");
                    leftArm = leftArm.TrimStart();
                    leftArm = leftArm[0].ToString().ToUpper() + leftArm.Substring(1);
                }
                else if (slotID == 1)
                {
                    rightArm = Language.main.Get(TechType.ExosuitClawArmModule);
                    rightArm = rightArm.Replace(exosuitName, "");
                    rightArm = rightArm.TrimStart();
                    rightArm = rightArm[0].ToString().ToUpper() + rightArm.Substring(1);
                }
            }
            else if (added)
            {
                if (slotID == 0)
                {
                    if (techType == TechType.ExosuitTorpedoArmModule)
                        leftArm = GetTorpedoName(__instance, 0);
                    else
                    {
                        leftArm = Language.main.Get(techType);
                        leftArm = leftArm.Replace(exosuitName, "");
                        leftArm = leftArm.TrimStart();
                        leftArm = leftArm[0].ToString().ToUpper() + leftArm.Substring(1);
                    }
                }
                else if (slotID == 1)
                {
                    if (techType == TechType.ExosuitTorpedoArmModule)
                        rightArm = GetTorpedoName(__instance, 1);
                    else
                    {
                        rightArm = Language.main.Get(techType);
                        rightArm = rightArm.Replace(exosuitName, "");
                        rightArm = rightArm.TrimStart();
                        rightArm = rightArm[0].ToString().ToUpper() + rightArm.Substring(1);
                    }
                }
            }
            armNamesChanged = true;
            //AddDebug("OnUpgradeModuleChange currentLeftArmType " + __instance.currentLeftArmType);
            //AddDebug("OnUpgradeModuleChange currentRightArmType " + __instance.currentRightArmType);
        }


    }

    [HarmonyPatch(typeof(ExosuitTorpedoArm), "Shoot")]
    class ExosuitTorpedoArm_Shoot_Patch
    {
        static void Postfix(ExosuitTorpedoArm __instance, TorpedoType torpedoType, bool __result)
        {
            //AddDebug("ExosuitTorpedoArm Shoot " + torpedoType.techType + " " + __result);
            Exosuit_Patch.GetArmNames(__instance.exosuit);
            Exosuit_Patch.armNamesChanged = true;
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

    [HarmonyPatch(typeof(ExosuitDrillArm))]
    class ExosuitDrillArm_Patch
    {
        [HarmonyPatch("StopEffects")]
        [HarmonyPrefix]
        static bool StopEffectsPrefix(ExosuitDrillArm __instance)
        { // dont stop drilling sound when not hitting anything
            //AddDebug("StopEffects ");
            if (__instance.drillFXinstance != null)
            {
                __instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
                UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 1.6f);
                __instance.drillFXinstance = null;
            }
            if (__instance.fxControl.emitters[0].fxPS != null && __instance.fxControl.emitters[0].fxPS.emission.enabled)
                __instance.fxControl.Stop(0);
            //__instance.loop.Stop();
            __instance.loopHit.Stop();
            return false;
        }

        [HarmonyPatch("IExosuitArm.OnUseUp")]
        [HarmonyPostfix]
        static void OnUseUpPostfix(ExosuitDrillArm __instance)
        {
            //AddDebug("OnUseUp ");
            __instance.loop.Stop();
        }
    }

    [HarmonyPatch(typeof(SeamothStorageContainer), "OnCraftEnd")]
    class SeamothStorageContainer_OnCraftEnd_Patch
    {
       static private IEnumerator OnTorpedoCraftEnd(SeamothStorageContainer smsc)
        {
            TaskResult<GameObject> taskResult = new TaskResult<GameObject>();
            TaskResult<Pickupable> pickupableResult = new TaskResult<Pickupable>();
            for (int index = 0; index < Main.config.freeTorpedos; ++index)
            {
                yield return CraftData.InstantiateFromPrefabAsync(TechType.WhirlpoolTorpedo, (IOut<GameObject>)taskResult);
                GameObject gameObject = taskResult.Get();
                if (gameObject != null)
                {
                    Pickupable p = gameObject.GetComponent<Pickupable>();
                    if (p != null)
                    {
                        pickupableResult.Set((Pickupable)null);
                        p.Pickup(false);
                        if (smsc.container.AddItem(p) == null)
                            UnityEngine.Object.Destroy(p.gameObject);
                    }
                }
            }
        }

        public static bool Prefix(SeamothStorageContainer __instance, TechType techType)
        {
            __instance.Init();
            //AddDebug("SeamothStorageContainer OnCraftEnd " + techType);
            if (techType == TechType.ExosuitTorpedoArmModule)
                UWE.CoroutineHost.StartCoroutine(OnTorpedoCraftEnd(__instance));

            return false;
        }
    }


}

