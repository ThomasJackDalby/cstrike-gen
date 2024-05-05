import random, json, os, math
from cstrikegen.constants import *

class MapGenerator:
    def __init__(self, map_file_path):
        self.map_file_path = map_file_path
        with open(map_file_path, "r") as map_file:
            self.map_data = json.load(map_file)  

        if "layouts" not in self.map_data: self.map_data["layouts"] = {}
        if "maps" not in self.map_data: self.map_data["maps"] = {}

        with open(self.map_data["tiles_file_path"], "r") as tiles_file:
            print(f"Loading tiles data from [{self.map_data['tiles_file_path']}]")
            self.tiles_data = json.load(tiles_file)

        self.empty_id = next((tile_id for tile_id in self.tiles_data['tiles'] if self.tiles_data['tiles'][tile_id]["name"] == "empty"), None)
        self.t_spawn_id = next((tile_id for tile_id in self.tiles_data['tiles'] if self.tiles_data['tiles'][tile_id]["name"] == "t_spawn"), None)
        self.ct_spawn_id = next((tile_id for tile_id in self.tiles_data['tiles'] if self.tiles_data['tiles'][tile_id]["name"] == "ct_spawn"), None)
        self.map_width = self.map_data["map_width"]
        self.map_length = self.map_data["map_length"]
        self.map_height = self.map_data["map_height"]
        self.outside_probability = self.map_data["outside_probability"]
        self.layout_tiles = [-1] * self.map_width * self.map_length * self.map_height
        self.map_tiles = [-1] * self.map_width * self.map_length * self.map_height

    def get_index(self, x, y, z):
        return x + y * self.map_width + z * self.map_width * self.map_length

    def get_xyz(self, index):
        x = int(index % self.map_width)
        y = int((index / self.map_width) % self.map_length)
        z = int(index / (self.map_width * self.map_length))
        return x, y, z

    def get_adjecant_face(self, nx, ny, nz, edge_id):
        if nx < 0 or nx >= self.map_width or ny < 0 or ny >= self.map_length:
            return FACE_TYPE_EDGE
        elif nz < 0:
            return FACE_TYPE_GROUND
        elif nz >= self.map_height:
            return FACE_TYPE_SKY

        adjecant_tile_index = self.get_index(nx, ny, nz)
        adjecant_tile_pattern_id = self.layout_tiles[adjecant_tile_index]
        if adjecant_tile_pattern_id == -1:
            return FACE_TYPE_EMPTY
        adjecant_tile_pattern = self.tiles_data["patterns"][str(adjecant_tile_pattern_id)]
        adjecant_face = int(adjecant_tile_pattern[edge_id])
        return adjecant_face

    def get_adjecant_faces(self, cx, cy, cz):
        face_px = self.get_adjecant_face(cx+1, cy, cz, FACE_NX)
        face_py = self.get_adjecant_face(cx, cy+1, cz, FACE_NY)
        face_nx = self.get_adjecant_face(cx-1, cy, cz, FACE_PX)
        face_ny = self.get_adjecant_face(cx, cy-1, cz, FACE_PY)
        face_pz = self.get_adjecant_face(cx, cy, cz+1, FACE_NZ)
        face_nz = self.get_adjecant_face(cx, cy, cz-1, FACE_PZ)
        faces = [face_px, face_py, face_nx, face_ny, face_pz, face_nz]
        return faces

    def get_pattern(self, index):
        cx, cy, cz = self.get_xyz(index)

        adjecant_faces = self.get_adjecant_faces(cx, cy, cz)
        if adjecant_faces[FACE_NZ] == FACE_TYPE_OUTSIDE:
            return PATTERN_OUTSIDE
        elif FACE_TYPE_EDGE not in adjecant_faces and FACE_TYPE_INSIDE not in adjecant_faces and random.random() > self.outside_probability:
            return PATTERN_OUTSIDE
        else:
            faces = [-1] * 5
            for face in FACES:
                adjecant_face = adjecant_faces[face]
                if adjecant_face == FACE_TYPE_EMPTY:
                    if face == FACE_PZ:
                        faces[face] = random.choice([FACE_TYPE_DOOR] * 1 + [FACE_TYPE_WALL])
                    else:    
                        faces[face] = random.choice([FACE_TYPE_DOOR, FACE_TYPE_WALL])
                elif adjecant_face in [FACE_TYPE_INSIDE, FACE_TYPE_DOOR, FACE_TYPE_WALL]:
                    faces[face] = adjecant_face
                elif adjecant_face == FACE_TYPE_EDGE or adjecant_face == FACE_TYPE_SKY:
                    faces[face] = FACE_TYPE_WALL
                elif adjecant_face == FACE_TYPE_OUTSIDE:
                    faces[face] = random.choice([FACE_TYPE_DOOR, FACE_TYPE_WALL])
                else:
                    print(f"Warning: Unknown adjecant face.")
                    faces[face] = FACE_TYPE_WALL
                    
            return "".join(map(str, faces))

    def generate(self, seed=None):
        if seed is None: seed = random.randint(0, 100000)
        print(f"Laying out map with [{seed}]...")
        random.seed(seed)

        self.pre_populate_map()
        self.layout(seed)
        self.populate(seed)
        self.save()

    def get_random_index(self, min_x, max_x, min_y, max_y, ignore=[]):
        while True:
            x = random.randint(min_x, max_x)
            y = random.randint(min_y, max_y)
            index = self.get_index(x, y, 0)
            if index not in ignore:
                return index

    def get_random_indexes(self, min_x, max_x, min_y, max_y, number_of_indexes, ignore=[]):
        indexes = []
        for _ in range(number_of_indexes):
            indexes.append(self.get_random_index(min_x, max_x, min_y, max_y, indexes + ignore))
        return indexes

    def pre_populate_map(self):
        number_of_waypoints = 2
        number_of_spawns = 1

        t_spawn_indexes = self.get_random_indexes(1, self.map_width-2, 1, 1, number_of_spawns)
        ct_spawn_indexes = self.get_random_indexes(1, self.map_width-2, self.map_length-2, self.map_length-2, number_of_spawns)
        # way_points = self.get_random_indexes(2, self.map_width-3, 2, self.map_length-3, number_of_waypoints)
        points = t_spawn_indexes + ct_spawn_indexes # + way_points

        def get_step_towards(a, b):
            ax, ay, _ = self.get_xyz(a)
            bx, by, _ = self.get_xyz(b)
            if ax == bx: ay += int(math.copysign(1, by - ay))
            elif ay == by: ax += int(math.copysign(1, bx - ax))
            else:
                if bool(random.randint(0, 1)): ay += int(math.copysign(1, by - ay))
                else: ax += int(math.copysign(1, bx - ax))
            return self.get_index(ax, ay, 0)

        def set_tile(index, tile_id):
            self.layout_tiles[index] = self.tiles_data["tiles"][tile_id]["pattern_id"]
            self.map_tiles[index] = tile_id

        def connect_points(a, b):
            set_tile(a, self.empty_id)
            while a != b:
                a = get_step_towards(a, b)
                set_tile(a, self.empty_id)

        # for i in range(len(points)):
        #     j = random.randint(0, len(points)-1)
        #     connect_points(points[i], points[j])

        for index in t_spawn_indexes:
            set_tile(index, self.t_spawn_id)
        for index in ct_spawn_indexes:
            set_tile(index, self.ct_spawn_id)

    def layout(self, seed):
        inverse_patterns = { pattern : int(pattern_id) for (pattern_id, pattern) in self.tiles_data["patterns"].items()}

        index = 0
        while index < len(self.layout_tiles):
            if self.layout_tiles[index] == -1:
                pattern = self.get_pattern(index)
                if pattern not in inverse_patterns:
                    print(f"No tiles with pattern {pattern}")
                    return False
                self.layout_tiles[index] = inverse_patterns[pattern]
            index += 1
        self.map_data["layouts"][str(seed)] = self.layout_tiles
        return True 

    def populate(self, seed):
        if str(seed) not in self.map_data["layouts"]:
            print("Must layout the seed first.")
            return False
        else:
            layout_tiles = self.map_data["layouts"][str(seed)]

        tile_ids = {}
        for tile_id in self.tiles_data["tiles"]:
            tile = self.tiles_data["tiles"][tile_id]
            if "manual" in tile and tile["manual"]:
                continue
            pattern_id = tile["pattern_id"]
            if pattern_id not in tile_ids:
                tile_ids[pattern_id] = []
            tile_ids[pattern_id].append(tile_id)
        
        for index in range(len(layout_tiles)):
            if self.map_tiles[index] == -1:
                pattern_id = layout_tiles[index]
                tile_id = random.choice(tile_ids[pattern_id])
                self.map_tiles[index] = tile_id

        self.map_data["maps"][seed] = self.map_tiles
        return True

    def display_map(self):
        for y in range(self.map_length):
            for x in range(self.map_width):
                index = self.get_index(x, y, 0)
                tile = self.map_tiles[index]
                if tile == self.empty_id: print(' ', end="")
                elif tile == self.ct_spawn_id: print('C', end="")
                elif tile == self.t_spawn_id: print('T', end="")
                else: print('.', end="")
            print("")

    def save(self):
        with open(self.map_file_path, "w") as map_file:
            json.dump(self.map_data, map_file)

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description='Generate tiles for a map.')
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
        file_path = args.file_path   
        seed = args.seed   

        map_generator = MapGenerator(file_path)
        map_generator.generate(seed)
        