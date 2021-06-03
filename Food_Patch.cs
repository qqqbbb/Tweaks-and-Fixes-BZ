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
        static float waterValueMult = 1f;
        static float foodValueMult = 1f;
        static float foodCons = .5f; // vanilla 0.4
        static float waterCons = .5f; // vanilla 0.55
        static float updateHungerInterval { get { return Main.config.hungerUpdateInterval / DayNightCycle.main.dayNightSpeed; } }
        static float hungerUpdateTime = 0f;

        public static void UpdateStats(Survival __instance)
        {
            //AddDebug("dayNightSpeed  " + DayNightCycle.main.dayNightSpeed);
            //AddDebug("UpdateStats  " + updateHungerInterval);
            float oldFood = __instance.food;
            float oldWater = __instance.water;

            __instance.food -= foodCons;
            __instance.water -= waterCons;
            if (__instance.food < -100f)
            {
                float damage = Mathf.Abs(__instance.food + 100f);
                __instance.food = -100f;
                Player.main.liveMixin.TakeDamage(damage, Player.main.gameObject.transform.position, DamageType.Starve);
            }
            if (__instance.water < -100f)
            {
                float damage = Mathf.Abs(__instance.water + 100f);
                __instance.water = -100f;
                Player.main.liveMixin.TakeDamage(damage, Player.main.gameObject.transform.position, DamageType.Starve);
            }
            float threshold1 = Main.config.newHungerSystem ? 0f : 20f;
            float threshold2 = Main.config.newHungerSystem ? -50f : 10f;
            __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, oldFood, threshold1, threshold2);
            __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, oldWater, threshold1, threshold2);
            hungerUpdateTime = Time.time + updateHungerInterval;

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

        [HarmonyPatch(typeof(Survival), "Start")]
        class Survival_Start_patch
        {
            public static void Postfix(Survival __instance)
            { // cancel UpdateHunger
                __instance.CancelInvoke();
                //if (Main.config.replaceHungerDamage)
                //{
                //AddDebug("Survival Start");
                //__instance.Invoke("UpdateHunger", updateHungerInterval);
                //AddDebug("updateHungerInterval " + updateHungerInterval);
                //}
            }
        }

        //[HarmonyPatch(typeof(Survival), "UpdateHunger")]
        internal class Survival_UpdateHunger_Patch
        {
            internal static bool Prefix(Survival __instance)
            {
                //AddDebug("UpdateHunger ");
                if (Main.config.newHungerSystem)
                {
                    //UpdateStats(__instance);
                    return false;
                }
                //if (Main.config.hungerUpdateInterval != 10)
                //    return false;
                //if (!GameModeUtils.RequiresSurvival())
                //    return false;

                //AddDebug("kUpdateHungerInterval " + __instance.kUpdateHungerInterval);
                //__instance.UpdateStats(Main.config.hungerUpdateInterval);
                //UpdateStats(__instance);
                return true;
            }
        }

        [HarmonyPatch(typeof(Survival), "UpdateHunger")]
        internal class Survival_UpdateHunger_Postfix_Patch
        {
            internal static void Postfix(Survival __instance)
            {
                //AddDebug("UpdateHunger ");
                hungerUpdateTime = Time.time + updateHungerInterval;
            }
        }

        //[HarmonyPatch(typeof(Survival), "UpdateStats")]
        class Survival_UpdateStats_patch
        {
            public static bool Prefix(Survival __instance)
            {
                if (!Main.config.newHungerSystem)
                    return true;

                //UpdateStats(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(Survival), "Eat")]
        class Survival_Eat_patch
        {
            static System.Random rndm = new System.Random();

            public static bool Prefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla && !Main.config.newHungerSystem && !Main.config.foodTweaks)
                    return true;

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
                int rndFood = rndm.Next(minFood, maxFood);
                float finalFood = Mathf.Min(food, rndFood);
                int rndWater = rndm.Next(minWater, maxWater);
                float finalWater = Mathf.Min(water, rndWater);
                if (Main.config.newHungerSystem && __instance.food > 100f && finalFood > 0)
                {
                    float mult = (200f - __instance.food) * .01f;
                    finalFood *= mult;
                }
                if (Main.config.newHungerSystem && __instance.water > 100f && finalWater > 0)
                {
                    float mult = (200f - __instance.water) * .01f;
                    finalWater *= mult;
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
                    __instance.bodyTemperature.AddCold(5f);

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
                    //AddDebug("coldMeterValue " + coldMeterValue);
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
                //}
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
                __result = eatable.removeOnUse;
                return false;
            }
        }

        [HarmonyPatch(typeof(Eatable), "Awake")]
        class Eatable_Awake_patch
        {
            public static void Postfix(Eatable __instance)
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
                    __instance.kDecayRate *= Main.config.foodDecayRateMult;
                }
                if (Main.config.foodTweaks)
                {
                    if (Main.crafterOpen)
                    { // food values not saved and will reset after reload
                        //AddDebug("Awake waterValueMult " + waterValueMult);
                        //AddDebug("Awake foodValueMult " + foodValueMult);
                        //if (waterValueMult != 0)
                        __instance.waterValue *= waterValueMult;
                        //if (foodValueMult != 0)
                        __instance.foodValue *= foodValueMult;
                    }
                    else if (Main.IsEatableFish(__instance.gameObject) && __instance.foodValue > 0)
                    {
                        __instance.waterValue = Mathf.Abs(__instance.foodValue) * .5f;
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
        }

        [HarmonyPatch(typeof(ItemsContainer), "NotifyRemoveItem")]
        class ItemsContainer_NotifyRemoveItem_patch
        {
            public static void Postfix(ItemsContainer __instance, InventoryItem item)
            {
                //if (crafterOpem && Inventory.main._container == __instance)
                if (Main.config.foodTweaks && Main.crafterOpen)
                { // cooking fish
                    //TechType tt = item.item.GetTechType();
                    if (Main.IsEatableFish(item.item.gameObject))
                    {
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        //AddDebug(" NotifyRemoveItem waterValue " + eatable.GetWaterValue() + " " + eatable.GetFoodValue());
                        waterValueMult = eatable.GetWaterValue() / eatable.waterValue;
                        foodValueMult = eatable.GetFoodValue() / eatable.foodValue;
                    }
                    else
                    {
                        waterValueMult = 1f;
                        foodValueMult = 1f;
                    }
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
