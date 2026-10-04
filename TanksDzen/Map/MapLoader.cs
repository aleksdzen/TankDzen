namespace TanksDzen.Map
{
    public static class MapLoader
    {
        public static GameMap Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Файл карты не найден: {filePath}");
            }

            string[] lines =
                File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                throw new Exception(
                    "Файл карты пуст.");
            }

            return new GameMap(lines);
        }
    }
}