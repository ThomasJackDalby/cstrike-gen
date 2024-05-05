using CSMP.IO;
using CSMP.IO.Hammer;
using CSMP.IO.MagicaVoxel;
using CSMP.Model;
using CSMP.Processing;
using CSMP.Tools;

namespace CSMP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TileGenerator generator = new TileGenerator();
            VoxExporter voxExporter = new VoxExporter()
                .Register(generator.Solid, 1)
                .Register(generator.Frame, 2)
                .Register(generator.Stairs, 3);

            foreach (Tile3D tile in generator.Generate()) voxExporter.Export($"tiles/{tile.ID}.vox", tile);
            
            CreateDefaultMap();
            //while (true) CreateDefaultMap();
        }

        public static void CreateDefaultMap()
        {
            Vector3D mapSize = new(16, 8, 4);
            const int tileSize = 16;

            Material solid = new("Solid");
            Material frame = new("Frame");
            Material stairs = new("Stairs");
            //Material stairsKey = new("Stairs-Key");

            Tile3DLoader tileLoader = new Tile3DLoader(tileSize)
                .Register(solid, 1)
                .Register(frame, 2)
                .Register(stairs, 3);
            //.Register(stairsKey, 4);

            Dictionary<string, Tile3D> tiles = tileLoader.LoadFolder("./tiles")
                .DistinctBy((a, b) => a.IsEquivalent(b))
                .ToDictionary(t => t.ID);
            tiles["solid"] = new Tile3DBuilder(tileSize).Fill(solid).Build("solid");

            foreach (Tile3D tile in tiles.Values)
            {
                if (tile.ID.Contains("empty")) tile.Tags.Add("empty");
            }

            MapGenerator generator = new MapGenerator(tileSize)
                .Add(new Tile3D("empty", tileSize))
                .Add(tiles.Values)
                .SetProbabilityFunction((position, tile, map) =>
                {
                    int factor = 100;

                    //if (tile.Tags.Contains("-f")) factor *= 3;
                    //if (tile.Tags.Contains("-r")) factor *= 3;
                    //if (tile.Tags.Contains("stairs")) factor *= 2;
                    if (tile.Tags.Contains("empty")) factor *= 2;
                    if (tile.Tags.Contains("door")) factor *= 3;
                    //if (tile.Tags.Contains("edge")) factor /= 4;
                    //if (tile.Tags.Contains("external")) factor /= 2;
                    return factor;
                });

            VoxelMap voxelMap = new(mapSize, tileSize);

            Tile3D solidTile = tiles["solid"];
            foreach ((int x, int y) in Enumerable2D.Range(0, voxelMap.Size.X, 0, voxelMap.Size.Y)) voxelMap.SetTile(new Vector3D(x, y, 0), solidTile);

            VoxExporter voxExporter = new VoxExporter()
                .Register(solid, 1)
                .Register(frame, 2);

            Random random = new Random();
            int seed = random.Next();

            voxelMap = generator.Generate(voxelMap, seed);
            if (voxelMap is null) throw new Exception();

            foreach ((int x, int y) in Enumerable2D.Range(0, voxelMap.Size.X, 0, voxelMap.Size.Y)) voxelMap.SetTile(new Vector3D(x, y, 0), null);
            voxExporter.Export($"map-{seed}.vox", voxelMap);

            Map map = new();
            foreach (Material material in voxelMap.Materials)
            {
                VoxelToCuboidConverter converter = new(voxelMap.GetVoxelsOfMaterial(material));
                foreach (Cuboid cuboid in converter.Convert())
                {
                    map.Components.Add(new Component
                    {
                        Cuboid = cuboid,
                        Material = material
                    });
                }
            }

            // add skybox
            Material sky = new("Sky");
            addMapLimits(map, sky);

            HammerVmfExporter vmfExporter = new HammerVmfExporter()
                //.Register(concrete, "concrete/concreteceiling001a")
                .Register(solid, "dev/reflectivity_50")
                .Register(frame, "dev/reflectivity_10")
                .Register(stairs, "dev/dev_measuregeneric01")
                .Register(sky, "tools/toolsskybox");

            using Stream stream = File.Open("map.vmf", FileMode.Create, FileAccess.Write);
            vmfExporter.Export(stream, map);
        }
        private static void addMapLimits(Map map, Material material)
        {
            int margin = 5;
            Cuboid limits = map.Limits;
            limits = new Cuboid(limits.Origin - margin, limits.Size + (2 * margin));

            Cuboid[] cuboids = new[]
            {
                    new Cuboid(new Vector3D(limits.Origin.X, limits.Origin.Y, limits.Origin.Z), new Vector3D(1, limits.Extent.Y - limits.Origin.Y, limits.Extent.Z - limits.Origin.Z)),
                    new Cuboid(new Vector3D(limits.Extent.X, limits.Origin.Y, limits.Origin.Z), new Vector3D(1, limits.Extent.Y - limits.Origin.Y, limits.Extent.Z - limits.Origin.Z)),
                    new Cuboid(new Vector3D(limits.Origin.X+1, limits.Origin.Y, limits.Origin.Z), new Vector3D(limits.Extent.X-limits.Origin.X-1, 1, limits.Extent.Z - limits.Origin.Z)),
                    new Cuboid(new Vector3D(limits.Origin.X+1, limits.Extent.Y, limits.Origin.Z), new Vector3D(limits.Extent.X-limits.Origin.X-1, 1, limits.Extent.Z - limits.Origin.Z)),
                    new Cuboid(new Vector3D(limits.Origin.X, limits.Origin.Y, limits.Origin.Z-1), new Vector3D(limits.Extent.X-limits.Origin.X+1, limits.Extent.Y - limits.Origin.Y+1, 1)),
                    new Cuboid(new Vector3D(limits.Origin.X, limits.Origin.Y, limits.Extent.Z), new Vector3D(limits.Extent.X-limits.Origin.X+1, limits.Extent.Y - limits.Origin.Y+1, 1)),
            };
            map.Components.AddRange(cuboids.Select(cuboid => new Component { Cuboid = cuboid, Material = material }));
        }

    }


    public class ConsoleProgressBar : IDisposable
    {
        private readonly char symbol = '#';
        private readonly int verticalPosition;
        private readonly int numberOfSections = 50;
        private readonly int total;
        private readonly double updatesPerSection;
        private int updatesPerCurrentSection;
        private int currentUpdate;
        private int currentSection;

        public ConsoleProgressBar(int total)
        {
            (_, verticalPosition) = Console.GetCursorPosition();
            this.total = total;
            updatesPerSection = (double)total / numberOfSections;
            updatesPerCurrentSection = (int)Math.Ceiling(updatesPerSection * (currentSection + 1));
        }

        public void Dispose()
        {
            Console.SetCursorPosition(0, verticalPosition);
            Console.WriteLine($"{new string(symbol, numberOfSections)} {total}/{total} (100.00%)");
        }

        public void Update()
        {
            currentUpdate += 1;
            if (currentUpdate > total) currentUpdate = total;
            if (currentUpdate > updatesPerCurrentSection)
            {
                Console.SetCursorPosition(currentSection, verticalPosition);
                Console.Write(symbol);
                Console.SetCursorPosition(numberOfSections+1, verticalPosition);
                Console.Write($"{currentUpdate}/{total} ({(double)currentUpdate / total:P})");
                currentSection += 1;
                updatesPerCurrentSection = (int)Math.Ceiling(updatesPerSection * (currentSection + 1));
            }
        }
    }
}
