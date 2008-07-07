using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Schedule {
        // * maybe make it a singleton
        // TODO
        int time;
        // there should be a priority queue inside
        public void add(EventHandler e, int timeoffset) { }
        //public EventHandler pop() { } 
    }
    public class Game {
        // TODO
        // * maybe make it a singleton
        // can contain:
        // * quotas for monsters
        // * atEnd()
        // * game loop
        // * time limit (?)
        private static Game instance = null;
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
    public class Map {
        List<Entity>[,] map;
        private static Map instance = null; // a singleton
        // TODO: put these constants somewhere else!
        private int defaultWidth = 10;
        private int defaultHeight = 10;

        public Map() {
            // better: call Map(int width, int height)
            if (instance == null) { // OK?
                map = new List<Entity>[defaultWidth, defaultHeight];
            }
        }
        public Map(int width, int height)
        {
            if (instance == null) { // OK?
                map = new List<Entity>[width, height];
            }
        }

        public static Map getInstance() {
            return instance;
        }
        // add entity to a given place
        public bool add(Entity ent, Coordinates coords)
        {
            if (ent.isWalkable() || (isWalkable(coords) && !hasEntity(ent,coords))) {
                map[coords.x, coords.y].Add(ent);
                return true;
            }
            return false;
        }
        // remove entity from given place
        public bool remove(Entity ent, Coordinates coords)
        {
            if(hasEntity(ent,coords)) {
                return map[coords.x, coords.y].Remove(ent);
            }
            return false;
        }
        // remove entity from map
        public bool remove(Entity ent)
        {
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
        public Coordinates find(Entity ent)
        {
            for(int x = 0; x < map.GetUpperBound(0); x++) {            
                for (int y = 0; y < map.GetUpperBound(1); y++) {
                    if (map[x, y].Contains(ent)) {
                        return new Coordinates(x, y);
                    }
                }
            }
            return Coordinates.invalid;
        }
        // search in the whole map
        public bool hasEntity(Entity ent)
        {
            return (!find(ent).Equals(Coordinates.invalid));
        }
        // search in list
        public bool hasEntity(Entity ent, Coordinates coords)
        {
            return map[coords.x, coords.y].Contains(ent);
        }
    }

    // Counted from [0,0]
    // x - vertical (goes from to to down)
    // y - horisontal (goes left to right)
    public struct Coordinates {
        public int x, y;
        public static Coordinates invalid = new Coordinates(-1,-1);
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
    }
    public enum Direction { Up = 0, Right, Left, Down };
    public enum Rotation { Forward = 0, CW, Backwards, CCW }

    public abstract class Entity {
        // Coordinates will be both in map and in entity.
        // We'll need to synchronize this.
        protected Coordinates coords;

        public virtual bool isWalkable() { return false; }
        public abstract void turn();
        // returns money for attack
        public abstract int acceptAttack(int hitcount);
    }

    // Better would be to use iterfaces in place of abstract classes,
    // but how to incorporate new fields into interface???
    public abstract class MovableEntity : Entity
    {
        protected Direction direction;

        // true, if step was made
        public bool go() {
            Map map = Map.getInstance();
            Coordinates step = coords.step(direction);
            if (map.isWalkable(step)) {
                map.move(this, coords, step);
                coords = step;
                return true;
            }
            return false;
        }
        public void rotate(Rotation rot) {
            direction = (Direction)(((int)direction + (int)rot) % 4);
        }
    }

    public abstract class LiveEntity : MovableEntity {
        protected int health;
        protected int maxHealth;
        protected int lives;
        protected int defaultLives;

        // change < 0 ... hurt
        // change > 0 ... stimpack
        // Take care of correct lives count when health goes through 0 or maxHealth.
        // Returns true, if still alive.
        public virtual bool changeHealth(int change) {
            health += change;
            if (health > maxHealth) {
                lives += health / maxHealth;
                health %= maxHealth;
            } else if (health < 0) {
                if (lives > 0) {
                    lives--;
                    health += maxHealth;
                }
                die();
                return false;
            }
            return true;
        }
        public abstract void die();
        public virtual void attack(Entity e, int hitcount) {
            e.acceptAttack(hitcount);
        }
    }

    public abstract class WalkableEntity : Entity {
        // other entities can walk through this one
        public override bool isWalkable() { return true; }
    }

    public class PlayerEntity : LiveEntity {
        // money for killing monsters, gathering bonuses, aligning diamonds, ...
        int money;
        public override void turn() { }
        public override int acceptAttack(int hitcount) {
            changeHealth(hitcount);
            return 0;
        }
        public override void die() {
            if ((lives >= 0) && (health >= 0)) {
                // TODO: respawn with the same entity
            } else {
                // TODO: end of game
            }
        }
        public override void attack(Entity e, int hitcount) {
            money += e.acceptAttack(hitcount);
        }
    }
    // maybe think of multiple types of mosters
    public class MonsterEntity : LiveEntity {
        protected enum States { Normal, Stunned }
        protected States state; // maybe better would be design patter State
        // TODO: put this constant somewhere else (eg. into a config file)
        static int moneyForKilling = 1;

        public MonsterEntity() {
            state = States.Normal;
        }
        public override void turn() {
            // chase the player nad try to kill him
        }
        public override int acceptAttack(int hitcount) {
            if (!changeHealth(hitcount)) {
                // it died, give money to the killer
                return moneyForKilling;
            }
            return 0;
        }
        public override void die() {
            // TODO: schedule respawning, make new entity
        }
    }

    public class Stone : Entity {
        public override void turn() { } // empty
        public override int acceptAttack(int hitcount) { return 0; } // empty
        // does not interact, except it is non-walkable
    }

    public class IceBlock : MovableEntity {
        protected enum States { Rest, Movement }
        protected States state;
        public IceBlock() {
            state = States.Rest;
        }
        public override void turn() {
            if (state == States.Movement) {
                if (!go()) { // make step, if in movement
                    state = States.Rest; // stop when encountering non-walkable place
                }
            }
        } 
        public override int acceptAttack(int hitcount) {
            // TODO: melt
            return 0;
        }
    }

    public class FreePlace : WalkableEntity {
        public override void turn() { } // empty
        public override int acceptAttack(int hitcount) { return 0; } // empty
        // does not interact, except it is walkable
    }

    // TODO: classes for various bonuses
    public class Bonus : WalkableEntity {
        public Bonus() {
            // TODO: schedule vanishing in given time
        }
        public override void turn() { }
        public override int acceptAttack(int hitcount) { return 0; }

        // TODO: Resolve, how to give player the bonus content,
        // if he steps on the same place as this bonus.
    }
}
