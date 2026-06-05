
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        public const string
            MODNAME = "Tweaks and Fixes",
            GUID = "qqqbbb.subnauticaBZ.tweaksAndFixes",
            VERSION = "3.9.0";

        public static bool baseLightSwitchLoaded = false;
        public static bool visibleLockerInteriorModLoaded = false;
        public static ManualLogSource logger;
        static string configPath = Paths.ConfigPath + Path.DirectorySeparatorChar + MODNAME + Path.DirectorySeparatorChar + "ConfigToEdit.cfg";
        static string configMenuPath = Paths.ConfigPath + Path.DirectorySeparatorChar + MODNAME + Path.DirectorySeparatorChar + "ConfigMenu.cfg";
        internal static OptionsMenu options;
        public static ConfigFile configMenu;
        public static ConfigMain configMain = new ConfigMain();
        public static ConfigFile configToEdit;
        public static bool gameLoaded; // uGUI.isLoading is false in main menu

        //[HarmonyPatch(typeof(IngameMenu), "QuitGameAsync")]
        internal class IngameMenu_QuitGameAsync_Patch
        {
            public static void Postfix(IngameMenu __instance, bool quitToDesktop)
            {
                //AddDebug("QuitGameAsync " + quitToDesktop);
                if (!quitToDesktop)
                    CleanUp();
            }
        }

        //[HarmonyPatch(typeof(Language), "Awake")]
        class Language_Awake_Patch
        {
            static void Postfix(Language __instance)
            {
                if (Language.main.currentLanguage == "English")
                {
                    //AddDebug("English"); Tooltip_SeaTruckUpgradeHorsePower
                    //LanguageHandler.SetLanguageLine("Tooltip_Bladderfish", "Unique outer membrane has potential as a natural water filter. Can also be used as a source of oxygen.");
                    //LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, "Unique outer membrane has potential as a natural water filter. Provides some oxygen when consumed raw.");

                }
            }
        }

        public static void CleanUp()
        {
            gameLoaded = false;
            //AddDebug("CleanUp");
            //logger.LogDebug("CleanUp !!!");
            QuickSlots_Patch.invChanged = true;
            Crush_Damage.extraCrushDepth = 0;
            Crush_Damage.crushDamageResistance = 0;
            Gravsphere_Patch.gravSphereFish.Clear();
            Seatruck_Patch.installedUpgrades.Clear();
            //fridges.Clear();
            Base_Patch.baseHullStrengths.Clear();
            PowerConsumption.seatruckPRs.Clear();
            CreatureDeath_Patch.creatureDeathsToDestroy.Clear();
            Survival_.healTime = 0;
            Pickupable_.pickupableStorage.Clear();
            Pickupable_.pickupableStorage_.Clear();
            Base_Light.VehicleDockingBay_Patch.savedPowerStatus.Clear();
            configMain.Load();
        }

        //[HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Finalizer(Player __instance)
            {
                //AddDebug("Player Start Finalizer");

            }
        }

        public static void LoadedGameSetup()
        {
            //AddDebug(" LoadedGameSetup ");
            PrefabFixer prefabFixer = new PrefabFixer();
            prefabFixer.IterateRootGameObjects();
            UWE.CoroutineHost.StartCoroutine(Util.SelectEquippedItem());
            KnownTech.Add(TechType.SnowBall, false, false);
            CreatureDeath_Patch.TryRemoveCorpses();
            if (PDAScanner.mapping.ContainsKey(TechType.Creepvine))
            { // unlock fibermesh by scanning creepvine
                PDAScanner.mapping[TechType.Creepvine].blueprint = TechType.FiberMesh;
            }
            Player_Movement.UpdateModifiers();
            Player.main.isUnderwaterForSwimming.changedEvent.AddHandler(Player.main, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(Player_Movement.OnPlayerUnderwaterChanged));
            MiscSettings.cameraBobbing = ConfigToEdit.cameraBobbing.Value;
            Application.runInBackground = MiscSettings.runInBackground;
            Drop_items_anywhere.OnGameLoadingFinished();
            gameLoaded = true;
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "Start")]
        class uGUI_MainMenu_Start_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
            }
        }

        [HarmonyPatch(typeof(MainMenuLoadButton), "Delete")]
        class MainMenuLoadButton_Delete_Patch
        {
            static void Postfix(MainMenuLoadButton __instance)
            {
                //AddDebug("MainMenuLoadButton Delete " + __instance.saveGame);
                //DeleteSaveSlotData(__instance.saveGame);
                configMain.DeleteCurrentSaveSlotData();
            }
        }

        static void SaveData()
        {
            //AddDebug("SaveData activeSlot " + Inventory.main.quickSlots.activeSlot);
            //logger.LogMessage("SaveData activeSlot " + Inventory.main.quickSlots.activeSlot);
            configMain.screenRes = new Screen_Resolution_Fix.ScreenRes(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen);
            if (Drop_Pod_Patch.podPowerSource && configMain.podPower != null)
                configMain.podPower[SaveLoadManager.main.currentSlot] = Drop_Pod_Patch.podPowerSource.power;

            configMain.activeSlot = Inventory.main.quickSlots.activeSlot;
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;

            if (heldItem != null)
            {
                PlaceTool pt = heldItem.item.GetComponent<PlaceTool>();
                if (pt)
                {
                    //AddDebug(" heldItem PlaceTool");
                    configMain.activeSlot = -1;
                }
            }
            configMain.Save();
        }

        //[HarmonyPatch(typeof(IngameMenu), "SaveGameAsync")]
        class IngameMenu_SaveGameAsync_Patch
        {
            static void Postfix(IngameMenu __instance)
            {
                AddDebug("IngameMenu SaveGameAsync ");
                configMain.Save();
                //DeleteSaveSlotData(__instance.saveGame);

            }
        }

        private void Start()
        {
            //config.Load();
            logger = this.Logger;
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
            Setup();
            configMain.Load();
            //RecipeData recipeData = new RecipeData(); 
            //recipeData.Ingredients = new List<Ingredient>()
            //{
            //    new Ingredient(TechType.Titanium, 3),
            //    new Ingredient(TechType.WiringKit, 1)
            //};
            //CraftDataHandler.SetTechData(TechType.CyclopsDecoy, recipeData);
            //CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.CyclopsDecoy, new string[1] { "Decoy" });
            Logger.LogInfo($"Plugin {MODNAME} {VERSION} is loaded ");
        }

        private static void RegisterSpawns()
        {
            //CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-50f, -11f, -430f)));
            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType.ScrapMetal, new Vector3(-304f, 15.3f, 256.36f), new Vector3(4f, 114.77f, 0f)));
            // thermalzone_rock_01_single_a
            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo("9c331be3-984a-4a6d-a040-5ffebb50f106", new Vector3(21f, -39.5f, -364.3f), new Vector3(30f, 50f, 340f))); // 21 -39.5 -364.3
            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo("9c331be3-984a-4a6d-a040-5ffebb50f106", new Vector3(-133f, -374f, -1336f), new Vector3(0, 308.571f, 0))); //  -133 -373 -1342
            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo("a3f8c8e0-0a2c-4f9b-b585-8804d15bc04b", new Vector3(-412.3f, -100.79f, -388.2f), new Vector3(310f, 0f, 90f))); // -412.3 -100.79 -388.2   
        }

        public void Setup()
        {
            configToEdit = new ConfigFile(configPath, true);
            ConfigToEdit.Bind();
            configMenu = new ConfigFile(configMenuPath, true);
            ConfigMenu.Bind();
            WaitScreenHandler.RegisterLateLoadTask(MODNAME, task => LoadedGameSetup());
            WaitScreenHandler.RegisterEarlyLoadTask(MODNAME, task => StartLoadingSetup());
            LanguageHandler.RegisterLocalizationFolder();
            SaveUtils.RegisterOnSaveEvent(SaveData);
            SaveUtils.RegisterOnQuitEvent(CleanUp);
            GetLoadedMods();
            ConfigToEdit.ParseConfig();
            options = new OptionsMenu();
            OptionsPanelHandler.RegisterModOptions(options);
            RegisterSpawns();
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
            //logger.LogDebug("Setup done ");
            Application.runInBackground = MiscSettings.runInBackground;
            //// vanilla desc just copies the name
            //LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeHorsePower, config.translatableStrings[21]);
            //// vanilla desc does not tell percent
            //LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeEnergyEfficiency, config.translatableStrings[22]);
        }

        public static void GetLoadedMods()
        {
            baseLightSwitchLoaded = Chainloader.PluginInfos.ContainsKey("com.ahk1221.baselightswitch") || Chainloader.PluginInfos.ContainsKey("Cookie_BaseLightSwitch");
            visibleLockerInteriorModLoaded = Chainloader.PluginInfos.ContainsKey("VisibleLockerInterior");
            //foreach (var plugin in Chainloader.PluginInfos)
            //    logger.LogInfo("loaded Mod " + plugin.Value);
        }

        private static void StartLoadingSetup()
        {
            AddTechTypesToClassIDtable();
            PrefabFixer prefabFixer = new PrefabFixer();
            prefabFixer.FixGlassPrefabs();
            BasePrefabFixer basePrefabFixer = new BasePrefabFixer();
            UWE.CoroutineHost.StartCoroutine(basePrefabFixer.FixBasePrefabs());
            Application.runInBackground = true;
        }

        private static void AddTechTypesToClassIDtable()
        {
            CraftData.PreparePrefabIDCache();
            CraftData.entClassTechTable["ef9ca323-9e02-4903-991c-eb3e597a279d"] = TechType.HoneyCombPlant;
            CraftData.entClassTechTable["cb000fd6-a31c-4a3a-97cd-d60a37eb8237"] = TechType.BarTable;
            //CraftData.entClassTechTable["2168257e-2533-403f-8b3a-a3bef63adaf9"] = TechType.HoverpadFragment;
            CraftData.entClassTechTable["5c6464c0-96e8-45dc-92ba-a68adb32017a"] = TechType.EscapePod;

            CraftData.entClassTechTable["83e334be-7d95-42bb-843d-47a7796d3396"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["76192466-013f-4214-b66a-c516b4be6538"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["5f8574f6-f76c-4f7b-80a0-8c7515206ea3"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["d58d925a-92ce-4c52-9e8f-6c2f6c62262f"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["02c1599d-bd38-443c-9636-4c9b8d7630cf"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["f58d2434-22f1-4e5f-9c44-20abdd549c38"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["c4daee1d-5dd0-4f0b-bc0b-3ab9bce2275f"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["55de6ed4-9d9e-45fc-b38c-1cd2b062a9f5"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["04d70662-c81e-432a-91bb-1b66c2f2d79e"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["1b97d93a-3a4b-4595-a212-4d15e2b2fe1d"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["807db98b-fb96-49a8-afec-b2ac9deea6e9"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["5f7acec4-e4b4-4d0a-9a3c-414a92bdc6c3"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["b4bd9288-82d9-4f01-95f2-0840aad66938"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["5d275278-3c52-46c0-91ca-be60c5932596"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["f5ff3e70-0526-4e54-9d87-6088a82b52f2"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["1364f0a9-37b9-4f00-987d-6f7be63d2925"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["88e879ac-9062-4155-ab10-78e8db2b1332"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["04edfa62-d232-40f8-a708-057ec34d99bf"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["a847db5e-bd9e-4863-ac36-769feae45168"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["0d419d64-9270-4d4e-b35d-26d0fbad0d9e"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["1b221b0c-3ec4-4bb5-8f0c-ff1bb28ce3e7"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["c41c82ee-5755-43ff-9f9e-306c9ad955c8"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["4e1147b1-843b-4e40-bbbc-222da10ab648"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["a5f82461-b30f-4008-bb79-a6c091e8fa32"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["5ec56d5c-1f1c-4697-81de-e36aa6bbe17f"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["45da5656-1b9a-4d3f-b0e1-b696519d4a9e"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["d043d6db-9b59-44f9-b597-22e39f04c491"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["d8faacb4-1cea-41ab-bc23-a85bd8b6a0f8"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["1952f16d-6a4b-4621-9f78-a0ff122732c9"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["7cfbc363-ea02-4532-a488-fe1fbbb4a910"] = TechType.LilyPadRoot;
            CraftData.entClassTechTable["f084b1f7-acd7-42ea-b1c9-45495e5a6da8"] = TechType.LilyPadRoot;

            CraftData.entClassTechTable["01c92aa5-6fc6-498f-8a39-5ba6faac9ba6"] = TechType.LilyPadMature;
            CraftData.entClassTechTable["e0846473-ebe7-4c39-829f-2cb4d51c309f"] = TechType.LilyPadMature;
            CraftData.entClassTechTable["284f0ace-ec6b-417e-b716-61f3e57ee983"] = TechType.LilyPadMature;
            CraftData.entClassTechTable["4441d77a-1fa8-4090-9cb0-20107eca0413"] = TechType.LilyPadMature;
            CraftData.entClassTechTable["aedaf376-aa3a-4679-a4eb-7d0d87983057"] = TechType.LilyPadMature;
            CraftData.entClassTechTable["6bb26dce-4734-4356-88b5-4572e603d25f"] = TechType.LilyPadMature;
        }

        [HarmonyPatch(typeof(ApplicationFocus), "OnRunInBackgroundChanged")]
        class ApplicationFocus_OnRunInBackgroundChanged_Patch
        {
            public static void Postfix(ApplicationFocus __instance)
            {
                //AddDebug("OnRunInBackgroundChanged " + MiscSettings.runInBackground);
                Application.runInBackground = MiscSettings.runInBackground;
            }
        }

    }
}