using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Battery_Patch
    {
        static EnergyMixin PlayerToolEM;
        static EnergyInterface propCannonEI;
        public static HashSet<PowerRelay> seatruckPRs = new HashSet<PowerRelay>();
        static Dictionary<string, float> defaultBatteryCharge = new Dictionary<string, float>();
        public static HashSet<TechType> notRechargableBatteries = new HashSet<TechType>();


        [HarmonyPatch(typeof(EnergyMixin), "ConsumeEnergy")]
        class EnergyMixin_OnAfterDeserialize_Patch
        {
            static void Prefix(EnergyMixin __instance, ref float amount)
            {
                //AddDebug(__instance.name + " EnergyMixin ConsumeEnergy");
                if (PlayerToolEM == __instance)
                {
                    //AddDebug(__instance.name + " EnergyMixin ConsumeEnergy");
                    amount *= ConfigMenu.toolEnergyConsMult.Value;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerTool), "OnDraw")]
        class PlayerTool_OnDraw_Patch
        {
            static void Postfix(PlayerTool __instance)
            {
                //AddDebug("PlayerTool OnDraw ");
                PlayerToolEM = __instance.energyMixin;
            }
        }

        [HarmonyPatch(typeof(PropulsionCannonWeapon), "OnDraw")]
        class PropulsionCannonWeapon_OnDraw_Patch
        {
            static void Postfix(PropulsionCannonWeapon __instance)
            {
                propCannonEI = __instance.propulsionCannon.energyInterface;
            }
        }

        [HarmonyPatch(typeof(EnergyInterface), "ConsumeEnergy")]
        class EnergyInterface_ConsumeEnergy_Patch
        {
            static void Prefix(EnergyInterface __instance, ref float amount)
            {
                if (propCannonEI == __instance)
                {
                    //AddDebug(" propCannon ConsumeEnergy");
                    amount *= ConfigMenu.toolEnergyConsMult.Value;
                }
            }
        }

        [HarmonyPatch(typeof(Vehicle), "ConsumeEngineEnergy")]
        class Vehicle_ConsumeEngineEnergy_Patch
        {
            static void Prefix(Vehicle __instance, ref float energyCost)
            {
                //AddDebug("Vehicle ConsumeEnergy");
                energyCost *= ConfigMenu.vehicleEnergyConsMult.Value;
            }
        }

        [HarmonyPatch(typeof(SeaTruckSegment), "Start")]
        class SeaTruckSegment_Start_Patch
        {
            static void Postfix(SeaTruckSegment __instance)
            {
                if (__instance.relay)
                    seatruckPRs.Add(__instance.relay);
            }
        }

        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.ConsumeEnergy))]
        class PowerSystem_ConsumeEnergy_Patch
        {
            static void Prefix(ref float amount, IPowerInterface powerInterface)
            {
                PowerRelay pr = powerInterface as PowerRelay;
                if (pr && seatruckPRs.Contains(pr))
                {
                    amount *= ConfigMenu.vehicleEnergyConsMult.Value;
                    //AddDebug(pr.name + " SeaTruck PowerRelay ConsumeEnergy ");
                }
                else
                {
                    //AddDebug(pr.name + " base PowerRelay ConsumeEnergy ");
                    amount *= ConfigMenu.baseEnergyConsMult.Value;
                }
            }
        }

        [HarmonyPatch(typeof(Battery), "OnAfterDeserialize")]
        class Battery_OnAfterDeserialize_Patch
        {
            static void Postfix(Battery __instance)
            {
                if (ConfigMenu.batteryChargeMult.Value == 1f || __instance.name.IsNullOrWhiteSpace())
                    return;

                //AddDebug(__instance.name + " Battery OnAfterDeserialize " + __instance._capacity);
                if (!defaultBatteryCharge.ContainsKey(__instance.name))
                {
                    defaultBatteryCharge[__instance.name] = __instance._capacity;
                }
                if (defaultBatteryCharge.ContainsKey(__instance.name))
                {
                    __instance._capacity = defaultBatteryCharge[__instance.name] * ConfigMenu.batteryChargeMult.Value;
                    if (__instance.charge > __instance._capacity)
                        __instance.charge = __instance._capacity;
                }
            }
        }


        [HarmonyPatch(typeof(Charger), "Start")]
        class Charger_Start_Patch
        {
            static void Postfix(Charger __instance)
            {
                //AddDebug(__instance.name + " Charger Start");
                foreach (TechType tt in notRechargableBatteries)
                {
                    if (__instance.allowedTech.Contains(tt))
                    {
                        __instance.allowedTech.Remove(tt);
                        //AddDebug("remove " + tt + " from " + __instance.name);
                    }
                }
                //Main.logger.LogMessage(__instance.name + " Charger Start");
                //foreach (var tt in __instance.allowedTech)
                //    Main.logger.LogMessage(__instance.name + " allowedTech " + tt);
            }
        }

        //[HarmonyPatch(typeof(Charger), "IsAllowedToAdd")]
        class Charger_IsAllowedToAdd_Patch
        {
            static bool Prefix(Charger __instance, Pickupable pickupable, ref bool __result)
            {
                if (pickupable == null)
                {
                    __result = false;
                    return false;
                }
                TechType t = pickupable.GetTechType();
                string name = t.AsString();
                TechTypeExtensions.FromString(name, out TechType tt, true);
                TechType techType = pickupable.GetTechType();

                //if (tt != TechType.None && Main.config.nonRechargeable.Contains(name) && tt == t)
                //{
                //    AddDebug("nonRechargeable " + name);
                //    __result = false;
                //    return false;
                //}
                if (__instance.allowedTech != null && __instance.allowedTech.Contains(techType))
                    __result = true;

                return false;
            }
        }


    }
}
