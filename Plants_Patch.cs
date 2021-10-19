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
        public static List<LargeWorldEntity> tableCorals = new List<LargeWorldEntity>();

        public static void AttachFruitPlant(GameObject go)
        { // FruitPlant will be saved
            PickPrefab[] pickPrefabs = go.GetComponentsInChildren<PickPrefab>(true);
            if (pickPrefabs.Length == 0)
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
            //Main.Log("AttachFruitPlant " + go.name);
            //Main.Log("AttachFruitPlantToKelpRoot ");
            List<PickPrefab> pickPrefabs = new List<PickPrefab>();
            foreach (Transform child in go.transform)
            {
                //Main.Log(go.name + " " + child.name);
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
            fp.fruitSpawnInterval = Main.config.fruitGrowTime * 1200f;
            if (fp.fruitSpawnInterval == 0f)
                fp.fruitSpawnInterval = 1f;
        }

        [HarmonyPatch(typeof(LargeWorldEntity), "Start")]
        class LargeWorldEntity_Start_Patch
        {
            public static void Prefix(LargeWorldEntity __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //if (tt == TechType.None && __instance.name.StartsWith("kelpcave_root"))
                {
                    //AttachFruitPlantToKelpRoot(__instance.gameObject);
                    //PrefabPlaceholder[] pphs = __instance.GetComponentsInChildren<PrefabPlaceholder>();
                    //Pickupable[] ps = __instance.GetComponentsInChildren<Pickupable>();
                    //AddDebug(__instance.name + " LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
                    //Main.Log(__instance.name +" LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
                }
                if (tt == TechType.GenericJeweledDisk)
                {
                    Animator a = __instance.GetComponentInChildren<Animator>();
                    if (a)
                        a.enabled = false;
                    Vector3 rot = __instance.gameObject.transform.eulerAngles;
                    //Main.Log("fix GenericJeweledDisk " + __instance.gameObject.transform.position.x + " " + __instance.gameObject.transform.position.y + " " + __instance.gameObject.transform.position.z + " rot " + rot.x + " " + rot.y + " " + rot.z);
                    __instance.gameObject.transform.eulerAngles = new Vector3(rot.x, rot.y, 0f);
                }
                else if (tt == TechType.SmallMelon)
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if ((x == 989 && y == 30 && z == -897) || (x == 989 && y == 29 && z == -896) || (x == 986 && y == 29 && z == -895))
                    { // make melons in Marg greenhouse pickupable
                        __instance.GetComponent<SphereCollider>().radius = .4f;
                        //AddDebug("make melons in Marg greenhouse pickupable " + x +" " + y +" " + z);
                    }
                }
                else if (tt == TechType.IceFruitPlant || tt == TechType.Creepvine || tt == TechType.HangingFruitTree || tt == TechType.SnowStalkerPlant || tt == TechType.LeafyFruitPlant || tt == TechType.HeatFruitPlant)
                {
                    //PickPrefab[] pickPrefabs = __instance.GetAllComponentsInChildren<PickPrefab>();
                    AttachFruitPlant(__instance.gameObject);
                }
                //else if (tt == TechType.KelpRootPustule) // KelpRoot tt is none
                //{ //  at awake parent may be null
                //GameObject parent = __instance.transform.parent.gameObject;
                //if (parent && parent.GetComponent<LargeWorldEntity>())
                //{
                //PrefabPlaceholder[] pphs = parent.GetComponentsInChildren<PrefabPlaceholder>();
                //Pickupable[] ps = parent.GetComponentsInChildren<Pickupable>();
                //if (pphs.Length > 0 && pphs.Length == ps.Length)
                //    AttachFruitPlantToKelpRoot(parent);
                //AddDebug("KelpRootPustule LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
                //Main.Log("KelpRootPustule LargeWorldEntity Start PrefabPlaceholder " + pphs.Length + " Pickupable " + ps.Length);
                //}
                //}
                else if (tt == TechType.HeatFruitPlant)
                { // sometimes floating HeatFruitPlant spawns near Marg greenhouse
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == 987 && y == 29 && z == -877)
                        __instance.transform.position = new Vector3(__instance.transform.position.x, 28f, __instance.transform.position.z);
                }
            }
            public static void Postfix(LargeWorldEntity __instance)
            {
                if (Main.config.alwaysBestLOD)
                {
                    LODGroup[] lodGroups = __instance.GetComponentsInChildren<LODGroup>();
                    foreach (LODGroup lodGroup in lodGroups)
                    {
                        if (lodGroup.lodCount == 1)
                            continue;
                        LOD[] lods = lodGroup.GetLODs();
                        Renderer LOD0Renderer = null;
                        Renderer LOD1Renderer = null;
                        List<Renderer> loPolyRenderers = new List<Renderer>();

                        foreach (LOD lod in lods)
                        {
                            foreach (Renderer r in lod.renderers)
                            {
                                if (r.name.EndsWith("_LOD0"))
                                    LOD0Renderer = r;
                                else if (r.name.EndsWith("_LOD1"))
                                { // creepVine dont have _LOD0 renderer
                                    LOD1Renderer = r;
                                    loPolyRenderers.Add(r);
                                }
                                else
                                    loPolyRenderers.Add(r);
                            }
                        }
                        if (LOD0Renderer)
                        {
                            lodGroup.enabled = false;
                            foreach (Renderer r in loPolyRenderers)
                                r.enabled = false;
                        }
                        else if (LOD1Renderer)
                        {
                            lodGroup.enabled = false;
                            foreach (Renderer r in loPolyRenderers)
                            {
                                if (!r.name.EndsWith("_LOD1"))
                                    r.enabled = false;
                            }
                        }
                    }
                }
            }

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
            [HarmonyPatch("Start")]
            [HarmonyPrefix]
            public static void StartPrefix(PickPrefab __instance)
            {
                if (__instance.pickTech == TechType.IceFruit)
                { // OnProtoDeserialize does not run 
                    string pos = (int)__instance.transform.position.x + "_" + (int)__instance.transform.position.y + "_" + (int)__instance.transform.position.z;
                    if (Main.config.iceFruitPickedState.ContainsKey(pos))
                    {
                        //AddDebug("IceFruit PickPrefab Start ");
                        bool active = Main.config.iceFruitPickedState[pos];
                        if(active)
                            __instance.SetPickedState(active);
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
            [HarmonyPatch("SetPickedState")]
            [HarmonyPostfix]
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
                __instance.fruitSpawnInterval = Main.config.fruitGrowTime * 1200f;
                if (__instance.fruitSpawnInterval == 0f)
                    __instance.fruitSpawnInterval = 1f;
            }
            [HarmonyPrefix]
            [HarmonyPatch(nameof(FruitPlant.Initialize))]
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
            [HarmonyPatch(nameof(FruitPlant.Initialize))]
            public static void Postfix(FruitPlant __instance)
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
            [HarmonyPrefix]
            [HarmonyPatch("GetGrowthDuration")]
            public static bool GetGrowthDurationPrefix(GrowingPlant __instance, ref float __result)
            {
                __result = __instance.growthDuration * Main.config.plantGrowthTimeMult * (NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f);
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("SetScale")]
            static bool SetScalePrefix(GrowingPlant __instance, Transform tr, float progress)
            {
                if (__instance.plantTechType == TechType.SnowStalkerPlant)
                {
                    float num = __instance.isIndoor ? __instance.growthWidthIndoor.Evaluate(progress) : __instance.growthWidth.Evaluate(progress);
                    float y = __instance.isIndoor ? __instance.growthHeightIndoor.Evaluate(progress) : __instance.growthHeight.Evaluate(progress);
                    num *= 2f;
                    tr.localScale = new Vector3(num, y * 2f, num);
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

        [HarmonyPatch(typeof(Eatable), "Awake")]
        class Eatable_Awake_Patch
        {
            static void Postfix(Eatable __instance)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable && plantable.plantTechType == TechType.SnowStalkerPlant)
                {
                    //AddDebug("Eatable Awake " + __instance.name);
                    plantable.size = Plantable.PlantSize.Large;
                }
            }
        }

        //[HarmonyPatch(typeof(Planter), "Start")]
        class Planter_Start_Patch
        {
            static void Prefix(Planter __instance)
            {
                if (__instance.bigSlots.Length == 1 && __instance.slots.Length == 4)
                {
                    __instance.initialized = false;
                    __instance.slots = new Transform[] { __instance.bigSlots[0] };
                    AddDebug(__instance.name + " bigSlots " + __instance.bigSlots.Length + " slots " + __instance.slots.Length);
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

        //[HarmonyPatch(typeof(Pickupable), nameof(Pickupable.OnHandClick))]
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
        //[HarmonyPatch(typeof(PickPrefab), nameof(PickPrefab.OnHandClick))]
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
