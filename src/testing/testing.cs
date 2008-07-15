using System;
using System.Collections.Generic;
using System.Text;
using Pango;

namespace testing
{
    class testing
    {
        static char entityToChar(Entity ent) {
            
            Dictionary<string, char> entityChars = new Dictionary<string, char>();
            entityChars["FreePlace"] = ' ';
            entityChars["PlayerEntity"] = '@';
            entityChars["MonsterEntity"] = 'Q';
            entityChars["StoneBlock"] = 'X';
            entityChars["IceBlock"] = '#';
            entityChars["DiamondBlock"] = '*';
            entityChars["HealthBonus"] = 'H';
            entityChars["MoneyBonus"] = '$';
            entityChars["LiveBonus"] = 'L';

            string type = ent.GetType().ToString();
            type = type.Substring(type.LastIndexOf('.')+1);
            if (!entityChars.ContainsKey(type)) {
                type = "FreePlace";
            }
            return entityChars[type];
        }
        static void printMap(Map map) {
            
            for (int x = 0; x < map.Height; x++) {
                for (int y = 0; y < map.Width; y++) {
                    Place place = map.Places[x, y];
                    if (place == null) {
                        System.Console.Write(" ");
                    } else {
                        Entity ent;
                        if (place.isWalkable()) {
                            ent = place.Walkable;
                        } else {
                            ent = place.NonWalkable;
                        }
                        System.Console.Write(entityToChar(ent));
                    }
                }
                System.Console.WriteLine();
            }
        }

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
        static void Main(string[] args) {
            string[] maplines = { "XXXXX", "X*#@X", "XQ #X", "XH$LX", "     ", "XXXXX" };
            string maptext = string.Join("\n", maplines);

            Map map = MapPersistence.createMapFromText(maptext);
        }
    }
}
