using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Pickupable_Patch
    {
        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();
        public static HashSet<TechType> unmovableItems = new HashSet<TechType>();
        public static Dictionary<TechType, int> eatableFoodValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> eatableWaterValue = new Dictionary<TechType, int> { };


        [HarmonyPatch(typeof(Pickupable))]
        class Pickupable_Patch_
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(Pickupable __instance)
            {
                TechType tt = __instance.GetTechType();
                //Main.logger.LogDebug("Pickupable  Awake " + tt);

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

            [HarmonyPrefix]
            [HarmonyPatch("OnHandHover")]
            static bool OnHandHoverPrefix(Pickupable __instance, GUIHand hand)
            {
                HandReticle main = HandReticle.main;
                if (!hand.IsFreeToInteract())
                    return false;

                TechType techType = __instance.GetTechType();
                if (__instance.AllowedToPickUp())
                {
                    string text1 = string.Empty;
                    string text2 = string.Empty;
                    Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                    bool canPickUp = exosuit == null || exosuit.HasClaw();
                    if (canPickUp)
                    {
                        ISecondaryTooltip component = __instance.gameObject.GetComponent<ISecondaryTooltip>();
                        if (component != null)
                            text2 = component.GetSecondaryTooltip();
                        text1 = __instance.usePackUpIcon ? LanguageCache.GetPackUpText(techType) : LanguageCache.GetPickupText(techType);
                        main.SetIcon(__instance.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
                    }
                    if (exosuit)
                    {
                        GameInput.Button button = canPickUp ? GameInput.Button.LeftHand : GameInput.Button.None;
                        if (exosuit.leftArmType != TechType.ExosuitClawArmModule)
                            button = GameInput.Button.RightHand;
                        HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, button);
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                    }
                    else if (techType == TechType.Beacon)
                    {
                        HandleBeaconText(__instance, text1, text2);
                    }
                    else
                    {
                        HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, GameInput.Button.LeftHand);
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                    }
                }
                else if (__instance.isPickupable && !Player.main.HasInventoryRoom(__instance))
                {
                    main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                    main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", true);
                }
                else
                {
                    main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                    main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                }
                return false;
            }

            private static void HandleBeaconText(Pickupable pickupable, string text1, string text2)
            {
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


        //[HarmonyPatch(typeof(Survival), "Use")]
        class Survival_Awake_Patch
        {
            static bool Prefix(Survival __instance, GameObject useObj, ref bool __result, Inventory inventory)
            {
                __result = false;
                if (useObj == null)
                    return false;

                TechType techType = CraftData.GetTechType(useObj);
                //AddDebug("Use" + techType);
                if (techType == TechType.None)
                {
                    Pickupable p = useObj.GetComponent<Pickupable>();
                    if (p)
                        techType = p.GetTechType();
                }
                if (techType == TechType.FirstAidKit)
                {
                    if (Player.main.GetComponent<LiveMixin>().health > 99.9f)
                        AddMessage(Language.main.Get("HealthFull"));
                    else
                    {
                        __result = true;
                        if (ConfigToEdit.medKitHPperSecond.Value >= ConfigMenu.medKitHP.Value)
                        {
                            Player.main.GetComponent<LiveMixin>().AddHealth(ConfigMenu.medKitHP.Value);
                        }
                        else
                        {
                            Main.configMain.SetHPtoHeal(ConfigMenu.medKitHP.Value);
                            //healTime = Time.time;
                        }
                    }
                }
                else if (techType == TechType.WaterPurificationTablet && inventory.DestroyItem(TechType.SnowBall))
                {
                    __instance.StartCoroutine(CraftData.AddToInventoryAsync(TechType.BigFilteredWater, (IOut<GameObject>)DiscardTaskResult<GameObject>.Instance));
                    __result = true;
                }
                if (__result)
                {
                    FMODAsset useSound = Player.main.GetUseSound(TechData.GetSoundType(techType));
                    if (useSound)
                        Utils.PlayFMODAsset(useSound, Player.main.transform.position);
                }
                return false;
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
