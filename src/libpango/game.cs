﻿using System;
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
        enum States { Prepared, Running, Paused, Finished };
        States state;

        // Money that player collected for killing monsters,
        // collecting bonuses, aligning diamonds, etc.
        int money;

        private Game() {
            int mapWidth = Config.Instance.getInt("Game.mapWidth");
            int mapHeight = Config.Instance.getInt("Game.mapHeight");

            state = States.Prepared;
            map = new Map(mapWidth, mapHeight);
            schedule = Schedule.Instance;
            money = 0;
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
        public void start() {
            if ((state == States.Prepared) || (state == States.Paused)) {
                state = States.Running;
                loop();
            }
        }
        public void end() {
            state = States.Finished;
        }
        public void pause() {
            if (state == States.Running) {
                state = States.Paused;
                // TODO: pause the loop (maybe wait()?)
            }
        }
        public void loop() {
            bool turnNotEmpty = false;

            // loop
            while (state != States.Finished){
                // call events for this time in the queue
                schedule.callCurrentEvents();

                // let all entities in the map perform their turn
                foreach (Entity ent in map) {
                    // if something was made turnNotEmpty is set true
                    turnNotEmpty |= ent.turn();
                }
                
                // Probably wait some time not to make the game so fast.
                // Think of how to make the turns last the same time.
                System.Threading.Thread.Sleep(10);

                schedule.increaseTime();
                if (!turnNotEmpty && schedule.empty()) {
                    // empty loop detected (and nothing left in the schedule)
                    // -> exit game
                    end();
                }
            }
        }
        public void receiveMoney(int amount) {
            money += amount;
        }
    }
}
