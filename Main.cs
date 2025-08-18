
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

//GameModeManager.GetOption<bool>(GameOption.Hunger)
namespace Tweaks_Fixes
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        public const string
            MODNAME = "Tweaks and Fixes",
            GUID = "qqqbbb.subnauticaBZ.tweaksAndFixes",
            VERSION = "3.0.0";
        public static Survival survival;
        public static List<ItemsContainer> fridges = new List<ItemsContainer>();
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
            fridges.Clear();
            UI_Patches.recyclotrons.Clear();
            Base_Patch.baseHullStrengths.Clear();
            Tools_Patch.fixedFish.Clear();
            Battery_Patch.seatruckPRs.Clear();
            CreatureDeath_Patch.creatureDeathsToDestroy.Clear();
            Survival_.healTime = 0;
            configMain.Load();
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Finalizer(Player __instance)
            {
                //AddDebug("Player Start Finalizer");
                survival = __instance.GetComponent<Survival>();
                //oceanLevel = Ocean.GetOceanLevel();
                //equipment = Inventory.main.equipment;
            }
        }


        //[HarmonyPatch(typeof(Player), "TrackTravelStats")]
        class Player_TrackTravelStats_Patch
        {
            static void Postfix(Player __instance)
            {

                AddDebug("TrackTravelStats");

            }
        }

        public static void LoadedGameSetup()
        {
            //AddDebug(" LoadedGameSetup ");
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
                DeleteSaveSlotData(__instance.saveGame);
            }
        }

        public static void DeleteSaveSlotData(string slotName)
        {
            //AddDebug("ClearSlotAsync " + slotName);
            configMain.podPower.Remove(slotName);
            configMain.lockerNames.Remove(slotName);
            configMain.iceFruitsPicked.Remove(slotName);
            configMain.Save();
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
                var mc = heldItem.item.GetComponent<VehicleInterface_MapController>();
                if (mc)
                {
                    //AddDebug(" save seaglide");
                    configMain.seaglideMap = mc.mapActive;
                }
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
            //configMain.Load();
            //RecipeData recipeData = new RecipeData(); 
            //recipeData.Ingredients = new List<Ingredient>()
            //{
            //    new Ingredient(TechType.Titanium, 3),
            //    new Ingredient(TechType.WiringKit, 1)
            //};
            //CraftDataHandler.SetTechData(TechType.CyclopsDecoy, recipeData);
            //CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.CyclopsDecoy, new string[1] { "Decoy" });
            Logger.LogInfo($"Plugin {GUID} {VERSION} is loaded ");
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
            LanguageHandler.RegisterLocalizationFolder();
            SaveUtils.RegisterOnSaveEvent(SaveData);
            SaveUtils.RegisterOnQuitEvent(CleanUp);
            GetLoadedMods();
            ConfigToEdit.ParseConfig();
            options = new OptionsMenu();
            OptionsPanelHandler.RegisterModOptions(options);
            AddTechTypesToClassIDtable();
            RegisterSpawns();
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
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

        private static void AddTechTypesToClassIDtable()
        {
            if (CraftData.entClassTechTable == null)
                CraftData.entClassTechTable = new Dictionary<string, TechType>();
            //CraftData.entClassTechTable["769f9f44-30f6-46ed-aaf6-fbba358e1676"] = TechType.BaseBioReactor;
            //CraftData.entClassTechTable["864f7780-a4c3-4bf2-b9c7-f4296388b70f"] = TechType.BaseNuclearReactor;
            CraftData.entClassTechTable["ef9ca323-9e02-4903-991c-eb3e597a279d"] = TechType.HoneyCombPlant;
        }



    }
}