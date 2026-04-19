using FMODUnity;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Recyclotron))]
    internal class Recyclotron_
    {
        [HarmonyPrefix, HarmonyPatch("Recycle")]
        static bool Prefix(Recyclotron __instance)
        {
            if (ConfigMenu.recyclotronSuccessChance.Value == 100)
                return true;

            __instance.StartCoroutine(RecycleAsync(__instance));
            return false;
        }

        static private IEnumerator RecycleAsync(Recyclotron recyclotron)
        {
            string errorMessage = null;
            if (!recyclotron.IsPowered())
                errorMessage = "RecyclotronErrorNoPower";
            else if (recyclotron.wasteList.Count > 1)
                errorMessage = "RecyclotronErrorTooManyItems";

            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessage.AddMessage(Language.main.Get(errorMessage));
                yield break;
            }
            if (recyclotron.wasteList.Count != 1)
                yield break;

            InventoryItem wasteItem = recyclotron.wasteList.GetLast().inventoryItem;
            GameObject recycleGameObject = wasteItem.item.gameObject;
            if (recyclotron.IsUsedBattery(recycleGameObject))
            {
                errorMessage = "RecyclotronErrorUsedItem";
                ErrorMessage.AddMessage(Language.main.Get(errorMessage));
                yield break;
            }
            TechType techType = CraftData.GetTechType(recycleGameObject);
            //AddDebug("recycle techType " + techType);
            List<Ingredient> list = recyclotron.GetIngredients();
            if (list == null || list.Count == 0 || Recyclotron.bannedTech.Contains(techType) || TechData.GetCraftAmount(techType) > 1 || ((IItemsContainer)recyclotron.storageContainer.container).AllowedToRemove(wasteItem.item, true) == false || recyclotron.storageContainer.container.HasRoomForComponents(techType) == false)
                yield break;

            float successChance = ConfigMenu.recyclotronSuccessChance.Value * .01f;
            EnergyMixin energyMixin = recycleGameObject.GetComponent<EnergyMixin>();
            if (energyMixin)
            {
                GameObject batteryGameObject = energyMixin.GetBatteryGameObject();
                if (batteryGameObject)
                {
                    energyMixin.RemoveBattery();
                    InventoryItem inventoryItem = new InventoryItem(batteryGameObject.GetComponent<Pickupable>());
                    inventoryItem.item.Initialize();
                    recyclotron.storageContainer.container.UnsafeAdd(inventoryItem);
                }
            }
            foreach (Ingredient ingredient in list)
            {
                for (int j = 0; j < ingredient.amount; j++)
                {
                    if (successChance < UnityEngine.Random.value)
                        continue;

                    if (!Recyclotron.batteryTech.Contains(ingredient.techType) || energyMixin == null)
                    {
                        TaskResult<GameObject> result = new TaskResult<GameObject>();
                        yield return CraftData.InstantiateFromPrefabAsync(ingredient.techType, result);
                        InventoryItem inventoryItem2 = new InventoryItem(result.Get().GetComponent<Pickupable>());
                        inventoryItem2.item.Initialize();
                        recyclotron.storageContainer.container.UnsafeAdd(inventoryItem2);
                    }
                }
            }
            recyclotron.recycleVFX.Play();
            ((IItemsContainer)recyclotron.storageContainer.container).RemoveItem(wasteItem, true, false);
            UnityEngine.Object.Destroy(recycleGameObject);
            RuntimeManager.PlayOneShotAttached(recyclotron.recycleSound.id, recyclotron.gameObject);
        }
    }
}
