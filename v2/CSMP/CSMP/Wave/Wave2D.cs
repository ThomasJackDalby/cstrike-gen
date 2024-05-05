using CSMP.Tools;

namespace CSMP.Wave
{
    public class Wave2D<CellType> : Wave<Vector2D, CellType>
    {
        private readonly Vector2D size;
        public Wave2D(int seed, Vector2D size, CellType[] cellTypes, Func<int, CellType, CellType, bool> isCompatible)
        : base(seed, 2, size.X * size.Y, cellTypes, isCompatible)
        {
            this.size = size;
        }
        public override Vector2D GetPosition(int index)
        {
            int x = index % size.X;
            int y = (index - x) / size.X;
            return new(x, y);
        }
        protected override int GetIndex(Vector2D position)
        {
            return position.Y * size.X + position.X;
        }
        protected override Vector2D GetAdjecantPosition(Vector2D position, int axis, bool direction)
        {
            int delta = direction ? 1 : -1;
            return axis switch
            {
                0 => new(position.X + delta, position.Y),
                1 => new(position.X, position.Y + delta),
                _ => throw new Exception()
            };
        }
        protected override bool IsPositionInLimits(Vector2D position, int axis)
        {
            return axis switch
            {
                0 => position.X >= 0 && position.X < size.X,
                1 => position.Y >= 0 && position.Y < size.Y,
                _ => throw new Exception()
            };
        }

        public static Vector2D GetPosition(Vector2D size, int index)
        {
            int x = index % size.X;
            int y = (index - x) / size.X;
            return new(x, y);
        }
        public static int GetIndex(Vector2D size, Vector2D position)
        {
            return position.Y * size.X + position.X;
        }
    }
}