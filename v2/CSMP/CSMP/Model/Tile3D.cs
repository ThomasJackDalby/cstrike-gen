using CSMP.Tools;

namespace CSMP.Model
{
    public class Tile3D
    {
        public string ID { get; }
        public int Size { get; }
        public HashSet<string> Tags { get; } = new();
        public IEnumerable<Material> Materials => materials.Distinct().WhereNotNull();

        private readonly Material?[] materials;
        private readonly IndexTo3DGridMapper mapper;

        public Tile3D(string id, int size)
        {
            ID = id;
            Size = size;
            mapper = new IndexTo3DGridMapper(size);
            materials = new Material[mapper.ArrayLength];
        }

        public IEnumerable<Vector3D> GetAllVoxels(bool onEdge = false) => GetVoxels(onEdge).Select(c => c.Position);
        public IEnumerable<Vector3D> GetVoxelsOfMaterial(Material material, bool onEdge = false) => GetVoxels(onEdge)
            .Where(c => c.Material == material)
            .Select(c => c.Position);
        public IEnumerable<(Vector3D Position, Material Material)> GetVoxels(bool onEdge = false)
        {
            int size = Size; // !onEdge ? Size - 1 : Size;
            return Enumerable3D.Range(new Vector3D(size, size, size))
            .Select(coord =>
            {
                Vector3D position = new(coord.i, coord.j, coord.k);
                Material? material = Get(position);
                return (position, material);
            })
            .Where(pair => pair.material is not null)
            .Select(v => (v.position, v.material!));
        }
        public Tile3D Rotate90(string id)
        {
            Tile3D other = Clone(id);
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    for (int z = 0; z < Size; z++)
                    {
                        Material? material = Get(x, y, z);
                        other.Set(new Vector3D(Size - y - 1, x, z), material);
                    }
                }
            }
            return other;
        }
        public Tile3D SetMaterial(Material targetMaterial, Material? sourceMaterial = null)
        {
            if (sourceMaterial is null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] is not null) materials[i] = targetMaterial;
                }
            }
            else
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == sourceMaterial) materials[i] = targetMaterial;
                }
            }
            return this;
        }

        public Tile3D Clone(string id)
        {
            Tile3D other = new(id, Size);
            foreach (string tag in Tags) other.Tags.Add(tag);
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    for (int z = 0; z < Size; z++)
                    {
                        other.Set(new Vector3D(x, y, z), Get(x, y, z));
                    }
                }
            }
            return other;
        }

        public bool Set(int index, Material? material)
        {
            materials[index] = material;
            return true;
        }
        public bool Set(Vector3D positionInTileCoords, Material? material)
        {
            if (positionInTileCoords < 0 || positionInTileCoords >= Size) return false;
            int index = mapper.GetIndex(positionInTileCoords);
            materials[index] = material;
            return true;
        }
        public Material? Get(int index) => materials[index];
        public Material? Get(int x, int y, int z) => Get(new Vector3D(x, y, z));
        public Material? Get(Vector3D positionInTileCoords)
        {
            if (positionInTileCoords < 0 || positionInTileCoords >= Size) return null;
            int index = mapper.GetIndex(positionInTileCoords);
            return materials[index];
        }
        public bool IsEquivalent(Tile3D other)
        {
            if (Size != other.Size) return false;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != other.materials[i]) return false;
            }
            return true;
        }
        public override string ToString() => $"Tile: {ID}";
    }
}