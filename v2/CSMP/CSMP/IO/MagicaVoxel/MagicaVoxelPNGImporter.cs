using CSMP.Model;
using CSMP.Tools;
using System.Drawing;

namespace CSMP.IO.MagicaVoxel
{
    public class Tile3DPngImporter
    {
        private readonly Dictionary<Color, Material> colourToMaterialMap;
        private readonly int tileSize;

        public Tile3DPngImporter(int tileSize, IEnumerable<(Color Color, Material Material)> colourToMaterialMappings)
        {
            this.tileSize = tileSize;
            colourToMaterialMap = colourToMaterialMappings
                .GroupBy(mapping => mapping.Color)
                .ToDictionary(mapping => mapping.Key, colourToMaterialMappings => colourToMaterialMappings.First().Material);
        }

        public Tile3D? Import(string filePath)
        {
            try
            {
                string id = Path.GetFileNameWithoutExtension(filePath);
                Bitmap bitmap = new(filePath);
                Tile3D tile = new(id, tileSize);
                for (int z = tileSize - 1; z >= 0; z--)
                {
                    for (int x = 0; x < tileSize; x++)
                    {
                        for (int y = 0; y < tileSize; y++)
                        {
                            Vector3D positionInTileCoords = new(x, y, z);
                            Color color = bitmap.GetPixel(x, y + (tileSize - z - 1) * tileSize);
                            if (color.A == 0) continue; // Empty cell

                            Material? material = colourToMaterialMap[color];
                            if (material is null) continue;
                            tile.Set(positionInTileCoords, material);
                        }
                    }
                }
                return tile;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}