using CSMP.Model;
using CSMP.Tools;
using Sledge.Formats.Map.Formats;
using Sledge.Formats.Map.Objects;
using System.Numerics;

namespace CSMP.IO.Hammer
{
    public class HammerVmfExporter
    {
        private readonly HammerVmfFormat hammerVmfFormat = new();

        private readonly Dictionary<Material, string> materialToTextureMap = new();

        private readonly string buyZoneTexture = "tools/toolstrigger";

        public HammerVmfExporter Register(Material material, string textureName)
        {
            materialToTextureMap[material] = textureName;
            return this;
        }

        public void Export(Stream stream, Map map)
        {
            MapFile mapFile = hammerVmfFormat.ReadFromFile(@"C:\Users\Tom\source\repos\thomasjackdalby\cstrike-gen\v1\components\walls.vmf");
            mapFile.Worldspawn.Children.Clear();
            foreach (Component component in map.Components)
            {
                if (component.Material is null 
                    || !materialToTextureMap.TryGetValue(component.Material, out string? textureName)) continue;
                Solid solid = convertCuboidToSolid(component.Cuboid, textureName);
                mapFile.Worldspawn.Children.Add(solid);
            }

            Entity counterBuyZone = new();
            counterBuyZone.ClassName = "func_buyzone";
            counterBuyZone.Properties.Add("TeamNum", "3");
            counterBuyZone.Children.Add(convertCuboidToSolid(new Cuboid(new Vector3D(0, 0, 15), new Vector3D(15, map.Limits.Size.Y, 15)), buyZoneTexture));
            mapFile.Worldspawn.Children.Add(counterBuyZone);

            Entity terrBuyZone = new();
            terrBuyZone.ClassName = "func_buyzone";
            terrBuyZone.Properties.Add("TeamNum", "2");
            terrBuyZone.Children.Add(convertCuboidToSolid(new Cuboid(new Vector3D(map.Limits.Size.X-18, 0, 15), new Vector3D(15, map.Limits.Size.Y, 15)), buyZoneTexture));
            mapFile.Worldspawn.Children.Add(terrBuyZone);

            hammerVmfFormat.Write(stream, mapFile, "");
        }

        public void Export(Stream stream, VoxelMap map)
        {
            MapFile mapFile = hammerVmfFormat.ReadFromFile(@"C:\Users\Tom\source\repos\thomasjackdalby\cstrike-gen\v1\components\walls.vmf");
            mapFile.Worldspawn.Children.Clear();
            List<Cuboid> allCuboids = new();
            foreach (Material material in map.Materials)
            {
                if (!materialToTextureMap.TryGetValue(material, out string? textureName)) continue;
                VoxelToCuboidConverter converter = new(map.GetVoxelsOfMaterial(material));
                foreach (Cuboid cuboid in converter.Convert())
                {
                    Solid solid = convertCuboidToSolid(cuboid, textureName);
                    mapFile.Worldspawn.Children.Add(solid);
                    allCuboids.Add(cuboid);
                }
            }

            hammerVmfFormat.Write(stream, mapFile, "");
        }

        

        private static Solid convertCuboidToSolid(Cuboid cuboid, string textureName)
        {
            Solid? solid = new();
            solid.Color = System.Drawing.Color.FromArgb(1, 0, 127, 244);

            Face createFace(int axis, bool sign)
            {
                Face? face = new()
                {
                    UAxis = axis switch
                    {
                        0 => new Vector3(0, 1, 0),
                        1 => new Vector3(1, 0, 0),
                        2 => new Vector3(1, 0, 0),
                    },
                    XScale = 0.25f,
                    YScale = 0.25f,
                    VAxis = axis switch
                    {
                        0 => new Vector3(0, 0, -1),
                        1 => new Vector3(0, 0, -1),
                        2 => new Vector3(0, -1, 0),
                    },
                    TextureName = textureName,
                    LightmapScale = 16,
                    SmoothingGroups = "0",
                };
                foreach (Vector3D vertex in getFace(cuboid, axis, sign)
                    .Select(i => new Vector3D(i.Item1, i.Item2, i.Item3))) face.Vertices.Add(convert(vertex));
                return face;
            }
            solid.Faces.Add(createFace(2, true));
            solid.Faces.Add(createFace(2, false));
            solid.Faces.Add(createFace(0, false));
            solid.Faces.Add(createFace(0, true));
            solid.Faces.Add(createFace(1, true));
            solid.Faces.Add(createFace(1, false));
            return solid;
        }


        private static (int, int, int)[] getFace(Cuboid cuboid, int axis, bool sign)
        {
            int scale = 10;
            int ox = cuboid.Origin.X * scale;
            int oy = cuboid.Origin.Y * scale;
            int oz = cuboid.Origin.Z * scale;
            int ex = cuboid.Extent.X * scale;
            int ey = cuboid.Extent.Y * scale;
            int ez = cuboid.Extent.Z * scale;
            if (axis == 2 && sign) return new (int, int, int)[] { (ox, ey, ez), (ex, ey, ez), (ex, oy, ez) };
            if (axis == 2 && !sign) return new (int, int, int)[] { (ox, oy, oz), (ex, oy, oz), (ex, ey, oz) };
            if (axis == 0 && !sign) return new (int, int, int)[] { (ox, ey, ez), (ox, oy, ez), (ox, oy, oz) };
            if (axis == 0 && sign) return new (int, int, int)[] { (ex, ey, oz), (ex, oy, oz), (ex, oy, ez) };
            if (axis == 1 && sign) return new (int, int, int)[] { (ex, ey, ez), (ox, ey, ez), (ox, ey, oz) };
            if (axis == 1 && !sign) return new (int, int, int)[] { (ex, oy, oz), (ox, oy, oz), (ox, oy, ez) };
            else throw new Exception();
        }

        private static Vector3 convert(Vector3D vector) => new Vector3(vector.X, vector.Y, vector.Z);
    }
}