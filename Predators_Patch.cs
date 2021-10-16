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

        public static bool IsVehicle(GameObject go)
        {
            return go.GetComponent<Vehicle>() || go.GetComponent<SeaTruckSegment>() || go.GetComponent<Hoverbike>();
        }

        public static bool IsPlayerInsideVehicle(GameObject go)
        {
            Vehicle vehicle = go.GetComponent<Vehicle>();
            if (vehicle && Player.main.currentMountedVehicle == vehicle)
                return true;
            if (Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment)
                return true;
            Hoverbike hb = go.GetComponent<Hoverbike>();
            return hb && hb.playerInHoverbike;
        }

        public static bool IsVehiclePowered(GameObject go)
        {
            Vehicle vehicle = go.GetComponent<Vehicle>();
            if (vehicle)
            {
                PowerRelay pr = vehicle.GetComponent<PowerRelay>();
                return pr && pr.isPowered;
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

        [HarmonyPatch(typeof(MeleeAttack))]
        internal class MeleeAttack__Patch
        {
            [HarmonyPatch("IsValidVehicle")]
            [HarmonyPrefix]
            public static bool IsValidVehiclePrefix(MeleeAttack __instance, GameObject target, ref bool __result)
            {
                if (GameModeUtils.IsInvisible() || Main.config.aggrMult == 0f || !IsVehicle(target))
                {
                    __result = false;
                    return false;
                }
                if (Main.config.emptyVehicleCanBeAttacked == Config.EmptyVehicleCanBeAttacked.Only_if_lights_on)
                {
                    if (!IsPlayerInsideVehicle(target) && IsLightOn(target))
                    {
                        __result = true;
                        return false;
                    }
                }
                else if (Main.config.emptyVehicleCanBeAttacked == Config.EmptyVehicleCanBeAttacked.No)
                {
                    __result = IsPlayerInsideVehicle(target);
                    return false;
                }
                __result = true;
                return false;
            }

        }
              
        [HarmonyPatch(typeof(CreatureAggressionManager), "OnMeleeAttack")]
        internal class CreatureAggressionManager_OnMeleeAttack_Patch
        {
            public static bool Prefix(CreatureAggressionManager __instance, GameObject target)
            {
                //AddDebug("AggressiveWhenSeeTarget start " + __instance.myTechType + " " + 
                BehaviourType behaviourType = CreatureData.GetBehaviourType(target);
                if (behaviourType != BehaviourType.Shark)
                    return true;

                if (__instance.fishAttackInterval > 0f && (behaviourType == BehaviourType.SmallFish || behaviourType == BehaviourType.MediumFish))
                {
                    if (__instance.aggressionToFishPaused)
                        __instance.CancelInvoke("EnableAggressionToFish");
                    if (__instance.aggressionToSmallFish != null)
                        __instance.aggressionToSmallFish.enabled = false;
                    if (__instance.aggressionToMediumFish != null)
                        __instance.aggressionToMediumFish.enabled = false;
                    __instance.aggressionToFishPaused = true;
                    __instance.Invoke("EnableAggressionToFish", __instance.fishAttackInterval);
                }
                if (__instance.sharksAttackInterval <= 0f|| behaviourType != BehaviourType.Shark)
                    return false;

                if (__instance.aggressionToSharksPaused)
                    __instance.CancelInvoke("EnableAggressionToSharks");
                if (__instance.aggressionToSharks != null)
                    __instance.aggressionToSharks.enabled = false;
                __instance.aggressionToSharksPaused = true;
                __instance.Invoke("EnableAggressionToSharks", __instance.sharksAttackInterval / Main.config.aggrMult);
                //AddDebug("CreatureAggressionManager EnableAggressionToSharks");
                return false;
            }
        }

        [HarmonyPatch(typeof(AggressiveWhenSeePlayer), "GetAggressionTarget")]
        internal class AggressiveWhenSeePlayer_GetAggressionTarget_Patch
        {
            public static bool Prefix(AggressiveWhenSeePlayer __instance, ref GameObject __result)
            {
                if (Main.config.aggrMult > 0f && Time.time > __instance.timeLastPlayerAttack + __instance.playerAttackInterval / Main.config.aggrMult && __instance.IsTargetValid(Player.main.gameObject))
                {
                    //AddDebug("AggressiveWhenSeePlayer " + __instance.name + " " + __instance.playerAttackInterval);
                    __result = Player.main.gameObject;
                    return false;
                }
                //AggressiveWhenSeeTarget awst = __instance;
                __result = __instance.targetType != EcoTargetType.None ? __instance.GetAggressionTarget() : null;
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "CanBeAttacked")]
        internal class Player_CanBeAttacked_Patch
        {
            public static bool Prefix(Player __instance, ref bool __result)
            {
                //AddDebug("AggressiveWhenSeeTarget start " + __instance.myTechType + " " + __instance.maxSearchRings);
                //__result = !__instance.IsInsideWalkable() && !__instance.justSpawned && !GameModeUtils.IsInvisible() && !Player.main.precursorOutOfWater && !PrecursorMoonPoolTrigger.inMoonpool;
                __result = !__instance.IsInsideWalkable() && !__instance.justSpawned && !GameModeUtils.IsInvisible() && Main.config.aggrMult > 0f;
                return false;
            }
        }

        [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
        internal class AggressiveWhenSeeTarget_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetAggressionTarget")]
            public static bool GetAggressionTargetPrefix(AggressiveWhenSeeTarget __instance, ref GameObject __result)
            {
                if (__instance.targetType != EcoTargetType.Shark || Main.config.aggrMult <= 1 || Main.config.predatorExclusion.Contains(__instance.myTechType))
                    return true;

                int searchRings = Mathf.RoundToInt(__instance.maxSearchRings * Main.config.aggrMult);
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
                if (__instance.targetType != EcoTargetType.Shark || Main.config.predatorExclusion.Contains(__instance.myTechType))
                    return true;

                if (target == Player.main.gameObject)
                {
                    //AddDebug(__instance.myTechType + " Player");
                    if (!Player.main.CanBeAttacked() || PrecursorMoonPoolTrigger.inMoonpool || GameModeUtils.IsInvisible())
                    {
                        __result = false;
                        return false;
                    }
                    //if (CreatureData.GetBehaviourType(__instance.myTechType) == BehaviourType.Leviathan)
                    { // prevent leviathan attacking player on land
                        //if (Player.main.depthLevel > -5f)
                        //{
                            //AddDebug(" prevent leviathan attacking player on land");
                        //    __result = false;
                        //    return false;
                        //}
                    }
                }
                //Vehicle vehicle = target.GetComponent<Vehicle>();
                //SeaTruckSegment sts = target.GetComponent<SeaTruckSegment>();
                if (IsVehicle(target))
                {
                    if (Main.config.aggrMult == 0 || GameModeUtils.IsInvisible())
                    {
                        __result = false;
                        return false;
                    }
                    if (!IsPlayerInsideVehicle(target))
                    {
                        if (Main.config.emptyVehicleCanBeAttacked == Config.EmptyVehicleCanBeAttacked.Only_if_lights_on && !IsLightOn(target))
                        {
                            __result = false;
                            return false;
                        }
                        else if (Main.config.emptyVehicleCanBeAttacked == Config.EmptyVehicleCanBeAttacked.No)
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
                float leashDistance = __instance.leashDistance * Main.config.aggrMult;
                //float aggrMult = Main.config.aggrMult > 1 ? Main.config.aggrMult : 1;
                if (dist > __instance.maxRangeScalar * Main.config.aggrMult || leashDistance > 0f && (__instance.creature.GetLeashPosition() - target.transform.position).sqrMagnitude > leashDistance * leashDistance)
                {
                    __result = false;
                    return false;
                }
                if (!Mathf.Approximately(__instance.minimumVelocity, 0f))
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

                if (__instance.targetType != EcoTargetType.Shark || Main.config.predatorExclusion.Contains(__instance.myTechType))
                    return true;

                if (Main.config.aggrMult <= 1 && __instance.creature && __instance.creature.Hunger.Value < __instance.hungerThreshold)
                    return true;

                if (Main.config.aggrMult == 3f && Player.main.CanBeAttacked() && __instance.creature.GetCanSeeObject(Player.main.gameObject))
                {
                    __instance.creature.Aggression.Add(__instance.aggressionPerSecond);
                    //__instance.lastScarePosition.lastScarePosition = Player.main.gameObject.transform.position;
                    __instance.lastTarget.SetTarget(Player.main.gameObject, __instance.targetPriority);
                    if (__instance.sightedSound != null && !__instance.sightedSound.GetIsPlaying() && !Creature_Tweaks.silentCreatures.Contains(__instance.myTechType))
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
                    GameObject target = __instance.lastTarget.target;
                    if (target != null && target != aggressionTarget && (__instance.lastTarget.targetPriority > __instance.targetPriority && Time.time <= __instance.lastTarget.targetTime + 5f))
                        return false;
                    __instance.creature.Aggression.Add(( mult));
                    __instance.lastTarget.SetTarget(aggressionTarget, __instance.targetPriority);
                    //__instance.lastScarePosition.lastScarePosition = aggressionTarget.transform.position;

                    if (__instance.sightedSound != null && !__instance.sightedSound.GetIsPlaying() && !Creature_Tweaks.silentCreatures.Contains(__instance.myTechType))
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
                    if (!Player.main.CanBeAttacked() || GameModeUtils.IsInvisible() || Main.config.aggrMult == 0f)
                    {
                        __result = false;
                        return false;
                    }
                }
                Vehicle vehicle = target.GetComponent<Vehicle>();
                SeaTruckSegment sts = target.GetComponent<SeaTruckSegment>();
                if (vehicle || sts)
                {
                    if (Main.config.aggrMult == 0 || GameModeUtils.IsInvisible())
                    {
                        __result = false;
                        return false;
                    }
                    bool playerInSeaTruck = Player.main._currentInterior != null && Player.main._currentInterior is SeaTruckSegment;
                    if ((vehicle && !Player.main.inExosuit) || (sts && !playerInSeaTruck))
                    {
                        if (Main.config.emptyVehicleCanBeAttacked == Config.EmptyVehicleCanBeAttacked.Only_if_lights_on)
                        {
                            PowerRelay pr = null;
                            if (vehicle)
                                pr = vehicle.GetComponent<PowerRelay>();
                            else if (sts) // seatruck has lights when unpowered
                                pr = sts.GetComponent<PowerRelay>();

                            __result = pr && pr.isPowered;
                            return false;
                        }
                        __result = Main.config.emptyVehicleCanBeAttacked == Config.EmptyVehicleCanBeAttacked.Yes;
                        return false;
                    }
                    Vector3 vel = Vector3.zero;
                    if (vehicle)
                        vel = vehicle.useRigidbody.velocity;
                    else if (sts)
                        vel = sts.GetComponent<Rigidbody>().velocity;

                    __result = vel.x > 1f || vel.y > 1f || vel.z > 1f;
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
                if (Main.config.aggrMult == 1 || type != EcoTargetType.Shark)
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
                    if (ecoTarget != null && !ecoTarget.Equals(null))
                    {
                        float sqrMagnitude = (wsPos - ecoTarget.GetPosition()).sqrMagnitude;
                        //if (agr > 1f)
                        //{
                        bool player = ecoTarget.GetGameObject() == Player.main.gameObject;
                        //bool vehicle = ecoTarget.GetGameObject().GetComponent<Vehicle>();
                        if (player)
                            sqrMagnitude /= Main.config.aggrMult;
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
                    float aggr = Main.config.aggrMult > 1f ? Main.config.aggrMult : 1f;
                    if (EcoRegionManager.main != null && (Mathf.Approximately(__instance.requiredAggression, 0f) || __instance.creature.Aggression.Value * aggr >= __instance.requiredAggression))
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
                if (Main.config.aggrMult == 0f || Main.config.predatorExclusion.Contains(CraftData.GetTechType(__instance.gameObject)))
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
                    Main.Log(techType + " eyeFOV " + __instance.eyeFOV);
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
                Player main = Player.main;
                if (main == null || main.GetMode() != Player.Mode.LockedPiloting)
                    return false;
                if (Main.config.aggrMult == 0)
                    return false;
                Vehicle vehicle = main.GetVehicle();
                if (vehicle == null || Vector3.Distance(vehicle.transform.position, __instance.transform.position) > __instance.range * Main.config.aggrMult)
                    return false;
                TechType myTT = CraftData.GetTechType(__instance.gameObject);
                if (Main.config.predatorExclusion.Contains(myTT))
                    return false;
                //__instance.creature.GetCanSeeObject(Player.main.gameObject))
                __instance.lastTarget.SetTarget(vehicle.gameObject, __instance.targetPriority);
                __instance.creature.Aggression.Add(__instance.aggressionPerSecond * __instance.updateAggressionInterval * Main.config.aggrMult);
                //AddDebug(" AggressiveToPilotingVehicle range " + __instance.range);
                //AddDebug(" prevVelocity " + vehicle.prevVelocity);
                return false;
            }
        }


    }
}

