using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Game
    {
        // TODO
        // can contain:
        // * quotas for monsters (initial), bonuses, etc.
        // * time limit (?)
        private static Game instance = null; // a singleton
        Map map;
        Schedule schedule;
        enum States { Prepared, Running, Paused, Finished };
        States state;

        private Game() {
            state = States.Prepared;
            map = new Map();
            schedule = Schedule.Instance;
        }
        public static Game Instance {
            get {
                if (instance == null) {
                    instance = new Game();
                }
                return instance;
            }
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
            // loop
            while (state != States.Finished){ // TODO: while game not ended yet
                // call events for this time in the queue
                schedule.callCurrentEvents();

                // let all entities in the map perform their turn
                foreach (Entity ent in map) {
                    ent.turn();
                }
                
                // Probably wait some time not to make the game so fast.
                // Think of how to make the turns last the same time.

                schedule.increaseTime();
            }
        }
    }
}
