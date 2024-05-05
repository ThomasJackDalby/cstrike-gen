using CSMP.Model;
using CSMP.Tools;

namespace CSMP.Processing
{
    public class Tile3DBuilder
    {
        public int Size { get; }
        private readonly Material?[] materials;
        private readonly IndexTo3DGridMapper mapper;
        private readonly HashSet<string> tags = new();

        public Tile3DBuilder(Tile3D tile)
            : this(tile.Size)
        {
            for (int i = 0; i < materials.Length; i++) materials[i] = tile.Get(i);
        }
        public Tile3DBuilder(int size)
        {
            this.Size = size;
            mapper = new IndexTo3DGridMapper(size);
            materials = new Material[mapper.ArrayLength];
        }

        public Tile3DBuilder AddTags(params string[] tags)
        {
            foreach (string tag in tags) this.tags.Add(tag);
            return this;
        }
        public Tile3DBuilder Rotate90()
        {
            Material?[] rotated = new Material?[Size];
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    for (int z = 0; z < Size; z++)
                    {
                        Material? material = materials[mapper.GetIndex(x, y, z)];
                        rotated[mapper.GetIndex(Size - y - 1, x, z)] = material;
                    }
                }
            }
            Array.Copy(rotated, materials, materials.Length);
            return this;
        }
        public Tile3DBuilder Fill(Material? material)
        {
            for (int i = 0; i < materials.Length; i++) materials[i] = material;
            return this;
        }
        public Tile3DBuilder SetColumn(Axis axis, int i, int j, Material? material)
        {
            Func<int, int> getIndex = axis switch
            {
                Axis.X => new Func<int, int>(k => mapper.GetIndex(k, i, j)),
                Axis.Y => new Func<int, int>(k => mapper.GetIndex(i, k, j)),
                Axis.Z => new Func<int, int>(k => mapper.GetIndex(i, j, k)),
                _ => throw new Exception()
            };
            for (int k = 0; k < Size; k++) materials[getIndex(k)] = material;
            return this;
        }
        public Tile3DBuilder SetPlane(Axis axis, int i, Material? material) => SetPlane(axis, i, 0, Size, 0, Size, material);
        public Tile3DBuilder SetPlane(Axis axis, int i, int minJ, int maxJ, int minK, int maxK, Material? material)
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
                for (int k = minK; k < maxK; k++) materials[getIndex(j, k)] = material;
            }
            return this;
        }
        public Tile3D Build(string id)
        {
            Tile3D tile = new Tile3D(id, Size);
            foreach (string tag in tags) tile.Tags.Add(tag);
            for (int i = 0; i < materials.Length; i++) tile.Set(i, materials[i]);
            return tile;
        }
    }
}
