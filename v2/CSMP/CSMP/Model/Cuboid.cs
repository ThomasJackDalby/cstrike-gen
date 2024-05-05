using CSMP.Tools;

namespace CSMP.Model
{
    public record Cuboid(Vector3D Origin, Vector3D Size)
    {
        public int Volume { get; } = Size.X * Size.Y * Size.Z;
        public Vector3D Extent { get; } = Origin + Size;

        public bool Intersects(Vector3D position) => Origin <= position && position <= Extent;
        public bool Intersects(Cuboid cuboid)
        {
            return Intersects(cuboid.Origin)
                || Intersects(cuboid.Extent)
                || cuboid.Intersects(Origin)
                || cuboid.Intersects(Extent);
        }
        public static Cuboid From(IEnumerable<Cuboid> cuboids)
        {
            int minX = cuboids.Select(c => c.Origin.X).Min();
            int minY = cuboids.Select(c => c.Origin.Y).Min();
            int minZ = cuboids.Select(c => c.Origin.Z).Min();
            int maxX = cuboids.Select(c => c.Origin.X).Max();
            int maxY = cuboids.Select(c => c.Origin.Y).Max();
            int maxZ = cuboids.Select(c => c.Origin.Z).Max();
            return new Cuboid(new Vector3D(minX, minY, minZ), new Vector3D(maxX, maxY, maxZ));
        }

        public IEnumerable<Vector3D> GetVoxels()
        {
            return Enumerable3D.Range(Origin.X, Size.X, Origin.Y, Size.Y, Origin.Z, Size.Z)
                .Select(v => new Vector3D(v.Item1, v.Item2, v.Item3));
        }

        public Vector3D[] GetFace(int axis, bool sign)
        {
            static Vector3D[] createFace(Vector3D s, Vector3D t, int axis)
            {
                return (axis switch
                {
                    0 => new (int, int, int)[] { (t.X, s.Y, s.Z), (t.X, t.Y, s.Z), (t.X, t.Y, t.Z), (t.X, s.Y, t.Z) },
                    1 => new (int, int, int)[] { (s.X, t.Y, s.Z), (t.X, t.Y, s.Z), (t.X, t.Y, t.Z), (s.X, t.Y, t.Z) },
                    2 => new (int, int, int)[] { (s.X, s.Y, t.Z), (t.X, s.Y, t.Z), (t.X, t.Y, t.Z), (s.X, t.Y, t.Z) },
                    _ => throw new Exception()
                }).Select(i => new Vector3D(i.Item1, i.Item2, i.Item3)).ToArray();
            }
            return axis switch
            {
                0 => sign
                    ? createFace(new Vector3D(Extent.X, Origin.Y, Origin.Z), Extent, 0)
                    : createFace(Origin, new Vector3D(Origin.X, Extent.Y, Extent.Z), 0),
                1 => sign
                    ? createFace(new Vector3D(Origin.X, Extent.Y, Origin.Z), Extent, 1)
                    : createFace(Origin, new Vector3D(Extent.X, Origin.Y, Extent.Z), 1),
                2 => sign
                    ? createFace(new Vector3D(Origin.X, Origin.Y, Extent.Z), Extent, 2)
                    : createFace(Origin, new Vector3D(Extent.X, Extent.Y, Origin.Z), 2),
                _ => throw new Exception()
            };
        }
    }
}
