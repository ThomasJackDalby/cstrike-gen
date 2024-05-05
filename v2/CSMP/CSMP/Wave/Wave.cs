using CSMP.Tools;
using Microsoft.VisualBasic;

namespace CSMP.Wave
{

    public abstract class Wave<Position, CellType>
    {
        public interface IMap
        {
            Position GetPosition(int index);

            CellType? GetCellType(Position position);
        }

        public const int UnobservedCell = -1;

        public IEnumerable<(Position, CellType?)> Cells => cells.Select((cellType, i) => (GetPosition(i), cellType));

        private readonly bool[][] allowedCellTypes;
        private readonly CellType?[] cells;
        private readonly CellType[] cellTypes;
        private readonly Func<Position, CellType, CellType?[], int> probabilityFunction;
        private readonly bool[][][] cellTypeCompatibilities;
        private readonly Random random;
        private readonly int numberOfAxes;
        private readonly IEnumerable<(Position, CellType)>? initialConditions;

        public Wave(
            int seed,
            int numberOfAxes,
            int numberOfCells,
            IEnumerable<CellType> cellTypes,
            Func<int, CellType, CellType, bool> isCompatible,
            Func<Position, CellType, CellType?[], int>? probabilityFunction = null,
            IEnumerable<(Position, CellType)>? initialConditions = null)
        {
            this.cellTypes = cellTypes.ToArray();
            this.numberOfAxes = numberOfAxes;
            this.initialConditions = initialConditions?.ToArray() ?? Array.Empty<(Position, CellType)>();
            this.probabilityFunction = probabilityFunction ?? new Func<Position, CellType, CellType?[], int>((a, b, c) => 1);

            cellTypeCompatibilities = CompileCellTypeCompatibilities(numberOfAxes, this.cellTypes, isCompatible);
            random = new Random(seed);
            cells = new CellType[numberOfCells];
            allowedCellTypes = Enumerable.Range(0, numberOfCells)
                .Select(i => Enumerable.Range(0, this.cellTypes.Length)
                    .Select(i => true).ToArray())
                .ToArray();
        }

        public IEnumerable<(Position, CellType)>? Collapse()
        {
            if (initialConditions is not null)
            {
                Console.WriteLine("Setting initial conditions...");
                using ConsoleProgressBar initBar = new(initialConditions.Count());
                foreach ((Position position, CellType? cellType) in initialConditions)
                {
                    int index = GetIndex(position);
                    if (cellType is null) throw new Exception();
                    if (!getAllowedCellTypes(index).Contains(cellType)) throw new Exception();
                    observe(index, cellType);
                    initBar.Update();
                }
            }

            Console.WriteLine("Collapsing the wave...");
            using ConsoleProgressBar waveBar = new(cells.Length);
            for (int i = 0; i < cells.Length; i++)
            {
                bool result = Update();
                waveBar.Update();
                if (!result) return null;
            }
            return Cells;
        }
        public bool Update()
        {
            if (!getUnobservedCells().Any()) return true;

            int index = getNextUnobservedNode();
            Position position = GetPosition(index);
            CellType[] allowedCellTypes = getAllowedCellTypes(index).ToArray();
            int[] allowedCellTypeDistribution = allowedCellTypes
                .Select(cellType => probabilityFunction(position, cellType, cells))
                .ToArray();
            if (allowedCellTypes.Length == 0) return false; // cannot solve

            CellType cellType = random.NextFrom(allowedCellTypes, allowedCellTypeDistribution);
            return observe(index, cellType);
        }

        public abstract Position GetPosition(int index);
        protected abstract int GetIndex(Position position);
        protected abstract Position GetAdjecantPosition(Position position, int axis, bool direction);
        protected abstract bool IsPositionInLimits(Position position, int axis);

        private IEnumerable<int> getUnobservedCells() => cells
                .Select((v, i) => (v, i))
                .Where(v => v.v is null)
                .Select(v => v.i);
        private int getNextUnobservedNode()
        {
            return getUnobservedCells()
                .OrderBy(index => allowedCellTypes[index].Where(v => v).Count())
                .First();
        }
        private bool observe(int index, CellType cellType)
        {
            cells[index] = cellType;
            for (int i = 0; i < cellTypes.Length; i++)
            {
                CellType? otherCellType = cellTypes[i];
                if (otherCellType is null || otherCellType.Equals(cellType)) continue;
                ban(index, i);
            }
            return true;
        }
        private void ban(int index, int bannedCellType)
        {
            if (allowedCellTypes[index][bannedCellType] == false) return; // already banned, don't propagate the wave
            allowedCellTypes[index][bannedCellType] = false;

            // check that all allowed options in neighbours are still supported now this one is banned
            Position position = GetPosition(index);
            for (int axis = 0; axis < numberOfAxes; axis++)
            {
                foreach (bool direction in new bool[] { true, false })
                {
                    Position adjecantPosition = GetAdjecantPosition(position, axis, direction);
                    int adjecantIndex = GetIndex(adjecantPosition);
                    if (!IsPositionInLimits(adjecantPosition, axis)) continue;

                    bool[] adjecantAllowedCellTypes = allowedCellTypes[adjecantIndex];
                    for (int adjecantCellType = 0; adjecantCellType < cellTypes.Length; adjecantCellType++)
                    {
                        if (adjecantAllowedCellTypes[adjecantCellType] != true) continue; // already not allowed so don't need to check
                        if (!isAllowed(index, adjecantCellType, axis, !direction)) ban(adjecantIndex, adjecantCellType);
                    }
                }
            }
        }
        private bool isAllowed(int index, int adjecantCellType, int axis, bool direction)
        {
            Func<int, int, bool> isCompat = direction 
                ? (a, b) => cellTypeCompatibilities[a][b][axis]
                : (a, b) => cellTypeCompatibilities[b][a][axis];
            return allowedCellTypes[index]
                .Select((v, cellType) => (v, cellType))
                .Where(v => v.v)
                .Select(v => v.cellType)
                .Any(cellType => isCompat(adjecantCellType, cellType));
        }
        private IEnumerable<CellType> getAllowedCellTypes(int index)
        {
            return allowedCellTypes[index]
                .Select((v, i) => (v, i))
                .Where(v => v.v)
                .Select(v => cellTypes[v.i]);
        }

        public static bool[][][] CompileCellTypeCompatibilities(int numberOfAxes, CellType[] cellTypes, Func<int, CellType, CellType, bool> isCompatible)
        {
            Console.WriteLine("Evalating cell compatibilities...");
            using ConsoleProgressBar bar = new(cellTypes.Length * cellTypes.Length * numberOfAxes);
            bool[][][] compatabilities = new bool[cellTypes.Length][][];
            for (int i = 0; i < cellTypes.Length; i++)
            {
                CellType cellTypeI = cellTypes[i];
                compatabilities[i] = new bool[cellTypes.Length][];
                for (int j = 0; j < cellTypes.Length; j++)
                {
                    CellType cellTypeJ = cellTypes[j];
                    compatabilities[i][j] = new bool[numberOfAxes];
                    for (int axis = 0; axis < numberOfAxes; axis++)
                    {
                        compatabilities[i][j][axis] = isCompatible(axis, cellTypeI, cellTypeJ);
                        bar.Update();
                    }
                }
            }
            return compatabilities;
        }
    }
}