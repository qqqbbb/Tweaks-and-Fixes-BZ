﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Battery_Patch
    {
        [HarmonyPatch(typeof(EnergyMixin), "ConsumeEnergy")]
        class EnergyMixin_OnAfterDeserialize_Patch
        {
            static void Prefix(EnergyMixin __instance, ref float amount)
            {
                if (__instance.GetComponent<Hoverbike>())
                {
                    //AddDebug("Hoverbike ConsumeEnergy");
                    amount *= Main.config.vehicleEnergyConsMult;
                }
                else
                {
                    //AddDebug(__instance.name + " EnergyMixin ConsumeEnergy");
                    amount *= Main.config.toolEnergyConsMult;
                }
            }
        }

        [HarmonyPatch(typeof(Vehicle), "ConsumeEngineEnergy")]
        class Vehicle_ConsumeEngineEnergy_Patch
        {
            static void Prefix(Vehicle __instance, ref float energyCost)
            {
                //AddDebug("Vehicle ConsumeEnergy");
                energyCost *= Main.config.vehicleEnergyConsMult;
            }
        }

        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.ConsumeEnergy))]
        class PowerSystem_ConsumeEnergy_Patch
        {
            static void Prefix(ref float amount, IPowerInterface powerInterface)
            {
                PowerRelay pr = powerInterface as PowerRelay;
                if (pr && pr.GetComponent<SeaTruckSegment>())
                {
                    amount *= Main.config.vehicleEnergyConsMult;
                    //AddDebug(pr.name + " SeaTruck PowerRelay ConsumeEnergy ");
                }
                else
                {
                    //AddDebug(pr.name + " base PowerRelay ConsumeEnergy ");
                    amount *= Main.config.baseEnergyConsMult;
                }
            }
        }

        //[HarmonyPatch(typeof(Charger), "Start")]
        class Charger_Start_Patch
        {
            static void Postfix(Charger __instance)
            {
                //foreach (string name in Main.config.nonRechargeable)
                //{
                //    TechTypeExtensions.FromString(name, out TechType tt, true);
                //    if (tt != TechType.None && __instance.allowedTech.Contains(tt))
                //    {
                        //AddDebug("nonRechargeable " + name);
                //        __instance.allowedTech.Remove(tt);
                //    }
                //}
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

        //[HarmonyPatch(typeof(Battery), "OnAfterDeserialize")]
        class Battery_OnAfterDeserialize_Patch
        {
            static void Postfix(Battery __instance)
            {
                //if (Main.crafterOpen)
                {
                    //AddDebug("crafterOpen");
                    float mult = Main.config.craftedBatteryCharge * .01f;
                    __instance._charge = __instance._capacity * mult;
                }
            }
        }


    }
}
