using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;
using static HandReticle;

namespace Tweaks_Fixes
{
    class UI_Patches
    {


        static List<TechType> fishTechTypes = new List<TechType> { TechType.Bladderfish, TechType.Boomerang, TechType.ArcticPeeper, TechType.DiscusFish, TechType.FeatherFish, TechType.Hoopfish, TechType.FeatherFishRed, TechType.SpinnerFish, TechType.NootFish, TechType.Symbiote, TechType.Spinefish, TechType.ArrowRay, TechType.Triops };


        static public string rightHandButton = string.Empty;
        static public string leftHandButton = string.Empty;
        static public string altToolButton = string.Empty;
        static public string beaconToolString = string.Empty;
        static public string beaconPickString = string.Empty;
        static public string fishDropString = string.Empty;
        static public string fishEatString = string.Empty;
        static public string lightFlareString = string.Empty;
        static public string throwFlareString = string.Empty;
        static public string lightAndThrowFlareString = string.Empty;
        static public string toggleBaseLights = string.Empty;
        static public string changeTorpedoExosuitButtonKeyboard = string.Empty;
        static public string cycleNextButton = string.Empty;
        static public string cyclePrevButton = string.Empty;
        static public string slot1Button = string.Empty;
        static public string slot2Button = string.Empty;
        static public string slot1Plus2Button = string.Empty;
        static public string exosuitLightsButton = string.Empty;
        static public string moveRightButton = string.Empty;
        static public string moveLeftButton = string.Empty;
        static public string swivelText = string.Empty;
        static public string deconstructButton = string.Empty;
        static public string exosuitChangeTorpedoButton = string.Empty;
        static public string propCannonEatString = string.Empty;
        static public string pickupString = string.Empty;
        static public string constructorString = string.Empty;
        static public string bladderfishTooltip = Language.main.Get("Tooltip_Bladderfish") + Language.main.Get("TF_bladderfish_tooltip");

        private static void HandleBaseLights(SubRoot subRoot)
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, toggleBaseLights);
            if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                Base_Patch.ToggleBaseLight(subRoot);
        }

        static void SetTooltips()
        {
            LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, bladderfishTooltip);
            LanguageHandler.SetTechTypeTooltip(TechType.SmallStove, Language.main.Get("TF_smallStove_tooltip"));
            // vanilla desc just copies the name
            LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeHorsePower, Language.main.Get("TF_SeaTruckUpgradeHorsePower_tooltip"));
            // vanilla desc does not tell percent
            LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeEnergyEfficiency, Language.main.Get("TF_SeaTruckUpgradeEnergyEfficiency_tooltip"));
        }

        static void GetStrings()
        {
            //AddDebug("GetStrings");
            SetTooltips();
            deconstructButton = uGUI.FormatButton(GameInput.Button.Deconstruct);
            exosuitLightsButton = ", " + LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.MoveDown);
            altToolButton = uGUI.FormatButton(GameInput.Button.AltTool);
            rightHandButton = uGUI.FormatButton(GameInput.Button.RightHand);
            leftHandButton = uGUI.FormatButton(GameInput.Button.LeftHand);
            moveLeftButton = uGUI.FormatButton(GameInput.Button.MoveLeft);
            moveRightButton = uGUI.FormatButton(GameInput.Button.MoveRight);

            pickupString = Language.main.Get("PickUp");
            pickupString = pickupString.Substring(0, pickupString.IndexOf('{')).Trim();
            propCannonEatString = TooltipFactory.stringEat + " (" + deconstructButton + ")";
            fishDropString = TooltipFactory.stringDrop + " (" + rightHandButton + ")";
            fishEatString = TooltipFactory.stringEat + " (" + altToolButton + ")";
            lightFlareString = Language.main.Get("TF_light_flare") + " (" + altToolButton + ")";
            throwFlareString = Language.main.Get("TF_throw_flare") + " (" + rightHandButton + ")";
            swivelText = Language.main.Get("TF_swivel_chair_left") + " (" + moveLeftButton + ")  " + Language.main.Get("TF_swivel_chair_right") + " (" + moveRightButton + ")";
            lightAndThrowFlareString = Language.main.Get("TF_light_and_throw_flare") + " (" + rightHandButton + ")";
            beaconToolString = TooltipFactory.stringDrop + " (" + rightHandButton + ")  " + Language.main.Get("BeaconLabelEdit") + " (" + deconstructButton + ")";
            beaconPickString = "(" + leftHandButton + ")\n" + Language.main.Get("BeaconLabelEdit");
            toggleBaseLights = Language.main.Get("TF_toggle_base_lights") + " (" + deconstructButton + ")";
            cycleNextButton = uGUI.FormatButton(GameInput.Button.CycleNext);
            cyclePrevButton = uGUI.FormatButton(GameInput.Button.CyclePrev);
            //changeTorpedoExosuitButtonGamepad = Main.config.translatableStrings[14] + "(" + altToolButton + ")" + Main.config.translatableStrings[15] + "(" + cycleNextButton + "), " + "(" + cyclePrevButton + ")" + Main.config.translatableStrings[16];
            slot1Button = "(" + uGUI.FormatButton(GameInput.Button.Slot1) + ")";
            slot2Button = "(" + uGUI.FormatButton(GameInput.Button.Slot2) + ")";
            slot1Plus2Button = slot1Button + slot2Button;
            exosuitChangeTorpedoButton = Language.main.Get("TF_change_torpedo") + "(" + deconstructButton + ")";
            constructorString = Language.main.Get("Climb") + "(" + leftHandButton + "), " + LanguageCache.GetPackUpText(TechType.Constructor) + " (" + rightHandButton + ")";
        }

        [HarmonyPatch(typeof(Fridge), "OnEnable")]
        class Fridge_OnEnable_Patch
        {
            static void Postfix(Fridge __instance)
            {
                Main.fridges.Add(__instance.storageContainer.container);
            }
        }

        [HarmonyPatch(typeof(GUIHand))]
        class GUIHand_Patch
        {
            public static void HandlePickupableResource(GUIHand guiHand, TechType techType, PlayerTool olayerTool)
            {
                //AddDebug("HandlePickupableResource");
                Rigidbody rb = guiHand.activeTarget.GetComponent<Rigidbody>();
                if (rb == null || !rb.isKinematic) // attached to terrain
                {
                    //if (techType != TechType.None)
                    HandReticle.main.SetText(TextType.Hand, techType.AsString(), true);
                    GUIHand.Send(guiHand.activeTarget, HandTargetEventType.Hover, guiHand);
                }
                else if (olayerTool is Knife)
                {
                    //HandReticle.main.SetText(HandReticle.TextType.Hand, Main.config.translatableStrings[18], false, GameInput.Button.RightHand);
                }
                else
                {
                    HandReticle.main.SetTextRaw(TextType.Hand, Language.main.Get("TF_need_knife_to_break_free_resource"));
                    HandReticle.main.SetIcon(IconType.Default);
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnUpdate")]
            public static void OnUpdatePostfix(GUIHand __instance)
            {
                //AddDebug("GUIHand OnUpdate lockMovement " + FPSInputModule.current.lockMovement);
                if (Main.gameLoaded == false || FPSInputModule.current.lockMovement)
                    return;

                PlayerTool tool = __instance.GetTool();
                if (tool)
                {
                    if (__instance.activeTarget != null && !__instance.suppressTooltip)
                    {

                    }
                    if (ConfigToEdit.flareTweaks.Value)
                    {
                        Flare flare = tool as Flare;
                        if (flare)
                        {
                            string text = string.Empty;
                            bool lit = flare.flareActivateTime > 0;
                            bool canThrow = Inventory.CanDropItemHere(tool.GetComponent<Pickupable>(), false);
                            if (!lit && canThrow)
                            {
                                StringBuilder stringBuilder = new StringBuilder(lightAndThrowFlareString);
                                stringBuilder.Append(",  ");
                                stringBuilder.Append(lightFlareString);
                                text = stringBuilder.ToString();
                            }
                            else if (lit && canThrow)
                                text = throwFlareString;
                            else if (!lit && !canThrow)
                                text = lightFlareString;

                            //AddDebug("Flare text " + text);
                            //AddDebug($"lit {lit} canThrow {canThrow}");
                            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                        }
                    }
                    if (ConfigToEdit.beaconTweaks.Value)
                    {
                        Beacon beacon = tool as Beacon;
                        if (beacon)
                        {
                            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, beaconToolString);
                            if (beacon.beaconLabel && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                                uGUI.main.userInput.RequestString(beacon.beaconLabel.stringBeaconLabel, beacon.beaconLabel.stringBeaconSubmit, beacon.beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beacon.beaconLabel.SetLabel));
                        }
                    }
                }
                else if (!Main.baseLightSwitchLoaded && !Player.main.pda.isInUse && !uGUI._main.craftingMenu.selected)
                {
                    SubRoot subRoot = Player.main.currentSub;
                    if (subRoot && subRoot.isBase && subRoot.powerRelay && subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline)
                    {
                        HandleBaseLights(subRoot);
                    }
                }
                InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
                //bool canEatFish = !GameModeManager.GetOption<bool>(GameOption.VegetarianDiet) && GameModeManager.GetOption<bool>(GameOption.Hunger) || GameModeManager.GetOption<bool>(GameOption.Thirst);
                if (Util.CanEatFish() && heldItem != null && Util.IsEatableFish(heldItem.item.gameObject))
                {
                    string text = string.Empty;
                    ItemAction allItemActions = Inventory.main.GetAllItemActions(heldItem);
                    if ((allItemActions & ItemAction.Drop) != ItemAction.None)
                    {
                        text = GUIHand.GetActionString(ItemAction.Drop, heldItem.item);
                        text = HandReticle.main.GetText(text, true, GameInput.Button.RightHand);
                    }
                    if (Util.CanPlayerEat())
                    {
                        string eatText = GUIHand.GetActionString(ItemAction.Eat, heldItem.item);
                        eatText = HandReticle.main.GetText(eatText, true, GameInput.Button.AltTool);
                        if (string.IsNullOrEmpty(text))
                            text = eatText;
                        else
                        {
                            StringBuilder sb = new StringBuilder(text);
                            sb.Append(", ");
                            sb.Append(eatText);
                            text = sb.ToString();
                        }
                        if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                        {
                            Inventory.main.ExecuteItemAction(ItemAction.Eat, heldItem);
                            return;
                        }
                    }
                    HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                }

                if (__instance.activeTarget == null)
                    return;

                TechType targetTT = CraftData.GetTechType(__instance.activeTarget);
                if (targetTT == TechType.None)
                    return;

                if (ConfigMenu.noBreakingWithHand.Value && PickupablePatch.notPickupableResources.Contains(targetTT))
                {
                    HandlePickupableResource(__instance, targetTT, tool);
                }
                //AddDebug("activeTarget layer " + __instance.activeTarget.layer);
                //if (__instance.activeTarget.layer == LayerID.NotUseable)
                //    AddDebug("activeTarget Not Useable layer ");

                if (ConfigToEdit.flareTweaks.Value)
                {
                    Flare flareTarget = __instance.activeTarget.GetComponent<Flare>();
                    if (flareTarget && flareTarget.energyLeft == 0f)
                    {
                        //AddDebug("activeTarget Flare");
                        StringBuilder sb = new StringBuilder(Language.main.Get("TF_burnt_out_flare"));
                        sb.Append(Language.main.Get(targetTT));
                        HandReticle.main.SetText(HandReticle.TextType.Hand, sb.ToString(), false);
                    }
                }
                //AddDebug("OnUpdate " + __instance.activeTarget.name);
                LiveMixin liveMixin = __instance.activeTarget.GetComponentInParent<LiveMixin>();
                if (liveMixin && !liveMixin.IsAlive())
                {
                    //AddDebug("health " + liveMixin.health);
                    Pickupable pickupable = liveMixin.GetComponent<Pickupable>();
                    //CreatureEgg ce = liveMixin.GetComponent<CreatureEgg>();
                    //if (ce)
                    //    name = Language.main.Get(ce.overrideEggType);
                    string name = Language.main.Get(targetTT);
                    //AddDebug("name " + name);
                    if (pickupable)
                    {
                        if (pickupable.overrideTechType != TechType.None)
                            name = Language.main.Get(pickupable.overrideTechType);

                        name = Language.main.GetFormat<string>("DeadFormat", name);
                        //name = LanguageCache.GetPickupText(techType);
                        name = Language.main.GetFormat<string>("PickUpFormat", name);
                        HandReticle.main.SetIcon(pickupable.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
                        HandReticle.main.SetText(HandReticle.TextType.Hand, name, false, GameInput.Button.LeftHand);
                    }
                    else
                    {
                        //AddDebug("name " + name);
                        name = Language.main.GetFormat<string>("DeadFormat", name);
                        //HandReticle.main.SetInteractTextRaw(name, string.Empty);
                        HandReticle.main.SetText(HandReticle.TextType.Hand, name, true);
                    }

                }
            }

        }

        [HarmonyPatch(typeof(Targeting), "GetTarget", new Type[] { typeof(float), typeof(GameObject), typeof(float) }, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
        class Targeting_GetTarget_Patch
        {
            public static void Postfix(Targeting __instance, ref GameObject result, ref bool __result)
            {
                if (result == null)
                    return;

                TechType tt = CraftData.GetTechType(result);
                if (tt == TechType.Creepvine || tt == TechType.CreepvineSeedCluster)
                {
                    //AddDebug("GetTarget TechType " + tt);
                    if (Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment)
                    {
                        __result = false;
                        result = null;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_MainMenu), "Update")]
        class uGUI_MainMenu_Update_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance)
            {
                //int num2 = ~(1 << LayerID.Player | 1 << LayerID.AllowPlayerAndVehicle | 1 << LayerID.OnlyVehicle);
                //AddDebug("num2 " + num2);
                //AddDebug("lastGroup " +__instance.lastGroup);
                //AddDebug("mouseScrollDelta " + Input.mouseScrollDelta);
                if (__instance.lastGroup == "SavedGames" || __instance.lastGroup == "NewGame")
                {
                    if (Input.mouseScrollDelta.y > 0f)
                        __instance.subMenu.SelectItemInDirection(0, -1);
                    else if (Input.mouseScrollDelta.y < 0f)
                        __instance.subMenu.SelectItemInDirection(0, 1);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_PDA), "Update")]
        class uGUI_PDA_Update_Patch
        {
            public static void Postfix(uGUI_PDA __instance)
            {
                if (Main.gameLoaded == false || GameInput.lastDevice != GameInput.Device.Keyboard || IngameMenu.main.isActiveAndEnabled || !Player.main.pda.isOpen)
                    return;

                //if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                if (Input.GetKeyDown(ConfigMenu.nextPDATabKey.Value))
                {
                    __instance.OpenTab(__instance.GetNextTab());
                }
                else if (Input.GetKeyDown(ConfigMenu.previousPDATabKey.Value))
                {
                    __instance.OpenTab(__instance.GetPreviousTab());
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_FeedbackCollector), "HintShow")]
        class uGUI_FeedbackCollector_HintShow_Patch
        {
            static bool Prefix(uGUI_FeedbackCollector __instance)
            {
                //AddDebug("uGUI_FeedbackCollector HintShow");
                return !ConfigToEdit.disableHints.Value;
            }
        }

        [HarmonyPatch(typeof(PlayerWorldArrows), "CreateWorldArrows")]
        internal class PlayerWorldArrows_CreateWorldArrows_Patch
        { // not used?
            internal static bool Prefix(PlayerWorldArrows __instance)
            {
                //AddDebug("CreateWorldArrows");
                return !ConfigToEdit.disableHints.Value;
            }
        }

        //[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
        class TooltipFactory_ItemCommons_Prefix_Patch
        {
            static void Prefix(StringBuilder sb, TechType techType, GameObject obj)
            {
                CreatureEgg creatureEgg = obj.GetComponent<CreatureEgg>();
                if (creatureEgg)
                {
                    LiveMixin liveMixin = obj.GetComponent<LiveMixin>();
                    if (!liveMixin.IsAlive())
                    {
                        TooltipFactory.WriteTitle(sb, "Dead ");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TooltipFactory))]
        class TooltipFactory_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            static void InitializePostfix()
            {
                //AddDebug("TooltipFactory Initialize ");
                if (string.IsNullOrEmpty(altToolButton))
                    GetStrings();
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnLanguageChanged")]
            static void OnLanguageChangedPostfix()
            {
                //AddDebug("TooltipFactory OnLanguageChanged ");
                //Main.languageCheck = Language.main.GetCurrentLanguage() == "English" || Main.config.translatableStrings[0] != "Burnt out ";
                GetStrings();
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnBindingsChanged")]
            static void OnBindingsChangedPostfix()
            {
                //AddDebug("TooltipFactory OnBindingsChanged ");
                GetStrings();
            }

            [HarmonyPrefix]
            [HarmonyPatch("ItemCommons")]
            static void ItemCommonsPrefix(StringBuilder sb, TechType techType, GameObject obj)
            {
                if (ConfigToEdit.flareTweaks.Value)
                {
                    Flare flare = obj.GetComponent<Flare>();
                    if (flare)
                    {
                        //AddDebug("flare.energyLeft " + flare.energyLeft);
                        if (flare.energyLeft <= 0f)
                            TooltipFactory.WriteTitle(sb, Language.main.Get("TF_burnt_out_flare"));
                        else if (flare.flareActivateTime > 0f)
                            TooltipFactory.WriteTitle(sb, Language.main.Get("TF_lit_flare"));
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("ItemCommons")]
            static void ItemCommonsPostfix(ref StringBuilder sb, TechType techType, GameObject obj)
            {
                if (Crush_Damage.crushDepthEquipment.ContainsKey(techType) && Crush_Damage.crushDepthEquipment[techType] > 0)
                { // IInventoryDescription
                    StringBuilder sb_ = new StringBuilder(Language.main.Get("TF_crush_depth_equipment"));
                    sb_.Append(Crush_Damage.crushDepthEquipment[techType].ToString());
                    sb_.Append(Language.main.Get("TF_meters"));
                    TooltipFactory.WriteDescription(sb, sb_.ToString());
                }
                if (Crush_Damage.crushDamageEquipment.ContainsKey(techType) && Crush_Damage.crushDamageEquipment[techType] > 0)
                { // IInventoryDescription
                    StringBuilder sb_ = new StringBuilder(Language.main.Get("TF_crush_damage_equipment"));
                    sb_.Append(Crush_Damage.crushDamageEquipment[techType].ToString());
                    sb_.Append(Language.main.Get("%"));
                    TooltipFactory.WriteDescription(sb, sb_.ToString());
                }
                Eatable eatable = obj.GetComponent<Eatable>();
                if (ConfigMenu.eatRawFish.Value != ConfigMenu.EatingRawFish.Default && fishTechTypes.Contains(techType) && GameModeManager.GetOption<bool>(GameOption.Hunger))
                {
                    //Eatable eatable = obj.GetComponent<Eatable>();
                    if (eatable)
                    {
                        sb.Clear();
                        string name = Language.main.Get(techType);
                        string secondaryTooltip = eatable.GetSecondaryTooltip();
                        if (!string.IsNullOrEmpty(secondaryTooltip))
                            name = Language.main.GetFormat<string, string>("DecomposingFormat", secondaryTooltip, name);
                        TooltipFactory.WriteTitle(sb, name);
                        TooltipFactory.WriteDebug(sb, techType);
                        int foodValue = Mathf.CeilToInt(eatable.GetFoodValue());
                        if (foodValue != 0)
                        {
                            string food = Language.main.GetFormat<int>("FoodFormat", foodValue);
                            int index = -1;
                            if (foodValue < 0)
                                index = food.LastIndexOf('-');
                            else
                                index = food.LastIndexOf('+');

                            if (index != -1)
                            {
                                if (foodValue > 0)
                                {
                                    if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                                        food = food.Substring(0, index) + "≈ 0";
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                                        food = food.Substring(0, index) + "≈ " + Mathf.CeilToInt(foodValue * .5f);
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                                        food = food.Substring(0, index) + "≈ " + (-Mathf.CeilToInt(foodValue * .5f));
                                }
                                //AddDebug("food  " + food);
                            }
                            TooltipFactory.WriteDescription(sb, food);
                        }
                        int waterValue = Mathf.CeilToInt(eatable.GetWaterValue());
                        if (waterValue != 0)
                        {
                            string water = Language.main.GetFormat<int>("WaterFormat", waterValue);
                            int index = -1;
                            if (waterValue < 0)
                                index = water.LastIndexOf('-');
                            else
                                index = water.LastIndexOf('+');

                            if (index != -1)
                            {
                                if (waterValue > 0)
                                {
                                    if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                                        water = water.Substring(0, index) + "≈ 0";
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                                        water = water.Substring(0, index) + "≈ " + Mathf.CeilToInt(waterValue * .5f);
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                                        water = water.Substring(0, index) + "≈ " + (-Mathf.CeilToInt(waterValue * .5f));
                                }
                                //AddDebug("water  " + water);
                            }
                            TooltipFactory.WriteDescription(sb, water);
                        }
                        TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
                    }
                }
                if (techType == TechType.Battery)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_Battery"));
                else if (techType == TechType.PowerCell)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_PowerCell"));
                else if (techType == TechType.PrecursorIonBattery)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_PrecursorIonBattery"));
                else if (techType == TechType.PrecursorIonPowerCell)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_PrecursorIonPowerCell"));
                else if (techType == TechType.FirstAidKit)
                {
                    sb.Clear();
                    string name = Language.main.Get(techType);
                    TooltipFactory.WriteTitle(sb, name);
                    TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
                    TooltipFactory.WriteDescription(sb, Language.main.GetFormat<float>("HealthFormat", ConfigMenu.medKitHP.Value));
                    //TooltipFactory.WriteDescription(sb, "Restores " + Main.config.medKitHP + " health.");
                    //AddDebug("ItemCommons " + sb.ToString());
                }
                else if (techType == TechType.SeaTruckUpgradeHorsePower && ConfigToEdit.replaceSeatruckHorsePowerUpgrade.Value)
                {
                    sb.Clear();
                    TooltipFactory.WriteTitle(sb, Language.main.Get(techType));
                    TooltipFactory.WriteDescription(sb, Language.main.Get("TF_SeaTruckUpgradeHorsePower_my_tooltip"));
                }

                if (eatable && Util.IsWater(eatable) && eatable.timeDecayStart > 0f)
                {
                    sb.Clear();
                    StringBuilder sb_ = new StringBuilder(Language.main.Get(techType));
                    float frozenPercent = Util.NormalizeToRange(eatable.timeDecayStart, 0f, eatable.waterValue, 0f, 100f);
                    sb_.Append(" ");
                    Mathf.Clamp(frozenPercent, frozenPercent, 100f);
                    sb_.Append(Mathf.RoundToInt(frozenPercent));
                    sb_.Append("%");
                    sb_.Append(Language.main.Get("TF_frozen_water"));
                    TooltipFactory.WriteTitle(sb, sb_.ToString());
                    //int healthValue = (int)eatable.GetHealthValue();  
                    //if (healthValue != 0f)
                    //    TooltipFactory.WriteDescription(sb, Language.main.GetFormat<int>("HealthFormat", healthValue));
                    int cold = Mathf.CeilToInt(eatable.GetColdMeterValue());
                    if (cold != 0)
                        TooltipFactory.WriteDescription(sb, Language.main.GetFormat<int>("HeatImpactFormat", -cold));
                    int foodValue = Mathf.CeilToInt(eatable.GetFoodValue());
                    int waterValue = Mathf.CeilToInt(eatable.GetWaterValue());
                    if (foodValue != 0)
                        TooltipFactory.WriteDescription(sb, Language.main.GetFormat<int>("FoodFormat", foodValue));
                    if (waterValue != 0)
                        TooltipFactory.WriteDescription(sb, Language.main.GetFormat<int>("WaterFormat", waterValue));
                    TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
                }
                if (ConfigMenu.invMultLand.Value > 0f || ConfigMenu.invMultWater.Value > 0f)
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        TooltipFactory.WriteDescription(sb, Language.main.Get("TF_mass") + rb.mass + Language.main.Get("TF_kg"));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HandReticle), "SetTextRaw")]
        class HandReticle_SetTextRaw_Patch
        {
            static bool Prefix(HandReticle __instance, HandReticle.TextType type, string text)
            {
                //AddDebug("SetTextRaw " + type + " " + text);
                if (ConfigToEdit.disableUseText.Value && (type == HandReticle.TextType.Use || type == HandReticle.TextType.UseSubscript))
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(HintSwimToSurface), "Update")]
        public static class HintSwimToSurface_Update_Patch
        {
            public static bool Prefix(HintSwimToSurface __instance)
            {
                return ConfigToEdit.lowOxygenWarning.Value;
            }
        }

        [HarmonyPatch(typeof(LowOxygenAlert), "Update")]
        public static class LowOxygenAlert_Update_Patch
        {
            public static bool Prefix(LowOxygenAlert __instance)
            {
                return ConfigToEdit.lowOxygenAudioWarning.Value;
            }
        }

        [HarmonyPatch(typeof(uGUI_EncyclopediaTab), "DisplayEntry")]
        public static class uGUI_EncyclopediaTab_Patch
        {
            public static void Postfix(uGUI_EncyclopediaTab __instance) => __instance.contentScrollRect.verticalNormalizedPosition = 1f;
        }


        [HarmonyPatch(typeof(uGUI_ExosuitHUD), "Update")]
        public static class uGUI_ExosuitHUD_Patch
        {
            static string tempSuffix;
            static int lastTemperature = int.MinValue;
            public static void Postfix(uGUI_ExosuitHUD __instance)
            {
                if (!Main.gameLoaded)
                    return;
                //AddDebug("uGUI_ExosuitHUD Update ");
                if (ConfigToEdit.showTempFahrenhiet.Value && Player.main.currentMountedVehicle != null && Player.main.currentMountedVehicle is Exosuit)
                {
                    if (__instance.lastTemperature == lastTemperature)
                        return;

                    __instance.textTemperature.text = IntStringCache.GetStringForInt((int)Util.CelciusToFahrenhiet(__instance.lastTemperature));
                    if (tempSuffix == null)
                    {
                        __instance.textTemperatureSuffix.text = __instance.textTemperatureSuffix.text.Replace("°C", "°F");
                        tempSuffix = __instance.textTemperatureSuffix.text;
                    }
                    else
                        __instance.textTemperatureSuffix.text = tempSuffix;

                    lastTemperature = __instance.lastTemperature;
                }
            }
        }

        [HarmonyPatch(typeof(ThermalPlant))]
        public static class ThermalPlant_Patch
        {

            [HarmonyPostfix, HarmonyPatch("UpdateUI")]
            public static void UpdateUIPostfix(ThermalPlant __instance)
            {
                //AddDebug("ThermalPlant UpdateUI");
                if (!Main.gameLoaded)
                    return;

                if (ConfigToEdit.showTempFahrenhiet.Value)
                {
                    __instance.temperatureText.text = (int)Util.CelciusToFahrenhiet(__instance.temperature) + "°F";
                }
            }

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(ThermalPlant __instance)
            {
                //AddDebug("ThermalPlant Start");
                CoroutineHost.StartCoroutine(FixTempDisplay(__instance.gameObject));
            }

            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            public static bool OnHandHoverPrefix(ThermalPlant __instance, GUIHand hand)
            {
                if (!__instance.constructable.constructed)
                    return false;

                HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<int, int>("ThermalPlantStatus", Mathf.RoundToInt(__instance.powerSource.GetPower()), Mathf.RoundToInt(__instance.powerSource.GetMaxPower())), false);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                //HandReticle.main.SetIcon(HandReticle.IconType.Interact);
                return false;
            }

            public static IEnumerator FixTempDisplay(GameObject go)
            {// fix disappearing temp display
                yield return new WaitForSeconds(2);
                go.SetActive(false);
                go.SetActive(true);
            }
        }


    }
}
