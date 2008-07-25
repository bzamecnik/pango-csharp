using System;
using System.Collections.Generic;
using System.Text;

// $Id$

namespace Pango
{
    // ----- class Entity ----------------------------------------
    public abstract class Entity
    {
        // ----- fields --------------------

        // Coordinates will be both in map and in entity.
        // It's synchronized in Map.add().
        protected Coordinates coords;
        // Prevent multiple turn() calls in one step for entities
        // which moved forward.
        public bool turnDone;

        // ----- constructors --------------------

        public Entity() {
            coords = Coordinates.invalid;
            turnDone = false;
        }

        // ----- properties --------------------
        
        public Coordinates Coords {
            get { return coords; }
            set { coords = value; }
        }
        
        // ----- methods --------------------

        // returns true if something was done
        public abstract bool turn();
        public abstract void acceptAttack(Entity sender, int hitcount);
        public void vanish() {
            Game.Instance.Map.remove(this, coords);
        }
        
    }

    // ----- class MovableEntity ----------------------------------------
    public abstract class MovableEntity : Entity
    {
        // NOTE: Better would be to use iterfaces in place of abstract classes,
        // but how to incorporate new fields into an interface?

        // ----- fields --------------------

        protected Direction direction;

        // ----- constructors --------------------

        public MovableEntity() {
            direction = Direction.Down;
        }

        // ----- properties --------------------

        public Direction Direction {
            get { return direction; }
            set { direction = value; }
        }

        // ----- methods --------------------

        // returns true, if a step was made
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
    }

    // ----- class LiveEntity ----------------------------------------

    public abstract class LiveEntity : MovableEntity
    {
        // ----- fields --------------------

        protected int health;
        protected int maxHealth;
        protected int lives;
        protected int defaultLives;
        protected int timeToRespawn;

        // ----- properties --------------------

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

        // ----- methods --------------------

        public abstract void die();

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

        public void respawn(LiveEntity newborn) {
            // move to random (walkable) place
            Map map = Game.Instance.Map;
            if (map != null) {
                map.add(newborn, map.getRandomWalkablePlace());
            }
        }
    }

    // ----- class WalkableEntity ----------------------------------------
    public abstract class WalkableEntity : Entity
    {
        // Other entities can walk through this one.
        //
        // This class adds no methods or properties,
        // its purpose is just to identify walkable entities
        // using 'is' keyword.
    }

    // ----- class PlayerEntity ----------------------------------------
    public class PlayerEntity : LiveEntity
    {
        // ----- fields --------------------

        protected enum States { Normal, Attack, Dead };
        protected States state;
        private Direction requestedDirection; // for user input
        // these flags are true if an action is scheduled for the next turn
        private bool requestedMovement;
        private bool requestedAttack;

        private int attackHitcount;

        // ----- constructors --------------------

        public PlayerEntity() {
            state = States.Normal;
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
            state = States.Normal;
            coords = p.coords;
            direction = p.direction;
            health = p.health;
            maxHealth = p.maxHealth;
            lives = p.lives;
            defaultLives = p.defaultLives;
            timeToRespawn = p.timeToRespawn;
            attackHitcount = p.attackHitcount;

            requestedDirection = direction;
            requestedMovement = false;
            requestedAttack = false;
        }

        // ----- methods --------------------

        public override bool turn() {
            // User's input is processed here.
            // In one turn player can rotate/move and/or attack.

            if (state == States.Dead) {
                return false;
            }

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

                state = States.Attack;
                Game.Instance.Schedule.add(delegate() { state = States.Normal; }, 1);
            }

            
            // interact with entities on the same place
            // TODO: map.getPlace(coords).Walkable would be enough
            foreach (Entity ent in map.getPlace(coords)) {
                if (ent.Equals(this)) {
                    continue;
                } else if (ent is BonusEntity) {
                    // take a bonus
                    ((BonusEntity)ent).giveBonus(this);
                }
            }
            return true;
        }

        public override void acceptAttack(Entity sender, int hitcount) {
            if (state != States.Dead) {
                changeHealth(-hitcount);
            }
        }

        public override void die() {
            state = States.Dead;
            if (lives >= 0) {
                // schedule respawning with a copy of this entity
                // TODO: check if not null
                Game.Instance.Schedule.add(delegate() {
                        PlayerEntity player = new PlayerEntity(this);    
                        respawn(player);
                        Game.Instance.Player = player;
                    }, timeToRespawn);
                int timeToVanish = Config.Instance.getInt("PlayerEntity.timeToVanishDead");
                Game.Instance.Schedule.add(delegate() {
                    vanish();
                }, timeToVanish);
            } else {
                Game.Instance.endGame(); // end of the game
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

        public override string ToString() {
            StringBuilder sb = new StringBuilder("player-");
            sb.Append(state.ToString().ToLower());
            if (state != States.Dead) {
                sb.Append("-");
                sb.Append(direction.ToString().ToLower());
            }
            return sb.ToString();
        }
    }

    // ----- class MonsterEntity ----------------------------------------
    public class MonsterEntity : LiveEntity
    {
        // THINK: maybe think of multiple types of mosters

        // ----- fields --------------------
        protected enum States { Normal, Stunned, Egg }
        protected States state; // THINK: maybe better would be State design pattern
        static int moneyForKilling = Config.Instance.getInt("MonsterEntity.moneyForKilling");
        static int attackHitcount = Config.Instance.getInt("MonsterEntity.attackHitcount");
        static int timeToIncubate = Config.Instance.getInt("MonsterEntity.timeToIncubate");
        bool memory;

        // ----- constructors --------------------

        public MonsterEntity() {
            state = States.Egg;
            health = maxHealth = Config.Instance.getInt("MonsterEntity.maxHealth");
            lives = defaultLives = Config.Instance.getInt("MonsterEntity.defaultLives");
            timeToRespawn = Config.Instance.getInt("MonsterEntity.timeToRespawn");
            memory = false;

            // TODO: check if not null
            Game.Instance.Schedule.add(delegate() {
                state = States.Normal;
            }, timeToIncubate);
        }

        // ----- properties --------------------

        public bool Active {
            get { return (state != States.Egg); }
        }

        // ----- methods --------------------

        public override bool turn() {
            Map map = Game.Instance.Map;
            if ((state == States.Stunned) || (state == States.Egg)) { return false; }
            //else if (state == States.Egg) {
            //    // if there are few monsters, it's time to incubate from eggs
            //    // Problems:
            //    // * differentiate incubating egg from eggs not prepared
            //    // * map would need a monster quota explicitly specified
            //    if (map.Monsters.Count < map.MonstersQuota) {
            //        Schedule.Instance.add(delegate() {
            //            state = States.Normal;
            //        }, timeToIncubate);
            //    }
            //    return true;
            //}
            
            // make monsters' movements slower
            // TODO: this constant to config
            if ((Game.Instance.Time % 2) != 0) {
                return false;
            }

            attack();
            
            // TODO: chase the player (a kind of simple "AI")
            // If the player is at a distant place, go in his direction.

            // For now a simpler algorithm would be enough

            // randomly rotate
            Random random = Game.Instance.Random;
            if (random.Next(4) == 0) {
                Rotation rot = (Rotation)random.Next(0, 3);
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
                case States.Egg:
                    die();
                    break;
            }
        }

        public override void die() {
            // it died, give money to the player
            Game.Instance.receiveMoney(moneyForKilling);
            //// schedule respawning, make new entity
            //if ((state == States.Normal) || (state == States.Stunned)) {
            //    Schedule.Instance.add(delegate() {
            //        respawn(new MonsterEntity());
            //    }, timeToRespawn);
            //}
            vanish();
        }

        public void stun(int time) {
            if (state == States.Egg) { return; }
            state = States.Stunned;
            // TODO: check if not null
            Game.Instance.Schedule.add(delegate() {
                state = States.Normal; // ok?
            }, time);
        }

        private void attack() {
            // turn in player's direction if they are neighbors
            if ((Game.Instance == null) || (Game.Instance.Player == null)) { return; }
            Coordinates playerCoords = Game.Instance.Player.Coords;
            if (coords.isNeighbor(playerCoords)) {
                // turn to the player
                Nullable<Direction> dir = Coordinates.diffDirection(coords, playerCoords);
                if (dir.HasValue) {
                    direction = dir.Value;
                }
                // attack the player if he is at front of monster
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

        public override string ToString() {
            StringBuilder sb =new StringBuilder("monster-");
            sb.Append(state.ToString().ToLower());
            if(state==States.Normal) {
                sb.Append("-");
                sb.Append(direction.ToString().ToLower());
            }
            return sb.ToString();
        }
    }

    // ----- class StoneBlock ----------------------------------------
    public class StoneBlock : Entity
    {
        // It is used for building a border around the map.
        // It does not interact much, except it is non-walkable and when
        // attacked while in the border wall it stuns monsters along.

        // ----- methods --------------------

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

        public override string ToString() {
            return "stoneblock";
        }
    }

    // ----- class MovableBlock ----------------------------------------
    public abstract class MovableBlock : MovableEntity
    {
        // ----- fields --------------------
        protected enum States { Normal, Movement }
        protected States state;

        // ----- constructor --------------------

        public MovableBlock() {
            state = States.Normal;
        }

        // ----- methods --------------------

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
                    state = States.Normal;
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

    // ----- class DiamondBlock ----------------------------------------
    public class DiamondBlock : MovableBlock
    {
        // Alingning Diamonds gives money and stuns monsters

        // ----- fields --------------------

        protected new enum States { Normal, Movement, Active };
        protected new States state;

        // ----- methods --------------------

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
                        state = States.Active;
                        //neighbor1.State = States.Active;
                        //neighbor2.State = States.Active;
                        // stun all monsters
                        foreach (MonsterEntity ent in map.Monsters) {
                            int time = Config.Instance.getInt("MonsterEntity.timeToWakeupDiamond");
                            (ent).stun(time);
                        }
                        
                        // give money to the player
                        int bonusMoney = Config.Instance.getInt("DiamondBlock.bonusMoney");
                        Game.Instance.receiveMoney(bonusMoney);
                        return;
                    }
                }
            }
        }

        public override string ToString() {
            string s = state.ToString().ToLower();
            if (s == "movement") { s = "normal"; }
            return string.Format("diamond-{0}", s);
        }
    }

    // ----- class IceBlock ----------------------------------------
    public class IceBlock : MovableBlock
    {
        // ----- fields --------------------
        protected new enum States { Normal, Movement, Melt }
        protected new States state;

        // ----- methods --------------------

        protected override void acceptAttackCantGoHook() {
            // IceBlock melts when attacked having no place to go
            state = States.Melt;
            int timeToMelt = Config.Instance.getInt("IceBlock.timeToMelt");
            Game.Instance.Schedule.add(delegate() { vanish(); }, timeToMelt);
        }

        protected override void turnCantGoHook() { } // empty

        public override string ToString() {
            string s = state.ToString().ToLower();
            if (s == "movement") { s = "normal"; }
            return string.Format("iceblock-{0}", s);
        }
    }

    // ----- class BonusEntity ----------------------------------------
    public abstract class BonusEntity : WalkableEntity
    {
        // Base class for various bonuses

        // ----- constructors --------------------

        public BonusEntity() {
            int timeToLive = Config.Instance.getInt("Bonus.timeToLive");
            // schedule vanishing in given time
            // TODO: check if not null
            Game.Instance.Schedule.add(delegate() { vanish(); }, timeToLive);
        }

        // ----- methods --------------------
        
        public override bool turn() { return false; }
        
        public override void acceptAttack(Entity sender, int hitcount) { } // empty
        
        // Player detects stepping on the bonus himself in his turn.
        public abstract void giveBonus(PlayerEntity player);
    }

    // ----- class MoneyBonus ----------------------------------------
    public class MoneyBonus : BonusEntity
    {
        // ----- methods --------------------

        public override void giveBonus(PlayerEntity player) {
            int bonusMoney = Config.Instance.getInt("MoneyBonus.bonusMoney");
            Game.Instance.receiveMoney(bonusMoney);
            vanish();
        }

        public override string ToString() {
            return "moneybonus";
        }
    }

    // ----- class HealthBonus ----------------------------------------
    public class HealthBonus: BonusEntity
    {
        // ----- methods --------------------

        public override void giveBonus(PlayerEntity player) {
            int changeHealthPercent = Config.Instance.getInt("HealthBonus.changeHealthPercent");
            // increase health by given percent
            player.changeHealth((int)(player.MaxHealth * ((float)changeHealthPercent / 100)));
            vanish();
        }

        public override string ToString() {
            return "healthbonus";
        }
    }

    // ----- class LiveBonus ----------------------------------------
    public class LiveBonus : BonusEntity
    {
        // ----- methods --------------------

        public override void giveBonus(PlayerEntity player) {
            player.Lives += 1; // add 1 life
            vanish();
        }

        public override string ToString() {
            return "livebonus";
        }
    }
}
