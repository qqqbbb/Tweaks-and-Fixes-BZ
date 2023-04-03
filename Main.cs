
using HarmonyLib;
using QModManager.API.ModLoading;
using QModManager.API;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using static ErrorMessage;
//GameModeManager.GetOption<bool>(GameOption.Hunger)
//uGUI.isLoading
namespace Tweaks_Fixes
{
    [QModCore]
    public class Main
    {
        public static Survival survival;
        public static BodyTemperature bodyTemperature;
        public static float oceanLevel;
        public static bool canBreathe = false;
        //!uGUI.isLoading
        //public static bool loadingDone = false;
        //public static bool languageCheck = false;
        public static System.Random rndm = new System.Random();
        public static List<ItemsContainer> fridges = new List<ItemsContainer>();
        public static bool baseLightSwitchLoaded = false;
        public static bool visibleLockerInteriorModLoaded = false;

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

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
            //loadingDone = false;
            canBreathe = false;
            //AddDebug("CleanUp");
            //Log("CleanUp !!!");
            QuickSlots_Patch.invChanged = true;
            //Base_Patch.bcls = new HashSet<BaseCellLighting>();
            Crush_Damage.extraCrushDepth = 0;
            //crafterOpen = false;
            Gravsphere_Patch.gravSphereFish = new HashSet<Pickupable>();
            CraftTree.fabricator = new CraftTree("Fabricator", CraftTree.FabricatorScheme());
            Seatruck_Patch.installedUpgrades = new HashSet<TechType>();
            fridges = new List<ItemsContainer>();
            UI_Patches.recyclotrons = new Dictionary<ItemsContainer, Recyclotron>();
            //Base_Patch.baseBuilt = new Dictionary<SubRoot, bool>();
            Tools_Patch.fixedFish = new List<PlayerTool>();
            config.Load();
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Postfix(Player __instance)
            {
                survival = __instance.GetComponent<Survival>();
                bodyTemperature = __instance.GetComponent<BodyTemperature>();
                //IngameMenuHandler.RegisterOnSaveEvent(config.Save);
                //guiHand = __instance.GetComponent<GUIHand>();
                //pda = __instance.GetPDA();
                oceanLevel = Ocean.GetOceanLevel();
                //equipment = Inventory.main.equipment;
                //if (config.cantScanExosuitClawArm)
                //    DisableExosuitClawArmScan();
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

        [HarmonyPatch(typeof(WaitScreen), "Hide")]
        internal class WaitScreen_Hide_Patch
        { // fires after game loads
            public static void Postfix(WaitScreen __instance)
            {
                //AddDebug(" WaitScreen Hide");
                //if (uGUI.isLoading)
                {
                    //AddDebug(" WaitScreen Hide  !!!");
                    UWE.CoroutineHost.StartCoroutine(Util.SelectEquippedItem());
                    KnownTech.Add(TechType.SnowBall, false, false);
                    //loadingDone = true;
                }
            }
        }

        //[HarmonyPatch(typeof(uGUI_SceneLoading), "End")]
        internal class uGUI_SceneLoading_End_Patch
        { // fires after game loads
            public static void Postfix(uGUI_SceneLoading __instance)
            {
                //if (!uGUI.main.hud.active)
                //{
                    //AddDebug(" is Loading");
                    //return;
                //}
                AddDebug(" uGUI_SceneLoading end");
                //loadingDone = true;
                //Base_Light.BaseCellLighting_Start_Patch.UpdateBaseLights(true);
                //Base_Light.BaseCellLighting_Start_Patch q = new Base_Light.BaseCellLighting_Start_Patch();
                //q.Invoke("UpdateBaseLights", 3f);
                //Base_Light.BaseCellLighting_Start_Patch.inv;
            }
        }

        [HarmonyPatch(typeof(SaveLoadManager), "ClearSlotAsync")]
        internal class SaveLoadManager_ClearSlotAsync_Patch
        {
            public static void Postfix(SaveLoadManager __instance, string slotName)
            {
                //AddDebug("ClearSlotAsync " + slotName);
                config.podPower.Remove(slotName);
                config.lockerNames.Remove(slotName);
                config.Save();
            }
        }

        static void SaveData()
        {
            //AddDebug("SaveData " + Inventory.main.quickSlots.activeSlot);
            //Main.config.activeSlot = Inventory.main.quickSlots.activeSlot;
            //if (Player.main.mode == Player.Mode.Normal)
            //    config.playerCamRot = MainCameraControl.main.viewModel.localRotation.eulerAngles.y;
            //else
            //    config.playerCamRot = -1f;

            if (Drop_Pod_Patch.podPowerSource)
                config.podPower[SaveLoadManager.main.currentSlot] = Drop_Pod_Patch.podPowerSource.power;

            config.activeSlot = Inventory.main.quickSlots.activeSlot;
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
            if (heldItem != null)
            {
                VehicleInterface_MapController mc = heldItem.item.GetComponent<VehicleInterface_MapController>();
                if (mc)
                {
                    //AddDebug(" save seaglide");
                    config.seaGlideMap = mc.mapActive;
                }
            }
            //if (heldItem.item.GetTechType() == TechType.Seaglide)
            //    config.activeSlot = -1;

            //config.crushDepth -= Crush_Damage.extraCrushDepth;
            config.Save();
            //config.crushDepth += Crush_Damage.extraCrushDepth;
        }

        [QModPatch]
        public static void Load()
        {
            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
            IngameMenuHandler.RegisterOnSaveEvent(SaveData);
            IngameMenuHandler.RegisterOnQuitEvent(CleanUp);
            CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.ScrapMetal, new Vector3(-304f, 15.3f, 256.36f), new Vector3(4f, 114.77f, 0f)));
            CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo("9c331be3-984a-4a6d-a040-5ffebb50f106", new Vector3(21f, -39.5f, -364.3f), new Vector3(30f, 50f, 340f)));
            CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo("a3f8c8e0-0a2c-4f9b-b585-8804d15bc04b", new Vector3(-412.3f, -100.79f, -388.2f), new Vector3(310f, 0f, 90f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-208f, -376f, -1332f), new Vector3(4f, 114.77f, 0f)));
          
            //RecipeData recipeData = new RecipeData();
            //recipeData.Ingredients = new List<Ingredient>()
            //{
            //    new Ingredient(TechType.Titanium, 3),
            //    new Ingredient(TechType.WiringKit, 1)
            //};
            //CraftDataHandler.SetTechData(TechType.CyclopsDecoy, recipeData);
            //CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.CyclopsDecoy, new string[1] { "Decoy" });
        }

        [QModPostPatch]
        public static void PostPatch()
        {
            //Log("PostPatch GetCurrentLanguage " + Language.main.GetCurrentLanguage());
            //Log("translatableStrings.Count " + config.translatableStrings.Count);
            //languageCheck = Language.main.GetCurrentLanguage() == "English") || !config.translatableStrings[0].Equals("Burnt out ");
            //IQMod iqMod = QModServices.Main.FindModById("DayNightSpeed");
            baseLightSwitchLoaded = QModServices.Main.ModPresent("BaseLightSwitch");
            visibleLockerInteriorModLoaded = QModServices.Main.ModPresent("lockerMod");

            //LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, config.translatableStrings[19]);
            //LanguageHandler.SetTechTypeTooltip(TechType.SmallStove, config.translatableStrings[20]);
            //// vanilla desc just copies the name
            //LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeHorsePower, config.translatableStrings[21]);
            //// vanilla desc does not tell percent
            //LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeEnergyEfficiency, config.translatableStrings[22]);

            foreach (var item in config.crushDepthEquipment)
            {
                TechTypeExtensions.FromString(item.Key, out TechType tt, true);
                //Log("crushDepthEquipment str " + item.Key);
                //Log("crushDepthEquipment TechType " + tt);
                if (tt != TechType.None)
                    Crush_Damage.crushDepthEquipment[tt] = item.Value;
            }
            foreach (var item in config.itemMass)
            {
                TechTypeExtensions.FromString(item.Key, out TechType tt, true);
                //Log("crushDepthEquipment str " + item.Key);
                //Log("crushDepthEquipment TechType " + tt);
                if (tt != TechType.None)
                    Pickupable_Patch.itemMass[tt] = item.Value;
            }
            foreach (string name in config.gravTrappable)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);

                if (tt != TechType.None)
                    Gravsphere_Patch.gravTrappable.Add(tt);
            }
            foreach (var kv in config.damageMult_)
            {
                TechTypeExtensions.FromString(kv.Key, out TechType tt, true);
                if (tt != TechType.None)
                    Damage_Patch.damageMult.Add(tt, kv.Value);
            }

        }
    }
}