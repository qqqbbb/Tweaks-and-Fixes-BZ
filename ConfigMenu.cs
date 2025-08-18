using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class ConfigMenu
    {

        public static ConfigEntry<KeyCode> transferAllItemsButton;
        public static ConfigEntry<KeyCode> transferSameItemsButton;
        public static ConfigEntry<KeyCode> quickslotButton;
        public static ConfigEntry<KeyCode> nextPDATabKey;
        public static ConfigEntry<KeyCode> previousPDATabKey;
        public static ConfigEntry<float> timeFlowSpeed;
        public static ConfigEntry<float> seaglideSpeedMult;
        public static ConfigEntry<float> playerWaterSpeedMult;
        public static ConfigEntry<float> playerGroundSpeedMult;

        public static ConfigEntry<float> exosuitSpeedMult;
        public static ConfigEntry<float> seatruckSpeedMult;
        public static ConfigEntry<float> hoverbikeSpeedMult;

        public static ConfigEntry<bool> useRealTempForPlayerTemp;
        public static ConfigEntry<float> coldMult;
        public static ConfigEntry<float> oxygenPerBreath;
        public static ConfigEntry<float> toolEnergyConsMult;
        public static ConfigEntry<float> vehicleEnergyConsMult;
        public static ConfigEntry<float> baseEnergyConsMult;
        public static ConfigEntry<float> knifeRangeMult;
        public static ConfigEntry<float> knifeDamageMult;
        public static ConfigEntry<int> medKitHP;
        public static ConfigEntry<float> craftTimeMult;
        public static ConfigEntry<float> buildTimeMult;
        public static ConfigEntry<bool> useBestLOD;

        public static ConfigEntry<int> crushDepth;
        public static ConfigEntry<float> crushDamage;
        public static ConfigEntry<float> vehicleCrushDamageMult;
        public static ConfigEntry<float> crushDamageProgression;
        public static ConfigEntry<EmptyVehiclesCanBeAttacked> emptyVehiclesCanBeAttacked;
        //public static ConfigEntry<int> hungerUpdateInterval;
        public static ConfigEntry<bool> newHungerSystem;
        public static ConfigEntry<int> maxPlayerFood;
        public static ConfigEntry<int> maxPlayerWater;
        public static ConfigEntry<float> fishFoodWaterRatio;
        public static ConfigEntry<EatingRawFish> eatRawFish;
        public static ConfigEntry<bool> cantEatUnderwater;
        public static ConfigEntry<bool> cantUseMedkitUnderwater;
        public static ConfigEntry<float> foodDecayRateMult;
        public static ConfigEntry<int> fruitGrowTime;
        public static ConfigEntry<float> fishSpeedMult;
        public static ConfigEntry<float> creatureSpeedMult;
        public static ConfigEntry<int> CreatureFleeChance;
        public static ConfigEntry<bool> creatureFleeUseDamageThreshold;
        public static ConfigEntry<bool> creatureFleeChanceBasedOnHealth;
        public static ConfigEntry<bool> waterparkCreaturesBreed;
        public static ConfigEntry<bool> noFishCatching;
        public static ConfigEntry<bool> noBreakingWithHand;
        public static ConfigEntry<bool> damageImpactEffect;
        public static ConfigEntry<bool> damageScreenFX;
        public static ConfigEntry<int> stalkerLoseToothChance;
        public static ConfigEntry<bool> realOxygenCons;
        public static ConfigEntry<int> dropPodMaxPower;
        public static ConfigEntry<float> batteryChargeMult;
        public static ConfigEntry<int> craftedBatteryCharge;
        public static ConfigEntry<DropItemsOnDeath> dropItemsOnDeath;
        public static ConfigEntry<float> invMultWater;
        public static ConfigEntry<float> invMultLand;
        public static ConfigEntry<float> waterFreezeRate;
        public static ConfigEntry<int> snowballWater;
        public static ConfigEntry<float> baseHullStrengthMult;
        public static ConfigEntry<float> drillDamageMult;
        public static ConfigEntry<float> foodLossMult;
        public static ConfigEntry<float> waterLossMult;
        public static ConfigEntry<int> foodWaterHealThreshold;


        public static void Bind()
        {  // “ ” ‛

            timeFlowSpeed = Main.configMenu.Bind("", "Time flow speed multiplier", 1f, "The higher the value the shorter days are. This also affects crafting time, building time, battery charging time.");
            playerWaterSpeedMult = Main.configMenu.Bind("", "Player speed multiplier in water", 1f);
            playerGroundSpeedMult = Main.configMenu.Bind("", "Player speed multiplier on ground", 1f);
            seaglideSpeedMult = Main.configMenu.Bind("", "Seaglide speed multiplier", 1f, "");
            exosuitSpeedMult = Main.configMenu.Bind("", "Prawn suit speed multiplier", 1f, "");
            seatruckSpeedMult = Main.configMenu.Bind("", "Seatruck speed multiplier", 1f, "");
            hoverbikeSpeedMult = Main.configMenu.Bind("", "Snowfox speed multiplier", 1f, "");
            oxygenPerBreath = Main.configMenu.Bind("", "Oxygen per breath", 3f, "Amount of oxygen you consume every breath.");
            coldMult = Main.configMenu.Bind("", "Getting cold rate multiplier", 1f, "The rate at which you get cold when outside and get warm when inside will be multiplied by this");
            useRealTempForPlayerTemp = Main.configMenu.Bind("", "Only ambient tempterature makes player warm", false, "In vanilla game when you are underwater you get warm if moving and get cold when not. When out of water, some areas (caves, your unpowered base) make you warm regardless of ambient tempterature. With this on you get warm only if ambient temperature is above 'Warm temperature' setting in the config file.");
            useBestLOD = Main.configMenu.Bind("", "Always use best LOD models", false, "A lot of models in the game use different levels of detail depending on how close you are to them. Some of them look different and you can see those objects change as you approach them. With this on best LOD models will always be used. It will affect the game performance, but with a good GPU it should not be noticable. The game has to be reloaded after changing this.");


            toolEnergyConsMult = Main.configMenu.Bind("", "Tool power consumption multiplier", 1f, "Amount of power consumed by your tools will be multiplied by this.");
            vehicleEnergyConsMult = Main.configMenu.Bind("", "Vehicle power consumption multiplier", 1f, "Amount of power consumed by your vehicles will be multiplied by this.");
            baseEnergyConsMult = Main.configMenu.Bind("", "Base power consumption multiplier", 1f, "Amount of power consumed by your things in your base will be multiplied by this.");
            knifeRangeMult = Main.configMenu.Bind("", "Knife range multiplier", 1f, "Applies to knife and thermoblade. You have to reequip your knife after changing this.");
            knifeDamageMult = Main.configMenu.Bind("", "Knife damage multiplier", 1f, "Applies to knife and thermoblade. You have to reequip your knife after changing this.");
            medKitHP = Main.configMenu.Bind("", "First aid kit HP", 50, "HP restored when using first aid kit.");
            craftTimeMult = Main.configMenu.Bind("", "Crafting time multiplier", 1f, "Crafting time will be multiplied by this when crafting things with fabricator or modification station.");
            buildTimeMult = Main.configMenu.Bind("", "Building time multiplier", 1f, "Building time will be multiplied by this when using builder tool.");
            //playerMoveTweaks = Main.configMenu.Bind("", "Player movement tweaks", false, "Player swimming speed is reduced to 70%. Player vertical, backward, sideways movement speed is halved. Any diving suit reduces your speed by 5% on land and in water. Fins reduce your speed by 10% on land. Lightweight high capacity tank reduces your speed by 5% on land. Every other tank reduces your speed by 10% on land and by 5% in water. No speed reduction when near wrecks. You can sprint only if moving forward. Seaglide works only if moving forward. When swimming while your PDA is open your movement speed is halved. When swimming while holding a tool in your hand your movement speed is reduced to 70%. Game has to be reloaded after changing this.");

            //exosuitMoveTweaks = Main.configMenu.Bind("", "Prawn suit movement tweaks", false, "Prawn suit can not move sideways. No time limit when using thrusters, but they consume twice more power. Vertical speed is reduced when using thrusters. Can't use thrusters to hover above ground when out of water.");
            //seatruckMoveTweaks = Main.configMenu.Bind("", "Seatruck movement tweaks", false, "Seatruck's vertical, sideways and backward speed is halved. Afterburner is active for as long as you hold the 'sprint' key but consumes twice more power. Horsepower upgrade increases seatruck's speed by 10%. You can install more than 1 Horsepower upgrade.");
            //crushDepth = Main.configMenu.Bind("", "Crush depth", 200, "Depth in meters below which player starts taking crush damage. Does not work if crush damage multiplier is 0.");
            //crushDamageMult = Main.configMenu.Bind("", "Crush damage multiplier", 0f, "Every 3 seconds player takes 1 damage multiplied by this for every meter below crush depth.");
            //vehicleCrushDamageMult = Main.configMenu.Bind("", "Vehicle crush damage multiplier", 0f, "Every 3 seconds vehicles take 1 damage multiplied by this for every meter below crush depth.");
            crushDepth = Main.configMenu.Bind("", "Crush depth", 200, "Depth in meters below which player starts taking crush damage set by 'Crush damage' setting.");
            crushDamage = Main.configMenu.Bind("", "Crush damage", 0f, "Player takes this damage when below crush depth.");
            crushDamageProgression = Main.configMenu.Bind("", "Crush damage progression", 0f, "If this is more than 0, the crush damage you take will be: 'Crush damage' value + 'Crush damage' value * this * number of meters below crush depth.");
            vehicleCrushDamageMult = Main.configMenu.Bind("", "Vehicle crush damage multiplier", 1f, "Vehicle crush damage will be multiplied by this.");

            emptyVehiclesCanBeAttacked = Main.configMenu.Bind("", "Unmanned vehicles can be attacked", EmptyVehiclesCanBeAttacked.Default, "By default unmanned seamoth or prawn suit can be attacked but cyclops can not.");
            //hungerUpdateInterval = Main.configMenu.Bind("", "Hunger update interval", 10, "Time in seconds it takes your hunger and thirst to update.");
            newHungerSystem = Main.configMenu.Bind("", "New hunger system", false, "You do not regenerate health when you are full. When you sprint you get hungry and thirsty twice as fast. You don't lose health when your food or water value is 0. Your food and water values can go as low as -100. When your food or water value is below 0 your movement speed will be reduced proportionally to that value. When either your food or water value is -100 your movement speed will be reduced by 50% and you will start taking hunger damage. The higher your food value above 100 is the less food you get when eating: when your food value is 110 you lose 10% of food, when it is 190 you lose 90%.");
            maxPlayerFood = Main.configMenu.Bind("", "Max player food", (int)SurvivalConstants.kMaxOverfillStat, "You food meter will be capped at this.");
            maxPlayerWater = Main.configMenu.Bind("", "Max player water", (int)SurvivalConstants.kMaxStat, "You water meter will be capped at this.");
            fishFoodWaterRatio = Main.configMenu.Bind("", "Fish water/food value ratio", 0f, "Fish's water value will be proportional to its food value if this is more than 0. If this is 0.1 then water value will be 10% of food value. If this is 0.9 then water value will be 90% of food value. Game has to be reloaded after changing this.");
            eatRawFish = Main.configMenu.Bind("", "Eating raw fish", EatingRawFish.Default, "This changes amount of food you get by eating raw fish. Harmless: it is a random number between 0 and fish's food value. Risky: it is a random number between fish's negative food value and fish's food value. Harmful: it is a random number between fish's negative food value and 0.");
            cantEatUnderwater = Main.configMenu.Bind("", "Can not eat underwater", false, "You will not be able to eat or drink when swimming underwater if this is on.");
            cantUseMedkitUnderwater = Main.configMenu.Bind("", "Can not use first aid kit underwater", false, "You will not be able to use first aid kit when swimming underwater if this is on.");
            foodDecayRateMult = Main.configMenu.Bind("", "Food decay rate multiplier", 1f, "Food decay rate will be multiplied by this. You have to reload the game after changing this.");
            fruitGrowTime = Main.configMenu.Bind("", "Fruit grow time", 0, "Time in days it takes a lantern tree fruit, a frost anemone heart, a creepvine seed cluster, a Preston's plant fruit to grow. If this is 0 then vanilla code will run. You have to reload your game after changing this.");
            fishSpeedMult = Main.configMenu.Bind("", "Catchable fish speed multiplier", 1f, "Swimming speed of fish that you can catch will be multiplied by this.");
            creatureSpeedMult = Main.configMenu.Bind("", "Creature speed multiplier", 1f, "Swimming speed of creatures that you can not catch will be multiplied by this.");
            CreatureFleeChance = Main.configMenu.Bind("", "Creature flee chance percent", 100, "Creature's flee chance percent when it is under attack and its flee damage threshold is reached.");
            creatureFleeUseDamageThreshold = Main.configMenu.Bind("", "Damage threshold for fleeing creatures", true, "Most creatures have damage threshold that has to be reached before they start fleeing. When this is off, every creature will flee if it takes any damage.");
            creatureFleeChanceBasedOnHealth = Main.configMenu.Bind("", "Creature flee chance depends on its health", false, "Only creatures's health will be used to decide if it should flee when under attack. Creature with 90% health has 10% chance to flee. Creature with 10% health has 90% chance to flee. This setting overrides both 'Creature flee chance percent' and 'Damage threshold for fleeing creatures'.");
            waterparkCreaturesBreed = Main.configMenu.Bind("", "Creatures in alien containment can breed", true);
            noFishCatching = Main.configMenu.Bind("", "Can not catch fish with bare hands", false, "To catch fish you will have to use knife, propulsion cannon, stasis rifle or grav trap. Does not apply if you are inside alien containment.");
            noBreakingWithHand = Main.configMenu.Bind("", "Can not break outcrop with bare hands", false, "You will have to use a knife or propulsion cannon to break outcrops or collect resources attached to rock or seabed. To craft your first knife pick up scrap metal next to your crashed ship.");
            damageImpactEffect = Main.configMenu.Bind("", "Player impact damage screen effects", true, "This toggles cracks on your swimming mask when you take damage.");
            damageScreenFX = Main.configMenu.Bind("", "Player damage screen effects", true, "This toggles red screen effects when you take damage.");
            stalkerLoseToothChance = Main.configMenu.Bind("", "Chance percent for stalker to lose its tooth", 50, "Probability percent for stalker to lose its tooth when it bites something hard.");

            realOxygenCons = Main.configMenu.Bind("", "Realistic oxygen consumption", false, "Vanilla oxygen consumption without rebreather has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth above 100 meters. With this on your oxygen consumption will increase in linear progression using 'Crush depth' setting. When you are at crush depth it will be equal to vanilla max oxygen consumption and will increase as you dive deeper.");
            dropPodMaxPower = Main.configMenu.Bind("", "Drop pod max power", 0, "If this is not 0, your drop pod's max power will be set to this. Drop pod's power will regenerate during the day. The game has to be reloaded after changing this.");
            batteryChargeMult = Main.configMenu.Bind("", "Battery charge multiplier", 1f, "Max charge of batteries and power cells will be multiplied by this. Game has to be reloaded after changing this.");
            craftedBatteryCharge = Main.configMenu.Bind("", "Crafted battery charge percent", 100, "Charge percent of batteries and power cells you craft will be set to this.");
            dropItemsOnDeath = Main.configMenu.Bind("", "Drop items when you die", DropItemsOnDeath.Default);
            invMultWater = Main.configMenu.Bind("", "Inventory weight multiplier in water", 0f, "When this is not 0 and you are swimming you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.");
            invMultLand = Main.configMenu.Bind("", "Inventory weight multiplier on land", 0f, "When this is not 0 and you are on land you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.");
            waterFreezeRate = Main.configMenu.Bind("", "Water freeze rate multiplier", 0f, "Bottled water will freeze at this rate if ambient temperature is below 0 C°. The game has to be reloaded after changing this.");
            snowballWater = Main.configMenu.Bind("", "Snowball water value", 0, "When you eat a snowball, you will get this amount of water and lose this amount of warmth. The game has to be reloaded after changing this.");
            baseHullStrengthMult = Main.configMenu.Bind("", "Base hull strength multiplier", 1f, "");
            drillDamageMult = Main.configMenu.Bind("", "Prawn suit drill arm damage multiplier", 1f, "");
            foodLossMult = Main.configMenu.Bind("", "Food loss multiplier", 1f, "Food value you lose when your hunger updates will be multiplied by this.");
            waterLossMult = Main.configMenu.Bind("", "Water loss multiplier", 1f, "Water value you lose when your hunger updates will be multiplied by this.");
            foodWaterHealThreshold = Main.configMenu.Bind("", "Food heal threshold", 150, "Your health regenerates when sum of your food and water values is greater than this");

            //transferAllItemsButton = Main.configMenu.Bind("", "Move all items button", KeyCode.None, "When you have a container open, press this button on a controller to move all items. If you are using a keyboard, you have to hold down this key and click an item.");
            //transferSameItemsButton = Main.configMenu.Bind("", "Move same items button", KeyCode.None, "When you have a container open, press this button on a controller to move all items of the same type. If you are using a keyboard, you have to hold down this key and click an item.");
            //quickslotButton = Main.configMenu.Bind("", "Quickslot cycle button", KeyCode.None, "Press ‛Cycle next‛ or ‛Cycle previous‛ button while holding down this button to cycle tools in your current quickslot.");
            //previousPDATabKey = Main.configMenu.Bind("", "Previous PDA tab key", KeyCode.None, "Key to switch to left PDA Tab. This works only with keyboard.");
            //nextPDATabKey = Main.configMenu.Bind("", "Next PDA tab key", KeyCode.None, "Key to switch to right PDA Tab. This works only with keyboard.");


        }

        public enum DropItemsOnDeath { Default, Drop_everything, Do_not_drop_anything }
        public enum EmptyVehiclesCanBeAttacked { Default, Yes, No, Only_if_lights_on }
        public enum EatingRawFish { Default, Harmless, Risky, Harmful }
    }
}
