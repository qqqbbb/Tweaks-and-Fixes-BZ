using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Fruits
    {
        const float creepVineSeedLightInt = 1.2f;

        [HarmonyPatch(typeof(FruitPlant))]
        class FruitPlant_Patch
        {
            public static void AwakePostfix(FruitPlant __instance)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                //AddDebug("FruitPlant Awake " + techType);
                if (techType == TechType.HeatFruitPlant)
                {
                    __instance.allowFruitSpawnByDefault = true;
                }
            }
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(FruitPlant __instance)
            {
                if (__instance.initialized)
                    return;

                TechType techType = CraftData.GetTechType(__instance.gameObject);
                if (techType == TechType.HeatFruitPlant || techType == TechType.LeafyFruitPlant)
                {
                    //AddDebug($"FruitPlant Start {techType} allowFruitSpawnByDefault {__instance.allowFruitSpawnByDefault} fruitSpawnEnabled {__instance.fruitSpawnEnabled}");
                    __instance.fruitSpawnEnabled = true;
                    __instance.Initialize();
                }
            }
            [HarmonyPostfix, HarmonyPatch("OnFruitHarvest")]
            public static void OnFruitHarvestPostfix(FruitPlant __instance, PickPrefab fruit)
            { // DayNightCycle.dayLengthSeconds is not constant
                //AddDebug("OnFruitHarvest " + fruit.name);
                if (ConfigToEdit.fruitGrowTime.Value == 0)
                    __instance.fruitSpawnInterval = 300;
                else
                    __instance.fruitSpawnInterval = ConfigToEdit.fruitGrowTime.Value * DayNightCycle.main.dayLengthSeconds;

                if (fruit.pickTech == TechType.IceFruit || fruit.pickTech == TechType.HeatFruit)
                {
                    SavePickedFruits(__instance);
                    //AddDebug("timeNextFruit " + __instance.timeNextFruit);
                }
            }

            private static void SavePickedFruits(FruitPlant fruitPlant)
            {
                int timeNextFruit = (int)fruitPlant.timeNextFruit;
                float inactiveFruits = fruitPlant.inactiveFruits.Count * .01f;
                fruitPlant.timeNextFruit = timeNextFruit + inactiveFruits;
                //AddDebug($"SavePickedFruits {fruitPlant.name} timeNextFruit {fruitPlant.timeNextFruit}");
            }

            [HarmonyPrefix, HarmonyPatch("Initialize")]
            public static bool InitializePrefix(FruitPlant __instance)
            {
                if (__instance.initialized)
                    return false;

                if (__instance.fruits.Length == 0)
                    return true;

                if (ConfigToEdit.fruitGrowTime.Value > 0)
                    __instance.fruitSpawnInterval = ConfigToEdit.fruitGrowTime.Value * DayNightCycle.main.dayLengthSeconds;

                //Main.logger.LogDebug($"{__instance.name} FruitPlant Initialize fruits " + __instance.fruits.Length);
                if (__instance.fruits[0].pickTech != TechType.HeatFruit && __instance.fruits[0].pickTech != TechType.IceFruit)
                    return true;

                __instance.inactiveFruits.Clear();
                float pickedFruits = __instance.timeNextFruit - (int)__instance.timeNextFruit;
                pickedFruits = (float)Math.Round(pickedFruits, 2);
                pickedFruits *= 100;

                if (pickedFruits > __instance.fruits.Length)
                {
                    pickedFruits = 0;
                    __instance.timeNextFruit = (int)__instance.timeNextFruit;
                }
                //Main.logger.LogDebug($"{__instance.name} FruitPlant Initialize pickedFruits {pickedFruits} timeNextFruit {__instance.timeNextFruit}");

                for (int index = 0; index < __instance.fruits.Length; ++index)
                {
                    PickPrefab pickPrefab = __instance.fruits[index];
                    pickPrefab.pickedEvent.AddHandler(__instance, new UWE.Event<PickPrefab>.HandleFunction(__instance.OnFruitHarvest));
                    pickPrefab.SetPickedState(false);
                }
                for (int index = 0; index < (int)pickedFruits; ++index)
                {
                    PickPrefab pickPrefab = __instance.fruits[index];
                    __instance.inactiveFruits.Add(pickPrefab);
                    pickPrefab.SetPickedState(true);
                    //AddDebug("FruitPlant Initialize SetPickedUp ");
                }
                __instance.initialized = true;
                return false;
            }
            // heat_fruit_plant(
            [HarmonyPostfix, HarmonyPatch("Initialize")]
            public static void InitializePostfix(FruitPlant __instance)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                if (techType == TechType.Creepvine)
                {
                    Light light = __instance.GetComponentInChildren<Light>();
                    //Light[] lights = __instance.GetComponentsInChildren<Light>();
                    //if (lights.Length > 1)
                    //    AddDebug(__instance.name + " LIGHTS " + lights.Length);
                    if (light == null || __instance.inactiveFruits == null || __instance.fruits == null)
                        return;

                    light.intensity = creepVineSeedLightInt - (float)__instance.inactiveFruits.Count / (float)__instance.fruits.Length * creepVineSeedLightInt;
                    //AddDebug(__instance.name + " Initialize intensity " + light.intensity);
                }
                else if (techType == TechType.HeatFruitPlant)
                {
                    //AddDebug("FruitPlant Initialize HeatFruitPlant fruitSpawnEnabled " + __instance.fruitSpawnEnabled);
                    for (int i = 0; i < __instance.fruits.Length; i++)
                    {
                        PickPrefab fruit = __instance.fruits[i];
                        //Main.logger.LogDebug("HeatFruitPlant fruit PickedState  " + fruit.GetPickedState());
                    }
                    Transform fruits = __instance.transform.Find("Fruits");
                    for (int i = 0; i < fruits.childCount; i++)
                    {
                        Transform fruit = fruits.GetChild(i);
                        //Main.logger.LogDebug("HeatFruitPlant fruits child " + fruit.name);
                        if (fruit.name == "SerializerEmptyGameObject")
                            UnityEngine.Object.Destroy(fruit.gameObject);
                    }
                }
            }

            [HarmonyPrefix, HarmonyPatch("Update")]
            public static bool UpdatePrefix(FruitPlant __instance)
            {
                if (Main.gameLoaded == false || __instance.fruitSpawnEnabled == false)
                    return false;

                //if (Time.frameCount % 60 != 0)
                //    return false;

                if (__instance.inactiveFruits.Count != 0 && DayNightCycle.main.timePassed >= __instance.timeNextFruit)
                {
                    PickPrefab random = __instance.inactiveFruits.GetRandom();
                    random.SetPickedState(false);
                    __instance.inactiveFruits.Remove(random);
                    __instance.timeNextFruit += __instance.fruitSpawnInterval;

                    TechType techType = CraftData.GetTechType(__instance.gameObject);
                    if (techType == TechType.IceFruitPlant || techType == TechType.HeatFruitPlant)
                    {
                        SavePickedFruits(__instance);
                    }
                }
                return false;
            }

        }
    }
}
