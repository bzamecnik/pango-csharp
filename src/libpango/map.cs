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
        private int defaultWidth = 10;
        private int defaultHeight = 10;

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
            if (instance == null) { // OK?
                map = new List<Entity>[width, height];
            }
        }
        public static Map getInstance() {
            return instance;
        }
        // add entity to a given place
        public bool add(Entity ent, Coordinates coords) {
            if (ent.isWalkable() || (isWalkable(coords) && !hasEntity(ent, coords))) {
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
        // field is walkable, if all entities there are walkable
        public bool isWalkable(Coordinates coords) {
            foreach (Entity ent in map[coords.x, coords.y]) {
                if (!ent.isWalkable()) { return false; }
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
            for (int x = 0; x < map.GetUpperBound(0); x++) {
                for (int y = 0; y < map.GetUpperBound(1); y++) {
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
        public List<Entity>.Enumerator getEntitiesByCoordsEnumerator(Coordinates coords) {
            return map[coords.x, coords.y].GetEnumerator();
        }

        // TODO: select random walkable field (for placing new entities, eg.
        // respawning or placing bonuses)
        public Coordinates getRandomWalkableField() {
            return Coordinates.invalid; // stub
        }
    }
}
