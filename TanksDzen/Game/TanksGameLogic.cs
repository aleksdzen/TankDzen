using System.Diagnostics;
using TanksDzen.Map;
using TanksDzen.Projectiles;
using TanksDzen.Units;

namespace TanksDzen.Game
{
    public class TanksGameLogic
    {
        private const int GameDelay = 30;
        private const int ProjectileDelay = 120;
        private const int EnemyMoveDelay = 600;
        private const int EnemyShootDelay = 900;

        private readonly Random _random = new Random();

        private readonly GameMap _gameMap;
        private readonly PlayerTank _player;
        private readonly List<EnemyTank> _enemies;

        private Projectile? _playerProjectile;

        private readonly List<Projectile> _enemyProjectiles =
            new List<Projectile>();

        private readonly Stopwatch _playerProjectileTimer =
            Stopwatch.StartNew();

        private readonly Stopwatch _enemyProjectileTimer =
            Stopwatch.StartNew();

        private readonly Stopwatch _enemyMoveTimer =
            Stopwatch.StartNew();

        private readonly Stopwatch _enemyShootTimer =
            Stopwatch.StartNew();

        private bool _isRunning;
        private bool _playerWon;
        private bool _playerLost;
        private bool _exitRequested;

        public bool ExitRequested => _exitRequested;

        public TanksGameLogic(
            GameMap gameMap,
            int enemyCount)
        {
            _gameMap = gameMap;

            _player = CreatePlayer();

            _enemies = CreateEnemies(
                enemyCount);
        }

        public bool Run()
        {
            _isRunning = true;
            _playerWon = false;
            _playerLost = false;
            _exitRequested = false;

            while (_isRunning)
            {
                ProcessInput();

                if (!_isRunning)
                {
                    break;
                }

                CheckPlayerOnEnemyProjectile();

                UpdatePlayerProjectile();

                UpdateEnemies();

                CheckPlayerOnEnemyProjectile();

                UpdateEnemyShooting();

                UpdateEnemyProjectiles();

                CheckProjectileCollisions();

                RemoveInactiveProjectiles();

                _gameMap.Draw(
                    _player,
                    _enemies,
                    _playerProjectile,
                    _enemyProjectiles);

                CheckGameState();

                Thread.Sleep(GameDelay);
            }

            return _playerWon;
        }

        private void ProcessInput()
        {
            if (!Console.KeyAvailable)
            {
                return;
            }

            ConsoleKey key =
                Console.ReadKey(true).Key;

            if (key == ConsoleKey.Escape)
            {
                _exitRequested = true;
                _isRunning = false;

                return;
            }

            List<Tank> allTanks =
                GetAllTanks();

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    _player.Move(
                        Direction.Up,
                        _gameMap,
                        allTanks);
                    break;

                case ConsoleKey.DownArrow:
                    _player.Move(
                        Direction.Down,
                        _gameMap,
                        allTanks);
                    break;

                case ConsoleKey.LeftArrow:
                    _player.Move(
                        Direction.Left,
                        _gameMap,
                        allTanks);
                    break;

                case ConsoleKey.RightArrow:
                    _player.Move(
                        Direction.Right,
                        _gameMap,
                        allTanks);
                    break;

                case ConsoleKey.Spacebar:
                    PlayerShoot();
                    break;
            }

            CheckPlayerOnEnemyProjectile();
        }

        private void PlayerShoot()
        {
            if (_playerProjectile != null &&
                _playerProjectile.IsActive)
            {
                return;
            }

            _playerProjectile =
                _player.Shoot();

            ProcessPlayerProjectile(
                _playerProjectile);

            CheckProjectileCollisions();

            _playerProjectileTimer.Restart();
        }

        private void UpdatePlayerProjectile()
        {
            if (_playerProjectile == null ||
                !_playerProjectile.IsActive)
            {
                return;
            }

            if (_playerProjectileTimer
                    .ElapsedMilliseconds <
                ProjectileDelay)
            {
                return;
            }

            _playerProjectile.Move();

            ProcessPlayerProjectile(
                _playerProjectile);

            CheckProjectileCollisions();

            _playerProjectileTimer.Restart();
        }

        private void ProcessPlayerProjectile(
            Projectile projectile)
        {
            if (!projectile.IsActive)
            {
                return;
            }

            if (!_gameMap.IsInside(
                    projectile.X,
                    projectile.Y))
            {
                projectile.Destroy();
                return;
            }

            EnemyTank? hitEnemy =
                _enemies.FirstOrDefault(
                    enemy =>
                        enemy.IsAlive &&
                        enemy.X == projectile.X &&
                        enemy.Y == projectile.Y);

            if (hitEnemy != null)
            {
                hitEnemy.Destroy();
                projectile.Destroy();

                return;
            }

            _gameMap.ProcessProjectileCollision(
                projectile);
        }

        private void UpdateEnemies()
        {
            if (_enemyMoveTimer
                    .ElapsedMilliseconds <
                EnemyMoveDelay)
            {
                return;
            }

            foreach (EnemyTank enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                Direction direction =
                    GetEnemyMoveDirection(enemy);

                enemy.Move(
                    direction,
                    _gameMap,
                    GetAllTanks());

                CheckEnemyOnPlayerProjectile(
                    enemy);
            }

            _enemyMoveTimer.Restart();
        }

        private Direction GetEnemyMoveDirection(
            EnemyTank enemy)
        {
            // Иногда противник всё равно
            // выбирает случайное направление.
            // Благодаря этому враги не двигаются
            // абсолютно одинаково.
            if (_random.Next(100) < 30)
            {
                return GetRandomDirection();
            }

            int horizontalDistance =
                _player.X - enemy.X;

            int verticalDistance =
                _player.Y - enemy.Y;

            // Сначала стараемся сокращать
            // наибольшее расстояние до игрока.
            if (Math.Abs(horizontalDistance) >
                Math.Abs(verticalDistance))
            {
                if (horizontalDistance < 0)
                {
                    return Direction.Left;
                }

                if (horizontalDistance > 0)
                {
                    return Direction.Right;
                }
            }

            if (verticalDistance < 0)
            {
                return Direction.Up;
            }

            if (verticalDistance > 0)
            {
                return Direction.Down;
            }

            return GetRandomDirection();
        }

        private void CheckEnemyOnPlayerProjectile(
            EnemyTank enemy)
        {
            if (!enemy.IsAlive)
            {
                return;
            }

            if (_playerProjectile == null ||
                !_playerProjectile.IsActive)
            {
                return;
            }

            if (enemy.X ==
                    _playerProjectile.X &&
                enemy.Y ==
                    _playerProjectile.Y)
            {
                enemy.Destroy();

                _playerProjectile.Destroy();
            }
        }

        private void UpdateEnemyShooting()
        {
            if (_enemyShootTimer
                    .ElapsedMilliseconds <
                EnemyShootDelay)
            {
                return;
            }

            if (!_player.IsAlive)
            {
                return;
            }

            foreach (EnemyTank enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                Direction? shootDirection =
                    GetDirectionToPlayer(enemy);

                if (shootDirection == null)
                {
                    continue;
                }

                enemy.SetDirection(
                    shootDirection.Value);

                Projectile projectile =
                    enemy.Shoot();

                ProcessEnemyProjectile(
                    projectile);

                if (projectile.IsActive)
                {
                    _enemyProjectiles.Add(
                        projectile);
                }
            }

            _enemyShootTimer.Restart();
        }

        private Direction? GetDirectionToPlayer(
            EnemyTank enemy)
        {
            if (enemy.X == _player.X)
            {
                if (!_gameMap.IsProjectilePathClear(
                        enemy.X,
                        enemy.Y,
                        _player.X,
                        _player.Y))
                {
                    return null;
                }

                if (_player.Y < enemy.Y)
                {
                    return Direction.Up;
                }

                return Direction.Down;
            }

            if (enemy.Y == _player.Y)
            {
                if (!_gameMap.IsProjectilePathClear(
                        enemy.X,
                        enemy.Y,
                        _player.X,
                        _player.Y))
                {
                    return null;
                }

                if (_player.X < enemy.X)
                {
                    return Direction.Left;
                }

                return Direction.Right;
            }

            return null;
        }

        private void UpdateEnemyProjectiles()
        {
            if (_enemyProjectileTimer
                    .ElapsedMilliseconds <
                ProjectileDelay)
            {
                return;
            }

            foreach (Projectile projectile
                     in _enemyProjectiles)
            {
                if (!projectile.IsActive)
                {
                    continue;
                }

                projectile.Move();

                ProcessEnemyProjectile(
                    projectile);
            }

            CheckProjectileCollisions();

            _enemyProjectileTimer.Restart();
        }

        private void ProcessEnemyProjectile(
            Projectile projectile)
        {
            if (!projectile.IsActive)
            {
                return;
            }

            if (!_gameMap.IsInside(
                    projectile.X,
                    projectile.Y))
            {
                projectile.Destroy();
                return;
            }

            if (_player.IsAlive &&
                projectile.X == _player.X &&
                projectile.Y == _player.Y)
            {
                _player.Destroy();
                projectile.Destroy();

                return;
            }

            _gameMap.ProcessProjectileCollision(
                projectile);
        }

        private void CheckPlayerOnEnemyProjectile()
        {
            if (!_player.IsAlive)
            {
                return;
            }

            foreach (Projectile projectile
                     in _enemyProjectiles)
            {
                if (!projectile.IsActive)
                {
                    continue;
                }

                if (projectile.X == _player.X &&
                    projectile.Y == _player.Y)
                {
                    _player.Destroy();
                    projectile.Destroy();

                    return;
                }
            }
        }

        private void CheckProjectileCollisions()
        {
            if (_playerProjectile == null ||
                !_playerProjectile.IsActive)
            {
                return;
            }

            foreach (Projectile enemyProjectile
                     in _enemyProjectiles)
            {
                if (!enemyProjectile.IsActive)
                {
                    continue;
                }

                if (_playerProjectile.X ==
                        enemyProjectile.X &&
                    _playerProjectile.Y ==
                        enemyProjectile.Y)
                {
                    _playerProjectile.Destroy();
                    enemyProjectile.Destroy();

                    return;
                }
            }
        }

        private void RemoveInactiveProjectiles()
        {
            _enemyProjectiles.RemoveAll(
                projectile =>
                    !projectile.IsActive);
        }

        private void CheckGameState()
        {
            if (!_player.IsAlive)
            {
                _playerLost = true;
                _isRunning = false;

                return;
            }

            bool allEnemiesDestroyed =
                _enemies.All(
                    enemy => !enemy.IsAlive);

            if (allEnemiesDestroyed)
            {
                _playerWon = true;
                _isRunning = false;
            }
        }

        private PlayerTank CreatePlayer()
        {
            for (int y = _gameMap.Height - 2;
                 y >= 1;
                 y--)
            {
                for (int x = 1;
                     x < _gameMap.Width - 1;
                     x++)
                {
                    if (_gameMap.CanTankMoveTo(
                            x,
                            y))
                    {
                        return new PlayerTank(
                            x,
                            y);
                    }
                }
            }

            throw new Exception(
                "На карте нет свободной клетки " +
                "для танка игрока.");
        }

        private List<EnemyTank> CreateEnemies(
            int count)
        {
            List<EnemyTank> enemies =
                new List<EnemyTank>();

            for (int y = 1;
                 y < _gameMap.Height - 1;
                 y++)
            {
                for (int x = 1;
                     x < _gameMap.Width - 1;
                     x++)
                {
                    if (enemies.Count >= count)
                    {
                        return enemies;
                    }

                    if (!_gameMap.CanTankMoveTo(
                            x,
                            y))
                    {
                        continue;
                    }

                    if (_player.X == x &&
                        _player.Y == y)
                    {
                        continue;
                    }

                    bool occupied =
                        enemies.Any(
                            enemy =>
                                enemy.X == x &&
                                enemy.Y == y);

                    if (occupied)
                    {
                        continue;
                    }

                    enemies.Add(
                        new EnemyTank(x, y));
                }
            }

            if (enemies.Count < count)
            {
                throw new Exception(
                    "На карте недостаточно свободных " +
                    "клеток для противников.");
            }

            return enemies;
        }

        private Direction GetRandomDirection()
        {
            int value = _random.Next(4);

            return value switch
            {
                0 => Direction.Up,
                1 => Direction.Down,
                2 => Direction.Left,
                _ => Direction.Right
            };
        }

        private List<Tank> GetAllTanks()
        {
            List<Tank> tanks =
                new List<Tank>();

            if (_player.IsAlive)
            {
                tanks.Add(_player);
            }

            foreach (EnemyTank enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    tanks.Add(enemy);
                }
            }

            return tanks;
        }
    }
}