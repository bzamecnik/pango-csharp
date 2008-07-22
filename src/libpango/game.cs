using System;
using System.Collections.Generic;
using System.Text;

// $Id$

namespace Pango
{
    public class Game
    {
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
        public event EventHandler onEnd;

        private Game() {
            schedule = Schedule.Instance;
            newGame();
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
        public int Level {
            get { return level; }
        }
        public PlayerEntity Player {
            get { return player; }
            set { player = value; }
        }
        public void loadMap() {
            map = MapPersistence.FromString(Config.Instance["Game.map"]);
            // set player reference
            foreach (Entity ent in map) {
                if ((ent != null) && (ent is PlayerEntity)) {
                    if (player == null) {
                        player = (PlayerEntity)ent;
                    } else {
                        // set player's coordinates according to map
                        // but retain player object from previous level
                        // TODO: think how to do this in a nicer way
                        player.Coords = ent.Coords;
                        // can't set ent directly (because of foreach cycle)
                        map.getPlace(ent.Coords).NonWalkable = player;
                    }
                    break;
                }
            }
        }
        private void nextLevel() {
            level++;
            // TODO: compute and set time bonus
            newLevelShared();
        }
        private void newGame() {
            level = 1;
            money = 0;
            player = null;
            newLevelShared();
        }
        private void newLevelShared() {
            schedule.clear();
            loadMap(); // sets player
            state = States.Prepared;
        }
        public void start() {
            if ((state == States.Prepared) || (state == States.Paused)) {
                state = States.Running;
                onStart(this, new EventArgs());
            }
        }
        public void endLevel() {
            if (state == States.Finished) { return; }
            state = States.Finished;
            
            if (player == null) {
                // the player have died, make a new whole game
                newGame();
            } else {
                // all monster were killed (and all remaining bonuses collected?)
                // * start a next level
                nextLevel();
            }
            onEnd(this, new EventArgs());
        }
        public void endGame() {
            newGame();
            onEnd(this, new EventArgs());
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
            Random r = new Random();
            // TODO: put this constants into config
            // * or it could change accoring to level difficulty
            if (r.Next(15) == 0) {
                addRandomBonuses();
            }

            schedule.increaseTime();

            if ((map.Monsters.Count <= 0) || (!turnNotEmpty && schedule.empty())) {
                // empty loop detected (and nothing left in the schedule)
                // or all monsters are killed -> exit level
                endLevel();
                return false;
            }
            return true;
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
