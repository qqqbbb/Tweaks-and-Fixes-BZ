using BepInEx.Configuration;
using Nautilus.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tweaks_Fixes
{
    public static class ConfigToEdit
    {
        public static ConfigEntry<bool> heatBladeCooks;
        public static ConfigEntry<bool> dontSpawnKnownFragments;
        public static ConfigEntry<bool> noKillParticles;
        public static ConfigEntry<bool> alwaysShowHealthFoodNunbers;
        public static ConfigEntry<bool> pdaClock;
        public static ConfigEntry<Button> transferAllItemsButton;
        public static ConfigEntry<Button> transferSameItemsButton;
        public static ConfigEntry<Button> quickslotButton;
        public static ConfigEntry<string> gameStartWarningText;
        public static ConfigEntry<string> newGameLoot;
        public static ConfigEntry<string> crushDepthEquipment;
        public static ConfigEntry<string> crushDamageEquipment;
        public static ConfigEntry<string> itemMass;
        public static ConfigEntry<string> unmovableItems;
        public static ConfigEntry<string> bloodColor;
        public static ConfigEntry<string> gravTrappable;
        public static ConfigEntry<float> medKitHPperSecond;
        //public static ConfigEntry<string> silentCreatures;
        public static ConfigEntry<string> eatableFoodValue;
        public static ConfigEntry<string> eatableWaterValue;
        public static ConfigEntry<bool> fixMelon;
        public static ConfigEntry<bool> randomPlantRotation;
        public static ConfigEntry<bool> silentReactor;
        //public static ConfigEntry<bool> newUIstrings;
        public static ConfigEntry<bool> disableUseText;
        public static ConfigEntry<bool> craftWithoutBattery;
        public static ConfigEntry<bool> builderPlacingWhenFinishedBuilding;
        public static ConfigEntry<bool> crushDamageScreenEffect;
        public static ConfigEntry<bool> disableGravityForExosuit;
        public static ConfigEntry<bool> replaceDealDamageOnImpactScript;
        //public static ConfigEntry<float> exosuitDealDamageMinSpeed;
        //public static ConfigEntry<float> exosuitTakeDamageMinSpeed;
        //public static ConfigEntry<float> exosuitTakeDamageMinMass;
        //public static ConfigEntry<float> seamothDealDamageMinSpeed;
        //public static ConfigEntry<float> seamothTakeDamageMinSpeed;
        //public static ConfigEntry<float> seamothTakeDamageMinMass;
        public static ConfigEntry<float> solarPanelMaxDepth;
        public static ConfigEntry<bool> newStorageUI;
        public static ConfigEntry<bool> canReplantMelon;
        public static ConfigEntry<int> eatingOutsideCold;
        public static ConfigEntry<int> brinewingAttackColdDamage;
        public static ConfigEntry<bool> fixCoral;
        public static ConfigEntry<string> notRespawningCreatures;
        public static ConfigEntry<string> notRespawningCreaturesIfKilledByPlayer;
        public static ConfigEntry<bool> warmKelpWater;
        public static ConfigEntry<int> brinicleGrowTimeMult;
        public static ConfigEntry<bool> exosuitTakesDamageFromCollisions;
        public static ConfigEntry<bool> vehiclesTakeDamageOnImpact;
        public static ConfigEntry<bool> vehiclesDealDamageOnImpact;
        public static ConfigEntry<bool> exosuitTakesDamageWhenCollidingWithTerrain;
        public static ConfigEntry<string> decayingFood;
        public static ConfigEntry<string> respawnTime;
        public static ConfigEntry<bool> craftVehicleUpgradesOnlyInMoonpool;
        public static ConfigEntry<int> warmTemp;
        public static ConfigEntry<int> insideBaseTemp;
        public static ConfigEntry<bool> propulsionCannonGrabFX;
        public static ConfigEntry<int> rockPuncherChanceToFindRock;
        public static ConfigEntry<bool> lowOxygenWarning;
        public static ConfigEntry<bool> lowOxygenAudioWarning;
        public static ConfigEntry<bool> disableHints;
        public static ConfigEntry<bool> dropHeldTool;
        public static ConfigEntry<int> freeTorpedos;
        public static ConfigEntry<bool> builderToolBuildsInsideWithoutPower;
        public static ConfigEntry<bool> cameraShake;
        public static ConfigEntry<bool> removeDeadCreaturesOnLoad;
        public static ConfigEntry<bool> scannerFX;
        public static ConfigEntry<bool> dropItemsAnywhere;
        public static ConfigEntry<bool> showTempFahrenhiet;
        public static ConfigEntry<string> notRechargableBatteries;
        public static ConfigEntry<bool> vehiclesHurtCreatures;
        public static ConfigEntry<bool> removeCreditsButton;
        public static ConfigEntry<bool> removeTroubleshootButton;
        public static ConfigEntry<bool> removeUnstuckButton;
        public static ConfigEntry<bool> removeFeedbackButton;
        public static ConfigEntry<bool> EnableDevButton;
        public static ConfigEntry<bool> seaglideWorksOnlyForward;
        public static ConfigEntry<int> oneHandToolSwimSpeedMod;
        public static ConfigEntry<int> oneHandToolWalkSpeedMod;
        public static ConfigEntry<int> twoHandToolSwimSpeedMod;
        public static ConfigEntry<int> twoHandToolWalkSpeedMod;
        public static ConfigEntry<int> playerSidewardSpeedMod;
        public static ConfigEntry<int> playerBackwardSpeedMod;
        public static ConfigEntry<int> playerVerticalSpeedMod;
        public static ConfigEntry<string> groundSpeedEquipment;
        public static ConfigEntry<string> waterSpeedEquipment;
        public static ConfigEntry<bool> disableExosuitSidestep;
        public static ConfigEntry<bool> exosuitThrusterWithoutLimit;
        public static ConfigEntry<bool> fixExosuitJumpParticleFX;
        public static ConfigEntry<bool> hoverbikeMoveOnWater;
        public static ConfigEntry<bool> hoverbikeBoostWithoutCooldown;
        public static ConfigEntry<bool> seaMonkeyBringGift;
        public static ConfigEntry<bool> seaMonkeyGrabTool;
        public static ConfigEntry<int> seatruckSidewardSpeedMod;
        public static ConfigEntry<int> seatruckBackwardSpeedMod;
        public static ConfigEntry<int> seatruckVertSpeedMod;
        public static ConfigEntry<bool> fixHoverbikeAnalogMovement;
        public static ConfigEntry<bool> fixSeatruckAnalogMovement;
        public static ConfigEntry<bool> seatruckAfterburnerWithoutCooldown;
        public static ConfigEntry<bool> replaceSeatruckHorsePowerUpgrade;
        public static ConfigEntry<bool> alwaysSpawnWhenKnifeHarvesting;
        public static ConfigEntry<bool> playerBreathBubbles;
        public static ConfigEntry<bool> playerBreathBubblesSoundFX;
        public static ConfigEntry<bool> trypophobiaMode;
        public static ConfigEntry<bool> brinicleBreakForNoReason;
        public static ConfigEntry<bool> consistentHungerUpdateTime;
        public static ConfigEntry<bool> cameraBobbing;
        public static ConfigEntry<bool> removeBigParticlesWhenKnifing;
        public static ConfigEntry<bool> flareFlicker;
        public static ConfigEntry<bool> propulsionCannonTweaks;
        public static ConfigEntry<bool> beaconTweaks;
        public static ConfigEntry<bool> flareTweaks;



        public static AcceptableValueRange<float> medKitHPperSecondRange = new AcceptableValueRange<float>(0.001f, 100f);
        public static AcceptableValueRange<int> percentRange = new AcceptableValueRange<int>(0, 100);

        public static void Bind()
        { // “ ” ‛
            //Main.logger.LogMessage("ConfigToEdit bind start ");
            heatBladeCooks = Main.configToEdit.Bind("TOOLS", "Thermoblade cooks fish on kill", true);
            dontSpawnKnownFragments = Main.configToEdit.Bind("MISC", "Do not spawn fragments for unlocked blueprints", false);
            noKillParticles = Main.configToEdit.Bind("CREATURES", "No particles when creature dies", false, "No yellow cloud particles will spawn when a creature dies. Game has to be reloaded after changing this. ");
            alwaysShowHealthFoodNunbers = Main.configToEdit.Bind("UI", "Always show numbers inside health, food and temperature meters in UI", false);
            pdaClock = Main.configToEdit.Bind("UI", "Show time and temperature in PDA", true);
            newGameLoot = Main.configToEdit.Bind("DROP POD", "Drop pod items", "FilteredWater 0, NutrientBlock 0, Flare 0", "Items you find in your drop pod when you start a new game. The format is item ID, space, number of items. Every entry is separated by comma.");
            crushDepthEquipment = Main.configToEdit.Bind("EQUIPMENT", "Crush depth equipment", "ReinforcedDiveSuit 0", "Allows you to make your equipment increase your crush depth. The format is: item ID, space, number of meters that will be added to your crush depth. Every entry is separated by comma.");
            crushDamageEquipment = Main.configToEdit.Bind("EQUIPMENT", "Crush damage equipment", "ReinforcedDiveSuit 0", "Allows you to make your equipment reduce your crush damage. The format is: item ID, space, crush damage percent that will be blocked. Every entry is separated by comma.");
            itemMass = Main.configToEdit.Bind("ITEMS", "Item mass", "", "Allows you to change mass of pickupable items. The format is: item ID, space, item mass in kg as decimal point number. Every entry is separated by comma.");
            unmovableItems = Main.configToEdit.Bind("ITEMS", "Unmovable items", "", "Comma separated list of pickupable item IDs. Items in this list can not be moved by bumping into them. You will always find them where you dropped them.");

            bloodColor = Main.configToEdit.Bind("CREATURES", "Blood color", "0.784 1.0 0.157", "Creatures‛ blood color will be set to this. Each value is a decimal point number from 0 to 1. First number is red. Second number is green. Third number is blue.");
            gravTrappable = Main.configToEdit.Bind("ITEMS", "Gravtrappable items", "seaglide, airbladder, flare, flashlight, builder, lasercutter, ledlight, divereel, propulsioncannon, welder, repulsioncannon, scanner, stasisrifle, knife, heatblade, metaldetector, teleportationtool, precursorkey_blue, precursorkey_orange, precursorkey_purple, precursorkey_red, precursorkey_white, compass, fins, fireextinguisher, suitboostertank, firstaidkit, doubletank, plasteeltank, coldsuit, flashlighthelmet, rebreather, reinforceddivesuit, maproomhudchip, tank, stillsuit, swimchargefins, ultraglidefins, highcapacitytank,", "Comma separated list of items affected by grav trap.");
            medKitHPperSecond = Main.configToEdit.Bind("ITEMS", "Amount of HP restored by first aid kit every second", 100f, new ConfigDescription("Set this to a low number to slowly restore HP after using first aid kit.", medKitHPperSecondRange));
            //silentCreatures = Main.configB.Bind("", "Silent creatures", "", "List of creature IDs separated by comma. Creatures in this list will be silent.");
            eatableFoodValue = Main.configToEdit.Bind("ITEMS", "Eatable component food value", "CreepvineSeedCluster 5", "Items from this list will be made eatable. The format is: item ID, space, food value. Every entry is separated by comma.");
            eatableWaterValue = Main.configToEdit.Bind("ITEMS", "Eatable component water value", "HangingFruit 5", "Items from this list will be made eatable. The format is: item ID, space, water value. Every entry is separated by comma.");

            eatingOutsideCold = Main.configToEdit.Bind("SURVIVAL", "Warmth lost when eating ouside", 0, "You will lose this amount of warmth when eating ouside.");

            fixMelon = Main.configToEdit.Bind("PLANTS", "Fix melon and Preston plant", false, "If true, you will be able to plant only 1 Preston plant or melon in a pot and only 4 in a planter.");
            randomPlantRotation = Main.configToEdit.Bind("PLANTS", "Random plant rotation", true, "If true plants in planters to have random rotation.");
            silentReactor = Main.configToEdit.Bind("BASE", "Silent nuclear reactor", false, "If true nuclear reactor will be silent.");
            //newUIstrings = Main.configB.Bind("", "New UI text", true, "If false new UI elements added by the mod wil be disabled.");
            newStorageUI = Main.configToEdit.Bind("UI", "New storage UI", true);
            disableUseText = Main.configToEdit.Bind("UI", "Disable quickslots text", false, "If true text above your quickslots will be disabled.");
            craftWithoutBattery = Main.configToEdit.Bind("TOOLS", "Craft without battery", false, "If true your newly crafted tools and vehicles will not have batteries in them.");
            notRechargableBatteries = Main.configToEdit.Bind("ITEMS", "Not rechargable batteries", "", "Comma separated list of battery IDs. Batteries from this list can not be recharged");
            builderPlacingWhenFinishedBuilding = Main.configToEdit.Bind("TOOLS", "Builder tool placing mode when finished building", true, "If false your builder tool will exit placing mode when you finish building.");
            crushDamageScreenEffect = Main.configToEdit.Bind("PLAYER", "Crush damage screen effect", true, "If false there will be no screen effects when player takes crush damage.");
            disableGravityForExosuit = Main.configToEdit.Bind("VEHICLES", "Disable gravity for prawn suit", false, "If true, prawn suit will ignore gravity when you are not piloting it. Use this if your prawn suit falls through the ground.");
            vehiclesHurtCreatures = Main.configToEdit.Bind("VEHICLES", "Vehicles hurt creatures", true, "Vehicles will not hurt creatures when colliding with them if this is false.");
            //exosuitDealDamageMinSpeed = Main.configB.Bind("", "Prawn suit min speed to deal damage", 7f, "Min speed in meters per second at which prawn suit deals damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //exosuitTakeDamageMinSpeed = Main.configB.Bind("", "Prawn suit min speed to take damage", 7f, "Min speed in meters per second at which prawn suit takes damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //exosuitTakeDamageMinMass = Main.configB.Bind("", "Min mass that can damage prawn suit", 5f, "Min mass in kg for objects that can damage prawn suit when colliding with it. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //seamothDealDamageMinSpeed = Main.configB.Bind("", "Seamoth min speed to deal damage", 7f, "Min speed in meters per second at which seamoth deals damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //seamothTakeDamageMinSpeed = Main.configB.Bind("", "Seamoth min speed to take damage", 7f, "Min speed in meters per second at which seamoth takes damage when colliding with objects. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            //seamothTakeDamageMinMass = Main.configB.Bind("", "Min mass that can damage seamoth", 5f, "Min mass in kg for objects that can damage seamoth when colliding with it. Works only if 'replaceDealDamageOnImpactScript' setting is true.");
            solarPanelMaxDepth = Main.configToEdit.Bind("BASE", "Solar panel max depth", 250f, "Depth in meters below which solar panel does not produce power.");

            canReplantMelon = Main.configToEdit.Bind("PLANTS", "Can replant melon", true, "If false gel sack and melon can't be replanted.");
            brinewingAttackColdDamage = Main.configToEdit.Bind("CREATURES", "Brinewing freeze damage", 0, "When this is not 0, brinewing attack will drain your cold meter by this amount instead of freezing you.");

            fixCoral = Main.configToEdit.Bind("MISC", "Fix table coral", true, "If true then table coral will always be attached horizontally to rocks and its animation will be disabled.");
            notRespawningCreatures = Main.configToEdit.Bind("CREATURES", "Not respawning creatures", "TrivalveBlue, TrivalveYellow", "Comma separated list of creature IDs that will not respawn.");
            notRespawningCreaturesIfKilledByPlayer = Main.configToEdit.Bind("CREATURES", "Not respawning creatures if killed by player", "TitanHolefish, BruteShark, Cryptosuchus, SnowStalker, SnowStalkerBaby, RockPuncher, SquidShark", "Comma separated list of creature IDs that will respawn only if killed by another creature.");
            respawnTime = Main.configToEdit.Bind("CREATURES", "Creature respawn time", "", "Number of days it takes a creature to respawn. The format is: creature ID, space, number of days it takes to respawn. By default fish and big creatures respawn in 12 hours, leviathans respawn after 1 day.");
            warmKelpWater = Main.configToEdit.Bind("SURVIVAL", "Warm kelp water", true, "Water is always warm near kelps. Set this to false to disable it.");
            brinicleGrowTimeMult = Main.configToEdit.Bind("MISC", "Brinicle grow time multiplier", 1, "Time it takes a brinicle to grow will be multiplied by this");
            brinicleBreakForNoReason = Main.configToEdit.Bind("MISC", "Brinicles break for no reason", true);
            replaceDealDamageOnImpactScript = Main.configToEdit.Bind("VEHICLES", "Replace DealDamageOnImpact script", false, "Replace script that handles vehicle collisions.");
            vehiclesTakeDamageOnImpact = Main.configToEdit.Bind("VEHICLES", "Vehicles take damage from collisions", true, "Works only if 'Replace DealDamageOnImpact script' is true");
            exosuitTakesDamageFromCollisions = Main.configToEdit.Bind("VEHICLES", "Prawn suit takes damage from collisions", true, "Works only if 'Replace DealDamageOnImpact script' is true");
            vehiclesDealDamageOnImpact = Main.configToEdit.Bind("VEHICLES", "Vehicles deal damage when colliding", true, "Works only if 'Replace DealDamageOnImpact script' is true");
            exosuitTakesDamageWhenCollidingWithTerrain = Main.configToEdit.Bind("VEHICLES", "Prawn suit takes damage when colliding with terrain", false, "Works only if 'Replace DealDamageOnImpact script' is true");
            decayingFood = Main.configToEdit.Bind("ITEMS", "Decaying food", "SpicyFruitSalad", "Comma separated list of food item IDs. Food from this list will decay.");
            craftVehicleUpgradesOnlyInMoonpool = Main.configToEdit.Bind("VEHICLES", "Only Vehicle upgrade console can craft vehicle upgrades", false, "Fabricator will not be able to craft vehicle upgrades if this is true.");
            warmTemp = Main.configToEdit.Bind("SURVIVAL", "Warm temperature", 15, "Player is warm when ambient temperature is above this celsius value.");
            insideBaseTemp = Main.configToEdit.Bind("SURVIVAL", "Temperature inside base", 22, "Celsius temperature inside powered base or vehicle. Used only when 'Only ambient tempterature makes player warm' setting is on.");
            gameStartWarningText = Main.configToEdit.Bind("MISC", "Game start warning text", "", "Text shown when the game starts. If this field is empty the warning will be skipped.");
            propulsionCannonGrabFX = Main.configToEdit.Bind("TOOLS", "Propulsion cannon sphere effect", true, "Blue sphere visual effect you see when holding an object with propulsion cannon will be disabled if this is false.");
            rockPuncherChanceToFindRock = Main.configToEdit.Bind("CREATURES", "Rock puncher chance percent to find rock", 20, new ConfigDescription("", percentRange));
            lowOxygenWarning = Main.configToEdit.Bind("MISC", "Low oxygen onscreen warning", true);
            lowOxygenAudioWarning = Main.configToEdit.Bind("MISC", "Low oxygen audio warning", true);
            disableHints = Main.configToEdit.Bind("MISC", "Disable tutorial messages", true, "This disables messages that tell you to 'eat something', 'break limestone', etc. Game has to be reloaded after changing this.");
            dropHeldTool = Main.configToEdit.Bind("PLAYER", "Drop held tool when taking damage", false, "Chance percent to drop your tool is equal to amount of damage taken.");
            freeTorpedos = Main.configToEdit.Bind("VEHICLES", "Free torpedos", 2, "Number of torpedos you get when installing new Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo Arm.");
            builderToolBuildsInsideWithoutPower = Main.configToEdit.Bind("TOOLS", "Builder tool does not need power when building inside", true);
            cameraShake = Main.configToEdit.Bind("UI", "Screen shaking", true, "");
            removeDeadCreaturesOnLoad = Main.configToEdit.Bind("CREATURES", "Remove dead creatures when loading saved game", true, "");
            scannerFX = Main.configToEdit.Bind("TOOLS", "Wierd visual effect on objects being scanned", true, "");
            dropItemsAnywhere = Main.configToEdit.Bind("ITEMS", "Player can drop inventory items anywhere", false, "This allows you to place placable items anywhere in the world and drop items anywhere except seatruck.");
            showTempFahrenhiet = Main.configToEdit.Bind("UI", "Show temperature in Fahrenhiet instead of Celcius", false, "");
            removeCreditsButton = Main.configToEdit.Bind("MENU BUTTOMS", "Remove credits button from main menu", false);
            removeTroubleshootButton = Main.configToEdit.Bind("MENU BUTTOMS", "Remove troubleshooting button from options menu", false);
            removeUnstuckButton = Main.configToEdit.Bind("MENU BUTTOMS", "Remove unstuck button from pause menu", false);
            removeFeedbackButton = Main.configToEdit.Bind("MENU BUTTOMS", "Remove feedback button from pause menu", false);
            EnableDevButton = Main.configToEdit.Bind("MENU BUTTOMS", "Enable developer button in pause menu", false);

            seaglideWorksOnlyForward = Main.configToEdit.Bind("PLAYER MOVEMENT", "Seaglide works only when moving forward", false, "");
            playerSidewardSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Player sideward speed modifier", 0, "Player's speed will be reduced by this percent when moving sideward.");
            playerBackwardSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Player backward speed modifier", 0, "Player's speed will be reduced by this percent when moving backward.");
            playerVerticalSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Player vertical speed modifier", 0, "Player's speed will be reduced by this percent when swimming up or down.");
            oneHandToolWalkSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "One handed tool walking speed modifier", 0, "Your walking speed will be reduced by this percent when holding tool or PDA with one hand.");
            twoHandToolWalkSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Two handed tool walking speed modifier", 0, "Your walking speed will be reduced by this percent when holding tool with both hands.");
            oneHandToolSwimSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "One handed tool swimming speed modifier", 0, "Your swimming speed will be reduced by this percent when holding tool or PDA with one hand.");
            twoHandToolSwimSpeedMod = Main.configToEdit.Bind("PLAYER MOVEMENT", "Two handed tool swimming speed modifier", 0, "Your swimming speed will be reduced by this percent when holding tool with both hands.");
            groundSpeedEquipment = Main.configToEdit.Bind("EQUIPMENT", "Ground speed equipment", "", "Equipment in this list affects your movement speed on ground. The format is: item ID, space, percent that will be added to your movement speed. Negative numbers will reduce your movement speed. Every entry is separated by comma.");
            waterSpeedEquipment = Main.configToEdit.Bind("EQUIPMENT", "Water speed equipment", "", "Equipment in this list affects your movement speed in water. The format is: item ID, space, percent that will be added to your movement speed. Negative numbers will reduce your movement speed. Every entry is separated by comma. If this list is not empty, vanilla script that changes your movement speed will not run. ");

            disableExosuitSidestep = Main.configToEdit.Bind("VEHICLES", "Disable prawn suit sidestep", false, "");
            exosuitThrusterWithoutLimit = Main.configToEdit.Bind("VEHICLES", "Prawn suit thrusters never overheat", false, "No time limit when using thrusters, but they consume twice more power than walking");
            fixExosuitJumpParticleFX = Main.configToEdit.Bind("VEHICLES", "Fix prawn suit jump particle effect", true, "Sand cloud particle effect will appear only when prawn suit jumps from or lands on terrain.");
            hoverbikeMoveOnWater = Main.configToEdit.Bind("VEHICLES", "Snowfox can move on water", false, "");
            hoverbikeBoostWithoutCooldown = Main.configToEdit.Bind("VEHICLES", "Snowfox boosts without cooldown", false, "After activating the boost will work until you stop driving but your snowfox will consume twice more power.");
            seaMonkeyBringGift = Main.configToEdit.Bind("CREATURES", "Seamonkeys bring gifts to player", true, "");
            seaMonkeyGrabTool = Main.configToEdit.Bind("CREATURES", "Seamonkeys grab tools from player‛s hands", true, "");
            seatruckSidewardSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seatruck sideward speed modifier", 0, "Seatruck's speed will be reduced by this percent when moving sideward.");
            seatruckBackwardSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seatruck backward speed modifier", 0, "Seatruck's speed will be reduced by this percent when moving backward.");
            seatruckVertSpeedMod = Main.configToEdit.Bind("VEHICLES", "Seatruck vertical speed modifier", 0, "Seatruck's speed will be reduced by this percent when moving up or down.");
            fixHoverbikeAnalogMovement = Main.configToEdit.Bind("VEHICLES", "Fix snowfox analog movement", true, "Vanilla snowfox does not use analog values from controller sticks");
            fixSeatruckAnalogMovement = Main.configToEdit.Bind("VEHICLES", "Fix seatruck analog movement", true, "Vanilla seatruck does not use analog values from controller sticks");
            seatruckAfterburnerWithoutCooldown = Main.configToEdit.Bind("VEHICLES", "Seatruck afterburner works without cooldown", false, "After activating afterburner, it will work until you stop driving the seatruck but will consume twice more power.");
            replaceSeatruckHorsePowerUpgrade = Main.configToEdit.Bind("VEHICLES", "Replace seatruck horsepower upgrade", false, "Seatruck horsepower upgrade will increase engine's horsepower output and energy consumption by 10%. More than 1 can be installed.");
            alwaysSpawnWhenKnifeHarvesting = Main.configToEdit.Bind("TOOLS", "Always spawn things you harvest with knife instead of adding them to inventory", false);
            playerBreathBubbles = Main.configToEdit.Bind("PLAYER", "Player breath bubbles particle effect", true);
            playerBreathBubblesSoundFX = Main.configToEdit.Bind("PLAYER", "Player breath bubbles sound effect", true);
            trypophobiaMode = Main.configToEdit.Bind("MISC", "Trypophobia mode", false, "Honeycomb Fungus will be removed from the game if this is true");
            consistentHungerUpdateTime = Main.configToEdit.Bind("PLAYER", "Consistent hunger update time", false, "In vanilla game your hunger updates every 10 real time seconds. If this is true, hunger update interval will be divided by 'time flow speed multiplier' from the mod options.");
            cameraBobbing = Main.configToEdit.Bind("PLAYER", "Screen bobbing when swimming", true);
            removeBigParticlesWhenKnifing = Main.configToEdit.Bind("CREATURES", "Remove big particles when slashing creatures with knife", false, "You will see less blood particles when slashing creatures with knife if this is true.");
            flareFlicker = Main.configToEdit.Bind("TOOLS", "Flare light flickering", true, "");
            propulsionCannonTweaks = Main.configToEdit.Bind("TOOLS", "Propulsion cannon tweaks", true, "Improvements to propulsion cannon UI prompts. Ability ot eat fish you are holding with propulsion cannon. When grabbing and holding table coral with propulsion cannon, you can put it in inventory. Your propulsion cannon will break outcrop when you try to grab it. Propulsion cannon can grab fruits on plants ");
            beaconTweaks = Main.configToEdit.Bind("TOOLS", "Beacon tweaks", true, "You do not have to aim for certain part of a beacon to rename it. You can rename a beacon while holding it in your hands.");
            flareTweaks = Main.configToEdit.Bind("TOOLS", "Flare tweaks", true, "Tooltip for flare will tell you if it is burnt out. When you look at a dropped flare, you see if it is burnt out. You can light flare and not throw it.");




            transferAllItemsButton = Main.configToEdit.Bind("BUTTOM BIND", "Move all items button", Button.None, "Press this button to move all items from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            transferSameItemsButton = Main.configToEdit.Bind("BUTTOM BIND", "Move same items button", Button.None, "Press this button to move all items of the same type from one container to another. This works only with controller. Use this if you can not bind a controller button in the mod menu.");
            quickslotButton = Main.configToEdit.Bind("BUTTOM BIND", "Quickslot cycle button", Button.None, "Press 'Cycle next' or 'Cycle previous' button while holding down this button to cycle tools in your current quickslot. This works only with controller. Use this if you can not bind a controller button in the mod menu.");


            //Main.logger.LogMessage("ConfigToEdit bind end ");
        }

        private static Dictionary<TechType, int> ParseIntDicFromString(string input)
        {
            Dictionary<TechType, int> dic = new Dictionary<TechType, int>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string amount;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                amount = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                int a = 0;
                try
                {
                    a = int.Parse(amount);
                }
                catch (Exception)
                {
                    Main.logger.LogWarning("Could not parse: " + input);
                    continue;
                }
                if (a < 1)
                    continue;

                dic.Add(tt, a);
            }
            return dic;
        }

        private static Dictionary<TechType, float> ParseFloatDicFromString(string input)
        {
            Dictionary<TechType, float> dic = new Dictionary<TechType, float>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string amount;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                amount = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                float fl = 0;
                try
                {
                    fl = float.Parse(amount);
                }
                catch (Exception)
                {
                    Main.logger.LogWarning("Could not parse: " + input);
                    continue;
                }
                if (fl < 1)
                    continue;

                dic.Add(tt, fl);
            }
            return dic;
        }

        private static HashSet<TechType> ParseSetFromString(string input)
        {
            HashSet<TechType> set = new HashSet<TechType>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string techType = entries[i].Trim();

                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;
                // no simple way to check if techType is pickupable
                set.Add(tt);
                //Main.logger.LogDebug("ParseSetFromString " + tt );
            }
            return set;
        }

        private static Dictionary<TechType, float> ParseSpeedEquipmentDic(string input)
        {
            Dictionary<TechType, float> dic = new Dictionary<TechType, float>();
            string[] entries = input.Split(',');
            for (int i = 0; i < entries.Length; i++)
            {
                string s = entries[i].Trim();
                string techType;
                string amount;
                int index = s.IndexOf(' ');
                if (index == -1)
                    continue;

                techType = s.Substring(0, index);
                amount = s.Substring(index);
                if (!TechTypeExtensions.FromString(techType, out TechType tt, true))
                    continue;

                float fl = 0;
                try
                {
                    fl = float.Parse(amount);
                }
                catch (Exception)
                {
                    Main.logger.LogWarning("Could not parse: " + input);
                    continue;
                }
                if (fl == 0)
                    continue;

                fl = Mathf.Clamp(fl, -100f, 100f);
                dic.Add(tt, fl * .01f);
            }
            return dic;
        }

        public static void ParseConfig()
        {
            Crush_Damage.crushDepthEquipment = ParseIntDicFromString(crushDepthEquipment.Value);
            Crush_Damage.crushDamageEquipment = ParseIntDicFromString(crushDamageEquipment.Value);
            Pickupable_Patch.itemMass = ParseFloatDicFromString(itemMass.Value);
            Pickupable_Patch.unmovableItems = ParseSetFromString(unmovableItems.Value);
            Gravsphere_Patch.gravTrappable = ParseSetFromString(gravTrappable.Value);
            //Creature_Tweaks.silentCreatures = ParseSetFromString<TechType>(silentCreatures.Value);
            Pickupable_Patch.eatableFoodValue = ParseIntDicFromString(eatableFoodValue.Value);
            Pickupable_Patch.eatableWaterValue = ParseIntDicFromString(eatableWaterValue.Value);
            Drop_Pod_Patch.newGameLoot = ParseIntDicFromString(newGameLoot.Value);
            CreatureDeath_Patch.notRespawningCreatures = ParseSetFromString(notRespawningCreatures.Value);
            CreatureDeath_Patch.notRespawningCreaturesIfKilledByPlayer = ParseSetFromString(notRespawningCreaturesIfKilledByPlayer.Value);
            Food.decayingFood = ParseSetFromString(decayingFood.Value);
            CreatureDeath_Patch.respawnTime = ParseIntDicFromString(respawnTime.Value);

            Enum.TryParse(transferAllItemsButton.Value.ToString(), out Inventory_Patch.transferAllItemsButton);
            Enum.TryParse(transferSameItemsButton.Value.ToString(), out Inventory_Patch.transferSameItemsButton);
            Enum.TryParse(quickslotButton.Value.ToString(), out QuickSlots_Patch.quickslotButton);
            Battery_Patch.notRechargableBatteries = ParseSetFromString(notRechargableBatteries.Value);
            //Main.logger.LogInfo("decayingFood str  " + decayingFood.Value);
            //Main.logger.LogInfo("decayingFood.Count  " + Food_Patch.decayingFood.Count);
            Player_Movement.CacheSettings();
            SeaTruck_movement.CacheSettings();
            Damage_.bloodColor = ParseBloodColor(bloodColor.Value);
            Player_Movement.waterSpeedEquipment = ParseSpeedEquipmentDic(waterSpeedEquipment.Value);
            Player_Movement.groundSpeedEquipment = ParseSpeedEquipmentDic(groundSpeedEquipment.Value);
        }

        private static Color ParseBloodColor(string input)
        {
            //Main.logger.LogMessage("ParseBloodColor " + input);
            float r = float.MaxValue;
            float g = float.MaxValue;
            float b = float.MaxValue;
            string[] entries = input.Split(' ');
            for (int i = 0; i < entries.Length; i++)
            {
                try
                {
                    if (r == float.MaxValue)
                        r = float.Parse(entries[i]);
                    else if (g == float.MaxValue)
                        g = float.Parse(entries[i]);
                    else if (b == float.MaxValue)
                        b = float.Parse(entries[i]);
                }
                catch (Exception)
                {
                    break;
                }
            }
            if (r == float.MaxValue || g == float.MaxValue || b == float.MaxValue)
            {
                Main.logger.LogWarning("Could not parse blood color: " + input);
                return new Color(0.784f, 1f, 0.157f);
            }
            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }

        public enum Button
        {
            Jump,
            PDA,
            Deconstruct,
            Exit,
            LeftHand,
            RightHand,
            CycleNext,
            CyclePrev,
            AltTool,
            TakePicture,
            Reload,
            Sprint,
            LookUp,
            LookDown,
            LookLeft,
            LookRight,
            None
        }

    }
}
