using FMOD.Studio;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Survival_
    {
        static bool updatingStats;
        private static bool usingMedkit;
        public static float healTime = 0f;
        private const float defaultWaterTemp = 6f;

        public static float UpdateStats(Survival survival, float timePassed)
        {
            float oldFood = survival.food;
            float oldWater = survival.water;
            float foodToLose = timePassed / SurvivalConstants.kFoodTime * SurvivalConstants.kMaxStat;
            float waterToLose = timePassed / SurvivalConstants.kWaterTime * SurvivalConstants.kMaxStat;

            if (Player.main.mode == Player.Mode.Normal && Player.main.IsUnderwaterForSwimming() == false && Player.main.groundMotor.IsGrounded() && Player.main.groundMotor.IsSprinting())
            {
                foodToLose *= 2;
                waterToLose *= 2;
            }
            //AddDebug("UpdateStats foodToLose " + foodToLose);
            survival.food -= foodToLose * ConfigMenu.foodLossMult.Value;
            survival.water -= waterToLose * ConfigMenu.waterLossMult.Value;
            float foodDamage = 0f;

            if (survival.food < -100f)
            {
                foodDamage = Mathf.Abs(survival.food + 100f);
                survival.food = -100f;
            }
            if (survival.water < -100f)
            {
                foodDamage += Mathf.Abs(survival.water + 100f);
                survival.water = -100f;
            }
            //if (foodDamage > 0)
            //    Player.main.liveMixin.TakeDamage(foodDamage, Player.main.gameObject.transform.position, DamageType.Starve);

            float threshold1 = ConfigMenu.newHungerSystem.Value ? 0f : 20f;
            float threshold2 = ConfigMenu.newHungerSystem.Value ? -50f : 10f;
            survival.UpdateWarningSounds(survival.foodWarningSounds, survival.food, oldFood, threshold1, threshold2);
            survival.UpdateWarningSounds(survival.waterWarningSounds, survival.water, oldWater, threshold1, threshold2);
            //hungerUpdateTime = Time.time + ConfigMenu.hungerUpdateInterval.Value;
            //AddDebug("Invoke  hungerUpdateInterval " + Main.config.hungerUpdateInterval);
            //AddDebug("Invoke dayNightSpeed " + DayNightCycle.main.dayNightSpeed);
            //__instance.Invoke("UpdateHunger", updateHungerInterval);
            return foodDamage;
        }

        [HarmonyPatch(typeof(Survival))]
        internal class Survival_Patch
        {
            static float foodBeforeUpdate;
            static float waterBeforeUpdate;

            [HarmonyPrefix, HarmonyPatch("UpdateWarningSounds")]
            static bool UpdateWarningSoundsPrefix(Survival __instance)
            {

                //AddDebug("UpdateWarningSounds ");
                if (ConfigMenu.foodLossMult.Value == 1 && ConfigMenu.waterLossMult.Value == 1)
                    return true;

                return !updatingStats;
            }

            public static float GetfoodWaterHealThreshold()
            {
                return ConfigMenu.foodWaterHealThreshold.Value;
            }

            [HarmonyPatch("UpdateHunger")]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeMatcher = new CodeMatcher(instructions)
             .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, SurvivalConstants.kFoodWaterHealThreshold))
             .ThrowIfInvalid("Could not find Ldc_R4 SurvivalConstants.kFoodWaterHealThreshold in UpdateHunger")
             .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<float>>(GetfoodWaterHealThreshold))
             .InstructionEnumeration();
                return codeMatcher;
            }


            [HarmonyPrefix, HarmonyPatch("UpdateStats")]
            static bool UpdateStatsPrefix(Survival __instance, float timePassed, ref float __result)
            {
                //if (ConfigMenu.foodLossMult.Value == 1 && ConfigMenu.waterLossMult.Value == 1)
                //    return true;

                //AddDebug("UpdateStats ");
                updatingStats = true;
                foodBeforeUpdate = __instance.food;
                waterBeforeUpdate = __instance.water;
                if (ConfigMenu.newHungerSystem.Value)
                {
                    __result = UpdateStats(__instance, timePassed);
                    return false;
                }
                return true;
            }

            [HarmonyPostfix, HarmonyPatch("UpdateStats")]
            static void UpdateStatsPostfix(Survival __instance, float timePassed, ref float __result)
            {
                if (ConfigMenu.foodLossMult.Value == 1 && ConfigMenu.waterLossMult.Value == 1)
                    return;

                float damage = 0;
                if (timePassed > Mathf.Epsilon)
                {
                    //float foodLost = foodBeforeUpdate - __instance.food;
                    //float waterLost = waterBeforeUpdate - __instance.water;
                    float foodToLose = (timePassed / SurvivalConstants.kFoodTime * SurvivalConstants.kMaxStat);
                    foodToLose *= ConfigMenu.foodLossMult.Value;
                    if (foodToLose > foodBeforeUpdate)
                        damage += ((foodToLose - foodBeforeUpdate) * SurvivalConstants.kStarveDamage);

                    __instance.food = Mathf.Clamp(foodBeforeUpdate - foodToLose, 0, SurvivalConstants.kMaxStat * 2f);
                    float waterToLose = (timePassed / SurvivalConstants.kWaterTime * SurvivalConstants.kMaxStat);
                    waterToLose *= ConfigMenu.waterLossMult.Value;
                    //AddDebug("foodToLose " + foodToLose);
                    //AddDebug("waterToLose " + waterToLose);
                    if (waterToLose > waterBeforeUpdate)
                        damage += ((waterToLose - waterBeforeUpdate) * SurvivalConstants.kStarveDamage);

                    __instance.water = Mathf.Clamp(waterBeforeUpdate - waterToLose, 0, SurvivalConstants.kMaxStat);
                    updatingStats = false;
                    __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, foodBeforeUpdate, SurvivalConstants.kLowFoodThreshold, SurvivalConstants.kCriticalFoodThreshold);
                    __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, waterBeforeUpdate, SurvivalConstants.kLowWaterThreshold, SurvivalConstants.kCriticalWaterThreshold);
                }
                //AddDebug("UpdateStats food " + __instance.food);
                //AddDebug("UpdateStats water " + __instance.water);
                __result = damage;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Eat")]
            public static bool EatPrefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                //AddDebug("Survival eat " + useObj.name);
                //if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Vanilla && !Main.config.newHungerSystem && ConfigMenu.maxPlayerWater.Value == 100 && ConfigMenu.maxPlayerFood.Value == 200)
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
                float playerMaxWater = ConfigMenu.maxPlayerWater.Value;
                float playerMaxFood = ConfigMenu.maxPlayerFood.Value;
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

                __instance.water = Mathf.Clamp(__instance.water, playerMinFood, playerMaxWater);
                __instance.food = Mathf.Clamp(__instance.food, playerMinFood, playerMaxFood);
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

            [HarmonyPrefix, HarmonyPatch("Use")]
            public static void UsePrefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                TechType techType = CraftData.GetTechType(useObj);
                //Main.logger.LogMessage("Survival Use " + techType);
                if (techType == TechType.FirstAidKit)
                    usingMedkit = true;
            }
        }

        [HarmonyPatch(typeof(LiveMixin), "AddHealth")]
        class LiveMixin_AddHealth_patch
        {
            public static bool Prefix(LiveMixin __instance, ref float healthBack, ref float __result)
            {
                if (usingMedkit == false)
                    return true;

                usingMedkit = false;
                if (ConfigToEdit.medKitHPperSecond.Value >= ConfigMenu.medKitHP.Value)
                    healthBack = ConfigMenu.medKitHP.Value;
                else
                {
                    Main.configMain.medKitHPtoHeal = ConfigMenu.medKitHP.Value;
                    healTime = Time.time;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            { // not checking savegame slot
                if (!Main.gameLoaded)
                    return;

                if (Main.configMain.medKitHPtoHeal > 0 && Time.time > healTime)
                {
                    healTime = Time.time + 1f;
                    __instance.liveMixin.AddHealth(ConfigToEdit.medKitHPperSecond.Value);
                    Main.configMain.medKitHPtoHeal -= ConfigToEdit.medKitHPperSecond.Value;
                    if (Main.configMain.medKitHPtoHeal < 0)
                        Main.configMain.medKitHPtoHeal = 0;

                    //AddDebug("Player Update heal " + Main.config.medKitHPperSecond);
                    //AddDebug("Player Update medKitHPtoHeal " + Main.config.medKitHPtoHeal);
                    //Main.config.Save();
                }
            }
        }


        [HarmonyPatch(typeof(WaterTemperatureSimulation), "GetTemperature", new Type[] { typeof(Vector3), typeof(float) }, new[] { ArgumentType.Normal, ArgumentType.Out })]
        class WaterTemperatureSimulation_GetTemperature_PrefixPatch
        {
            public static bool Prefix(WaterTemperatureSimulation __instance, ref float __result, Vector3 wsPos, ref float posBaseTemperature)
            {
                if (ConfigToEdit.warmKelpWater.Value)
                    return true;

                float baseTemperature = defaultWaterTemp;
                WaterBiomeManager waterBiomeManager = WaterBiomeManager.main;
                WaterscapeVolume.Settings settings = null;
                if (waterBiomeManager && waterBiomeManager.GetSettings(wsPos, false, out settings))
                    baseTemperature = settings.temperature;

                if (ConfigToEdit.warmKelpWater.Value == false)
                {
                    int biomeIndex = -1;
                    if (LargeWorld.main)
                    {
                        biomeIndex = waterBiomeManager.GetBiomeIndex(waterBiomeManager.GetBiome(wsPos, false));
                        if (biomeIndex >= 0 && biomeIndex < waterBiomeManager.biomeSettings.Count)
                        {
                            WaterBiomeManager.BiomeSettings biomeSettings = waterBiomeManager.biomeSettings[biomeIndex];
                            //AddDebug("GetTemperature biomeSettings " + biomeSettings.name);
                            if (biomeSettings.name == "arcticKelp")
                                baseTemperature = defaultWaterTemp;
                        }
                    }
                    //string temp = "";
                    //if (settings != null)
                    //    temp = ((int)settings.temperature).ToString();
                    //AddDebug("GetTemperature waterBiomeManager settings.temperature " + temp);
                }
                EcoRegionManager ecoRegionManager = EcoRegionManager.main;
                if (ecoRegionManager != null)
                {
                    float distance;
                    IEcoTarget nearestTarget = ecoRegionManager.FindNearestTarget(EcoTargetType.HeatArea, wsPos, out distance, null, 3);
                    if (nearestTarget != null)
                    {
                        float num = Mathf.Clamp(60f - distance, 0f, 60f);
                        baseTemperature += num;
                        Debug.DrawLine(wsPos, nearestTarget.GetPosition(), Color.red, 5f);
                    }
                }
                posBaseTemperature = baseTemperature;
                __result = __instance.GetFinalTemperature(baseTemperature, wsPos);
                return false;
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
