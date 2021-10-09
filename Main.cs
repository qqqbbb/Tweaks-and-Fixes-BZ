
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
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [QModCore]
    public class Main
    {
        public static GUIHand guiHand;
        public static PDA pda;
        public static Survival survival;
        public static float oceanLevel;
        public static Equipment equipment;
        public static bool crafterOpen = false;
        public static bool canBreathe = false;
        public static bool loadingDone = false;
        public static bool english = false;
        public static System.Random rndm = new System.Random();

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

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

        public static int NormalizeToRange(int value, int oldMin, int oldMax, int newMin, int newMax)
        {
            int oldRange = oldMax - oldMin;
            int newValue;

            if (oldRange == 0)
                newValue = newMin;
            else
            {
                int newRange = newMax - newMin;
                newValue = ((value - oldMin) * newRange) / oldRange + newMin;
            }
            return newValue;
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
            //if (creature && eatable && liveMixin && liveMixin.IsAlive())
            //    return true;

            return creature && eatable && liveMixin && liveMixin.IsAlive();
        }

        public static bool IsEatableFish(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            Eatable eatable = go.GetComponent<Eatable>();
            //if (creature && eatable)
            //    return true;

            return creature && eatable;
        }

        public static void CleanUp()
        {
            loadingDone = false;
            canBreathe = false;
            //AddDebug("CleanUp");
            QuickSlots_Patch.invChanged = true;
            Base_Light.bcls = new HashSet<BaseCellLighting>();
            Crush_Damage.extraCrushDepth = 0;
            crafterOpen = false;
            Gravsphere_Patch.gravSphereFish = new HashSet<Pickupable>();
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
                //IngameMenuHandler.RegisterOnSaveEvent(config.Save);
                guiHand = __instance.GetComponent<GUIHand>();
                pda = __instance.GetPDA();
                oceanLevel = Ocean.GetOceanLevel();
                equipment = Inventory.main.equipment;
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

        [HarmonyPatch(typeof(WaitScreen), nameof(WaitScreen.Hide))]
        internal class WaitScreen_Hide_Patch
        { // fires after game loads
            public static void Postfix(WaitScreen __instance)
            {
                if (!loadingDone)
                {
                    //AddDebug(" WaitScreen Hide");
                    UWE.CoroutineHost.StartCoroutine(SelectEquippedItem());

                    loadingDone = true;
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
                loadingDone = true;
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
                config.podPower.Remove(SaveLoadManager.main.currentSlot);
                config.Save();
            }
        }

        [HarmonyPatch(typeof(GhostCrafter), "OnOpenedChanged")]
        class GhostCrafter_OnOpenedChanged_patch
        {
            public static void Postfix(GhostCrafter __instance, bool opened)
            {
                //AddDebug(" GhostCrafter OnOpenedChanged " + opened);
                crafterOpen = opened;
            }
        }

        static void SaveData()
        {
            //AddDebug("SaveData " + Inventory.main.quickSlots.activeSlot);
            //Main.config.activeSlot = Inventory.main.quickSlots.activeSlot;
            if (Player.main.mode == Player.Mode.Normal)
                config.playerCamRot = MainCameraControl.main.viewModel.localRotation.eulerAngles.y;
            else
                config.playerCamRot = -1f;

            if (Drop_Pod_Patch.podPowerSource)
                config.podPower[SaveLoadManager.main.currentSlot] = Drop_Pod_Patch.podPowerSource.power;

            config.activeSlot = Inventory.main.quickSlots.activeSlot;
            //InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
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

            LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, "Unique outer membrane has potential as a natural water filter. Provides some oxygen when consumed raw.");
        }
        //public static bool dayNightSpeedLoaded = false;
        [QModPostPatch]
        public static void PostPatch()
        {
            //Log("PostPatch GetCurrentLanguage " + Language.main.GetCurrentLanguage());
            english = Language.main.GetCurrentLanguage() == "English";
            //IQMod iqMod = QModServices.Main.FindModById("DayNightSpeed");
            //dayNightSpeedLoaded = iqMod != null;
            LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeHorsePower, "Increases the Seatruck's speed when hauling two or more modules.");
            LanguageHandler.SetTechTypeTooltip(TechType.SeaTruckUpgradeEnergyEfficiency, "Reduces vehicle energy consumption by 20% percent.");
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