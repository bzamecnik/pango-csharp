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

        // NOTE: both key and value is of type string.
        // There may be some values of other types than int.
        private Dictionary<string, string> values;
        private static Config instance;

        // ----- constructors --------------------

        private Config() {
            values = new Dictionary<string, string>();

            // Default values
            addInt("PlayerEntity.maxHealth", 100);
            addInt("PlayerEntity.defaultLives", 3);
            addInt("PlayerEntity.timeToRespawn", 7);
            addInt("PlayerEntity.attackHitcount", 25);
            addInt("PlayerEntity.timeToVanishDead", 5);

            addInt("MonsterEntity.attackHitcount", 40);
            addInt("MonsterEntity.defaultLives", 0);
            addInt("MonsterEntity.maxHealth", 75);
            addInt("MonsterEntity.moneyForKilling", 100);
            addInt("MonsterEntity.slowFactor", 2);
            addInt("MonsterEntity.timeToIncubate", 5);
            addInt("MonsterEntity.timeToRespawn", 25);
            addInt("MonsterEntity.timeToWakeupDiamond", 30);
            addInt("MonsterEntity.timeToWakeupWall", 10);

            addInt("Bonus.timeToLive", 40);
            addInt("MoneyBonus.bonusMoney", 20);
            addInt("HealthBonus.changeHealthPercent", 25);
            addInt("IceBlock.timeToMelt", 1);
            addInt("DiamondBlock.bonusMoney", 1000);

            addInt("Game.bonusAddProbability", 15);
            addInt("Game.mapCount", 0);
            values["Game.mapFile"] = "maps.txt";
            addInt("Game.stepInterval", 150);
            addInt("Game.timeBeforeLevel", 10);
            addInt("Game.moneyForLevel", 200);
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
