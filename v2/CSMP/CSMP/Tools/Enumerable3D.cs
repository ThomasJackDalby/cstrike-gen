namespace CSMP.Tools
{
    public static class Enumerable3D
    {
        public static IEnumerable<(int i, int j, int k)> Range(Vector3D extent)
            => Range(new Vector3D(0, 0, 0), extent);
        public static IEnumerable<(int i, int j, int k)> Range(Vector3D origin, Vector3D extent)
            => Range(origin.X, extent.X - origin.X, origin.Y, extent.Y - origin.Y, origin.Z, extent.Z - origin.Z);
        public static IEnumerable<(int i, int j, int k)> Range(int startI, int countI, int startJ, int countJ, int startK, int countK) => Enumerable2D
            .Range(startI, countI, startJ, countJ)
            .SelectMany(v => Enumerable
                .Range(startK, countK)
                .Select(k => (v.i, v.j, k)));
    }
}