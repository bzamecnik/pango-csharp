using System;
using System.Collections.Generic;
using System.Text;

namespace Pango
{
    // TODO: organize directions better
    public enum Direction { Up = 0, Right, Left, Down };
    public enum Rotation { Forward = 0, CW, Backwards, CCW }

    public class DirectionUtils {
        public static Direction rotate(Direction direction, Rotation rot) {
            return (Direction)(((int)direction + (int)rot) % 4);
        }
        public static Coordinates step(Direction dir) {
            switch (dir) {
                case Direction.Up:
                    return new Coordinates(-1, 0);
                case Direction.Right:
                    return new Coordinates(0, 1);
                case Direction.Down:
                    return new Coordinates(1, 0);
                case Direction.Left:
                    return new Coordinates(0, -1);
            }
            return Coordinates.invalid;
        }
        // iterator thrhough Direction and Rotation items
    }

    // Counted from [0,0]
    // x - vertical (goes from up to down) -> height
    // y - horizontal (goes left to right) -> width
    public class Coordinates
    {
        public int x, y;
        public bool isInvalid;
        public static Coordinates invalid = new Coordinates();
        public Coordinates() {
            this.x = 0;
            this.y = 0;
            isInvalid = true;
        }
        public Coordinates(int x, int y) {
            this.x = x;
            this.y = y;
            isInvalid = false;
        }
        public static Coordinates step(Coordinates c, Direction dir) {
            return c + DirectionUtils.step(dir);
        }
        public Coordinates step(Direction dir) {
            return Coordinates.step(this, dir);
        }
        public static Coordinates operator +(Coordinates coords1, Coordinates coords2) {
            if (coords1.Equals(Coordinates.invalid) || coords2.Equals(Coordinates.invalid)) {
                return Coordinates.invalid;
            } else {
                return new Coordinates(coords1.x + coords2.x, coords1.y + coords2.y);
            }
        }
        public bool areNeighbors(Coordinates coords1, Coordinates coords2) {
            return (((coords1.x == coords2.x) && (Math.Abs(coords1.y - coords2.y) == 1)) ||
                ((coords1.y == coords2.y) && (Math.Abs(coords1.x - coords2.x) == 1)));
        }
        public bool Equals(Coordinates other) {
            return ((x == other.x) && (y == other.y) && (isInvalid == other.isInvalid));
        }
    }

    public class Map
    {
        List<Entity>[,] map;
        // count number of walkable places (for getRandomWalkablePlace())
        int walkablePlaces;

        public List<Entity>[,] Places {
            get { return map; }
        }

        public int Height {
            get {
                if (map != null) {
                    // map.GetUpperBound(int) -> [0,n-1]
                    return map.GetUpperBound(0) + 1;
                }
                else { return 0; }
            }
        }
        public int Width {
            get {
                if (map != null) { return map.GetUpperBound(1) + 1; }
                else { return 0; }
            }
        }

        public Map(int width, int height) {
            map = new List<Entity>[height, width];
            // initialize places
            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    map[x, y] = new List<Entity>();
                }
            }
            walkablePlaces = height * width;
        }
        // load an existing map
        public Map(List<Entity>[,] map) {
            this.map = map; // ok?
            // initialize possibly non-initialized places
            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    List<Entity> l = map[x, y];
                    if (l == null) {
                        l = new List<Entity>();
                    }
                }
            }
        }

        // TODO: randomly generate map
        // * at first, we'll use hand made maps
        // * all maps (including randomly generated ones) must obey
        //   some rules:
        //   * have exactly 1 player (no multiplayer yet)
        //   * have some monsters
        //   * have walls around

        // Add an entity to a given place
        public bool add(Entity ent, Coordinates coords) {
            if (coords.isInvalid) { return false; }
            if ((ent is WalkableEntity) || (isWalkable(coords) && !hasEntity(ent, coords))) {
                if (!(ent is WalkableEntity) && isWalkable(coords)) {
                    // this place is beeing made non-walkable
                    walkablePlaces--;
                }
                ent.Coords = coords;
                List<Entity> l = map[coords.x, coords.y];
                if (l == null) {
                    l = new List<Entity>();
                }
                l.Add(ent);
                return true;
            }
            return false;
        }
        // Remove entity from given place
        public bool remove(Entity ent, Coordinates coords) {
            if (hasEntity(ent, coords)) {
                List<Entity> l = map[coords.x, coords.y];
                if (l != null) {
                    bool entWasNonWalkable = !(ent is WalkableEntity);
                    bool removeReturnValue = l.Remove(ent);
                    if (entWasNonWalkable && isWalkable(coords)) {
                        // this place was made walkable again
                        walkablePlaces++;
                    }
                    return removeReturnValue;
                }
            }
            return false;
        }
        // Remove entity from map
        public bool remove(Entity ent) {
            Coordinates coords = find(ent);
            if (!coords.Equals(Coordinates.invalid)) {
                return remove(ent, coords);
            }
            return false;
        }
        // Place is walkable if all entities there are walkable
        public bool isWalkable(Coordinates coords) {
            if (coords.isInvalid) { return false; } // better: expception
            foreach (Entity ent in map[coords.x, coords.y]) {
                //if (!ent.isWalkable()) { return false; }
                if (!(ent is WalkableEntity)) { return false; }
            }
            return true;
        }
        // Place is smitable (moving block can smite it)
        // if there is no other block (i.e. all entities are either
        // walkable or live)
        public bool isSmitable(Coordinates coords) {
            foreach (Entity ent in map[coords.x, coords.y]) {
                if (!((ent is WalkableEntity) || (ent is LiveEntity)))
                    { return false; }
            }
            return true;
        }
        // Move an entity from one place to another
        public bool move(Entity ent, Coordinates from, Coordinates to) {
            // TODO: this is a bit ugly, isn't it
            if (hasEntity(ent)) {
                remove(ent, from);
                add(ent, to);
                return true;
            }
            return false;
        }
        // Search for entity in the whole map
        public Coordinates find(Entity ent) {
            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    List<Entity> l = map[x, y];
                    if ((l != null) && l.Contains(ent)) {
                        return new Coordinates(x, y);
                    }
                }
            }
            return Coordinates.invalid;
        }
        // Search in the whole map
        public bool hasEntity(Entity ent) {
            return (!find(ent).Equals(Coordinates.invalid));
        }
        // Search on a place (in a list)
        public bool hasEntity(Entity ent, Coordinates coords) {
            List<Entity> l = map[coords.x, coords.y];
            return ((l != null) && l.Contains(ent));
        }
        public IEnumerator<Entity> GetEnumerator() {
            foreach (List<Entity> l in map) {
                if (l != null) {
                    foreach (Entity ent in l) {
                        yield return ent;
                    }
                }
            }
        }
        public List<Entity> getEntitiesByCoords(Coordinates coords) {
            // TODO: is this efficient? doesn't is return a copy of the list?
            return map[coords.x, coords.y];
        }

        //public Dictionary<Coordinates, List<Entity>> getNeighbors(Coordinates coords) {
        //    // TODO: is this efficient? doesn't is return copies?
        //    Dictionary<Coordinates, List<Entity>> neighbors = new Dictionary<Coordinates, List<Entity>>();
        //    Direction[] dirs = new Direction[] {
        //        Direction.Up,
        //        Direction.Right,
        //        Direction.Down,
        //        Direction.Left
        //    };
        //    foreach (Direction dir in dirs) {
        //        Coordinates step = coords.step(dir);
        //        if (validCoordinates(step)) {
        //            neighbors.Add(step, map[step.x, step.y]);
        //        }
        //    }
        //    return neighbors;
        //}
        public List<Entity> getNeighbors(Coordinates coords) {
            // TODO: is this efficient? doesn't is return copies?
            List<Entity> neighbors = new List<Entity>();
            // TODO: make an iterator for directions
            Direction[] dirs = new Direction[] {
                Direction.Up,
                Direction.Right,
                Direction.Down,
                Direction.Left
            };
            foreach (Direction dir in dirs) {
                Coordinates step = coords.step(dir);
                if (validCoordinates(step)) {
                    foreach (Entity ent in map[step.x, step.y]) {
                        neighbors.Add(ent);
                    }
                }
            }
            return neighbors;
        }
        public bool validCoordinates(Coordinates coords) {
            return ((!coords.Equals(Coordinates.invalid)) &&
                    (coords.x >= 0) && (coords.x < Height) &&
                    (coords.y >= 0) && (coords.y < Width));
        }

        // Select random walkable place (for placing new entities, eg.
        // respawning or placing bonuses)
        public Coordinates getRandomWalkablePlace() {
            if (walkablePlaces <= 0) {
                return Coordinates.invalid;
            }
            Random rand = new Random();
            Map map = Game.Instance.Map;
            int randomX, randomY;
            do {
                randomX = rand.Next(map.Height);
                randomY = rand.Next(map.Width);
            } while(!map.isWalkable(new Coordinates(randomX, randomY)));
            // * Note: there IS a walkable place, so the cycle will eventually finish.
            // * Think of a better algorithm.
            // * THINK: Maybe split this function into two:
            //   * select random place
            //   * check if it is walkable (in a cycle)
            return new Coordinates(randomX, randomY);
        }
    }

    public class MapPersistence {
        static Dictionary<string, char> entityCharTable = initEntityCharTable();
        static Dictionary<char, string> charEntityTable = initCharEntityTable();
        static Dictionary<string, char> initEntityCharTable() {
            Dictionary<string, char> table = new Dictionary<string, char>();
            table["FreePlace"] = ' '; // TODO: remove
            table["PlayerEntity"] = '&';
            table["MonsterEntity"] = 'Q';
            table["StoneBlock"] = 'X';
            table["IceBlock"] = '#';
            table["DiamondBlock"] = '*';
            table["HealthBonus"] = 'H';
            table["MoneyBonus"] = '$';
            table["LiveBonus"] = 'L';
            return table;
        }
        static Dictionary<char, string> initCharEntityTable() {
            Dictionary<char, string> table = new Dictionary<char, string>();
            foreach (KeyValuePair<string, char> pair in entityCharTable) {
                table[pair.Value] = pair.Key;
            }
            return table;
        }
        static char entityToChar(Entity ent) {
            string type = ent.GetType().ToString();
            type = type.Substring(type.LastIndexOf('.') + 1);
            if (!entityCharTable.ContainsKey(type)) {
                type = "FreePlace";
            }
            return entityCharTable[type];
        }
        static string charToEntity(char c) {
            if (charEntityTable.ContainsKey(c)) {
                return charEntityTable[c];
            } else {
                return string.Empty;
            }
        }
        static Entity createEntity(string entityName) {
            // create only known entity types
            switch(entityName){
                case "PlayerEntity": return new PlayerEntity();
                case "MonsterEntity": return new MonsterEntity();
                case "StoneBlock": return new StoneBlock();
                case "IceBlock": return new IceBlock();
                case "DiamondBlock": return new DiamondBlock();
                case "HealthBonus": return new HealthBonus();
                case "MoneyBonus": return new MoneyBonus();
                case "LiveBonus": return new LiveBonus();
                default: return null;
            }
        }
        public static Map createMapFromText(string text) {
            // eg:
            // XXXXXXX
            // X@  LQX
            // X   Q X
            // XXXXXXX
            string[] lines = text.Split(null);
            int mapWidth = 0;
            foreach (string line in lines) {
                // find longest line -> width
                mapWidth = Math.Max(mapWidth, line.Length);
            }
            int mapHeight = lines.Length;
            List<Entity>[,] map = new List<Entity>[mapHeight, mapWidth];
            for (int x = 0; x < mapHeight; x++) {
                string line = lines[x];
                for (int y = 0; y < mapWidth; y++) {
                    char c;
                    if(y <= line.Length) {
                        c = line[y];
                    } else {
                        c = ' ';
                    }
                    Entity ent = createEntity(charToEntity(c));
                    map[x, y].Add(ent);
                }
            }

            return new Map(map);
        }
    }
}
