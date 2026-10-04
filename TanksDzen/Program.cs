using System.Text;
using TanksDzen.Game;
using TanksDzen.Map;

namespace TanksDzen
{
    internal class Program
    {
        private const int LevelCount = 3;

        static void Main(string[] args)
        {
            Console.OutputEncoding =
                Encoding.UTF8;

            Console.CursorVisible = false;

            try
            {
                bool gameCompleted = true;
                bool exitRequested = false;

                for (int level = 1;
                     level <= LevelCount;
                     level++)
                {
                    ShowLevelScreen(level);

                    string mapFileName =
                        $"Level{level}.txt";

                    string mapPath =
                        Path.Combine(
                            AppContext.BaseDirectory,
                            "Maps",
                            mapFileName);

                    GameMap gameMap =
                        MapLoader.Load(mapPath);

                    int enemyCount =
                        2 + level;

                    TanksGameLogic game =
                        new TanksGameLogic(
                            gameMap,
                            enemyCount);

                    bool levelCompleted =
                        game.Run();

                    if (game.ExitRequested)
                    {
                        exitRequested = true;
                        gameCompleted = false;
                        break;
                    }

                    if (!levelCompleted)
                    {
                        gameCompleted = false;
                        break;
                    }

                    if (level < LevelCount)
                    {
                        ShowLevelCompletedScreen(
                            level);
                    }
                }

                if (exitRequested)
                {
                    return;
                }

                if (gameCompleted)
                {
                    ShowGameCompletedScreen();
                }
                else
                {
                    ShowDefeatScreen();
                }
            }
            catch (Exception exception)
            {
                Console.Clear();
                Console.ResetColor();

                Console.WriteLine(
                    $"Ошибка: {exception.Message}");

                Console.WriteLine();

                Console.WriteLine(
                    "Нажми любую клавишу...");

                Console.ReadKey(true);
            }
            finally
            {
                Console.CursorVisible = true;
                Console.ResetColor();
            }
        }

        private static void ShowLevelScreen(
            int level)
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Cyan;

            Console.WriteLine();

            Console.WriteLine(
                "==============================");

            Console.WriteLine(
                $"          УРОВЕНЬ {level}");

            Console.WriteLine(
                "==============================");

            Console.ResetColor();

            Console.WriteLine();

            Console.WriteLine(
                $"Карта: Level{level}.txt");

            Console.WriteLine(
                $"Противников: {2 + level}");

            Console.WriteLine();

            Console.WriteLine(
                "Стрелки - движение");

            Console.WriteLine(
                "Space - выстрел");

            Console.WriteLine(
                "Esc - выход");

            Console.WriteLine();

            Console.WriteLine(
                "Нажми любую клавишу...");

            Console.ReadKey(true);

            Console.Clear();
        }

        private static void ShowLevelCompletedScreen(
            int level)
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine();

            Console.WriteLine(
                "==============================");

            Console.WriteLine(
                $"     УРОВЕНЬ {level} ПРОЙДЕН");

            Console.WriteLine(
                "==============================");

            Console.ResetColor();

            Console.WriteLine();

            Console.WriteLine(
                "Все противники уничтожены.");

            Console.WriteLine();

            Console.WriteLine(
                "Нажми любую клавишу, " +
                "чтобы перейти дальше...");

            Console.ReadKey(true);
        }

        private static void ShowGameCompletedScreen()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine();

            Console.WriteLine(
                "==============================");

            Console.WriteLine(
                "          ПОБЕДА!");

            Console.WriteLine(
                "==============================");

            Console.ResetColor();

            Console.WriteLine();

            Console.WriteLine(
                "Вы прошли все уровни!");

            Console.WriteLine();

            Console.WriteLine(
                "Нажми любую клавишу для выхода...");

            Console.ReadKey(true);
        }

        private static void ShowDefeatScreen()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine();

            Console.WriteLine(
                "==============================");

            Console.WriteLine(
                "        ВЫ ПРОИГРАЛИ");

            Console.WriteLine(
                "==============================");

            Console.ResetColor();

            Console.WriteLine();

            Console.WriteLine(
                "Ваш танк был уничтожен.");

            Console.WriteLine();

            Console.WriteLine(
                "Нажми любую клавишу для выхода...");

            Console.ReadKey(true);
        }
    }
}