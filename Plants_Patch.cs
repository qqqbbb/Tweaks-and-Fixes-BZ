using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Plants_Patch
    {// fruit test -583 -30 -212   -520 -85 -80     -573 -34 -110
        static float creepVineSeedLightInt = 1.2f;

        public static void AttachFruitPlant(GameObject go)
        { // FruitPlant will be saved
            PickPrefab[] pickPrefabs = go.GetComponentsInChildren<PickPrefab>(true);
            if (pickPrefabs == null || pickPrefabs.Length == 0)
                return;

            FruitPlant fp = go.EnsureComponent<FruitPlant>();
            fp.fruitSpawnEnabled = true;
            //AddDebug(__instance.name + " fruitSpawnInterval orig " + fp.fruitSpawnInterval);
            fp.fruits = pickPrefabs;
            foreach (PickPrefab pp in pickPrefabs)
            {
                //string pos = (int)__instance.transform.position.x + "_" + (int)__instance.transform.position.y + "_" + (int)__instance.transform.position.z;
                if (!pp.gameObject.activeSelf && !fp.inactiveFruits.Contains(pp))
                    fp.inactiveFruits.Add(pp);
            }
        }

        public static void AttachFruitPlantToKelpRoot(GameObject go)
        { // FruitPlant will be saved
            PickPrefab[] pickPrefabs_ = go.GetComponentsInChildren<PickPrefab>();
            if (pickPrefabs_.Length == 0)
                return;

            TechTag techTag_ = go.GetComponent<TechTag>();
            if (techTag_ && techTag_.type == TechType.KelpRoot)
                return;

            //AddDebug("AttachFruitPlant " + go.name);
            List<PickPrefab> pickPrefabs = new List<PickPrefab>();
            foreach (Transform child in go.transform)
            {
                Pickupable p = child.gameObject.GetComponent<Pickupable>();
                if (p)
                {
                    //AddDebug("Attach PickPrefab " + p.transform.parent.name);
                    //Main.Log("Attach PickPrefab " + p.transform.parent.name);
                    foreach (Transform grandChild in child.gameObject.transform)
                    {
                        PickPrefab oldPP = grandChild.gameObject.GetComponent<PickPrefab>();
                        if (oldPP)
                        {
                            //AddDebug("Destroy PickPrefab " + oldPP.transform.parent.name);
                            //Main.Log("Destroy PickPrefab " + oldPP.transform.parent.name);
                            UnityEngine.Object.Destroy(oldPP);
                        }
                    }
                    PickPrefab pp = child.gameObject.EnsureComponent<PickPrefab>();
                    pp.pickTech = TechType.KelpRootPustule;
                    pickPrefabs.Add(pp);
                    UnityEngine.Object.Destroy(p);
                }
            }
            FruitPlant fp = go.EnsureComponent<FruitPlant>();
            fp.fruitSpawnEnabled = true;
            //AddDebug(__instance.name + " fruitSpawnInterval orig " + fp.fruitSpawnInterval);
            fp.fruits = pickPrefabs.ToArray();
            TechTag techTag = go.EnsureComponent<TechTag>();
            techTag.type = TechType.KelpRoot;
            //foreach (PickPrefab pp in pickPrefabs)
            //{
            //if (!pp.enabled && !fp.inactiveFruits.Contains(pp))
            //    fp.inactiveFruits.Add(pp);
            //}
            if (Main.config.fruitGrowTime > 0)
                fp.fruitSpawnInterval = Main.config.fruitGrowTime * Main.dayLengthSeconds;
        }

        //[HarmonyPatch(typeof(ResourceTracker))]
        class ResourceTracker_Patch
        {
            [HarmonyPatch("Start")]
            [HarmonyPostfix]
            public static void StartPrefix(ResourceTracker __instance)
            {
                if (__instance.techType == TechType.GenericJeweledDisk)
                {
                    __instance.gameObject.transform.localRotation = Quaternion.Euler(__instance.gameObject.transform.localRotation.x, __instance.gameObject.transform.localRotation.y, 0f);
                }
            }
        }

        [HarmonyPatch(typeof(PickPrefab))]
        class PickPrefab_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            public static void StartPrefix(PickPrefab __instance)
            {
                if (__instance.pickTech == TechType.IceFruit)
                { // OnProtoDeserialize does not run 
                    string pos = (int)__instance.transform.position.x + "_" + (int)__instance.transform.position.y + "_" + (int)__instance.transform.position.z;
                    if (Main.config.iceFruitPickedState.ContainsKey(pos))
                    {
                        //AddDebug("IceFruit PickPrefab Start ");
                        if (Main.config.iceFruitPickedState[pos])
                            __instance.SetPickedState(true);
                    }
                }
            }
            //[HarmonyPatch("SetPickedUp")]
            //[HarmonyPostfix]
            public static void SetPickedUpPostfix(PickPrefab __instance)
            {
                //AddDebug("PickPrefab SetPickedUp " + tt);
                //ResourceTracker rt = __instance.GetComponent<ResourceTracker>();
                //if (rt && rt.techType == TechType.KelpRootPustule)
                //{
                //FruitPlant fp_ = __instance.transform.parent.gameObject.GetComponent<FruitPlant>();
                //if (fp_)
                //{
                //    fp_.OnFruitHarvest(__instance);
                //    rt.Unregister();
                //    return;
                //}
                //}
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt != TechType.CreepvineSeedCluster)
                    return;

                PrefabIdentifier pi = __instance.GetComponentInParent<PrefabIdentifier>();
                if (!pi)
                    return;
                FruitPlant fp = pi.GetComponent<FruitPlant>();
                if (!fp)
                    return;
                Light light = pi.GetComponentInChildren<Light>();
                if (light == null || fp.inactiveFruits == null || fp.fruits == null)
                    return;
                light.intensity = creepVineSeedLightInt - (float)fp.inactiveFruits.Count / (float)fp.fruits.Length * creepVineSeedLightInt;
                //AddDebug(" intensity " + light.intensity);
            }

            [HarmonyPostfix]
            [HarmonyPatch("SetPickedState")]
            public static void SetPickedStatePostfix(PickPrefab __instance, bool newPickedState)
            {
                //AddDebug(__instance.pickTech + " SetPickedState " + newPickedState);
                //if (newPickedState)
                //    return;

                //ResourceTracker rt = __instance.GetComponent<ResourceTracker>();
                //if (rt && rt.techType == TechType.KelpRootPustule)
                //    rt.Register();
                if (__instance.pickTech == TechType.IceFruit)
                { // not checking save slot
                    string pos = (int)__instance.transform.position.x + "_" + (int)__instance.transform.position.y + "_" + (int)__instance.transform.position.z;
                    //AddDebug("pos " + pos);
                    Main.config.iceFruitPickedState[pos] = newPickedState;
                }
                else if (__instance.pickTech == TechType.CreepvineSeedCluster)
                {
                    FruitPlant fp = __instance.GetComponentInParent<FruitPlant>();
                    if (!fp)
                        return;
                    Light light = fp.GetComponentInChildren<Light>();
                    if (light)
                    {
                        float inactiveFruits = fp.inactiveFruits.Count;
                        if (!newPickedState)
                            inactiveFruits -= 1;
                        //AddDebug("inactiveFruits " + inactiveFruits);
                        light.intensity = creepVineSeedLightInt - inactiveFruits / (float)fp.fruits.Length * creepVineSeedLightInt;
                        //AddDebug("SetPickedState CreepvineSeed " + newPickedState + " " + light.intensity);
                    }
                }
            }
            //[HarmonyPatch("OnHandHover")]
            //[HarmonyPrefix]
            public static void OnHandHoverPrefix(PickPrefab __instance)
            {
                AddDebug("OnHandHover " + __instance.pickTech);
            }
        }

        [HarmonyPatch(typeof(FruitPlant))]
        class FruitPlant_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void AwakePostfix(FruitPlant __instance)
            {
                if (Main.config.fruitGrowTime > 0)
                    __instance.fruitSpawnInterval = Main.config.fruitGrowTime * Main.dayLengthSeconds;
            }
            [HarmonyPrefix]
            [HarmonyPatch("Initialize")]
            public static bool InitializePrefix(FruitPlant __instance)
            {
                if (__instance.initialized)
                    return false;

                __instance.inactiveFruits.Clear();
                if (__instance.fruits == null)
                {
                    //AddDebug(__instance.name + " fruits null");
                    return false;
                }
                for (int index = 0; index < __instance.fruits.Length; ++index)
                {
                    __instance.fruits[index].pickedEvent.AddHandler(__instance, new UWE.Event<PickPrefab>.HandleFunction(__instance.OnFruitHarvest));
                    if (__instance.fruits[index].GetPickedState())
                        __instance.inactiveFruits.Add(__instance.fruits[index]);
                }
                __instance.initialized = true;
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            public static void InitializePostfix(FruitPlant __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == TechType.Creepvine)
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
            }
        }

        [HarmonyPatch(typeof(GrowingPlant))]
        class GrowingPlant_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("GetGrowthDuration")]
            public static bool GetGrowthDurationPrefix(GrowingPlant __instance, ref float __result)
            {
                //__result = __instance.growthDuration * Main.config.plantGrowthTimeMult * (NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f);
                return false;
            }
         
            [HarmonyPrefix]
            [HarmonyPatch("SetScale")]
            static bool SetScalePrefix(GrowingPlant __instance, Transform tr, float progress)
            {
                if (!ConfigToEdit.fixMelon.Value)
                    return true;

                TechType tt = __instance.plantTechType;

                if (tt == TechType.SnowStalkerPlant || tt == TechType.MelonPlant)
                {
                    float mult = 1.7f;
                    if (tt == TechType.MelonPlant)
                        mult = 1.2f;

                    float num = __instance.isIndoor ? __instance.growthWidthIndoor.Evaluate(progress) : __instance.growthWidth.Evaluate(progress);
                    float y = __instance.isIndoor ? __instance.growthHeightIndoor.Evaluate(progress) : __instance.growthHeight.Evaluate(progress);
                    num *= mult;
                    tr.localScale = new Vector3(num, y * mult, num);
                    if (__instance.passYbounds != null)
                        __instance.passYbounds.UpdateWavingScale(tr.localScale);
                    else
                    {
                        if (__instance.wavingScaler != null)
                            __instance.wavingScaler.UpdateWavingScale(tr.localScale);
                    }
                    //AddDebug("SnowStalkerPlant maxProgress " + __instance.maxProgress);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Inventory), "OnAddItem")]
        class Inventory_OnAddItem_Patch
        {
            public static void Postfix(Inventory __instance, InventoryItem item)
            {
                if (!ConfigToEdit.fixMelon.Value)
                    return;

                if (item._techType == TechType.MelonSeed || item._techType == TechType.SnowStalkerFruit)
                {
                    if (item.item)
                    { 
                        Plantable p = item.item.GetComponent<Plantable>();
                        if (p)
                            p.size = Plantable.PlantSize.Large;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Plantable))]
        class Plantable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnProtoDeserialize")]
            static void OnProtoDeserializePostfix(Plantable __instance)
            {
                TechType tt = __instance.plantTechType;
                if (!ConfigToEdit.fixMelon.Value)
                    return;

                if (tt == TechType.SnowStalkerPlant || tt == TechType.MelonPlant)
                {
                    //AddDebug("Plantable OnProtoDeserialize " + __instance.plantTechType);
                    //AddDebug("Planter AddItem fix " + p.plantTechType);
                    __instance.size = Plantable.PlantSize.Large;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Spawn")]
            public static void SpawnPostfix(ref GameObject __result, Plantable __instance)
            {
                if (ConfigToEdit.randomPlantRotation.Value && __instance.plantTechType != TechType.HeatFruitPlant)
                {
                    //AddDebug("Plantable Spawn " + __result.name);
                    Vector3 rot = __result.transform.eulerAngles;
                    float y = UnityEngine.Random.Range(0, 360);
                    __result.transform.eulerAngles = new Vector3(rot.x, y, rot.z);
                }
            }
        }

        [HarmonyPatch(typeof(TechData), "GetItemSize")]
        class TechData_GetItemSize_Patch
        {
            static void Postfix(TechType techType, ref Vector2int __result)
            {
                if (!ConfigToEdit.fixMelon.Value)
                    return;

                if (techType == TechType.MelonPlant || techType == TechType.SnowStalkerPlant)
                {
                    __result = new Vector2int(2, 2);
                }
            }
        }

        //[HarmonyPatch(typeof(Eatable), "Awake")]
        class Eatable_Awake_Patch
        {
            static void Postfix(Eatable __instance)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable)
                {
                    if (plantable.plantTechType == TechType.SnowStalkerPlant)
                    {
                        AddDebug("Eatable Awake SnowStalkerPlant " + __instance.name);
                        plantable.size = Plantable.PlantSize.Large;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(FruitPlant), "OnFruitHarvest")]
        class FruitPlant_OnFruitHarvest_Patch
        {
            public static void Postfix(FruitPlant __instance, PickPrefab fruit)
            {
                AddDebug("FruitPlant OnFruitHarvest " + fruit.pickTech);
            }
        }

        //[HarmonyPatch(typeof(Pickupable), "OnHandClick")]
        class Pickupable_OnHandClick_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (__instance.GetTechType() == TechType.KelpRootPustule)
                {
                    AddDebug("KelpRootPustule Pickupable OnHandClick ");
                }
            }
        }
      
        //[HarmonyPatch(typeof(PickPrefab), "OnHandClick")]
        class PickPrefab_OnHandClick_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (__instance.GetTechType() == TechType.KelpRootPustule)
                {
                    AddDebug("KelpRootPustule PickPrefab OnHandClick ");
                }
            }
        }


    }
}
