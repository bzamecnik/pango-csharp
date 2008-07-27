using System;
using System.Collections.Generic;
using System.Text;
using CodEx;

// $Id$

namespace Pango
{
    // TODO: organize directions better

    // ----- enum Direction ----------------------------------------
    public enum Direction { Up = 0, Right, Down, Left };

    // ----- enum Rotation ----------------------------------------
    public enum Rotation { Forward = 0, CW, Backwards, CCW }

    // ----- class DirectionUtils ----------------------------------------
    public static class DirectionUtils {

        // ----- properties --------------------

        public static int Count {
            get { return 4; }
        }

        // ----- methods --------------------

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
        // TODO: iterator thrhough Direction and Rotation items
    }

    // ----- class Coordinates ----------------------------------------
    public class Coordinates
    {
        // Counted from [0,0]
        // x - vertical (goes from up to down) -> height
        // y - horizontal (goes left to right) -> width

        // ----- fields --------------------

        public int x, y;
        public bool isInvalid;
        public static readonly Coordinates invalid = new Coordinates();

        // ----- constructors --------------------

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
        
        // ----- methods --------------------
        
        public static Coordinates operator +(Coordinates coords1, Coordinates coords2) {
            if (coords1.Equals(Coordinates.invalid) || coords2.Equals(Coordinates.invalid)) {
                return Coordinates.invalid;
            } else {
                return new Coordinates(coords1.x + coords2.x, coords1.y + coords2.y);
            }
        }

        public static Coordinates operator -(Coordinates coords1, Coordinates coords2) {
            if (coords1.Equals(Coordinates.invalid) || coords2.Equals(Coordinates.invalid)) {
                return Coordinates.invalid;
            } else {
                return new Coordinates(coords1.x - coords2.x, coords1.y - coords2.y);
            }
        }

        // Difference direction from coords1 to coords2 as:
        // * Direction, if they are neighbors
        // * null, else
        public static Nullable<Direction> diffDirection(Coordinates coords1, Coordinates coords2) {
            Coordinates diff = coords2 - coords1;
            if (diff.x == 0) {
                if (diff.y == 1) {
                    return Direction.Right;
                } else if (diff.y == -1) {
                    return Direction.Left;
                }
            } else if (diff.y == 0) {
                if (diff.x == -1) {
                    return Direction.Up;
                } else if (diff.x == 1) {
                    return Direction.Down;
                }
            }
            return null;
        }

        public bool Equals(Coordinates other) {
            return ((x == other.x) && (y == other.y) && (isInvalid == other.isInvalid));
        }

        public static bool areNeighbors(Coordinates coords1, Coordinates coords2) {
            //return (((coords1.x == coords2.x) && (Math.Abs(coords1.y - coords2.y) == 1)) ||
            //    ((coords1.y == coords2.y) && (Math.Abs(coords1.x - coords2.x) == 1)));
            Coordinates diff = coords1 - coords2;
            return ((Math.Abs(diff.x) + Math.Abs(diff.y)) == 1);
        }

        public bool isNeighbor(Coordinates coords) {
            return areNeighbors(this, coords);
        }

        public static Coordinates step(Coordinates c, Direction dir) {
            return c + DirectionUtils.step(dir);
        }

        public Coordinates step(Direction dir) {
            return Coordinates.step(this, dir);
        }
    }

    // ----- class Place ----------------------------------------
    public class Place {

        // ----- fields --------------------

        Entity walkable;
        Entity nonWalkable;

        // ----- constructors --------------------

        public Place() {
            walkable = null;
            nonWalkable = null;
        }
        
        // ----- properties --------------------

        public Entity Walkable {
            get { return walkable; }
            set { walkable = value; }
        }
        public Entity NonWalkable {
            get { return nonWalkable; }
            set { nonWalkable = value; }
        }

        // ----- methods --------------------

        // true if entity was really added
        public bool add(Entity ent) {
            if (ent is WalkableEntity) {
                if (walkable == null) {
                    walkable = ent;
                    return true;
                }
            } else {
                if (nonWalkable == null) {
                    nonWalkable = ent;
                    return true;
                }
            }
            return false;
        }

        // true if entity was really removed
        public bool remove(Entity ent) {
            if (!contains(ent)) { return false; }
            if (ent is WalkableEntity) {
                if (walkable != null) {
                    walkable = null;
                    return true;
                }
            } else {
                if (nonWalkable != null) {
                    nonWalkable = null;
                    return true;
                }
            }
            return false;
        }

        public bool isWalkable() {
            return (nonWalkable == null);
        }

        public bool contains(Entity ent) {
            return (((walkable != null) && walkable.Equals(ent))
                    || ((nonWalkable != null) && nonWalkable.Equals(ent)));
        }

        public IEnumerator<Entity> GetEnumerator() {
            if (walkable != null) {
                yield return walkable;
            }
            if (nonWalkable != null) {
                yield return nonWalkable;
            }
        }
    }

    // ----- class Map ----------------------------------------
    public class Map
    {
        // ----- fields --------------------

        Place[,] map;
        // count number of walkable places (for getRandomWalkablePlace())]
        int walkablePlaces;
        // count various entity types - MonsterEntity, BonusEntity
        List<MonsterEntity> monsters;
        List<BonusEntity> bonuses;

        // ----- constructors --------------------

        public Map(int width, int height) {
            map = new Place[height, width];
            // initialize places
            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    map[x, y] = new Place();
                }
            }
            walkablePlaces = height * width;
            monsters = new List<MonsterEntity>();
            bonuses = new List<BonusEntity>();
        }

        // load an existing map
        public Map(Entity[,] array) {
            monsters = new List<MonsterEntity>();
            bonuses = new List<BonusEntity>();
            map = new Place[array.GetUpperBound(0) + 1, array.GetUpperBound(1) + 1];
            walkablePlaces = Height * Width;
            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    map[x, y] = new Place();
                    Entity ent = array[x, y];
                    if (ent != null) {
                        add(ent, new Coordinates(x,y));
                    }
                }
            }
        }

        // ----- properties --------------------

        public Place[,] Places {
            get { return map; }
        }

        public int Height {
            get {
                if (map != null) {
                    // map.GetUpperBound(int) -> [0,n-1]
                    return map.GetUpperBound(0) + 1;
                } else { return 0; }
            }
        }

        public int Width {
            get {
                if (map != null) { return map.GetUpperBound(1) + 1; } else { return 0; }
            }
        }

        public List<MonsterEntity> Monsters { get { return monsters; } }

        public List<BonusEntity> Bonuses { get { return bonuses; } }
        
        public int ActiveMonsters {
            get {
                int count = 0;
                if (monsters != null) {
                    foreach (MonsterEntity monster in monsters) {
                        if (monster.Active) { count++; }
                    }
                }
                return count;
            }
        }

        // ----- methods --------------------

        // TODO: randomly generate map
        // * at first, we'll use hand made maps
        // * all maps (including randomly generated ones) must obey
        //   some rules:
        //   * have exactly 1 player (no multiplayer yet)
        //   * have some monsters
        //   * have walls around

        // Add an entity to a given place
        public bool add(Entity ent, Coordinates coords) {
            if ((ent == null) || !areValidCoordinates(coords)) { return false; }
            ent.Coords = coords;
            Place place = getPlace(coords);
            bool added = add(ent, place);
            if (added) {
                if ((ent is MonsterEntity) && (monsters != null)
                    && (!monsters.Contains((MonsterEntity)ent))) {
                    monsters.Add((MonsterEntity)ent);
                }
                if ((ent is BonusEntity) && (bonuses != null)
                    && (!bonuses.Contains((BonusEntity)ent))) {
                    bonuses.Add((BonusEntity)ent);
                }
            }
            return added;
        }

        private bool add(Entity ent, Place place) {
            if ((ent == null) || (place == null)) { return false; }
            bool added = place.add(ent);
            if (added && !(ent is WalkableEntity)) {
                // this place was made non-walkable
                walkablePlaces--;
            }
            return added;
        }

        // Remove entity from given coords
        public bool remove(Entity ent, Coordinates coords) {
            if ((ent == null) || !hasEntity(ent, coords)) { return false; }
            
            bool removeReturnValue = false;
            Place place = getPlace(coords);
            
            removeReturnValue = place.remove(ent);
            if ((ent is MonsterEntity) && (monsters != null)
                && (monsters.Contains((MonsterEntity)ent))) {
                monsters.Remove((MonsterEntity)ent);
            }
            if ((ent is BonusEntity) && (bonuses != null)
                && (bonuses.Contains((BonusEntity)ent))) {
                bonuses.Remove((BonusEntity)ent);
            }
            return removeReturnValue;
        }

        // remove entity from given place
        private bool remove(Entity ent, Place place) {
            if ((ent == null) || (place == null)) { return false; }
            bool entWasNonWalkable = !(ent is WalkableEntity);
            bool removed = place.remove(ent);
            if (removed && entWasNonWalkable && place.isWalkable()) {
                // this place was made walkable again
                walkablePlaces++;
            }
            return removed;
        }

        // Remove entity from map
        public bool remove(Entity ent) {
            if (ent == null) { return false; }
            Coordinates coords = find(ent);
            if (!coords.Equals(Coordinates.invalid)) {
                return remove(ent, coords);
            }
            return false;
        }

        // Place is walkable if all entities there are walkable
        public bool isWalkable(Coordinates coords) {
            if (!areValidCoordinates(coords)) { return false; } // better: expception
            return map[coords.x, coords.y].isWalkable();
        }

        // Place is smitable (moving block can smite it)
        // if there is no other block (i.e. all entities are either
        // walkable or live)
        public bool isSmitable(Coordinates coords) {
            Place place = map[coords.x, coords.y];
            if (place.isWalkable()) { return true; }
            if (((place.Walkable != null) && (place.Walkable is LiveEntity))
                || ((place.NonWalkable != null) && (place.NonWalkable is LiveEntity))) {
                return true;
            }
            return false;
        }

        // Move an entity from one place to another
        public bool move(Entity ent, Coordinates from, Coordinates to) {
            // TODO: isn't it a bit ugly?
            if (hasEntity(ent)) {
                remove(ent, getPlace(from));
                add(ent, getPlace(to));
                return true;
            }
            return false;
        }

        // Search for entity in the whole map
        public Coordinates find(Entity ent) {
            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    Place place = map[x, y];
                    if ((place != null) && place.contains(ent)) {
                        return new Coordinates(x, y);
                    }
                }
            }
            return Coordinates.invalid;
        }

        // Search an entity in the whole map
        public bool hasEntity(Entity ent) {
            return (!find(ent).Equals(Coordinates.invalid));
        }

        // Search an entity at a place given by its coordinates
        public bool hasEntity(Entity ent, Coordinates coords) {
            Place place = getPlace(coords);
            return ((place != null) && place.contains(ent));
        }

        public IEnumerator<Entity> GetEnumerator() {
            foreach (Place place in map) {
                if (place != null) {
                    foreach (Entity ent in place) {
                        yield return ent;
                    }
                }
            }
        }

        public Place getPlace(Coordinates coords) {
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
        //        if (areValidCoordinates(step)) {
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
                if (areValidCoordinates(step)) {
                    foreach (Entity ent in map[step.x, step.y]) {
                        neighbors.Add(ent);
                    }
                }
            }
            return neighbors;
        }

        public bool areValidCoordinates(Coordinates coords) {
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
            //   * select a random place
            //   * check if it is walkable (in a cycle)
            return new Coordinates(randomX, randomY);
        }

        public override string ToString() {
            return MapPersistence.ToString(this);
        }
    }

    // ----- class MapPersistence ----------------------------------------

    public static class MapPersistence {

        // ----- fields --------------------

        static Dictionary<string, char> entityCharTable = initEntityCharTable();
        static Dictionary<char, string> charEntityTable = initCharEntityTable();

        // ----- methods --------------------

        static Dictionary<string, char> initEntityCharTable() {
            Dictionary<string, char> table = new Dictionary<string, char>();
            table["null"] = ' ';
            table["PlayerEntity"] = 'X';
            table["MonsterEntity"] = 'Q';
            table["StoneBlock"] = '#';
            table["IceBlock"] = '*';
            table["DiamondBlock"] = 'D';
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
            string type;
            if (ent == null) {
                type = "null";
            } else {
                type = ent.GetType().ToString();
                type = type.Substring(type.LastIndexOf('.') + 1);
            }
            if (!entityCharTable.ContainsKey(type)) {
                type = string.Empty;
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

        public static Map FromString(string text) {
            // eg:
            // XXXXXXX
            // X@  LQX
            // X   Q X
            // XXXXXXX
            string[] lines = text.Split('\n');
            int mapWidth = 0;
            int mapHeight = 0;
            for (int x = 0; x < lines.Length; x++) {
                // find longest line -> width
                mapWidth = Math.Max(mapWidth, lines[x].Length);
                if (lines[x].Length > 0) {
                    mapHeight++; // don't count empty lines
                } else {
                    break;
                }
            }
            Entity[,] map = new Entity[mapHeight, mapWidth];
            for (int x = 0; x < mapHeight; x++) {
                string line = lines[x];
                if (line.Length <= 0) {
                    // stop on first empty line
                    break;
                }
                for (int y = 0; y < mapWidth; y++) {
                    char c;
                    if(y < line.Length) {
                        c = line[y];
                    } else {
                        // fill short lines - maybe redundant
                        c = ' ';
                    }
                    Entity ent = createEntity(charToEntity(c));
                    if (ent != null) {
                        ent.Coords = new Coordinates(x, y);
                    }
                    map[x, y] = ent;
                }
            }
            return new Map(map);
        }

        public static string ToString(Map map) {
            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < map.Height; x++) {
                for (int y = 0; y < map.Width; y++) {
                    Place place = map.Places[x, y];
                    if (place != null) {
                        Entity ent;
                        if (place.isWalkable()) {
                            ent = place.Walkable;
                        } else {
                            ent = place.NonWalkable;
                        }
                        sb.Append(entityToChar(ent));
                    } else {
                        sb.Append(' ');
                        
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string readMapFromFile(string fileName) {
            Reader r = new Reader(fileName);
            string map = readMapFromFile(r);
            r.Close();
            return map;
        }

        static string readMapFromFile(Reader reader) {
            StringBuilder sb = new StringBuilder();
            while (!reader.EOF()) {
                string line = reader.Line().Trim(new char[] { '\r', '\t' });
                if (line.Length > 0) {
                    sb.Append(line + '\n');
                } else {
                    break;
                }
            }
            return sb.ToString();
        }

        // TODO: Read multiple maps from one file
        // and put them into Config as Game.map.1, Game.map.2, ...
        public static void loadMapsFromFile(string fileName) {
            Reader r = new Reader(fileName);
            int count = 0;
            while (!r.EOF()) {
                string map = readMapFromFile(r);
                if(map.Length > 0){
                    count++;
                    Config.Instance[string.Format("Game.map.{0}", count)] = map;
                }
            }
            Config.Instance.addInt("Game.mapCount", count);
            r.Close();
        }
    }
}
