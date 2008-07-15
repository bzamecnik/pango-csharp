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

        private Game() {
            schedule = Schedule.Instance;
            state = States.Prepared;
            level = 1;
            money = 0;
            player = null;
            loadMap();
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
        }
        public States State {
            get { return state; }
        }
        public PlayerEntity Player {
            get { return player; }
            set { player = value; }
        }
        private void loadMap() {
            //int mapWidth = Config.Instance.getInt("Game.mapWidth");
            //int mapHeight = Config.Instance.getInt("Game.mapHeight");
            //map = new Map(mapWidth, mapHeight);
            
            //List<Entity>[,] loadedMap = new List<Entity>[,];
            
            // load from file or generate randomly
            
            //map = new Map(loadedMap);

            // set player reference
        }
        private void nextLevel() {
            level++;
            // loadMap();
        }
        public void start() {
            if ((state == States.Prepared) || (state == States.Paused)) {
                state = States.Running;
                loop();
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
                nextLevel();
                start();
            }
        }
        public void pause() {
            switch (state) {
                case States.Running: 
                    state = States.Paused;
                    // TODO: pause the loop (maybe wait()?)
                    break;
                case States.Paused:
                    start();
                    break;
            }
        }
        private void loop() {
            bool turnNotEmpty = false;

            while (state != States.Finished){
                // TODO: hook place for refreshing the map in the GUI
                // * OR: make s call for a step only

                // call events for this time in the queue
                schedule.callCurrentEvents();

                // let all entities in the map perform their turn
                foreach (Entity ent in map) {
                    // if something was made turnNotEmpty is set true
                    turnNotEmpty |= ent.turn();
                }
                
                schedule.increaseTime();

                if (!turnNotEmpty && schedule.empty()) {
                    // empty loop detected (and nothing left in the schedule)
                    // -> exit game
                    end();
                } else {
                    // Wait some time not to make the game so fast.
                    System.Threading.Thread.Sleep(200);
                    // Think of how to make the turns last the same time.
                }
            }
        }
        public void receiveMoney(int amount) {
            money += amount;
        }
    }
}
