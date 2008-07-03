using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Schedule {
        int time;
        public void add(EventHandler e, int timeoffset) { }
        public EventHandler get() { }
    }
    public class Game {}
    public class Map {
        List<Entity>[,] map;

        Map(int width, int height) {
            map = new List<Entity>[width,height];
        }

        bool add(Entity ent, Coordinates coords) {
            if (ent.isWalkable() || isWalkable(coords)) {
                map[coords.x, coords.y].Add(ent);
                return true;
            }
            return false;
        }
        bool remove(Entity ent, Coordinates coords) {
            if(hasEntity(ent,coords)) {
                return map[coords.x, coords.y].Remove(ent);
            }
            return false;
        }
        bool remove(Entity ent) {
            Coordinates coords = find(ent);
            if (!coords.Equals(Coordinates.invalid)) {
                return remove(ent, coords);
            }
            return false;
        }
        // field is walkable, if all entities there are walkable
        bool isWalkable(Coordinates coords) {
            foreach (Entity ent in map[coords.x, coords.y]) {
                if (!ent.isWalkable()) { return false; }
            }
            return true;
        }
        void move(Entity ent, Coordinates from, Coordinates to) { }
        // search for entity in the whole map
        Coordinates find(Entity ent) {
            for (int y = 0; y < map.GetUpperBound(0); y++) {
                for(int x = 0; x < map.GetUpperBound(1); x++) {            
                    if (map[y, x].Contains(ent)) {
                        return new Coordinates(y, x);
                    }
                }
            }
            return Coordinates.invalid;
        }
        // search in the whole map
        bool hasEntity(Entity ent) {
            return (!find(ent).Equals(Coordinates.invalid));
        }
        // search in list
        bool hasEntity(Entity ent, Coordinates coords) {
            return map[coords.x, coords.y].Contains(ent);
        }
    }

    // Counted from [0,0]
    public struct Coordinates {
        public int x, y;
        public static Coordinates invalid = new Coordinates(-1,-1);
        public Coordinates(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
    public enum Direction { Up, Right, Left, Down }
    public enum Rotation { Forward, CW, Backwards, CCW }

    public abstract class Entity {
        // Coordinates will be both in map and in entity.
        // We'll need to synchronize this.
        Coordinates coords;

        public virtual bool isWalkable() { return false; }
        public abstract void turn();
        public abstract void acceptAttack(int hitcount);
    }

    // Better would be to use iterfaces in place of abstract classes,
    // but how to incorporate new fields into interface???
    public abstract class MovableEntity : Entity
    {
        Direction direction;

        public bool go(Direction dir) { } // true, if step was made
        public void rotate(Rotation rot) { }
    }

    public abstract class LiveEntity : MovableEntity {
        int health;
        int maxHealth;
        int lives;
        int defaultLives;

        // change < 0 ... hurt
        // change > 0 ... stimpack
        // Take care of correct lives count when health goes
        // throuh 0 or maxHealth.
        // returns true, if still alive
        public virtual bool changeHealth(int change) {
            health += change;
            if (health > maxHealth) {
                lives += health / maxHealth;
                health %= maxHealth;
            } else if (health < 0) {
                if (lives > 0) {
                    lives--;
                    health += maxHealth;
                } else {
                    die();
                    return false;
                }
            }
            return true;
        }

        public abstract void die();
        public abstract void attack(Entity e, int hitcount);
    }

    public abstract class WalkableEntity : Entity {
        // other entities walk through this one
        public override bool isWalkable() { return true; }
    }

    public class PlayerEntity : LiveEntity {
        // money for killing monsters, gathering bonuses, aligning diamonds, ...
        int money;
        public override void turn() { }
        public override void acceptAttack(int hitcount) { }
    }
    // maybe think of multiple types of mosters
    public class MonsterEntity : LiveEntity {
        public override void turn() { }
        public override void acceptAttack(int hitcount) { }
    }

    public class Stone : Entity {
        public override void turn() { }
        public override void acceptAttack(int hitcount) { }
    }
    public class IceBlock : MovableEntity {
        public override void turn() { }
        public override void acceptAttack(int hitcount) { }
    }

    public class FreePlace : WalkableEntity {
        public override void turn() { }
        public override void acceptAttack(int hitcount) { }
    }
    // multiple bonuses
    public class Bonus : WalkableEntity {
        public override void turn() { }
        public override void acceptAttack(int hitcount) { }
    }
}
