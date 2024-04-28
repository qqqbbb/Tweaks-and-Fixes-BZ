using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Craft_Patch
    {
        static bool crafting = false;
        static float hoverBikeBuildTime = 0f;
        static float timeDecayStart = 0f;

        [HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_Patch
        {
            static void Prefix(Crafter __instance, TechType techType, ref float duration)
            {
                //AddDebug("Craft " + techType);
                duration *= ConfigMenu.craftTimeMult.Value;
                //return true;
            }
        }

        [HarmonyPatch(typeof(HoverpadConstructor), "TryStartConstructBike")]
        internal class HoverpadConstructor_Patch
        {
            public static void Prefix(HoverpadConstructor __instance)
            {
                if (hoverBikeBuildTime == 0f)
                    hoverBikeBuildTime = __instance.timeToConstruct;

                __instance.timeToConstruct = hoverBikeBuildTime * ConfigMenu.craftTimeMult.Value;
                //AddDebug("TryStartConstructBike " + __instance.timeToConstruct);
            }
        }

        [HarmonyPatch(typeof(Constructable), "GetConstructInterval")]
        class Constructable_GetConstructInterval_Patch
        {
            static void Postfix(ref float __result)
            {
                if (NoCostConsoleCommand.main.fastBuildCheat)
                    return;
                //AddDebug("GetConstructInterval " );
                __result *= ConfigMenu.buildTimeMult.Value;
            }
        }

        [HarmonyPatch(typeof(TreeNode), "AddNode", new Type[] { typeof(TreeNode) })]
        class TreeNode_Addnode_Prefix_Patch
        {
            public static bool Prefix(TreeNode __instance, TreeNode node)
            {
                //Main.Log("AddNode " + node.id);
                //AddDebug("AddNode " + node.id);
                if (ConfigToEdit.craftVehicleUpgradesOnlyInMoonpool.Value)
                {
                    if (node.id == "Upgrades")
                    {
                        //Main.Log("AddNode Upgrades !!! " + node.id + " parent " + __instance.id);
                        //AddDebug("AddNode Upgrades !!!");
                        return false;
                    }
                    else if (__instance.id == "Root")
                    { // upgrades from senna mods will be added to root if Upgrades node removed from fabricator
                        if ( node.id == "SeaTruckSpeedMK1" || node.id == "SeaTruckSpeedMK2" || node.id == "SeaTruckSpeedMK3" || node.id == "SeaTruckArmorMK1" || node.id == "SeaTruckArmorMK2" || node.id == "SeaTruckArmorMK3" || node.id == "SeaTruckDepthMK4" || node.id == "SeaTruckDepthMK5" || node.id == "SeaTruckDepthMK6")
                            return false;
                    }
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(CrafterLogic), "NotifyCraftEnd")]
        class CrafterLogic_NotifyCraftEnd_Patch
        {
            static void Postfix(CrafterLogic __instance, GameObject target, TechType techType)
            {
                //AddDebug("CrafterLogic NotifyCraftEnd timeDecayStart " + timeDecayStart);
                if (timeDecayStart > 0)
                {
                    //AddDebug("CrafterLogic NotifyCraftEnd timeDecayStart" + timeDecayStart);
                    Eatable eatable = target.GetComponent<Eatable>();
                    if (eatable)
                        eatable.timeDecayStart = timeDecayStart;
                }
                Battery battery = target.GetComponent<Battery>();
                if (battery)
                {
                    if (ConfigMenu.batteryChargeMult.Value != 1f)
                        battery._capacity *= ConfigMenu.batteryChargeMult.Value;
                    //AddDebug("crafterOpen");
                    float mult = ConfigMenu.craftedBatteryCharge.Value * .01f;
                    battery._charge = battery._capacity * mult;
                }
                timeDecayStart = 0f;
                crafting = false;
            }
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("ConsumeResourcesForRecipe")]
            static void Prefix(Inventory __instance, TechType techType, uGUI_IconNotifier.AnimationDone endFunc )
            {
                crafting = true;
                //AddDebug("ConsumeResourcesForRecipe");
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnRemoveItem")]
            static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnRemoveItem " + item.item.GetTechName());
                if (crafting)
                {
                    if (Util.IsEatableFish(item.item.gameObject))
                    {
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        timeDecayStart = eatable.timeDecayStart;
                        //AddDebug("OnRemoveItem save timeDecayStart " + timeDecayStart);
                    }
                    //else
                    //    timeDecayStart = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(EnergyMixin), "OnCraftEnd")]
        public class EnergyMixin_OnCraftEnd_Patch
        {
            private static void Prefix(EnergyMixin __instance, TechType techType)
            { // applies to tools and vehicles
                //AddDebug("EnergyMixin OnCraftEnd " + techType);
                if (!ConfigToEdit.craftWithoutBattery.Value || techType == TechType.MapRoomCamera || techType == TechType.FlashlightHelmet || techType == TechType.TeleportationTool || techType == TechType.SpyPenguin || techType == TechType.SpyPenguinRemote)
                    return;

                __instance.defaultBattery = TechType.None;
            }
        }


    }
}
