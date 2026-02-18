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
        static float foodLowScalar = SurvivalConstants.kLowFoodThreshold / 100f;
        static float waterLowScalar = SurvivalConstants.kLowWaterThreshold / 100f;
        static float foodCriticalScalar = SurvivalConstants.kCriticalFoodThreshold / 100f;
        static float waterCriticalScalar = SurvivalConstants.kCriticalWaterThreshold / 100f;

        public static float UpdateStats(Survival survival, float timePassed)
        {
            if (timePassed < Mathf.Epsilon)
                return 0;

            float oldFood = survival.food;
            float oldWater = survival.water;
            float foodToLose = timePassed / SurvivalConstants.kFoodTime * SurvivalConstants.kMaxStat;
            float waterToLose = timePassed / SurvivalConstants.kWaterTime * SurvivalConstants.kMaxStat;
            float minFood = ConfigToEdit.starvationThreshold.Value;
            float minWater = ConfigToEdit.dehydrationThreshold.Value;
            if (ConfigToEdit.foodLossMultSprint.Value > 1 && Player.main.mode == Player.Mode.Normal && Player.main.IsUnderwaterForSwimming() == false && Player.main.groundMotor.IsGrounded() && Player.main.groundMotor.IsSprinting())
            {
                foodToLose *= ConfigToEdit.foodLossMultSprint.Value;
                waterToLose *= ConfigToEdit.foodLossMultSprint.Value;
            }
            //AddDebug("UpdateStats foodToLose " + foodToLose);
            survival.food -= foodToLose * ConfigMenu.foodLossMult.Value;
            survival.water -= waterToLose * ConfigMenu.waterLossMult.Value;
            float starveDamage = 0;

            if (survival.food < minFood)
            {
                starveDamage = ConfigToEdit.starveDamage.Value;
                survival.food = minFood;
            }
            else if (survival.water < minWater)
            {
                starveDamage = ConfigToEdit.starveDamage.Value;
                survival.water = minWater;
            }
            float foodLowThreshold = Mathf.Lerp(minFood, ConfigToEdit.playerFullFood.Value, foodLowScalar);
            float waterLowThreshold = Mathf.Lerp(minWater, ConfigToEdit.playerFullWater.Value, waterLowScalar);
            float foodCriticalThreshold = Mathf.Lerp(minFood, ConfigToEdit.playerFullFood.Value, foodCriticalScalar);
            float waterCriticalThreshold = Mathf.Lerp(minWater, ConfigToEdit.playerFullWater.Value, foodCriticalScalar);
            survival.UpdateWarningSounds(survival.foodWarningSounds, survival.food, oldFood, foodLowThreshold, foodCriticalThreshold);
            survival.UpdateWarningSounds(survival.waterWarningSounds, survival.water, oldWater, waterLowThreshold, waterCriticalThreshold);
            return starveDamage;
        }

        [HarmonyPatch(typeof(Survival))]
        internal class Survival_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            static void StartPostfix(Survival __instance)
            {
                if (ConfigToEdit.consistentHungerUpdateTime.Value)
                {
                    __instance.CancelInvoke();
                    __instance.StartCoroutine(UpdateHunger(__instance));
                }
            }

            static IEnumerator UpdateHunger(Survival survival)
            {
                while (ConfigToEdit.consistentHungerUpdateTime.Value)
                {
                    yield return new WaitForSeconds(GetHungerUpdateTime(survival));
                    //AddDebug("UpdateHunger");
                    survival.UpdateHunger();
                }
            }

            private static float GetHungerUpdateTime(Survival survival)
            {
                return survival.kUpdateHungerInterval / DayNightCycle.main._dayNightSpeed;
            }

            public static float GetfoodWaterHealThreshold()
            {
                return ConfigMenu.foodHealThreshold.Value;
            }

            static public float GetFishFoodValue(float food)
            {
                if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.TF_eat_raw_fish_setting_harmless || food <= 0)
                    return food;

                float min = 0, max = 0;
                if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.TF_eat_raw_fish_setting_harmless)
                {
                    min = 0;
                    max = food;
                }
                else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.TF_eat_raw_fish_setting_risky)
                {
                    min = -food;
                    max = food;
                }
                else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.TF_eat_raw_fish_setting_harmful)
                {
                    min = -food;
                    max = 0;
                }
                return UnityEngine.Random.Range(min, max);
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

            [HarmonyPrefix, HarmonyPatch("UpdateHunger")]
            static bool UpdateHungerPrefix(Survival __instance)
            {
                if (Main.gameLoaded == false || GameModeManager.GetOption<bool>(GameOption.Hunger) == false && GameModeManager.GetOption<bool>(GameOption.Thirst) == false)
                    return false;

                return true;
            }

            [HarmonyPrefix, HarmonyPatch("UpdateStats")]
            static bool UpdateStatsPrefix(Survival __instance, ref float timePassed, ref float __result)
            {
                //AddDebug("UpdateStats " + __instance.required);
                __result = UpdateStats(__instance, timePassed);
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("Eat")]
            public static bool EatPrefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                if (useObj == null)
                    return false;

                Eatable eatable = useObj.GetComponent<Eatable>();
                if (eatable == null)
                    return false;

                bool canBeUsed = eatable.maxCharges == 0 || eatable.charges > 0;
                if (!canBeUsed)
                    return false;

                //AddDebug("Eat " + eatable.name);
                float food = eatable.foodValue;
                float water = eatable.waterValue;
                float playerMinFood = ConfigToEdit.starvationThreshold.Value;
                float playerMinWater = ConfigToEdit.dehydrationThreshold.Value;
                float playerMaxWater = ConfigToEdit.PlayerMaxWater.Value;
                float playerFullWater = ConfigToEdit.playerFullWater.Value;
                float playerMaxFood = ConfigToEdit.playerMaxFood.Value;
                float playerFullFood = ConfigToEdit.playerFullFood.Value;
                float healthValue = eatable.GetHealthValue();
                float coldMeterValue = eatable.GetColdMeterValue();
                //AddDebug($"playerMinFood {playerMinFood} playerMaxFood {playerMaxFood}");

                TechType techType = CraftData.GetTechType(useObj);
                if (techType == TechType.None)
                {
                    if (useObj.TryGetComponent(out Pickupable p))
                        techType = p.GetTechType();
                }
                if (Util.IsRawFish(useObj))
                {
                    food = GetFishFoodValue(food);
                    water = GetFishFoodValue(water);
                }
                if (food > 0 && __instance.food > playerFullFood && playerFullFood < playerMaxFood)
                {
                    float mult = (playerMaxFood - __instance.food) * .01f;
                    food *= mult;
                }
                if (water > 0 && __instance.water > playerFullWater && playerFullWater < playerMaxWater)
                {
                    float mult = (playerMaxWater - __instance.water) * .01f;
                    water *= mult;
                }
                __instance.onEat.Trigger(food);
                __instance.food += food;
                __instance.onDrink.Trigger(water);
                __instance.water += water;
                //AddDebug($"food {food} water {water} ");
                __instance.water = Mathf.Clamp(__instance.water, playerMinWater, playerMaxWater);
                __instance.food = Mathf.Clamp(__instance.food, playerMinFood, playerMaxFood);
                if (eatable.maxCharges > 0)
                    eatable.ConsumeCharge();

                if (food > 0)
                    GoalManager.main.OnCustomGoalEvent("Eat_Something");
                if (water > 0)
                    GoalManager.main.OnCustomGoalEvent("Drink_Something");

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
                if (techType == TechType.Bladderfish && GameModeManager.GetOption<bool>(GameOption.OrganicOxygenSources))
                    Player.main.GetComponent<OxygenManager>().AddOxygen(SurvivalConstants.kBladderFishO2OnEat);

                if (!__instance.InConversation())
                {
                    float foodOkThreshold = Mathf.Lerp(playerMinFood, playerMaxFood, foodLowScalar);
                    float waterOkThreshold = Mathf.Lerp(playerMinWater, playerMaxWater, waterLowScalar);
                    if (water > 0 && __instance.water > waterOkThreshold && __instance.water - water < waterOkThreshold)
                        __instance.vitalsOkNotification.Play();
                    else if (food > 0 && __instance.food > foodOkThreshold && __instance.food - food < foodOkThreshold)
                        __instance.vitalsOkNotification.Play();

                    FMODAsset useSound = __instance.player.GetUseSound(TechData.GetSoundType(techType));
                    if (eatable.IsRotten())
                        useSound = __instance.ateRottenFoodSound;

                    if (useSound)
                        Utils.PlayFMODAsset(useSound, __instance.player.transform.position);
                }
                if (ConfigMenu.waterFreezeRate.Value > 0)
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
                if (ConfigToEdit.eatingOutsideCold.Value > 0 && __instance.bodyTemperature.isExposed)
                {
                    //AddDebug("eating  isExposed ");
                    __instance.bodyTemperature.AddCold(ConfigToEdit.eatingOutsideCold.Value);
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
                    Main.configMain.SetHPtoHeal(ConfigMenu.medKitHP.Value);
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

                float hpToHeal = ConfigMain.GetHPtoHeal();
                if (hpToHeal > 0 && Time.time > healTime)
                {
                    healTime = Time.time + 1f;
                    __instance.liveMixin.AddHealth(ConfigToEdit.medKitHPperSecond.Value);
                    //AddDebug("AddHealth " + Main.config.medKitHPperSecond);
                    Main.configMain.SetHPtoHeal(hpToHeal - ConfigToEdit.medKitHPperSecond.Value);
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
