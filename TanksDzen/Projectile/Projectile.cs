using TanksDzen.Units;

namespace TanksDzen.Projectiles
{
    public class Projectile
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Direction Direction { get; }

        public bool IsActive { get; private set; }

        public Projectile(
            int x,
            int y,
            Direction direction)
        {
            X = x;
            Y = y;
            Direction = direction;
            IsActive = true;
        }

        public void Move()
        {
            switch (Direction)
            {
                case Direction.Up:
                    Y--;
                    break;

                case Direction.Down:
                    Y++;
                    break;

                case Direction.Left:
                    X--;
                    break;

                case Direction.Right:
                    X++;
                    break;
            }
        }

        public void Destroy()
        {
            IsActive = false;
        }
    }
}