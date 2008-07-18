using System;
using System.Collections.Generic;
using System.Text;
using Pango;

namespace testing
{
    class testing
    {
        

        void test1() {
            /*
            Config.Instance["Game.mapWidth"] = "3";
            Config.Instance["Game.mapHeight"] = "3";
            Game game = Game.Instance;
            game.Map.add(new MonsterEntity(), new Coordinates(0, 0));
            game.Map.add(new FreePlace(), new Coordinates(0, 1));
            game.Map.add(new FreePlace(), new Coordinates(1, 0));
            game.Map.add(new MonsterEntity(), new Coordinates(1, 0));
            game.Map.add(new FreePlace(), new Coordinates(1, 1));
            game.Map.add(new PlayerEntity(), new Coordinates(1, 1));
            

            printMap(game.Map);
            //game.start();

            System.Console.ReadKey();
            */
        }
        public static void lookHook(object sender, EventArgs e) {
            System.Console.WriteLine(Game.Instance.Map.ToString());
            System.ConsoleKeyInfo cki = System.Console.ReadKey(true);
            if (cki.Key == System.ConsoleKey.Escape) {
                Game.Instance.end();
            }
        }

        static void Main(string[] args) {
            string[] maplines = { "XXXXX", "X*#@X", "XQ #X", "XH$LX", "XX", "XXXXX" };
            string maptext = string.Join("\n", maplines);

            Map map = MapPersistence.FromString(maptext);
            Game game = Game.Instance;
            game.Map = map;
            Entity ent = map.Places[2, 1].NonWalkable;
            if ((ent != null) && (ent is MonsterEntity)) {
                int time = Config.Instance.getInt("MonsterEntity.timeToWakeupDiamond");
                ((MonsterEntity)ent).stun();
            }
            game.loopStep += new EventHandler(lookHook);
            game.start();
            
        }
    }
}
