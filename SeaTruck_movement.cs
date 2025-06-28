using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class SeaTruck_movement
    {
        public static float seatruckSidewardMod;
        public static float seatruckBackwardMod;
        public static float seatruckVertMod;
        public static float origSeatruckPowerEfficiency;
        public static bool afterBurnerActive;
        public static int horsePowerUpgrades;
        static float origAcceleration;

        public static void CacheSettings()
        {
            seatruckBackwardMod = 1 - Mathf.Clamp(ConfigToEdit.seatruckBackwardSpeedMod.Value, 0, 100) * .01f;
            seatruckSidewardMod = 1 - Mathf.Clamp(ConfigToEdit.seatruckSidewardSpeedMod.Value, 0, 100) * .01f;
            seatruckVertMod = 1 - Mathf.Clamp(ConfigToEdit.seatruckVertSpeedMod.Value, 0, 100) * .01f;
        }

        public static int GetNumHPUpgrades(SeaTruckUpgrades seaTruckUpgrades)
        {
            int count = 0;
            for (int slotID = 0; slotID < SeaTruckUpgrades.slotIDs.Length; ++slotID)
            {
                TechType tt = seaTruckUpgrades.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
                if (tt == TechType.SeaTruckUpgradeHorsePower)
                    count++;
            }
            //AddDebug("GetNumPowerUpgrades " + count);
            return count;
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades))]
        class SeaTruckUpgrades_Patch
        {
            [HarmonyPrefix, HarmonyPatch("IsAllowedToAdd")]
            public static bool IsAllowedToAddPrefix(SeaTruckUpgrades __instance, Pickupable pickupable, ref bool __result)
            {
                if (!ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value)
                    return true;

                if (pickupable == null)
                    return false;

                if (__instance.modules.GetCount(pickupable.GetTechType()) == 0 || __instance.IsEquipped(pickupable))
                    __result = true;
                else if (pickupable.GetTechType() == TechType.SeaTruckUpgradeHorsePower)
                    __result = true;
                else
                {
                    __result = false;
                    //AddMessage(Language.main.Get("SeaTruckErrorMultipleTechTypes"));
                }
                //AddDebug("IsAllowedToAdd " + pickupable.GetTechType() + " " + __result);
                return false;
            }

            [HarmonyPostfix, HarmonyPatch("OnUpgradeModuleUse")]
            public static void OnUpgradeModuleUsePostfix(SeaTruckUpgrades __instance, TechType techType, int slotID)
            {
                if (ConfigToEdit.seatruckAfterburnerWithoutCooldown.Value && techType == TechType.SeaTruckUpgradeAfterburner)
                    afterBurnerActive = true;

            }
            //[HarmonyPostfix, HarmonyPatch("OnUpgradeModuleChange")]
            public static void OnUpgradeModuleChangePostfix(SeaTruckUpgrades __instance, int slotID, TechType techType, bool added)
            {
                //AddDebug($"OnUpgradeModuleChange {techType} acc {__instance.motor.acceleration}");
                origSeatruckPowerEfficiency = __instance.motor.powerEfficiencyFactor;
                horsePowerUpgrades = GetNumHPUpgrades(__instance);
                //if (origAcceleration != __instance.motor.acceleration)
                //    origAcceleration = __instance.motor.acceleration;
                //AddDebug("OnUpgradeModuleChange horsePowerUpgrades " + horsePowerUpgrades);
                //AddDebug("OnUpgradeModuleChange seatruckPowerEfficiency " + seatruckPowerEfficiency);
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor))]
        class SeaTruckMotor_Patch
        {
            [HarmonyPrefix, HarmonyPatch("GetWeight")]
            public static void GetWeightPostfix(SeaTruckMotor __instance, ref float __result)
            {
                //AddDebug("GetWeight " + __instance.truckSegment.GetWeight() + " GetAttachedWeight " + __instance.truckSegment.GetAttachedWeight());
                if (ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value)
                {
                    __result = __instance.truckSegment.GetWeight() + __instance.truckSegment.GetAttachedWeight();
                }
            }

            [HarmonyPrefix, HarmonyPatch("FixedUpdate")]
            public static void FixedUpdatePrefix(SeaTruckMotor __instance)
            {
                if (!ConfigToEdit.fixSeatruckAnalogMovement.Value && !ConfigToEdit.seatruckAfterburnerWithoutCooldown.Value && !ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value && ConfigMenu.seatruckSpeedMult.Value == 1 && seatruckVertMod == 1 && seatruckBackwardMod == 1 && seatruckSidewardMod == 1)
                    return;

                if (!__instance.piloting)
                    return;

                Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() || __instance.inputStackDummy.activeInHierarchy ? GameInput.GetMoveDirection() : Vector3.zero;

                //AddDebug($"prefix SeaTruckMotor.acceleration {__instance.acceleration}");
                origAcceleration = __instance.acceleration;
                origSeatruckPowerEfficiency = __instance.powerEfficiencyFactor;
                float powerEfficiencyFactor = origSeatruckPowerEfficiency;
                if (ConfigToEdit.seatruckAfterburnerWithoutCooldown.Value && afterBurnerActive)
                {
                    powerEfficiencyFactor = origSeatruckPowerEfficiency * 2;
                    if (moveDirection == Vector3.zero)
                        afterBurnerActive = false;
                    else
                        afterBurnerActive = true;

                    __instance.afterBurnerActive = afterBurnerActive;
                }
                else
                    powerEfficiencyFactor = origSeatruckPowerEfficiency;

                if (moveDirection == Vector3.zero)
                    return;

                float acceleration = origAcceleration;
                if (ConfigToEdit.fixSeatruckAnalogMovement.Value)
                {
                    float x = Mathf.Abs(moveDirection.x);
                    float z = Mathf.Abs(moveDirection.z);
                    if (x > 0 || z > 0)
                    {
                        if (x > z)
                            acceleration *= x;
                        else
                            acceleration *= z;
                    }
                }
                if (seatruckVertMod < 1 || seatruckBackwardMod < 1 || seatruckSidewardMod < 1)
                {
                    float x = Mathf.Abs(moveDirection.x);
                    float y = Mathf.Abs(moveDirection.y);
                    float z = Mathf.Abs(moveDirection.z);
                    float zz = z;
                    if (moveDirection.z < 0)
                        zz *= seatruckBackwardMod;

                    acceleration *= (x * seatruckSidewardMod + y * seatruckVertMod + zz) / (x + y + z);
                }
                if (ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value && horsePowerUpgrades > 0)
                {
                    float mod_ = acceleration * horsePowerUpgrades * .1f;
                    acceleration += mod_;
                    mod_ = powerEfficiencyFactor * horsePowerUpgrades * .1f;
                    powerEfficiencyFactor += mod_;
                }
                __instance.acceleration = acceleration * ConfigMenu.seatruckSpeedMult.Value;
                __instance.powerEfficiencyFactor = powerEfficiencyFactor;
                //AddDebug(" acceleration f " + __instance.acceleration.ToString("0.0"));
                //AddDebug(" powerEfficiencyFactor f " + __instance.powerEfficiencyFactor);
            }

            [HarmonyPostfix, HarmonyPatch("FixedUpdate")]
            public static void FixedUpdatePostfix(SeaTruckMotor __instance)
            {
                if (!ConfigToEdit.fixSeatruckAnalogMovement.Value && !ConfigToEdit.seatruckAfterburnerWithoutCooldown.Value && !ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value && ConfigMenu.seatruckSpeedMult.Value == 1 && seatruckVertMod == 1 && seatruckBackwardMod == 1 && seatruckSidewardMod == 1)
                    return;

                if (!__instance.piloting)
                    return;

                __instance.acceleration = origAcceleration;
                __instance.powerEfficiencyFactor = origSeatruckPowerEfficiency;
                //AddDebug($"Postfix SeaTruckMotor.acceleration {__instance.acceleration}");
            }

        }



    }
}
