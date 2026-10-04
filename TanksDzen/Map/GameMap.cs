using TanksDzen.Projectiles;
using TanksDzen.Units;

namespace TanksDzen.Map
{
    public class GameMap
    {
        public const int CellWidth = 4;
        public const int CellHeight = 2;

        private readonly MapCell[,] _cells;

        public int Width { get; }
        public int Height { get; }

        public GameMap(string[] lines)
        {
            if (lines.Length == 0)
            {
                throw new Exception("Карта пустая.");
            }

            int textWidth =
                lines.Max(line => line.Length);

            Width = textWidth / CellWidth;
            Height = lines.Length / CellHeight;

            _cells = new MapCell[Width, Height];

            ParseMap(lines);
        }

        private void ParseMap(string[] lines)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int textX = x * CellWidth;
                    int textY = y * CellHeight;

                    char symbol =
                        GetSymbol(
                            lines,
                            textX,
                            textY);

                    MapCellType type =
                        GetCellType(symbol);

                    _cells[x, y] =
                        new MapCell(type);
                }
            }
        }

        private char GetSymbol(
            string[] lines,
            int x,
            int y)
        {
            if (y < 0 ||
                y >= lines.Length)
            {
                return ' ';
            }

            if (x < 0 ||
                x >= lines[y].Length)
            {
                return ' ';
            }

            return lines[y][x];
        }

        private MapCellType GetCellType(
            char symbol)
        {
            if (symbol == '▓')
            {
                return MapCellType.Wall;
            }

            if (symbol == '█')
            {
                return MapCellType.Water;
            }

            return MapCellType.Empty;
        }

        public MapCell GetCell(int x, int y)
        {
            if (!IsInside(x, y))
            {
                throw new ArgumentOutOfRangeException();
            }

            return _cells[x, y];
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 &&
                   x < Width &&
                   y >= 0 &&
                   y < Height;
        }

        public bool CanTankMoveTo(int x, int y)
        {
            if (!IsInside(x, y))
            {
                return false;
            }

            return _cells[x, y]
                .CanTankMoveThrough();
        }

        public bool CanProjectileMoveTo(
            int x,
            int y)
        {
            if (!IsInside(x, y))
            {
                return false;
            }

            return _cells[x, y]
                .CanProjectileMoveThrough();
        }

        public bool IsProjectilePathClear(
            int startX,
            int startY,
            int endX,
            int endY)
        {
            if (startX == endX)
            {
                int minY =
                    Math.Min(startY, endY) + 1;

                int maxY =
                    Math.Max(startY, endY);

                for (int y = minY; y < maxY; y++)
                {
                    if (_cells[startX, y].Type ==
                        MapCellType.Wall)
                    {
                        return false;
                    }
                }

                return true;
            }

            if (startY == endY)
            {
                int minX =
                    Math.Min(startX, endX) + 1;

                int maxX =
                    Math.Max(startX, endX);

                for (int x = minX; x < maxX; x++)
                {
                    if (_cells[x, startY].Type ==
                        MapCellType.Wall)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public void ProcessProjectileCollision(
            Projectile projectile)
        {
            if (!projectile.IsActive)
            {
                return;
            }

            if (!IsInside(
                    projectile.X,
                    projectile.Y))
            {
                projectile.Destroy();
                return;
            }

            MapCell cell =
                _cells[
                    projectile.X,
                    projectile.Y];

            if (cell.Type == MapCellType.Wall)
            {
                cell.TakeDamage();
                projectile.Destroy();
            }
        }

        public void Draw(
            PlayerTank player,
            List<EnemyTank> enemies,
            Projectile? playerProjectile,
            List<Projectile> enemyProjectiles)
        {
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < Height; y++)
            {
                for (int row = 0;
                     row < CellHeight;
                     row++)
                {
                    for (int x = 0;
                         x < Width;
                         x++)
                    {
                        if (player.IsAlive &&
                            player.X == x &&
                            player.Y == y)
                        {
                            DrawTank(
                                player,
                                row,
                                ConsoleColor.Green);

                            continue;
                        }

                        EnemyTank? enemy =
                            enemies.FirstOrDefault(
                                tank =>
                                    tank.IsAlive &&
                                    tank.X == x &&
                                    tank.Y == y);

                        if (enemy != null)
                        {
                            DrawTank(
                                enemy,
                                row,
                                ConsoleColor.Red);

                            continue;
                        }

                        if (playerProjectile != null &&
                            playerProjectile.IsActive &&
                            playerProjectile.X == x &&
                            playerProjectile.Y == y)
                        {
                            DrawProjectile(
                                playerProjectile,
                                row,
                                ConsoleColor.Yellow);

                            continue;
                        }

                        Projectile? enemyProjectile =
                            enemyProjectiles.FirstOrDefault(
                                currentProjectile =>
                                    currentProjectile.IsActive &&
                                    currentProjectile.X == x &&
                                    currentProjectile.Y == y);

                        if (enemyProjectile != null)
                        {
                            DrawProjectile(
                                enemyProjectile,
                                row,
                                ConsoleColor.Magenta);

                            continue;
                        }

                        DrawCell(_cells[x, y]);
                    }

                    Console.WriteLine();
                }
            }

            Console.ResetColor();

            Console.WriteLine(
                $"Противников: " +
                $"{enemies.Count(enemy => enemy.IsAlive)}");

            Console.WriteLine(
                "Стрелки - движение | " +
                "Space - огонь | Esc - выход");
        }

        private void DrawCell(MapCell cell)
        {
            switch (cell.Type)
            {
                case MapCellType.Wall:
                    Console.ForegroundColor =
                        ConsoleColor.DarkYellow;

                    Console.Write("▓▓▓▓");
                    break;

                case MapCellType.Water:
                    Console.ForegroundColor =
                        ConsoleColor.Blue;

                    Console.Write("████");
                    break;

                default:
                    Console.Write("    ");
                    break;
            }

            Console.ResetColor();
        }

        private void DrawTank(
            Tank tank,
            int row,
            ConsoleColor color)
        {
            Console.ForegroundColor = color;

            if (row == 0)
            {
                switch (tank.Direction)
                {
                    case Direction.Up:
                        Console.Write(" ▲  ");
                        break;

                    case Direction.Down:
                        Console.Write("███ ");
                        break;

                    case Direction.Left:
                        Console.Write("██  ");
                        break;

                    case Direction.Right:
                        Console.Write(" ██ ");
                        break;
                }
            }
            else
            {
                switch (tank.Direction)
                {
                    case Direction.Up:
                        Console.Write("███ ");
                        break;

                    case Direction.Down:
                        Console.Write(" ▼  ");
                        break;

                    case Direction.Left:
                        Console.Write("◄██ ");
                        break;

                    case Direction.Right:
                        Console.Write("██► ");
                        break;
                }
            }

            Console.ResetColor();
        }

        private void DrawProjectile(
            Projectile projectile,
            int row,
            ConsoleColor color)
        {
            Console.ForegroundColor = color;

            if (row == 0)
            {
                if (projectile.Direction ==
                        Direction.Up ||
                    projectile.Direction ==
                        Direction.Down)
                {
                    Console.Write(" ●  ");
                }
                else
                {
                    Console.Write("    ");
                }
            }
            else
            {
                if (projectile.Direction ==
                        Direction.Left ||
                    projectile.Direction ==
                        Direction.Right)
                {
                    Console.Write(" ●  ");
                }
                else
                {
                    Console.Write("    ");
                }
            }

            Console.ResetColor();
        }
    }
}