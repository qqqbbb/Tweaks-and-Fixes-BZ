﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Tweaks_Fixes
{
    internal class Silent_Creatures
    {
        public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };


        [HarmonyPatch(typeof(Creature))]
        public static class Creature_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(Creature __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    //AddDebug(tt + " Creature Start");
                    foreach (FMOD_CustomEmitter ce in __instance.GetComponentsInChildren<FMOD_CustomEmitter>())
                        ce.evt.setVolume(0);
                }
            }
        }

        [HarmonyPatch(typeof(AttackLastTarget), "StartPerform")]
        class AttackLastTarget_StartPerform_Patch
        {
            public static void Prefix(AttackLastTarget __instance)
            {
                if (__instance.attackStartSound == null)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    __instance.attackStartSound.evt.setVolume(0);
                    //AddDebug(tt + " AttackLastTarget StartPerform");
                }
            }
        }

        [HarmonyPatch(typeof(MeleeAttack), "OnEnable")]
        class MeleeAttack_OnEnable_Patch
        {
            public static void Postfix(MeleeAttack __instance)
            {
                if (__instance.biteSound == null)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    __instance.biteSound.evt.setVolume(0);
                    //AddDebug(tt + " MeleeAttack OnEnable");
                }
            }
        }

        [HarmonyPatch(typeof(AggressiveWhenSeeTarget), "Start")]
        class AggressiveWhenSeeTarget_Start_Patch
        {
            public static void Postfix(AggressiveWhenSeeTarget __instance)
            {
                if (__instance.sightedSound == null)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    __instance.sightedSound.evt.setVolume(0);
                    //AddDebug(tt + " AggressiveWhenSeeTarget Start");
                }
            }
        }
    }
}
