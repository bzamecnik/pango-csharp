using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Schedule {
        // * maybe make it a singleton
        int time;
        // priority queue
        SortedList<int, Queue<EventHandler>> pqueue;

        public Schedule() {
            time = 0;
            pqueue = new SortedList<int, Queue<EventHandler>>();
        }
        public void add(EventHandler eh, int timeoffset) {
            int priority = time + timeoffset;
            if (!pqueue.ContainsKey(priority)) {
                pqueue.Add(priority, new Queue<EventHandler>());
                pqueue[priority].Enqueue(eh);
            }
        }
        public void callCurrentEvents() {
            if (pqueue.ContainsKey(time)) {
                EventHandler eh;
                Queue<EventHandler> queue = pqueue[time];
                while(queue.Count > 0) {
                    eh = queue.Dequeue();
                    //eh.Invoke(...)
                }
                pqueue.Remove(time);
            }
        }
    }
    public class Game {
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
                ent.Coords = coords;
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
        public List<Entity>.Enumerator getEntitiesByCoordsEnumerator(Coordinates coords) {
            return map[coords.x, coords.y].GetEnumerator();
        }

        // TODO: select random walkable field (for placing new entities, eg.
        // respawning or placing bonuses)
        public Coordinates getRandomWalkableField() {
            return Coordinates.invalid; // stub
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
        public bool areNeighbors(Coordinates coords1, Coordinates coords2) {
            return (((coords1.x == coords2.x) && (Math.Abs(coords1.y - coords2.y) == 1)) ||
                ((coords1.y == coords2.y) && (Math.Abs(coords1.x - coords2.x) == 1)));
        }
        public bool Equals(Coordinates other) {
            return ((x == other.x) && (y == other.y));
        }
    }
    public enum Direction { Up = 0, Right, Left, Down };
    public enum Rotation { Forward = 0, CW, Backwards, CCW }

    public abstract class Entity {
        // Coordinates will be both in map and in entity.
        // We'll need to synchronize this.
        protected Coordinates coords;

        public Entity() {
            coords = Coordinates.invalid;
        }
        public virtual bool isWalkable() { return false; }
        public abstract void turn();
        // returns money for attack
        public abstract int acceptAttack(int hitcount);
        public void vanish() {
            Map.getInstance().remove(this, coords);
        }
        public Coordinates Coords {
            get { return coords; }
            set { coords = value; }
        }
    }

    // Better would be to use iterfaces in place of abstract classes,
    // but how to incorporate new fields into interface???
    public abstract class MovableEntity : Entity
    {
        protected Direction direction;

        public MovableEntity() {
            direction = Direction.Down;
        }

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

        public Direction Direction {
            get { return direction; }
            set { direction = value; }
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

        public virtual void attack(Entity ent, int hitcount) {
            ent.acceptAttack(hitcount);
        }
    }

    public abstract class WalkableEntity : Entity {
        // other entities can walk through this one
        public override bool isWalkable() { return true; }
    }

    public class PlayerEntity : LiveEntity {
        // money for killing monsters, gathering bonuses, aligning diamonds, ...
        int money;

        public PlayerEntity() {
            // TODO: put these constants somewhere else
            health = maxHealth = 100;
            lives = defaultLives = 3;
        }

        public override void turn() {
            // user's input will be processed here
            //Map map = Map.getInstance();
            //List<Entity>.Enumerator ents = map.getEntitiesByCoordsEnumerator();
            //foreach (Entity ent in ents) {
            //    if (ent.Equals(this)) { continue; }
            //    //...
            //}
        }
        public override int acceptAttack(int hitcount) {
            changeHealth(hitcount);
            return 0;
        }
        public override void die() {
            if ((lives >= 0) && (health >= 0)) {
                // TODO: respawn with the same entity
                vanish();
            } else {
                // TODO: end of game
            }
        }
        public override void attack(Entity ent, int hitcount) {
            receiveMoney(ent.acceptAttack(hitcount));
        }
        public void receiveMoney(int amount) {
            money += amount;
        }
    }
    // maybe think of multiple types of mosters
    public class MonsterEntity : LiveEntity {
        protected enum States { Normal, Stunned }
        protected States state; // maybe better would be design patter State
        // TODO: put this constant somewhere else (eg. into a config file)
        static int moneyForKilling = 10;

        public MonsterEntity() {
            state = States.Normal;
            // TODO: put these constants somewhere else
            health = maxHealth = 50;
            lives = defaultLives = 1;
        }
        public override void turn() {
            // chase the player nad try to kill him (a kind of simple "AI")
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
            vanish();
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
            Map map = Map.getInstance();
            
            if (map.isWalkable(coords.step(direction))) {
                state = States.Movement;
            } else {
                // melt
                vanish();
            }
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
