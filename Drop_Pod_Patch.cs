using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Drop_Pod_Patch
    {
        static Light podLight;
        public static PowerSource podPowerSource;
        static GhostCrafter podGhostCrafter;
        public static Dictionary<TechType, int> newGameLoot = new Dictionary<TechType, int>();


        public static IEnumerator SpawnStartLoot(ItemsContainer container)
        {
            foreach (KeyValuePair<TechType, int> loot in newGameLoot)
            {
                //TechTypeExtensions.FromString(loot.Key, out TechType tt, true);
                //AddDebug("Start Loot tt " + tt);
                // Main.Log("Start Loot " + tt + " " + loot.Value);
                TaskResult<GameObject> result = new TaskResult<GameObject>();
                TaskResult<GameObject> taskResult = result;
                for (int i = 0; i < loot.Value; i++)
                {
                    yield return CraftData.InstantiateFromPrefabAsync(loot.Key, (IOut<GameObject>)taskResult);
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

        class RegenerateSunPowerSource : MonoBehaviour
        {
            //float regenerationThreshhold = 25f;
            float regenerationInterval = 10f;
            float regenerationAmount = 1f;
            float lightPowerCost = .025f;

            void Start()
            {
                //AddDebug("RegenerateSunPowerSource Start");
                //regenerationThreshhold = Main.config.dropPodMaxPower;
                this.InvokeRepeating("Regenerate", 0f, regenerationInterval);
            }

            void Regenerate()
            {
                float amount = podPowerSource.power - lightPowerCost + regenerationAmount * DayNightCycle.main.GetLocalLightScalar();
                podPowerSource.SetPower(amount);
                //AddDebug("RegenerateSunPowerSource Regenerate " + amount);
                if (podLight)
                    podLight.enabled = podPowerSource.power > 0f;
            }
        }

        [HarmonyPatch(typeof(Fabricator), "Start")]
        class Fabricator_Start_Patch
        {
            static void Postfix(Fabricator __instance)
            {
                if (ConfigMenu.dropPodMaxPower.Value == 0)
                    return;

                GameObject dropPod = __instance.transform.parent.gameObject;
                if (dropPod.GetComponent<LifepodDrop>())
                {
                    podGhostCrafter = __instance;
                    //podLight = dropPod.GetComponentInChildren<Light>();
                    __instance.needsPower = true;
                    //AddDebug("Fabricator Start needsPower " + __instance.needsPower);
                    podPowerSource = __instance.gameObject.EnsureComponent<PowerSource>();
                    podPowerSource.maxPower = ConfigMenu.dropPodMaxPower.Value;
                    __instance.gameObject.AddComponent<RegenerateSunPowerSource>();
                    if (Main.configMain.podPower.ContainsKey(SaveLoadManager.main.currentSlot))
                        podPowerSource.power = Main.configMain.podPower[SaveLoadManager.main.currentSlot];
                }
            }
        }

        [HarmonyPatch(typeof(GhostCrafter), "OnHandHover")]
        class GhostCrafter_OnHandHover_Patch
        {
            static bool Prefix(GhostCrafter __instance, GUIHand hand)
            {
                if (ConfigMenu.dropPodMaxPower.Value == 0 || __instance != podGhostCrafter)
                    return true;
                //AddDebug("GhostCrafter OnHandHover " + __instance.powerRelay.GetPower());
                if (!__instance.enabled || __instance.logic == null)
                    return false;

                string text1 = __instance.handOverText;
                string text2 = string.Empty;
                if (__instance.logic.inProgress)
                {
                    text1 = __instance.logic.craftingTechType.AsString();
                    HandReticle.main.SetProgress(__instance.logic.progress);
                    HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1.5f);
                }
                else if (__instance.HasCraftedItem())
                {
                    text1 = __instance.logic.currentTechType.AsString();
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                }
                else
                {
                    if (__instance.HasEnoughPower())
                        text2 = Language.main.GetFormat<int, int>("PowerCellStatus", Mathf.FloorToInt(__instance.powerRelay.GetPower()), Mathf.FloorToInt(__instance.powerRelay.GetMaxPower()));
                    else
                        text2 = Language.main.Get("Unpowered");

                    HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                }
                HandReticle.main.SetText(HandReticle.TextType.Hand, text1, true, GameInput.Button.LeftHand);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, true);
                return false;
            }
        }


        [HarmonyPatch(typeof(LifepodDrop))]
        class LifepodDrop_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnWaterCollision")]
            public static void OnWaterCollisionPostfix(LifepodDrop __instance)
            {
                StorageContainer sc = __instance.GetComponentInChildren<StorageContainer>();
                if (sc)
                    UWE.CoroutineHost.StartCoroutine(SpawnStartLoot(sc.container));
            }
            [HarmonyPostfix]
            [HarmonyPatch("IInteriorSpace.GetInsideTemperature")]
            public static void GetInsideTemperaturePostfix(LifepodDrop __instance, ref float __result)
            {
                //AddDebug("LifepodDrop inside GetInsideTemperature " + __result);
                __result = ConfigToEdit.insideBaseTemp.Value;
                if (ConfigMenu.useRealTempForPlayerTemp.Value && ConfigMenu.dropPodMaxPower.Value > 0)
                {
                    //AddDebug("LifepodDrop GetPower " + podPowerSource.GetPower());
                    if (podPowerSource && podPowerSource.GetPower() <= 0)
                        //__result = Player_Patches.ambientTemperature;
                        __result = WaterTemperatureSimulation.main.GetTemperature(__instance.transform.position);
                }
            }
        }

        //[HarmonyPatch(typeof(LifepodDrop), "Start")]
        class LifepodDrop_Start_Patch
        {
            static void Postfix(LifepodDrop __instance)
            {
                VFXSurface surface = __instance.gameObject.EnsureComponent<VFXSurface>();
                surface.surfaceType = VFXSurfaceTypes.metal;
            }
        }

    }
}
