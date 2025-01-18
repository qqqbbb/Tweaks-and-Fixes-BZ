
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
            VERSION = "2.11.0";
        public static Survival survival;
        public static float oceanLevel;
        public static System.Random rndm = new System.Random();
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
            //Player_Movement.invItemsMod = float.MinValue;
            //Player_Movement.equipmentSpeedMod = float.MaxValue;
            //Player_Movement.toolMod = float.MaxValue;
            configMain.Load();
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Postfix(Player __instance)
            {
                survival = __instance.GetComponent<Survival>();
                oceanLevel = Ocean.GetOceanLevel();
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
            gameLoaded = true;
            //if (ConfigToEdit.targetFrameRate.Value > 9)
            //    Application.targetFrameRate = ConfigToEdit.targetFrameRate.Value;
        }


        [HarmonyPatch(typeof(uGUI_MainMenu), "Start")]
        class uGUI_MainMenu_Start_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                if (ConfigToEdit.targetFrameRate.Value >= 10)
                    Application.targetFrameRate = ConfigToEdit.targetFrameRate.Value;
            }
        }

        [HarmonyPatch(typeof(WaitScreen), "Hide")]
        internal class WaitScreen_Hide_Patch
        { // fires after game loads
            public static void Postfix(WaitScreen __instance)
            {
                //AddDebug(" WaitScreen Hide");
                //if (!Main.gameLoaded)
                {
                    //AddDebug(" WaitScreen Hide  !!!");
                    LoadedGameSetup();
                }
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
                AddDebug("IngameMenu SaveGameAsync !!! ");
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
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType.ScrapMetal, new Vector3(-304f, 15.3f, 256.36f), new Vector3(4f, 114.77f, 0f)));
            // thermalzone_rock_01_single_a
            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo("9c331be3-984a-4a6d-a040-5ffebb50f106", new Vector3(21f, -39.5f, -364.3f), new Vector3(30f, 50f, 340f))); // 21 -39.5 -364.3
            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo("9c331be3-984a-4a6d-a040-5ffebb50f106", new Vector3(-133f, -374f, -1336f), new Vector3(0, 308.571f, 0))); //  -133 -373 -1342

            CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo("a3f8c8e0-0a2c-4f9b-b585-8804d15bc04b", new Vector3(-412.3f, -100.79f, -388.2f), new Vector3(310f, 0f, 90f))); // -412.3 -100.79 -388.2   

            //CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-208f, -376f, -1332f), new Vector3(4f, 114.77f, 0f)));

            //RecipeData recipeData = new RecipeData(); 
            //recipeData.Ingredients = new List<Ingredient>()
            //{
            //    new Ingredient(TechType.Titanium, 3),
            //    new Ingredient(TechType.WiringKit, 1)
            //};
            //CraftDataHandler.SetTechData(TechType.CyclopsDecoy, recipeData);
            //CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.CyclopsDecoy, new string[1] { "Decoy" });
        }

        public void Setup()
        {
            configToEdit = new ConfigFile(configPath, true);
            ConfigToEdit.Bind();
            configMenu = new ConfigFile(configMenuPath, true);
            ConfigMenu.Bind();
            //SaveUtils.RegisterOnFinishLoadingEvent(LoadedGameSetup); // runs before game loads
            LanguageHandler.RegisterLocalizationFolder();
            SaveUtils.RegisterOnSaveEvent(SaveData);
            SaveUtils.RegisterOnQuitEvent(CleanUp);
            GetLoadedMods();
            ConfigToEdit.ParseConfig();
            options = new OptionsMenu();
            OptionsPanelHandler.RegisterModOptions(options);
            //// vanilla desc just copies the name
            //LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeHorsePower, config.translatableStrings[21]);
            //// vanilla desc does not tell percent
            //LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeEnergyEfficiency, config.translatableStrings[22]);
        }

        public static void GetLoadedMods()
        {
            //logger.LogInfo("Chainloader.PluginInfos Count " + Chainloader.PluginInfos.Count);
            //AddDebug("Chainloader.PluginInfos Count " + Chainloader.PluginInfos.Count);
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                //logger.LogInfo("loaded Mod " + metadata.GUID);
                if (metadata.GUID == "Cookie_BaseLightSwitch")
                    baseLightSwitchLoaded = true;
                else if (metadata.GUID == "VisibleLockerInterior")
                    visibleLockerInteriorModLoaded = true;

                //"c1oud5_SeatruckLightsSwitch"
            }
        }

    }
}