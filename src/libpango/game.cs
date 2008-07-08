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
        // * atEnd()
        // * game loop
        // * time limit (?)
        private static Game instance = null; // // a singleton
        Map map;
        private Game() {
            map = new Map();
        }
        public static Game getInstance() {
            if (instance == null) {
                instance = new Game();
            }
            return instance;
        }
    }
}
