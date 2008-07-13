using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public abstract class Entity
    {
        // Coordinates will be both in map and in entity.
        // It's synchronized in Map.add().
        protected Coordinates coords;

        public Entity() {
            coords = Coordinates.invalid;
        }
        public abstract void turn();
        public abstract void acceptAttack(int hitcount);
        public void vanish() {
            Game.Map.remove(this, coords);
        }
        public Coordinates Coords {
            get { return coords; }
            set { coords = value; }
        }
    }

    // Better would be to use iterfaces in place of abstract classes,
    // but how to incorporate new places into an interface???
    public abstract class MovableEntity : Entity
    {
        protected Direction direction;

        public MovableEntity() {
            direction = Direction.Down;
        }

        // true, if step was made
        public bool go() {
            Map map = Game.Map;
            Coordinates step = coords.step(direction);
            if (map.validCoordinates(step) && canGo(step)) {
                map.move(this, coords, step);
                coords = step;
                return true;
            }
            return false;
        }
        public virtual bool canGo(Coordinates coords) {
            Map map = Game.Map;
            return map.isWalkable(coords);
        }

        public void rotate(Rotation rot) {
            direction = DirectionUtils.rotate(direction, rot);
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
        protected int timeToRespawn;

        // change < 0 ... hurt
        // change > 0 ... stimpack
        // Take care of correct lives count when health goes through 0 or maxHealth.
        // Returns true, if still alive.
        // TODO: maybe better would be to make this an accessor (get/set)
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
        public int Health {
            get { return health; }
        }
        public int MaxHealth {
            get { return maxHealth; }
        }
        public int Lives {
            get { return lives; }
            set { lives = value; }
        }

        public abstract void die();

        public virtual void respawn(LiveEntity newborn) {
            Map map = Game.Map;
            // move to random (walkable) place
            // maybe casting newborn would be needed (?)
            map.add(newborn, map.getRandomWalkablePlace());
        }
    }

    public abstract class WalkableEntity : Entity
    {
        // Other entities can walk through this one.
        //
        // This class adds no methods or properties,
        // its purpose is just to identify walkable entities
        // using 'is' keyword.
    }

    public class PlayerEntity : LiveEntity
    {
        public PlayerEntity() {
            // TODO: Handle exceptions
            health = maxHealth = Config.Instance.getInt("PlayerEntity.maxHealth");
            lives = defaultLives = Config.Instance.getInt("PlayerEntity.defaultLives");
            timeToRespawn = Config.Instance.getInt("PlayerEntity.timeToRespawn");
        }
        public PlayerEntity(PlayerEntity p) {
            coords = p.coords;
            direction = p.direction;
            health = p.health;
            maxHealth = p.maxHealth;
            lives = p.lives;
            defaultLives = p.defaultLives;
        }

        public override void turn() {
            // User's input will be processed here:

            // * find out the requested action
            //   * rotate/move
            //     * Note: player should be faster than monsters
            //   * attack
            // * perform the action
            //   * make a step, if needed

            // * interact with entities on the same place
            Map map = Game.Map;
            foreach (Entity ent in map.getEntitiesByCoords(coords)) {
                if (ent.Equals(this)) {
                    continue;
                } else if (ent is Bonus) {
                    // Take a bonus
                    ((Bonus)ent).giveBonus(this);
                }
            }
        }
        public override void acceptAttack(int hitcount) {
            changeHealth(hitcount);
        }
        public override void die() {
            if ((lives >= 0) && (health >= 0)) {
                // Schedule respawning with a copy of this entity
                Schedule.Instance.add(delegate() {
                        respawn(new PlayerEntity(this));
                    }, timeToRespawn);
                vanish();
            } else {
                Game.Instance.end(); // End of the game
            }
        }
    }

    // THINK: maybe think of multiple types of mosters
    public class MonsterEntity : LiveEntity
    {
        protected enum States { Normal, Stunned } // + Egg
        protected States state; // THINK: maybe better would be State design pattern
        static int moneyForKilling = Config.Instance.getInt("MonsterEntity.moneyForKilling");
        static int attackHitcount = Config.Instance.getInt("MonsterEntity.attackHitcount");

        public MonsterEntity() {
            state = States.Normal;
            health = maxHealth = Config.Instance.getInt("MonsterEntity.maxHealth");
            lives = defaultLives = Config.Instance.getInt("MonsterEntity.defaultLives");
            timeToRespawn = Config.Instance.getInt("MonsterEntity.timeToRespawn");
        }
        public override void turn() {
            // TODO: chase the player (a kind of simple "AI")
            // If the player is at a distant place, go in his direction.

            // Attack player if he is at a neighboring place.
            Map map = Game.Map;
            foreach (Entity ent in map.getNeighbors(coords)) {
                if (ent is PlayerEntity) {
                    // TODO: turn in player's direction
                    ent.acceptAttack(attackHitcount);
                    break;
                }
            }
        }
        public override void acceptAttack(int hitcount) {
            if (!changeHealth(hitcount)) {
                // it died, give money to the player
                Game.Instance.receiveMoney(moneyForKilling);
            }
        }
        public override void die() {
            // schedule respawning, make new entity
            Schedule.Instance.add(delegate() {
                    respawn(new MonsterEntity());
                }, timeToRespawn);
            vanish();
        }
    }

    public class StoneBlock : Entity
    {
        // Does not interact, except it is non-walkable.
        // Can be used for building a border around the map.

        public override void turn() { } // empty
        public override void acceptAttack(int hitcount) { } // empty

    }

    public abstract class MovableBlock : MovableEntity {
        protected enum States { Rest, Movement }
        protected States state;

        public MovableBlock() {
            state = States.Rest;
        }
        public override void turn() {
            if (state == States.Movement) {
                // make a step, if in movement
                if (go()) {
                    Map map = Game.Map;
                    foreach (Entity ent in map.getEntitiesByCoords(coords)) {
                        if (ent is LiveEntity) {
                            // set health to zero, effectively killing the entity
                            ((LiveEntity)ent).changeHealth(-((LiveEntity)ent).Health);
                        }
                    }
                } else {
                    // stop when encountering a non-walkable place
                    state = States.Rest;
                }
                postTurnHook();
                
            }
        }
        public override bool canGo(Coordinates coords) {
            Map map = Game.Map;
            return map.isSmitable(coords);
        }
        public override void acceptAttack(int hitcount) {
            Map map = Game.Map;

            if (canGo(coords.step(direction))) {
                state = States.Movement;
            } else {
                acceptAttackCantGoHook();
            }
        }
        protected abstract void acceptAttackCantGoHook();
        protected abstract void postTurnHook();
    }

    public class DiamondBlock : MovableBlock
    {
        // Alingning Diamonds gives money and stuns monsters

        protected override void acceptAttackCantGoHook() {
            // empty - DiamondBlock doesn't melt
        }
        protected override void postTurnHook() {
            // TODO: DiamondBlock: check alingning
            // if aligned, stun all monsters & give money to the player
        }
    }
    public class IceBlock : MovableBlock
    {
        protected override void acceptAttackCantGoHook() {
            // IceBlock melts, when attacked having no place to go
            vanish();
        }
        protected override void postTurnHook() { } // empty
    }

    public class FreePlace : WalkableEntity
    {
        // Does not interact, except it is walkable

        public override void turn() { } // empty
        public override void acceptAttack(int hitcount) { } // empty
    }

    // Base class for various bonuses
    public abstract class Bonus : WalkableEntity
    {
        protected int timeToLive;

        public Bonus() {
            int timeToLive = Config.Instance.getInt("Bonus.timeToLive");
            // schedule vanishing in given time
            Schedule.Instance.add(delegate() { vanish();  }, timeToLive);
        }
        public override void turn() { }
        public override void acceptAttack(int hitcount) { }
        // Player detects stepping on the bonus himself in his turn.
        public abstract void giveBonus(PlayerEntity player);
    }

    public class MoneyBonus : Bonus
    {
        public override void giveBonus(PlayerEntity player) {
            int bonusMoney = Config.Instance.getInt("MoneyBonus.bonusMoney");
            Game.Instance.receiveMoney(bonusMoney);
            vanish();
        }
    }

    public class HealthBonus: Bonus
    {
        public override void giveBonus(PlayerEntity player) {
            int changeHealthPercent = Config.Instance.getInt("HealthBonus.changeHealthPercent");
            // increase health by 25%
            player.changeHealth(player.MaxHealth * (changeHealthPercent / 100));
            vanish();
        }
    }

    public class LiveBonus : Bonus
    {
        public override void giveBonus(PlayerEntity player) {
            player.Lives += 1; // add 1 life
            vanish();
        }
    }
}
