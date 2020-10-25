import random, json, os

FACE_TYPE_SOLID = 0
FACE_TYPE_OPEN = 1
FACE_TYPE_DOOR = 2
FACE_TYPES = [FACE_TYPE_SOLID, FACE_TYPE_OPEN, FACE_TYPE_DOOR]

FACE_NX = 0
FACE_PX = 1
FACE_NY = 2
FACE_PY = 3
FACE_NZ = 4
FACE_PZ = 5
FACES = [FACE_NX, FACE_PX, FACE_NY, FACE_PY, FACE_NZ, FACE_PZ]

class MapGenerator:
    def __init__(self, map_file_path):
        self.map_file_path = map_file_path
        with open(map_file_path, "r") as map_file:
            self.map_data = json.load(map_file)  

        self.map_width = self.map_data["map_width"]
        self.map_length = self.map_data["map_length"]
        self.map_height = self.map_data["map_height"]
        self.map_tiles = [-1] * self.map_width * self.map_length * self.map_height

    def get_index(self, x, y, z):
        return x + y * self.map_width + z * self.map_width * self.map_length

    def get_xyz(self, index):
        x = int(index % self.map_width)
        y = int((index / self.map_width) % self.map_length)
        z = int(index / (self.map_width * self.map_length))
        return x, y, z

    def get_face(self, nx, ny, nz, edge_id):
        if nx < 0 or nx >= self.map_width or ny < 0 or ny >= self.map_length or nz < 0 or nz >= self.map_height:
            return FACE_TYPE_SOLID

        adjecant_tile_index = self.get_index(nx, ny, nz)
        adjecant_tile_id = self.map_tiles[adjecant_tile_index]

        if adjecant_tile_id == -1:
            return random.choice(FACE_TYPES)            
        else:
            adjecant_tile = self.id_to_tile_map[adjecant_tile_id]
            adjecant_edge = adjecant_tile[edge_id]
            if adjecant_edge == FACE_TYPE_SOLID or adjecant_edge == FACE_TYPE_DOOR:
                return random.choice([adjecant_edge, FACE_TYPE_OPEN])
            elif adjecant_edge == FACE_TYPE_OPEN:
                return random.choice(FACE_TYPES)

    def generate(self, map_seed=None):
        if map_seed is None: map_seed = random.randint(0, 100000)
        print(f"Generating map from using [{map_seed}]...")
        random.seed(map_seed)

        self.tile_to_id_map = {}
        self.id_to_tile_map = {}
        self.map_tiles = [-1] * self.map_width * self.map_length * self.map_height

        index = 0
        tile_id = 0
        while index < len(self.map_tiles):
            if index < 0:
                return False

            if self.map_tiles[index] == -1:
                cx, cy, cz = self.get_xyz(index)
                face_nx = self.get_face(cx-1, cy, cz, FACE_NX)
                face_px = self.get_face(cx+1, cy, cz, FACE_PX)
                face_ny = self.get_face(cx, cy-1, cz, FACE_NY)
                face_py = self.get_face(cx, cy+1, cz, FACE_PY)
                face_nz = self.get_face(cx, cy, cz-1, FACE_NZ)
                face_pz = self.get_face(cx, cy, cz+1, FACE_PZ)
                tile = (face_nx, face_px, face_ny, face_py, face_nz, face_pz)
                
                if tile not in self.tile_to_id_map:
                    self.tile_to_id_map[tile] = tile_id
                    self.id_to_tile_map[tile_id] = tile
                    tile_id += 1
                self.map_tiles[index] = self.tile_to_id_map[tile]
            index += 1

        if "maps" not in self.map_data:
            self.map_data["maps"] = {}
        self.map_data["maps"][f"{map_seed}"] = self.map_tiles
        
        with open(self.map_file_path, "w") as map_file:
            json.dump(self.map_data, map_file)

        return True 


if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description='Generate a map.')
    parser.add_argument('file_path', help='map config filepath')
    parser.add_argument('-s', '--seed', type=int, help='map seed')
    args = parser.parse_args()

    if not os.path.exists(args.file_path):
        print(f"No map file found at [{args.file_path}]. Saving out a template.")
        map_data = {
            "map_width" : 4,
            "map_length" : 4,
            "map_height" : 2,
        }
        with open(args.file_path, "w") as file:
            json.dump(map_data, file,indent=4, sort_keys=True )
    else:
        map_generator = MapGenerator(args.file_path)
        result = map_generator.generate(args.seed)    
        if not result:
            print("FAILED :(")