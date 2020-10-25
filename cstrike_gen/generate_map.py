import random, json, os
from cstrike_gen import *

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
        elif FACE_TYPE_EDGE not in adjecant_faces and FACE_TYPE_INSIDE not in adjecant_faces and random.random() > 0.5:
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
                    print(f"unknown case - adjecant_face: {adjecant_face}")
                    faces[face] = FACE_TYPE_WALL
                # if face == FACE_PZ and faces[face] == FACE_TYPE_INSIDE:
                #     faces[face] = FACE_TYPE_DOOR
            return "".join(map(str, faces))

    def layout(self, seed=None):
        # creates a map layout, which details from which tile can you pass between etc
        if seed is None: seed = random.randint(0, 100000)
        print(f"Laying out map with [{seed}]...")
        random.seed(seed)

        inverse_patterns = { pattern : int(pattern_id) for (pattern_id, pattern) in self.tiles_data["patterns"].items()}
        self.layout_tiles = [-1] * self.map_width * self.map_length * self.map_height

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
        print(f"Populating map with [{seed}]...")
        if str(seed) not in self.map_data["layouts"]:
            print("Must layout the seed first.")
            return False
        else:
            layout_tiles = self.map_data["layouts"][str(seed)]

        tile_ids = {}
        for tile_id in self.tiles_data["tiles"]:
            tile = self.tiles_data["tiles"][tile_id]
            pattern_id = tile["pattern_id"]
            if pattern_id not in tile_ids:
                tile_ids[pattern_id] = []
            tile_ids[pattern_id].append(tile_id)

        map_tiles = [-1] * len(layout_tiles)
        for index in range(len(layout_tiles)):
            pattern_id = layout_tiles[index]
            tile_id = random.choice(tile_ids[pattern_id])
            map_tiles[index] = tile_id

        self.map_data["maps"][seed] = map_tiles
        return True

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
        result = map_generator.layout(seed)
        if not result: print("FAILED :(")

        result = map_generator.populate(seed)
        if not result: print("FAILED :(")

        map_generator.save()