using CSMP.Tools;

namespace CSMP.Wave
{
    public class Wave3D<CellType> : Wave<Vector3D, CellType>
    {
        private readonly Vector3D size;

        public Wave3D(int seed, Vector3D size, 
            IEnumerable<CellType> cellTypes, 
            Func<int, CellType, CellType, bool> isCompatible,
            Func<Vector3D, CellType, CellType[], int>? probabilityFunction = null,
            IEnumerable<(Vector3D, CellType)>? initialConditions = null)
        : base(seed, 3, size.X * size.Y * size.Z, cellTypes, isCompatible, probabilityFunction, initialConditions)
        {
            this.size = size;
        }

        public override Vector3D GetPosition(int index)
        {
            int z = index / (size.X * size.Y);
            index -= (z * size.X * size.Y);
            int y = index / size.X;
            int x = index % size.X;
            return new(x, y, z);
        }
        protected override int GetIndex(Vector3D position)
        {
            return position.X + position.Y * size.X + position.Z * size.X * size.Y;
        }
        protected override Vector3D GetAdjecantPosition(Vector3D position, int axis, bool direction)
        {
            int delta = direction ? 1 : -1;
            return axis switch
            {
                0 => new(position.X + delta, position.Y, position.Z),
                1 => new(position.X, position.Y + delta, position.Z),
                2 => new(position.X, position.Y, position.Z + delta),
                _ => throw new Exception()
            };
        }
        protected override bool IsPositionInLimits(Vector3D position, int axis)
        {
            return axis switch
            {
                0 => position.X >= 0 && position.X < size.X,
                1 => position.Y >= 0 && position.Y < size.Y,
                2 => position.Z >= 0 && position.Z < size.Z,
                _ => throw new Exception()
            };
        }
    }
}