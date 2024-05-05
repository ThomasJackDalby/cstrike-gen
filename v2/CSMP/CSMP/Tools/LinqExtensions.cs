namespace CSMP.Tools
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> self)
        {
            foreach (T? item in self)
            {
                if (item is null) continue;
                yield return item;
            }
        }

        public static T NextFrom<T>(this Random random, ICollection<T> collection) => collection.ElementAt(random.Next(collection.Count));
        public static int NextFromWeights(this Random random, double[] weights)
        {
            double sum = 0;
            for (int i = 0; i < weights.Length; i++) sum += weights[i];
            double threshold = random.NextDouble() * sum;

            double partialSum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                partialSum += weights[i];
                if (partialSum >= threshold) return i;
            }
            return 0;
        }
        public static T[] Fill<T>(this T[] self, T value)
        {
            for (int i = 0; i < self.Length; i++) self[i] = value;
            return self;
        }
        public static T NextFrom<T>(this Random self, T[] values, int[] distribution)
        {
            int total = distribution.Sum();
            int value = self.Next(total);
            int current = 0;
            for (int i = 0; i < values.Length - 1; i++)
            {
                if (value < current + distribution[i]) return values[i];
                current += distribution[i];
            }
            return values[^1];
        }

        public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> self, Func<T, T, bool> comparer)
        {
            List<T> temp = self.ToList();
            while(temp.Count > 0)
            {
                T a = temp.First();
                yield return a;
                temp.RemoveAll(b => comparer(a, b));
            }
        }
    }
}
