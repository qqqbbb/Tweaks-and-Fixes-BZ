
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
        static HashSet<TechType> eqiupped;
        static Queue<InventoryItem> toEqiup;
        static HashSet<TechType> toEqiupTT;
        public static bool invChanged = true;

        public static void GetTools()
        {
            toEqiup = new Queue<InventoryItem>();
            toEqiupTT = new HashSet<TechType>();
            GetEquippedTools();
            //Main.Log("GetTools " );
            foreach (InventoryItem item in Inventory.main.container)
            {
                if (item.item.GetComponent<PlayerTool>() && !item.item.GetComponent<Eatable>())
                { // eatable fish is PlayerTool
                    TechType techType = item.item.GetTechType();
                    if (!eqiupped.Contains(techType) && !toEqiupTT.Contains(techType))
                    {
                        toEqiup.Enqueue(item);
                        toEqiupTT.Add(techType);
                        //AddDebug("toEqiup " + techType);
                        //Main.Log("toEqiup " + techType);
                    }
                }
            }
        }

        public static void GetEquippedTools()
        {
            eqiupped = new HashSet<TechType>();
            //Main.Log("GetEquippedTools");
            foreach (TechType item in Inventory.main.quickSlots.GetSlotBinding())
            {
                eqiupped.Add(item);
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
            Inventory.main.quickSlots.Bind(activeSlot, toEqiup.Peek());
            toEqiup.Dequeue();
            toEqiup.Enqueue(currentItem);
            Inventory.main.quickSlots.SelectImmediate(activeSlot);
            //GetEquippedTools();
        }

        [HarmonyPatch(typeof(Inventory), "OnAddItem")]
        internal class Inventory_OnAddItem_Patch
        { // this called during loading and tools returned are wrong
            public static void Postfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.isBindable)
                {
                    //GetTools();
                    invChanged = true;
                    //AddDebug("Inventory OnAddItem ");
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "OnRemoveItem")]
        internal class Inventory_OnRemoveItem_Patch
        {
            public static void Postfix(Inventory __instance, InventoryItem item)
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

        [HarmonyPatch(typeof(QuickSlots), "Bind")]
        internal class QuickSlots_Bind_Patch
        {
            public static void Postfix(QuickSlots __instance)
            {
                GetEquippedTools();
                //AddDebug(" Bind ");
            }
        }

        [HarmonyPatch(typeof(QuickSlots), "SlotNext")]
        internal class QuickSlots_SlotNext_Patch
        {
            public static bool Prefix(QuickSlots __instance)
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
        }

        [HarmonyPatch(typeof(QuickSlots), "SlotPrevious")]
        internal class QuickSlots_SlotPrevious_Patch
        {
            public static bool Prefix(QuickSlots __instance)
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
        }

    }
}
