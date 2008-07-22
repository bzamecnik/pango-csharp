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

        // NOTE: both key and value is of type string for future.
        // There may be some values of other types than int.
        private Dictionary<string, string> values;
        private static Config instance;

        // ----- constructors --------------------

        private Config() {
            values = new Dictionary<string, string>();

            // Default values
            addInt("PlayerEntity.maxHealth", 100);
            addInt("PlayerEntity.defaultLives", 3);
            addInt("PlayerEntity.timeToRespawn", 3);
            addInt("PlayerEntity.attackHitcount", 25);

            addInt("MonsterEntity.moneyForKilling", 50);
            addInt("MonsterEntity.attackHitcount", 40);
            addInt("MonsterEntity.maxHealth", 50);
            addInt("MonsterEntity.defaultLives", 0);
            addInt("MonsterEntity.timeToRespawn", 25);
            addInt("MonsterEntity.timeToIncubate", 15);
            addInt("MonsterEntity.timeToWakeupDiamond", 30);
            addInt("MonsterEntity.timeToWakeupWall", 10);

            addInt("Bonus.timeToLive", 40);
            addInt("MoneyBonus.bonusMoney", 100);
            addInt("HealthBonus.changeHealthPercent", 25);
            addInt("DiamondBlock.bonusMoney", 1000);

            addInt("Game.stepInterval", 250); // better 100
            values["Game.map"] = "";
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
