using HarmonyLib;
using System;
using System.Collections.Generic;
using static ErrorMessage;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Seatruck_Patch
    {
        public static bool afterBurnerWasActive = false;
        //public static SeaTruckUpgrades seaTruckUpgrades = null;
        public static int powerUpgrades = 0;

        //[HarmonyPatch(typeof(SeaTruckUpgrades), "Start")]
        class SeaTruckUpgrades_Start_Patch
        {
            public static void Postfix(SeaTruckUpgrades __instance)
            {
                //seaTruckUpgrades = __instance;
            }
        }

        public static int GetNumPowerUpgrades(SeaTruckUpgrades seaTruckUpgrades)
        {
            int count = 0;
            for (int slotID = 0; slotID < SeaTruckUpgrades.slotIDs.Length; ++slotID)
            {
                TechType tt = seaTruckUpgrades.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
                if (tt == TechType.SeaTruckUpgradeHorsePower)
                    count++;
            }
            return count;
        }

        [HarmonyPatch(typeof(SeaTruckMotor), "GetWeight")]
        class SeaTruckMotor_GetWeight_Patch
        {
            public static bool Prefix(SeaTruckMotor __instance, ref float __result)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return true;

                __result = __instance.truckSegment.GetWeight() + __instance.truckSegment.GetAttachedWeight() * 0.8f;
                return false;
            }
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades), "IsAllowedToAdd")]
        class SeaTruckUpgrades_IsAllowedToAdd_Patch
        {
            public static bool Prefix(SeaTruckUpgrades __instance, Pickupable pickupable, ref bool __result)
            {
                if (!Main.config.seatruckMoveTweaks)
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
                    AddMessage(Language.main.Get("SeaTruckErrorMultipleTechTypes"));
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades), "OnUpgradeModuleChange")]
        class SeaTruckUpgrades_OnUpgradeModuleChange_Patch
        {
            public static void Postfix(SeaTruckUpgrades __instance, TechType techType, bool added)
            {
                powerUpgrades = GetNumPowerUpgrades(__instance);
                //AddDebug("OnUpgradeModuleChange " + techType + " " + added);
                //AddDebug("powerUpgrades " + powerUpgrades);
            }
        }

        [HarmonyPatch(typeof(SeaTruckUpgrades), "TryActivateAfterBurner")]
        class SeaTruckUpgrades_TryActivateAfterBurner_Patch
        {
            public static bool Prefix(SeaTruckUpgrades __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return true;

                for (int slotID = 0; slotID < SeaTruckUpgrades.slotIDs.Length; ++slotID)
                {
                    TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
                    if (techTypeInSlot == TechType.SeaTruckUpgradeAfterburner)
                    {
                        if (!__instance.ConsumeEnergy(techTypeInSlot))
                            break;
                        __instance.OnUpgradeModuleUse(techTypeInSlot, slotID);
                        break;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor), "Update")]
        class SeaTruckMotor_Update_Patch
        {
            public static void Prefix(SeaTruckMotor __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return;

                if (__instance.afterBurnerActive)
                {
                    //AddDebug("BOOST");
                    //__instance.afterBurnerTime = Time.time + 1f;
                    afterBurnerWasActive = true;
                }
            }
            public static void Postfix(SeaTruckMotor __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return;

                if (GameInput.GetButtonHeld(GameInput.Button.Sprint))
                {
                    if (afterBurnerWasActive || __instance.afterBurnerActive)
                    {
                        //AddDebug("BOOST");
                        __instance.afterBurnerActive = true;
                    }
                    else
                    {
                        afterBurnerWasActive = false;
                        __instance.afterBurnerActive = false;
                    }
                }
                else
                {
                    afterBurnerWasActive = false;
                    __instance.afterBurnerActive = false;
                }
            }
        }

        [HarmonyPatch(typeof(SeaTruckMotor), "FixedUpdate")]
        class SeaTruckMotor_FixedUpdate_Patch
        {
            public static bool Prefix(SeaTruckMotor __instance)
            {
                if (!Main.config.seatruckMoveTweaks)
                    return true;

                if (__instance.transform.position.y < Ocean.GetOceanLevel() && __instance.useRigidbody != null && (__instance.IsPowered() && !__instance.IsBusyAnimating()))
                {
                    if (__instance.piloting)
                    {
                        Vector3 input = AvatarInputHandler.main.IsEnabled() || __instance.inputStackDummy.activeInHierarchy ? GameInput.GetMoveDirection() : Vector3.zero;
                        input = input.normalized;
                        input.x *= .5f;
                        input.y *= .5f;
                        if (input.z < 0)
                            input.z *= .5f;

                        Int2 int2;
                        int2.x = input.x <= 0.0 ? (input.x >= 0.0 ? 0 : -1) : 1;
                        int2.y = input.z <= 0.0 ? (input.z >= 0.0 ? 0 : -1) : 1;
                        __instance.leverDirection = int2;
                        if (__instance.afterBurnerActive)
                            input.z = 1f;

                        float powerBonus = 1 + powerUpgrades * .1f;
                        if (__instance.relay == null) // player pushing module
                            powerBonus = 1f;
                        Vector3 vector3_2 = MainCameraControl.main.rotation * input;
                        float acceleration = 1f / Mathf.Max(1f, __instance.GetWeight() * 0.35f) * __instance.acceleration * powerBonus;
                        if (__instance.afterBurnerActive)
                        {
                            //AddDebug("afterBurnerActive " + __instance.afterBurnerActive);
                            acceleration += 7f;
                        }
                        //AddDebug("__instance.relay " + __instance.relay);

                        __instance.useRigidbody.AddForce(acceleration * vector3_2, ForceMode.Acceleration);
                        if (__instance.relay && input != Vector3.zero)
                        {
                            float mult = __instance.afterBurnerActive ? 2f : 1f;
                            mult *= powerBonus;
                            __instance.relay.ConsumeEnergy(mult * Time.deltaTime * __instance.powerEfficiencyFactor * 0.12f, out float _);
                        }

                    }
                    __instance.StabilizeRoll();
                }
                if (!__instance.truckSegment.IsFront() || __instance.IsPowered() && !__instance.truckSegment.ReachingOutOfWater() && (!__instance.seatruckanimation || __instance.seatruckanimation.currentAnimation != SeaTruckAnimation.Animation.Enter))
                    return false;
                __instance.StabilizePitch();
                return false;
            }

        }


    }
}
