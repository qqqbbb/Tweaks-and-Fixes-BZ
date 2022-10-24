using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Starting_Loot_Patch 
    {
        public static IEnumerator SpawnStartLoot (ItemsContainer container)
        {
            foreach (KeyValuePair<string, int> loot in Main.config.startingLoot)
            {
                //TechTypeExtensions.FromString(loot.Key, out TechType tt, true);
                TechTypeExtensions.FromString(loot.Key, out TechType tt, true);
                if (tt == TechType.None)
                    continue;
                //AddDebug("Start Loot tt " + tt);
                // Main.Log("Start Loot " + tt + " " + loot.Value);
                TaskResult<GameObject> result = new TaskResult<GameObject>();
                TaskResult<GameObject> taskResult = result;
                for (int i = 0; i < loot.Value; i++)
                {
                    yield return CraftData.InstantiateFromPrefabAsync(tt, (IOut<GameObject>)taskResult);
                    Pickupable p = result.Get().GetComponent<Pickupable>();
                    p.Initialize();
                    if (container.HasRoomFor(p))
                    {
                        //Main.Log("Add " + tt);
                        container.UnsafeAdd(new InventoryItem(p));
                    }
                    else
                    {
                        //Main.Log("destroy " + tt);
                        UnityEngine.Object.Destroy(p.gameObject);
                        i = loot.Value;
                    }
                }
                result = null;
            }
        }

        [HarmonyPatch(typeof(LifepodDrop), "OnWaterCollision")]
        class LifepodDrop_OnWaterCollision_Patch
        {
            public static void Postfix(LifepodDrop __instance)
            {
                StorageContainer sc = __instance.GetComponentInChildren<StorageContainer>();
                if (sc)
                    UWE.CoroutineHost.StartCoroutine(SpawnStartLoot(sc.container));
            }
        }

    }
}
