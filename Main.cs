
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

        public static Component CopyComponent(Component original, GameObject destination)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            var dst = destination.GetComponent(type) as T;
            if (!dst) dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(dst, field.GetValue(original));
            }
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            }
            return dst as T;
        }

        public static T[] GetComponentsInDirectChildren<T>(Component parent, bool includeInactive = false) where T : Component
        {
            List<T> tmpList = new List<T>();
            foreach (Transform transform in parent.transform)
            {
                if (includeInactive || transform.gameObject.activeInHierarchy)
                    tmpList.AddRange(transform.GetComponents<T>());
            }
            return tmpList.ToArray();
        }

        public static T[] GetComponentsInDirectChildren<T>(GameObject parent, bool includeInactive = false) where T : Component
        {
            List<T> tmpList = new List<T>();
            foreach (Transform transform in parent.transform)
            {
                if (includeInactive || transform.gameObject.activeInHierarchy)
                {
                    T[] components = transform.GetComponents<T>();
                    if (components.Length > 0)
                        tmpList.AddRange(components);
                }
            }
            return tmpList.ToArray();
        }

        public static GameObject GetParent(GameObject go)
        {
            //if (go.name.Contains("(Clone)"))
            if (go.GetComponent<PrefabIdentifier>())
            {
                //AddDebug("name " + go.name);
                return go;
            }
            Transform t = go.transform;
            while (t.parent != null)
            {
                //if (t.parent.name.Contains("(Clone)"))
                if (t.parent.GetComponent<PrefabIdentifier>())
                {
                    //AddDebug("parent.name " + t.parent.name);
                    return t.parent.gameObject;
                }
                t = t.parent.transform;
            }
            return null;
        }

        public static float GetTemperature(GameObject go)
        {
            //AddDebug("GetTemperature " + go.name);
            if (go.GetComponentInParent<Player>())
                return GetPlayerTemperature();

            if (go.transform.parent && go.transform.parent.parent)
            {
                Fridge fridge = go.transform.parent.parent.GetComponent<Fridge>();
                if (fridge && fridge.powerConsumer.IsPowered())
                {
                    //AddDebug("GetTemperature " + go.name + " in fridge");
                    return -1f;
                }
            }

            IInteriorSpace currentInterior = go.GetComponentInParent<IInteriorSpace>();
            if (currentInterior != null)
                return currentInterior.GetInsideTemperature();

            if (go.transform.position.y < Ocean.GetOceanLevel())
                return WaterTemperatureSimulation.main.GetTemperature(go.transform.position);
            else
                return WeatherManager.main.GetFeelsLikeTemperature();
        }

        public static float GetPlayerTemperature()
        {
            //AddDebug("GetPlayerTemperature ");
            //IInteriorSpace currentInterior = Player.main.GetComponentInParent<IInteriorSpace>();
            //if (currentInterior != null)
            //    return currentInterior.GetInsideTemperature();
            if (Player.main.inExosuit)
            {
                if (config.useRealTempForColdMeter && Player.main.currentMountedVehicle.IsPowered())
                    return config.vehicleTemp;
                else if (!config.useRealTempForColdMeter)
                    return config.vehicleTemp;
            }
            else if (Player.main.inHovercraft && !config.useRealTempForColdMeter)
            {
                return config.vehicleTemp;
            }
            else if(Player.main._currentInterior != null && !Player.main._currentInterior.Equals(null) && Player.main._currentInterior is SeaTruckSegment)
            {
                SeaTruckSegment sts = Player.main._currentInterior as SeaTruckSegment;
                //AddDebug("SeaTruck IsPowered " + sts.relay.IsPowered());
                if (sts.relay.IsPowered())
                    return config.vehicleTemp;
            }
            return Player_Patches.ambientTemperature;
        }

        public static float NormalizeTo01range(int value, int min, int max)
        {
            float fl;
            int oldRange = max - min;

            if (oldRange == 0)
                fl = 0f;
            else
                fl = ((float)value - (float)min) / (float)oldRange;

            return fl;
        }

        public static float NormalizeTo01range(float value, float min, float max)
        {
            float fl;
            float oldRange = max - min;

            if (oldRange == 0)
                fl = 0f;
            else
                fl = ((float)value - (float)min) / (float)oldRange;

            return fl;
        }

        public static float NormalizeToRange(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float oldRange = oldMax - oldMin;
            float newValue;

            if (oldRange == 0)
                newValue = newMin;
            else
            {
                float newRange = newMax - newMin;
                newValue = ((value - oldMin) * newRange) / oldRange + newMin;
            }
            return newValue;
        }

        public static bool IsEatableFishAlive(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            Eatable eatable = go.GetComponent<Eatable>();
            LiveMixin liveMixin = go.GetComponent<LiveMixin>();
            if (creature && eatable && liveMixin && liveMixin.IsAlive())
                return true;
            else
                return false;
        }

        public static bool IsEatableFish(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            Eatable eatable = go.GetComponent<Eatable>();
            if (creature && eatable)
                return true;
            else
                return false;
        }

        public static void CookFish(GameObject go)
        {
            //int currentSlot = Inventory.main.quickSlots.desiredSlot;
            //AddDebug("currentSlot " + currentSlot);
            Inventory.main.quickSlots.DeselectImmediate();
            //Inventory.main._container.DestroyItem(tt);
            //Inventory.main.ConsumeResourcesForRecipe(tt);
            TechType processed = TechData.GetProcessed(CraftData.GetTechType(go));
            if (processed != TechType.None)
            { // cooked fish cant be in quickslot
              //AddDebug("CookFish " + processed);
              //UWE.CoroutineHost.StartCoroutine(Main.AddToInventory(processed));
                CraftData.AddToInventory(processed);
                //Inventory.main.quickSlots.desiredSlot
                UnityEngine.Object.Destroy(go);
                //Inventory.main.quickSlots.SelectInternal(int slotID);
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

        public static void Message(string str)
        {
            int count = main.messages.Count;

            if (count == 0)
            {
                AddDebug(str);
            }
            else
            {
                _Message message = main.messages[main.messages.Count - 1];
                message.messageText = str;
                message.entry.text = str;
            }
        }

        public static void Log(string str, QModManager.Utility.Logger.Level lvl = QModManager.Utility.Logger.Level.Debug)
        {
            QModManager.Utility.Logger.Log(lvl, str);
        }

        public static bool IsPlayerInVehicle()
        {
            if (Player.main._currentInterior != null && !Player.main._currentInterior.Equals(null) && Player.main._currentInterior is SeaTruckSegment)
                return true;

            return Player.main.inExosuit || Player.main.inHovercraft;
        }

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

        public static IEnumerator PlaySound(FMODAsset sound, float delay)
        {
            yield return new WaitForSeconds(delay);
            //AddDebug("PlaySound " + sound.name);
            Utils.PlayFMODAsset(sound, Player.main.transform);
        }

        static IEnumerator SelectEquippedItem()
        { // need this for seaglide
            while (!uGUI.main.hud.active)
                yield return null;
            yield return new WaitForSeconds(.5f);
            if (config.activeSlot != -1)
            {
                //Inventory.main.quickSlots.SelectImmediate(config.activeSlot);
                //Inventory.main.quickSlots.DeselectImmediate();
                Inventory.main.quickSlots.Select(config.activeSlot);
            }
        }

        static public IEnumerator AddToInventory(TechType techType)
        {
            GameObject gameObject = null;
            //AddDebug("AddToInventory " + techType);
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(techType, (IOut<GameObject>)result);
            gameObject = result.Get();
            result = (TaskResult<GameObject>)null;
            if (gameObject != null)
            {
                //addedToInv = gameObject;
                //Eatable eatable = gameObject.GetComponent<Eatable>();
                //if (eatable != null)
                //    eatable.SetDecomposes(true); gameObject.EnsureComponent<EcoTarget>().SetTargetType(EcoTargetType.DeadMeat);
                Pickupable pickupable = gameObject.GetComponent<Pickupable>();
                if (pickupable)
                {
                    Inventory.main.ForcePickup(pickupable);
                    //AddDebug("ForcePickup " + pickupable.name);
                }
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
                    UWE.CoroutineHost.StartCoroutine(SelectEquippedItem());
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