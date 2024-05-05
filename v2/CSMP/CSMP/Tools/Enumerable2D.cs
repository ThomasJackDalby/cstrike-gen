namespace CSMP.Tools
{
    public static class Enumerable2D
    {
        public static IEnumerable<(int i, int j)> Range(int startI, int countI, int startJ, int countJ) => Enumerable
            .Range(startI, countI)
            .SelectMany(i => Enumerable
                .Range(startJ, countJ)
                .Select(j => (i, j)));
    }
}