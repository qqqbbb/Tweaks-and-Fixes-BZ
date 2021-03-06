using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class UI_Patches
    {
        static bool textInput = false;
        static bool fishTooltip = false;
        static bool chargerOpen = false;

        public static Dictionary<ItemsContainer, Recyclotron> recyclotrons = new Dictionary<ItemsContainer, Recyclotron>() ;
        static Recyclotron openRecyclotron = null;
        static List<TechType> landPlantSeeds = new List<TechType> { TechType.HeatFruit, TechType.PurpleVegetable, TechType.FrozenRiverPlant2Seeds, TechType.LeafyFruit, TechType.HangingFruit, TechType.MelonSeed, TechType.SnowStalkerFruit, TechType.PinkFlowerSeed, TechType.PurpleRattleSpore, TechType.OrangePetalsPlantSeed };
        static List<TechType> waterPlantSeeds = new List<TechType> { TechType.CreepvineSeedCluster, TechType.SmallMaroonPlantSeed, TechType.TwistyBridgesMushroomChunk, TechType.JellyPlantSeed, TechType.PurpleBranchesSeed, TechType.RedBushSeed, TechType.GenericRibbonSeed, TechType.GenericSpiralChunk, TechType.SpottedLeavesPlantSeed, TechType.PurpleStalkSeed, TechType.DeepLilyShroomSeed };
        static HashSet<ItemsContainer> landPlanters = new HashSet<ItemsContainer>();
        static HashSet<ItemsContainer> waterPlanters = new HashSet<ItemsContainer>();
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
        static public string toggleBaseLightString = string.Empty;

        static void GetStrings()
        {
            //AddDebug("GetStrings");
            if (Main.config.translatableStrings.Count < 14)
            {
                Main.config.translatableStrings = new List<string>
        {"Burnt out ", "Lit ", "Increases the Seatruck engine's horsepower and energy consumption by 10%. More than 1 can be used simultaneously.", " frozen", "Increases your safe diving depth by ", " meters.", "mass ", "Throw", "Light and throw", "Light", ": min ", ", max ", "Need a knife to break it", "Need a knife to break it free"};
                Main.config.Save();
            }
            altToolButton = uGUI.FormatButton(GameInput.Button.AltTool);
            rightHandButton = uGUI.FormatButton(GameInput.Button.RightHand);
            leftHandButton = uGUI.FormatButton(GameInput.Button.LeftHand);
            fishDropString = TooltipFactory.stringDrop + " (" + rightHandButton + ")";
            fishEatString = TooltipFactory.stringEat + " (" + altToolButton + ")";
            lightFlareString = Main.config.translatableStrings[9] + " (" + altToolButton + ")";
            throwFlareString = Main.config.translatableStrings[7] + " (" + rightHandButton + ")";
            lightAndThrowFlareString = Main.config.translatableStrings[8] + " (" + rightHandButton + ")";
            beaconToolString = TooltipFactory.stringDrop + " (" + rightHandButton + ")  " + Language.main.Get("BeaconLabelEdit") + " (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
            beaconPickString = "(" + rightHandButton + ")\n" + Language.main.Get("BeaconLabelEdit") + " (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
            toggleBaseLightString = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.Deconstruct);
        }

        [HarmonyPatch(typeof(Recyclotron), "Start")]
        class Recyclotron_Start_Patch
        {
            static void Postfix(Recyclotron __instance)
            {
                recyclotrons[__instance.storageContainer.container] = __instance;
            }
        }

        [HarmonyPatch(typeof(Aquarium), "Start")]
        class Aquarium_Start_Patch
        {
            static void Postfix(Aquarium __instance)
            {
                //AddDebug("Trashcan " + __instance.biohazard + " " + __instance.storageContainer.hoverText);
                //__instance.storageContainer.hoverText = Language.main.Get("LabTrashcan");
                if (__instance.storageContainer.container.allowedTech == null)
                {
                    //AddDebug("Aquarium allowedTech == null ");
                    __instance.storageContainer.container.allowedTech = new HashSet<TechType> { TechType.SeaMonkeyBaby, TechType.PenguinBaby, TechType.Bladderfish, TechType.Boomerang, TechType.ArcticPeeper,  TechType.Hoopfish, TechType.ArrowRay, TechType.DiscusFish, TechType.FeatherFish, TechType.FeatherFishRed, TechType.NootFish, TechType.Spinefish, TechType.SpinnerFish, TechType.Symbiote, TechType.Triops };
                }
            }
        }

        [HarmonyPatch(typeof(Trashcan), "OnEnable")]
        class Trashcan_OnEnable_Patch
        {
            static void Postfix(Trashcan __instance)
            {
                //AddDebug("Trashcan " + __instance.biohazard + " " + __instance.storageContainer.hoverText);
                if (__instance.biohazard)
                {
                    //__instance.storageContainer.hoverText = Language.main.Get("LabTrashcan");
                    if (__instance.storageContainer.container.allowedTech == null)
                    {
                        //AddDebug("LabTrashcan allowedTech == null ");
                        __instance.storageContainer.container.allowedTech = new HashSet<TechType> { TechType.ReactorRod, TechType.DepletedReactorRod };
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_ItemsContainer), "OnAddItem")]
        class uGUI_ItemsContainer_OnAddItem_Patch
        {
            static void Postfix(uGUI_ItemsContainer __instance, InventoryItem item)
            {
                //AddDebug("uGUI_ItemsContainer OnAddItem " + item.item.GetTechName());
                if (openRecyclotron)
                {
                    //AddDebug("uGUI_ItemsContainer OnAddItem " + item.item.GetTechName());
                    if (!openRecyclotron.IsAllowedToAdd(item.item, false))
                        __instance.items[item].SetChroma(0f);
                }
                else if(chargerOpen)
                {
                    Battery battery = item.item.GetComponent<Battery>();
                    if (battery && battery.charge == battery.capacity)
                    {
                        //AddDebug(pair.Key.item.GetTechType() + " charge == capacity ");
                        __instance.items[item].SetChroma(0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Fridge), "OnEnable")]
        class Fridge_OnEnable_Patch
        {
            static void Postfix(Fridge __instance)
            {
                Main.fridges.Add(__instance.storageContainer.container);
            }
        }

        [HarmonyPatch(typeof(BaseBioReactor), "Start")]
        class BaseBioReactor_Start_Patch
        {
            static void Postfix(BaseBioReactor __instance)
            {
                if (__instance.container.allowedTech == null)
                {
                    //AddDebug("BaseBioReactor container.allowedTech == null ");
                    __instance.container.allowedTech = new HashSet<TechType>();
                    foreach (var pair in BaseBioReactor.charge)
                        __instance.container.allowedTech.Add(pair.Key);
                }
            }
        }

        [HarmonyPatch(typeof(Planter), "Start")]
        class Planter_Start_Patch
        {
            static void Postfix(Planter __instance)
            {
                ItemsContainerType type = __instance.GetContainerType();
                if (type == ItemsContainerType.LandPlants)
                    landPlanters.Add(__instance.storageContainer.container);
                else if (type == ItemsContainerType.WaterPlants)
                    waterPlanters.Add(__instance.storageContainer.container);
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab))]
        class uGUI_InventoryTab_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnOpenPDA")]
            static void OnOpenPDAPostfix(uGUI_InventoryTab __instance)
            {
                IItemsContainer itemsContainer = Inventory.main.GetUsedStorage(0);
                ItemsContainer container = itemsContainer as ItemsContainer;
                //    AddDebug("GetUsedStorageCount " + Inventory.main.GetUsedStorageCount());
                if (container != null)
                {
                    //AddDebug(" container ");
                    //ItemsContainerType itemsContainerType = ItemsContainerType.
                    if (landPlanters.Contains(container))
                    {
                        //AddDebug(" landPlanter ");
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!landPlantSeeds.Contains(pair.Key.item.GetTechType()))
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    else if (waterPlanters.Contains(container))
                    {
                        //AddDebug(" waterPlanter ");
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!waterPlantSeeds.Contains(pair.Key.item.GetTechType()))
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    else if(Main.fridges.Contains(container))
                    {
                        foreach (var pair in __instance.inventory.items)
                        {
                            Eatable eatable = pair.Key.item.GetComponent<Eatable>();
                            if (!eatable)
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    else if (recyclotrons.ContainsKey(container))
                    {
                        openRecyclotron = recyclotrons[container];
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!openRecyclotron.IsAllowedToAdd(pair.Key.item, false))
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    else
                    {
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!container.IsTechTypeAllowed(pair.Key.item.GetTechType()))
                                pair.Value.SetChroma(0f);
                        }
                    }
                    return;
                }
                Equipment equipment = itemsContainer as Equipment;
                if (equipment != null)
                {
                    //AddDebug(" equipment  ");
                    bool charger = equipment.GetCompatibleSlot(EquipmentType.BatteryCharger, out string s) || equipment.GetCompatibleSlot(EquipmentType.PowerCellCharger, out string ss);
                    //AddDebug("charger " + charger);
                    foreach (var pair in __instance.inventory.items)
                    {
                        EquipmentType itemType = TechData.GetEquipmentType(pair.Key.item.GetTechType());
                        //AddDebug(pair.Key.item.GetTechType() + " " + equipmentType);
                        string slot = string.Empty;
                        if (equipment.GetCompatibleSlot(itemType, out slot))
                        {
                            if (charger)
                            {
                                chargerOpen = true;
                                Battery battery = pair.Key.item.GetComponent<Battery>();
                                if (battery && battery.charge == battery.capacity)
                                    pair.Value.SetChroma(0f);
                            }
                        }
                        else
                            pair.Value.SetChroma(0f);
                    }
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnClosePDA")]
            static void OnClosePDAPostfix(uGUI_InventoryTab __instance)
            {
                chargerOpen = false;
                openRecyclotron = null;
                foreach (var pair in __instance.inventory.items)
                    pair.Value.SetChroma(1f);
            }
        }

        private static IEnumerator IntroSequence(ExpansionIntroManager introManager, uGUI_ExpansionIntro __instance)
        {
            IntroVignette.isIntroActive = true;
            if (FPSInputModule.current)
                FPSInputModule.current.lockPauseMenu = true;
            __instance.fader.SetState(true);
            Player.main.playerController.inputEnabled = false;
            __instance.PauseGameTime();
            yield return new WaitForSecondsRealtime(0.5f);
            MainMenuMusic.Stop();
            introManager.TriggerStartScreenAudio();
            __instance.mainText.SetText("");
            yield return new WaitForSecondsRealtime(2f);
            while (!LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
                yield return new WaitForSecondsRealtime(1f);
            __instance.mainText.SetText(Language.main.Get("PressAnyButton"));
            __instance.mainText.SetState(true);
            VRLoadingOverlay.Hide();
            if (!QuickLaunchHelper.IsQuickLaunching())
            {
                while (!Input.anyKeyDown)
                    yield return null;
            }
            if (FPSInputModule.current)
                FPSInputModule.current.lockPauseMenu = false;
            yield return introManager.Play(Player.main, __instance);
            IntroVignette.isIntroActive = false;
            __instance.ResumeGameTime();
            __instance.StopCoroutine(__instance.coroutine);
            __instance.coroutine = (Coroutine)null;
            //gui.StartCoroutine(gui.ControlsHints());
        }

        [HarmonyPatch(typeof(uGUI_ExpansionIntro), "Play")]
        class uGUI_ExpansionIntro_Plays_Patch
        { 
            static bool Prefix(uGUI_ExpansionIntro __instance, ExpansionIntroManager introManager)
            {
                //AddDebug("uGUI_ExpansionIntro Play");
                if (!Main.config.disableHints)
                    return true;

                if (__instance.showing)
                    return false;
                //AddDebug("StartCoroutine IntroSequence");
                __instance.coroutine = __instance.StartCoroutine(IntroSequence(introManager, __instance));
                InputHandlerStack.main.Push((IInputHandler)__instance);
                return false;
            }
        }

        //[HarmonyPatch(typeof(Inventory), "GetAllItemActions")]
        class Inventory_GetAllItemActions_Patch
        {
            static bool Prefix(Inventory __instance, InventoryItem item, ref ItemAction __result)
            {
                //AddDebug("GetAllItemActions " + item.item.GetTechName() + " " + __result);

                return false;
            }
            static void Postfix(Inventory __instance, InventoryItem item, ItemAction __result)
            {
                //AddDebug("GetAllItemActions " + item.item.GetTechName() + " " + __result);

            }
        }

        //[HarmonyPatch(typeof(GUIHand), "GetActionString")]
        class GUIHand_GetActionString_Patch
        { 
            public static void Postfix(GUIHand __instance, ItemAction action, Pickupable pickupable, ref string __result)
            {
                GameModeUtils.GetGameMode(out GameModeOption mode, out GameModeOption _);
                bool survival = !GameModeUtils.IsOptionActive(mode, GameModeOption.NoSurvival);
                //AddDebug("survival " + survival);
                if (Main.IsEatableFish(pickupable.gameObject))
                {
                    bool cantEat = Main.config.cantEatUnderwater && Player.main.IsUnderwater();
                    //AddDebug("GetActionString " + action + " " + __result);
                    //__result = "ItemActionEat";
                    string dropText = __result;

                    //string dropText = HandReticle.main.GetText(GUIHand.GetActionString(action, pickupable), true, GameInput.Button.LeftHand);
                    //HandReticle.main.SetText(HandReticle.TextType.Use, GUIHand.GetActionString(action, pickupable), true, GameInput.Button.RightHand);
                    //HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, text);
                }
            }
        }

        [HarmonyPatch(typeof(GUIHand), "OnUpdate")]
        class GUIHand_OnUpdate_Patch
        {
            public static void Postfix(GUIHand __instance)
            { // UI tells you if looking at dead fish 
                PlayerTool tool = __instance.GetTool();
                if (tool)
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

                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                        if (!lit && GameInput.GetButtonDown(GameInput.Button.AltTool))
                            Flare_Patch.LightFlare(flare);
                    }
                    Beacon beacon = tool as Beacon;
                    if (beacon)
                    {
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, beaconToolString);
                        if (beacon.beaconLabel && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                            uGUI.main.userInput.RequestString(beacon.beaconLabel.stringBeaconLabel, beacon.beaconLabel.stringBeaconSubmit, beacon.beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beacon.beaconLabel.SetLabel));
                    }
                }
                else if (!Main.pda.isInUse && !textInput && !uGUI._main.craftingMenu.selected)
                {
                    SubRoot subRoot = Player.main.currentSub;
                    if (subRoot && subRoot.isBase && subRoot.powerRelay && subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline)
                    {
                        HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, toggleBaseLightString);
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                            Base_Patch.ToggleBaseLight(subRoot);
                    }
                }

                GameModeUtils.GetGameMode(out GameModeOption mode, out GameModeOption _);
                bool survival = !GameModeUtils.IsOptionActive(mode, GameModeOption.NoSurvival);
                //AddDebug("survival " + survival);
                InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
                if (survival && heldItem != null && Main.IsEatableFish(heldItem.item.gameObject))
                {
                    string text = string.Empty;
                    ItemAction allItemActions = Inventory.main.GetAllItemActions(heldItem);
                    if ((allItemActions & ItemAction.Drop) != ItemAction.None)
                    {
                        text = GUIHand.GetActionString(ItemAction.Drop, heldItem.item);
                        text = HandReticle.main.GetText(text, true, GameInput.Button.RightHand);
                    }
                    bool cantEat = Main.config.cantEatUnderwater && Player.main.IsUnderwater();
                    if (!cantEat)
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
                if (!__instance.activeTarget)
                    return;
                //AddDebug("activeTarget layer " + __instance.activeTarget.layer);
                //if (__instance.activeTarget.layer == LayerID.NotUseable)
                //    AddDebug("activeTarget Not Useable layer ");
                TechType targetTT = CraftData.GetTechType(__instance.activeTarget);
                if (targetTT == TechType.None)
                    return;

                Flare flareTarget = __instance.activeTarget.GetComponent<Flare>();
                if (flareTarget && Main.languageCheck && flareTarget.energyLeft == 0f)
                {
                    //AddDebug("activeTarget Flare");
                    StringBuilder sb = new StringBuilder(Main.config.translatableStrings[0]);
                    sb.Append(Language.main.Get(targetTT));
                    HandReticle.main.SetText(HandReticle.TextType.Hand, sb.ToString(), false);
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

        [HarmonyPatch(typeof(uGUI_MainMenu), "Update")]
        class uGUI_MainMenu_Update_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance)
            {
                //AddDebug("lastGroup " +__instance.lastGroup);
                if (__instance.lastGroup == "SavedGames")
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                        __instance.subMenu.SelectItemInDirection(0, -1);
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                        __instance.subMenu.SelectItemInDirection(0, 1);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_PDA), "Update")]
        class uGUI_PDA_Update_Patch
        {
            public static void Postfix(uGUI_PDA __instance)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                        __instance.OpenTab(__instance.GetNextTab());
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
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
                    return !Main.config.disableHints;
            }
        }

        [HarmonyPatch(typeof(PlayerWorldArrows), "CreateWorldArrows")]
        internal class PlayerWorldArrows_CreateWorldArrows_Patch
        { // not used?
            internal static bool Prefix(PlayerWorldArrows __instance)
            {
                //AddDebug("CreateWorldArrows");
                return !Main.config.disableHints;
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
                Main.languageCheck = Language.main.GetCurrentLanguage() == "English" || Main.config.translatableStrings[0] != "Burnt out ";
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
                if (Main.languageCheck)
                { 
                    Flare flare = obj.GetComponent<Flare>();
                    if (flare)
                    {
                        //AddDebug("flare.energyLeft " + flare.energyLeft);
                        if (flare.energyLeft <= 0f)
                            TooltipFactory.WriteTitle(sb, Main.config.translatableStrings[0]);
                        else if (flare.flareActivateTime > 0f)
                            TooltipFactory.WriteTitle(sb, Main.config.translatableStrings[1]);
                    }
                    fishTooltip = Main.IsEatableFish(obj);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("ItemCommons")]
            static void ItemCommonsPostfix(ref StringBuilder sb, TechType techType, GameObject obj)
            {
                if (Main.languageCheck)
                {
                    if (Crush_Damage.crushDepthEquipment.ContainsKey(techType) && Crush_Damage.crushDepthEquipment[techType] > 0)
                    { // IInventoryDescription
                        StringBuilder sb_ = new StringBuilder(Main.config.translatableStrings[4]);
                        sb_.Append(Crush_Damage.crushDepthEquipment[techType].ToString());
                        sb_.Append(Main.config.translatableStrings[5]);
                        TooltipFactory.WriteDescription(sb, sb_.ToString());
                    }
                    if (techType == TechType.FirstAidKit)
                    {
                        sb.Clear();
                        string name = Language.main.Get(techType);
                        TooltipFactory.WriteTitle(sb, name);
                        TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
                        TooltipFactory.WriteDescription(sb, Language.main.GetFormat<float>("HealthFormat", Main.config.medKitHP));
                        //TooltipFactory.WriteDescription(sb, "Restores " + Main.config.medKitHP + " health.");
                        //AddDebug("ItemCommons " + sb.ToString());
                    }
                    else if (techType == TechType.SeaTruckUpgradeHorsePower && Main.config.seatruckMoveTweaks)
                    {
                        sb.Clear();
                        TooltipFactory.WriteTitle(sb, Language.main.Get(techType));
                        TooltipFactory.WriteDescription(sb, Main.config.translatableStrings[2]);
                    }
                    Eatable eatable = obj.GetComponent<Eatable>();
                    if (eatable && Food_Patch.IsWater(eatable) && eatable.timeDecayStart > 0f)
                    {
                        sb.Clear();
                        StringBuilder sb_ = new StringBuilder(Language.main.Get(techType));
                        float frozenPercent = Main.NormalizeToRange(eatable.timeDecayStart, 0f, eatable.waterValue, 0f, 100f);
                        sb_.Append(" ");
                        Mathf.Clamp(frozenPercent, frozenPercent, 100f);
                        sb_.Append(Mathf.RoundToInt(frozenPercent));
                        sb_.Append("%");
                        sb_.Append(Main.config.translatableStrings[3]);
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
                }

                if (Main.config.invMultLand > 0f || Main.config.invMultWater > 0f)
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        StringBuilder sb_ = new StringBuilder(Main.config.translatableStrings[6]);
                        sb_.Append(rb.mass);
                        TooltipFactory.WriteDescription(sb, sb_.ToString());
                    }
            
                }
            }
        }

        [HarmonyPatch(typeof(Language), "FormatString", new Type[] { typeof(string), typeof(object[]) })]
        class Language_FormatString_Patch
        {
            static void Postfix(string format, ref string __result, object[] args)
            {
                //AddDebug("FormatString " + format + " " + args.Length);
                //AddDebug("FormatString " + __result);
                if (!Main.languageCheck || !fishTooltip || Main.config.eatRawFish == Config.EatingRawFish.Vanilla || args.Length == 0 || args[0].GetType() != typeof(int))
                    return;
                //AddDebug("FormatString GetType " + args[0].GetType());
                int value = (int)args[0];
                if (value > 0f && format.Contains("FOOD:") || format.Contains("H₂O:"))
                {
                    string[] tokens = __result.Split(':');
                    string min = Main.config.translatableStrings[10];
                    string max = Main.config.translatableStrings[11];
                    StringBuilder sb_ = new StringBuilder(tokens[0]);
                    sb_.Append(min);
                    if (Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                    {
                        //__result = tokens[0] + min + "0" + max + value;
                        sb_.Append("0");
                        sb_.Append(max);
                        sb_.Append(value);
                    }
                    else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                    {
                        //__result = tokens[0] + min + "-" + value + max + value;
                        sb_.Append("-");
                        sb_.Append(value);
                        sb_.Append(max);
                        sb_.Append(value);
                    }
                    else if (Main.config.eatRawFish == Config.EatingRawFish.Harmful)
                    {
                        //__result = tokens[0] + min + "-" + value + max + "0";
                        sb_.Append("-");
                        sb_.Append(value);
                        sb_.Append(max);
                        sb_.Append("0");
                    }
                    __result = sb_.ToString();
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "ExecuteItemAction", new Type[] { typeof(ItemAction), typeof(InventoryItem)})]
        class Inventory_ExecuteItemAction_Patch
        {
            public static bool Prefix(Inventory __instance, InventoryItem item, ItemAction action)
            {
                //AddDebug("ExecuteItemAction " + action);
                IItemsContainer oppositeContainer = __instance.GetOppositeContainer(item);
                if (action != ItemAction.Switch || oppositeContainer == null || item.container is Equipment || oppositeContainer is Equipment)
                    return true;

                ItemsContainer container = (ItemsContainer)item.container;
                List<InventoryItem> itemsToTransfer = new List<InventoryItem>();
                if (Input.GetKey(Main.config.transferAllItemsKey))
                {
                    //AddDebug("LeftShift ");
                    foreach (TechType itemType in container.GetItemTypes())
                        container.GetItems(itemType, itemsToTransfer);
                }
                else if (Input.GetKey(Main.config.transferSameItemsKey))
                {
                    //AddDebug("LeftControl ");
                    container.GetItems(item.item.GetTechType(), itemsToTransfer);
                }
                foreach (InventoryItem ii in itemsToTransfer)
                {
                    //AddDebug("itemsToTransfer " + ii.item.name);
                    Inventory.AddOrSwap(ii, oppositeContainer);
                }
                if (itemsToTransfer.Count > 0)
                    return false;
                else
                    return true;
            }
        }
               
        [HarmonyPatch(typeof(uGUI_HealthBar), "LateUpdate")]
        class uGUI_HealthBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_HealthBar __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
                    return true;
                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    LiveMixin component = main.GetComponent<LiveMixin>();
                    if (component != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            component.onHealDamage.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnHealDamage));
                        }
                        float has = component.health - component.tempDamage;
                        float maxHealth = component.maxHealth;
                        __instance.SetValue(has, maxHealth);
                        float time = 1f - Mathf.Clamp01(has / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0F)
                            __instance.pulseDelay = 0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0F)
                            __instance.pulseTime = 0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0f && num2 <= 0F)
                            __instance.statePulse.normalizedTime = 0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if (__instance.statePulse.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;
                __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_FoodBar), "LateUpdate")]
        class uGUI_FoodBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_FoodBar __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
                    return true;

                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    //Survival component = main.GetComponent<Survival>();
                    if (Main.survival != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            Main.survival.onEat.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnEat));
                        }
                        float food = Main.survival.food;
                        float capacity = 100f;
                        __instance.SetValue(food, capacity);
                        float time = 1f - Mathf.Clamp01(food / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0f)
                            __instance.pulseDelay = 0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0f)
                            __instance.pulseTime = 0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0f && num2 <= 0f)
                            __instance.pulseAnimationState.normalizedTime = 0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if (__instance.pulseAnimationState != null && __instance.pulseAnimation.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0.0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;
                __instance.icon.localRotation = Quaternion.Euler(0.0f, __instance.rotationCurrent, 0.0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_WaterBar), "LateUpdate")]
        class uGUI_WaterBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_WaterBar __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
                    return true;

                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    //Survival component = main.GetComponent<Survival>();
                    if (Main.survival != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            Main.survival.onDrink.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnDrink));
                        }
                        float water = Main.survival.water;
                        float capacity = 100f;
                        __instance.SetValue(water, capacity);
                        float time = 1f - Mathf.Clamp01(water / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0f)
                            __instance.pulseDelay = 0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0f)
                            __instance.pulseTime = 0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0f && num2 <= 0f)
                            __instance.pulseAnimationState.normalizedTime = 0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if ((TrackedReference)__instance.pulseAnimationState != (TrackedReference)null && __instance.pulseAnimationState.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0.0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;
                __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_BodyHeatMeter), "LateUpdate")]
        class uGUI_BodyHeatMeter_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_BodyHeatMeter __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
                    return true;
                int num1 = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player player = Player.main;
                if (player != null)
                {
                    BodyTemperature bt = player.GetComponent<BodyTemperature>();
                    if (bt != null)
                    {
                        float currentBodyHeatValue = bt.currentBodyHeatValue;
                        float maxBodyHeatValue = bt.maxBodyHeatValue;
                        __instance.SetValue(currentBodyHeatValue, maxBodyHeatValue);
                        float time = 1f - Mathf.Clamp01(currentBodyHeatValue / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0f)
                            __instance.pulseDelay = 0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0f)
                            __instance.pulseTime = 0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0f && num2 <= 0f)
                            __instance.statePulse.normalizedTime = 0f;
                        __instance.pulseTween.duration = num2;
                        Vector4 overlay1St = __instance.bar.overlay1ST;
                        overlay1St.w = -Time.time * __instance.overlay1Speed;
                        __instance.bar.overlay1ST = overlay1St;
                        Vector4 overlay2St = __instance.bar.overlay2ST;
                        overlay2St.w = -Time.time * __instance.overlay2Speed;
                        __instance.bar.overlay2ST = overlay2St;
                        float num3 = Mathf.Clamp01(MathExtensions.EvaluateLine(0.5f, 1f, 1f, 0f, currentBodyHeatValue / maxBodyHeatValue));
                        __instance.bar.overlay1Alpha = num3 * __instance.overlay1Alpha;
                        __instance.bar.overlay2Alpha = num3 * __instance.overlay2Alpha;
                    }
                    PDA pda = player.GetPDA();
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if (__instance.stateMaximize.normalizedTime > 0.5F)
                    __instance.showNumbers = false;
                int num4 = __instance.showNumbers ? 1 : 0;
                if (num1 != num4)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;
                __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_InputGroup))]
        class uGUI_InputGroup_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnSelect")]
            static void OnSelectPostfix(uGUI_InputGroup __instance)
            {
                //AddDebug("uGUI_InputGroup OnSelect");
                textInput = true;
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnDeselect")]
            static void OnDeselectPostfix(uGUI_InputGroup __instance)
            {
                //AddDebug("uGUI_InputGroup OnDeselect");
                textInput = false;
            }
        }


    }
}
