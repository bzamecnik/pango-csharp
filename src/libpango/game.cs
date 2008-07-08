using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Game
    {
        // TODO
        // can contain:
        // * quotas for monsters
        // * time limit (?)
        private static Game instance = null; // a singleton
        Map map;
        Schedule schedule;
        private Game() {
            map = new Map();
            schedule = Schedule.getInstance();
        }
        public static Game getInstance() {
            if (instance == null) {
                instance = new Game();
            }
            return instance;
        }
        public void start() { }
        public void end() { }
        public void loop() {
            // loop
            while (true){ // TODO: while game not ended yet
                // call events for this time in the queue
                schedule.callCurrentEvents();

                // let all entities in the map perform their turn
                foreach (Entity ent in map) {
                    ent.turn();
                }
                
                schedule.increaseTime();
            }
        }
    }
}
