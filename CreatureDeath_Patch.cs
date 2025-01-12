using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(CreatureDeath))]
    class CreatureDeath_Patch
    {
        public static HashSet<TechType> notRespawningCreatures = new HashSet<TechType> { };
        public static HashSet<TechType> notRespawningCreaturesIfKilledByPlayer = new HashSet<TechType> { };
        internal static Dictionary<TechType, int> respawnTime = new Dictionary<TechType, int>();
        public static HashSet<CreatureDeath> creatureDeathsToDestroy = new HashSet<CreatureDeath>();

        public static void TryRemoveCorpses()
        {
            //AddDebug("TryRemoveCorpses " + creatureDeathsToDestroy.Count);
            foreach (var cd in creatureDeathsToDestroy)
            {
                Pickupable pickupable = cd.GetComponent<Pickupable>();
                if (pickupable && pickupable.inventoryItem != null)
                {
                    //AddDebug("try RemoveCorpse inventoryItem " + cd.name);
                    continue;
                }
                //AddDebug("RemoveCorpse " + cd.name);
                if (ConfigToEdit.removeDeadCreaturesOnLoad.Value)
                    UnityEngine.Object.Destroy(cd.gameObject);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void StartPostfix(CreatureDeath __instance)
        {
            TechType techType = CraftData.GetTechType(__instance.gameObject);
            //if (!creatureDeaths.Contains(techType))
            //{
            //    creatureDeaths.Add(techType);
            //    Main.logger.LogMessage("CreatureDeath " + techType + " respawns " + __instance.respawn+ " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature + " respawnInterval " + __instance.respawnInterval);
            //}
            __instance.respawn = !notRespawningCreatures.Contains(techType);
            __instance.respawnOnlyIfKilledByCreature = notRespawningCreaturesIfKilledByPlayer.Contains(techType);
            //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawn " + __instance.respawn);
            //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
            if (respawnTime.ContainsKey(techType))
                __instance.respawnInterval = respawnTime[techType] * DayNightCycle.main.dayLengthSeconds;
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnTakeDamage")]
        static void OnTakeDamagePostfix(CreatureDeath __instance, DamageInfo damageInfo)
        {
            //AddDebug("OnTakeDamage " + damageInfo.dealer.name);
            if (!ConfigToEdit.heatBladeCooks.Value && damageInfo.type == DamageType.Heat && damageInfo.dealer == Player.mainObject)
                __instance.lastDamageWasHeat = false;
        }
        [HarmonyPrefix]
        [HarmonyPatch("RemoveCorpse")]
        static bool RemoveCorpsePrefix(CreatureDeath __instance)
        {
            creatureDeathsToDestroy.Add(__instance);
            return false;
        }
        //[HarmonyPostfix]
        //[HarmonyPatch("OnKill")]
        static void OnKillPostfix(CreatureDeath __instance)
        {
            //AddDebug("OnKill ");
            AquariumFish aquariumFish = __instance.GetComponent<AquariumFish>();
            if (aquariumFish)
                UnityEngine.Object.Destroy(aquariumFish);
        }
    }
}
