using System;
using System.Collections.Generic;
using System.Text;

// $Id$

namespace Pango
{
    // ----- class Game ----------------------------------------
    public class Game
    {
        // ----- fields --------------------

        private static Game instance = null; // a singleton
        Map map;
        Schedule schedule;
        public enum States { Intro, Prepared, Running, Paused, Finishing, Finished };
        States state;
        int level;
        // Money that player collected for killing monsters,
        // collecting bonuses, aligning diamonds, etc.
        // * No multiplayer for now.
        int money;
        PlayerEntity player;
        Random random;

        // ----- events --------------------

        public event EventHandler onLoopStep;
        public event EventHandler onStart;
        public event EventHandler onPause;
        public event EventHandler onEnd;
        public event EventHandler onLoadMap;

        // ----- constuctor --------------------

        private Game() {
            state = States.Intro;
            schedule = new Schedule();
            random = new Random();
            newGame();
        }

        // ----- properties --------------------

        public static Game Instance {
            get {
                if (instance == null) {
                    instance = new Game();
                }
                return instance;
            }
        }
        public Map Map {
            get { return map; }
            set {
                if ((Game.Instance.State != Game.States.Running)
                  || (Game.Instance.State != Game.States.Paused)) {
                    map = value;
                }
            }
        }
        public Schedule Schedule {
            get { return schedule; }
            set {
                if ((Game.Instance.State != Game.States.Running)
                  || (Game.Instance.State != Game.States.Paused)) {
                    schedule = value;
                }
            }
        }
        public States State {
            get { return state; }
        }
        public int Time {
            get { return schedule.Time; }
        }
        public int Money {
            get { return money; }
        }
        public int Level {
            get { return level; }
        }
        public PlayerEntity Player {
            get { return player; }
            set { player = value; }
        }
        public Random Random {
            get { return random; }
        }

        // ----- methods --------------------

        // loads map from Config and set player reference
        public void loadMap() {
            map = MapPersistence.FromString(Config.Instance[getMapName()]);
            // set player reference
            foreach (Entity ent in map) {
                if ((ent != null) && (ent is PlayerEntity)) {
                    if (player == null) {
                        player = (PlayerEntity)ent;
                    } else {
                        // Set player's coordinates according to map
                        // but retain player object from previous level.
                        // Reset state to Normal.
                        // TODO: think how to do this in a nicer way
                        PlayerEntity p = new PlayerEntity(player);
                        p.Coords = ent.Coords;
                        // can't set ent directly (because of foreach cycle)
                        map.getPlace(ent.Coords).NonWalkable = p;
                        player = p;
                    }
                    break;
                }
            }
            if (onLoadMap != null) {
                onLoadMap(Game.Instance, new EventArgs());
            }
        }
        private string getMapName() {
            int count = Config.Instance.getInt("Game.mapCount");
            if (count > 0) {
                // level and maps counted from 1
                // maps will eventually repeat
                return string.Format("Game.map.{0}", ((level - 1) % count) + 1);
            } else {
                return "";
            }
        }

        private void nextLevel() {
            level++;
            // TODO: compute and set time bonus
            // -exp(...)
            // eg. 1000 points in 0 time, 10 points in 1000 time
            newLevelShared();
            state = States.Prepared;
        }

        private void newGame() {
            level = 1;
            money = 0;
            player = null;
            newLevelShared();
            state = States.Intro;
        }

        private void newLevelShared() {
            schedule.clear();
            schedule.clearTime();
            if (state == States.Finished) {
                loadMap(); // sets player
            }
        }

        public void start() {
            switch (state) {
                case States.Intro:
                    state = States.Prepared;
                    break;
                default:
                    state = States.Running;
                    if (onStart != null) {
                        onStart(this, new EventArgs());
                    }
                    break;
            }
        }

        public void endLevel() {
            if (state == States.Finished) { return; }
            state = States.Finishing;
            // wait some time
            if (onEnd != null) {
                onEnd(this, new EventArgs());
            }
            int timeBeforeLevel = Config.Instance.getInt("Game.timeBeforeLevel");
            if (player == null) {
                // the player have died, make a new whole game
                Game.Instance.Schedule.add(delegate() {
                    state = States.Finished;
                    newGame();
                }, timeBeforeLevel);
            } else {
                // all monsters were killed (and all remaining bonuses collected?)
                // TODO: give a bonus money for completing a level
                int moneyForLevel = Config.Instance.getInt("Game.moneyForLevel");
                Game.Instance.Schedule.add(delegate() {
                    state = States.Finished;
                    nextLevel();
                    receiveMoney(moneyForLevel);
                }, timeBeforeLevel);
            }
        }

        public void endGame() {
            state = States.Finishing;
            onEnd(this, new EventArgs());
            // wait some time
            int timeBeforeLevel = Config.Instance.getInt("Game.timeBeforeLevel");
            Game.Instance.Schedule.add(delegate() {
                endGameImmediately();
            }, timeBeforeLevel);
        }
        public void endGameImmediately() {
            state = States.Finished;
            newGame();
            state = States.Intro;
        }

        public void pause() {
            switch (state) {
                case States.Running: 
                    state = States.Paused;
                    onPause(this, new EventArgs());
                    break;
                case States.Paused:
                    start();
                    break;
            }
        }

        // returns true if game continues
        public bool step() {
            bool turnNotEmpty = false;

            onLoopStep(this, new EventArgs());
            if (state == States.Finishing) {
                schedule.increaseTime();
                schedule.callCurrentEvents();
                return true;
            }
            if (state != States.Running) { return false; }

            // call events for this time in the queue
            schedule.callCurrentEvents();

            // prevent multiple turn() calls in one step for entities
            // which moved forward
            foreach (Entity ent in map) {
                ent.turnDone = false;
            }

            // let all entities in the map perform their turn
            foreach (Entity ent in map) {
                // if something was made turnNotEmpty is set true
                if (!ent.turnDone) {
                    turnNotEmpty |= ent.turn();
                    ent.turnDone = true;
                }
            }
            
            // sometimes add bonuses at a random place
            // * the factor could change accoring to level difficulty
            // * don't add bonuses, when finishing (ie. all monsters are killed)
            if (map.Monsters.Count > 0) {
                int bonusAddProbability = Config.Instance.getInt("Game.bonusAddProbability");
                if (random.Next(bonusAddProbability) == 0) {
                    addRandomBonus();
                }
            }

            schedule.increaseTime();

            if (!turnNotEmpty && schedule.empty()) {
                // empty loop detected (and nothing left in the schedule)
                endGame();
            }
            if ((map.Monsters.Count <= 0) && (map.Bonuses.Count <= 0)) {
                // or all monsters are killed -> exit level
                endLevel();
            }
            return true;
        }

        public void receiveMoney(int amount) {
            money += amount;
        }

        // add randomly selected bonus at a random place
        private void addRandomBonus() {
            Entity bonus = null;
            // distribute probablity among various bonuses
            int chance = random.Next(10);
            if (chance <= 5) {
                bonus = new MoneyBonus();
            } else if ((chance > 5) && (chance <= 8)) {
                bonus = new HealthBonus();
            } else {
                bonus = new LiveBonus();
            }
            if (bonus != null) {
                map.add(bonus, map.getRandomWalkableCoords());
            }
        }
    }
}
