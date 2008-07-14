using System;
using System.Collections.Generic;
using System.Text;
using Pango;

namespace testing
{
    class testing
    {
        static void Main(string[] args) {
            Config.Instance["Game.mapWidth"] = "2";
            Config.Instance["Game.mapHeight"] = "2";
            Game game = Game.Instance;
            game.Map.add(new MonsterEntity(), new Coordinates(0, 0));
            game.Map.add(new FreePlace(), new Coordinates(0, 1));
            game.Map.add(new FreePlace(), new Coordinates(1, 0));
            game.Map.add(new MonsterEntity(), new Coordinates(1, 0));
            game.Map.add(new FreePlace(), new Coordinates(1, 1));
            game.Map.add(new PlayerEntity(), new Coordinates(1, 1));
            game.start();
        }
    }
}
