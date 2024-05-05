namespace CSMP.Tools
{
    public class IndexTo3DGridMapper
    {
        public int ArrayLength { get; }

        private readonly Vector3D size;

        public IndexTo3DGridMapper(int size)
            : this(new Vector3D(size, size, size))
        { }
        public IndexTo3DGridMapper(Vector3D size)
        {
            this.size = size;
            ArrayLength = size.X * size.Y * size.Z;
        }

        public int GetIndex(Vector3D coordinate) => GetIndex(coordinate.X, coordinate.Y, coordinate.Z);
        public int GetIndex(int x, int y, int z) => z * size.X * size.Y + y * size.X + x;
    }
}