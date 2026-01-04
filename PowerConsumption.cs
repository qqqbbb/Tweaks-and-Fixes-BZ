using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class PowerConsumption
    {
        static EnergyMixin PlayerToolEM;
        static EnergyInterface propCannonEI;
        public static HashSet<PowerRelay> seatruckPRs = new HashSet<PowerRelay>();

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


    }
}
