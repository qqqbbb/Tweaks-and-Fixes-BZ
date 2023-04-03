using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Damage_Patch
    {
        static public Dictionary<TechType, float> damageMult = new Dictionary<TechType, float>();
        static HashSet<DealDamageOnImpact> hoverBikes = new HashSet<DealDamageOnImpact>();
        static HashSet<DealDamageOnImpact> ddoiVanillaScript = new HashSet<DealDamageOnImpact>();

        static void SetBloodColor(GameObject go)
        {  
            ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
            //AddDebug("SetBloodColor " + go.name + " " + pss.Length);
            //Main.Log("SetBloodColor " + go.name );
            foreach (ParticleSystem ps in pss)
            {
                //ps.startColor = new Color(1f, 0f, 0f);
                ParticleSystem.MainModule psMain = ps.main;
                //Main.Log("startColor " + psMain.startColor.color);
                //AddDebug("startColor " + psMain.startColor.color);
                Color newColor = new Color(Main.config.bloodColor["Red"], Main.config.bloodColor["Green"], Main.config.bloodColor["Blue"], psMain.startColor.color.a);
                //newColor = Color.blue;
                //Main.Log("blood Color " + newColor);
                //AddDebug("blood Color " + newColor);
                psMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
                //psMain.startSizeMultiplier *= .1f;
            }
        }

        [HarmonyPatch(typeof(DealDamageOnImpact))]
        class DealDamageOnImpact_Patch
        { // seatruck mirroredSelfDamageFraction .12 hoverbike mirroredSelfDamageFraction 1
            static Rigidbody prevColTarget;
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(DealDamageOnImpact __instance)
            {
                //AddDebug(" DealDamageOnImpact Start " + __instance.name);
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //if (tt == TechType.SeaTruck)
                //    __instance.mirroredSelfDamageFraction = 1f;
                if (__instance.GetComponent<Hoverbike>())
                    hoverBikes.Add(__instance);
                    //__instance.mirroredSelfDamageFraction = .25f;
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnCollisionEnter")]
            static bool OnCollisionEnterPrefix(DealDamageOnImpact __instance, Collision collision)
            {
                if (!__instance.enabled || collision.contacts.Length == 0 || __instance.exceptions.Contains(collision.gameObject))
                    return false;

                if (hoverBikes.Contains(__instance) || ddoiVanillaScript.Contains(__instance))
                    return true;

                ddoiVanillaScript.Add(__instance);
                float damageMax = Mathf.Max(0f, Vector3.Dot(-collision.contacts[0].normal, __instance.prevVelocity));
                float colMag = collision.relativeVelocity.magnitude;
                //AddDebug(" colMag " + colMag);
                //AddDebug(" speedMinimumForDamage " + __instance.speedMinimumForDamage);
                if (colMag < __instance.speedMinimumForDamage)
                    return OnCollisionEnterFinished(__instance);

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
                    __instance.impactSound.SetParameterValue(__instance.velocityParamIndex, damageMax);
                    __instance.impactSound.Play();
                    __instance.timeLastImpactSound = Time.time;
                }
                if (!__instance.allowDamageToPlayer)
                {
                    GameObject colTarget = collision.gameObject;
                    GameObject entityRoot = UWE.Utils.GetEntityRoot(collision.gameObject);
                    if (entityRoot)
                        colTarget = entityRoot;

                    if (colTarget.Equals(Player.main.gameObject))
                        return OnCollisionEnterFinished(__instance);
                }
                if (!__instance.damageBases && UWE.Utils.GetComponentInHierarchy<Base>(collision.gameObject))
                    return OnCollisionEnterFinished(__instance);

                LiveMixin targetLM = __instance.GetLiveMixin(collision.contacts[0].otherCollider.gameObject);
                Vector3 position = collision.contacts[0].point;
                Rigidbody rb = Utils.FindAncestorWithComponent<Rigidbody>(collision.gameObject);
                float targetMass = rb != null ? rb.mass : 5000f;
                //AddDebug(" targetMass " + targetMass);
                float myMass = __instance.GetComponent<Rigidbody>().mass;
                float colMult = Mathf.Clamp((1f + (myMass - targetMass) * 0.001f), 0f, damageMax);
                float targetDamage = colMag * colMult;

                if (targetLM && targetLM.IsAlive() && Time.time > __instance.timeLastDamage + __instance.minDamageInterval)
                {
                    bool skip = false;
                    if (prevColTarget && rb && prevColTarget.Equals(rb) && Time.time < __instance.timeLastDamage + 3f)
                        skip = true;

                    if (!skip)
                    {
                        //AddDebug(" myMass " + myMass);
                        //AddDebug(targetLM.name + " health " + targetLM.health + " damage " + targetDamage);
                        targetLM.TakeDamage(targetDamage, position, DamageType.Collide, __instance.gameObject);
                        __instance.timeLastDamage = Time.time;
                        prevColTarget = rb;
                    }
                }
                if (!__instance.mirroredSelfDamage || colMag < __instance.speedMinimumForSelfDamage)
                    return OnCollisionEnterFinished(__instance);

                LiveMixin myLM = __instance.GetLiveMixin(__instance.gameObject);
                bool tooSmall = rb && rb.mass <= __instance.minimumMassForDamage;
                if (__instance.mirroredSelfDamageFraction == 0f || !myLM || Time.time <= __instance.timeLastDamagedSelf + 1f || tooSmall)
                    return OnCollisionEnterFinished(__instance);

                //float num3 = targetDamage * __instance.mirroredSelfDamageFraction;
                float myDamage = colMag * Mathf.Clamp((1f + (targetMass - myMass) * 0.001f), 0f, damageMax);
                if (__instance.capMirrorDamage != -1f)
                    myDamage = Mathf.Min(__instance.capMirrorDamage, myDamage);
                myLM.TakeDamage(myDamage, position, DamageType.Collide, __instance.gameObject);
                __instance.timeLastDamagedSelf = Time.time;
                //AddDebug(__instance.name + " speedMinimumForDamage " + __instance.speedMinimumForDamage + " self damage " + myDamage);
                ddoiVanillaScript.Remove(__instance);
                return false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnCollisionEnter")]
            static bool OnCollisionEnterFinished(DealDamageOnImpact dealDamageOnImpact)
            {
                ddoiVanillaScript.Remove(dealDamageOnImpact);
                return false;
            }
        }

        [HarmonyPatch(typeof(LiveMixin))]
        class LiveMixin_Start_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(LiveMixin __instance)
            {
                //    Main.Log("deathEffect " + __instance.name);
                //    AddDebug("deathEffect " + __instance.name);
                if (__instance.data.damageEffect || __instance.data.deathEffect)
                {
                    VFXSurface surface = __instance.GetComponent<VFXSurface>();
                    if (surface && surface.surfaceType == VFXSurfaceTypes.organic)
                    {
                        if (__instance.data.damageEffect )
                            SetBloodColor(__instance.data.damageEffect);

                        if (__instance.data.deathEffect)
                            SetBloodColor(__instance.data.deathEffect);
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("TakeDamage")]
            static bool TakeDamagePrefix(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position = default(Vector3), DamageType type = DamageType.Normal, GameObject dealer = null)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                bool isBaseCell = __instance.GetComponent<BaseCell>() != null;
                float damageTakenModifier = GameModeManager.GetDamageTakenModifier(techType, isBaseCell);
                bool killed = false;
                bool flag2 = damageTakenModifier == 0f;
                if (__instance.health > 0f && !__instance.invincible && !flag2)
                {
                    float damage = 0f;
                    if (!__instance.shielded)
                        damage = DamageSystem.CalculateDamage(techType, damageTakenModifier, originalDamage, type, __instance.gameObject, dealer);
                    __instance.health = Mathf.Max(0f, __instance.health - damage);
                    if (type == DamageType.Poison)
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
                        return killed;
                    //Main.config.crushDamageMoaning = false;
                    //bool skipCrushDamageSound = !Main.config.crushDamageMoaning && type == DamageType.Pressure;
                    //AddDebug("skipCrushDamageSound " + skipCrushDamageSound);
                    if (damage > 0f && damage >= __instance.minDamageForSound && type != DamageType.Radiation)
                    {
                        //AddDebug("DamageSound " );
                        if (__instance.damageClip)
                            __instance.damageClip.Play();

                        if (__instance.damageSound)
                            Utils.PlayFMODAsset(__instance.damageSound, __instance.damageInfo.position);
                    }
                    if (__instance.loopingDamageEffect && !__instance.loopingDamageEffectObj && __instance.GetHealthFraction() < __instance.loopEffectBelowPercent)
                    {
                        __instance.loopingDamageEffectObj = UWE.Utils.InstantiateWrap(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj.transform.parent = __instance.transform;
                    }
                    if (type == DamageType.Electrical && Time.time > __instance.timeLastElecDamageEffect + 2.5f && __instance.electricalDamageEffect != null)
                    {
                        FixedBounds fixedBounds = __instance.gameObject.GetComponent<FixedBounds>();
                        Bounds bounds = !(fixedBounds != null) ? UWE.Utils.GetEncapsulatedAABB(__instance.gameObject) : fixedBounds.bounds;
                        GameObject gameObject = UWE.Utils.InstantiateWrap(__instance.electricalDamageEffect, bounds.center, Quaternion.identity);
                        gameObject.transform.parent = __instance.transform;
                        gameObject.transform.localScale = bounds.size * 0.65f;
                        __instance.timeLastElecDamageEffect = Time.time;
                    }
                    else if (Time.time > __instance.timeLastDamageEffect + 1f && damage > 0f && __instance.damageEffect != null && (type == DamageType.Normal || type == DamageType.Collide || (type == DamageType.Explosive || type == DamageType.Puncture) || (type == DamageType.LaserCutter || type == DamageType.Drill)))
                    {
                        //AddDebug("TakeDamage damageEffect");
                        Utils.SpawnPrefabAt(__instance.damageEffect, __instance.transform, __instance.damageInfo.position);
                        __instance.timeLastDamageEffect = Time.time;
                    }
                    if (__instance.health <= 0f || __instance.health - __instance.tempDamage <= 0f)
                    {
                        killed = true;
                        if (!__instance.IsCinematicActive())
                            __instance.Kill(type);
                        else
                        {
                            __instance.cinematicModeActive = true;
                            __instance.SyncUpdatingState();
                        }
                    }
                }
                __result = killed;
                return false;
            }
        }

        [HarmonyPatch(typeof(DamageFX), "AddHudDamage")]
        class DamageFX_AddHudDamage_Patch
        {
            public static bool Prefix(DamageFX __instance, float damageScalar, Vector3 damageSource, DamageInfo damageInfo, bool isUnderwater)
            {
                //Main.config.crushDamageScreenEffect = false;
                //AddDebug("AddHudDamage " + damageInfo.type);
                if (!Main.config.crushDamageScreenEffect && damageInfo.type == DamageType.Pressure)
                    return false;

                if (Main.config.damageImpactEffect)
                    __instance.CreateImpactEffect(damageScalar, damageSource, damageInfo.type, isUnderwater);

                if (Main.config.damageScreenFX)
                    __instance.PlayScreenFX(damageInfo);

                return false;
            }
        }

        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage", new Type[] { typeof(TechType), typeof(float), typeof(float), typeof(DamageType), typeof(GameObject), typeof(GameObject)})]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result, TechType techType)
            {
                if (__result <= 0f)
                    return;

                if (techType == TechType.Player)
                {
                    //__result *= Main.config.playerDamageMult;
                    //AddDebug("Player takes damage " + __result);
                    //if (__result == 0f)
                    //    return;

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
                SeaTruckSegment sts = target.GetComponent<SeaTruckSegment>();
                if (sts)
                //if (sts || target.GetComponent<Vehicle>() || target.GetComponent<Hoverbike>())
                {
                    //AddDebug("Vehicle takes damage"); 
                    //__result *= Main.config.vehicleDamageMult;
                    if (__result > 0 && sts && type == DamageType.Normal)
                    { // play sfx when predators attack truck
                        SeaTruckSegment cabin = SeaTruckSegment.GetHead(sts);
                        if (cabin && cabin.isMainCab)
                        {
                            DealDamageOnImpact ddoi = cabin.GetComponent<DealDamageOnImpact>();
                            if (ddoi && ddoi.impactSound && ddoi.timeLastImpactSound + .5f < Time.time)
                            {
                                //AddDebug("DealDamageOnImpact sound");
                                //ddoi.impactSound.SetParameterValue(ddoi.velocityParamIndex, __result);
                                ddoi.impactSound.Play();
                                ddoi.timeLastImpactSound = Time.time;
                            }
                        }
                    }
                }
                //else if (target.GetComponent<BaseCell>())
                //    AddDebug("base takes damage");
                else
                {
                    if (damageMult.ContainsKey(techType))
                        __result *= damageMult[techType];
                }
            }
        }

        [HarmonyPatch(typeof(DamagePlayerInRadius), "DoDamage")]
        class DamagePlayerInRadius_DoDamage_Patch
        { // cook fish using thermal lily
            static int damageTicksToCook = 2;
            static int damageTicks = 0;
            static GameObject fishToCook = null;

            static bool Prefix(DamagePlayerInRadius __instance)
            {
                if (!__instance.enabled || !__instance.gameObject.activeInHierarchy || __instance.damageRadius <= 0f || __instance.isPilotingPlayerProtected && Player.main.IsPiloting() && !Player.main.inHovercraft)
                    return false;

                float distanceToPlayer = __instance.tracker.distanceToPlayer;
                if (distanceToPlayer <= __instance.damageRadius)
                {
                    //if (__instance.doDebug)
                    //    Debug.Log((__instance.gameObject.name + ".DamagePlayerInRadius() - dist/damageRadius: " + distanceToPlayer + "/" + __instance.damageRadius + " => damageAmount: " + __instance.damageAmount));
                    if (__instance.damageType == DamageType.Radiation && Player.main.radiationAmount == 0f)
                        return false;
                    //if (__instance.doDebug)
                    //    Debug.Log(("TakeDamage: " + __instance.damageAmount + " " + __instance.damageType.ToString()));
                    //AddDebug("TakeDamage: " + __instance.damageAmount);
                    Player.main.GetComponent<LiveMixin>().TakeDamage(__instance.damageAmount, __instance.transform.position, __instance.damageType);

                    if (Inventory.main.quickSlots.heldItem == null)
                        fishToCook = null;
                    else
                    {
                        GameObject fish = Inventory.main.quickSlots.heldItem.item.gameObject;
                        //TechType tt = CraftData.GetTechType(fish);
                        if (Util.IsEatableFish(fish))
                        {
                            if (fishToCook == fish)
                            {
                                if (damageTicks == damageTicksToCook)
                                { 
                                    fishToCook = null;
                                    Util.CookFish(fish);
                                }
                                else
                                    damageTicks++;
                            }
                            else
                            {
                                fishToCook = fish;
                                damageTicks = 1;
                            }
                        }
                    }
                }
                //else
                //{
                //    if (!__instance.doDebug)
                //        return;
                //    Debug.Log((__instance.gameObject.name + ".DamagePlayerInRadius() - dist/damageRadius: " + distanceToPlayer + "/" + __instance.damageRadius + " => no damage"));
                //}
                return false;
            }
       
        }

        [HarmonyPatch(typeof(VFXSurfaceTypeDatabase), "SetPrefab")]
        class VFXSurfaceTypeDatabase_SetPrefab_Patch
        {
            static void Postfix(VFXSurfaceTypeDatabase __instance, VFXSurfaceTypes surfaceType, VFXEventTypes eventType, GameObject prefab)
            {
                if (surfaceType == VFXSurfaceTypes.organic)
                {
                    //Main.Log("VFXSurfaceTypeDatabase SetPrefab surfaceType " + surfaceType + " eventType " + eventType);
                    SetBloodColor(prefab);
                }

            }
        }

        [HarmonyPatch(typeof(PlayerFrozenMixin))]
        class PlayerFrozenMixin_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Freeze")]
            static bool Freeze_Prefix(PlayerFrozenMixin __instance)
            {
                bool flare_ = false;
                Flare flare = Tools_Patch.equippedTool as Flare;
                if (flare && flare.flareActivateTime > 0f)
                    flare_ = true;

                if (Main.config.brinewingAttackColdDamage > 0)
                {
                    float damage = Main.config.brinewingAttackColdDamage;
                    if (flare_)
                        damage *= .5f;

                    Main.bodyTemperature.AddCold(damage);
                    return false;
                }
                if (flare_)
                    return false;

                return true;
            }
        }

        //[HarmonyPatch(typeof(Crash), "Start")]
        class Crash_Start_Patch
        {
            public static void Postfix(Crash __instance)
            {
                SetBloodColor(__instance.detonateParticlePrefab);
            }
        }

        //[HarmonyPatch(typeof(BrinewingBrine), "OnTriggerEnter")]
        class BrinewingBrine_OnTriggerEnter_Patch
        {
            static bool Prefix(BrinewingBrine __instance, Collider collider)
            {
                if (collider.isTrigger && collider.gameObject.layer != LayerMask.NameToLayer("Useable"))
                    return false;
                GameObject gameObject = collider.attachedRigidbody == null ? collider.gameObject : collider.attachedRigidbody.gameObject;
                if (gameObject == null)
                    return false;
                GameObject dealer = __instance.brinewing != null ? __instance.brinewing.gameObject : null;
                if (gameObject.Equals(dealer))
                    return false;
                //bool isFrozen = false;
                //FrozenMixin fm = gameObject.GetComponent<FrozenMixin>();
                //if (fm != null && !fm.IsFrozenInsideIce())
                //{
                //    isFrozen = fm.IsFrozen();
                //    fm.FreezeForTime(__instance.freezeCreatureTime);
                //}
                //if (!isFrozen)
                //{
                BodyTemperature bt = gameObject.GetComponent<BodyTemperature>();
                if (bt)
                {
                    //AddDebug("coldMeterMaxValue " + bt.coldMeterMaxValue);
                    //AddDebug("AddCold " + __instance.coldDamage * 100f);
                    bt.AddCold(__instance.coldDamage * 100f);
                }
                else
                {
                    LiveMixin lm = gameObject.GetComponent<LiveMixin>();
                    if (lm != null && lm.IsAlive())
                    {
                        lm.TakeDamage(__instance.coldDamage * 10, __instance.transform.position, DamageType.Cold, dealer);
                        lm.NotifyCreatureDeathsOfCreatureAttack();
                        //if (__instance.GetComponent<Player>())
                        //    AddDebug("Take Cold Damage " + __instance.coldDamage * 100f);
                    }
                }
                    //if (fm is PlayerFrozenMixin && __instance.brinewing != null)
                    //    __instance.brinewing.OnFreezePlayer();
                //}
                __instance.Despawn();
                return false;
            }
        }

        //[HarmonyPatch(typeof(VFXSurfaceTypeManager), "Play", new Type[] { typeof(VFXSurfaceTypes), typeof(VFXEventTypes), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
        class VFXSurfaceTypeManager_Play_Patch
        { // blood color
            static bool Prefix(VFXSurfaceTypeManager __instance, ref ParticleSystem __result, VFXSurfaceTypes surfaceType, VFXEventTypes eventType, Vector3 position, Quaternion orientation, Transform parent)
            {
                ParticleSystem particleSystem = null;
                GameObject fxprefab = __instance.GetFXprefab(surfaceType, eventType);
                if (fxprefab != null)
                {
                    //AddDebug("VFXSurfaceTypeManager Play surfaceType " + surfaceType + " eventType " + eventType);
                    GameObject fx = UnityEngine.Object.Instantiate<GameObject>(fxprefab, position, orientation);
                    if (eventType == VFXEventTypes.exoDrill)
                    {
                        fx.transform.parent = null;
                        fx.GetComponent<VFXFakeParent>().Parent(parent, Vector3.zero, Vector3.zero);
                        fx.GetComponent<VFXLateTimeParticles>().Play();
                        particleSystem = fx.GetComponent<ParticleSystem>();
                    }
                    else
                    {
                        fx.transform.parent = parent;
                        if (surfaceType == VFXSurfaceTypes.organic)
                        {
                            //AddDebug("VFXSurfaceTypeManager Play " + parent.name);
                            SetBloodColor(fx);
                        }
                        particleSystem = fx.GetComponent<ParticleSystem>();
                        //var startSize = particleSystem.main.startSize;
                        //startSize.curveMultiplier = .1f;
                        //var sub = particleSystem.subEmitters;
                        //sub.enabled = false;
                        particleSystem.Play();
                    }
                }
                //particleSystem.startColor = new Color(1f, 1f, 1f);
                __result = particleSystem;
                //__result = null;
                return false;
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
                        //EventInstance fmodEvent = Utils.GetFMODEvent(__instance.hitSound, __instance.transform.position);
                        //int num1 = (int)fmodEvent.setParameterValueByIndex(__instance.surfaceParamIndex, (float)vfxSurfaceTypes);
                        //int num2 = (int)fmodEvent.start();
                        //int num3 = (int)fmodEvent.release();
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

    }
}
