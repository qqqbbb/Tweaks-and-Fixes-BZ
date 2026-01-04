using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Exosuit))]
    class Exosuit_Patch
    {
        //event:/sub/seamoth/seaglide_light_on
        //{fe76457f-0c94-4245-a080-8a5b2f8853c4}
        //event:/sub/seamoth/seaglide_light_off
        //{b52592a9-19f5-45d1-ad56-7d355fc3dcc3}

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
        static Vector3 aimTargetLeftPos;
        static Vector3 aimTargetRightPos;

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
        {// torpedoStorage has torpedos after torpedo arm removed
            if (exosuit.leftArmType != TechType.ExosuitTorpedoArmModule && exosuit.rightArmType != TechType.ExosuitTorpedoArmModule)
                return false;

            List<TorpedoType> torpedos = GetTorpedos(exosuit, torpedoStorage);
            //AddDebug("torpedos Count " + torpedos.Count);
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
            List<TorpedoType> torpedoTypes = new List<TorpedoType>(exosuit.torpedoTypes);
            List<TorpedoType> torpedos = GetTorpedos(exosuit, torpedoStorage);
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
                        return Language.main.Get(torpedoType) + " " + torpedoStorage.GetCount(torpedoType);
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
            if (Language.main.GetCurrentLanguage() == "English")
                name = ShortenArmName(name);

            return name;
        }

        public static void GetArmNames(Exosuit exosuit)
        {
            //AddDebug("GetArmNames Left Arm " + exosuit.currentLeftArmType);
            //AddDebug("GetArmNames Right Arm " + exosuit.currentRightArmType);
            //AddDebug("GetArmNames GetCurrentLanguage " + Language.main.GetCurrentLanguage());

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
                if (Language.main.GetCurrentLanguage() == "English")
                    leftArm = ShortenArmName(leftArm);
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
                if (Language.main.GetCurrentLanguage() == "English")
                    rightArm = ShortenArmName(rightArm);
                //AddDebug("GetArmNames rightArm " + rightArm);
            }
        }

        private static string ShortenArmName(string armName)
        {
            //AddDebug("ShortenArmName armName " + armName);
            armName = armName.Replace(Language.main.Get("Exosuit"), "");
            armName = armName.Trim();
            armName = Util.UppercaseFirstCharacter(armName);
            return armName;
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

        private static void CheckExosuitButtons(Exosuit exosuit)
        {
            if (!IngameMenu.main.isActiveAndEnabled && !Player.main.pda.isInUse && Player.main.inExosuit && Player.main.currentMountedVehicle == exosuit)
            {
                if (GameInput.lastDevice == GameInput.Device.Controller)
                {
                    bool leftTorpedo = HasMoreThan1TorpedoType(exosuit, torpedoStorageLeft);
                    bool rightTorpedo = HasMoreThan1TorpedoType(exosuit, torpedoStorageRight);

                    if (leftTorpedo && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                    {
                        ChangeTorpedo(exosuit, 0);
                    }
                    else if (rightTorpedo && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                    {
                        ChangeTorpedo(exosuit, 1);
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void StartPostfix(Exosuit __instance)
        {
            //AddDebug("Exosuit Start " + __instance.onGroundForceMultiplier);
            //exosuitName = Language.main.Get("Exosuit");
            //rightButton = uGUI.FormatButton(GameInput.Button.RightHand);
            //leftButton = uGUI.FormatButton(GameInput.Button.LeftHand);
            //if (Player.main.currentMountedVehicle == null)
            //    AddDebug("Exosuit Start currentMountedVehicle == null");
            //else
            //    AddDebug("Exosuit Start currentMountedVehicle " + Player.main.currentMountedVehicle.ToString());
            if (Player.main.currentMountedVehicle && Player.main.currentMountedVehicle == __instance)
            {
                GetArmNames(__instance);
                armNamesChanged = true;
            }
            exosuitStarted = true;
        }


        [HarmonyPrefix, HarmonyPatch("Update")]
        public static void UpdatePrefix(Exosuit __instance)
        {
            if (!Main.gameLoaded)
                return;

            if (__instance.IsPowered() == false)
            {
                aimTargetLeftPos = __instance.aimTargetLeft.position;
                aimTargetRightPos = __instance.aimTargetRight.position;
            }
        }

        [HarmonyPostfix, HarmonyPatch("Update")]
        public static void UpdatePostfix(Exosuit __instance)
        {
            if (!Main.gameLoaded)
                return;
            //if (__instance.powersliding)
            //    AddDebug("powersliding " + __instance.powersliding);
            //if (__instance.horizontalJetsActive)
            //    AddDebug("horizontalJetsActive " + __instance.horizontalJetsActive);
            //if (__instance.thrustPower < 1F)
            //    AddDebug("thrustPower " + __instance.thrustPower.ToString("0.0"));
            CheckExosuitButtons(__instance);
            if (__instance.IsPowered() == false)
            { // fix bug: exosuit can move arms when unpowered
                __instance.aimTargetLeft.position = aimTargetLeftPos;
                __instance.aimTargetRight.position = aimTargetRightPos;
            }
        }

        [HarmonyPostfix, HarmonyPatch("SlotKeyDown")]
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
            //AddDebug("SlotNext ");
        }

        [HarmonyPrefix, HarmonyPatch("UpdateUIText")]
        public static bool UpdateUITextPrefix(Exosuit __instance, bool hasPropCannon)
        {
            //AddDebug("Exosuit UpdateUIText  ");

            if (armNamesChanged || !__instance.hasInitStrings || __instance.lastHasPropCannon != hasPropCannon)
            {
                bool leftTorpedo = HasMoreThan1TorpedoType(__instance, torpedoStorageLeft);
                bool rightTorpedo = HasMoreThan1TorpedoType(__instance, torpedoStorageRight);
                //AddDebug("UpdateUIText leftTorpedo " + leftTorpedo + " rightTorpedo " + rightTorpedo);
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
                else if (GameInput.lastDevice == GameInput.Device.Controller)
                { // alt tool button used by prop arm
                    if (leftTorpedo || rightTorpedo)
                        __instance.sb.Append(UI_Patches.exosuitChangeTorpedoButton);
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

        [HarmonyPostfix, HarmonyPatch("OnPilotModeBegin")]
        public static void OnPilotModeBeginPostfix(Exosuit __instance)
        {
            if (ConfigToEdit.disableGravityForExosuit.Value)
            {
                Util.FreezeObject(__instance.gameObject, false);
            }
        }

        [HarmonyPostfix, HarmonyPatch("EnterVehicle")]
        public static void EnterVehiclePostfix(Exosuit __instance)
        { // runs before Exosuit.Start
          //AddDebug("EnterVehicle");
          //if (ConfigMenu.exosuitMoveTweaks.Value)
          //    __instance.onGroundForceMultiplier = 2f;
          //else
          //    __instance.onGroundForceMultiplier = 4f;

            if (exosuitStarted)
            {
                GetArmNames(__instance);
                armNamesChanged = true;
            }
        }

        [HarmonyPostfix, HarmonyPatch("OnPilotModeEnd")]
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
                acceleration *= ConfigMenu.exosuitSpeedMult.Value;
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

        [HarmonyPrefix, HarmonyPatch("GetTemperature")]
        public static bool GetTemperaturePrefix(Vehicle __instance, ref float __result)
        { // fix thermometer values wnen above water
            if (Player.main.inExosuit && Player.main.currentMountedVehicle == __instance)
            {
                //BodyTemperature bt = Player.main.GetComponent<BodyTemperature>();
                __result = Player_.ambientTemperature;
                //AddDebug("ambient Temperature " + (int)Player_.ambientTemperature);
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
            //AddDebug("  ToggleSlot  " + slotID + " " + state);
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

    [HarmonyPatch(typeof(SeamothStorageContainer), "OnCraftEnd")]
    class SeamothStorageContainer_OnCraftEnd_Patch
    {
        static private IEnumerator OnTorpedoCraftEnd(SeamothStorageContainer smsc)
        {
            TaskResult<GameObject> taskResult = new TaskResult<GameObject>();
            TaskResult<Pickupable> pickupableResult = new TaskResult<Pickupable>();
            for (int index = 0; index < ConfigToEdit.freeTorpedos.Value; ++index)
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

    [HarmonyPatch(typeof(SupplyCrate), "Start")]
    class SupplyCrate_Start_Patch
    {
        public static void Postfix(SupplyCrate __instance)
        {
            ExosuitClawArm_Patch.supplyCrates.Add(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(ExosuitClawArm))]
    class ExosuitClawArm_Patch
    {
        public static HashSet<GameObject> supplyCrates = new HashSet<GameObject>();

        [HarmonyPostfix, HarmonyPatch("IExosuitArm.GetInteractableRoot")]
        static void GetInteractableRootPostfix(ExosuitClawArm __instance, GameObject target, ref GameObject __result)
        {
            if (target && supplyCrates.Contains(target))
            {
                //AddDebug("ExosuitClawArm GetInteractableRoot Postfix target " + target.name);
                __result = target;
            }
        }

        [HarmonyPostfix, HarmonyPatch("TryUse", new Type[] { typeof(float) }, new[] { ArgumentType.Out })]
        static void TryUsePostfix(ExosuitClawArm __instance, ref float cooldownDuration, ref bool __result)
        {
            if (__instance.timeUsed + __instance.cooldownPickup > Time.time)
                return;

            //GameObject target = __instance.exosuit.GetActiveTarget();
            //if (target)
            //    AddDebug("TryUse target " + target.name);
            //else
            //    AddDebug("TryUse no target ");

            __instance.animator.ResetTrigger("bash");
            __instance.animator.SetTrigger("use_tool");
            __instance.timeUsed = Time.time;
            if (TryPickUpGrabbedObject(__instance))
                return;

            TryUseCrate(__instance);
        }

        private static bool TryUseCrate(ExosuitClawArm clawArm)
        {
            GameObject target = clawArm.exosuit.GetActiveTarget();
            if (target == null)
                return false;

            SupplyCrate supplyCrate = null;
            if (supplyCrates.Contains(target))
                supplyCrate = target.GetComponent<SupplyCrate>();

            if (supplyCrate == null)
                return false;

            if (supplyCrate.sealedComp && supplyCrate.sealedComp.IsSealed())
                return false;

            if (supplyCrate.open == false)
            {
                supplyCrate.ToggleOpenState();
                return true;
            }
            if (supplyCrate.itemInside == null)
                return false;

            if (AddToExosuitContainer(clawArm.exosuit, supplyCrate.itemInside, clawArm.pickupSounds, clawArm.front, 5f))
            {
                supplyCrate.itemInside = null;
                return true;
            }
            return false;
        }

        private static bool TryPickUpGrabbedObject(ExosuitClawArm clawArm)
        {
            GameObject grabbedObject = GetGrabbedObject(clawArm.exosuit);
            if (grabbedObject == null)
                return false;

            Pickupable pickupable = grabbedObject.GetComponent<Pickupable>();
            if (pickupable == null)
                return false;
            //AddDebug("Pick Up Grabbed Object " + pickupable.name);
            return AddToExosuitContainer(clawArm.exosuit, pickupable, clawArm.pickupSounds, clawArm.front, 5f);
        }

        private static bool AddToExosuitContainer(Exosuit exosuit, Pickupable pickupable, TechSoundData pickupSounds = null, Transform soundPos = default, float soundRadius = default)
        {
            if (ConfigToEdit.canPickUpContainerWithItems.Value == false && Pickupable_.pickupableStorage_.ContainsKey(pickupable))
            {
                PickupableStorage ps = Pickupable_.pickupableStorage_[pickupable];
                ErrorMessage.AddDebug(Language.main.Get(ps.cantPickupClickText));
                return false;
            }
            ItemsContainer exosuitContainer = exosuit.storageContainer.container;
            if (exosuitContainer.HasRoomFor(pickupable))
            {
                pickupable.Initialize();
                InventoryItem inventoryItem = new InventoryItem(pickupable);
                exosuitContainer.UnsafeAdd(inventoryItem);
                if (pickupSounds)
                    Utils.PlayFMODAsset(pickupSounds.GetPickupSound(TechData.GetSoundType(pickupable.GetTechType())), soundPos, soundRadius);

                return true;
            }
            else
            {
                ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
            }
            return false;
        }

        private static GameObject GetGrabbedObject(Exosuit exosuit)
        {
            if (exosuit.currentLeftArmType == TechType.ExosuitPropulsionArmModule)
            {
                ExosuitPropulsionArm propArm = exosuit.leftArm as ExosuitPropulsionArm;
                if (propArm && propArm.propulsionCannon.grabbedObject)
                    return propArm.propulsionCannon.grabbedObject;
            }
            if (exosuit.currentRightArmType == TechType.ExosuitPropulsionArmModule)
            {
                ExosuitPropulsionArm propArm = exosuit.rightArm as ExosuitPropulsionArm;
                if (propArm && propArm.propulsionCannon.grabbedObject)
                    return propArm.propulsionCannon.grabbedObject;
            }
            return null;
        }
    }

}

