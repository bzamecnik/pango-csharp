using System;
using System.Collections.Generic;
using System.Text;

namespace Pango
{
    public class Config
    {
        // * Singleton wrapper over dictionary (for now)
        // * Later it could load/save configuration from/into a file
        
        // Note: both key and value is of type string for future.
        // There may be some values of other types than int.
        private Dictionary<string, string> values;
        private static Config instance;

        private Config() {
            values = new Dictionary<string, string>();

            // Default values
            addInt("PlayerEntity.maxHealth", 100);
            addInt("PlayerEntity.defaultLives", 3);
            addInt("PlayerEntity.timeToRespawn", 2);
            addInt("PlayerEntity.attackHitcount", 25);

            addInt("MonsterEntity.moneyForKilling", 10);
            addInt("MonsterEntity.attackHitcount", 40);
            addInt("MonsterEntity.maxHealth", 50);
            addInt("MonsterEntity.defaultLives", 1);
            addInt("MonsterEntity.timeToRespawn", 5);

            addInt("Bonus.timeToLive", 20);

            addInt("MoneyBonus.bonusMoney", 50);

            addInt("HealthBonus.changeHealthPercent", 25);

            addInt("Game.mapWidth", 20);
            addInt("Game.mapHeight", 20);
        }
        public static Config Instance {
            get {
                if (instance == null) {
                    instance = new Config();
                }
                return instance;
            }
        }
        public string this[string key]{
            get { return values[key]; }
            set { values[key] = value; }
        }
        public void addInt(string key, int value) {
            this[key] = value.ToString();
        }
        public int getInt(string key) {
            return Int32.Parse(this[key]);
        }
    }
}
