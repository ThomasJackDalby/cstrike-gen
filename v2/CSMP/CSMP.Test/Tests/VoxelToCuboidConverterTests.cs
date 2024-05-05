using CSMP.Model;
using CSMP.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSMP.Test.Tests
{
    public class VoxelToCuboidConverterTests
    {


        [Fact]
        public void CheckRandom()
        {
            // Assemble
            // Need to create a random grid of connected voxels
            // randomly place in a grid, then remove orphaned ones? infact, don't need to do that
            Random random = new Random(12345);
            Vector3D centre = new Vector3D(2, 2, 2);
            Vector3D[] voxels = Enumerable3D.Range(new Vector3D(4, 4, 4))
                .Select(coords =>
                {
                    Vector3D voxel = new(coords.i, coords.j, coords.k);
                    double value = 10 - Math.Abs((voxel - centre).Sum());
                    if (random.NextDouble() > value) return voxel;
                    return null;
                })
                .WhereNotNull()
                .ToArray();
            VoxelToCuboidConverter converter = new(voxels);

            // Action
            Cuboid[] cuboids = converter.Convert().ToArray();

            // Assemble
            // Output to .vox?

        }

        [Fact]
        public void CheckSingle()
        {
            // Assemble
            Vector3D[] voxels = new Vector3D[] { new Vector3D(0, 0, 0) };
            VoxelToCuboidConverter converter = new(voxels);

            // Action
            Cuboid[] cuboids = converter.Convert().ToArray();

            // Assemble
            Assert.Single(cuboids);
            Cuboid cuboid = cuboids[0];
            Assert.Equal(new Vector3D(0, 0, 0), cuboid.Origin);
            Assert.Equal(new Vector3D(1, 1, 1), cuboid.Size);
        }
        [Fact]
        public void CheckBasic()
        {
            // Assemble
            Vector3D[] voxels = new Vector3D[] 
            {
                new Vector3D(0, 0, 0),
                new Vector3D(1, 0, 0),
                new Vector3D(2, 0, 0),
                new Vector3D(0, 1, 0),
                new Vector3D(1, 1, 0),
                new Vector3D(2, 1, 0),
            };
            VoxelToCuboidConverter converter = new(voxels);

            // Action
            Cuboid[] cuboids = converter.Convert().ToArray();

            // Assemble
            Assert.Single(cuboids);
            Cuboid cuboid = cuboids[0];
            Assert.Equal(new Vector3D(0, 0, 0), cuboid.Origin);
            Assert.Equal(new Vector3D(3, 2, 1), cuboid.Size);
        }
    }
}
