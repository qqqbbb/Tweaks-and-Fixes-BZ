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
        //static float waterValueMult = 1f;
        //static float foodValueMult = 1f;
        static float foodCons = .5f; // vanilla 0.4
        static float waterCons = .5f; // vanilla 0.55
        //static float updateHungerInterval { get { return Main.config.hungerUpdateInterval / DayNightCycle.main.dayNightSpeed; } }
        static float hungerUpdateTime = 0f;
        static float snowBallMeltRate = 0.05f;
        //static float waterFreezeRate = 0.03f;
        static float waterFreezeRate = 1f;

        public static bool IsWater(Eatable eatable)
        {
            return eatable.waterValue > 0f && eatable.foodValue <= 0f && eatable.GetComponent<SnowBall>() == null;
        }

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
            float temp = Main.GetTemperature(eatable.gameObject);
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
            float temp = Main.GetTemperature(eatable.gameObject);
            //AddDebug(eatable.name + " CheckWater " + eatable.timeDecayStart);
            if (temp < 0f)
            {
                //AddDebug(" freeze " + eatable.name);
                //eatable.UnpauseDecay();
                if (eatable.timeDecayStart < eatable.waterValue)
                    eatable.timeDecayStart += waterFreezeRate * DayNightCycle.main._dayNightSpeed;
                else if (eatable.timeDecayStart > eatable.waterValue)
                    eatable.timeDecayStart = eatable.waterValue;
            }
            else if (temp > 0f)
            {
                if (eatable.timeDecayStart > 0f)
                    eatable.timeDecayStart -= waterFreezeRate * DayNightCycle.main._dayNightSpeed;
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
            float temp = Main.GetTemperature(eatable.gameObject);
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
                if (Main.config.snowballWater > 0)
                {
                    Eatable eatable = __instance.gameObject.EnsureComponent<Eatable>();
                    eatable.kDecayRate = snowBallMeltRate;
                    eatable.decomposes = true;
                    eatable.waterValue = Main.config.snowballWater;
                    eatable.coldMeterValue = Main.config.snowballWater;
                    //AddDebug("SnowBall Awake waterValue " + eatable.waterValue);
                    __instance.GetComponent<WorldForces>().underwaterGravity = .5f;
                }
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
            __instance.food -= foodCons;
            __instance.water -= waterCons;
            if (Player_Movement.timeSprinted > 0f)
            {
                float sprintFoodCons = foodCons * Player_Movement.timeSprinted * Main.config.hungerUpdateInterval * .01f;
                //AddDebug("UpdateStats timeSprinted " + Player_Movement.timeSprinted);
                //AddDebug("UpdateStats sprintFoodCons " + sprintFoodCons);
                __instance.food -= sprintFoodCons;
                __instance.water -= sprintFoodCons;
                Player_Movement.timeSprintStart = 0f;
                Player_Movement.timeSprinted = 0f;
            }
            float foodDamage = 0f;
            if (__instance.food < -100f)
            {
                foodDamage = Mathf.Abs(__instance.food + 100f);
                __instance.food = -100f;
            }
            if (__instance.water < -100f)
            {
                foodDamage += Mathf.Abs(__instance.water + 100f);
                __instance.water = -100f;
            }
            if (foodDamage > 0)
                Player.main.liveMixin.TakeDamage(foodDamage, Player.main.gameObject.transform.position, DamageType.Starve);
            float threshold1 = Main.config.newHungerSystem ? 0f : 20f;
            float threshold2 = Main.config.newHungerSystem ? -50f : 10f;
            __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, oldFood, threshold1, threshold2);
            __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, oldWater, threshold1, threshold2);
            hungerUpdateTime = Time.time + Main.config.hungerUpdateInterval;

            //AddDebug("Invoke  hungerUpdateInterval " + Main.config.hungerUpdateInterval);
            //AddDebug("Invoke dayNightSpeed " + DayNightCycle.main.dayNightSpeed);
            //__instance.Invoke("UpdateHunger", updateHungerInterval);
        }

        [HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_patch
        {
            public static void Postfix(Player __instance)
            {
                if (!GameModeUtils.RequiresSurvival() || Main.survival.freezeStats || !Main.loadingDone)
                    return;

                if (hungerUpdateTime > Time.time)
                    return;

                if (Main.config.newHungerSystem)
                {
                    UpdateStats(Main.survival);
                    //__instance.Invoke("UpdateHunger", updateHungerInterval);
                    //AddDebug("updateHungerInterval " + updateHungerInterval);
                }
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
                        if (Main.IsEatableFishAlive(go))
                            Main.CookFish(go);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(PlayerTool), "Awake")]
        internal class PlayerTool_Patch
        {
            public static void Postfix(PlayerTool __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
   
                //if (Main.cooking)
                {
                    AddDebug(__instance.name + " Awake ");
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
                hungerUpdateTime = Time.time + Main.config.hungerUpdateInterval;
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
                int playerMinFood = Main.config.newHungerSystem ? -100 : 0;
                float playerMaxWater = Main.config.newHungerSystem ? 200f : 100f;
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
                if (Main.IsEatableFish(useObj))
                {
                    if (food > 0)
                    {
                        if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla)
                        {
                            minFood = food;
                            maxFood = food;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                        {
                            minFood = 0;
                            maxFood = food;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                        {
                            minFood = -food;
                            maxFood = food;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmful)
                        {
                            minFood = -food;
                            maxFood = 0;
                        }
                    }
                    if (water > 0)
                    {
                        if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla)
                        {
                            minWater = water;
                            maxWater = water;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                        {
                            minWater = 0;
                            maxWater = water;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                        {
                            minWater = -water;
                            maxWater = water;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmful)
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
                if (Main.config.newHungerSystem && __instance.food > 100f && finalFood > 0)
                {
                    float mult = (200f - __instance.food) * .01f;
                    finalFood *= mult;
                }
                if (Main.config.newHungerSystem && __instance.water > 100f && finalWater > 0)
                {
                    float mult = (200f - __instance.water) * .01f;
                    finalWater *= mult;
                    //AddDebug("newHungerSystem finalWater " + finalWater);
                }
                if (finalWater < 0 && __instance.water + finalWater < playerMinFood)
                {
                    int waterDamage = (int)(__instance.water + finalWater - playerMinFood);
                    //AddDebug("waterDamage " + waterDamage);
                    Player.main.liveMixin.TakeDamage(Mathf.Abs(waterDamage), Player.main.gameObject.transform.position, DamageType.Starve);
                }
                if (finalFood < 0 && __instance.food + finalFood < playerMinFood)
                {
                    int foodDamage = (int)(__instance.food + finalFood - playerMinFood);
                    //AddDebug("foodDamage " + foodDamage);
                    Player.main.liveMixin.TakeDamage(Mathf.Abs(foodDamage), Player.main.gameObject.transform.position, DamageType.Starve);
                }
                if (Main.config.foodTweaks && __instance.bodyTemperature.isExposed)
                {
                    //AddDebug("eating  isExposed ");
                    __instance.bodyTemperature.AddCold(5f);
                }
                //AddDebug("finalFood " + finalFood);
                //AddDebug("finalWater " + finalWater);

                if (finalFood > 0)
                    GoalManager.main.OnCustomGoalEvent("Eat_Something");
                if (finalWater > 0)
                    GoalManager.main.OnCustomGoalEvent("Drink_Something");

                __instance.onEat.Trigger((float)finalFood);
                __instance.food += finalFood;
                __instance.onDrink.Trigger((float)finalWater);
                __instance.water += finalWater;
                if (healthValue != 0)
                {
                    //AddDebug("healthValue " + healthValue);
                    if (healthValue > 0)
                    {
                        __instance.liveMixin.AddHealth(healthValue);
                        GoalManager.main.OnCustomGoalEvent("Heal_Damage");
                    }
                    else if (healthValue <= -1.0)
                        __instance.liveMixin.TakeDamage(-healthValue, type: DamageType.FoodPoison);
                }
                if (coldMeterValue != 0)
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

                int warn = Main.config.newHungerSystem ? 0 : 20;
                if (!__instance.InConversation())
                {
                    if (finalWater > 0 && __instance.water > warn && __instance.water - finalWater < warn)
                        __instance.vitalsOkNotification.Play();
                    else if (finalFood > 0 && __instance.food > warn && __instance.food - finalWater < warn)
                        __instance.vitalsOkNotification.Play();
                }
                if (IsWater(eatable))
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
            //[HarmonyPrefix]
            //[HarmonyPatch("StartDespawnInvoke")]
            static bool StartDespawnInvokePrefix(Eatable __instance)
            {
                __instance.InvokeRepeating("IterateDespawn", 1f, 1f);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            static bool AwakePrefix(Eatable __instance)
            {
                if (__instance.GetComponent<SnowBall>())
                {
                    __instance.decomposes = true;
                }
                else if (IsWater(__instance))
                {
                    if (__instance.timeDecayPause > 0f)
                    {
                        __instance.waterValue = __instance.timeDecayPause;
                        //AddDebug(__instance.name + " used water " + __instance.waterValue);
                    }
                    return false;
                }
                return true;
            }
            
            [HarmonyPostfix]
            [HarmonyPatch( "Awake")]
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
                if (__instance.decomposes)
                {
                    //AddDebug(__instance.name + " kDecayRate " + __instance.kDecayRate);
                    __instance.kDecayRate *= Main.config.foodDecayRateMult;
                }
                else if (IsWater(__instance))
                {
                    __instance.decomposes = true;
                    //__instance.kDecayRate = waterFreezeRate;
                    //__instance.SetDecomposes(true);
                    //__instance.PauseDecay();
                    __instance.StartDespawnInvoke();
                }
                if (Main.config.foodTweaks)
                {
                    if (Main.IsEatableFish(__instance.gameObject) && __instance.foodValue > 0)
                    {
                        __instance.waterValue = __instance.foodValue * .5f;
                    }
                    //else if (__instance.decomposes)
                    //{
                        //if (Main.IsEatableFish(__instance.gameObject))
                        //{
                            //AddDebug("dead Fish " + __instance.gameObject.name);
                        //    __instance.waterValue = Mathf.Abs(__instance.foodValue) * .5f;
                        //}

                        //Main.Log(tt + " decomposes " + __instance.kDecayRate);
                        //Main.Log(tt + " decomposes half" + __instance.kDecayRate);
                    //}
                }
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
                else if (IsWater(__instance))
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
                if (!Main.loadingDone)
                    return false;
                //AddDebug(" IterateDespawn " + __instance.name);
                if (__instance.decomposes && __instance.foodValue > 0f)
                {
                    CheckFood(__instance);
                    //return false;
                }
                else if(__instance.GetComponent<SnowBall>())
                {
                    CheckSnowball(__instance);
                    return false;
                }
                else if (IsWater(__instance))
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
                if (IsWater(__instance))
                    __result = "";
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("GetWaterValue")]
            public static void GetWaterValuePostfix(Eatable __instance, ref float __result)
            {
                //if (IsWater(__instance))
                {
                    //if (__result > __instance.waterValue)
                    //    __result = __instance.waterValue;
                    //else if (__result < 0f)
                    //    __result = 0f;
                    //__result = __instance.waterValue - __instance.timeDecayPause;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetDecayValue")]
            public static void GetDecayValuePostfix(Eatable __instance, ref float __result)
            {
                //AddDebug(__instance.name + " GetDecayValue " );
                if (IsWater(__instance))
                {
                    //AddDebug(__instance.name + " Water GetDecayValue ");
                    __result = __instance.timeDecayStart;
                    //AddDebug(__instance.name + " GetDecayValue " + __result);
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
                //TechType tt = item.item.GetTechType();
                Main.Log("CrafterLogic ConsumeResources " + techType);
            }
        }

        //[HarmonyPatch(typeof(Crafter), "OnCraftingBegin")]
        class Crafter_OnCraftingBegin_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //TechType tt = item.item.GetTechType();
                Main.Log("Crafter OnCraftingBegin " + techType);
            }
        }

        //[HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //TechType tt = item.item.GetTechType();
                Main.Log("Crafter Craft " + techType);
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
