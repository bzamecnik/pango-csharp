using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public enum Direction { Up = 0, Right, Left, Down };
    public enum Rotation { Forward = 0, CW, Backwards, CCW }

    // Counted from [0,0]
    // x - vertical (goes from to to down)
    // y - horisontal (goes left to right)
    public struct Coordinates
    {
        public int x, y;
        public static Coordinates invalid = new Coordinates(-1, -1);
        public Coordinates(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public static Coordinates step(Coordinates c, Direction dir) {
            switch (dir) {
                case Direction.Up:
                    return new Coordinates(c.x - 1, c.y);
                case Direction.Right:
                    return new Coordinates(c.x, c.y + 1);
                case Direction.Down:
                    return new Coordinates(c.x + 1, c.y);
                case Direction.Left:
                    return new Coordinates(c.x, c.y - 1);
            }
            return c;
        }
        public Coordinates step(Direction dir) {
            return Coordinates.step(this, dir);
        }
        public bool areNeighbors(Coordinates coords1, Coordinates coords2) {
            return (((coords1.x == coords2.x) && (Math.Abs(coords1.y - coords2.y) == 1)) ||
                ((coords1.y == coords2.y) && (Math.Abs(coords1.x - coords2.x) == 1)));
        }
        public bool Equals(Coordinates other) {
            return ((x == other.x) && (y == other.y));
        }
    }

    public class Map
    {
        List<Entity>[,] map;
        private static Map instance = null; // a singleton
        // TODO: put these constants somewhere else!
        private static int defaultWidth = 20;
        private static int defaultHeight = 20;
        public int Height {
            get {
                if (map != null) {
                    return map.GetUpperBound(0);
                } else {
                    return 0;
                }
            }
        }
        public int Width {
            get {
                if (map != null) {
                    return map.GetUpperBound(1);
                } else {
                    return 0;
                }
            }
        }

        // TODO: singleton shouldn't expose its constructors
        // * but how to pass the arguments (map size) when creating the map?
        public Map() {
            createMap(defaultWidth, defaultHeight);
        }
        public Map(int width, int height) {
            createMap(width, height);
        }
        private void createMap(int width, int height) {
            // for sharing code in both constructors
            if (instance == null) {
                map = new List<Entity>[height, width];
            }
        }
        public static Map Instance {
            get {
                if (instance == null) {
                    instance = new Map();
                }
                return instance;
            }
        }
        // add entity to a given place
        public bool add(Entity ent, Coordinates coords) {
            if ((ent is WalkableEntity) || (isWalkable(coords) && !hasEntity(ent, coords))) {
                ent.Coords = coords;
                map[coords.x, coords.y].Add(ent);
                return true;
            }
            return false;
        }
        // remove entity from given place
        public bool remove(Entity ent, Coordinates coords) {
            if (hasEntity(ent, coords)) {
                return map[coords.x, coords.y].Remove(ent);
            }
            return false;
        }
        // remove entity from map
        public bool remove(Entity ent) {
            Coordinates coords = find(ent);
            if (!coords.Equals(Coordinates.invalid)) {
                return remove(ent, coords);
            }
            return false;
        }
        // place is walkable, if all entities there are walkable
        public bool isWalkable(Coordinates coords) {
            foreach (Entity ent in map[coords.x, coords.y]) {
                //if (!ent.isWalkable()) { return false; }
                if (!(ent is WalkableEntity)) { return false; }
            }
            return true;
        }
        // place is smitable (moving block can smite it),
        // if there is no other block (i.e. all entities are either
        // walkable or live)
        public bool isSmitable(Coordinates coords) {
            foreach (Entity ent in map[coords.x, coords.y]) {
                if (!((ent is WalkableEntity) || (ent is LiveEntity)))
                    { return false; }
            }
            return true;
        }
        public bool move(Entity ent, Coordinates from, Coordinates to) {
            // TODO: this is a bit ugly
            if (hasEntity(ent)) {
                remove(ent, from);
                add(ent, to);
                return true;
            }
            return false;
        }
        // search for entity in the whole map
        public Coordinates find(Entity ent) {
            for (int x = 0; x < Height; x++) {
                for (int y = 0; y < Width; y++) {
                    if (map[x, y].Contains(ent)) {
                        return new Coordinates(x, y);
                    }
                }
            }
            return Coordinates.invalid;
        }
        // search in the whole map
        public bool hasEntity(Entity ent) {
            return (!find(ent).Equals(Coordinates.invalid));
        }
        // search in list
        public bool hasEntity(Entity ent, Coordinates coords) {
            return map[coords.x, coords.y].Contains(ent);
        }
        public IEnumerator<Entity> GetEnumerator() {
            foreach (List<Entity> listent in map) {
                foreach (Entity ent in listent) {
                    yield return ent;
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

        // TODO: select random walkable place (for placing new entities, eg.
        // respawning or placing bonuses)
        public Coordinates getRandomWalkablePlace() {
            return Coordinates.invalid; // stub
        }
    }
}
