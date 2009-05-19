using System;
using System.Collections.Generic;
using System.Text;

// $Id$

namespace Pango
{
    // ----- class Config ----------------------------------------
    public class Config
    {
        // Singleton wrapper over dictionary (for now).
        // Later it could load/save configuration from/into a file
        // or ApplicationSettings.

        // ----- fields --------------------

        // NOTE: both key and value are of string type.
        // There might be some values of other types than int.
        private Dictionary<string, string> values;
        private static Config instance;

        // ----- constructors --------------------

        private Config() {
            values = new Dictionary<string, string>();

            // Default values
            addInt("PlayerEntity.maxHealth", 100);
            addInt("PlayerEntity.defaultLives", 3);
            addInt("PlayerEntity.timeToRespawn", 7); // in game ticks
            addInt("PlayerEntity.attackHitcount", 25);
            addInt("PlayerEntity.timeToVanishDead", 5);

            addInt("MonsterEntity.attackHitcount", 40);
            addInt("MonsterEntity.defaultLives", 0);
            addInt("MonsterEntity.maxHealth", 75);
            addInt("MonsterEntity.moneyForKilling", 100);
            addInt("MonsterEntity.slowFactor", 2);
            addInt("MonsterEntity.timeToIncubate", 5); // in game ticks
            addInt("MonsterEntity.timeToRespawn", 25); // in game ticks
            addInt("MonsterEntity.timeToWakeupDiamond", 30); // in game ticks
            addInt("MonsterEntity.timeToWakeupWall", 10); // in game ticks

            addInt("Bonus.timeToLive", 40); // in game ticks
            addInt("MoneyBonus.bonusMoney", 20);
            addInt("HealthBonus.changeHealthPercent", 25);
            addInt("IceBlock.timeToMelt", 1); // in game ticks
            addInt("DiamondBlock.bonusMoney", 1000);

            addInt("Game.bonusAddProbability", 15); // probabilitiy: 1/bonusAddProbability
            addInt("Game.mapCount", 0); // number of levels in the map
            values["Game.mapFile"] = "maps.txt";
            addInt("Game.stepInterval", 150); // in milliseconds
            addInt("Game.timeBeforeLevel", 10); // in game ticks
            addInt("Game.moneyForLevel", 200); // money bonus for level completion
        }

        // ----- properties --------------------

        public static Config Instance {
            get {
                if (instance == null) {
                    instance = new Config();
                }
                return instance;
            }
        }
        
        // ----- indexers --------------------
        
        public string this[string key]{
            get { return values[key]; }
            set { values[key] = value; }
        }

        // ----- methods --------------------

        public void addInt(string key, int value) {
            this[key] = value.ToString();
        }
        public int getInt(string key) {
            return Int32.Parse(this[key]);
        }
    }
}
