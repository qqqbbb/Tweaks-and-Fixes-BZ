using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Damage_
    {
        static public Color bloodColor;
        private static bool clawArmHit;
        public static Dictionary<TechType, float> damageModifiers = new Dictionary<TechType, float>();

        static void SetBloodColor(GameObject go)
        {
            //0.784f, 1f, 0.157f
            if (bloodColor == default)
                return;

            ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
            //AddDebug("SetBloodColor " + go.name + " " + pss.Length);
            //Main.Log("SetBloodColor " + go.name );
            foreach (ParticleSystem ps in pss)
            {
                //ps.startColor = new Color(1f, 0f, 0f);
                ParticleSystem.MainModule psMain = ps.main;
                //Main.Log("startColor " + psMain.startColor.color);
                //AddDebug("startColor " + psMain.startColor.color);
                //Color newColor = new Color(ConfigToEdit.bloodColor.Value.x, ConfigToEdit.bloodColor.Value.y, ConfigToEdit.bloodColor.Value.z, psMain.startColor.color.a);
                //newColor = Color.blue;
                //Main.Log("blood Color " + newColor);
                //AddDebug("blood Color " + newColor);
                psMain.startColor = new ParticleSystem.MinMaxGradient(bloodColor);
                //psMain.startSizeMultiplier *= .1f;
            }
        }

        [HarmonyPatch(typeof(DealDamageOnImpact))]
        class DealDamageOnImpact_Patch
        { // seatruck mirroredSelfDamageFraction .12, hoverbike mirroredSelfDamageFraction 1, exosuit mirroredSelfDamage false
            //static Rigidbody prevColTarget;
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            static void StartPostfix(DealDamageOnImpact __instance)
            {
                //AddDebug(" DealDamageOnImpact Start " + __instance.name);
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //AddDebug(" DealDamageOnImpact Start " + tt);
                //if (tt == TechType.SeaTruck)
                //    __instance.mirroredSelfDamageFraction = 1f;
                //if (__instance.GetComponent<Hoverbike>())
                //    hoverBikes.Add(__instance);
                //__instance.mirroredSelfDamageFraction = .25f;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnCollisionEnter")]
            public static bool OnCollisionEnterPrefix(DealDamageOnImpact __instance, Collision collision)
            {
                if (!ConfigToEdit.replaceDealDamageOnImpactScript.Value)
                    return true;

                if (!__instance.enabled || collision.contacts.Length == 0 || __instance.exceptions.Contains(collision.gameObject))
                    return false;

                bool terrain = collision.gameObject.GetComponent<TerrainChunkPieceCollider>();
                GameObject colTarget = collision.gameObject;
                if (!terrain)
                    colTarget = Util.GetEntityRoot(collision.gameObject);

                if (!colTarget)
                    colTarget = collision.gameObject;

                if (collision.gameObject.GetComponentInParent<Player>())
                {
                    if (!__instance.allowDamageToPlayer)
                    {
                        //AddDebug(__instance.name + " collided with player");
                        return false;
                    }
                    colTarget = Player.mainObject;
                }
                //if (colTarget)
                //    AddDebug(__instance.name + " OnCollisionEnter " + colTarget.name);
                //else
                //    AddDebug(__instance.name + " OnCollisionEnter colTarget null ");

                // collision.contacts generates garbage
                ContactPoint contactPoint = collision.GetContact(0);
                Vector3 impactPoint = contactPoint.point;
                float damageMult = Mathf.Max(0f, Vector3.Dot(-contactPoint.normal, __instance.prevVelocity));

                damageMult = Mathf.Clamp(damageMult, 0f, 10f);

                Rigidbody otherRB = collision.rigidbody;
                float myMass = __instance.GetComponent<Rigidbody>().mass;
                float massRatioInv;
                float massRatio;
                if (terrain)
                {
                    massRatio = .01f;
                    massRatioInv = 100f;
                }
                else
                {
                    if (otherRB)
                    {
                        massRatio = myMass / otherRB.mass;
                        massRatioInv = otherRB.mass / myMass;
                        //AddDebug("myMass " + myMass + " other mass " + otherRB.mass);
                    }
                    else
                    {
                        Bounds otherBounds = Util.GetAABB(colTarget);
                        Bounds myBounds = Util.GetAABB(__instance.gameObject);
                        massRatioInv = otherBounds.size.magnitude / myBounds.size.magnitude;
                        massRatio = myBounds.size.magnitude / otherBounds.size.magnitude;
                        //AddDebug("myBounds " + myBounds.size.magnitude + " otherBounds " + otherBounds.size.magnitude);
                    }
                }
                TechType myTT = CraftData.GetTechType(__instance.gameObject);
                TechType otherTT = CraftData.GetTechType(colTarget);

                bool vehicle = myTT == TechType.SeaTruck || myTT == TechType.Exosuit || myTT == TechType.SeaTruckAquariumModule || myTT == TechType.SeaTruckDockingModule || myTT == TechType.SeaTruckFabricatorModule || myTT == TechType.SeaTruckSleeperModule || myTT == TechType.SeaTruckStorageModule || myTT == TechType.SeaTruckTeleportationModule || myTT == TechType.Hoverbike;

                bool otherVehicle = otherTT == TechType.SeaTruck || otherTT == TechType.Exosuit || otherTT == TechType.SeaTruckAquariumModule || otherTT == TechType.SeaTruckDockingModule || otherTT == TechType.SeaTruckFabricatorModule || otherTT == TechType.SeaTruckSleeperModule || otherTT == TechType.SeaTruckStorageModule || otherTT == TechType.SeaTruckTeleportationModule || otherTT == TechType.Hoverbike;

                DealDamageOnImpact otherDDOI = colTarget.GetComponent<DealDamageOnImpact>();

                bool canDealDamage = true;
                if (vehicle && !ConfigToEdit.vehiclesDealDamageOnImpact.Value)
                    canDealDamage = false;
                if (otherVehicle && !ConfigToEdit.vehiclesTakeDamageOnImpact.Value)
                    canDealDamage = false;
                if (otherDDOI && damageMult < otherDDOI.speedMinimumForDamage)
                    canDealDamage = false;
                if (damageMult < __instance.speedMinimumForDamage)
                    canDealDamage = false;

                //AddDebug("damageMult " + damageMult + " speedMinimumForDamage " + __instance.speedMinimumForDamage);
                if (__instance.impactSound && __instance.timeLastImpactSound + 0.5 < Time.time)
                {
                    if (__instance.checkForFishHitSound)
                    {
                        bool fish = false;
                        GameObject gameObject = collision.gameObject;
                        GameObject entityRoot = UWE.Utils.GetEntityRoot(collision.gameObject);
                        if (entityRoot != null)
                            gameObject = entityRoot;

                        if (gameObject.GetComponent<Creature>() != null)
                        {
                            BehaviourType behaviourType = CreatureData.GetBehaviourType(gameObject);
                            fish = behaviourType == BehaviourType.SmallFish || behaviourType == BehaviourType.MediumFish;
                        }
                        __instance.impactSound.SetParameterValue(__instance.hitFishParamIndex, fish ? 1f : 0f);
                    }
                    __instance.impactSound.SetParameterValue(__instance.velocityParamIndex, damageMult);
                    __instance.impactSound.Play();
                    __instance.timeLastImpactSound = Time.time;
                }

                if (canDealDamage && Time.time > __instance.timeLastDamage + 1f)
                {
                    LiveMixin otherLM = __instance.GetLiveMixin(colTarget);
                    if (otherLM)
                    {
                        //AddDebug("damageMult " + damageMult);
                        //AddDebug("massRatio " + massRatio);
                        //AddDebug(otherLM.name + " max HP " + otherLM.maxHealth + " HP " + (int)otherLM.health);
                        if (otherLM.health > 0 && damageMult > 0)
                        {
                            VFXSurfaceTypes mySurfaceType = VFXSurfaceTypes.none;
                            if (vehicle)
                                mySurfaceType = VFXSurfaceTypes.metal;
                            else
                                mySurfaceType = Util.GetObjectSurfaceType(__instance.gameObject);

                            float massRatioClamped = Mathf.Clamp(massRatio, 0, damageMult);
                            massRatioClamped = Util.NormalizeToRange(massRatioClamped, 0f, 10f, 1f, 2f);
                            if (mySurfaceType == VFXSurfaceTypes.metal || mySurfaceType == VFXSurfaceTypes.glass || mySurfaceType == VFXSurfaceTypes.rock)
                                massRatioClamped *= 2f;

                            float damage = damageMult * massRatioClamped;
                            //AddDebug(__instance.name + " damage " + (int)damage);
                            //AddDebug(__instance.name + " speedMinimumForDamage " + __instance.speedMinimumForDamage);
                            otherLM.TakeDamage(damage, impactPoint, DamageType.Collide, __instance.gameObject);
                            __instance.timeLastDamage = Time.time;
                        }
                    }
                }

                bool canTakeDamage = true;
                if (vehicle && !ConfigToEdit.vehiclesTakeDamageOnImpact.Value)
                    canTakeDamage = false;
                if (otherVehicle && !ConfigToEdit.vehiclesDealDamageOnImpact.Value)
                    canTakeDamage = false;
                if (damageMult < __instance.speedMinimumForSelfDamage)
                    canTakeDamage = false;
                if (myTT == TechType.Exosuit && !ConfigToEdit.exosuitTakesDamageFromCollisions.Value)
                    canTakeDamage = false;

                LiveMixin myLM = __instance.GetLiveMixin(__instance.gameObject);
                bool tooSmall = otherRB && otherRB.mass <= __instance.minimumMassForDamage;

                if (!canTakeDamage || damageMult <= 0 || __instance.mirroredSelfDamageFraction == 0f || !myLM || Time.time < __instance.timeLastDamagedSelf + 1f || tooSmall)
                    return false;

                if (terrain && myTT == TechType.Exosuit && !ConfigToEdit.exosuitTakesDamageWhenCollidingWithTerrain.Value)
                    return false;
                //float myDamage = colMag * Mathf.Clamp((1f + massRatio * 0.001f), 0f, damageMult);
                VFXSurfaceTypes surfaceType = VFXSurfaceTypes.none;
                if (terrain)
                    surfaceType = Utils.GetTerrainSurfaceType(impactPoint, contactPoint.normal);
                else
                    surfaceType = Util.GetObjectSurfaceType(colTarget);

                //AddDebug(colTarget.name + " surface " + surfaceType);
                float massRatioInvClamped = Mathf.Clamp(massRatioInv, 0, damageMult);
                massRatioInvClamped = Util.NormalizeToRange(massRatioInvClamped, 0f, 10f, 1f, 2f);
                if (terrain || surfaceType == VFXSurfaceTypes.glass || surfaceType == VFXSurfaceTypes.metal || surfaceType == VFXSurfaceTypes.rock)
                    massRatioInvClamped *= 2f;

                float myDamage = damageMult * massRatioInvClamped;
                //AddDebug(__instance.name + " maxHealth " + myLM.maxHealth + " health " + (int)myLM.health);
                if (__instance.capMirrorDamage != -1f)
                    myDamage = Mathf.Min(__instance.capMirrorDamage, myDamage);

                myLM.TakeDamage(myDamage, impactPoint, DamageType.Collide, __instance.gameObject);
                //AddDebug(__instance.name + " myDamage " + (int)myDamage);
                //AddDebug(__instance.name + " speedMinimumForSelfDamage " + __instance.speedMinimumForSelfDamage);
                __instance.timeLastDamagedSelf = Time.time;
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
                if (ConfigToEdit.bloodColor.Value == "0.784 1.0 0.157")
                    return;

                VFXSurface surface = __instance.GetComponent<VFXSurface>();
                if (surface && surface.surfaceType == VFXSurfaceTypes.organic)
                {
                    if (__instance.data.damageEffect)
                        SetBloodColor(__instance.data.damageEffect);

                    if (__instance.data.deathEffect)
                        SetBloodColor(__instance.data.deathEffect);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("TakeDamage")]
            static bool TakeDamagePrefix(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
            {
                if (damageModifiers?.Count > 0)
                {
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    //var d = originalDamage;
                    if (damageModifiers.ContainsKey(tt))
                        originalDamage *= damageModifiers[tt];

                    //AddDebug($"TakeDamage or {d} mod {damageModifiers[tt]} d {originalDamage}");
                }
                if (originalDamage > 0 && type == DamageType.Normal || type == DamageType.Fire || type == DamageType.Collide || type == DamageType.Acid || type == DamageType.Heat || type == DamageType.Drill || type == DamageType.Explosive || type == DamageType.Puncture)
                {
                    FrozenMixin frozenMixin = __instance.GetComponent<FrozenMixin>();
                    if (frozenMixin && frozenMixin.IsFrozen())
                    {
                        //AddDebug("frozenMixin tekes damage ");
                        frozenMixin.Unfreeze();
                        return false;
                    }
                }
                bool hitByPlayer = dealer == Player.main.gameObject || clawArmHit || type == DamageType.Drill;
                if (ConfigToEdit.removeBigParticlesWhenKnifing.Value && Main.gameLoaded && hitByPlayer && originalDamage > 0 && type == DamageType.Normal || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture || type == DamageType.LaserCutter)
                { // dont spawn big damage particles if knifed by player
                    if (__instance.damageEffect)
                    {
                        //AddDebug(" damageEffect  " + __instance.damageEffect.name);
                        __instance.timeLastDamageEffect = Time.time;
                    }
                    else if (__instance.GetComponentInChildren<VFXSurface>())
                    {
                        //AddDebug(" vfxSurface  ");
                        __instance.timeLastDamageEffect = Time.time;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ExosuitClawArm))]
        class ExosuitClawArm_Patch
        {

            [HarmonyPrefix, HarmonyPatch("OnHit")]
            static void OnHitPrefix(ExosuitClawArm __instance)
            {
                clawArmHit = true;
            }
            [HarmonyPostfix, HarmonyPatch("OnHit")]
            static void OnHitPostfix(ExosuitClawArm __instance)
            {
                clawArmHit = false;
            }
        }

        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage", new Type[] { typeof(TechType), typeof(float), typeof(float), typeof(DamageType), typeof(GameObject), typeof(GameObject) })]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result, TechType techType)
            {
                if (__result <= 0f)
                    return;

                //if (type == DamageType.Drill)
                //{
                //    __result *= ConfigMenu.drillDamageMult.Value;
                //AddDebug("CalculateDamage Drill");
                //}
                if (techType == TechType.Player)
                {
                    //__result *= Main.config.playerDamageMult;
                    //AddDebug("Player takes damage " + __result);
                    //if (__result == 0f)
                    //    return;

                    if (ConfigToEdit.dropHeldTool.Value)
                    {
                        if (type != DamageType.Cold && type != DamageType.Poison && type != DamageType.Starve && type != DamageType.Radiation && type != DamageType.Pressure)
                        {
                            float rnd = UnityEngine.Random.Range(1f, Player.main.liveMixin.maxHealth);
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
                    if (__result > 0 && type == DamageType.Normal)
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
                if (dealer && !ConfigToEdit.vehiclesHurtCreatures.Value && Creature_Patch.creatureTT.Contains(techType) && Util.IsVehicle(dealer))
                {
                    //AddDebug("CalculateDamage by " + dealer.name + " to " + techType);
                    __result = 0;
                }
            }
        }

        [HarmonyPatch(typeof(DamagePlayerInRadius), "DoDamage")]
        class DamagePlayerInRadius_DoDamage_Patch
        { // thermal lily cookS fish 
            static int damageTicksToCook = 3;
            static int damageTicks = 0;
            static DamagePlayerInRadius damageDealer;

            static void Postfix(DamagePlayerInRadius __instance)
            {
                if (__instance.tracker.distanceToPlayer < __instance.damageRadius)
                {
                    //AddDebug($"damageTicks {damageTicks}");
                    damageDealer = __instance;
                    damageTicks++;
                    if (Inventory.main.quickSlots.heldItem != null)
                    {
                        if (damageTicks >= damageTicksToCook)
                        {
                            GameObject fish = Inventory.main.quickSlots.heldItem.item.gameObject;
                            if (Util.IsRawFish(fish))
                            {
                                Util.CookFish(fish);
                                damageTicks = 0;
                            }
                        }
                    }
                    else
                        damageTicks = 0;
                }
                else if (__instance == damageDealer)
                {
                    damageDealer = null;
                    damageTicks = 0;
                }
            }
        }

        [HarmonyPatch(typeof(VFXSurfaceTypeDatabase), "SetPrefab")]
        class VFXSurfaceTypeDatabase_SetPrefab_Patch
        {
            static void Postfix(VFXSurfaceTypeDatabase __instance, VFXSurfaceTypes surfaceType, VFXEventTypes eventType, GameObject prefab)
            {
                if (surfaceType == VFXSurfaceTypes.organic && ConfigToEdit.bloodColor.Value != "0.784 1.0 0.157")
                {
                    //Main.Log("VFXSurfaceTypeDatabase SetPrefab surfaceType " + surfaceType + " eventType " + eventType);
                    SetBloodColor(prefab);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerFrozenMixin))]
        class PlayerFrozenMixin_Patch
        {
            [HarmonyPrefix, HarmonyPatch("Freeze")]
            static bool Freeze_Prefix(PlayerFrozenMixin __instance)
            {
                bool flareBurning = false;
                Flare flare = Tools_Patch.equippedTool as Flare;
                if (flare && flare.flareActivateTime > 0f && flare.energyLeft > 0f)
                    flareBurning = true;

                if (ConfigToEdit.brinewingAttackColdDamage.Value > 0)
                {
                    float damage = ConfigToEdit.brinewingAttackColdDamage.Value;
                    if (flareBurning)
                        damage *= .5f;

                    BodyTemperature bodyTemperature = __instance.GetComponent<BodyTemperature>();
                    if (bodyTemperature)
                        bodyTemperature.AddCold(damage);

                    return false;
                }
                if (flareBurning)
                    return false;

                return true;
            }
        }


    }
}
