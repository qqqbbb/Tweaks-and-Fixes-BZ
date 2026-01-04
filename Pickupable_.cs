using HarmonyLib;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Pickupable_
    {
        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();
        public static HashSet<TechType> unmovableItems = new HashSet<TechType>();
        public static Dictionary<TechType, int> eatableFoodValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> eatableWaterValue = new Dictionary<TechType, int> { };
        public static Dictionary<Pickupable, StorageContainer> pickupableStorage = new Dictionary<Pickupable, StorageContainer>();
        public static Dictionary<Pickupable, PickupableStorage> pickupableStorage_ = new Dictionary<Pickupable, PickupableStorage>();

        [HarmonyPatch(typeof(Pickupable))]
        class Pickupable_Patch_
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(Pickupable __instance)
            {
                TechType tt = __instance.GetTechType();
                //Main.logger.LogDebug("Pickupable  Awake " + tt);
                if (tt == TechType.SmallStorage || tt == TechType.QuantumLocker)
                {
                    PickupableStorage ps = __instance.GetComponentInChildren<PickupableStorage>();
                    if (ps)
                        pickupableStorage_.Add(__instance, ps);

                    StorageContainer sc = __instance.GetComponentInChildren<StorageContainer>();
                    if (sc)
                        pickupableStorage.Add(__instance, sc);
                }
                if (eatableFoodValue.ContainsKey(tt))
                {
                    Util.MakeEatable(__instance.gameObject, eatableFoodValue[tt]);
                }
                if (eatableWaterValue.ContainsKey(tt))
                {
                    Util.MakeDrinkable(__instance.gameObject, eatableWaterValue[tt]);
                }
                if (unmovableItems.Contains(tt))
                { // isKinematic gets saved
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                if (itemMass.ContainsKey(tt))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                        rb.mass = itemMass[tt];
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnHandHover")]
            static void OnHandHoverPostfix(Pickupable __instance, GUIHand hand)
            {
                //AddDebug("Pickupable OnHandHover " + __instance.name);
                Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                if (ConfigToEdit.canPickUpContainerWithItems.Value == false && exosuit && pickupableStorage_.ContainsKey(__instance))
                {
                    //AddDebug(__instance.name + " Pickupable OnHandHover AllowedToPickUp " + __instance.AllowedToPickUp());
                    if (__instance.AllowedToPickUp() == false)
                    {
                        PickupableStorage ps = pickupableStorage_[__instance];
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, Language.main.Get(ps.cantPickupClickText), true);
                    }
                }
                HandleBeaconText(__instance);
            }

            [HarmonyPostfix, HarmonyPatch("AllowedToPickUp")]
            public static void AllowedToPickUpPostfix(Pickupable __instance, ref bool __result)
            {
                if (pickupableStorage.ContainsKey(__instance))
                { // fix bug: exosuit can pick up containers with items
                    if (ConfigToEdit.canPickUpContainerWithItems.Value)
                        __result = true;
                    else
                        __result = pickupableStorage[__instance].container.IsEmpty();
                    //AddDebug(__instance.name + " Pickupable AllowedToPickUp " + __result);
                    return;
                }
            }

            private static void HandleBeaconText(Pickupable pickupable)
            {
                if (pickupable.name != "Beacon(Clone)" || pickupable.AllowedToPickUp() == false)
                    return;

                Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                if (exosuit && !exosuit.HasClaw())
                    return;

                string text1 = LanguageCache.GetPickupText(TechType.Beacon);
                string text2 = string.Empty;
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                BeaconLabel beaconLabel = pickupable.GetComponentInChildren<BeaconLabel>();
                if (beaconLabel)
                {
                    if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                        uGUI.main.userInput.RequestString(beaconLabel.stringBeaconLabel, beaconLabel.stringBeaconSubmit, beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beaconLabel.SetLabel));

                    text2 = beaconLabel.labelName;
                }
                StringBuilder stringBuilder = new StringBuilder(text1);
                stringBuilder.Append(UI_Patches.beaconPickString);
                HandReticle.main.SetText(HandReticle.TextType.Hand, stringBuilder.ToString(), false, GameInput.Button.Deconstruct);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
            }
        }

        [HarmonyPatch(typeof(ExosuitClawArm))]
        public static class ExosuitClawArm_Patch
        {
            [HarmonyPrefix, HarmonyPatch("OnPickup")]
            public static bool OnPickupPrefix(ExosuitClawArm __instance)
            {
                if (ConfigToEdit.canPickUpContainerWithItems.Value)
                    return true;

                GameObject target = __instance.exosuit.GetActiveTarget();
                if (target)
                {
                    Pickupable p = target.GetComponent<Pickupable>();
                    //AddDebug("ExosuitClawArm OnPickup pickupableStorage " + pickupableStorage.ContainsKey(p));
                    if (p && pickupableStorage.ContainsKey(p))
                    {
                        bool empty = pickupableStorage[p].IsEmpty();
                        //AddDebug("ExosuitClawArm OnPickup pickupableStorage " + empty);
                        return empty;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("GetItemAction")]
            static void GetItemActionPostfix(Inventory __instance, ref ItemAction __result, InventoryItem item, int button)
            {
                //AddDebug("GetItemAction button " + button + " " + item.item.name + " " + __result);
                Pickupable pickupable = item.item;
                //TechType tt = pickupable.GetTechType();
                if (__result == ItemAction.Eat)
                {
                    Eatable eatable = pickupable.gameObject.GetComponent<Eatable>();
                    if (ConfigMenu.cantEatUnderwater.Value && Player.main.IsUnderwater())
                        __result = ItemAction.None;
                    else if (Util.IsWater(eatable) && eatable.GetWaterValue() < 0.5f)
                        __result = ItemAction.None;
                }
                else if (__result == ItemAction.Use && ConfigMenu.cantUseMedkitUnderwater.Value && Player.main.IsUnderwaterForSwimming() && pickupable.GetTechType() == TechType.FirstAidKit)
                {
                    //AddDebug("cantUseMedkitUnderwater");
                    __result = ItemAction.None;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("ExecuteItemAction", new Type[] { typeof(ItemAction), typeof(InventoryItem) })]
            static void ExecuteItemActionPrefix(Inventory __instance, ref ItemAction action, InventoryItem item)
            {
                //AddDebug(item.item.name + " ExecuteItemAction " + action);
                if (ConfigMenu.cantUseMedkitUnderwater.Value && action == ItemAction.Use && item.item.GetTechType() == TechType.FirstAidKit && Player.main.IsUnderwaterForSwimming())
                {
                    //AddDebug("ExecuteItemAction FirstAidKit ");
                    action = ItemAction.None;
                }
            }
        }


        //[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
        class TooltipFactory_ItemActions_Patch
        { // for some items UI did not tell they can be dropped 
            internal static bool Prefix(StringBuilder sb, global::InventoryItem item)
            {
                //AddDebug("ItemActions " + item.item.name);
                bool canBindItem = Inventory.main.GetCanBindItem(item) && GameInput.IsKeyboardAvailable();
                ItemAction itemAction1 = Inventory.main.GetItemAction(item, 0);
                ItemAction itemAction2 = Inventory.main.GetItemAction(item, 1);
                ItemAction itemAction3 = Inventory.main.GetItemAction(item, 2);
                ItemAction itemAction4 = Inventory.main.GetItemAction(item, 3);
                bool usingController = GameInput.GetPrimaryDevice() == GameInput.Device.Controller;
                //if (!canBindItem && (itemAction1 | itemAction3 | itemAction4) == ItemAction.None)
                //if (itemAction1 == ItemAction.None && itemAction2 == ItemAction.None && itemAction3 == ItemAction.None && itemAction4 == ItemAction.None)
                //{
                //    AddDebug("return");
                //    return false;
                //}

                if (canBindItem && !usingController)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringKeyRange15, TooltipFactory.stringBindQuickSlot);
                if (itemAction4 != ItemAction.None)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringButton3, TooltipFactory.GetUseActionString(itemAction4));
                if (itemAction1 != ItemAction.None)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringButton0, TooltipFactory.GetUseActionString(itemAction1));
                if (itemAction3 != ItemAction.None)
                    TooltipFactory.WriteAction(sb, TooltipFactory.stringButton2, TooltipFactory.GetUseActionString(itemAction3));
                if (itemAction2 == ItemAction.None)
                    return false;
                //AddDebug("WriteAction");
                TooltipFactory.WriteAction(sb, TooltipFactory.stringButton1, TooltipFactory.GetUseActionString(itemAction2));
                return false;
            }
        }

    }
}
