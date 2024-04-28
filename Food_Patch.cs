using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Food_Patch : MonoBehaviour
    {
        static float foodCons = .5f; // vanilla 0.4
        static float waterCons = .5f; // vanilla 0.55
        //static float updateHungerInterval { get { return Main.config.hungerUpdateInterval / DayNightCycle.main.dayNightSpeed; } }
        static float hungerUpdateTime = 0f;
        static float snowBallMeltRate = 0.05f;
        public static HashSet<TechType> decayingFood = new HashSet<TechType>();

        public static void CheckSnowball(Eatable eatable)
        {
            InventoryItem inventoryItem = eatable.GetComponent<Pickupable>().inventoryItem;
            ItemsContainer container = null;
            if (inventoryItem != null)
            {
                container = inventoryItem.container as ItemsContainer;
                if (Main.fridges.Contains(container))
                //if (container != null && container.tr.parent && container.tr.parent.GetComponent<Fridge>())
                {
                    //AddDebug("snowball in fridge " );
                    return;
                }
            }
            else
            {
                float dist = Vector3.Distance(Player.main.transform.position, eatable.transform.position);
                //AddDebug( " dist " + dist);
                if (dist > 33f)
                {
                    eatable.CancelInvoke();
                    UnityEngine.Object.Destroy(eatable.gameObject);
                    return;
                }
            }
            if (eatable.GetWaterValue() <= 0f)
            {
                if (container != null)
                    container.RemoveItem(inventoryItem.item);
                //AddDebug("Destroy snowball ");
                eatable.CancelInvoke();
                UnityEngine.Object.Destroy(eatable.gameObject);
                return;
            }
            float temp = Util.GetTemperature(eatable.gameObject);
            //AddDebug(eatable.name + " temperature " + temp);
            if (temp > 0)
            {
                //eatable.kDecayRate = decayRate * temp ;
                eatable.UnpauseDecay();
            }
            else if (temp < 0)
                eatable.PauseDecay();
            //AddDebug(" GetWaterValue " + eatable.GetWaterValue());
            //AddDebug("timePassedAsFloat " + DayNightCycle.main.timePassedAsFloat);
            //AddDebug("timeDecayStart " + eatable.timeDecayStart);
            //AddDebug("DecayValue " + eatable.GetDecayValue());
            //AddDebug("snowball GetDecayValue " + eatable.GetDecayValue());
        }

        public static void CheckWater(Eatable eatable)
        {   // __instance.timeDecayStart stores decay value
            float temp = Util.GetTemperature(eatable.gameObject);
            //TechType tt = CraftData.GetTechType(eatable.gameObject);
            //if (tt == TechType.BigFilteredWater)
            //AddDebug(eatable.name + " CheckWater " + temp);
            //AddDebug(eatable.name + " CheckWater " + eatable.timeDecayStart);
            if (temp < 0f)
            {
                //AddDebug(" freeze " + eatable.name);
                //eatable.UnpauseDecay();
                if (eatable.timeDecayStart < eatable.waterValue)
                    eatable.timeDecayStart += ConfigMenu.waterFreezeRate.Value * DayNightCycle.main._dayNightSpeed;
                else if (eatable.timeDecayStart > eatable.waterValue)
                    eatable.timeDecayStart = eatable.waterValue;
            }
            else if (temp > 0f)
            {
                if (eatable.timeDecayStart > 0f)
                    eatable.timeDecayStart -= ConfigMenu.waterFreezeRate.Value * DayNightCycle.main._dayNightSpeed;
                else if (eatable.timeDecayStart < 0f)
                    eatable.timeDecayStart = 0f;
                //AddDebug(" thaw " + eatable.name);
                //eatable.timeDecayStart += eatable.kDecayRate;
                //if (eatable.GetWaterValue() < eatable.waterValue && eatable.timeDecayPause < DayNightCycle.main.timePassedAsFloat)
                //{
                //    AddDebug(" thaw " + eatable.name);
                //    eatable.timeDecayPause -= waterFreezeRate * 33.33f * DayNightCycle.main._dayNightSpeed;
                //}
                //eatable.PauseDecay();
            }
            //AddDebug(eatable.name + " CheckWater done " + eatable.timeDecayStart);
            //AddDebug(" GetWaterValue " + eatable.GetWaterValue());
            //AddDebug("timePassedAsFloat " + DayNightCycle.main.timePassedAsFloat);
            //AddDebug("timeDecayStart " + eatable.timeDecayStart);
            //AddDebug("DecayValue " + eatable.GetDecayValue());
            //AddDebug("snowball GetDecayValue " + eatable.GetDecayValue());
        }

        public static void CheckFood(Eatable eatable)
        {
            //AddDebug(" CheckFood " + eatable.name);
            float temp = Util.GetTemperature(eatable.gameObject);
            if (temp < 0f)
                eatable.PauseDecay();
            else
                eatable.UnpauseDecay();
        }

        [HarmonyPatch(typeof(SnowBall), "Awake")]
        class SnowBall_Awake_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(SnowBall __instance)
            {
                //AddDebug("SnowBall Awake");
                if (ConfigMenu.snowballWater.Value > 0)
                {
                    Eatable eatable = __instance.gameObject.EnsureComponent<Eatable>();
                    eatable.kDecayRate = snowBallMeltRate;
                    eatable.decomposes = true;
                    eatable.waterValue = ConfigMenu.snowballWater.Value;
                    eatable.coldMeterValue = ConfigMenu.snowballWater.Value;
                    //AddDebug("SnowBall Awake waterValue " + eatable.waterValue);
                    __instance.GetComponent<WorldForces>().underwaterGravity = .5f;
                }
                LiveMixin lm = __instance.gameObject.AddComponent<LiveMixin>();
                lm.data = ScriptableObject.CreateInstance<LiveMixinData>();
                lm.data.maxHealth = 1;
                lm.data.destroyOnDeath = true;
                //lm.data.explodeOnDestroy = false;
                lm.data.knifeable = true;
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.snow); // vanilla is metal
                //SnowBallChecker snowBallChecker = __instance.gameObject.EnsureComponent<SnowBallChecker>();
                //snowBallChecker.InvokeRepeating("CheckSnowball", 1f, checkInterval);
            }

            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            static bool UpdatePrefix(SnowBall __instance)
            {
                if (__instance.throwing)
                    __instance.sequence.Update();

                return false;
            }
        }

        public static void UpdateStats(Survival __instance)
        {
            //AddDebug("dayNightSpeed  " + DayNightCycle.main.dayNightSpeed);
            //AddDebug("UpdateStats  " + updateHungerInterval);
            float oldFood = __instance.food;
            float oldWater = __instance.water;
            bool hunger = GameModeManager.GetOption<bool>(GameOption.Hunger);
            bool thirst = GameModeManager.GetOption<bool>(GameOption.Thirst);

            if (hunger)
                __instance.food -= foodCons;

            if (thirst)
                __instance.water -= waterCons;

            if (Player_Movement.timeSprinted > 0f)
            {
                float sprintFoodCons = foodCons * Player_Movement.timeSprinted * ConfigMenu.hungerUpdateInterval.Value * .01f;
                //AddDebug("UpdateStats timeSprinted " + Player_Movement.timeSprinted);
                //AddDebug("UpdateStats sprintFoodCons " + sprintFoodCons);
                if (hunger)
                    __instance.food -= sprintFoodCons;

                if (thirst)
                    __instance.water -= sprintFoodCons;

                Player_Movement.timeSprintStart = 0f;
                Player_Movement.timeSprinted = 0f;
            }
            float foodDamage = 0f;
            if (hunger && __instance.food < -100f)
            {
                foodDamage = Mathf.Abs(__instance.food + 100f);
                __instance.food = -100f;
            }
            if (thirst && __instance.water < -100f)
            {
                foodDamage += Mathf.Abs(__instance.water + 100f);
                __instance.water = -100f;
            }
            if (foodDamage > 0)
                Player.main.liveMixin.TakeDamage(foodDamage, Player.main.gameObject.transform.position, DamageType.Starve);

            float threshold1 = ConfigMenu.newHungerSystem.Value ? 0f : 20f;
            float threshold2 = ConfigMenu.newHungerSystem.Value ? -50f : 10f;
            if (GameModeManager.GetOption<bool>(GameOption.ShowHungerAlerts))
            {
                __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, oldFood, threshold1, threshold2);
                __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, oldWater, threshold1, threshold2);
            }
            hungerUpdateTime = Time.time + ConfigMenu.hungerUpdateInterval.Value;
            //AddDebug("Invoke  hungerUpdateInterval " + Main.config.hungerUpdateInterval);
            //AddDebug("Invoke dayNightSpeed " + DayNightCycle.main.dayNightSpeed);
            //__instance.Invoke("UpdateHunger", updateHungerInterval);
        }

        [HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_patch
        {
            public static void Postfix(Player __instance)
            {
                if (!GameModeManager.GetOption<bool>(GameOption.Hunger) || Main.survival.freezeStats || uGUI.isLoading || hungerUpdateTime > Time.time)
                    return;

                //AddDebug("UpdateHunger");
                if (ConfigMenu.newHungerSystem.Value)
                    UpdateStats(Main.survival);
                else
                    Main.survival.UpdateHunger();
            }
        }

        [HarmonyPatch(typeof(ToggleOnClick), "SwitchOn")]
        internal class ToggleOnClick_Patch
        {
            public static void Postfix(ToggleOnClick __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //AddDebug("SwitchOn " + tt);
                if (tt == TechType.SmallStove)
                {
                    PlayerTool heldTool = Inventory.main.GetHeldTool();
                    if (heldTool)
                    {
                        GameObject go = heldTool.gameObject;
                        if (Util.IsCreatureAlive(go) && Util.IsEatableFish(go))
                            Util.CookFish(go);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Survival))]
        internal class Survival_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(Survival __instance)
            { // cancel UpdateHunger
                __instance.CancelInvoke();
            }

            [HarmonyPostfix]
            [HarmonyPatch("UpdateHunger")]
            internal static void UpdateHungerPostfix(Survival __instance)
            {
                //AddDebug("UpdateHunger ");
                hungerUpdateTime = Time.time + ConfigMenu.hungerUpdateInterval.Value;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Eat")]
            public static bool EatPrefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                //AddDebug("Survival eat " + useObj.name);
                //if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla && !Main.config.newHungerSystem && !Main.config.foodTweaks)
                //    return true;

                Eatable eatable = useObj.GetComponent<Eatable>();
                //bool removeOnUse = false;
                //bool wasUsed = false;
                bool canBeUsed = eatable.maxCharges == 0 || eatable.charges > 0;
                if (!canBeUsed)
                {
                    __result = false;
                    return false;
                }
                int food = (int)eatable.GetFoodValue();
                int water = (int)eatable.GetWaterValue();
                float healthValue = eatable.GetHealthValue();
                float coldMeterValue = eatable.GetColdMeterValue();
                int playerMinFood = ConfigMenu.newHungerSystem.Value ? -100 : 0;
                float playerMaxWater = ConfigMenu.newHungerSystem.Value ? 200f : 100f;
                float playerMaxFood = 200f;
                int minFood = food;
                int maxFood = food;
                int minWater = water;
                int maxWater = water;
                //removeOnUse = eatable.removeOnUse;
                //AddDebug("maxCharges " + eatable.maxCharges);
                //AddDebug("charges " + eatable.charges);
                //AddDebug("food " + food);
                //AddDebug("water " + water);
                if (Util.IsEatableFish(useObj))
                {
                    if (food > 0)
                    {
                        if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Vanilla)
                        {
                            minFood = food;
                            maxFood = food;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                        {
                            minFood = 0;
                            maxFood = food;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                        {
                            minFood = -food;
                            maxFood = food;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                        {
                            minFood = -food;
                            maxFood = 0;
                        }
                    }
                    if (water > 0)
                    {
                        if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Vanilla)
                        {
                            minWater = water;
                            maxWater = water;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                        {
                            minWater = 0;
                            maxWater = water;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                        {
                            minWater = -water;
                            maxWater = water;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                        {
                            minWater = -water;
                            maxWater = 0;
                        }
                    }
                }
                int rndFood = Main.rndm.Next(minFood, maxFood);
                float finalFood = Mathf.Min(food, rndFood);
                int rndWater = Main.rndm.Next(minWater, maxWater);
                //AddDebug("minWater " + minWater + " maxWater " + maxWater);
                float finalWater = Mathf.Min(water, rndWater);
                //AddDebug("finalWater " + finalWater);
                if (ConfigMenu.newHungerSystem.Value && __instance.food > 100f && finalFood > 0f)
                {
                    float mult = (200f - __instance.food) * .01f;
                    finalFood *= mult;
                }
                if (ConfigMenu.newHungerSystem.Value && __instance.water > 100f && finalWater > 0f)
                {
                    float mult = (200f - __instance.water) * .01f;
                    finalWater *= mult;
                    //AddDebug("newHungerSystem finalWater " + finalWater);
                }
                if (finalWater < 0f && __instance.water + finalWater < playerMinFood)
                {
                    int waterDamage = (int)(__instance.water + finalWater - playerMinFood);
                    //AddDebug("waterDamage " + waterDamage);
                    Player.main.liveMixin.TakeDamage(Mathf.Abs(waterDamage), Player.main.gameObject.transform.position, DamageType.Starve);
                }
                if (finalFood < 0f && __instance.food + finalFood < playerMinFood)
                {
                    int foodDamage = (int)(__instance.food + finalFood - playerMinFood);
                    //AddDebug("foodDamage " + foodDamage);
                    Player.main.liveMixin.TakeDamage(Mathf.Abs(foodDamage), Player.main.gameObject.transform.position, DamageType.Starve);
                }
                if (ConfigToEdit.eatingOutsideCold.Value > 0 && __instance.bodyTemperature.isExposed)
                {
                    //AddDebug("eating  isExposed ");
                    __instance.bodyTemperature.AddCold(ConfigToEdit.eatingOutsideCold.Value);
                }
                //AddDebug("finalFood " + finalFood);
                //AddDebug("finalWater " + finalWater);

                if (finalFood > 0)
                    GoalManager.main.OnCustomGoalEvent("Eat_Something");
                if (finalWater > 0)
                    GoalManager.main.OnCustomGoalEvent("Drink_Something");

                __instance.onEat.Trigger(finalFood);
                __instance.food += finalFood;
                __instance.onDrink.Trigger(finalWater);
                __instance.water += finalWater;
                if (healthValue != 0f)
                {
                    //AddDebug("healthValue " + healthValue);
                    if (healthValue > 0f)
                    {
                        __instance.liveMixin.AddHealth(healthValue);
                        GoalManager.main.OnCustomGoalEvent("Heal_Damage");
                    }
                    else if (healthValue <= -1f)
                        __instance.liveMixin.TakeDamage(-healthValue, type: DamageType.FoodPoison);
                }
                if (coldMeterValue != 0f)
                {
                    //AddDebug(" survival eat coldMeterValue " + coldMeterValue);
                    __instance.bodyTemperature.AddCold(coldMeterValue);
                }
                TechType techType = CraftData.GetTechType(useObj);
                if (techType == TechType.None)
                {
                    Pickupable p = useObj.GetComponent<Pickupable>();
                    if (p)
                        techType = p.GetTechType();
                }
                FMODAsset useSound = __instance.player.GetUseSound(TechData.GetSoundType(techType));
                if (eatable.IsRotten())
                    useSound = __instance.ateRottenFoodSound;

                if (useSound)
                    Utils.PlayFMODAsset(useSound, __instance.player.transform.position);

                if (techType == TechType.Bladderfish)
                    Player.main.GetComponent<OxygenManager>().AddOxygen(15f);

                if (eatable.maxCharges > 0)
                    eatable.ConsumeCharge();

                Mathf.Clamp(__instance.water, playerMinFood, playerMaxWater);
                Mathf.Clamp(__instance.food, playerMinFood, playerMaxFood);

                int warn = ConfigMenu.newHungerSystem.Value ? 0 : 20;
                if (!__instance.InConversation())
                {
                    if (finalWater > 0f && __instance.water > warn && __instance.water - finalWater < warn)
                        __instance.vitalsOkNotification.Play();

                    else if (finalFood > 0f && __instance.food > warn && __instance.food - finalWater < warn)
                        __instance.vitalsOkNotification.Play();
                }
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(eatable))
                {
                    float notFrozenWater = eatable.GetWaterValue();
                    eatable.removeOnUse = eatable.waterValue == notFrozenWater;
                    //AddDebug(" waterValue " + eatable.waterValue + " finalWater " + finalWater);
                    if (eatable.waterValue > notFrozenWater)
                    {
                        __instance.bodyTemperature.AddCold(notFrozenWater);
                        eatable.waterValue -= notFrozenWater;
                        //AddDebug("new waterValue " + eatable.waterValue);
                        eatable.timeDecayPause = eatable.waterValue;
                        eatable.timeDecayStart = eatable.timeDecayPause;
                    }
                }
                __result = eatable.removeOnUse;
                return false;
            }

        }
        
        [HarmonyPatch(typeof(Eatable))]
        class Eatable_patch
        {
            /*
            //[HarmonyPrefix]
            //[HarmonyPatch("StartDespawnInvoke")]
            static bool StartDespawnInvokePrefix(Eatable __instance)
            {
                __instance.InvokeRepeating("IterateDespawn", 1f, 1f);
                return false;
            }
            */
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            static bool AwakePrefix(Eatable __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogDebug("Eatable Awake " + tt);
                if (__instance.GetComponent<SnowBall>())
                {
                    __instance.decomposes = true;
                }
                else if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    if (__instance.timeDecayPause > 0f)
                    {
                        __instance.waterValue = __instance.timeDecayPause;
                        //AddDebug(__instance.name + " used water " + __instance.waterValue);
                    }
                    return false;
                }
                if (decayingFood.Contains(CraftData.GetTechType(__instance.gameObject)))
                {
                    __instance.decomposes = true;
                }
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void AwakePostfix(Eatable __instance)
            {
                //AddDebug("Eatable awake " + __instance.gameObject.name);
                //Main.Log("Eatable awake " + __instance.gameObject.name + " decomposes "+ __instance.decomposes);
                //__instance.kDecayRate *= .5f;
                //string tt = CraftData.GetTechType(__instance.gameObject).AsString();
                //Main.Log("Eatable awake " + tt );
                //Main.Log("kDecayRate " + __instance.kDecayRate);
                //Main.Log("waterValue " + __instance.waterValue);
                //Creature creature = __instance.GetComponent<Creature>();

                if (Util.IsFood(__instance))
                {
                    //AddDebug(__instance.name + " kDecayRate " + __instance.kDecayRate);
                    __instance.kDecayRate *= ConfigMenu.foodDecayRateMult.Value;
                }
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    __instance.decomposes = true;
                    //__instance.kDecayRate = waterFreezeRate;
                    //__instance.SetDecomposes(true);
                    //__instance.PauseDecay();
                    __instance.StartDespawnInvoke();
                }
                if (ConfigMenu.fishFoodWaterRatio.Value > 0)
                {
                    if (Util.IsEatableFish(__instance.gameObject) && __instance.foodValue > 0)
                        __instance.waterValue = __instance.foodValue * ConfigMenu.fishFoodWaterRatio.Value;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetDecomposes")]
            public static void SetDecomposesPrefix(Eatable __instance, ref bool value)
            { // SetDecomposes runs when fish killed
                if (Util.IsFood(__instance) && value && ConfigMenu.foodDecayRateMult.Value == 0)
                    value = false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetHealthValue")]
            public static void GetHealthValuePostfix(Eatable __instance, ref float __result)
            {
                if (__instance.GetComponent<SnowBall>())
                {
                    __result = 0f;
                }
                //else if (IsWater(__instance))
                //{
                //    __result = 0f;
                //}
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetFoodValue")]
            public static void GetFoodValuePostfix(Eatable __instance, ref float __result)
            {
                if (__instance.GetComponent<SnowBall>())
                    __result = 0f;
                else if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    if (__instance.GetWaterValue() > 0)
                        __result = __instance.foodValue;
                    else
                        __result = 0f;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("IterateDespawn")]
            static bool IterateDespawnPrefix(Eatable __instance)
            {
                if (uGUI.isLoading)
                    return false;
                //AddDebug(" IterateDespawn " + __instance.name);
                if (__instance.decomposes && __instance.foodValue > 0f)
                {
                    CheckFood(__instance);
                    //return false;
                }
                else if (__instance.GetComponent<SnowBall>())
                {
                    CheckSnowball(__instance);
                    return false;
                }
                else if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    CheckWater(__instance);
                    return false;
                }
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetSecondaryTooltip")]
            public static void GetSecondaryTooltipPostfix(Eatable __instance, ref string __result)
            {
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                    __result = "";
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetDecayValue")]
            public static void GetDecayValuePostfix(Eatable __instance, ref float __result)
            {
                //AddDebug(__instance.name + " GetDecayValue " );
                if (ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(__instance))
                {
                    //AddDebug(__instance.name + " Water GetDecayValue ");
                    __result = __instance.timeDecayStart;
                    //AddDebug(__instance.name + " GetDecayValue " + __result);
                }
            }


        }
         
        [HarmonyPatch(typeof(Fridge))]
        class Fridge_patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("AddItem")]
            static bool AddItemPrefix(Fridge __instance, InventoryItem item)
            { // dont touch timeDecayStart if water
                if (item == null || item.item == null)
                    return false;

                Eatable eatable = item.item.GetComponent<Eatable>();
                bool water = ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(eatable);
                if (eatable == null || water || !eatable.decomposes || !__instance.powerConsumer.IsPowered())
                    return false;

                eatable.PauseDecay();
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("RemoveItem")]
            static bool RemoveItemPrefix(Fridge __instance, InventoryItem item)
            { // dont touch timeDecayStart if water
                if (item == null || item.item == null)
                    return false;

                Eatable eatable = item.item.GetComponent<Eatable>();
                bool water = ConfigMenu.waterFreezeRate.Value > 0f && Util.IsWater(eatable);
                if (eatable == null || water || !eatable.decomposes)
                    return false;

                eatable.UnpauseDecay();
                return false;
            }
        }

        [HarmonyPatch(typeof(Plantable), "OnProtoDeserialize")]
        class Inventory_OnProtoDeserialize_patch
        {
            public static void Postfix(Plantable __instance)
            {
                //AddDebug(" OnProtoDeserialize " + __instance.plantTechType);
                if (!ConfigToEdit.canReplantMelon.Value)
                {
                    TechType tt = __instance.plantTechType;
                    if (tt == TechType.Melon || tt == TechType.SmallMelon || tt == TechType.JellyPlant)
                        Destroy(__instance);
                }
            }
        }

        //[HarmonyPatch(typeof(Inventory), "ConsumeResourcesForRecipe")]
        class Inventory_ConsumeResourcesForRecipe_patch
        {
            public static void Postfix(Inventory __instance, TechType techType)
            {
                //ITechData techData = CraftData.Get(techType);
                //if (techData == null)
                //    return;
                //int index = 0;
                //Main.Log("ConsumeResourcesForRecipe " + techType);
                //for (int ingredientCount = techData.ingredientCount; index < ingredientCount; ++index)
                //{
                //    IIngredient ingredient = techData.GetIngredient(index);
                //    TechType ingredientTT = ingredient.techType;
                //    Main.Log(" TechType " + ingredientTT);
                //}
            }
        }

        //[HarmonyPatch(typeof(CrafterLogic), "ConsumeResources")]
        class CrafterLogic_ConsumeResources_patch
        {
            public static void Postfix(CrafterLogic __instance, TechType techType)
            {
                //Util.Log("CrafterLogic ConsumeResources " + techType);
            }
        }

        //[HarmonyPatch(typeof(Crafter), "OnCraftingBegin")]
        class Crafter_OnCraftingBegin_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //Util.Log("Crafter OnCraftingBegin " + techType);
            }
        }

        //[HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //Util.Log("Crafter Craft " + techType);
            }
        }

        //[HarmonyPatch(typeof(uGUI_CraftingMenu), "Action")]
        class CraftingAnalytics_OnCraft_patch
        {
            //public static void Postfix(uGUI_CraftingMenu __instance, uGUI_CraftNode sender)
            //{
            //if (sender.action == TreeAction.Craft)
            //    AddDebug(" uGUI_CraftingMenu Craft " + sender.techType0);
            //    Main.Log(" uGUI_CraftingMenu  action " + sender.action);
            //    Main.Log(" uGUI_CraftingMenu techType0 " + sender.techType0);
            //}
        }
    }
}
