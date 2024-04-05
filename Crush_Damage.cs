using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Crush_Damage
    {
        public static float crushInterval = 3f;
        public static int extraCrushDepth = 0;
        public static float crushDamageResistance = 0f;
        public static Dictionary<TechType, int> crushDepthEquipment = new Dictionary<TechType, int>();
        public static Dictionary<TechType, int> crushDamageEquipment = new Dictionary<TechType, int>();

        public static void CrushDamagePlayer()
        {
            if (!Player.main.gameObject.activeInHierarchy || !Player.main.IsSwimming())
                return;

            float depth = Ocean.GetDepthOf(Player.main.gameObject);
            float crushDepth = Main.config.crushDepth + extraCrushDepth;
            if (depth < crushDepth)
                return;

            float mult = 1f - crushDamageResistance;
            float damage = (depth - crushDepth) * Main.config.crushDamageMult * mult;
            if (Player.main.liveMixin)
                Player.main.liveMixin.TakeDamage(damage, Utils.GetRandomPosInView(), DamageType.Pressure);
            //AddDebug(" CrushDamageUpdate " + damage);
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnEquip")]
            static void OnEquipPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("crushDepthEquipment.Count " + crushDepthEquipment.Count);
                TechType tt = item.item.GetTechType();

                if (crushDepthEquipment.ContainsKey(tt))
                {
                    //Main.config.crushDepth += crushDepthEquipment[tt];
                    extraCrushDepth += crushDepthEquipment[tt];
                    //AddDebug("crushDepth " + Main.config.crushDepth);
                }
                if (crushDamageEquipment.ContainsKey(tt))
                {
                    //AddDebug("crushDamageEquipment " + crushDamageEquipment[tt]);
                    float res = crushDamageEquipment[tt] * .01f;
                    crushDamageResistance += Mathf.Clamp01(res);
                }
            }
                    
            [HarmonyPostfix]
            [HarmonyPatch("OnUnequip")]
            static void OnUnequipPostfix(Inventory __instance, InventoryItem item)
            {
                //Main.Message("Depth Class " + __instance.GetDepthClass());
                //TechTypeExtensions.FromString(loot.Key, out TechType tt, false);
                TechType tt = item.item.GetTechType();

                if (crushDepthEquipment.ContainsKey(tt))
                {
                    //Main.config.crushDepth -= crushDepthEquipment[tt];
                    extraCrushDepth -= crushDepthEquipment[tt];
                    //AddDebug("crushDepth " + Main.config.crushDepth);
                }
                if (crushDamageEquipment.ContainsKey(tt))
                {
                    //AddDebug("crushDamageEquipment " + crushDamageEquipment[tt]);
                    crushDamageResistance -= crushDamageEquipment[tt];
                    if (crushDamageResistance < 0f)
                        crushDamageResistance = 0f;
                }
            }
        }


        [HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            private static float crushTime = 0f;
            static void Postfix(Player __instance)
            {
                //Main.Message("Depth Class " + __instance.GetDepthClass());
                if (Main.config.crushDamageMult > 0f && crushInterval + crushTime < Time.time)
                {
                    crushTime = Time.time;
                    CrushDamagePlayer();
                }
            }
        }

        //[HarmonyPatch(typeof(Player), "GetCrushDamage")]
        class Player_CrushDamageUpdate_Patch
        {
            static void Postfix(Player __instance)
            {
                AddDebug("Player GetCrushDamage ");

            }
        }

        //[HarmonyPatch(typeof(SeaTruckSegment), "Start")]
        class SeaTruckSegment_Start_Patch
        {
            static void Postfix(SeaTruckSegment __instance)
            {
                //Main.Message("Depth Class " + __instance.GetDepthClass());
                //if (Main.config.crushDamageMult > 0f && crushInterval + crushTime < Time.time)
                //{
                //    crushTime = Time.time;
                //    CrushDamage();
                //}
            }
        }

        [HarmonyPatch(typeof(CrushDamage), "CrushDamageUpdate")]
        class CrushDamage_CrushDamageUpdate_Patch
        { // player does not have this
            public static bool Prefix(CrushDamage __instance)
            {
                //if (Main.config.vehicleCrushDamageMult == 0f)
                //    return true;

                if (!__instance.gameObject.activeInHierarchy || !__instance.enabled || !__instance.GetCanTakeCrushDamage())
                    return false;

                //AddDebug(__instance + " CrushDamageUpdate " );

                float depth = __instance.depthCache.Get();
                if (depth < __instance.crushDepth)
                    return false;

                float damage = (depth - __instance.crushDepth) * Main.config.vehicleCrushDamageMult;
                if (damage == 0f)
                    damage = __instance.damagePerCrush;
                //AddDebug("damage " + damage);
                __instance.liveMixin.TakeDamage(damage, __instance.transform.position, DamageType.Pressure);
                foreach (SeaTruckSegment sts in __instance.GetComponentsInChildren<SeaTruckSegment>())
                //for (SeaTruckSegment sts = __instance; sts; sts = sts.isFrontConnected ? sts.frontConnection.GetConnection().truckSegment : null)
                {
                    //AddDebug(("crush damage " + damage));
                    if (!sts.isMainCab)
                    {
                        sts.GetComponent<LiveMixin>().TakeDamage(damage, __instance.transform.position, DamageType.Pressure);
                    }
                }
                if (__instance.soundOnDamage)
                    __instance.soundOnDamage.Play();

                return false;
            }
        }

    }
}
