using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public abstract class Entity
    {
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

    public abstract class LiveEntity : MovableEntity
    {
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

    public abstract class WalkableEntity : Entity
    {
        // other entities can walk through this one
        public override bool isWalkable() { return true; }
    }

    public class PlayerEntity : LiveEntity
    {
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
    public class MonsterEntity : LiveEntity
    {
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

    public class Stone : Entity
    {
        public override void turn() { } // empty
        public override int acceptAttack(int hitcount) { return 0; } // empty
        // does not interact, except it is non-walkable
    }

    public class IceBlock : MovableEntity
    {
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

    public class FreePlace : WalkableEntity
    {
        public override void turn() { } // empty
        public override int acceptAttack(int hitcount) { return 0; } // empty
        // does not interact, except it is walkable
    }

    // TODO: classes for various bonuses
    public class Bonus : WalkableEntity
    {
        public Bonus() {
            // TODO: schedule vanishing in given time
        }
        public override void turn() { }
        public override int acceptAttack(int hitcount) { return 0; }

        // TODO: Resolve, how to give player the bonus content,
        // if he steps on the same place as this bonus.
    }
}
