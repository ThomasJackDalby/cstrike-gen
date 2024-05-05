namespace CSMP.Tools
{
    public record Vector3D(int X, int Y, int Z)
    {
        public int Sum() => X + Y + Z;
        
        public static Vector3D operator +(Vector3D a, int b) => new(a.X + b, a.Y + b, a.Z + b);
        public static Vector3D operator -(Vector3D a, int b) => new(a.X - b, a.Y - b, a.Z - b);
        public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3D operator *(Vector3D a, Vector3D b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vector3D operator *(Vector3D a, int value) => new(a.X * value, a.Y * value, a.Z * value);

        public static bool operator <(Vector3D self, Vector3D other)
        {
            if (self.X >= other.X) return false;
            if (self.Y >= other.Y) return false;
            if (self.Z >= other.Z) return false;
            return true;
        }
        public static bool operator >(Vector3D self, Vector3D other)
        {
            if (self.X <= other.X) return false;
            if (self.Y <= other.Y) return false;
            if (self.Z <= other.Z) return false;
            return true;
        }
        public static bool operator <=(Vector3D self, Vector3D other)
        {
            if (self.X > other.X) return false;
            if (self.Y > other.Y) return false;
            if (self.Z > other.Z) return false;
            return true;
        }
        public static bool operator >=(Vector3D self, Vector3D other)
        {
            if (self.X < other.X) return false;
            if (self.Y < other.Y) return false;
            if (self.Z < other.Z) return false;
            return true;
        }
        public static bool operator ==(Vector3D self, int value) => self == new Vector3D(value, value, value);
        public static bool operator !=(Vector3D self, int value) => !(self == value);
        public static bool operator <(Vector3D self, int value) => self < new Vector3D(value, value, value);
        public static bool operator >(Vector3D self, int value) => self > new Vector3D(value, value, value);
        public static bool operator <=(Vector3D self, int value) => self <= new Vector3D(value, value, value);
        public static bool operator >=(Vector3D self, int value) => self >= new Vector3D(value, value, value);
    }
}