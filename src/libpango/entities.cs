using System;
using System.Collections.Generic;
using System.Text;

namespace Pango
{
    public abstract class Entity
    {
        // Coordinates will be both in map and in entity.
        // It's synchronized in Map.add().
        protected Coordinates coords;
        // prevent multiple turn() calls in one step for entities
        // which moved forward
        public bool turnDone;

        public Entity() {
            coords = Coordinates.invalid;
            turnDone = false;
        }
        public abstract bool turn(); // returns true if something was done
        public abstract void acceptAttack(Entity sender, int hitcount);
        public void vanish() {
            Game.Instance.Map.remove(this, coords);
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
            Map map = Game.Instance.Map;
            Coordinates step = coords.step(direction);
            if (map.areValidCoordinates(step) && canGo(step)) {
                map.move(this, coords, step);
                coords = step;
                return true;
            }
            return false;
        }
        public virtual bool canGo(Coordinates coords) {
            Map map = Game.Instance.Map;
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
            } else if (health <= 0) {
                if (lives >= 0) {
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
            Map map = Game.Instance.Map;
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
        private Direction requestedDirection; // for user input
        // these flags are true if an action is scheduled for the next turn
        private bool requestedMovement;
        private bool requestedAttack;

        private int attackHitcount;
        
        public PlayerEntity() {
            // TODO: Handle exceptions
            health = maxHealth = Config.Instance.getInt("PlayerEntity.maxHealth");
            lives = defaultLives = Config.Instance.getInt("PlayerEntity.defaultLives");
            timeToRespawn = Config.Instance.getInt("PlayerEntity.timeToRespawn");
            attackHitcount = Config.Instance.getInt("PlayerEntity.attackHitcount");

            requestedDirection = direction;
            requestedMovement = false;
            requestedAttack = false;
        }
        public PlayerEntity(PlayerEntity p) {
            coords = p.coords;
            direction = p.direction;
            health = p.health;
            maxHealth = p.maxHealth;
            lives = p.lives;
            defaultLives = p.defaultLives;
            timeToRespawn = p.timeToRespawn;

            requestedDirection = direction;
            requestedMovement = false;
            requestedAttack = false;
        }

        public override bool turn() {
            // User's input is processed here.
            // In one turn player can rotate/move and/or attack.
            // * Note: player should be faster than monsters

             Map map = Game.Instance.Map;

            // rotate/move
            if (requestedMovement) {
                //// rotate and move separately
                //if (requestedDirection != direction) {
                //    go(); // move
                //} else {
                //    direction = requestedDirection; // rotate
                //}
                
                // rotate and move on a single key press
                direction = requestedDirection;
                go(); // move
                requestedMovement = false; // clear
            }

            // attack
            if (requestedAttack) {
                // attack entities on the place ahead
                Coordinates step = coords.step(direction);
                if (map.areValidCoordinates(step)) {
                    foreach (Entity ent in map.getPlace(step)) {
                        if (!ent.Equals(this)) {
                            ent.acceptAttack(this, attackHitcount);
                        }
                    }
                }
                requestedAttack = false; // clear
            }

            
            // interact with entities on the same place
            foreach (Entity ent in map.getPlace(coords)) {
                if (ent.Equals(this)) {
                    continue;
                } else if (ent is Bonus) {
                    // Take a bonus
                    ((Bonus)ent).giveBonus(this);
                }
            }
            return true;
        }
        public override void acceptAttack(Entity sender, int hitcount) {
            changeHealth(-hitcount);
        }
        public override void die() {
            if (lives >= 0) {
                // Schedule respawning with a copy of this entity
                Schedule.Instance.add(delegate() {
                        PlayerEntity player = new PlayerEntity(this);    
                        respawn(player);
                        Game.Instance.Player = player;
                    }, timeToRespawn);
                vanish();
            } else {
                Game.Instance.Player = null;
                Game.Instance.end(); // End of the game
            }
        }
        // this will be called when an arrow key is pressed
        public void requestMovement(Direction dir) {
            requestedMovement = true;
            requestedDirection = dir;
        }
        // this will be called when ATTACK key is pressed
        public void requestAttack() {
            requestedAttack = true;
        }
    }

    // THINK: maybe think of multiple types of mosters
    public class MonsterEntity : LiveEntity
    {
        protected enum States { Normal, Stunned } // + Egg
        protected States state; // THINK: maybe better would be State design pattern
        static int moneyForKilling = Config.Instance.getInt("MonsterEntity.moneyForKilling");
        static int attackHitcount = Config.Instance.getInt("MonsterEntity.attackHitcount");
        bool memory;

        public MonsterEntity() {
            state = States.Normal;
            health = maxHealth = Config.Instance.getInt("MonsterEntity.maxHealth");
            lives = defaultLives = Config.Instance.getInt("MonsterEntity.defaultLives");
            timeToRespawn = Config.Instance.getInt("MonsterEntity.timeToRespawn");
            memory = false;
        }
        public override bool turn() {
            if (state == States.Stunned) { return false; }

            Map map = Game.Instance.Map;

            attack();
            
            // TODO: chase the player (a kind of simple "AI")
            // If the player is at a distant place, go in his direction.

            // For now a simpler algorithm would be enough

            // randomly rotate
            Random r = new Random();
            if (r.Next(4) == 0) {
                Rotation rot = (Rotation)r.Next(0, 3);
                rotate(rot);
            }

            // labyrinth walking algoritm - from Programming II lessons
            if (map.isWalkable(coords.step(DirectionUtils.rotate(direction,Rotation.CW)))) {
                if (memory) {
                    go();
                    memory = false;
                } else {
                    rotate(Rotation.CW);
                    memory = true;
                }
            } else if (map.isWalkable(coords.step(direction))) {
                go();
            } else {
                rotate(Rotation.CCW);
            }
            return true;
        }
        public override void acceptAttack(Entity sender, int hitcount) {
            switch (state) {
                case States.Normal:
                    changeHealth(-hitcount);
                    break;
                case States.Stunned:
                    changeHealth(-health); // die()
                    break;
            }
        }
        public override void die() {
            // Schedule respawning, make new entity.
            // It died, give money to the player.
            Game.Instance.receiveMoney(moneyForKilling);
            Schedule.Instance.add(delegate() {
                    respawn(new MonsterEntity());
                }, timeToRespawn);
            vanish();
        }
        public void stun(int time) {
            state = States.Stunned;
            Schedule.Instance.add(delegate() {
                state = States.Normal; // ok?
            }, time);
        }
        private void attack() {
            // Turn in player's direction, if they are neighbors
            if ((Game.Instance !=null) || (Game.Instance.Player != null)) { return; }
            Coordinates playerCoords = Game.Instance.Player.Coords;
            if (coords.isNeighbor(playerCoords)) {
                Nullable<Direction> dir = Coordinates.diffDirection(coords, playerCoords);
                if (dir.HasValue) {
                    direction = dir.Value;
                }
                // Attack player if he is at front of monster
                Map map = Game.Instance.Map;
                Place place = map.getPlace(coords.step(direction));
                foreach (Entity ent in place) {
                    if (ent is PlayerEntity) {
                        ent.acceptAttack(this, attackHitcount);
                        break;
                    }
                }
            }
        }
    }

    public class StoneBlock : Entity
    {
        // It is used for building a border around the map.
        // It does not interact much, except it is non-walkable and when
        // attacked while in the border wall it stuns monsters along.

        public override bool turn() { return false; } // empty
        public override void acceptAttack(Entity sender, int hitcount) {
            Map map = Game.Instance.Map;
            List<Entity> monsters = new List<Entity>();
            if (coords.x == 0) {
                // top wall
                monsters = getMonstersAlongWall(0, map.Width, 1, null);
            } else if(coords.x == (map.Height - 1)) {
                // bottom wall
                monsters = getMonstersAlongWall(0, map.Width, map.Height - 2, null);
            } else if (coords.y == 0) {
                // left wall
                monsters = getMonstersAlongWall(0, map.Height, null, 1);
            } else if (coords.y == (map.Width - 1)) {
                // right wall
                monsters = getMonstersAlongWall(0, map.Height, null, map.Width - 2);
            }
            int time = Config.Instance.getInt("MonsterEntity.timeToWakeupWall");
            foreach(MonsterEntity monster in monsters) {
                monster.stun(time);
            }
        }
        private List<Entity> getMonstersAlongWall(
            int from, int to,
            Nullable<int> coordX, Nullable<int> coordY)
        {
            Map map = Game.Instance.Map;
            List<Entity> monsters = new List<Entity>();
            for (int i = from; i < to; i++) {
                int x, y;
                if (coordX.HasValue) { x = coordX.Value; } else { x = i; }
                if (coordY.HasValue) { y = coordY.Value; } else { y = i; }
                Entity monster = map.getPlace(new Coordinates(x, y)).NonWalkable;
                if (monster is MonsterEntity) {
                    monsters.Add(monster);
                }
            }
            return monsters;
        }

    }

    public abstract class MovableBlock : MovableEntity {
        protected enum States { Rest, Movement }
        protected States state;

        public MovableBlock() {
            state = States.Rest;
        }
        public override bool turn() {
            if (state == States.Movement) {
                Coordinates step = coords.step(direction);
                Map map = Game.Instance.Map;
                if (map.areValidCoordinates(step) && canGo(step)) {
                    // kill entities in front of this
                    foreach (Entity ent in map.getPlace(step)) {
                        if (ent == this) { break; }
                        LiveEntity liveent = ent as LiveEntity;
                        if (liveent != null) {
                            // set health to zero, effectively killing the entity
                            (liveent).changeHealth(-(liveent).Health);
                        }
                    }
                    // make a step
                    go();
                } else {
                    // stop when encountering a non-walkable place
                    state = States.Rest;
                    turnCantGoHook();
                }
                return true;
            } else {
                return false;
            }
        }
        public override bool canGo(Coordinates coords) {
            Map map = Game.Instance.Map;
            return map.isSmitable(coords);
        }
        public override void acceptAttack(Entity sender, int hitcount) {
            Map map = Game.Instance.Map;

            if ((sender is PlayerEntity) && coords.isNeighbor(sender.Coords)) {
                // set movement direction according to the attacker's position
                Nullable<Direction> dir = Coordinates.diffDirection(sender.Coords, coords);
                if (dir.HasValue) {
                    direction = dir.Value;
                }

                if (canGo(coords.step(direction))) {
                    state = States.Movement;
                } else {
                    acceptAttackCantGoHook();
                }
            }
        }
        protected abstract void acceptAttackCantGoHook();
        protected abstract void turnCantGoHook();
    }

    public class DiamondBlock : MovableBlock
    {
        // Alingning Diamonds gives money and stuns monsters

        protected override void acceptAttackCantGoHook() {
            // empty - DiamondBlock doesn't melt
        }
        protected override void turnCantGoHook() {
            // Check diamonds alingning. If aligned,
            // stun all monsters and give money to the player
            Map map = Game.Instance.Map;
            foreach (Entity neighbor1 in map.getNeighbors(coords)) {
                if (!(neighbor1 is DiamondBlock)
                    || (neighbor1 == this)) { continue; }
                foreach (Entity neighbor2 in map.getNeighbors(neighbor1.Coords)) {
                    if (!(neighbor2 is DiamondBlock)
                        || (neighbor2 == this)) { continue; }
                    if (((coords.x == neighbor1.Coords.x)
                            && (coords.x == neighbor2.Coords.x))
                        || ((coords.y == neighbor1.Coords.y)
                            && (coords.y == neighbor2.Coords.y))) {
                        // 3 diamond block aligned!
                        // stun all monsters
                        // TODO: optimize this
                        foreach (Entity ent in map) {
                            if (ent is MonsterEntity) {
                                int time = Config.Instance.getInt("MonsterEntity.timeToWakeupDiamond");
                                ((MonsterEntity)ent).stun(time);
                            }
                        }
                        
                        // give money to the player
                        int bonusMoney = Config.Instance.getInt("DiamondBlock.bonusMoney");
                        Game.Instance.receiveMoney(bonusMoney);
                        return;
                    }
                }
            }
        }
    }
    public class IceBlock : MovableBlock
    {
        protected override void acceptAttackCantGoHook() {
            // IceBlock melts, when attacked having no place to go
            vanish();
        }
        protected override void turnCantGoHook() { } // empty
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
        public override bool turn() { return false; }
        public override void acceptAttack(Entity sender, int hitcount) { } // empty
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
            player.changeHealth((int)(player.MaxHealth * ((float)changeHealthPercent / 100)));
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
