using CSMP.Model;

namespace CSMP.IO.MagicaVoxel
{
    public class Tile3DLoader
    {
        private readonly VoxImporter voxImporter;

        public Tile3DLoader(int tileSize)
        {
            voxImporter = new VoxImporter(tileSize);
        }

        public Tile3DLoader Register(Material material, int index)
        {
            voxImporter.Register(material, index);
            return this;
        }

        public IEnumerable<Tile3D> LoadFolder(string folderPath)
        {
            return Directory.EnumerateFiles(folderPath, "*.vox")
                .SelectMany(filePath => LoadFile(filePath));
        }
        public IEnumerable<Tile3D> LoadFile(string filePath)
        {
            Tile3D? tile = voxImporter.Import(filePath);
            if (tile is null) yield break;

            Tile3D? tile90 = tile.Rotate90($"{tile.ID}-90");
            Tile3D? tile180 = tile90.Rotate90($"{tile.ID}-180");
            Tile3D? tile270 = tile180.Rotate90($"{tile.ID}-270");

            yield return tile;
            yield return tile90;
            yield return tile180;
            yield return tile270;
        }
    }
}
