using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Predators_Patch
    {
        //static HashSet<SubRoot> cyclops = new HashSet<SubRoot>();
        //static Dictionary<AttackCyclops, AggressiveWhenSeeTarget> attackCyclopsAWST = new Dictionary<AttackCyclops, AggressiveWhenSeeTarget>();
        public static HashSet<TechType> predatorExclusion = new HashSet<TechType> { TechType.Crash };

        public static bool IsVehiclePowered(GameObject go)
        {
            Vehicle vehicle = go.GetComponent<Vehicle>();
            if (vehicle)
            {
                return vehicle.IsPowered();
            }
            SeaTruckSegment sts = go.GetComponent<SeaTruckSegment>();
            if (sts)
            {
                PowerRelay pr = sts.GetComponent<PowerRelay>();
                return pr && pr.isPowered;
            }
            Hoverbike hb = go.GetComponent<Hoverbike>();
            if (hb)
            {
                EnergyMixin em = hb.GetComponent<EnergyMixin>();
                return em && em.charge > 0f;
            }
            return false;
        }

        public static bool IsLightOn(GameObject go)
        {
            Vehicle vehicle = go.GetComponent<Vehicle>();
            if (vehicle)
            {
                Transform lightsT = vehicle.transform.Find("lights_parent");
                return lightsT && lightsT.gameObject.activeSelf;
            }
            SeaTruckSegment sts = go.GetComponent<SeaTruckSegment>();
            if (sts)
            {
                PowerRelay pr = sts.GetComponent<PowerRelay>();
                return pr && pr.isPowered;
            }
            Hoverbike hb = go.GetComponent<Hoverbike>();
            if (hb)
            {
                Transform lightT = hb.transform.Find("Deployed/Lights");
                return lightT && lightT.gameObject.activeSelf;
            }
            return false;
        }

        public static bool IsVehicleMoving(GameObject go)
        {
            Vehicle vehicle = go.GetComponent<Vehicle>();
            SeaTruckSegment sts = go.GetComponent<SeaTruckSegment>();
            if (!vehicle && !sts)
                return false;

            Vector3 vel = Vector3.zero;
            if (vehicle)
                vel = vehicle.useRigidbody.velocity;
            else if (sts)
                vel = sts.GetComponent<Rigidbody>().velocity;

            return vel.x > 1f || vel.y > 1f || vel.z > 1f;
        }

        [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
        internal class AggressiveWhenSeeTarget_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetAggressionTarget")]
            public static bool GetAggressionTargetPrefix(AggressiveWhenSeeTarget __instance, ref GameObject __result)
            {
                //if (__instance.maxSearchRings > 1)
                //    AddDebug(__instance.name + " maxSearchRings " + __instance.maxSearchRings);
                float aggrMult = GameModeManager.GetCreatureAggressionModifier();
                if (__instance.targetType != EcoTargetType.Shark || aggrMult <= 1 || predatorExclusion.Contains(__instance.myTechType))
                    return true;

                int searchRings = Mathf.RoundToInt(__instance.maxSearchRings * aggrMult);
                IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(__instance.targetType, __instance.transform.position, __instance.isTargetValidFilter, searchRings);
                __result = ecoTarget == null ? null : ecoTarget.GetGameObject();
                //if (__result == Player.main.gameObject)
                //AddDebug(__instance.myTechType + " AggressionTarget PLAYER ");
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("IsTargetValid", new Type[] { typeof(GameObject) })]
            public static bool IsTargetValidPrefix(GameObject target, AggressiveWhenSeeTarget __instance, ref bool __result)
            {
                if (__instance.targetType != EcoTargetType.Shark || predatorExclusion.Contains(__instance.myTechType))
                    return true;

                float aggrMult = GameModeManager.GetCreatureAggressionModifier();

                if (target == Player.main.gameObject)
                {
                    //AddDebug(__instance.myTechType + " Player");
                    if (!Player.main.CanBeAttacked() || PrecursorMoonPoolTrigger.inMoonpool || aggrMult == 0)
                    {
                        __result = false;
                        return false;
                    }
                }
                if (Util.IsVehicle(target))
                {
                    if (aggrMult == 0)
                    {
                        __result = false;
                        return false;
                    }
                    if (!Util.IsPlayerInVehicle())
                    {
                        if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Only_if_lights_on && !IsLightOn(target))
                        {
                            __result = false;
                            return false;
                        }
                        else if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.No)
                        {
                            __result = false;
                            return false;
                        }
                        //if (CreatureData.GetBehaviourType(__instance.myTechType) == BehaviourType.Leviathan)
                        //{ // prevent leviathan attack on land
                        //    if (Ocean.main.GetDepthOf(target) < 5f)
                        //    {
                        //        __result = false;
                        //        return false;
                        //    }
                        //}
                    }
                }
                if (target == null || __instance.creature.IsFriendlyTo(target))
                {
                    __result = false;
                    return false;
                }
                TechType targetTT = CraftData.GetTechType(target);
                if (__instance.ignoreSameKind && targetTT == __instance.myTechType)
                {
                    __result = false;
                    return false;
                }
                if (__instance.ignoreFrozen)
                {
                    FrozenMixin fm = target.GetComponent<FrozenMixin>();
                    if (fm != null && fm.IsFrozen())
                    {
                        __result = false;
                        return false;
                    }
                }
                float dist = Vector3.Distance(target.transform.position, __instance.transform.position);

                float leashDistance = __instance.leashDistance * aggrMult;
                //float aggrMult = Main.config.aggrMult > 1 ? Main.config.aggrMult : 1;
                if (dist > __instance.maxRangeScalar * aggrMult || leashDistance > 0f && (__instance.creature.GetLeashPosition() - target.transform.position).sqrMagnitude > leashDistance * leashDistance)
                {
                    __result = false;
                    return false;
                }
                if (!Util.Approximately(__instance.minimumVelocity, 0f))
                {
                    Rigidbody rb = target.GetComponentInChildren<Rigidbody>();
                    if (rb && rb.velocity.magnitude <= __instance.minimumVelocity)
                    {
                        __result = false;
                        return false;
                    }
                }
                __result = __instance.creature.GetCanSeeObject(target);
                //__result = !Physics.Linecast(__instance.transform.position, target.transform.position, Voxeland.GetTerrainLayerMask());
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("ScanForAggressionTarget")]
            public static bool ScanForAggressionTargetPrefix(AggressiveWhenSeeTarget __instance)
            {
                if (EcoRegionManager.main == null || !__instance.gameObject.activeInHierarchy || !__instance.enabled)
                    return false;

                if (__instance.targetType != EcoTargetType.Shark || predatorExclusion.Contains(__instance.myTechType))
                    return true;

                float aggrMult = GameModeManager.GetCreatureAggressionModifier();

                if (aggrMult <= 1 && __instance.creature && __instance.creature.Hunger.Value < __instance.hungerThreshold)
                    return true;

                if (aggrMult > 5f && Player.main.CanBeAttacked() && __instance.creature.GetCanSeeObject(Player.main.gameObject))
                {
                    __instance.creature.Aggression.Add(__instance.aggressionPerSecond);
                    //__instance.lastScarePosition.lastScarePosition = Player.main.gameObject.transform.position;
                    __instance.lastTarget.SetTarget(Player.main.gameObject, __instance.targetPriority);
                    if (__instance.sightedSound != null && !__instance.sightedSound.GetIsPlaying() && !Creature_Patch.silentCreatures.Contains(__instance.myTechType))
                        __instance.sightedSound.StartEvent();

                    //AddDebug(__instance.myTechType + " attack player " + );
                    return false;
                }
                GameObject aggressionTarget = __instance.GetAggressionTarget();
                if (aggressionTarget != null)
                {
                    float dist = Vector3.Distance(aggressionTarget.transform.position, __instance.transform.position);
                    float num2 = DayNightUtils.Evaluate(__instance.maxRangeScalar, __instance.maxRangeMultiplier);
                    //distMult < 0 if target very close
                    float mult = __instance.aggressionPerSecond * __instance.distanceAggressionMultiplier.Evaluate((num2 - dist) / num2);
                    //if (mult < 1f)
                    //    mult = 1f;
                    if (mult <= 0f)
                        return false;
                    //Main.Log(__instance.myTechType + " " + aggressionTarget.name + " aggr dist " + dist + " distMult " + distMult + " aggr/second " + __instance.aggressionPerSecond);
                    //float infection = 1f;
                    //UnityEngine.Debug.DrawLine(aggressionTarget.transform.position, __instance.transform.position, Color.white);
                    //__instance.lastTarget.target = aggressionTarget;
                    GameObject lastTarget = __instance.lastTarget.target;
                    if (lastTarget != null && lastTarget != aggressionTarget && (__instance.lastTarget.targetPriority > __instance.targetPriority && Time.time <= __instance.lastTarget.targetTime + 5f))
                        return false;

                    __instance.creature.Aggression.Add((mult));
                    __instance.lastTarget.SetTarget(aggressionTarget, __instance.targetPriority);
                    //__instance.lastScarePosition.lastScarePosition = aggressionTarget.transform.position;
                    if (__instance.sightedSound != null && !__instance.sightedSound.GetIsPlaying() && !Creature_Patch.silentCreatures.Contains(__instance.myTechType))
                        __instance.sightedSound.StartEvent();
                }
                return false;
            }

        }

        [HarmonyPatch(typeof(AttackLastTarget), "CanAttackTarget")]
        class AttackLastTarget_CanAttackTarget_Patch
        {
            static bool Prefix(AttackLastTarget __instance, GameObject target, ref bool __result)
            {
                if (target == null || __instance.creature.IsFriendlyTo(target))
                {
                    __result = false;
                    return false;
                }
                LiveMixin lm = target.GetComponent<LiveMixin>();
                if (!lm || !lm.IsAlive())
                {
                    __result = false;
                    return false;
                }
                if (target == Player.main.gameObject)
                {
                    if (!Player.main.CanBeAttacked() || GameModeManager.HasNoCreatureAggression())
                    {
                        __result = false;
                        return false;
                    }
                }
                Vehicle vehicle = target.GetComponent<Vehicle>();
                SeaTruckSegment sts = target.GetComponent<SeaTruckSegment>();
                if (vehicle || sts)
                {
                    if (GameModeManager.HasNoCreatureAggression())
                    {
                        __result = false;
                        return false;
                    }
                    bool playerInSeaTruck = Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment;
                    if ((vehicle && !Player.main.inExosuit) || (sts && !playerInSeaTruck))
                    {
                        if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Only_if_lights_on)
                        {
                            __result = IsLightOn(target);
                            return false;
                        }
                        __result = ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Yes;
                        return false;
                    }
                    //Vector3 vel = Vector3.zero;
                    //if (vehicle)
                    //    vel = vehicle.useRigidbody.velocity;
                    //else if (sts)
                    //    vel = sts.GetComponent<Rigidbody>().velocity;

                    //__result = vel.x > 1f || vel.y > 1f || vel.z > 1f;
                    __result = true;
                    return false;
                }
                bool underwater = target.transform.position.y < 0f;
                __result = (!__instance.ignoreAboveWaterTargets || underwater) && !(__instance.ignoreUnderWaterTargets & underwater);
                return false;
            }
        }

        [HarmonyPatch(typeof(EcoRegion), "FindNearestTarget")]
        internal class EcoRegion_FindNearestTarget_Patch
        {
            public static bool PreFix(EcoRegion __instance, EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid, ref float bestDist, ref IEcoTarget best)
            {
                float aggrMult = GameModeManager.GetCreatureAggressionModifier();
                if (aggrMult == 1f || type != EcoTargetType.Shark)
                    return true;
                //ProfilingUtils.BeginSample("EcoRegion.FindNearestTarget");
                __instance.timeStamp = Time.time;
                //float agr = Main.config.aggrMult > 1 ? Main.config.aggrMult : 1;
                HashSet<IEcoTarget> ecoTargetSet;
                float minSqrMagnitude = float.MaxValue;
                if (!__instance.ecoTargets.TryGetValue(type, out ecoTargetSet))
                    return false;

                foreach (IEcoTarget ecoTarget in ecoTargetSet)
                {
                    //HashSet<IEcoTarget>.Enumerator enumerator = ecoTargetSet.GetEnumerator();
                    //while (enumerator.MoveNext())
                    //{
                    //IEcoTarget current = enumerator.Current;
                    if (ecoTarget != null)
                    {
                        float sqrMagnitude = (wsPos - ecoTarget.GetPosition()).sqrMagnitude;
                        //if (agr > 1f)
                        //{
                        bool player = ecoTarget.GetGameObject() == Player.main.gameObject;
                        //bool vehicle = ecoTarget.GetGameObject().GetComponent<Vehicle>();
                        //float aggrMult = GameModeManager.GetCreatureAggressionModifier();
                        if (player)
                            sqrMagnitude /= aggrMult;
                        //if (agr == 3)
                        //    sqrMagnitude = 0f;
                        //}
                        if (sqrMagnitude < minSqrMagnitude && (isTargetValid == null || isTargetValid(ecoTarget)))
                        {
                            best = ecoTarget;
                            minSqrMagnitude = sqrMagnitude;
                        }
                    }
                }
                if (best != null)
                    bestDist = Mathf.Sqrt(minSqrMagnitude);

                return false;
            }
        }

        //[HarmonyPatch(typeof(MoveTowardsTarget), "UpdateCurrentTarget")]
        internal class MoveTowardsTarget_UpdateCurrentTarget_Patch
        { // this does not target player
            public static bool Prefix(MoveTowardsTarget __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (__instance.targetType == EcoTargetType.Shark)
                {
                    //AddDebug(tt + " aggr " + __instance.creature.Aggression.Value + " req aggr " + __instance.requiredAggression);
                    float aggrMult = GameModeManager.GetCreatureAggressionModifier();
                    float aggr = aggrMult > 1f ? aggrMult : 1f;
                    if (EcoRegionManager.main != null && (Util.Approximately(__instance.requiredAggression, 0f) || __instance.creature.Aggression.Value * aggr >= __instance.requiredAggression))
                    {
                        AggressiveWhenSeeTarget awst = __instance.GetComponent<AggressiveWhenSeeTarget>();
                        if (awst)
                            aggr *= awst.maxSearchRings;
                        IEcoTarget nearestTarget = EcoRegionManager.main.FindNearestTarget(__instance.targetType, __instance.transform.position, __instance.isTargetValidFilter, (int)aggr);
                        __instance.currentTarget = nearestTarget;
                    }
                    return false;
                }
                else
                    return true;
            }
        }

        [HarmonyPatch(typeof(AttachToVehicle), "IsValidTarget")]
        internal class AttachToVehicle_IsValidTarget_Patch
        {
            public static void Postfix(AttachToVehicle __instance, IEcoTarget target, ref bool __result)
            {
                float aggrMult = GameModeManager.GetCreatureAggressionModifier();

                if (aggrMult == 0f || predatorExclusion.Contains(CraftData.GetTechType(__instance.gameObject)))
                    __result = false;
            }
        }

        //[HarmonyPatch(typeof(Creature), "Start")]
        internal class Creature_Start_Patch
        {
            public static void Postfix(Creature __instance)
            {
                if (__instance.hasEyes)
                {
                    TechType techType = CraftData.GetTechType(__instance.gameObject);
                    //Util.Log(techType + " eyeFOV " + __instance.eyeFOV);
                }

                //AddDebug("Creature Start " + techType);
                //if (Main.config.canAttackSub.Contains(techType))
                {
                    //AttackCyclops attackCyclops = __instance.gameObject.GetComponent<AttackCyclops>();
                    //if (!attackCyclops)
                    //{
                    //    AddDebug("Add  AttackCyclops");
                    //    attackCyclops = __instance.gameObject.AddComponent<AttackCyclops>();
                    //    attackCyclops.aggressiveToNoise = new CreatureTrait(0);
                    //    attackCyclops.maxDistToLeash = 150f;
                    //    attackCyclops.evaluatePriority = .9f;
                    //    attackCyclops.swimVelocity = 25f;
                    //    attackCyclops.attackPause = 3f;
                    //}
                }
            }
        }

        [HarmonyPatch(typeof(AggressiveToPilotingVehicle), "UpdateAggression")]
        internal class AggressiveToPilotingVehicle_UpdateAggression_Patch
        {
            public static bool Prefix(AggressiveToPilotingVehicle __instance)
            {
                //AddDebug(" AggressiveToPilotingVehicle UpdateAggression" + __instance.name);
                Player main = Player.main;
                //if (main == null || main.GetMode() != Player.Mode.LockedPiloting)
                if (main == null || !Util.IsPlayerInVehicle())
                    return false;

                float aggrMult = GameModeManager.GetCreatureAggressionModifier();
                if (aggrMult == 0)
                    return false;

                TechType myTT = CraftData.GetTechType(__instance.gameObject);
                if (predatorExclusion.Contains(myTT))
                    return false;

                Vehicle vehicle = main.GetVehicle();
                SeaTruckSegment sts = main.GetComponentInParent<SeaTruckSegment>();

                if (vehicle)
                {
                    if (Vector3.Distance(vehicle.transform.position, __instance.transform.position) > __instance.range * aggrMult)
                        return false;

                    __instance.lastTarget.SetTarget(vehicle.gameObject, __instance.targetPriority);
                    __instance.creature.Aggression.Add(__instance.aggressionPerSecond * __instance.updateAggressionInterval * aggrMult);
                }
                else if (sts)
                {
                    if (Vector3.Distance(sts.transform.position, __instance.transform.position) > __instance.range * aggrMult)
                        return false;

                    __instance.lastTarget.SetTarget(sts.gameObject, __instance.targetPriority);
                    __instance.creature.Aggression.Add(__instance.aggressionPerSecond * __instance.updateAggressionInterval * aggrMult);
                }
                //__instance.creature.GetCanSeeObject(Player.main.gameObject))
                //AddDebug(" AggressiveToPilotingVehicle range " + __instance.range);
                //AddDebug(" prevVelocity " + vehicle.prevVelocity);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeleeAttack))]
        internal class MeleeAttack_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("OnEnable")]
            public static void OnEnablePrefix(MeleeAttack __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                AddDebug(tt + " canBitePlayer " + __instance.canBitePlayer + " canBiteVehicle " + __instance.canBiteVehicle);
                //testMeleeAttack.Add(tt + " canBitePlayer " + __instance.canBitePlayer + " canBiteVehicle " + __instance.canBiteVehicle + " canBiteCyclops " + __instance.canBiteCyclops);
            }

            public static bool IsValidVehicle(GameObject target)
            {
                float aggrMult = GameModeManager.GetCreatureAggressionModifier();
                if (aggrMult == 0f || !Util.IsVehicle(target))
                    return false;

                if (Util.IsPlayerInVehicle())
                    return true;
                else if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Only_if_lights_on && IsLightOn(target))
                    return true;
                else
                    return ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Yes;
            }

            [HarmonyPrefix]
            [HarmonyPatch("CanDealDamageTo")]
            public static bool CanDealDamageToPrefix(MeleeAttack __instance, GameObject target, ref bool __result)
            { // skipped some code
                //AddDebug(__instance.name + " CanDealDamageTo " + target.name );
                Player player = target.GetComponent<Player>();
                if (player != null)
                {
                    //AddDebug(__instance.name + " CanDealDamageTo player CanBeAttacked " + player.CanBeAttacked() + " canBitePlayer " + __instance.canBitePlayer);
                    __result = __instance.canBitePlayer && player.CanBeAttacked();
                    return false;
                }
                if (__instance.biteOnlyCurrentTarget)
                {
                    __result = target == __instance.lastTarget.target;
                    return false;
                }
                if (__instance.canBiteVehicle && Util.IsVehicle(target) && IsValidVehicle(target))
                {
                    __result = true;
                    return false;
                }
                __result = __instance.canBiteCreature && target.GetComponent<Creature>() != null;
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //if (tt == TechType.BruteShark)
                //{
                //    LargeWorldEntity lwe = target.GetComponentInParent<LargeWorldEntity>();
                //    string name = lwe.gameObject.name;
                //    AddDebug(__instance.name + " CanDealDamageTo " + name + " " + __result);
                //}
                return false;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("CanBite")]
            public static void CanBitePostfix(MeleeAttack __instance, GameObject target, bool __result)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                TechType tt1 = CraftData.GetTechType(target);
                if (tt1 != TechType.None)
                    AddDebug(tt + " MeleeAttack CanBite " + tt1 + " " + __result);
            }
        }



    }
}

