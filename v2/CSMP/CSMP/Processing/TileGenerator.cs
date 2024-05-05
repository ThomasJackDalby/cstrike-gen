using CSMP.Model;
using CSMP.Tools;

namespace CSMP.Processing
{
    internal class TileGenerator
    {
        public Material Solid { get; } = new Material("solid");
        public Material Frame { get; } = new Material("frame");
        public Material Stairs { get; } = new Material("stairs");

        private const int size = 16;

        enum WallType
        {
            Solid,
            Window,
            Door
        }

        public IEnumerable<Tile3D> GenerateBaseTiles()
        {
            yield return new Tile3D("empty", size);

            // single walls
            string tags = "swd";

            for (WallType i = 0; (int)i < 3; i++)
            {
                Tile3DBuilder builder = new Tile3DBuilder(size);
                if (i == WallType.Solid) builder.SetPlane(Axis.X, 0, Solid);
                else if (i == WallType.Door) builder.AddDoor(Axis.X, 0, Solid, Frame);
                else if (i == WallType.Window) builder.AddWindow(Axis.X, 0, Solid, Frame);
                yield return builder.Build($"wall-{tags[(int)i]}");
            }

            for (WallType i = 0; (int)i < 3; i++)
            {
                for (WallType j = 0; (int)j < 3; j++)
                {
                    Tile3DBuilder builder = new Tile3DBuilder(size);
                    if (i == WallType.Solid) builder.SetPlane(Axis.X, 0, Solid);
                    else if (i == WallType.Door) builder.AddDoor(Axis.X, 0, Solid, Frame);
                    else if (i == WallType.Window) builder.AddWindow(Axis.X, 0, Solid, Frame);

                    if (j == WallType.Solid) builder.SetPlane(Axis.Y, 0, Solid);
                    else if (j == WallType.Door) builder.AddDoor(Axis.Y, 0, Solid, Frame);
                    else if (j == WallType.Window) builder.AddWindow(Axis.Y, 0, Solid, Frame);

                    yield return builder.Build($"corner-{tags[(int)i]}{tags[(int)j]}");
                }
            }

            int margin = 4;
            var b = new Tile3DBuilder(size)
                .AddTags("stairs");
            for (int z = 1; z < 16; z++) b.SetPlane(Axis.Z, z, z, size - 1, margin, size - margin, Stairs);
            b.SetPlane(Axis.X, size-1, Solid);
            yield return b.Build("stairs");

            yield return new Tile3DBuilder(size)
                .AddTags("external")
                .SetColumn(Axis.Z, 0, 0, Solid)
                .Build("point-external");

            yield return new Tile3DBuilder(size)
                .AddTags("external")
                .SetColumn(Axis.X, 0, 0, Solid)
                .Build("wall-external");

            yield return new Tile3DBuilder(size)
                .AddTags("external")
                .SetColumn(Axis.X, 0, 0, Solid)
                .SetColumn(Axis.Y, 0, 0, Solid)
                .Build("corner-external");
        }

        public IEnumerable<Tile3D> Generate()
        {
            return GenerateBaseTiles()
                .SelectMany(tile => AddFloorAndRoof(tile))
                .SelectMany(tile => AddRotations(tile))
                .DistinctBy((a, b) => a.IsEquivalent(b));
        }

        public IEnumerable<Tile3D> AddFloorAndRoof(Tile3D tile)
        {
            bool noRoof = tile.Tags.Contains("stairs");
            bool mustFloor = tile.Tags.Contains("door") || tile.Tags.Contains("stairs");
            if (!mustFloor) yield return tile;
            Tile3DBuilder builder = new(tile);
            yield return builder
                .SetPlane(Axis.Z, 0, Solid)
                .AddTags("floor")
                .Build($"{tile.ID}-f");
            if (!noRoof) yield return builder
                .SetPlane(Axis.Z, tile.Size - 1, Solid)
                .AddTags("roof")
                .Build($"{tile.ID}-f-r");
            if (!noRoof && !mustFloor) yield return new Tile3DBuilder(tile)
                .SetPlane(Axis.Z, tile.Size - 1, Solid)
                .AddTags("roof")
                .Build($"{tile.ID}-r");
        }

        public IEnumerable<Tile3D> AddRotations(Tile3D tile)
        {
            yield return tile;

            Tile3D? tile90 = tile.Rotate90($"{tile.ID}-90");
            Tile3D? tile180 = tile90.Rotate90($"{tile.ID}-180");
            Tile3D? tile270 = tile180.Rotate90($"{tile.ID}-270");
            yield return tile90;
            yield return tile180;
            yield return tile270;

        }
    }
    public enum Axis { X, Y, Z }

    public static class Tile3DBuilderExtensions
    {
        public static Tile3DBuilder AddDoor(this Tile3DBuilder builder, Axis axis, int i, Material wall, Material? frame = null, Material? door = null)
        {
            int doorMargin = 5;
            int frameMargin = doorMargin - 1;
            builder.AddTags("door").SetPlane(axis, i, wall);
            if (frame is not null) builder.SetPlane(axis, i, frameMargin, builder.Size - frameMargin, 0, builder.Size - frameMargin, frame);
            builder.SetPlane(axis, i, doorMargin, builder.Size - doorMargin, 0, builder.Size - doorMargin, door);
            return builder;
        }
        public static Tile3DBuilder AddWindow(this Tile3DBuilder builder, Axis axis, int i, Material wall, Material? frame = null, Material? window = null)
        {
            int windowSideMargin = 4;
            int windowTopMargin = 5;
            int windowBottomMargin = 6;

            int frameSideMargin = windowSideMargin - 1;
            int frameTopMargin = windowTopMargin - 1;
            int frameBottomMargin = windowBottomMargin - 1;

            builder.AddTags("door").SetPlane(axis, i, wall);
            if (frame is not null) builder.SetPlane(axis, i, frameSideMargin, builder.Size - frameSideMargin, frameBottomMargin, builder.Size - frameTopMargin, frame);
            builder.SetPlane(axis, i, windowSideMargin, builder.Size - windowSideMargin, windowBottomMargin, builder.Size - windowTopMargin, window);
            return builder;
        }
    }
}
