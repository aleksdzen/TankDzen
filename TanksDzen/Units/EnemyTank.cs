namespace TanksDzen.Units
{
    public class EnemyTank : Tank
    {
        public EnemyTank(int x, int y)
            : base(x, y)
        {
            Direction = Direction.Down;
        }
    }
}