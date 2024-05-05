namespace CSMP.IO
{
    public class MagicaVoxReader
    {
        public IEnumerable<Voxel> Read(string filePath)
        {
            using FileStream file = new(filePath, FileMode.Open);
            using BinaryReader reader = new(file);

            readString(reader, "VOX ".Length);
            readUInt(reader); // 150
            readString(reader, "MAIN".Length);
            readUInt(reader); // 0
            readUInt(reader); // Count * 4 + 0x434
            readString(reader, "SIZE".Length);
            readUInt(reader);
            readUInt(reader);
            uint maxX = readUInt(reader); // Size X
            uint maxY = readUInt(reader); // Size Y
            uint maxZ = readUInt(reader); // Size Z
            readString(reader, "XYZI".Length);
            uint a = readUInt(reader); // 4 + Count * 4
            readUInt(reader); // 0
            uint count = readUInt(reader); // Count
            List<Voxel> voxels = new();
            for(int v =0;v<count;v++) voxels.Add(readVoxel(reader));
            readString(reader, "RGBA".Length);
            readUInt(reader); // 0x400
            readUInt(reader); // 0
            //for (var i = 0; i < 256; i++) read(writer, Palette[i]);
            return voxels;
        }

        private static Voxel readVoxel(BinaryReader reader)
        {
           byte x = reader.ReadByte();
           byte y = reader.ReadByte();
           byte z = reader.ReadByte();
           byte index = reader.ReadByte();
           return new Voxel(x, y, z, index);
        }
        private static string readString(BinaryReader reader, int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; ++i) chars[i] = (char)reader.ReadByte();
            return new string(chars);
        }
        private static uint readUInt(BinaryReader reader)
        {
            uint a = reader.ReadByte();
            uint b = (uint)reader.ReadByte() << 8;
            uint c = (uint)reader.ReadByte() << 16;
            uint d = (uint)reader.ReadByte() << 24;
            uint result = a + b + c + d;
            return result;
        }
    }
}