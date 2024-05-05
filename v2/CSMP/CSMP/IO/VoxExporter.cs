using CSMP.Model;
using CSMP.Tools;

namespace CSMP.IO
{
    public class VoxExporter
    {
        private readonly Dictionary<Material, int> materialToIndexMap = new();

        public VoxExporter Register(Material material, int index)
        {
            materialToIndexMap[material] = index;
            return this;
        }
        public void Export(string filePath, VoxelMap map)
        {
            MagicaVoxWriter writer = new((byte)(map.Size.X * (map.TileSize - 1) + 1), (byte)(map.Size.Y * (map.TileSize - 1) + 1), (byte)(map.Size.Z * (map.TileSize - 1) + 1));
            foreach (Material material in map.Materials)
            {
                if (!materialToIndexMap.TryGetValue(material, out int index)) continue;
                foreach (Vector3D voxel in map.GetVoxelsOfMaterial(material)) writer.SetVoxel(new Voxel((byte)voxel.X, (byte)voxel.Y, (byte)voxel.Z, (byte)index));
            }
            writer.Save(filePath);
        }
        public void Export(string filePath, Tile3D tile)
        {
            MagicaVoxWriter writer = new((byte)tile.Size, (byte)tile.Size, (byte)tile.Size);
            foreach (Material material in tile.Materials)
            {
                if (!materialToIndexMap.TryGetValue(material, out int index)) continue;
                foreach (Vector3D voxel in tile.GetVoxelsOfMaterial(material)) writer.SetVoxel(new Voxel((byte)voxel.X, (byte)voxel.Y, (byte)voxel.Z, (byte)index));
            }
            writer.Save(filePath);
        }
    }
}