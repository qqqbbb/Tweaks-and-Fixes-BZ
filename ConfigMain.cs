using BepInEx;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    public class ConfigMain : JsonFile
    {
        public ConfigMain()
        {
        }

        public override string JsonFilePath => Paths.ConfigPath + Path.DirectorySeparatorChar + Main.MODNAME + Path.DirectorySeparatorChar + "config.json";
        public Screen_Resolution_Fix.ScreenRes screenRes;
        public Dictionary<string, HashSet<string>> exosuitLights = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> seaglideLights = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> seaglideMap = new Dictionary<string, HashSet<string>>();
        public int activeSlot = -1;
        public Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>> lockerNames = new Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>>();
        static public Dictionary<string, float> hpToHeal = new Dictionary<string, float>();
        public Dictionary<string, HashSet<Vector3Int>> iceFruitsPicked = new Dictionary<string, HashSet<Vector3Int>>();

        public Dictionary<string, float> podPower = new Dictionary<string, float>();
        //public Dictionary<string, HashSet<string>> objectsSurvivedDespawn = new Dictionary<string, HashSet<string>> { };
        //public HashSet<string> objectsDespawned = new HashSet<string> { };
        //public List<string> removeLight = new List<string> { };
        //public List<string> biomesRemoveLight = new List<string> { };

        public Dictionary<string, HashSet<string>> baseLights = new Dictionary<string, HashSet<string>>();

        internal void DeleteCurrentSaveSlotData()
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            podPower.Remove(currentSlot);
            iceFruitsPicked.Remove(currentSlot);
            lockerNames.Remove(currentSlot);
            baseLights.Remove(currentSlot);
            hpToHeal.Remove(currentSlot);
            seaglideMap.Remove(currentSlot);
            seaglideLights.Remove(currentSlot);
            exosuitLights.Remove(currentSlot);
            Save();
        }

        internal static float GetHPtoHeal()
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (hpToHeal.ContainsKey(currentSlot))
                return hpToHeal[currentSlot];

            return 0;
        }

        internal void SetHPtoHeal(float hp)
        {
            if (hp < 0)
                hp = 0;

            hpToHeal[SaveLoadManager.main.currentSlot] = hp;
        }

        internal bool GetBaseLights(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (baseLights.ContainsKey(currentSlot))
            {
                int x = (int)pos.x;
                int y = (int)pos.y;
                int z = (int)pos.z;
                string key = x + "_" + y + "_" + z;
                if (baseLights[currentSlot].Contains(key))
                    return false;
            }
            return true;
        }

        internal bool GetBaseLights()
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (baseLights.ContainsKey(currentSlot) && Player.main.currentSub && Player.main.currentSub.isBase)
            {
                Vector3 pos = Player.main.currentSub.transform.position;
                int x = (int)pos.x;
                int y = (int)pos.y;
                int z = (int)pos.z;
                string key = x + "_" + y + "_" + z;
                if (baseLights[currentSlot].Contains(key))
                    return false;
            }
            return true;
        }

        internal void SaveBaseLights(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (baseLights.ContainsKey(currentSlot) == false)
                baseLights[currentSlot] = new HashSet<string>();

            int x = (int)pos.x;
            int y = (int)pos.y;
            int z = (int)pos.z;
            string key = x + "_" + y + "_" + z;
            baseLights[currentSlot].Add(key);
        }

        internal void DeleteBaseLights(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (baseLights.ContainsKey(currentSlot) == false)
                return;

            int x = (int)pos.x;
            int y = (int)pos.y;
            int z = (int)pos.z;
            string key = x + "_" + y + "_" + z;
            baseLights[currentSlot].Remove(key);
        }

        internal bool GetExosuitLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (exosuitLights.ContainsKey(currentSlot) == false)
                return false;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return false;

            //AddDebug("GetExosuitLights " + exosuitLights[currentSlot].Contains(pi.id));
            return exosuitLights[currentSlot].Contains(pi.id);
        }

        internal bool GetSeaglideLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideLights.ContainsKey(currentSlot) == false)
                return false;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return false;

            return seaglideLights[currentSlot].Contains(pi.id);
        }

        internal bool GetSeaglideMap(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideMap.ContainsKey(currentSlot) == false)
                return true;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return true;

            return !seaglideMap[currentSlot].Contains(pi.id);
        }

        internal void SaveSeaglideMap(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideMap.ContainsKey(currentSlot) == false)
                seaglideMap[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                seaglideMap[currentSlot].Add(pi.id);
        }

        internal void DeleteSeaglideMap(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideMap.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    seaglideMap[currentSlot].Remove(pi.id);
            }
        }

        internal void SaveExosuitLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (exosuitLights.ContainsKey(currentSlot) == false)
                exosuitLights[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                exosuitLights[currentSlot].Add(pi.id);
        }

        internal void SaveSeaglideLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideLights.ContainsKey(currentSlot) == false)
                seaglideLights[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                seaglideLights[currentSlot].Add(pi.id);
        }

        internal void DeleteSeaglideLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideLights.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    seaglideLights[currentSlot].Remove(pi.id);
            }
        }

        internal void DeleteExosuitLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (exosuitLights.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    exosuitLights[currentSlot].Remove(pi.id);
            }
        }
        //public Dictionary<string, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<string, Dictionary<TechType, int>> { { "Stalker", new Dictionary<TechType, int> { { TechType.StalkerTooth, 2 } } }, { "Gasopod", new Dictionary<TechType, int> { { TechType.GasPod, 5 } } } };

        //public bool LEDLightWorksInHand = true;
        //public int growingPlantUpdateInterval = 0;
        //public bool pdaTabSwitchHotkey = true;


    }

}