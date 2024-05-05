using CSMP.Model;

namespace CSMP.Tools
{
    public class VoxelToCuboidConverter
    {
        private readonly List<Cuboid> cuboids = new();
        private readonly HashSet<Vector3D> voxels = new();

        public VoxelToCuboidConverter(IEnumerable<Vector3D> voxels)
        {
            this.voxels = new HashSet<Vector3D>(voxels);
        }

        public IEnumerable<Cuboid> Convert()
        {
            Console.WriteLine("Converting voxels to cuboids...");
            while (voxels.Count > 0)
            {
                Vector3D seedPoint = voxels.First();
                voxels.Remove(seedPoint);

                Cuboid cuboid = getLargestCuboidFromSeedPosition(seedPoint);
                Console.Write("|");
                cuboids.Add(cuboid);
                foreach (Vector3D voxel in cuboid.GetVoxels()) voxels.Remove(voxel);
            }
            Console.WriteLine($"Done. Generated {cuboids.Count} cuboids.");
            return cuboids;
        }

        private Cuboid getLargestCuboidFromSeedPosition(Vector3D seedPosition) => expandAndReturnLargestCuboid(new Cuboid(seedPosition, new Vector3D(1, 1, 1)));
        private Cuboid expandAndReturnLargestCuboid(Cuboid cuboid, Direction direction = 0)
        {
            Cuboid currentLargestCuboid = cuboid;
            for (; (int)direction < 6; direction++)
            {
                int axis = direction.GetAxis();
                bool sign = direction.GetSign();
                Cuboid expansion = getExpansion(cuboid, axis, sign);
                bool valid = isExpansionValid(expansion);
                Cuboid expandedCuboid = getExpandedCuboid(cuboid, axis, sign);
                if (valid == false) continue;

                Cuboid childLargestCuboid = expandAndReturnLargestCuboid(expandedCuboid, direction);
                if (currentLargestCuboid is null || childLargestCuboid.Volume > currentLargestCuboid.Volume) currentLargestCuboid = childLargestCuboid;
            }
            return currentLargestCuboid;

            bool isExpansionValid(Cuboid expansion)
            {
                if (expansion.GetVoxels().Any(voxel => !voxels.Contains(voxel))) return false;
                return true;
            }
            static Vector3D getDelta(int axis) => axis switch
            {
                0 => new Vector3D(1, 0, 0),
                1 => new Vector3D(0, 1, 0),
                2 => new Vector3D(0, 0, 1),
                _ => throw new Exception()
            };
            static Cuboid getExpansion(Cuboid cuboid, int axis, bool sign)
            {
                Vector3D delta = getDelta(axis);
                Vector3D factor = (delta - new Vector3D(1, 1, 1)) * -1;
                Vector3D origin = sign
                    ? factor * cuboid.Origin + cuboid.Extent * delta
                    : cuboid.Origin - delta;
                Vector3D size = factor * cuboid.Size + delta;
                return new Cuboid(origin, size);
            }
            static Cuboid getExpandedCuboid(Cuboid cuboid, int axis, bool sign)
            {
                Vector3D delta = getDelta(axis);
                return sign
                    ? new Cuboid(cuboid.Origin, cuboid.Size + delta)
                    : new Cuboid(cuboid.Origin - delta, cuboid.Size + delta);
            }
        }
    }

    public enum Direction
    {
        XPositive = 0,
        YPositive = 1,
        ZPositive = 2,
        XNegative = 3,
        YNegative = 4,
        ZNegative = 5,
    }

    public static class DirectionExtensions
    {
        public static int GetAxis(this Direction direction)
        {
            return direction switch
            {
                Direction.XPositive => 0,
                Direction.XNegative => 0,
                Direction.YPositive => 1,
                Direction.YNegative => 1,
                Direction.ZPositive => 2,
                Direction.ZNegative => 2,
                _ => throw new ArgumentException(null, nameof(direction))
            };
        }
        public static bool GetSign(this Direction direction)
        {
            return direction switch
            {
                Direction.XPositive => true,
                Direction.YPositive => true,
                Direction.ZPositive => true,
                Direction.XNegative => false,
                Direction.YNegative => false,
                Direction.ZNegative => false,
                _ => throw new ArgumentException(null, nameof(direction))
            };
        }
        public static Direction GetNextDirection(this Direction direction) => direction + 1;
    }
}
