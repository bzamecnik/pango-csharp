using System;
using System.Collections.Generic;
using System.Text;

namespace Pango
{
    public class Game
    {
        // TODO: Game can contain:
        // * quotas for monsters (initial), bonuses, etc.
        // * time limit (?)
        private static Game instance = null; // a singleton
        Map map;
        Schedule schedule;
        public enum States { Prepared, Running, Paused, Finished };
        States state;
        int level;
        // Money that player collected for killing monsters,
        // collecting bonuses, aligning diamonds, etc.
        // * No multiplayer for now.
        int money;
        PlayerEntity player;
        public event EventHandler loopStep;
        public event EventHandler onStart;
        public event EventHandler onPause;

        private Game() {
            schedule = Schedule.Instance;
            state = States.Prepared;
            level = 1;
            money = 0;
            player = null;
            map = null;
        }
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
        public States State {
            get { return state; }
        }
        public int Time {
            get { return schedule.Time; }
        }
        public int Money {
            get { return money; }
        }
        public PlayerEntity Player {
            get { return player; }
            set { player = value; }
        }
        public void loadMap(Map loadedMap) {
            map = loadedMap;
            // set player reference
            foreach (Entity ent in map) {
                if ((ent != null) && (ent is PlayerEntity)) {
                    player = (PlayerEntity)ent;
                    break;
                }
            }
        }
        private void nextLevel() {
            level++;
            // loadMap();
            state = States.Prepared;
        }
        public void start() {
            if ((state == States.Prepared) || (state == States.Paused)) {
                state = States.Running;
                onStart(this, new EventArgs());
            }
        }
        public void end() {
            if (state == States.Finished) { return; }
            state = States.Finished;
            if (player == null) {
                // the player have died
                // * exit the whole game    
            } else {
                // all monster were killed (and all remaining bonuses collected)
                // * start a new game (next level)
                //nextLevel(); // TODO: call it manually
                //start();
            }
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

            loopStep(this, new EventArgs());

            if (state != States.Finished){
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
                Random r = new Random();
                // TODO: put this constants into config
                if (r.Next(15) == 0) {
                    addRandomBonuses();
                }

                schedule.increaseTime();

                if (!turnNotEmpty && schedule.empty()) {
                    // empty loop detected (and nothing left in the schedule)
                    // -> exit game
                    end();
                    return false;
                }
                return true;
            }
            return false;
        }
        public void receiveMoney(int amount) {
            money += amount;
        }
        // add randomly selected bonus at a random place
        private void addRandomBonuses() {
            Random r = new Random();
            Entity bonus = null;
            // distribute probablity amongs various bonuses
            int chance = r.Next(10);
            if (chance <= 5) {
                bonus = new MoneyBonus();
            } else if ((chance > 5) && (chance <= 7)) {
                bonus = new LiveBonus();
            } else {
                bonus = new HealthBonus();
            }
            if (bonus != null) {
                map.add(bonus, map.getRandomWalkablePlace());
            }
        }
    }
}
