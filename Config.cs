using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Options;

namespace Tweaks_Fixes
{ 
    [Menu("Tweaks and Fixes")]
    public class Config : ConfigFile
    {
        //[Slider("Day/night cycle speed", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = ""), OnChange(nameof(UpdateGameSpeed))]
        //public float gameSpeed = 1f;
        //[Slider("Day/night cycle speed 10x mult", 0, 4, DefaultValue = 1, Step = 1, Format = "{0:F0}", Tooltip = ""), OnChange(nameof(UpdateGameSpeed))]
        //public int gameSpeedMult = 1;
        [Slider("Player speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Your swimming, walking and running speed will be multiplied by this.")]
        public float playerSpeedMult = 1f;
        //[Slider("Player damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage player takes will be multiplied by this.")]
        //public float playerDamageMult = 1f;
        //[Slider("Vehicle damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage your vehicles take will be multiplied by this.")]
        //public float vehicleDamageMult = 1f;
        [Slider("Prawn suit speed multiplier", .5f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Your prawn suit speed will be multiplied by this.")]
        public float vehicleSpeedMult = 1f;
        [Slider("Seatruck speed multiplier", .5f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Your seatruck speed will be multiplied by this.")]
        public float seatruckSpeedMult = 1f;
        [Slider("Snowfox speed multiplier", .5f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Your snowfox speed will be multiplied by this.")]
        public float snowfoxSpeedMult = 1f;
        //[Slider("Predator aggression multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "The higher it is the more aggressive predators are towards you. When it's 0 you and your vehicles will never be attacked. When it's 3 predators attack you on sight and never flee.")]
        //public float aggrMult = 1f;
        [Slider("Oxygen per breath", 0f, 6f, DefaultValue = 3f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Amount of oxygen you consume every breath.")]
        public float oxygenPerBreath = 3f;
        [Slider("Getting cold rate multiplier", 0f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "The rate at which you get cold when outside and get warm when inside will be multiplied by this")]
        public float coldMult = 1f;
        [Slider("First aid kit HP", 10, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "HP restored by using first aid kit.")]
        public int medKitHP = 50;
        //[Slider("First aid kit HP per second", 1, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "HP restored every second after using first aid kit.")]
        [Toggle("Can't use first aid kit underwater", Tooltip = "")]
        public bool cantUseMedkitUnderwater = false;
        [Slider("Crafting time multiplier", 0.1f, 4f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Crafting time will be multiplied by this when crafting things with fabricator or modification station.")]
        public float craftTimeMult = 1f;
        [Slider("Building time multiplier", 0.1f, 4f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Building time will be multiplied by this when using builder tool.")]
        public float buildTimeMult = 1f;
        [Toggle("Player movement tweaks", Tooltip = "Player vertical, backward, sideways movement speed is halved. Any diving suit reduces your speed by 10% on land and in water. Fins reduce your speed by 10% on land. Lightweight high capacity tank reduces your speed by 5%. Every other tank reduces your speed by 10% on both land and water. Camera now does not bob up and down when swimming. You can sprint only if moving forward. Seaglide works only if moving forward. When swimming while your PDA is open your movement speed is halved. When swimming while holding a tool in your hand your movement speed is reduced to 70%.")]
        public bool playerMoveTweaks = false;
        [Toggle("Only ambient tempterature makes player warm", Tooltip = "In vanilla game when you are underwater you get warm if moving and get cold when not. When out of water some areas (caves, your unpowered base) make you warm regardless of ambient tempterature. With this on you get warm only if ambient temperature is above getWarmTemp setting.")]
        public bool useRealTempForColdMeter = false;
        [Slider("Inventory weight multiplier in water", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are swimming you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultWater = 0f;
        [Slider("Inventory weight multiplier on land", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are on land you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultLand = 0f;
        [Toggle("Snowfox movement tweaks", Tooltip = "Snowfox can move on water as good as on land. Its backward and sideways speed is halved. You can boost for as long as you hold the sprint button. When boosting power consumption is doubled.")]
        public bool hoverbikeMoveTweaks = false;
        [Toggle("Prawn suit movement tweaks", Tooltip = "Prawn suit can not move sideways. No time limit when using thrusters, but they consume twice more power. Vertical speed is reduced when using thrusters. Can't use thrusters to hover above ground when out of water.")]
        public bool exosuitMoveTweaks = false;
        [Toggle("Seatruck movement tweaks", Tooltip = "Seatruck's vertical, sideways and backward speed is halved. Afterburner is active for as long as you hold the 'sprint' key but consumes twice more power. Horsepower upgrade increases seatruck's speed by 10%. You can install more than 1 Horsepower upgrade.")]
        public bool seatruckMoveTweaks = false;
        [Toggle("Always use best LOD models", Tooltip = "A lot of models in the game use different levels of detail depending on how close you are to them. Some of them look different and you can see those objects change as you approach them. With this on best LOD models will always be used. It will affect the game's performance, but with a good GPU it should not be noticable. The game has to be reloaded after changing this.")]
        public bool alwaysBestLOD = false;
        [Choice("Unmanned vehicles can be attacked", Tooltip = "When 'Only_if_lights_on' is selected, you have to unpower your seatruck to prevent attacks on it.")]
        public EmptyVehicleCanBeAttacked emptyVehicleCanBeAttacked;
        [Slider("Drop pod max power", 0, 100, DefaultValue = 0, Step = 5, Format = "{0:F0}", Tooltip = "If this is not 0 your drop pod's max power will be set to this. Drop pod's power will regenerate during the day. The game has to be reloaded after changing this.")]
        public int dropPodMaxPower = 0;
        [Slider("Fruit growth time", 0, 50, DefaultValue = 1, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes a lantern tree fruit, a frost anemone heart, a creepvine seed cluster, a Preston's plant fruit to grow. You have to reload your game after changing this.")]
        public int fruitGrowTime = 1;
        [Toggle("Do not spawn fragments for unlocked blueprints", Tooltip = "You have to reload your game after changing this.")]
        public bool dontSpawnKnownFragments = false;

        [Slider("Crush depth", 50, 500, DefaultValue = 200, Step = 10, Format = "{0:F0}", Tooltip = "Depth below which player starts taking damage. Does not work if crush damage multiplier is 0.")]
        public int crushDepth = 200;
        [Slider("Crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 every 3 seconds player takes 1 damage multiplied by this for every meter below crush depth.")]
        public float crushDamageMult = 0f;
        [Slider("Vehicle crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0, every 3 seconds vehicles take 1 damage multiplied by this for every meter below crush depth.")]
        public float vehicleCrushDamageMult = 0f;
        //[Toggle("Instantly open PDA", Tooltip = "Your PDA will open and close instantly. Direction you are looking at will not change when you open it. Game has to be reloaded after changing this.")]
        public bool instantPDA = false;
        [Slider("Hunger update interval", 1, 100, DefaultValue = 10, Step = 1, Format = "{0:F0}", Tooltip = "Time interval in seconds after which your hunger and thirst update.")]
        public int hungerUpdateInterval = 10;
        [Toggle("New hunger system", Tooltip = "You don't regenerate health when you are full. When you sprint you get hungry and thirsty twice as fast. You don't lose health when your food or water value is 0. Your food and water values can go as low as -100. When your food or water value is below 0 your movement speed will be reduced proportionally to that value. When either your food or water value is -100 your movement speed will be reduced by 50% and you will start taking hunger damage. Your max food and max water value is 200. The higher your food value above 100 is the less food you get when eating: when your food value is 110 you lose 10% of food, when it's 190 you lose 90%.")]
        public bool newHungerSystem = false;

        [Choice("Eating raw fish", Tooltip = "When it's not vanilla, amount of food you get by eating raw fish changes. Harmless: it's a random number between 0 and fish's food value. Risky: it's a random number between fish's food negative value and fish's food value. Harmful: it's a random number between fish's food negative value and 0.")]
        public EatingRawFish eatRawFish;
        [Toggle("Food tweaks", Tooltip = "Raw fish water value is half of its food value. Cooked rotten fish has no food value. Eating outside decreases your warmth. Game has to be reloaded after changing this.")]
        public bool foodTweaks = false;
        [Toggle("Thermoblade cooks fish on kill", Tooltip = "")]
        public bool heatBladeCooks = true;
        [Toggle("Can't eat underwater", Tooltip = "You won't be able to eat or drink underwater.")]
        public bool cantEatUnderwater = false;

        [Slider("Food decay rate multiplier", 0.1f, 2f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "Food decay rate will be multiplied by this. The game has to be reloaded after changing this.")]
        public float foodDecayRateMult = 1f;
        [Slider("Water freeze rate multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Bottled water will freeze at this rate if ambient temperature is below 0 C°. The game has to be reloaded after changing this.")]
        public float waterFreezeRate = 1f;
        [Slider("Snowball water value", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "When you eat a snowball, you will get this amount of water and lose this amount of warmth. The game has to be reloaded after changing this.")]
        public int snowballWater = 0;
        [Slider("Catchable fish speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Swimming speed of fish that you can catch will be multiplied by this.")]
        public float fishSpeedMult = 1f;
        [Slider("Other creatures speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Swimming speed of creatures that you can't catch will be multiplied by this.")]
        public float creatureSpeedMult = 1f;
        //[Slider("Egg hatching period multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Time it takes a creature egg to hatch in AC will be multiplied by this.")]
        //public float eggHatchTimeMult = 1f;
        //[Slider("Plant growth time multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Time it takes a plant to grow will be multiplied by this.")]
        //public float plantGrowthTimeMult = 1f;
        [Slider("Knife attack range multiplier", 1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeRangeMult = 1f;
        [Slider("Knife damage multiplier", 1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeDamageMult = 1f;

        [Toggle("Can't catch fish with bare hands", Tooltip = "To catch fish you will have to use propulsion cannon or grav trap. Does not apply if you are inside alien containment.")]
        public bool noFishCatching = false;
        [Toggle("Can't break outcrop with bare hands", Tooltip = "You will have to use a knife to break outcrops or collect resources attached to rock or seabed. A piece of scrap metal will spawn next to your crashed ship, so you can craft knife.")]
        public bool noBreakingWithHand = false;
        [Toggle("Disable tutorial messages", Tooltip = "Disable messages that tell you how controls work.")]
        public bool disableHints = false;
        [Toggle("Realistic oxygen consumption", Tooltip = "Vanilla game oxygen consumption has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth between 100 and 0 meters. With this on your oxygen consumption will increase in linear progression using 'Crush depth' setting. When you are at crush depth it will be vanilla max oxygen consumption and will increase as you dive deeper.")]
        public bool realOxygenCons = false;

        [Toggle("Drop held tool when taking damage", Tooltip = "Chance to drop your tool is proportional to amount of damage taken. If you take 30 damage, there is 30% chance you will drop your tool.")]
        public bool dropHeldTool = false;

        //[Toggle("Predators less likely to flee", Tooltip = "Predators don't flee when their health is above 50%. When it's not, chance to flee is proportional to their health. The more health they have the less likely they are to flee.")]
        //public bool predatorsDontFlee = false;
        //[Toggle("Every creature respawns", Tooltip = "By default big creatures never respawn if killed by player.")]
        //public bool creaturesRespawn = false;
        [Choice("Creatures respawn if killed by player", Tooltip = "By default big creatures and leviathans never respawn if killed by player.")]
        public CreatureRespawn creatureRespawn;
        //public bool creaturesRespawn = false;
        [Slider("Fish respawn time", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes small fish to respawn after it was killed or caught. If it's 0, default (6 hours) value will be used. Game has to be reloaded after changing this.")]
        public int fishRespawnTime = 0;
        [Slider("Big creatures respawn time", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes a creature that you can't catch to respawn after it was killed. If it's 0, default (12 hours) value will be used. Game has to be reloaded after changing this.")]
        public int creatureRespawnTime = 0;
        [Slider("Leviathan respawn time", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes a leviathan to respawn after it was killed. If it's 0, default (1 day) value will be used. Game has to be reloaded after changing this.")]
        public int leviathanRespawnTime = 0;
        //[Slider("flare light intensity", 0.1f, 1f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "You have to reequip your flare after changing this.")]
        //public float flareIntensity = 1f;
        //[Toggle("Unlock prawn suit only by scanning prawn suit", Tooltip = "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arms. Game has to be reloaded after changing this.")]
        //public bool cantScanExosuitClawArm = false;

        //[Toggle("Remove light from open databox", Tooltip = "Disable databox light when you open it so it does not draw your attention next time you see it. Game has to be reloaded after changing this.")]
        //public bool disableDataboxLight = false;
        //[Slider("Life pod power cell max charge", 10, 100, DefaultValue = 25, Step = 1, Format = "{0:F0}", Tooltip = "Max charge for each of its 3 power cells. Game has to be reloaded after changing this.")]
        //public int escapePodMaxPower = 25;
        //[Toggle("Life pod power tweaks", Tooltip = "When your life pod is damaged its max power is reduced to 50%. When you crashland your life pod's power cells are 30% charged. Game has to be reloaded after changing this.")]
        //public bool escapePodPowerTweak = false; 
        [Slider("Tool power consumption multiplier", 0f, 4f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of power consumed by your tools will be multiplied by this.")]
        public float toolEnergyConsMult = 1f;
        [Slider("Vehicle power consumption multiplier", 0f, 4f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of power consumed by your vehicles will be multiplied by this.")]
        public float vehicleEnergyConsMult = 1f;
        [Slider("Base power consumption multiplier", 0f, 4f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of power consumed by things in your base will be multiplied by this. Leave this at 1 if using EasyCraft mod.")]
        public float baseEnergyConsMult = 1f;
        [Slider("Crafted battery charge percent", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Charge percent of batteries and power cells you craft will be set to this.")]
        public int craftedBatteryCharge = 100;

        [Slider("Free torpedos", 0, 6, DefaultValue = 2, Step = 1, Format = "{0:F0}", Tooltip = "Number of torpedos you get when installing Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo Arm.")]
        public int freeTorpedos = 2;
        [Toggle("Only Vehicle upgrade console can craft vehicle upgrades", Tooltip = "Fabricator will not be able to craft vehicle upgrades. Game has to be reloaded after changing this.")]
        public bool craftVehicleUpgradesOnlyInMoonpool = false;
        [Choice("Losing items when you die", Tooltip = "When set to 'All' you will drop every item in your inventory when you die.")]
        public LoseItemsOnDeath loseItemsOnDeath;
        //[Toggle("Can't repair vehicles in water", Tooltip = "")]
        //public bool cantRepairVehicleInWater = false;
        [Toggle("No particles when creature dies", Tooltip = "No particles (yellow cloud) will spawn when a creature dies. Game has to be reloaded after changing this.")]
        public bool noKillParticles = false;
        [Toggle("Always show health and food values in UI", Tooltip = "Health and food values will be always shown not only when PDA is open.")]
        public bool alwaysShowHealthNunbers = false;
        [Slider("Brinewing freeze damage", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "When this is not 0 brinewing attack will drain your cold meter by this amount instead of freezing you.")]
        public int brinewingAttackColdDamage = 0;
        [Toggle("Camera bobbing", Tooltip = "Camera bobbing when swimming."), OnChange(nameof(ToggleCameraBobbing))]
        public bool cameraBobbing = true;
        //[Toggle("Turn off lights in your base"), OnChange(nameof(UpdateBaseLight))]
        //public bool baseLightOff = false;
        //[Toggle("PDA clock", Tooltip = " After changing this you have to reload the game.")]
        public bool pdaClock = true;
        [Keybind("Quickslot cycle key", Tooltip = "Press 'Cycle next' or 'Cycle previous' key while holding down this key to cycle tools in your current quickslot.")]
        public KeyCode quickslotKey = KeyCode.LeftAlt;
        [Keybind("Move all items key", Tooltip = "When you have a container open, hold down this key and click an item to move all items.")]
        public KeyCode transferAllItemsKey = KeyCode.LeftControl;
        [Keybind("Move the same items key", Tooltip = "When you have a container open, hold down this key and click an item to move all items of the same type.")]
        public KeyCode transferSameItemsKey = KeyCode.LeftShift;

        //public float playerCamRot = -1f;
        public int activeSlot = -1;
        //public Dictionary<string, bool> escapePodSmokeOut = new Dictionary<string, bool>();
        public HashSet<TechType> notPickupableResources = new HashSet<TechType>
        {{TechType.Salt}, {TechType.Quartz}, {TechType.AluminumOxide}, {TechType.Lithium} , {TechType.Sulphur}, {TechType.Diamond}, {TechType.Kyanite}, {TechType.Magnetite}, {TechType.Nickel}, {TechType.UraniniteCrystal}  };
        //public Dictionary<string, Dictionary<int, bool>> openedWreckDoors = new Dictionary<string, Dictionary<int, bool>>();
        public float medKitHPtoHeal = 0f;
        public float getWarmTemp = 15f;
        public float vehicleTemp = 20f;

        public Dictionary<string, Dictionary<string, bool>> baseLights = new Dictionary<string, Dictionary<string, bool>>();
        public Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>> lockerNames = new Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>>();
        public Dictionary<string, int> startingLoot = new Dictionary<string, int>
        {
             { "FilteredWater", 0 },
             { "NutrientBlock", 0 },
             { "Flare", 0 },
        };
        public Dictionary<string, int> crushDepthEquipment = new Dictionary<string, int>
        {
             { "ReinforcedDiveSuit", 0 },
        };
        public Dictionary<string, float> itemMass = new Dictionary<string, float>
        {
            { "ScrapMetal", 120 },
        };
        public Dictionary<string, float> bloodColor = new Dictionary<string, float>
        {
            { "Red", 0.784f },
            { "Green", 1f },
            { "Blue", 0.157f },
        };
        //public HashSet<string> nonRechargeable = new HashSet<string>{
        //    { "someBattery" },
        //};
        public Dictionary<string, float> damageMult_ = new Dictionary<string, float> { { "Creepvine", 1f } };
        public Dictionary<string, float> podPower = new Dictionary<string, float>();

        public HashSet<string> gravTrappable = new HashSet<string>{
            { "seaglide" },
            { "airbladder" },
            { "flare" },
            { "flashlight" },
            { "builder" },
            { "lasercutter" },
            { "ledlight" },
            { "divereel" },
            { "propulsioncannon" },
            { "welder" },
            { "repulsioncannon" },
            { "scanner" },
            { "stasisrifle" },
            { "knife" },
            { "heatblade" },
            { "metaldetector" },
            { "teleportationtool" },
            
            { "precursorkey_blue" },
            { "precursorkey_orange" },
            { "precursorkey_purple" },

            { "suitboostertank" }, 
            { "coldsuit" },
            { "flashlighthelmet" },
            { "coffee" },
            { "compass" },
            { "fins" },
            { "fireextinguisher" },
            { "firstaidkit" },
            { "doubletank" },
            { "plasteeltank" },
            { "radiationsuit" },
            { "radiationhelmet" },
            { "radiationgloves" },
            { "rebreather" },
            { "reinforceddivesuit" },
            { "maproomhudchip" },
            { "tank" },
            { "stillsuit" },
            { "swimchargefins" },
            { "ultraglidefins" },
            { "highcapacitytank" },
        };
        public float medKitHPperSecond = 50f;
        public HashSet<TechType> predatorExclusion = new HashSet<TechType> { TechType.Crash};
        public Dictionary<string, bool> iceFruitPickedState = new Dictionary<string, bool> ();

        //static void UpdateGameSpeed()
        //{
        //    if (DayNightCycle.main)
        //        DayNightCycle.main._dayNightSpeed = Main.config.gameSpeed;
        //}

        static void UpdateBaseLight()
        {
            //if (Main.loadingDone)
            //{
            //    Base_Light.UpdateBaseLights();
            //}
        }

        static void ToggleCameraBobbing()
        {
            MiscSettings.cameraBobbing = !MiscSettings.cameraBobbing;
        }

        public enum EatingRawFish { Vanilla, Harmless, Risky, Harmful }
        public enum LoseItemsOnDeath { Vanilla, All, None }
        public enum EmptyVehicleCanBeAttacked { Yes, No, Only_if_lights_on }
        public enum CreatureRespawn { Vanilla, Big_creatures_only, Leviathans_only, Big_creatures_and_leviathans }
        public bool silentReactor = false;
        public bool randomPlantRotation = true;
        public bool fixMelons = true;
        public bool fixCoral = true;
        public bool seaGlideMap = true;
        //private void EatRawFishChangedEvent(ChoiceChangedEventArgs e)
        //{
        //    AddDebug("EatRawFishChangedEvent " + eatRawFish); 
        //}
        public List<string> translatableStrings = new List<string>
        {"Burnt out ", // used for flare
         "Lit ",// 1 used for flare
         "Increases the Seatruck engine's horsepower and energy consumption by 10%. More than 1 can be installed.", // 2 SeaTruckUpgradeHorsePower my desc
        " frozen", // 3 frozen water
        "Increases your safe diving depth by ", // 4 crushDepthEquipment
        " meters.", // 5 crushDepthEquipment
        "mass ",     // 6 invMultLand invMultWater
        "Throw",    // 7 flare
        "Light and throw",  // 8 flare
        "Light",    // 9 flare   
        ": min ",     // 10    eatRawFish tooltip 
        ", max ",     // 11    eatRawFish tooltip 
        "Need a knife to break it",  // 12  breaking outcrop
        "Need a knife to break it free",  // 13  picking up attached resource
        " Hold ",                   // 14  exosuit UI
        " and press ",               // 15  exosuit UI
        " to change torpedo ",      // 16  exosuit UI
        ", Change torpedo ",         // 17  exosuit UI
        "Break it free",            // 18 Break free attached resource
        "Unique outer membrane has potential as a natural water filter. Provides some oxygen when consumed raw.",   // 19 Bladderfish desc
        "Low-power conduction unit. Can be used to cook fish.", // 20 SmallStove desc
        "Increases the Seatruck's speed when hauling two or more modules.", // 21 SeaTruckUpgradeHorsePower vanilla desc
        "Reduces vehicle energy consumption by 20% percent.", // 22 SeaTruckUpgradeEnergyEfficiency desc
        }; // edit UI_Patches.Getstrings after adding new strings
       

    }

}