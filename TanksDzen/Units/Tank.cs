using TanksDzen.Map;
using TanksDzen.Projectiles;

namespace TanksDzen.Units
{
    public class Tank
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }

        public Direction Direction { get; protected set; }

        public bool IsAlive { get; protected set; }

        public Tank(int x, int y)
        {
            X = x;
            Y = y;

            Direction = Direction.Up;
            IsAlive = true;
        }

        public void SetDirection(Direction direction)
        {
            Direction = direction;
        }

        public void Move(
            Direction direction,
            GameMap map,
            IEnumerable<Tank>? otherTanks = null)
        {
            Direction = direction;

            int nextX = X;
            int nextY = Y;

            switch (direction)
            {
                case Direction.Up:
                    nextY--;
                    break;

                case Direction.Down:
                    nextY++;
                    break;

                case Direction.Left:
                    nextX--;
                    break;

                case Direction.Right:
                    nextX++;
                    break;
            }

            if (!map.CanTankMoveTo(nextX, nextY))
            {
                return;
            }

            if (otherTanks != null)
            {
                bool cellOccupied =
                    otherTanks.Any(
                        tank =>
                            tank != this &&
                            tank.IsAlive &&
                            tank.X == nextX &&
                            tank.Y == nextY);

                if (cellOccupied)
                {
                    return;
                }
            }

            X = nextX;
            Y = nextY;
        }

        public Projectile Shoot()
        {
            int projectileX = X;
            int projectileY = Y;

            switch (Direction)
            {
                case Direction.Up:
                    projectileY--;
                    break;

                case Direction.Down:
                    projectileY++;
                    break;

                case Direction.Left:
                    projectileX--;
                    break;

                case Direction.Right:
                    projectileX++;
                    break;
            }

            return new Projectile(
                projectileX,
                projectileY,
                Direction);
        }

        public void Destroy()
        {
            IsAlive = false;
        }
    }
}