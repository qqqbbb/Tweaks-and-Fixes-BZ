using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class InventoryItemIconColorChanger
    {
        [HarmonyPatch(typeof(uGUI_ItemsContainer))]
        class uGUI_ItemsContainer_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("OnRemoveItem")]
            static void OnRemoveItemostfix(uGUI_ItemsContainer __instance, InventoryItem item)
            {
                if (__instance.container._label != "InventoryLabel")
                    return;
                //AddDebug("uGUI_ItemsContainer OnRemoveItem " + item.item.GetTechName());

                IItemsContainer openContainer = Util.GetOpenContainer();
                if (openContainer == null)
                    return;

                if (openContainer.label == "RecyclotronStorageLabel")
                {

                }
            }
            [HarmonyPostfix, HarmonyPatch("OnAddItem")]
            static void OnAddItemPostfix(uGUI_ItemsContainer __instance, InventoryItem item)
            {
                if (Main.gameLoaded == false)
                    return;

                //AddDebug("uGUI_ItemsContainer OnAddItem " + item.item.GetTechName());
                if (__instance.container._label != "InventoryLabel")
                    return;
                //AddDebug(" uGUI_ItemsContainer label " + __instance.container._label);
                //AddDebug("uGUI_ItemsContainer OnAddItem " + item.item.GetTechName());
                IItemsContainer openContainer = Util.GetOpenContainer();
                if (openContainer == null)
                    return;

                uGUI_ItemIcon icon = __instance.items[item];
                if (icon == null)
                    return;

                if (openContainer.AllowedToAdd(item.item, false))
                    icon.SetChroma(1);
                else
                    icon.SetChroma(0);

                if (IsCharger(openContainer))
                    DoBattery(item, icon);
            }
        }

        private static void DoBattery(InventoryItem item, uGUI_ItemIcon icon)
        {
            Battery battery = item.item.GetComponent<Battery>();
            //if (battery != null)
            //    AddDebug($"battery.charge {battery.charge} battery.capacity {battery.capacity}");

            if (battery && battery.charge == battery.capacity)
                icon.SetChroma(0f);
        }

        private static bool IsCharger(IItemsContainer container)
        {
            return container.label == "PowerCellChargerLabel" || container.label == "BatteryChargerStorageLabel";
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab))]
        class uGUI_InventoryTab_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnOpenPDA")]
            static void OnOpenPDAPostfix(uGUI_InventoryTab __instance)
            {
                IItemsContainer openContainer = Util.GetOpenContainer();
                if (openContainer == null)
                    return;

                //AddDebug("label " + openContainer.label);
                Equipment equipment = openContainer as Equipment;
                ItemsContainer container = openContainer as ItemsContainer;
                //if (openContainer.label == "RecyclotronStorageLabel")
                //{
                //    AddDebug("RecyclotronStorageLabel Count " + container._items.Count);
                //}
                if (openContainer.label == "FridgeStorageLabel")
                {
                    foreach (var pair in __instance.inventory.items)
                    {
                        Eatable eatable = pair.Key.item.GetComponent<Eatable>();
                        if (!eatable)
                            pair.Value.SetChroma(0f);
                    }
                }
                else if (container != null)
                {
                    //AddDebug("container Count " + container._items.Count);
                    foreach (var pair in __instance.inventory.items)
                    {
                        //TechType tt = pair.Key.item.GetTechType();
                        //AddDebug(tt + " Allowed " + container.IsTechTypeAllowed(tt));
                        //AddDebug(tt + " Allowed " + openContainer.AllowedToAdd(pair.Key.item, false));
                        if (!openContainer.AllowedToAdd(pair.Key.item, false))
                            pair.Value.SetChroma(0f);
                    }
                }
                else if (equipment != null)
                {
                    //bool chargerOpen = equipment.GetCompatibleSlot(EquipmentType.BatteryCharger, out string s) || equipment.GetCompatibleSlot(EquipmentType.PowerCellCharger, out string ss);
                    foreach (var pair in __instance.inventory.items)
                    {
                        TechType tt = pair.Key.item.GetTechType();
                        EquipmentType itemType = TechData.GetEquipmentType(tt);
                        //AddDebug(pair.Key.item.GetTechType() + " " + itemType);
                        string slot = string.Empty;
                        if (equipment.GetCompatibleSlot(itemType, out slot))
                        {
                            //EquipmentType chargerType = Equipment.GetSlotType(slot);
                            //AddDebug(__instance.name + " Compatible eq " + tt);
                            //if (chargerType == EquipmentType.BatteryCharger || chargerType ==  EquipmentType.PowerCellCharger)
                            if (IsCharger(openContainer))
                            {
                                if (Charger_.notRechargableBatteries.Contains(tt))
                                {
                                    pair.Value.SetChroma(0f);
                                    continue;
                                }
                                DoBattery(pair.Key, pair.Value);
                            }
                        }
                        else
                            pair.Value.SetChroma(0f);
                    }
                }
            }

            [HarmonyPrefix, HarmonyPatch("OnClosePDA")]
            static void OnClosePDAPreix(uGUI_InventoryTab __instance)
            {
                IItemsContainer openContainer = Util.GetOpenContainer();
                if (openContainer != null)
                {
                    foreach (var pair in __instance.inventory.items)
                        pair.Value.SetChroma(1f);
                }
            }
        }


    }
}
