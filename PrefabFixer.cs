using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;

namespace Tweaks_Fixes
{
    internal class PrefabFixer
    {
        static readonly int zOffset = Shader.PropertyToID("_ZOffset");

        static readonly Dictionary<TechType, MaterialZoffsetData> glassMaterialZoffsets = new Dictionary<TechType, MaterialZoffsetData> {
            { TechType.Hoverpad, new MaterialZoffsetData("AnimatedMesh/Hoverpad_geo", 1, 10000) },
            //{ TechType.HoverpadFragment, new MaterialZoffsetData(null, 1, 10000) },
        { TechType.SeaTruckAquariumModule, new MaterialZoffsetData("seatruck_module_aquarium_anim/Seatruck_module_Aquarium_interior_geo", 2, 10000) },
        { TechType.SmallVentGarden, new MaterialZoffsetData("Vent_garden_swimming_anim/vent_garden_geo/newest_standing_geo/vent_garden_bulb_swimming", 3, 10000) }
        };

        static readonly Dictionary<string, MaterialZoffsetData> glassMaterialZoffsets_ = new Dictionary<string, MaterialZoffsetData> {
            { "9fccfbb8-7611-40b5-99bd-513e95993bd3", new MaterialZoffsetData(null, 0, 1000) },// ice_pool_01_a
            { "61af0fdd-f077-42b3-b8f8-c5ccef8ce3c0", new MaterialZoffsetData(null, 0, 1000) },// ice_pool_01_b
            { "0e04bfbd-0e32-4451-bed3-7955a20aea44", new MaterialZoffsetData(null, 0, 1000) },// ice_pool_01_c
            { "2168257e-2533-403f-8b3a-a3bef63adaf9",new MaterialZoffsetData(null, 1, 10000) }, // Hoverpad_Fragmment

        };
        public static void FixGlassPrefabs()
        {
            foreach (var kv in glassMaterialZoffsets)
            {
                UWE.CoroutineHost.StartCoroutine(ChangeMaterialZoffsetAsync(kv.Key, kv.Value));
            }
            foreach (var kv in glassMaterialZoffsets_)
            {
                UWE.CoroutineHost.StartCoroutine(ChangeMaterialZoffsetAsync(kv.Key, kv.Value));
            }
        }

        static IEnumerator ChangeMaterialZoffsetAsync(TechType techType, MaterialZoffsetData data)
        {
            //Main.logger.LogDebug("ChangeMaterialZoffsetAsync " + techType);

            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogDebug($"ChangeMaterialZoffsetAsync {techType} prefab null");
                yield break;
            }
            //else
            //    Main.logger.LogDebug($"ChangeMaterialZoffsetAsync {techType} {prefab.name}");

            Renderer renderer;
            if (data.rendererPath == null)
                renderer = prefab.GetComponentInChildren<Renderer>();
            else
            {
                Transform rendererT = prefab.transform.Find(data.rendererPath);
                renderer = rendererT.GetComponent<Renderer>();
            }
            //Main.logger.LogDebug($"ChangeMaterialZoffset {techType} {renderer.name} materials");
            if (renderer == null)
            {
                Main.logger.LogDebug($"ChangeMaterialZoffsetAsync {techType} {prefab.name} renderer null");
            }
            if (data.materialIndex >= renderer.materials.Length)
            {
                //Main.logger.LogDebug("ChangeMaterialZoffsetAsync wrong materialIndex");
                yield break;
            }
            Material material = renderer.materials[data.materialIndex];
            //Main.logger.LogDebug("Set offset " + material.name);
            material.SetFloat(zOffset, data.offsetValue);
        }

        static IEnumerator ChangeMaterialZoffsetAsync(string classID, MaterialZoffsetData data)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogDebug("ChangeMaterialZoffsetAsync No prefab for " + classID);
                yield break;
            }
            Renderer renderer;
            if (data.rendererPath == null)
            {
                renderer = prefab.transform.GetComponentInChildren<Renderer>();
            }
            else
            {
                Transform rendererT = prefab.transform.Find(data.rendererPath);
                renderer = rendererT.GetComponent<Renderer>();
            }
            //Main.logger.LogDebug($"ChangeMaterialZoffset {techType} {renderer.name} materials");
            if (data.materialIndex >= renderer.materials.Length)
            {
                //Main.logger.LogDebug("ChangeMaterialZoffsetAsync wrong materialIndex");
                yield break;
            }
            Material material = renderer.materials[data.materialIndex];
            //Material material = renderer.sharedMaterials[data.materialIndex];
            //Main.logger.LogDebug("Set offset " + material.name);
            material.SetFloat(zOffset, data.offsetValue);
        }


    }

    class MaterialZoffsetData
    {
        public string rendererPath;
        public int materialIndex;
        public int offsetValue;

        public MaterialZoffsetData(string rendererPath, int materialIndex, int offsetValue)
        {
            this.rendererPath = rendererPath;
            this.materialIndex = materialIndex;
            this.offsetValue = offsetValue;
        }
    }

}
