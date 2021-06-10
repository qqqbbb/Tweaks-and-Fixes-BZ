using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Storage_Patch
    {
        public static string GetText(ColoredLabel label, PickupableStorage ps, StorageContainer sc, GUIHand hand)
        {
            string text = string.Empty;
            if (sc)
            {
                text = HandReticle.main.GetText(sc.hoverText, true, GameInput.Button.LeftHand);
            }
            if (label && label.enabled)
            {
                text += "\n" + HandReticle.main.GetText(label.stringEditLabel, true, GameInput.Button.RightHand);
            }
            if (ps)
            {
                if (ps.storageContainer.IsEmpty() || ps.allowPickupWhenNonEmpty)
                    //ps.pickupable.OnHandHover(hand);
                    text += "\n" + OnPickupableHandHover(ps.pickupable, hand);
                else if (!string.IsNullOrEmpty(ps.cantPickupHoverText))
                {
                    text += "\n" + HandReticle.main.GetText(ps.cantPickupHoverText, true);
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, ps.cantPickupHoverText, true);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                }
            }
            return text;
        }

        public static void processInput(ColoredLabel label, PickupableStorage ps, StorageContainer sc, GUIHand hand)
        {
            if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
            {
                if (sc)
                    sc.Open(sc.transform);
                //AddDebug("LeftHand");
            }
            else if (GameInput.GetButtonDown(GameInput.Button.RightHand))
            {
                if (label && label.enabled)
                    label.signInput.Select(true);
                //AddDebug("RightHand");
            }
            else if (GameInput.GetButtonDown(GameInput.Button.AltTool))
            {
                if (ps.storageContainer.IsEmpty() || ps.allowPickupWhenNonEmpty)
                    ps.pickupable.OnHandClick(hand);
                //AddDebug("AltTool");
            }
        }

        public static string OnPickupableHandHover(Pickupable pickupable, GUIHand hand)
        {
            HandReticle handReticle = HandReticle.main;
            if (!hand.IsFreeToInteract())
                return string.Empty;

            string text1 = string.Empty;
            string text2 = string.Empty;
            TechType techType = pickupable.GetTechType();
            GameInput.Button button = GameInput.Button.AltTool;

            if (pickupable.AllowedToPickUp())
            {
                Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                bool canPickup = exosuit == null || exosuit.HasClaw();
                if (canPickup)
                {
                    ISecondaryTooltip secTooltip = pickupable.gameObject.GetComponent<ISecondaryTooltip>();
                    if (secTooltip != null)
                        text2 = secTooltip.GetSecondaryTooltip();
                    text1 = pickupable.usePackUpIcon ? LanguageCache.GetPackUpText(techType) : LanguageCache.GetPickupText(techType);
                    //handReticle.SetIcon(pickupable.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
                }
                if (exosuit)
                {
                    //button = canPickup ? GameInput.Button.LeftHand : GameInput.Button.None;
                    //if (exosuit.leftArmType != TechType.ExosuitClawArmModule)
                    //    button = GameInput.Button.RightHand;
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, button);

                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                }
                else
                {
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, GameInput.Button.LeftHand);
                    //button = GameInput.Button.RightHand;
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                }
            }
            else if (pickupable.isPickupable && !Player.main.HasInventoryRoom(pickupable))
            {
                //handReticle.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                handReticle.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", true);
            }
            else
            {
                //handReticle.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                handReticle.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
            }
            text1 = HandReticle.main.GetText(text1, true, button);
            return text1;
        }

        public static StorageContainer GetSeaTruckStorage(GameObject seatruck, ColoredLabel label)
        {
            //AddDebug("GetSeaTruckStorage");
            StorageContainer[] containers = Main.GetComponentsInDirectChildren<StorageContainer>(seatruck);
            foreach (StorageContainer c in containers)
            {
                if (label.name == "Label" && c.name == "StorageContainer (2)")
                    return c;
                else if (label.name == "Label (1)" && c.name == "StorageContainer (3)")
                    return c;
                else if (label.name == "Label (2)" && c.name == "StorageContainer")
                    return c;
                else if (label.name == "Label (3)" && c.name == "StorageContainer (4)")
                    return c;
                else if (label.name == "Label (4)" && c.name == "StorageContainer (1)")
                    return c;
            }
            return null;
        }

        public static ColoredLabel GetSeaTruckLabel(GameObject seatruck, StorageContainer container)
        {
            //AddDebug("GetSeaTruckLabel");
            ColoredLabel[] labels = Main.GetComponentsInDirectChildren<ColoredLabel>(seatruck);
            foreach (ColoredLabel l in labels)
            {
                if (l.name == "Label" && container.name == "StorageContainer (2)")
                    return l;
                else if (l.name == "Label (1)" && container.name == "StorageContainer (3)")
                    return l;
                else if (l.name == "Label (2)" && container.name == "StorageContainer")
                    return l;
                else if (l.name == "Label (3)" && container.name == "StorageContainer (4)")
                    return l;
                else if (l.name == "Label (4)" && container.name == "StorageContainer (1)")
                    return l;
            }
            return null;
        }

        //[HarmonyPatch(typeof(SeaTruckSegment), "Start")]
        class SeaTruckSegment_Start_patch
        {
            public static void Postfix(SeaTruckSegment __instance)
            {
                //Main.Log("SeaTruckSegment " + __instance.name);
                if (__instance.name == "SeaTruckStorageModule(Clone)")
                {
                    StorageContainer[] containers = Main.GetComponentsInDirectChildren<StorageContainer>(__instance);
                    ColoredLabel[] labels = Main.GetComponentsInDirectChildren<ColoredLabel>(__instance);
                    Main.Log("SeaTruck containers " + containers.Length);
                    int i = 0;
                    foreach (StorageContainer c in containers)
                    {
                        //int pos = (int)(c.transform.position.x + c.transform.position.y + c.transform.position.z);
                        string id = c.GetComponent<ChildObjectIdentifier>().Id;
                        Main.Log("container " + i + " " + c.name);
                        i++;
                    }
                    i = 0;
                    Main.Log("SeaTruck labels " + labels.Length);
                    foreach (ColoredLabel l in labels)
                    {
                        //int pos = (int)(l.transform.position.x + l.transform.position.y + l.transform.position.z);
                        string id = l.GetComponent<ChildObjectIdentifier>().Id;
                        Main.Log("label " + i + " " + l.name);
                        i++;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StorageContainer), "OnHandHover")]
        class StorageContainer_OnHandHover_patch
        {
            public static bool Prefix(StorageContainer __instance, GUIHand hand)
            {
                //AddDebug("StorageContainer OnHandHover name " + __instance.name);
                //HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, "Subscript");
                // HandReticle.main.SetTextRaw(HandReticle.TextType.Use, "Use");
                //str = LanguageCache.GetButtonFormat("AirBladderUseTool", GameInput.Button.RightHand);
                if (!__instance.enabled || __instance.disableUseability)
                    return false;
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (c && !c.constructed)
                    return false;

                GameObject parent = Main.GetParent(__instance.gameObject);

                //GameObject parent = __instance.transform.parent.gameObject;
                ColoredLabel label = null;
                PickupableStorage ps = null;
                if (parent.name == "SeaTruckStorageModule(Clone)")
                    label = GetSeaTruckLabel(parent, __instance);
                else
                {
                    label = parent.GetComponentInChildren<ColoredLabel>();
                    ps = parent.GetComponentInChildren<PickupableStorage>();
                }
                //if (label)
                    //AddDebug("StorageContainer OnHandHover label");
                //if (ps)
                    //AddDebug("StorageContainer OnHandHover PickupableStorage");
                string text = GetText(label, ps, __instance, hand);
                 //string text = HandReticle.main.GetText(__instance.hoverText, true, GameInput.Button.LeftHand);
                 //HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.hoverText, true, GameInput.Button.LeftHand);
                 //HandReticle.main.SetText(HandReticle.TextType.HandSubscript, __instance.IsEmpty() ? "Empty" : string.Empty, true);

                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, text);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                processInput(label, ps, __instance, hand);
                return false;
            }
        }

        //[HarmonyPatch(typeof(ColoredLabel), "OnEnable")]
        class ColoredLabel_OnEnable_patch
        {
            public static void Postfix(ColoredLabel __instance)
            {
                BoxCollider collider = __instance.GetComponent<BoxCollider>();
                if (collider)
                {
                    AddDebug("Destroy collider");
                    UnityEngine.Object.Destroy(collider);
                }
            }
        }

        [HarmonyPatch(typeof(ColoredLabel), "OnHandHover")]
        class ColoredLabel_OnHandHover_patch
        {
            public static bool Prefix(ColoredLabel __instance, GUIHand hand)
            {
                //AddDebug("ColoredLabel OnHandHover name " + __instance.name);
                GameObject parent = Main.GetParent(__instance.gameObject);
                StorageContainer container = null;
                PickupableStorage ps = null;
                if (parent.name == "SeaTruckStorageModule(Clone)")
                    container = GetSeaTruckStorage(parent, __instance);
                else
                {
                    container = parent.GetComponentInChildren<StorageContainer>();
                    ps = parent.GetComponentInChildren<PickupableStorage>();
                }
                if (container && container.enabled && !container.disableUseability)
                {
                    Constructable c = container.gameObject.GetComponent<Constructable>();
                    if (c && !c.constructed)
                        container = null;
                        //AddDebug("ColoredLabel container ");
                }
                if (!container)
                    return false;

                //if (container)
                //    AddDebug("ColoredLabel OnHandHover container");
                //if (ps)
                //    AddDebug("ColoredLabel OnHandHover PickupableStorage");
                string text = GetText(__instance, ps, container, hand);
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, text);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                processInput(__instance, ps, container, hand);
                return false;
            }
        }

        [HarmonyPatch(typeof(PickupableStorage), "OnHandHover")]
        class PickupableStorage_OnHandHover_patch
        {
            public static bool Prefix(PickupableStorage __instance, GUIHand hand)
            {
                //AddDebug("PickupableStorage OnHandHover");
                GameObject parent = Main.GetParent(__instance.gameObject);
                StorageContainer container = parent.GetComponentInChildren<StorageContainer>();
                if (container && container.enabled && !container.disableUseability)
                {
                    Constructable c = container.gameObject.GetComponent<Constructable>();
                    if (c && !c.constructed)
                        container = null;
                    //AddDebug("ColoredLabel container ");
                }
                if (!container)
                    return false;

                ColoredLabel label = parent.GetComponentInChildren<ColoredLabel>();
                string text = GetText(label, __instance, container, hand);
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, text);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                processInput(label, __instance, container, hand);
                return false;
            }
        }

        [HarmonyPatch(typeof(PickupableStorage), "OnHandClick")]
        class PickupableStorage_OnHandClick_patch
        {
            public static bool Prefix(PickupableStorage __instance, GUIHand hand)
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(ColoredLabel), "OnHandClick")]
        class ColoredLabel_OnHandClick_patch
        {
            public static bool Prefix(ColoredLabel __instance, GUIHand hand)
            {
                return false;
            }
        }
        [HarmonyPatch(typeof(StorageContainer), "OnHandClick")]
        class StorageContainer_OnHandClick_patch
        {
            public static bool Prefix(StorageContainer __instance, GUIHand guiHand)
            {
                return false;
            }
        }
        //[HarmonyPatch(typeof(Pickupable), "OnHandHover")]
        class Pickupable_OnHandHover_patch
        {
            public static bool Prefix(Pickupable __instance, GUIHand hand)
            {
                //AddDebug("ColoredLabel OnHandHover");
                HandReticle main = HandReticle.main;
                if (!hand.IsFreeToInteract())
                    return false;
                TechType techType = __instance.GetTechType();
                if (__instance.AllowedToPickUp())
                {
                    string text1 = string.Empty;
                    string text2 = string.Empty;
                    Exosuit vehicle = Player.main.GetVehicle() as Exosuit;
                    bool canPickup = vehicle == null || vehicle.HasClaw();
                    if (canPickup)
                    {
                        //AddDebug("Pickupable OnHandHover flag");
                        ISecondaryTooltip secTooltip = __instance.gameObject.GetComponent<ISecondaryTooltip>();
                        if (secTooltip != null)
                            text2 = secTooltip.GetSecondaryTooltip();
                        AddDebug("Pickupable OnHandHover flag text2 " + text2); // null
                        text1 = __instance.usePackUpIcon ? LanguageCache.GetPackUpText(techType) : LanguageCache.GetPickupText(techType);
                        AddDebug("Pickupable OnHandHover flag text1 " + text1); // pack up
                        main.SetIcon(__instance.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
                    }
                    if (vehicle)
                    {
                        AddDebug("Pickupable OnHandHover vehicle");
                        GameInput.Button button = canPickup ? GameInput.Button.LeftHand : GameInput.Button.None;
                        if (vehicle.leftArmType != TechType.ExosuitClawArmModule)
                            button = GameInput.Button.RightHand;
                        HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, button);
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                    }
                    else
                    {
                        AddDebug("Pickupable OnHandHover ");
                        HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, GameInput.Button.LeftHand);
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                    }
                }
                else if (__instance.isPickupable && !Player.main.HasInventoryRoom(__instance))
                {
                    AddDebug("Pickupable OnHandHover no Room");
                    main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                    main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", true);
                }
                else
                {
                    AddDebug("Pickupable OnHandHover Room");
                    main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                    main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                }
                return false;
            }
        }

    }
}
