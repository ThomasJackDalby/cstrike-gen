using CSMP.Model;
using CSMP.Tools;
using CSMP.Wave;

namespace CSMP
{
    public class MapGenerator
    {
        public Func<Vector3D, Tile3D, Tile3D?[], int>? ProbabilityFunction { get; set; }

        private readonly List<Tile3D> tiles = new();
        private readonly int tileSize;

        public MapGenerator(int tileSize)
        {
            this.tileSize = tileSize;
        }

        public MapGenerator SetProbabilityFunction(Func<Vector3D, Tile3D, Tile3D?[], int> probabilityFunction)
        {
            ProbabilityFunction = probabilityFunction;
            return this;
        }
        public MapGenerator Add(params Tile3D[] tiles) => Add((IEnumerable<Tile3D>)tiles);
        public MapGenerator Add(IEnumerable<Tile3D> tiles)
        {
            this.tiles.AddRange(tiles);
            return this;
        }

        public VoxelMap Generate(Vector3D mapSize, int? seed = null) => Generate(new VoxelMap(mapSize, tileSize), seed);
        public VoxelMap Generate(VoxelMap map, int? seed = null)
        {
            Random random;
            if (seed.HasValue) random = new Random(seed.Value);
            else random = new Random();

            IEnumerable<(Vector3D, Tile3D)>? result = null;
            while (result is null)
            {
                int iterSeed = random.Next();
                Console.WriteLine($"Trying seed [{iterSeed}]");
                Wave3D<Tile3D> wave = new(
                    iterSeed,
                    map.Size,
                    tiles,
                    isCompatible,
                    initialConditions: map.Tiles,
                    probabilityFunction: ProbabilityFunction);
                result = wave.Collapse();
            }
            if (result is null) throw new Exception();

            foreach ((Vector3D tileIndex, Tile3D tile) in result)
            {
                map.SetTile(new Vector3D(tileIndex.X, tileIndex.Y, tileIndex.Z), tile);
            }

            return map;
        }

        private static bool isCompatible(int direction, Tile3D a, Tile3D b)
        {
            int size = a.Size;
            if (direction == 0) // x-axis
            {
                return Enumerable.Range(0, size)
                    .SelectMany(z => Enumerable.Range(0, size).Select(y => (y, z)))
                    .All(coord => a.Get(size - 1, coord.y, coord.z) == b.Get(0, coord.y, coord.z));
            }
            else if (direction == 1) // y-axis
            {
                return Enumerable.Range(0, size)
                    .SelectMany(z => Enumerable.Range(0, size).Select(x => (x, z)))
                    .All(coord => a.Get(coord.x, size - 1, coord.z) == b.Get(coord.x, 0, coord.z));
            }
            else if (direction == 2) // z-axis
            {
                return Enumerable.Range(0, size)
                    .SelectMany(y => Enumerable.Range(0, size).Select(x => (x, y)))
                    .All(coord => a.Get(coord.x, coord.y, size - 1) == b.Get(coord.x, coord.y, 0));
            }
            else throw new Exception();
        }
    }
}
