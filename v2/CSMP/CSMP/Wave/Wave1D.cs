namespace CSMP.Wave
{
    public class Wave1D<CellType> : Wave<int, CellType>
    {
        private readonly int size;

        public Wave1D(int seed, int size, CellType[] cellTypes, Func<int, CellType, CellType, bool> isCompatible)
            : base(seed, 1, size, cellTypes, isCompatible)
        {
            this.size = size;
        }

        public override int GetPosition(int index) => index;
        protected override bool IsPositionInLimits(int position, int axis)
        {
            if (position < 0) return false;
            if (position >= size) return false;
            return true;
        }
        protected override int GetAdjecantPosition(int index, int axis, bool direction) => direction ? index + 1 : index - 1;
        protected override int GetIndex(int position) => position;
    }
}