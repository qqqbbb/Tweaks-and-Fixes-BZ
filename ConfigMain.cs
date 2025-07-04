﻿using BepInEx;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
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
        public bool exosuitLights = false;
        public bool seaglideLights = false;
        public bool seaglideMap = false;
        public int subThrottleIndex = -1;
        public int activeSlot = -1;

        public Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>> lockerNames = new Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>>();
        static public Dictionary<string, float> HPtoHeal = new Dictionary<string, float>();
        public Dictionary<string, HashSet<Vector3Int>> iceFruitsPicked = new Dictionary<string, HashSet<Vector3Int>>();

        public Dictionary<string, float> podPower = new Dictionary<string, float>();
        //public Dictionary<string, HashSet<string>> objectsSurvivedDespawn = new Dictionary<string, HashSet<string>> { };
        //public HashSet<string> objectsDespawned = new HashSet<string> { };
        //public List<string> removeLight = new List<string> { };
        //public List<string> biomesRemoveLight = new List<string> { };

        public Dictionary<string, Dictionary<string, bool>> baseLights = new Dictionary<string, Dictionary<string, bool>>();

        internal static float GetHPtoHeal()
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (HPtoHeal.ContainsKey(currentSlot))
                return HPtoHeal[currentSlot];

            return 0;
        }

        internal void SetHPtoHeal(float hp)
        {
            if (hp < 0)
                hp = 0;

            HPtoHeal[SaveLoadManager.main.currentSlot] = hp;
        }
        //public Dictionary<string, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<string, Dictionary<TechType, int>> { { "Stalker", new Dictionary<TechType, int> { { TechType.StalkerTooth, 2 } } }, { "Gasopod", new Dictionary<TechType, int> { { TechType.GasPod, 5 } } } };

        //public bool LEDLightWorksInHand = true;
        //public int growingPlantUpdateInterval = 0;
        //public bool pdaTabSwitchHotkey = true;


    }

}