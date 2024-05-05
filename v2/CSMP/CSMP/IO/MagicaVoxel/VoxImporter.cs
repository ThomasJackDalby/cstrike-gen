using CSMP.Model;
using CSMP.Tools;

namespace CSMP.IO.MagicaVoxel
{
    public class VoxImporter
    {
        private readonly Dictionary<int, Material> indexToMaterialMap = new();
        private readonly int tileSize;
        private readonly MagicaVoxReader reader = new();

        public VoxImporter(int tileSize)
        {
            this.tileSize = tileSize;
        }

        public VoxImporter Register(Material material, int index)
        {
            indexToMaterialMap[index] = material;
            return this;
        }
        public Tile3D? Import(string filePath)
        {
            string id = Path.GetFileNameWithoutExtension(filePath);
            Tile3D tile = new(id, tileSize);
            foreach (Voxel voxel in reader.Read(filePath))
            {
                Material? material = indexToMaterialMap[voxel.Index];
                tile.Set(new Vector3D(voxel.X, voxel.Y, voxel.Z), material);
            }
            return tile;
        }
    }
}