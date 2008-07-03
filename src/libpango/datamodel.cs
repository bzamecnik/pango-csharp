using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Schedule {
        int time;
        public void add(EventHandler e, int timeoffset);
        public EventHandler get();
    }
    public class Game {}
    public class Map {
        List<Entity>[,] map;
    }

    public enum Direction { Up, Right, Left, Down }
    public enum Rotation { Forward, CW, Backwards, CCW }

    public abstract class Entity {
        int xCoord;
        int yCoord;

        public virtual bool isWalkable() { return false; }
        public abstract void turn();
        public abstract void acceptAttack();
    }

    public interface ILive {
        int health;
        int maxHealth;
        int lives;
        int defaultLives;

        // change < 0 ... hurt
        // change > 0 ... stimpack
        // Take care of correct lives count when health goes
        // throuh 0 or maxHealth.
        bool changeHealth(int change);
    }

    public interface IMovable {
        Direction direction;

        bool go(Direction dir); // true, if step was made
        void rotate(Rotation rot);
    }

    public interface IOffensive {
        void attack(Entity e, int hitcount);
    }

    public class WalkableEntity : Entity {
        // other entities walk through this one
        public virtual bool isWalkable() { return true; }
    }

    public class PlayerEntity : Entity, ILive, IMovable, IOffensive {}
    public class MonsterEntity : Entity, ILive, IMovable, IOffensive {}

    public class Stone : Entity { }
    public class IceBlock : Entity { }

    public class FreePlace : WalkableEntity { }
    public class Bonus : WalkableEntity { }
}
