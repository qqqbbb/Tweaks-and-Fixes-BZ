
using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class QuickSlots_Patch
    {
        static HashSet<TechType> equipped;
        static Queue<InventoryItem> toEquip;
        static HashSet<TechType> toEquipTT;
        public static bool invChanged = true;

        public static void GetTools()
        {
            toEquip = new Queue<InventoryItem>();
            toEquipTT = new HashSet<TechType>();
            GetEquippedTools();
            //Main.Log("GetTools " );
            foreach (InventoryItem item in Inventory.main.container)
            {
                if (item.item.GetComponent<PlayerTool>() && !item.item.GetComponent<Eatable>())
                { // eatable fish is PlayerTool
                    TechType techType = item.item.GetTechType();
                    if (!equipped.Contains(techType) && !toEquipTT.Contains(techType))
                    {
                        toEquip.Enqueue(item);
                        toEquipTT.Add(techType);
                        //AddDebug("toEqiup " + techType);
                        //Main.Log("toEqiup " + techType);
                    }
                }
            }
        }

        public static void GetEquippedTools()
        {
            equipped = new HashSet<TechType>();
            //Main.Log("GetEquippedTools");
            foreach (TechType item in Inventory.main.quickSlots.GetSlotBinding())
            {
                equipped.Add(item);
                //Main.Log("eqiupped " + item);
            }
        }

        private static void EquipNextTool()
        {
            if (invChanged)
            {
                GetTools();
                invChanged = false;
            }
            int activeSlot = Inventory.main.quickSlots.activeSlot;
            InventoryItem currentItem = Inventory.main.quickSlots.binding[activeSlot];
            //if (currentItem == null) 
            //    AddDebug("currentItem == null ");
            //AddDebug("currentItem " + currentItem.item.GetTechName());
            //AddDebug("toEqiup Remove " + toEqiup.Peek().item.GetTechName());
            Inventory.main.quickSlots.Bind(activeSlot, toEquip.Peek());
            toEquip.Dequeue();
            toEquip.Enqueue(currentItem);
            Inventory.main.quickSlots.SelectImmediate(activeSlot);
            //GetEquippedTools();
        }

        [HarmonyPatch(typeof(Inventory))]
        internal class Inventory_OnAddItem_Patch
        { // when this called during loading returned tools are wrong
            [HarmonyPostfix]
            [HarmonyPatch("OnAddItem")]
            public static void OnAddItemPostfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.isBindable)
                {
                    //GetTools();
                    invChanged = true;
                    //AddDebug("Inventory OnAddItem ");
                }
            }
          
            [HarmonyPostfix]
            [HarmonyPatch("OnRemoveItem")]
            public static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.isBindable)
                {
                    //GetTools();
                    invChanged = true;
                    //AddDebug("Inventory OnRemoveItem ");
                }
            }
        }

        //[HarmonyPatch(typeof(PDA), "Close")]
        internal class PDA_Close_Patch
        {
            public static void Postfix(PDA __instance)
            {
                //GetEquippedTools();
                invChanged = false;
                //AddDebug("PDA Close ");
            }
        }

        [HarmonyPatch(typeof(QuickSlots))]
        internal class QuickSlots_Bind_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Bind")]
            public static void BindPostfix(QuickSlots __instance)
            {
                GetEquippedTools();
                //AddDebug(" Bind ");
            }
            [HarmonyPrefix]
            [HarmonyPatch("SlotNext")]
            public static bool SlotNextPrefix(QuickSlots __instance)
            {
                if (Input.GetKey(Main.config.quickslotKey))
                {
                    Pickupable pickupable = Inventory.main.GetHeld();
                    if (pickupable != null)
                    {
                        EquipNextTool();
                        return false;
                    }
                }
                return true;
            }
            [HarmonyPrefix]
            [HarmonyPatch("SlotPrevious")]
            public static bool SlotPreviousPrefix(QuickSlots __instance)
            {
                if (Input.GetKey(Main.config.quickslotKey) && Inventory.main.GetHeld() != null)
                {
                    EquipNextTool();
                    return false;
                }
                return true;
            }
        }


    }
}
