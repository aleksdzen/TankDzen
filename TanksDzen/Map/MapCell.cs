namespace TanksDzen.Map
{
    public class MapCell
    {
        public MapCellType Type { get; private set; }

        public int Health { get; private set; }

        public MapCell(MapCellType type)
        {
            Type = type;

            if (type == MapCellType.Wall)
            {
                Health = 3;
            }
        }

        public bool CanTankMoveThrough()
        {
            return Type == MapCellType.Empty;
        }

        public bool CanProjectileMoveThrough()
        {
            return Type == MapCellType.Empty ||
                   Type == MapCellType.Water;
        }

        public void TakeDamage()
        {
            if (Type != MapCellType.Wall)
            {
                return;
            }

            Health--;

            if (Health <= 0)
            {
                Type = MapCellType.Empty;
            }
        }
    }
}