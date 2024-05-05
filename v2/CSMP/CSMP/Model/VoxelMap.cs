using CSMP.Processing;
using CSMP.Tools;

namespace CSMP.Model
{
    public class VoxelMap
    {
        public int TileSize { get; }
        public Vector3D Size { get; }
        public IEnumerable<Material> Materials => tiles
            .OfType<Tile3D>()
            .SelectMany(tile => tile.Materials)
            .Distinct();

        public IEnumerable<(Vector3D, Tile3D)> Tiles => GetTiles();

        private readonly Tile3D?[] tiles;
        private readonly IndexTo3DGridMapper mapper;

        public VoxelMap(Vector3D size, int tileSize)
        {
            Size = size;
            TileSize = tileSize;
            mapper = new IndexTo3DGridMapper(size);
            tiles = new Tile3D[mapper.ArrayLength];
        }

        //public IEnumerable<Vector3D> GetAllVoxels()
        //{
        //    return GetTiles()
        //        .SelectMany(item => item.Tile.GetAllVoxels()
        //            .Select(position => position + item.Position * (TileSize - 1)));
        //}
        public IEnumerable<Vector3D> GetVoxelsOfMaterial(Material material)
        {
            return GetTiles()
                .SelectMany(tile =>
                {
                    bool onEdge = tile.Position.X == Size.X - 1 || tile.Position.Y == Size.Y - 1 || tile.Position.Z == Size.Z - 1;
                    return tile.Tile
                        .GetVoxelsOfMaterial(material, onEdge)
                        .Select(tilePosition => tilePosition + tile.Position * (TileSize - 1));
                });
        }
        public IEnumerable<(Vector3D Position, Tile3D Tile)> GetTiles()
        {
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    for (int z = 0; z < Size.Z; z++)
                    {
                        Tile3D? tile = tiles[mapper.GetIndex(x, y, z)];
                        if (tile is not null) yield return (new Vector3D(x, y, z), tile);
                    }
                }
            }
        }
        public Tile3D? GetTile(Vector3D tileIndex) => tiles[mapper.GetIndex(tileIndex.X, tileIndex.Y, tileIndex.Z)];
        public Material? GetMateral(Vector3D positionInMapCoords)
        {
            Vector3D tileIndex = GetTileIndexFromMapCoords(positionInMapCoords);
            Tile3D? tile = GetTile(tileIndex);
            if (tile is null) return null;

            Vector3D tileOriginInMapCoords = ConvertTileIndexToMapCoords(tileIndex);
            Vector3D positionInTileCoords = positionInMapCoords - tileOriginInMapCoords;
            return tile.Get(positionInTileCoords);
        }
        public Vector3D GetTilePosition(Vector3D mapPosition)
        {
            Vector3D tileIndex = GetTileIndexFromMapCoords(mapPosition);
            Vector3D tilePosition = ConvertTileIndexToMapCoords(tileIndex);
            return mapPosition - tilePosition;
        }
        public Vector3D ConvertTileIndexToMapCoords(Vector3D tileIndex)
        {
            int x = tileIndex.X * Size.X;
            int y = tileIndex.Y * Size.Y;
            int z = tileIndex.Z * Size.Z;
            return new(x, y, z);
        }
        public Vector3D GetTileIndexFromMapCoords(Vector3D mapPosition)
        {
            int x = (int)Math.Floor((double)mapPosition.X / Size.X);
            int y = (int)Math.Floor((double)mapPosition.Y / Size.Y);
            int z = (int)Math.Floor((double)mapPosition.Z / Size.Z);
            return new(x, y, z);
        }
        public bool ClearTile(Vector3D tileIndex) => SetTile(tileIndex, null);
        public bool SetTile(Vector3D tileIndex, Tile3D? tile)
        {
            if (tileIndex < 0 || tileIndex >= Size) return false;
            if (tile is not null && tile.Size != TileSize) return false;
            tiles[mapper.GetIndex(tileIndex.X, tileIndex.Y, tileIndex.Z)] = tile;
            return true;
        }

        public VoxelMap SetPlane(Axis axis, int i, int minJ, int maxJ, int minK, int maxK, Tile3D? tile)
        {
            Func<int, int, int> getIndex = axis switch
            {
                Axis.X => new Func<int, int, int>((j, k) => mapper.GetIndex(i, j, k)),
                Axis.Y => new Func<int, int, int>((j, k) => mapper.GetIndex(j, i, k)),
                Axis.Z => new Func<int, int, int>((j, k) => mapper.GetIndex(j, k, i)),
                _ => throw new Exception()
            };
            for (int j = minJ; j < maxJ; j++)
            {
                for (int k = minK; k < maxK; k++) tiles[getIndex(j, k)] = tile;
            }
            return this;
        }
    }
}