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
        public static bool afterBurnerWasActive;
        public static float seatruckSidewardMod;
        public static float seatruckBackwardMod;
        public static float seatruckVertMod;
        public static float seatruckPowerEfficiency;
        public static bool afterBurnerActive;
        public static Vector3 moveDir;
        public static int horsePowerUpgrades = 0;

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

        [HarmonyPatch(typeof(GameInput), "GetMoveDirection")]
        class GameInput_GetMoveDirection_Patch
        {
            static void Postfix(GameInput __instance, ref Vector3 __result)
            {
                if (!Main.gameLoaded || !Player.main.inSeatruckPilotingChair || __result == Vector3.zero || moveDir == __result)
                    return;

                if (seatruckSidewardMod < 1 && __result.x != 0)
                    __result = new Vector3(__result.x * seatruckSidewardMod, __result.y, __result.z);

                if (seatruckBackwardMod < 1 && __result.z < 0)
                    __result = new Vector3(__result.x, __result.y, __result.z * seatruckBackwardMod);

                if (seatruckVertMod < 1 && __result.y != 0)
                    __result = new Vector3(__result.x, __result.y * seatruckVertMod, __result.z);

                moveDir = __result;
            }
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
            [HarmonyPostfix, HarmonyPatch("OnUpgradeModuleChange")]
            public static void OnUpgradeModuleChangePostfix(SeaTruckUpgrades __instance, int slotID, TechType techType, bool added)
            {
                //AddDebug("OnUpgradeModuleChange " + techType);
                seatruckPowerEfficiency = __instance.motor.powerEfficiencyFactor;
                horsePowerUpgrades = GetNumHPUpgrades(__instance);
                //AddDebug("OnUpgradeModuleChange horsePowerUpgrades " + horsePowerUpgrades);
                //AddDebug("OnUpgradeModuleChange seatruckPowerEfficiency " + seatruckPowerEfficiency);
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor))]
        class SeaTruckMotor_Patch
        {
            static float origAcceleration;

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(SeaTruckMotor __instance)
            {
                origAcceleration = __instance.acceleration;
                seatruckPowerEfficiency = __instance.powerEfficiencyFactor;
            }
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
                if (!ConfigToEdit.fixSeatruckAnalogMovement.Value && !ConfigToEdit.seatruckAfterburnerWithoutCooldown.Value && !ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value)
                    return;

                if (!__instance.piloting)
                    return;

                Vector3 moveDirection = AvatarInputHandler.main.IsEnabled() || __instance.inputStackDummy.activeInHierarchy ? GameInput.GetMoveDirection() : Vector3.zero;

                float powerEfficiencyFactor = seatruckPowerEfficiency;
                if (ConfigToEdit.seatruckAfterburnerWithoutCooldown.Value && afterBurnerActive)
                {
                    powerEfficiencyFactor = seatruckPowerEfficiency * 2;
                    if (moveDirection == Vector3.zero)
                        afterBurnerActive = false;
                    else
                        afterBurnerActive = true;
                }
                else
                    powerEfficiencyFactor = seatruckPowerEfficiency;

                if (moveDirection == Vector3.zero)
                    return;

                float acceleration = origAcceleration;
                //AddDebug(" acceleration " + acceleration);
                if (ConfigToEdit.fixSeatruckAnalogMovement.Value)
                {
                    float x = Mathf.Abs(moveDirection.x);
                    float z = Mathf.Abs(moveDirection.z);
                    if (x > z)
                        acceleration *= x;
                    else if (x < z)
                        acceleration *= z;
                }
                if (ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value && horsePowerUpgrades > 0)
                {
                    float mod = acceleration * horsePowerUpgrades * .1f;
                    acceleration += mod;
                    mod = powerEfficiencyFactor * horsePowerUpgrades * .1f;
                    powerEfficiencyFactor += mod;
                }
                __instance.acceleration = acceleration;
                __instance.powerEfficiencyFactor = powerEfficiencyFactor;
                //AddDebug(" acceleration f " + __instance.acceleration);
                //AddDebug(" powerEfficiencyFactor f " + __instance.powerEfficiencyFactor);
            }
            [HarmonyPostfix, HarmonyPatch("FixedUpdate")]
            public static void FixedUpdatePostfix(SeaTruckMotor __instance)
            {
                __instance.powerEfficiencyFactor = seatruckPowerEfficiency;
                __instance.acceleration = origAcceleration;
            }

        }



    }
}
