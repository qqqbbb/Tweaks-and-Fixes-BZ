
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;
using static Story.ToggleMusicTrackData;

namespace Tweaks_Fixes
{ 
    [HarmonyPatch(typeof(Exosuit))]
    class Exosuit_Patch
    {
        //event:/sub/seamoth/seaglide_light_on
        //{fe76457f-0c94-4245-a080-8a5b2f8853c4}
        //event:/sub/seamoth/seaglide_light_off
        //{b52592a9-19f5-45d1-ad56-7d355fc3dcc3}
        static FMODAsset lightOnSound;
        static FMODAsset lightOffSound;
        private const float vertThrustAcc = 4.25f;
        private const float horThrustAcc = 7.5f;
        private const float thrustUpgradeAcc = 1.33f;
        public static string exosuitName;
        public static string leftArm;
        public static string rightArm;
        public static bool armNamesChanged = false;
        public static bool exosuitStarted = false;
        public static TorpedoType selectedTorpedoLeft;
        public static TorpedoType selectedTorpedoRight;
        public static ItemsContainer torpedoStorageLeft;
        public static ItemsContainer torpedoStorageRight;
        public static float changeTorpedoTimeLeft = 0;
        public static float changeTorpedoTimeRight = 0;
        public static float changeTorpedoInterval = .5f;

        public static List<TorpedoType> GetTorpedos(Exosuit exosuit, ItemsContainer torpedoStorage)
        {
            if (torpedoStorage == null)
                return null;

            List<TorpedoType> torpedos = new List<TorpedoType>();

            for (int index = 0; index < exosuit.torpedoTypes.Length; ++index)
            {
                TechType torpedoType = exosuit.torpedoTypes[index].techType;
                if (torpedoStorage.Contains(torpedoType))
                    torpedos.Add(exosuit.torpedoTypes[index]);
            }
            return torpedos;
        }

        public static bool HasMoreThan1TorpedoType(Exosuit exosuit)
        {
            List<TorpedoType> torpedosLeft = GetTorpedos(exosuit, torpedoStorageLeft);
            List<TorpedoType> torpedosRight = GetTorpedos(exosuit, torpedoStorageRight);

            if (torpedosLeft != null && torpedosLeft.Count > 1)
                return true;
            if (torpedosRight != null && torpedosRight.Count > 1)
                return true;

            return false;
        }

        public static bool HasMoreThan1TorpedoType(Exosuit exosuit, ItemsContainer torpedoStorage)
        {
            List<TorpedoType> torpedos = GetTorpedos(exosuit, torpedoStorage);

            if (torpedos != null && torpedos.Count > 1)
                return true;

            return false;
        }

        public static void ChangeTorpedo(Exosuit exosuit, int slot)
        {
            //AddDebug("ChangeTorpedo " + slot);
            if (torpedoStorageLeft == null && torpedoStorageRight == null)
                return;

            List<TorpedoType> torpedos;
            TorpedoType selectedTorpedo = null;
            if (slot == 0)
                torpedos = GetTorpedos(exosuit, torpedoStorageLeft);
            else
                torpedos = GetTorpedos(exosuit, torpedoStorageRight);

            //AddDebug("ChangeTorpedo torpedos.Count " + torpedos.Count);
            if (torpedos.Count == 0)
            {
                //selectedTorpedo = null;
                return;
            }
            bool found1 = false;
            for (int index = 0; index < torpedos.Count; ++index)
            {
                if (slot == 0)
                {
                    selectedTorpedo = selectedTorpedoLeft;
                    if (selectedTorpedoLeft == null)
                        selectedTorpedoLeft = torpedos[index];
                }
                else if (slot == 1)
                {
                    selectedTorpedo = selectedTorpedoRight;
                    if (selectedTorpedoRight == null)
                        selectedTorpedoRight = torpedos[index];
                }
                if (torpedos[index].techType == selectedTorpedo.techType)
                {
                    if (index + 1 == torpedos.Count)
                    {
                        selectedTorpedo = torpedos[0];
                        //AddDebug("ChangeTorpedo last index " + selectedTorpedo.techType);
                        found1 = true;
                        break;
                    }
                    else if (torpedos.Count > 1)
                    {
                        //AddDebug("ChangeTorpedo " + selectedTorpedo.techType);
                        selectedTorpedo = torpedos[index + 1];
                        //AddDebug("ChangeTorpedo new " + selectedTorpedo.techType);
                        found1 = true;
                        break;
                    }
                    else
                    {
                        selectedTorpedo = torpedos[0];
                        //AddDebug("ChangeTorpedo 1 type " + selectedTorpedo.techType);
                    }
                }
            }
            if (!found1)
            {
                selectedTorpedo = torpedos[0];
                //AddDebug("ChangeTorpedo prev not found " + selectedTorpedo.techType);
            }
            if (slot == 0)
            {
                selectedTorpedoLeft = selectedTorpedo;
                leftArm = GetTorpedoName(exosuit, 0, true);
                armNamesChanged = true;
            }
            else if (slot == 1)
            {
                selectedTorpedoRight = selectedTorpedo;
                rightArm = GetTorpedoName(exosuit, 1, true);
                armNamesChanged = true;
            }
        }

        static string GetTorpedoName(Exosuit exosuit, int slot, bool next = false)
        { // runs before UI_Patches.GetStrings when game loads
            //AddDebug("GetTorpedoName " + slot + " " + next);
            if (slot == 0 && torpedoStorageLeft == null)
                return "";

            if (slot == 1 && torpedoStorageRight == null)
                return "";

            ItemsContainer torpedoStorage;
            TorpedoType selectedTorpedo;
            if (slot == 0)
            {
                torpedoStorage = torpedoStorageLeft;
                selectedTorpedo = selectedTorpedoLeft;
            }
            else
            {
                torpedoStorage = torpedoStorageRight;
                selectedTorpedo = selectedTorpedoRight;
            }
            //if (torpedoStorage == null)
            //    AddDebug("GetTorpedoName torpedoStorage == null");
            List <TorpedoType> torpedoTypes = new List <TorpedoType>(exosuit.torpedoTypes);
            List <TorpedoType> torpedos = GetTorpedos(exosuit, torpedoStorage);
            if (!next)
            {
                if (selectedTorpedo != null)
                {
                    //AddDebug("GetTorpedoName selectedTorpedo " + selectedTorpedo);
                    torpedoTypes.Add(selectedTorpedo);
                }
                for (int index = torpedoTypes.Count - 1; index >= 0; --index)
                {
                    TechType torpedoType = torpedoTypes[index].techType;
                    //AddDebug(torpedoType + " " + container.GetCount(torpedoType));
                    if (torpedoStorage.Contains(torpedoType))
                    {
                        if (slot == 0 && selectedTorpedoLeft == null)
                            selectedTorpedoLeft = torpedoTypes[index];
                        else if (slot == 1 && selectedTorpedoRight == null)
                            selectedTorpedoRight = torpedoTypes[index];

                        //AddDebug("GetTorpedoName return " + torpedoType);
                        return Language.main.Get(torpedoType) + " x" + torpedoStorage.GetCount(torpedoType);
                    }
                }
            }
            else
            {
                //AddDebug("GetTorpedoName selectedTorpedo " + selectedTorpedo.techType);
                for (int index = 0; index < torpedoTypes.Count; ++index)
                {
                    TechType torpedoType = torpedoTypes[index].techType;
                    //AddDebug(torpedoType + " " + container.GetCount(torpedoType));
                    if (selectedTorpedo.techType == torpedoType)
                    {
                        if (torpedoStorage.Contains(torpedoType))
                            return Language.main.Get(torpedoType) + " x" + torpedoStorage.GetCount(torpedoType);
                    }
                }
            }
            //exitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " " + Main.config.translatableStrings[2] + " (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
            if (torpedos != null || torpedos.Count == 0)
                return "";

            string name = Language.main.Get(TechType.ExosuitTorpedoArmModule);
            name = name.Replace(exosuitName, "");
            name = name.TrimStart();
            name = name[0].ToString().ToUpper() + name.Substring(1);
            return name;
        }

        public static void GetArmNames(Exosuit exosuit)
        {
            //AddDebug("GetArmNames Left Arm " + exosuit.currentLeftArmType);
            //AddDebug("GetArmNames Right Arm " + exosuit.currentRightArmType);
            if (exosuit.currentLeftArmType == TechType.ExosuitTorpedoArmModule)
            {
                //AddDebug("GetArmNames left torpedo" );
                torpedoStorageLeft = exosuit.GetStorageInSlot(0, TechType.ExosuitTorpedoArmModule);
                //if (torpedoStorageLeft == null)
                //    AddDebug("GetArmNames torpedoStorageLeft null");
                leftArm = GetTorpedoName(exosuit, 0);
                //if (torpedoStorageLeft == null)
                //    AddDebug("GetArmNames torpedoStorageLeft null");
            }
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
            {
                //AddDebug("GetArmNames right torpedo");
                torpedoStorageRight = exosuit.GetStorageInSlot(1, TechType.ExosuitTorpedoArmModule);
                //if (torpedoStorageRight == null)
                //    AddDebug("GetArmNames torpedoStorageRight null");
                rightArm = GetTorpedoName(exosuit, 1);
            }
            else
            {
                rightArm = Language.main.Get(exosuit.currentRightArmType);
                rightArm = rightArm.Replace(exosuitName, "");
                rightArm = rightArm.TrimStart();
                rightArm = rightArm[0].ToString().ToUpper() + rightArm.Substring(1);
                //AddDebug("GetArmNames rightArm " + rightArm);
            }
        }

        public static void VehicleFixedUpdate(Vehicle vehicle)
        {
            bool pilotingMode = vehicle.GetPilotingMode();
            if (pilotingMode != vehicle.lastPilotingState)
            {
                //AddDebug("GetPilotingMode " + vehicle.GetPilotingMode());
                if (pilotingMode)
                    vehicle.OnPilotModeBegin();
                else
                    vehicle.OnPilotModeEnd();

                vehicle.lastPilotingState = pilotingMode;
            }
            //if (!__instance.IsPowered())
            //    return false;
            if (vehicle.CanPilot())
            {
                //AddDebug("vehicle.CanPilot " + vehicle.energyInterface.hasCharge);
                if (pilotingMode)
                    vehicle.ApplyPhysicsMove();
                if (vehicle.stabilizeRoll)
                    vehicle.StabilizeRoll();
            }
            vehicle.prevVelocity = vehicle.useRigidbody.velocity;
        }

        public static void VehicleUpdate(Vehicle vehicle)
        {
            if (vehicle.CanPilot())
            {
                vehicle.steeringWheelYaw = Mathf.Lerp(vehicle.steeringWheelYaw, 0f, Time.deltaTime);
                vehicle.steeringWheelPitch = Mathf.Lerp(vehicle.steeringWheelPitch, 0f, Time.deltaTime);
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
                    if (vector2.x != 0f)
                        vehicle.useRigidbody.AddTorque(vehicle.transform.up * vector2.x * vehicle.sidewaysTorque, ForceMode.VelocityChange);
                }
                else if (vehicle.controlSheme == Vehicle.ControlSheme.Hoverbike)
                    vehicle.useRigidbody.AddRelativeTorque(new Vector3(vector2.y, 0f, 0f));
            }
            bool powered = vehicle.IsPowered();
            if (vehicle.wasPowered != powered)
            {
                vehicle.wasPowered = powered;
                vehicle.OnPoweredChanged(powered);
            }
            vehicle.ReplenishOxygen();
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void StartPostfix(Exosuit __instance)
        {
            //AddDebug("Exosuit Start " + __instance.onGroundForceMultiplier);
           
            //exosuitName = Language.main.Get("Exosuit");
            //rightButton = uGUI.FormatButton(GameInput.Button.RightHand);
            //leftButton = uGUI.FormatButton(GameInput.Button.LeftHand);
            if (Player.main.currentMountedVehicle && Player.main.currentMountedVehicle == __instance)
            {
                GetArmNames(__instance);
                armNamesChanged = true;
            }
            lightOnSound = ScriptableObject.CreateInstance<FMODAsset>();
            lightOnSound.path = "event:/sub/seamoth/seaglide_light_on";
            lightOnSound.id = "{fe76457f-0c94-4245-a080-8a5b2f8853c4}";
            lightOffSound = ScriptableObject.CreateInstance<FMODAsset>();
            lightOffSound.path = "event:/sub/seamoth/seaglide_light_off";
            lightOffSound.id = "{b52592a9-19f5-45d1-ad56-7d355fc3dcc3}";

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
            SetLights(__instance, Main.config.exosuitLights);
            exosuitStarted = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("ApplyJumpForce")]
        static bool ApplyJumpForcePrefix(Exosuit __instance)
        {
            if (Main.config.exosuitMoveTweaks || __instance.timeLastJumped + 1f > Time.time)
                return false;

            //AddDebug("ApplyJumpForce");
            bool underwater = __instance.IsUnderwater();

            if (__instance.onGround)
            {
                Utils.PlayFMODAsset(__instance.jumpSound, __instance.transform);
                if (underwater)
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
            float jumpForce = 5f;
            if (__instance.jumpJetsUpgraded)
                jumpForce = 7f;

            __instance.useRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            __instance.timeLastJumped = Time.time;
            __instance.timeOnGround = 0f;
            __instance.onGround = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnLand")]
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

        // thrusters consumes 2x energy
        // no limit on thrusters
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(Exosuit __instance)
        {
            //Vehicle vehicle = __instance as Vehicle;
            //vehicle.Update();
            if (!Main.config.exosuitMoveTweaks)
                return true;

            //AddDebug("thrustConsumption " + __instance.thrustConsumption);
            //AddDebug("verticalJetConsumption " + __instance.verticalJetConsumption);
            //AddDebug("horizontalJetConsumption " + __instance.horizontalJetConsumption);

            VehicleUpdate(__instance);

            __instance.openedFraction = !__instance.storageContainer.GetOpen() ? Mathf.Clamp01(__instance.openedFraction - Time.deltaTime * 2f) : Mathf.Clamp01(__instance.openedFraction + Time.deltaTime * 2f);
            __instance.storageFlap.localEulerAngles = new Vector3(__instance.startFlapPitch + __instance.openedFraction * 80f, 0f, 0f);
            bool pilotingMode = __instance.GetPilotingMode();
            bool piloting = __instance.GetPilotingMode() && !__instance.docked;
            if (pilotingMode)
            {
                Player.main.transform.localPosition = Vector3.zero;
                Player.main.transform.localRotation = Quaternion.identity;
                Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                __instance.lastMoveDirection = moveDirection;
                bool movingUp = moveDirection.y > 0f;
                bool sprinting = GameInput.GetButtonHeld(GameInput.Button.Sprint);
                bool powered = __instance.IsPowered() && __instance.liveMixin.IsAlive();
                __instance.GetEnergyValues(out float charge, out float capacity); // my
                float powerMult = 1f; // my
                if (movingUp && sprinting)
                    powerMult = 3f;
                else if (sprinting || movingUp)
                    powerMult = 2f;

                if (__instance.jumpJetsUpgraded)
                    powerMult *= 1.33f;

                if ((movingUp | sprinting) & powered && __instance.IsUnderwater())
                {
                    __instance.thrustPower = Util.NormalizeTo01range(charge, 0f, capacity);// my
                    //__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower - Time.deltaTime * __instance.thrustConsumption * verticalJetConsumption);
                    if (movingUp && (__instance.onGround || Time.time - __instance.timeOnGround <= 1f) && !__instance.jetDownLastFrame)
                        __instance.ApplyJumpForce();

                    __instance.jetsActive = true;
                    __instance.horizontalJetsActive = sprinting;
                    __instance.verticalJetsActive = movingUp;
                }
                else
                {
                    __instance.jetsActive = false;
                    __instance.horizontalJetsActive = false;
                    __instance.verticalJetsActive = false;
                    __instance.thrustPower = Util.NormalizeTo01range(charge, 0f, capacity);// my
                    //__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower + Time.deltaTime * __instance.thrustConsumption * num2);
                }
                __instance.jetDownLastFrame = movingUp;
                __instance.footStepSounds.soundsEnabled = !__instance.powersliding;
                __instance.movementEnabled = !__instance.powersliding;
                __instance.powersliding = __instance.IsHorizontalBoostActive() && __instance.IsUnderwater() && __instance.onGround; // my

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
                //if (__instance.powersliding)
                //    __instance.loopingSlideSound.Play();
                //else
                //    __instance.loopingSlideSound.Stop();

                if ((movingUp || moveDirection.x != 0f ? 1 : (moveDirection.z != 0f ? 1 : 0)) != 0)
                {
                    //AddDebug("powerMult " + powerMult);
                    __instance.ConsumeEngineEnergy(0.08333334f * Time.deltaTime * powerMult);
                }
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

                    if (piloting)
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
                //if (__instance.powersliding && __instance.onGround && __instance.timeLastSlideEffect + 0.5f < Time.time)
                //{
                //    if (__instance.IsUnderwater())
                //        __instance.fxcontrol.Play(4);
                //    else
                //        __instance.fxcontrol.Play(3);
                //    __instance.timeLastSlideEffect = Time.time;
                //}
            }
            if (!piloting)
            {
                bool flag2 = false;
                bool flag3 = false;
                if (!Util.Approximately(__instance.aimTargetLeft.transform.localPosition.y, 0f))
                    __instance.aimTargetLeft.transform.localPosition = new Vector3(__instance.aimTargetLeft.transform.localPosition.x, Mathf.MoveTowards(__instance.aimTargetLeft.transform.localPosition.y, 0f, Time.deltaTime * 50f), __instance.aimTargetLeft.transform.localPosition.z);
                else
                    flag2 = true;

                if (!Util.Approximately(__instance.aimTargetRight.transform.localPosition.y, 0f))
                    __instance.aimTargetRight.transform.localPosition = new Vector3(__instance.aimTargetRight.transform.localPosition.x, Mathf.MoveTowards(__instance.aimTargetRight.transform.localPosition.y, 0f, Time.deltaTime * 50f), __instance.aimTargetRight.transform.localPosition.z);
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

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(Exosuit __instance)
        {
            //if (__instance.powersliding)
            //    AddDebug("powersliding " + __instance.powersliding);
            //if (__instance.horizontalJetsActive)
            //    AddDebug("horizontalJetsActive " + __instance.horizontalJetsActive);
            //if (__instance.thrustPower < 1F)
            //    AddDebug("thrustPower " + __instance.thrustPower.ToString("0.0"));

            if (!IngameMenu.main.isActiveAndEnabled && !Player.main.pda.isInUse && Player.main.inExosuit && Player.main.currentMountedVehicle == __instance && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
            {
                ToggleLights(__instance);
            }
        }

        private static void SetLights(Exosuit exosuit, bool active)
        {
            if (active && !exosuit.energyInterface.hasCharge)
                return;

            Transform lightsT = exosuit.transform.Find("lights_parent");
            if (lightsT)
            {
                lightsT.gameObject.SetActive(active);
                //AddDebug("SetLights " + active);
            }
        }

        private static void ToggleLights(Exosuit exosuit)
        {
            Transform lightsT = exosuit.transform.Find("lights_parent");
            if (lightsT)
            {
                //AddDebug("IngameMenu isActiveAndEnabled " + IngameMenu.main.isActiveAndEnabled);
                if (!lightsT.gameObject.activeSelf && exosuit.energyInterface.hasCharge)
                {
                    lightsT.gameObject.SetActive(true);
                    Main.config.exosuitLights = true;
                    Utils.PlayFMODAsset(lightOnSound, Player.main.transform);
                }
                else if (lightsT.gameObject.activeSelf)
                {
                    lightsT.gameObject.SetActive(false);
                    Main.config.exosuitLights = false;
                    Utils.PlayFMODAsset(lightOffSound, Player.main.transform);
                }
                //AddDebug("lights " + lightsT.gameObject.activeSelf);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("FixedUpdate")]
        public static bool FixedUpdatePostfix(Exosuit __instance)
        { // reduce vert thrust speed. jumpJetsUpgrade affects vert and hor speed the same way. powersliding
            if (!Main.config.exosuitMoveTweaks)
                return true;

            VehicleFixedUpdate(__instance);

            //AddDebug("FixedUpdate");
            bool isGrappling = __instance.GetIsGrappling();
            __instance.worldForces.handleGravity = !__instance.onGround;
            bool isUnderwater = __instance.IsUnderwater();
            if (isUnderwater && __instance.thrustPower > 0f && __instance.jetsActive)
            {
                float thrustPower = 0.8f + __instance.thrustPower * 0.2f;
                if (__instance.jumpJetsUpgraded)
                    thrustPower *= thrustUpgradeAcc;   // my

                if (!__instance.onGround)
                    __instance.worldForces.handleGravity = false;

                if (__instance.horizontalJetsActive)
                {
                    Vector3 direction = new Vector3(__instance.lastMoveDirection.x, 0f, __instance.lastMoveDirection.z);
                    direction = direction.sqrMagnitude > 0f ? __instance.transform.TransformDirection(direction).normalized : __instance.transform.forward;
                    if (!__instance.verticalJetsActive)
                    {
                        Vector3 forward = MainCamera.camera.transform.forward;
                        direction += Vector3.up * Mathf.Clamp(forward.y, -0.75f, 0.75f);
                    }
                    Vector3 acceleration = horThrustAcc * direction * thrustPower;
                    acceleration *= Main.config.exosuitSpeedMult;
                    if (__instance.powersliding)
                        acceleration *= 4f; // my

                    __instance.useRigidbody.AddForce(acceleration, ForceMode.Acceleration);
                }
                if (__instance.verticalJetsActive && isUnderwater)
                {
                    //AddDebug("verticalJetsActive");
                    Vector3 acceleration = Vector3.up * vertThrustAcc * thrustPower;
                    acceleration *= Main.config.exosuitSpeedMult;  // my
                    __instance.useRigidbody.AddForce(acceleration, ForceMode.Acceleration);
                }
            }
            float drag = 1f;
            if (__instance.onGround && !isGrappling)
                drag = 3f;
            else if (!isUnderwater)
                drag = 0.1f;

            __instance.useRigidbody.drag = drag;
            UWE.Utils.SetIsKinematicAndUpdateInterpolation(__instance.useRigidbody, __instance.ShouldSetKinematic());
            if (__instance.constructionFallOverride && __instance.transform.position.y < 0f)
                __instance.constructionFallOverride = false;
            if (isGrappling)
                __instance.useRigidbody.AddForce(Vector3.down * 3f, ForceMode.Acceleration);

            if (__instance.cinematicMode || !__instance.rotationDirty)
                return false;

            Vector3 localEulerAngles = __instance.transform.localEulerAngles;
            Quaternion quaternion = Quaternion.Euler(0f, localEulerAngles.y, 0f);
            if (Mathf.Abs(localEulerAngles.x) < 1f / 1000f && Mathf.Abs(localEulerAngles.z) < 1f / 1000f)
                __instance.rotationDirty = false;
            else
                quaternion = Quaternion.Lerp(__instance.transform.localRotation, quaternion, Time.fixedDeltaTime * 3f);
            if (__instance.transform.parent != null)
                quaternion = __instance.transform.parent.rotation * quaternion;
            __instance.useRigidbody.MoveRotation(quaternion);

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SlotKeyDown")]
        public static void SlotKeyDownPostfix(Exosuit __instance, int slotID)
        {
            //AddDebug("SlotKeyDown " + slotID);
            //ItemsContainer container = null;
            int torpedoSlot = -1;
            float changeTorpedoTime = 0f;
            //AddDebug("HasMoreThan1TorpedoType  Left " + HasMoreThan1TorpedoType(__instance, torpedoStorageLeft));
            //AddDebug("HasMoreThan1TorpedoType Right " + HasMoreThan1TorpedoType(__instance, torpedoStorageRight));
            //if (selectedTorpedoLeft == null)
            //    AddDebug("SlotKeyDown selectedTorpedoLeft == null ");

            if (slotID == 2 && torpedoStorageLeft != null && HasMoreThan1TorpedoType(__instance, torpedoStorageLeft))
            {
                //AddDebug("SlotKeyDown left torpedo " + selectedTorpedoLeft.techType);
                //container = torpedoStorageLeft;
                torpedoSlot = 0;
                //AddDebug("ToggleSlot torpedoStorageLeft");
                changeTorpedoTime = changeTorpedoTimeLeft;
            }
            if (slotID == 3 && torpedoStorageRight != null && HasMoreThan1TorpedoType(__instance, torpedoStorageRight))
            {
                //AddDebug("SlotKeyDown Right torpedo " + selectedTorpedoRight.techType);
                //container = torpedoStorageRight;
                torpedoSlot = 1;
                //AddDebug("ToggleSlot torpedoStorageRight");
                changeTorpedoTime = changeTorpedoTimeRight;
            }
            if (torpedoSlot == -1)
                return;

            if (Time.time - changeTorpedoTime > changeTorpedoInterval)
            {
                //Main.Log("changeTorpedoTime " + changeTorpedoTime);
                ChangeTorpedo(__instance, torpedoSlot);
                if (torpedoSlot == 0)
                    changeTorpedoTimeLeft = Time.time;
                else if (torpedoSlot == 1)
                    changeTorpedoTimeRight = Time.time;

                return;
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("SlotNext")]
        public static void SlotNextPostfix(Exosuit __instance)
        {
            AddDebug("SlotNext " );
        }


        [HarmonyPrefix]
        [HarmonyPatch("UpdateUIText")]
        public static bool UpdateUITextPrefix(Exosuit __instance, bool hasPropCannon)
        {
            //AddDebug("UpdateUIText  " );
            if (armNamesChanged || !__instance.hasInitStrings || __instance.lastHasPropCannon != hasPropCannon)
            {
                bool leftTorpedo = HasMoreThan1TorpedoType(__instance, torpedoStorageLeft);
                bool rightTorpedo = HasMoreThan1TorpedoType(__instance, torpedoStorageRight);
                bool lightsText = false;
                string buttonFormat1 = LanguageCache.GetButtonFormat("ExosuitBoost", GameInput.Button.Sprint);
                string buttonFormat2 = LanguageCache.GetButtonFormat("ExosuitJump", GameInput.Button.MoveUp);
                string buttonFormat3 = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
                List<TorpedoType> torpedosLeft = GetTorpedos(__instance, torpedoStorageLeft);
                List<TorpedoType> torpedosRight = GetTorpedos(__instance, torpedoStorageRight);
                if (hasPropCannon && torpedosRight != null && torpedosRight.Count > 0 || hasPropCannon && torpedosLeft != null && torpedosLeft.Count > 0 || (torpedosRight != null && torpedosRight.Count == 0 && torpedosLeft != null && torpedosLeft.Count == 0))
                {
                    buttonFormat3 += UI_Patches.exosuitLightsButton;
                    lightsText = true;
                }
                //AddDebug("UpdateUIText lastDevice " + GameInput.lastDevice);
                //AddDebug("UpdateUIText leftTorpedo " + leftTorpedo);
                //AddDebug("UpdateUIText rightTorpedo " + rightTorpedo);
                //string lightsButton = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.Deconstruct);
                __instance.sb.Length = 0;
                __instance.sb.AppendLine(Language.main.GetFormat<string, string, string>("ExosuitBoostJumpExitFormat", buttonFormat1, buttonFormat2, buttonFormat3));

                if (!string.IsNullOrEmpty(leftArm))
                {
                    __instance.sb.Append(leftArm);
                    __instance.sb.Append(" ");
                    __instance.sb.Append(UI_Patches.leftHandButton);
                }
                if (!string.IsNullOrEmpty(rightArm))
                {
                    if (!string.IsNullOrEmpty(leftArm))
                        __instance.sb.Append(", ");

                    __instance.sb.Append(rightArm);
                    __instance.sb.Append(" ");
                    __instance.sb.Append(UI_Patches.rightHandButton);
                    //__instance.sb.Append(",");
                }
                if (!lightsText)
                    __instance.sb.Append(UI_Patches.exosuitLightsButton);

                if (GameInput.lastDevice == GameInput.Device.Keyboard)
                {
                    //AddDebug("UpdateUIText leftTorpedo HasMoreThan1TorpedoType " + leftTorpedo);
                    //AddDebug("UpdateUIText rightTorpedo HasMoreThan1TorpedoType " + rightTorpedo);
                    if (leftTorpedo && rightTorpedo)
                    {
                        __instance.sb.Append(Language.main.Get("TF_change_torpedo"));
                        __instance.sb.Append(UI_Patches.slot1Plus2Button);
                        __instance.sb.Append(UI_Patches.changeTorpedoExosuitButtonKeyboard);
                    }
                    else if (leftTorpedo)
                    {
                        __instance.sb.Append(Language.main.Get("TF_change_torpedo"));
                        __instance.sb.Append(UI_Patches.slot1Button);
                        __instance.sb.Append(UI_Patches.changeTorpedoExosuitButtonKeyboard);
                    }
                    else if (rightTorpedo)
                    {
                        __instance.sb.Append(Language.main.Get("TF_change_torpedo"));
                        __instance.sb.Append(UI_Patches.slot2Button);
                        __instance.sb.Append(UI_Patches.changeTorpedoExosuitButtonKeyboard);
                    }
                }
                if (hasPropCannon)
                {
                    __instance.sb.Append(", ");
                    __instance.sb.AppendLine(LanguageCache.GetButtonFormat("PropulsionCannonToRelease", GameInput.Button.AltTool));
                }
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

        [HarmonyPostfix]
        [HarmonyPatch("OnPilotModeBegin")]
        public static void OnPilotModeBeginPostfix(Exosuit __instance)
        {
            if (ConfigToEdit.disableGravityForExosuit.Value)
            {
                Util.FreezeObject(__instance.gameObject, false);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("EnterVehicle")]
        public static void EnterVehiclePostfix(Exosuit __instance)
        { // runs before Exosuit.Start
          //AddDebug("EnterVehicle");
            if (Main.config.exosuitMoveTweaks)
                __instance.onGroundForceMultiplier = 2f;
            else
                __instance.onGroundForceMultiplier = 4f;

            if (exosuitStarted)
            {
                GetArmNames(__instance);
                armNamesChanged = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnPilotModeEnd")]
        public static void OnPilotModeEndPostfix(Exosuit __instance)
        {
            //AddDebug("OnPilotModeEnd");
            if (ConfigToEdit.disableGravityForExosuit.Value)
            {
                Util.FreezeObject(__instance.gameObject, true);
            }
            selectedTorpedoLeft = null;
            selectedTorpedoRight = null;
            //torpedoStorageLeft = null;
            //torpedoStorageRight = null;
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
            if (col.contacts.Length == 0)
                return false;

            Exosuit exosuit = __instance.GetComponent<Exosuit>();
            Rigidbody rb = UWE.Utils.GetRootRigidbody(col.gameObject);
            if (exosuit && !rb)
                return false;// no sounds when walking on ground

            float magnitude = col.relativeVelocity.magnitude;
            //FMODAsset asset = !rootRigidbody || rootRigidbody.mass >= 10.0 ? (magnitude <= 8.0 ? (magnitude <= 4.0 ? __instance.hitSoundSlow : __instance.hitSoundMedium) : __instance.hitSoundFast) : __instance.hitSoundSmall;
            FMODAsset asset = null;
            if (!rb || rb.mass >= 10f)
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

    [HarmonyPatch(typeof(Vehicle))]
    class Vehicle_patch
    {
        public static void ApplyPhysicsMoveVanilla(Vehicle vehicle)
        { // Main.config.vehicleSpeedMult
            if (vehicle.worldForces.IsAboveWater() != vehicle.wasAboveWater)
            {
                vehicle.PlaySplashSound();
                vehicle.wasAboveWater = vehicle.worldForces.IsAboveWater();
            }
            if (!(vehicle.moveOnLand | (vehicle.transform.position.y < Ocean.GetOceanLevel() && vehicle.transform.position.y < vehicle.worldForces.waterDepth && !vehicle.forceWalkMotorMode)) || !vehicle.movementEnabled)
                return;

            if (vehicle.controlSheme == Vehicle.ControlSheme.Submersible)
            {
                Vector3 vector3_1 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                vector3_1.Normalize();
                double num1 = Mathf.Abs(vector3_1.x) * vehicle.sidewardForce;
                float num2 = Mathf.Max(0.0f, vector3_1.z) * vehicle.forwardForce;
                float num3 = Mathf.Max(0.0f, -vector3_1.z) * vehicle.backwardForce;
                float num4 = Mathf.Abs(vector3_1.y * vehicle.verticalForce);
                float denominator = vector3_1.z >= 0.0 ? vehicle.forwardForce : vehicle.backwardForce;
                double num5 = num2;
                Vector3 vector3_2 = ((float)(num1 + num5) + num3 + num4) * vector3_1;
                Vector3 vector3_3 = new Vector3(UWE.Utils.SafeDiv(vector3_2.x, vehicle.sidewardForce), UWE.Utils.SafeDiv(vector3_2.y, vehicle.verticalForce), UWE.Utils.SafeDiv(vector3_2.z, denominator));
                vector3_3.Normalize();
                Vector3 acceleration = vehicle.transform.rotation * new Vector3(vector3_3.x * vehicle.sidewardForce, vector3_3.y * vehicle.verticalForce, vector3_3.z * denominator) * Time.deltaTime;
                for (int index = 0; index < vehicle.accelerationModifiers.Length; ++index)
                    vehicle.accelerationModifiers[index].ModifyAcceleration(ref acceleration);
                vehicle.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
            }
            else
            {
                if (vehicle.controlSheme != Vehicle.ControlSheme.Submarine && vehicle.controlSheme != Vehicle.ControlSheme.Mech)
                    return;
                Vector3 vector3_1 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                Vector3 vector3_2 = new Vector3(vector3_1.x, 0.0f, vector3_1.z);
                float num = (float)(Mathf.Abs(vector3_2.x) * vehicle.sidewardForce + Mathf.Max(0.0f, vector3_2.z) * vehicle.forwardForce + Mathf.Max(0.0f, -vector3_2.z) * vehicle.backwardForce);
                Vector3 vector3_3 = vehicle.transform.rotation * vector3_2;
                vector3_3.y = 0.0f;
                Vector3 vector = Vector3.Normalize(vector3_3);
                if (vehicle.onGround)
                {
                    vector = Vector3.ProjectOnPlane(vector, vehicle.surfaceNormal);
                    vector.y = Mathf.Clamp(vector.y, -0.5f, 0.5f);
                    num *= vehicle.onGroundForceMultiplier;
                }
                Vector3 vector3_4 = new Vector3(0.0f, vector3_1.y, 0f);
                vector3_4.y *= vehicle.verticalForce * Time.deltaTime;
                Vector3 acceleration = num * vector * Time.deltaTime + vector3_4;
                acceleration *= Main.config.exosuitSpeedMult;
                vehicle.OverrideAcceleration(ref acceleration);
                for (int index = 0; index < vehicle.accelerationModifiers.Length; ++index)
                    vehicle.accelerationModifiers[index].ModifyAcceleration(ref acceleration);
                vehicle.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("TorpedoShot")]
        public static bool TorpedoShotPrefix(Vehicle __instance, ItemsContainer container, ref TorpedoType torpedoType, Transform muzzle, ref bool __result)
        { // __instance is null !
            //if (__instance == null)
            //    AddDebug("TorpedoShotPrefix  Vehicle is null  ");

            if (container == Exosuit_Patch.torpedoStorageLeft)
                torpedoType = Exosuit_Patch.selectedTorpedoLeft;
            else if (container == Exosuit_Patch.torpedoStorageRight)
                torpedoType = Exosuit_Patch.selectedTorpedoRight;

            //AddDebug("TorpedoShot " + torpedoType.techType);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("TorpedoShot")]
        public static void TorpedoShotPostfix(Vehicle __instance, ItemsContainer container, ref TorpedoType torpedoType, Transform muzzle)
        { // __instance is null !
            //if (SeaMoth_patch.selectedTorpedo == null)
            //    AddDebug("TorpedoShot __instance == null");
            __instance = Player.main.currentMountedVehicle;

            if (__instance is Exosuit)
            {
                if (Exosuit_Patch.selectedTorpedoLeft != null && !Exosuit_Patch.torpedoStorageLeft.Contains(Exosuit_Patch.selectedTorpedoLeft.techType))
                    Exosuit_Patch.ChangeTorpedo(__instance as Exosuit, 0);
                else if (Exosuit_Patch.selectedTorpedoRight != null && !Exosuit_Patch.torpedoStorageRight.Contains(Exosuit_Patch.selectedTorpedoRight.techType))
                    Exosuit_Patch.ChangeTorpedo(__instance as Exosuit, 1);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ApplyPhysicsMove")] // disable strafing
        public static bool ApplyPhysicsMovePrefix(Vehicle __instance)
        {
            if (!__instance.GetPilotingMode())
                return false;

            if (!Main.config.exosuitMoveTweaks)
            {
                ApplyPhysicsMoveVanilla(__instance);
                return false;
            }
            //AddDebug("controlSheme " + __instance.controlSheme);
            //AddDebug("onGroundForceMultiplier " + __instance.onGroundForceMultiplier);
            if (__instance.worldForces.IsAboveWater() != __instance.wasAboveWater)
            {
                __instance.PlaySplashSound();
                __instance.wasAboveWater = __instance.worldForces.IsAboveWater();
            }
            if (!(__instance.moveOnLand | (__instance.transform.position.y < Ocean.GetOceanLevel() && __instance.transform.position.y < __instance.worldForces.waterDepth && !__instance.forceWalkMotorMode)) || !__instance.movementEnabled)
                return false;

            if (__instance.controlSheme == Vehicle.ControlSheme.Submersible)
            {
                Vector3 vector3_1 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                vector3_1.Normalize();
                double num1 = Mathf.Abs(vector3_1.x) * __instance.sidewardForce;
                float num2 = Mathf.Max(0.0f, vector3_1.z) * __instance.forwardForce;
                float num3 = Mathf.Max(0.0f, -vector3_1.z) * __instance.backwardForce;
                float num4 = Mathf.Abs(vector3_1.y * __instance.verticalForce);
                float denominator = vector3_1.z >= 0.0 ? __instance.forwardForce : __instance.backwardForce;
                double num5 = num2;
                Vector3 vector3_2 = ((float)(num1 + num5) + num3 + num4) * vector3_1;
                Vector3 vector3_3 = new Vector3(UWE.Utils.SafeDiv(vector3_2.x, __instance.sidewardForce), UWE.Utils.SafeDiv(vector3_2.y, __instance.verticalForce), UWE.Utils.SafeDiv(vector3_2.z, denominator));
                vector3_3.Normalize();
                Vector3 acceleration = __instance.transform.rotation * new Vector3(vector3_3.x * __instance.sidewardForce, vector3_3.y * __instance.verticalForce, vector3_3.z * denominator) * Time.deltaTime;
                for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                    __instance.accelerationModifiers[index].ModifyAcceleration(ref acceleration);
                __instance.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
            }
            else
            {
                if (__instance.controlSheme != Vehicle.ControlSheme.Submarine && __instance.controlSheme != Vehicle.ControlSheme.Mech)
                    return false;

                Vector3 direction = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                Vector3 horDirection = new Vector3(0f, 0f, direction.z); // MY
                float num = (float)(Mathf.Abs(horDirection.x) * __instance.sidewardForce + Mathf.Max(0f, horDirection.z) * __instance.forwardForce + Mathf.Max(0f, -horDirection.z) * __instance.backwardForce);
                Vector3 vector3_3 = __instance.transform.rotation * horDirection;
                vector3_3.y = 0f;
                Vector3 vector = Vector3.Normalize(vector3_3);
                if (__instance.onGround)
                {
                    vector = Vector3.ProjectOnPlane(vector, __instance.surfaceNormal);
                    vector.y = Mathf.Clamp(vector.y, -0.5f, 0.5f);
                    num *= __instance.onGroundForceMultiplier;
                }
                Vector3 vertDirection = new Vector3(0f, direction.y, 0f);
                vertDirection.y *= __instance.verticalForce * Time.deltaTime;
                Vector3 acceleration = num * vector * Time.deltaTime + vertDirection;
                if (__instance.wasAboveWater)
                    acceleration *= 1.33f;

                acceleration *= Main.config.exosuitSpeedMult;
                __instance.OverrideAcceleration(ref acceleration);
                for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                    __instance.accelerationModifiers[index].ModifyAcceleration(ref acceleration);
                __instance.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetTemperature")]
        public static bool GetTemperaturePrefix(Vehicle __instance, ref float __result)
        { // fix thermometer values wnen above water
            if (Player.main.inExosuit && Player.main.currentMountedVehicle == __instance)
            {
                //BodyTemperature bt = Player.main.GetComponent<BodyTemperature>();
                __result = Player_Patches.ambientTemperature;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnProtoDeserialize")]
        static void OnProtoDeserializePostfix(Vehicle __instance)
        {
            //AddDebug("Vehicle OnProtoDeserialize ");
            if (ConfigToEdit.disableGravityForExosuit.Value && __instance is Exosuit && Player.main.currentMountedVehicle != __instance)
            {
                Util.FreezeObject(__instance.gameObject, true);
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("ToggleSlot", new Type[] { typeof(int), typeof(bool) })]
        public static bool ToggleSlotPrefix(int slotID, bool state, Vehicle __instance)
        {
            AddDebug("  ToggleSlot  " + slotID + " " + state);
            return true;
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("EnterVehicle")]
        public static void EnterVehiclePostfix(Vehicle __instance)
        {
            Exosuit_Patch.selectedTorpedoLeft = null;
            Exosuit_Patch.selectedTorpedoRight = null;
        }
    }

    [HarmonyPatch(typeof(ExosuitDrillArm))]
    class ExosuitDrillArm_Patch
    { // dont stop drilling sound when not hitting anything
        [HarmonyPrefix]
        [HarmonyPatch("StopEffects")]
        static bool StopEffectsPrefix(ExosuitDrillArm __instance)
        {
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
        [HarmonyPostfix]
        [HarmonyPatch("IExosuitArm.OnUseUp")]
        static void OnUseUpPostfix(ExosuitDrillArm __instance)
        {
            //AddDebug("OnUseUp ");
            __instance.loop.Stop();
        }
        //[HarmonyPrefix]
        //[HarmonyPatch("IExosuitArm.OnUseDown")]
        static bool OnUseDownPostfix(ExosuitDrillArm __instance, ref bool __result, ref float cooldownDuration)
        {
            AddDebug("OnUseDown ");
            //__instance.animator.SetBool("use_tool", true);
            __instance.drilling = true;
            __instance.loop.Play();
            cooldownDuration = 0f;
            __instance.drillTarget = null;
            __result = true;
            return false;
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
                        pickupableResult.Set(null);
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



    [HarmonyPatch(typeof(ExosuitClawArm))]
    class ExosuitClawArm_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("IExosuitArm.GetInteractableRoot")]
        static void GetInteractableRootPostfix(ExosuitClawArm __instance, GameObject target, ref GameObject __result)
        {
            //AddDebug("ExosuitClawArm GetInteractableRoot Postfix target " + target.name);
            if (__result == null && target.GetComponent<SupplyCrate>())
                __result = target.gameObject;
        }
        [HarmonyPrefix]
        [HarmonyPatch("TryUse", new Type[] { typeof(float) }, new[] { ArgumentType.Out })]
        static bool TryUsePrefix(ExosuitClawArm __instance, ref float cooldownDuration, ref bool __result)
        { // open supply crates
            if (Time.time - __instance.timeUsed >= __instance.cooldownTime)
            {
                Pickupable pickupable = null;
                PickPrefab pickPrefab = null;
                SupplyCrate supplyCrate = null;
                __result = false;
                bool playAnim = false;
                GameObject target = __instance.exosuit.GetActiveTarget();
                if (target)
                {
                    pickupable = target.GetComponent<Pickupable>();
                    pickPrefab = target.GetComponent<PickPrefab>();
                    supplyCrate = target.GetComponent<SupplyCrate>();
                }
                if (pickupable != null && pickupable.isPickupable)
                {
                    if (__instance.exosuit.storageContainer.container.HasRoomFor(pickupable))
                    {
                        __instance.animator.SetTrigger("use_tool");
                        __instance.cooldownTime = cooldownDuration = __instance.cooldownPickup;
                        __result = true;
                        return false;
                    }
                    else
                        AddMessage(Language.main.Get(ExosuitClawArm.noRoomNotification));
                }
                else if (pickPrefab)
                {
                    __instance.animator.SetTrigger("use_tool");
                    __instance.cooldownTime = cooldownDuration = __instance.cooldownPickup;
                    __result = true;
                    return false;
                }
                else if (supplyCrate)
                {
                    if (supplyCrate.sealedComp && supplyCrate.sealedComp.IsSealed())
                        return false;

                    if (!supplyCrate.open)
                    {
                        supplyCrate.ToggleOpenState();
                        playAnim = true;
                    }
                    else if (supplyCrate.open)
                    {
                        if (supplyCrate.itemInside)
                        {
                            if (__instance.exosuit.storageContainer.container.HasRoomFor(supplyCrate.itemInside))
                            {
                                ItemsContainer container = __instance.exosuit.storageContainer.container;
                                supplyCrate.itemInside.Initialize();
                                InventoryItem inventoryItem = new InventoryItem(supplyCrate.itemInside);
                                container.UnsafeAdd(inventoryItem);
                                Utils.PlayFMODAsset(__instance.pickupSounds.GetPickupSound(TechData.GetSoundType(supplyCrate.itemInside.GetTechType())), __instance.front, 5f);
                                supplyCrate.itemInside = null;
                                playAnim = true;
                            }
                            else
                            {
                                AddMessage(Language.main.Get(ExosuitClawArm.noRoomNotification));
                                return false;
                            }
                        }
                    }
                    if (playAnim)
                    {
                        __instance.animator.SetTrigger("use_tool");
                        __instance.cooldownTime = cooldownDuration = __instance.cooldownPickup;
                        //supplyCrate.OnHandClick(null);
                        __result = true;
                    }
                    return false;
                }
                else
                {
                    __instance.animator.SetTrigger("bash");
                    __instance.cooldownTime = cooldownDuration = __instance.cooldownPunch;
                    __instance.fxControl.Play(0);
                    __result = true;
                    return false;
                }
            }
            cooldownDuration = 0f;
            __result = false;
            return false;
        }
    }


}

