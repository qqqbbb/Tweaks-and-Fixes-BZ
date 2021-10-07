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
    {// fruit test -560 -30 -250   -520 -85 -80
        static float creepVineSeedLightInt = 1.2f;

        public static void AttachFruitPlant(GameObject go)
        {
            if (go == null)
            {
                AddDebug("AttachFruitPlant go is null");
                return;
            }
            PickPrefab[] pickPrefabs = go.GetComponentsInChildren<PickPrefab>(true);
            if (pickPrefabs.Length == 0)
                return;

            FruitPlant fp = go.EnsureComponent<FruitPlant>();
            fp.fruitSpawnEnabled = true;
            //AddDebug(__instance.name + " fruitSpawnInterval orig " + fp.fruitSpawnInterval);
            fp.fruits = pickPrefabs;
            fp.fruitSpawnInterval = Main.config.fruitGrowTime * 1200f;
            if (fp.fruitSpawnInterval == 0f)
                fp.fruitSpawnInterval = 1f;
        }

        [HarmonyPatch(typeof(LargeWorldEntity), "Start")]
        class LargeWorldEntity_Start_Patch
        {
            public static void Postfix(LargeWorldEntity __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt == TechType.GenericJeweledDisk)
                {
                    Animator a = __instance.GetComponentInChildren<Animator>();
                    if (a)
                        a.enabled = false;
                    __instance.gameObject.transform.rotation = Quaternion.Euler(__instance.gameObject.transform.rotation.x, __instance.gameObject.transform.rotation.y, 0);
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
                else if (tt == TechType.IceFruitPlant || tt == TechType.Creepvine || tt == TechType.HangingFruitTree || tt == TechType.SnowStalkerPlant)
                {
                    //PickPrefab[] pickPrefabs = __instance.GetAllComponentsInChildren<PickPrefab>();
                    AttachFruitPlant(__instance.gameObject);
                }
                else if (tt == TechType.KelpRootPustule)
                { //  at awake parent may be null
                    GameObject parent = __instance.transform.parent.gameObject;
                    if (!parent.GetComponent<LargeWorldEntity>()) // not on root
                        return;

                    Pickupable p = __instance.GetComponent<Pickupable>();
                    //UnityEngine.Object.Destroy(p);
                    p.enabled = false;
                    //ResourceTracker rt = __instance.GetComponent<ResourceTracker>();
                    //rt.pickupable = null;
                    PickPrefab pp = __instance.GetComponentInChildren<PickPrefab>(true);
                    UnityEngine.Object.Destroy(pp);
                    pp = __instance.gameObject.AddComponent<PickPrefab>();
                    pp.pickTech = TechType.KelpRootPustule;
                    FruitPlant fp = parent.GetComponent<FruitPlant>();
                    if (fp)
                        return;

                    TechTag techTag = parent.AddComponent<TechTag>();
                    techTag.type = TechType.KelpRoot;
                    AttachFruitPlant(parent);
                }
                if (Main.config.alwaysBestLOD)
                {
                    LODGroup lod = __instance.GetComponent<LODGroup>();
                    if (lod)
                    {
                        lod.enabled = false;
                        MeshRenderer[] renderers = __instance.GetComponentsInChildren<MeshRenderer>();
                        //if (tt != TechType.None)
                        //    AddDebug("disable LOD " + tt + " " + __instance.name);
                        //else
                        // AddDebug("disable LOD " + __instance.name);
                        for (int i = 1; i < renderers.Length; i++)
                            renderers[i].enabled = false;
                    }
                }

            }
        }

        [HarmonyPatch(typeof(PickPrefab))]
        class PickPrefab_Patch
        {
            [HarmonyPatch("SetPickedUp")]
            [HarmonyPostfix]
            public static void SetPickedUpPostfix(PickPrefab __instance)
            {
                //AddDebug("PickPrefab SetPickedUp " + tt);
                ResourceTracker rt = __instance.GetComponent<ResourceTracker>();
                if (rt && rt.techType == TechType.KelpRootPustule)
                {
                    FruitPlant fp_ = __instance.transform.parent.gameObject.GetComponent<FruitPlant>();
                    if (fp_)
                    {
                        fp_.OnFruitHarvest(__instance);
                        rt.Unregister();
                        return;
                    }
                }
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
                if (!light)
                    return;
                light.intensity = creepVineSeedLightInt - (float)fp.inactiveFruits.Count / (float)fp.fruits.Length * creepVineSeedLightInt;
                //AddDebug(" intensity " + light.intensity);
            }
            [HarmonyPatch("SetPickedState")]
            [HarmonyPostfix]
            public static void SetPickedStatePostfix(PickPrefab __instance, bool newPickedState)
            {
                if (newPickedState)
                    return;

                ResourceTracker rt = __instance.GetComponent<ResourceTracker>();
                if (rt && rt.techType == TechType.KelpRootPustule)
                    rt.Register();
                else if (__instance.pickTech == TechType.CreepvineSeedCluster)
                {
                    FruitPlant fp = __instance.GetComponentInParent<FruitPlant>();
                    if (!fp)
                        return;
                    //AddDebug("SetPickedState CreepvineSeedCluster");
                    Light light = fp.GetComponentInChildren<Light>();
                    if (light)
                    {
                        float inactiveFruits = fp.inactiveFruits.Count - 1;
                        light.intensity = creepVineSeedLightInt - inactiveFruits / (float)fp.fruits.Length * creepVineSeedLightInt;
                        //AddDebug("intensity " + light.intensity);
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

        [HarmonyPatch(typeof(FruitPlant), "Initialize")]
        class FruitPlant_Initialize_Patch
        {
            public static void Postfix(FruitPlant __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == TechType.Creepvine)
                {
                    Light light = __instance.GetComponentInChildren<Light>();
                    //Light[] lights = __instance.GetComponentsInChildren<Light>();
                    //if (lights.Length > 1)
                    //    AddDebug(__instance.name + " LIGHTS " + lights.Length);
                    if (!light)
                        return;

                    light.intensity = creepVineSeedLightInt - (float)__instance.inactiveFruits.Count / (float)__instance.fruits.Length * creepVineSeedLightInt;
                    //AddDebug(__instance.name + " Initialize intensity " + light.intensity);
                }
            }
        }
           
        [HarmonyPatch(typeof(GrowingPlant), "GetGrowthDuration")]
        class GrowingPlant_GetGrowthDuration_Patch
        {
            public static bool Prefix(GrowingPlant __instance, ref float __result)
            {
                __result = __instance.growthDuration * Main.config.plantGrowthTimeMult * (NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f);
                return false;
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


    }
}
