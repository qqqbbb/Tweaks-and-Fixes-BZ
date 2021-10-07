using HarmonyLib;
using System;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Damage_Patch
    {
        static public Dictionary<TechType, float> damageMult = new Dictionary<TechType, float>();

        static void SetBloodColor(GameObject go)
        {   // GenericCreatureHit(Clone)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(0.784, 1.000, 0.157, 0.588)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(1.000, 0.925, 0.333, 1.000)
            // xKnifeHit_Organic(Clone)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
            //AddDebug("SetBloodColor " + go.name + " " + pss.Length);
            //Main.Log("SetBloodColor " + go.name );
            foreach (ParticleSystem ps in pss)
            {
                //ps.startColor = new Color(1f, 0f, 0f);
                ParticleSystem.MainModule psMain = ps.main;
                //Main.Log("startColor " + psMain.startColor.color);
                Color newColor = new Color(Main.config.bloodColor["Red"], Main.config.bloodColor["Green"], Main.config.bloodColor["Blue"], psMain.startColor.color.a);
                psMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
            }
        }

        [HarmonyPatch(typeof(VFXDestroyAfterSeconds), "OnEnable")]
        class VFXDestroyAfterSeconds_OnEnable_Patch
        {
            public static void Prefix(VFXDestroyAfterSeconds __instance)
            {// particles from GenericCreatureHit play on awake
                //AddDebug("GenericCreatureHit OnEnable " + __instance.gameObject.name);
                if (__instance.gameObject.name == "GenericCreatureHit(Clone)")
                {
                    //AddDebug("GenericCreatureHit OnEnable");
                    //setBloodColor = true;
                    SetBloodColor(__instance.gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(VFXSurfaceTypeManager), "Play", new Type[] { typeof(VFXSurfaceTypes), typeof(VFXEventTypes), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
        class VFXSurfaceTypeManager_Play_Patch
        { // blood color
            static bool Prefix(VFXSurfaceTypeManager __instance, ref ParticleSystem __result, VFXSurfaceTypes surfaceType, VFXEventTypes eventType, Vector3 position, Quaternion orientation, Transform parent)
            {
                //ProfilingUtils.BeginSample("VFXSurfaceTypeManager.Play");
                ParticleSystem particleSystem = null;
                GameObject fxprefab = __instance.GetFXprefab(surfaceType, eventType);
                if (fxprefab != null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(fxprefab, position, orientation);
                    if (eventType == VFXEventTypes.exoDrill)
                    {
                        gameObject.transform.parent = null;
                        gameObject.GetComponent<VFXFakeParent>().Parent(parent, Vector3.zero, Vector3.zero);
                        gameObject.GetComponent<VFXLateTimeParticles>().Play();
                        particleSystem = gameObject.GetComponent<ParticleSystem>();
                    }
                    else
                    {
                        gameObject.transform.parent = parent;

                        if (surfaceType == VFXSurfaceTypes.organic)
                        {
                            //AddDebug("VFXSurfaceTypeManager Play " + parent.name);
                            SetBloodColor(gameObject);
                        }
  
                        particleSystem = gameObject.GetComponent<ParticleSystem>();
                        particleSystem.Play();
                    }
                }
                //particleSystem.startColor = new Color(1f, 1f, 1f);
                //ProfilingUtils.EndSample();
                __result = particleSystem;
                return false;
            }
        }

        [HarmonyPatch(typeof(DealDamageOnImpact), "OnCollisionEnter")]
        class DealDamageOnImpact_OnCollisionEnter_Patch
        { // seatruck mirroredSelfDamageFraction .12
            static Rigidbody prevColTarget;
            static bool Prefix(DealDamageOnImpact __instance, Collision collision)
            {
                if (!__instance.enabled || collision.contacts.Length == 0 || __instance.exceptions.Contains(collision.gameObject))
                    return false;
                float damageMult = Mathf.Max(0f, Vector3.Dot(-collision.contacts[0].normal, __instance.prevVelocity));
                float colMag = collision.relativeVelocity.magnitude;
                if (colMag < __instance.speedMinimumForDamage)
                    return false;
                if (__instance.impactSound && __instance.timeLastImpactSound + .5f < Time.time)
                {
                    //AddDebug("minDamageInterval " + __instance.minDamageInterval);
                    //AddDebug("damageMult " + damageMult);
                    if (__instance.checkForFishHitSound)
                    {
                        bool smallFish = false;
                        GameObject gameObject = collision.gameObject;
                        GameObject entityRoot = UWE.Utils.GetEntityRoot(collision.gameObject);
                        if (entityRoot != null)
                            gameObject = entityRoot;
                        if (gameObject.GetComponent<Creature>() != null)
                        {
                            BehaviourType behaviourType = CreatureData.GetBehaviourType(gameObject);
                            smallFish = behaviourType == BehaviourType.SmallFish || behaviourType == BehaviourType.MediumFish;
                        }
                        __instance.impactSound.SetParameterValue(__instance.hitFishParamIndex, smallFish ? 1f : 0f);
                    }
                    __instance.impactSound.SetParameterValue(__instance.velocityParamIndex, damageMult);
                    __instance.impactSound.Play();
                    __instance.timeLastImpactSound = Time.time;
                }
                if (!__instance.allowDamageToPlayer)
                {
                    GameObject gameObject = collision.gameObject;
                    GameObject entityRoot = UWE.Utils.GetEntityRoot(collision.gameObject);
                    if (entityRoot)
                        gameObject = entityRoot;
                    if (gameObject.Equals(Player.main.gameObject))
                        return false;
                }
                if (!__instance.damageBases && UWE.Utils.GetComponentInHierarchy<Base>(collision.gameObject))
                    return false;
                LiveMixin targetLM = __instance.GetLiveMixin(collision.contacts[0].otherCollider.gameObject);
                Vector3 position = collision.contacts[0].point;
                Rigidbody rb = Utils.FindAncestorWithComponent<Rigidbody>(collision.gameObject);
                float targetMass = rb != null ? rb.mass : 5000f;
                //AddDebug(" targetMass " + targetMass);
                float myMass = __instance.GetComponent<Rigidbody>().mass;
                float colMult = Mathf.Clamp((1f + (myMass - targetMass) * 0.001f), 0f, damageMult);
                float targetDamage = colMag * colMult;
                if (targetLM && targetLM.IsAlive() && Time.time > __instance.timeLastDamage + __instance.minDamageInterval)
                {
                    bool skip = false;
                    if (prevColTarget == rb && Time.time < __instance.timeLastDamage + 3f)
                        skip = true;
                    if (!skip)
                    {
                        //AddDebug(" myMass " + myMass);
                        //AddDebug(targetLM.name + " damage " + targetDamage);
                        targetLM.TakeDamage(targetDamage, position, DamageType.Collide, __instance.gameObject);
                        __instance.timeLastDamage = Time.time;
                        prevColTarget = rb;
                    }
                }
                if (!__instance.mirroredSelfDamage || colMag < __instance.speedMinimumForSelfDamage)
                    return false;

                LiveMixin myLM = __instance.GetLiveMixin(__instance.gameObject);
                bool tooSmall = rb && rb.mass <= __instance.minimumMassForDamage;
                if (__instance.mirroredSelfDamageFraction == 0f || !myLM || Time.time <= __instance.timeLastDamagedSelf + 1f || tooSmall)
                    return false;
                //float num3 = targetDamage * __instance.mirroredSelfDamageFraction;
                float myDamage = colMag * Mathf.Clamp((1f + (targetMass - myMass) * 0.001f), 0f, damageMult);
                if (__instance.capMirrorDamage != -1.0)
                    myDamage = Mathf.Min(__instance.capMirrorDamage, myDamage);
                myLM.TakeDamage(myDamage, position, DamageType.Collide, __instance.gameObject);
                __instance.timeLastDamagedSelf = Time.time;
                //AddDebug(__instance.name + " self damage " + myDamage);
                return false;
            }
        }

        //[HarmonyPatch(typeof(LiveMixin), "Kill")]
        class LiveMixin_Kill_Patch
        {
            static void Postfix(LiveMixin __instance, DamageType damageType)
            {
                //ProfilingUtils.BeginSample("LiveMixin.Kill");
                __instance.health = 0.0f;
                __instance.tempDamage = 0.0f;
                __instance.SyncUpdatingState();
                if (__instance.deathClip)
                    Utils.PlayEnvSound(__instance.deathClip, __instance.transform.position, 25f);
                if (__instance.deathEffect != null)
                {
                    GameObject go = UWE.Utils.InstantiateWrap(__instance.deathEffect, __instance.transform.position, Quaternion.identity);
                }

                if (__instance.passDamageDataOnDeath)
                    __instance.gameObject.BroadcastMessage("OnKill", damageType, SendMessageOptions.DontRequireReceiver);
                else if (__instance.broadcastKillOnDeath)
                    __instance.gameObject.BroadcastMessage("OnKill", SendMessageOptions.DontRequireReceiver);
                if (__instance.destroyOnDeath)
                {
                    //if (__instance.explodeOnDestroy)
                    //{
                    //    Living component = __instance.gameObject.GetComponent<Living>();
                    //    if (component)
                    //        component.enabled = false;
                    //    ExploderObject.ExplodeGameObject(__instance.gameObject);
                    //}
                    //else
                    {
                        __instance.CleanUp();
                        UWE.Utils.DestroyWrap(__instance.gameObject);
                    }
                }
                //ProfilingUtils.EndSample();
            }
        }

        //[HarmonyPatch(typeof(Knife), "OnToolUseAnim")]
        class Knife_OnToolUseAnim_Patch
        {
            public static bool Prefix(Knife __instance, GUIHand hand)
            {
                Vector3 position = new Vector3();
                GameObject closestObj = null;
                Vector3 normal;
                UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref closestObj, ref position, out normal);
                if (closestObj == null)
                {
                    InteractionVolumeUser ivu = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                    if (ivu != null && ivu.GetMostRecent() != null)
                        closestObj = ivu.GetMostRecent().gameObject;
                }
                if (closestObj)
                {
                    LiveMixin liveMixin = closestObj.FindAncestor<LiveMixin>();
                    //if (liveMixin && Knife.IsValidTarget(liveMixin))
                    {
                        if (liveMixin && Knife.IsValidTarget(liveMixin))
                        {
                            bool wasAlive = liveMixin.IsAlive();
                            liveMixin.TakeDamage(__instance.damage, position, __instance.damageType, Player.main.gameObject);
                            __instance.GiveResourceOnDamage(closestObj, liveMixin.IsAlive(), wasAlive);
                        }
                        //Utils.PlayFMODAsset(__instance.attackSound, __instance.transform);
                        VFXSurface vfxSurface = closestObj.GetComponent<VFXSurface>();
                        Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                        ParticleSystem particleSystem = VFXSurfaceTypeManager.main.Play(vfxSurface, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);
                        VFXSurfaceTypes vfxSurfaceTypes = Utils.GetObjectSurfaceType(closestObj);
                        if (vfxSurfaceTypes == VFXSurfaceTypes.none)
                            vfxSurfaceTypes = Utils.GetTerrainSurfaceType(position, normal, VFXSurfaceTypes.sand);
                        EventInstance fmodEvent = Utils.GetFMODEvent(__instance.hitSound, __instance.transform.position);
                        int num1 = (int)fmodEvent.setParameterValueByIndex(__instance.surfaceParamIndex, (float)vfxSurfaceTypes);
                        int num2 = (int)fmodEvent.start();
                        int num3 = (int)fmodEvent.release();
                    }
                    //else
                    //    closestObj = null;
                }
                //if (closestObj != null || hand.GetActiveTarget() != null)
                //    return false;
                Utils.PlayFMODAsset(Player.main.IsUnderwater() ? __instance.swingWaterSound : __instance.swingSound, __instance.transform.position);

                return false;
            }
        }

        [HarmonyPatch(typeof(LiveMixin))]
        class LiveMixin_Start_Patch
        {
            [HarmonyPatch(nameof(LiveMixin.Start))]
            [HarmonyPostfix]
            static void Postfix(LiveMixin __instance)
            {
                //if (__instance.data.deathEffect)
                //{
                //    Main.Log("deathEffect " + __instance.data.deathEffect);
                //}
                if (Main.config.noKillParticles)
                {
                    //__instance.data.damageEffect = null;
                    __instance.data.deathEffect = null;
                }
            }
            [HarmonyPatch(nameof(LiveMixin.TakeDamage))]
            [HarmonyPrefix]
            static bool Prefix(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position = default(Vector3), DamageType type = DamageType.Normal, GameObject dealer = null)
            {
                //if (dealer)
                //    AddDebug("dealer " + dealer.name);
                //ProfilingUtils.BeginSample("LiveMixin.TakeDamage");
                bool killed = false;
                bool creativeMode = GameModeUtils.IsInvisible() && __instance.invincibleInCreative;
                if (__instance.health > 0f && !__instance.invincible && !creativeMode)
                {
                    float damage = 0f;
                    if (!__instance.shielded)
                        damage = DamageSystem.CalculateDamage(originalDamage, type, __instance.gameObject, dealer);

                    __instance.health = Mathf.Max(0f, __instance.health - damage);
                    if (type == DamageType.Cold || type == DamageType.Poison)
                    {
                        __instance.tempDamage += damage;
                        __instance.SyncUpdatingState();
                    }
                    __instance.damageInfo.Clear();
                    __instance.damageInfo.originalDamage = originalDamage;
                    __instance.damageInfo.damage = damage;
                    __instance.damageInfo.position = position == new Vector3() ? __instance.transform.position : position;
                    __instance.damageInfo.type = type;
                    __instance.damageInfo.dealer = dealer;
                    __instance.NotifyAllAttachedDamageReceivers(__instance.damageInfo);
                    if (__instance.shielded)
                    {
                        __result = killed;
                        return false;
                    }

                    if (__instance.damageClip && damage > 0f && (damage >= __instance.minDamageForSound && type != DamageType.Radiation))
                    {
                        //ProfilingUtils.BeginSample("LiveMixin.TakeDamage.PlaySound");
                        Utils.PlayEnvSound(__instance.damageClip, __instance.damageInfo.position);
                        //ProfilingUtils.EndSample();
                    }
                    if (__instance.loopingDamageEffect && !__instance.loopingDamageEffectObj && __instance.GetHealthFraction() < __instance.loopEffectBelowPercent)
                    {
                        //__instance.loopingDamageEffectObj = UWE.Utils.InstantiateWrap(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj = UnityEngine.Object.Instantiate<GameObject>(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj.transform.parent = __instance.transform;
                    }
                    if (type == DamageType.Electrical && Time.time > __instance.timeLastElecDamageEffect + 2.5f && __instance.electricalDamageEffect != null)
                    {
                        FixedBounds fixedBounds = __instance.gameObject.GetComponent<FixedBounds>();
                        Bounds bounds = fixedBounds == null ? UWE.Utils.GetEncapsulatedAABB(__instance.gameObject) : fixedBounds.bounds;
                        //GameObject electricalDamageEffect = UWE.Utils.InstantiateWrap(__instance.electricalDamageEffect, bounds.center, Quaternion.identity);
                        GameObject electricalDamageEffect = UnityEngine.Object.Instantiate<GameObject>(__instance.electricalDamageEffect, bounds.center, Quaternion.identity);
                        electricalDamageEffect.transform.parent = __instance.transform;
                        electricalDamageEffect.transform.localScale = bounds.size * 0.65f;
                        __instance.timeLastElecDamageEffect = Time.time;
                    }
                    else if (dealer != Player.main.gameObject && Time.time > __instance.timeLastDamageEffect + 1f && damage > 0f && type == DamageType.Normal || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture || type == DamageType.LaserCutter || type == DamageType.Drill)
                    { // dont spawn damage particles if knifed by player
                        VFXSurface vfxSurface = __instance.GetComponentInChildren<VFXSurface>();
                        if (vfxSurface)
                        {
                            //AddDebug("Spawn vfxSurface Prefab ");
                            //Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                            //setBloodColor = true;
                            ParticleSystem particleSystem = VFXSurfaceTypeManager.main.Play(vfxSurface, VFXEventTypes.knife, position, Quaternion.identity, Player.main.transform);
                            __instance.timeLastDamageEffect = Time.time;
                        }
                        else if (__instance.damageEffect)
                        {
                            //AddDebug("Spawn damageEffect Prefab " + __instance.damageEffect.name);
                            GameObject go = Utils.SpawnPrefabAt(__instance.damageEffect, __instance.transform, __instance.damageInfo.position);
                            //setBloodColor = true;
                            if (__instance.GetComponent<Creature>())
                            {
                                //AddDebug("take damage " + __instance.name);
                                SetBloodColor(go);
                            }
                            __instance.timeLastDamageEffect = Time.time;
                        }
                    }
                    if (__instance.health <= 0f || __instance.health - __instance.tempDamage <= 0f)
                    {
                        killed = true;
                        if (__instance.IsCinematicActive())
                        {
                            __instance.cinematicModeActive = true;
                            __instance.SyncUpdatingState();
                        }
                        else
                            __instance.Kill(type);
                    }
                }
                __result = killed;
                return false;
            }
        }

        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                if (__result > 0f)
                {
                    if (target == Player.mainObject)
                    {
                        __result *= Main.config.playerDamageMult;
                        //AddDebug("Player takes damage " + __result);
                        if (__result == 0f)
                            return;

                        if (Main.config.dropHeldTool)
                        {
                            if (type != DamageType.Cold && type != DamageType.Poison && type != DamageType.Starve && type != DamageType.Radiation && type != DamageType.Pressure)
                            {
                                int rnd = Main.rndm.Next(1, (int)Player.main.liveMixin.maxHealth);
                                if (rnd < damage)
                                {
                                    //AddDebug("DropHeldItem");
                                    Inventory.main.DropHeldItem(true);
                                }
                            }
                        }
                    }
                    else if (target.GetComponent<Vehicle>() || target.GetComponent<SeaTruckSegment>() || target.GetComponent<Hoverbike>())
                    {
                        //AddDebug("Vehicle takes damage"); 
                        __result *= Main.config.vehicleDamageMult;
                    }
                    //else if (target.GetComponent<BaseCell>())
                    //{
                    //    AddDebug("base takes damage");
                    //}
                    else
                    {
                        TechType tt = CraftData.GetTechType(target);
                        if (damageMult.ContainsKey(tt))
                            __result *= damageMult[tt];
                    }
                }

            }
        }
    }
}
