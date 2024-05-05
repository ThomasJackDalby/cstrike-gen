//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CSMP.Model
//{
//    internal class MapBuilder
//    {



//        public void SetXColumn(int y, int z, Tile3D tile)
//        {
//            for (int x = 0; x < Size.X; x++) SetTile(new Vector3D(x, y, z), tile);
//        }
//        public void SetYColumn(int x, int z, Tile3D tile)
//        {
//            for (int y = 0; y < Size.Y; y++) SetTile(new Vector3D(x, y, z), tile);
//        }
//        public void SetZColumn(int x, int y, Tile3D tile)
//        {
//            for (int z = 0; z < Size.Z; z++) SetTile(new Vector3D(x, y, z), tile);
//        }
//        public void SetXPlane(int x, Tile3D tile)
//        {
//            foreach ((int y, int z) in Enumerable2D.Range(0, Size.Y, 0, Size.Z)) SetTile(new Vector3D(x, y, z), tile);
//        }
//        public void SetYPlane(int y, Tile3D tile)
//        {
//            foreach ((int x, int z) in Enumerable2D.Range(0, Size.X, 0, Size.Z)) SetTile(new Vector3D(x, y, z), tile);
//        }
//        public void SetZPlane(int z, Tile3D tile)
//        {
//            foreach ((int x, int y) in Enumerable2D.Range(0, Size.X, 0, Size.Y)) SetTile(new Vector3D(x, y, z), tile);
//        }
//        public void GenerateSkyBox()
//        {
//            //Tile3D empty = tiles["empty"];
//            //Tile3D solid = tiles["solid"];
//            //Tile3D skybox_floor = tiles["empty-f"].Clone("grass-floor").SetMaterial(sky);
//            //Tile3D skybox_roof = tiles["empty-r"].Clone("skybox-roof").SetMaterial(sky);

//            //Tile3D skybox_wall = tiles["wall"].Clone("skybox-wall").SetMaterial(sky);
//            //Tile3D skybox_wall_90 = skybox_wall.Rotate90("skybox-wall-90");
//            //Tile3D skybox_wall_180 = skybox_wall_90.Rotate90("skybox-wall-180");
//            //Tile3D skybox_wall_270 = skybox_wall_180.Rotate90("skybox-wall-270");

//            //map.SetXPlane(0, skybox_wall);
//            //map.SetXPlane(map.Size.X - 1, skybox_wall_180);
//            //map.SetYPlane(0, skybox_wall_90);
//            //map.SetYPlane(map.Size.Y - 1, skybox_wall_270);
//            //map.SetZPlane(0, skybox_floor);
//            //map.SetZPlane(map.Size.Z - 1, skybox_roof);

//            //Tile3D skybox_wall_roof = tiles["wall-r"].Clone("skybox-wall-r").SetMaterial(sky);
//            //Tile3D skybox_wall_90_roof = skybox_wall_roof.Rotate90("skybox-wall-r-90");
//            //Tile3D skybox_wall_180_roof = skybox_wall_90_roof.Rotate90("skybox-wall-r-180");
//            //Tile3D skybox_wall_270_roof = skybox_wall_180_roof.Rotate90("skybox-wall-r-270");
//            //map.SetXColumn(0, map.Size.Z - 1, skybox_wall_90_roof);
//            //map.SetXColumn(map.Size.Y - 1, map.Size.Z - 1, skybox_wall_270_roof);
//            //map.SetYColumn(0, map.Size.Z - 1, skybox_wall_roof);
//            //map.SetYColumn(map.Size.X - 1, map.Size.Z - 1, skybox_wall_180_roof);

//            //Tile3D skybox_wall_floor = tiles["wall-f"].Clone("skybox-wall-f").SetMaterial(sky);
//            //Tile3D skybox_wall_90_floor = skybox_wall_floor.Rotate90("skybox-wall-f-90");
//            //Tile3D skybox_wall_180_floor = skybox_wall_90_floor.Rotate90("skybox-wall-f-180");
//            //Tile3D skybox_wall_270_floor = skybox_wall_180_floor.Rotate90("skybox-wall-f-270");
//            //map.SetXColumn(0, 0, skybox_wall_90_floor);
//            //map.SetXColumn(map.Size.Y - 1, 0, skybox_wall_270_floor);
//            //map.SetYColumn(0, 0, skybox_wall_floor);
//            //map.SetYColumn(map.Size.X - 1, 0, skybox_wall_180_floor);

//            //Tile3D skybox_corner = tiles["corner"].Clone("skybox-corner").SetMaterial(sky);
//            //Tile3D skybox_corner_90 = skybox_corner.Rotate90("skybox-corner-90");
//            //Tile3D skybox_corner_180 = skybox_corner_90.Rotate90("skybox-corner-180");
//            //Tile3D skybox_corner_270 = skybox_corner_180.Rotate90("skybox-corner-270");
//            //map.SetZColumn(0, 0, skybox_corner_90);
//            //map.SetZColumn(0, map.Size.Y - 1, skybox_corner);
//            //map.SetZColumn(map.Size.X - 1, map.Size.Y - 1, skybox_corner_270);
//            //map.SetZColumn(map.Size.X - 1, 0, skybox_corner_180);

//            //Tile3D skybox_corner_roof = tiles["corner-r"].Clone("skybox-corner-r").SetMaterial(sky);
//            //Tile3D skybox_corner_roof_90 = skybox_corner_roof.Rotate90("skybox-corner-r-90");
//            //Tile3D skybox_corner_roof_180 = skybox_corner_roof_90.Rotate90("skybox-corner-r-180");
//            //Tile3D skybox_corner_roof_270 = skybox_corner_roof_180.Rotate90("skybox-corner-r-270");
//            //map.SetTile(new Vector3D(0, 0, map.Size.Z-1), skybox_corner_roof);
//            //map.SetTile(new Vector3D(0, map.Size.Y - 1, map.Size.Z-1), skybox_corner_roof_270);
//            //map.SetTile(new Vector3D(map.Size.X - 1, map.Size.Y - 1, map.Size.Z-1), skybox_corner_roof_180);
//            //map.SetTile(new Vector3D(map.Size.X - 1, 0, map.Size.Z-1), skybox_corner_roof_90);

//            //Tile3D skybox_corner_floor = tiles["corner-f"].Clone("skybox-corner-f").SetMaterial(sky);
//            //Tile3D skybox_corner_floor_90 = skybox_corner_floor.Rotate90("skybox-corner-f-90");
//            //Tile3D skybox_corner_floor_180 = skybox_corner_floor_90.Rotate90("skybox-corner-f-180");
//            //Tile3D skybox_corner_floor_270 = skybox_corner_floor_180.Rotate90("skybox-corner-f-270");
//            //map.SetTile(new Vector3D(0, 0, 0), skybox_corner_floor_90);
//            //map.SetTile(new Vector3D(0, map.Size.Y - 1, 0), skybox_corner_floor);
//            //map.SetTile(new Vector3D(map.Size.X - 1, map.Size.Y - 1, 0), skybox_corner_floor_270);
//            //map.SetTile(new Vector3D(map.Size.X - 1, 0, 0), skybox_corner_floor_180);

//        }


//    }
//}
