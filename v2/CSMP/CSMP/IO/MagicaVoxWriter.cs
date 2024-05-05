namespace CSMP.IO
{
    public class MagicaVoxWriter
    {
        public byte X { get; }
        public byte Y { get; }
        public byte Z { get; }
        public uint Count { get; private set; }
        public Dictionary<string, Voxel> Voxels { get; private set; }
        public List<RGBA> Palette { get; private set; }

        public MagicaVoxWriter(byte maxX, byte maxY, byte maxZ)
        {
            X = maxX;
            Y = maxY;
            Z = maxZ;
            Count = 0;
            Voxels = new Dictionary<string, Voxel>();
            Palette = new List<RGBA>();

            // Initialize palette
            // Notice 0 represent no voxel and is included as the first element, i from 256-0 inclusive
            // The end result here is to generate from [255, 255, 255, 255] to [0, 0, 0, 255], the last one being "index 0"
            for (int i = 255; i >= 0; i--)
            {
                byte b = (byte)i;
                Palette.Add(new RGBA(b, b, b, 255));
            }
        }

        public void SetVoxel(Voxel voxel)
        {
            if (voxel.X > X || voxel.Y > Y || voxel.Z > Z) return;

            string key = voxel.GetKey();
            if (voxel.Index != 0)
            {
                if (!Voxels.ContainsKey(key)) Count++;
                Voxels[key] = voxel;
            }
            else
            {
                if (Voxels.ContainsKey(key)) Count--;
                Voxels.Remove(key);
            }
        }
        public void Save(string filePath)
        {
            using FileStream file = new(filePath, FileMode.Create);
            using BinaryWriter writer = new(file);

            write(writer, "VOX ");
            write(writer, 150);
            write(writer, "MAIN");
            write(writer, 0);
            write(writer, Count * 4 + 0x434);

            write(writer, "SIZE");
            write(writer, 12);
            write(writer, 0);
            write(writer, X); // Size X
            write(writer, Y); // Size Y
            write(writer, Z); // Size Z
            write(writer, "XYZI");
            write(writer, 4 + Count * 4);
            write(writer, 0);
            write(writer, Count);
            foreach (var voxel in Voxels.Values) write(writer, voxel);
            write(writer, "RGBA");
            write(writer, 0x400);
            write(writer, 0);
            for (var i = 0; i < 256; i++) write(writer, Palette[i]);
        }

        private void write(BinaryWriter data, string str)
        {
            for (var i = 0; i < str.Length; ++i) data.Write((byte)str[i]);
        }
        private void write(BinaryWriter data, uint n)
        {
            data.Write((byte)(n & 0xff));
            data.Write((byte)((n >> 8) & 0xff));
            data.Write((byte)((n >> 16) & 0xff));
            data.Write((byte)((n >> 24) & 0xff));
        }
        private void write(BinaryWriter data, RGBA rgba)
        {
            data.Write(rgba.R);
            data.Write(rgba.G);
            data.Write(rgba.B);
            data.Write(rgba.A);
        }
        private void write(BinaryWriter data, Voxel voxel)
        {
            data.Write(voxel.X);
            data.Write(voxel.Y);
            data.Write(voxel.Z);
            data.Write(voxel.Index);
        }

       

        public struct RGBA
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public RGBA(byte r, byte g, byte b, byte a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }
        }
    }

    public struct Voxel
    {
        public byte X;
        public byte Y;
        public byte Z;
        public byte Index;

        public Voxel(byte x, byte y, byte z, byte index)
        {
            X = x;
            Y = y;
            Z = z;
            Index = index;
        }

        public string GetKey() => X + "_" + Y + "_" + Z;
    }
}