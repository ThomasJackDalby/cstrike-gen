# 

import json, os, itertools
from cstrike_gen import *

COLUMN_IDS = [1, 2, 3, 4]
WALL_IDS = [5, 6, 7, 8]
CORNER_IDS = [9, 10, 11, 12]
BEAM_IDS = [13, 14, 15, 16]
FLOOR_ID = 17
ROOF_ID = 18
DOOR_IDS = [22, 23, 24, 25]
ROOF_DOOR_ID = 26
SKY_ID = 19

class TileGenerator:
    def __init__(self, tiles_file_path):
        self.tiles_file_path = tiles_file_path
        self.tiles_data = {
            "edge_id" : 2,
            "component_files" : {
                0 : "C:/Users/Tom/source/repos/thomasjackdalby/cstrike_gen/components/walls_v2.vmf",
                1 : "C:/Users/Tom/source/repos/thomasjackdalby/cstrike_gen/components/t_spawn.vmf",
                2 : "C:/Users/Tom/source/repos/thomasjackdalby/cstrike_gen/components/ct_spawn.vmf",
                3 : "C:/Users/Tom/source/repos/thomasjackdalby/cstrike_gen/components/doors_v2.vmf",
                4 : "C:/Users/Tom/source/repos/thomasjackdalby/cstrike_gen/components/windows_v2.vmf"
            },
            "components" : {
                1 : { "name" : "column_a0", "component_file" : 0, "solids" : [11] },
                2 : { "name" : "column_a1", "component_file" : 0, "solids" : [7] },
                3 : { "name" : "column_a2", "component_file" : 0, "solids" : [9] },
                4 : { "name" : "column_a3", "component_file" : 0, "solids" : [10] },
                5 : { "name" : "wall_a0", "component_file" : 0, "solids" : [3] },
                6 : { "name" : "wall_a1", "component_file" : 0, "solids" : [8] },
                7 : { "name" : "wall_a2", "component_file" : 0, "solids" : [1] },
                8 : { "name" : "wall_a3", "component_file" : 0, "solids" : [13] },
                9 : { "name" : "corner_a0", "component_file" : 0, "solids" : [16] },
                10 : { "name" : "corner_a1", "component_file" : 0, "solids" : [15] },
                11 : { "name" : "corner_a2", "component_file" : 0, "solids" : [14] },
                12 : { "name" : "corner_a3", "component_file" : 0, "solids" : [17] },
                13 : { "name" : "beam_a0", "component_file" : 0, "solids" : [4] },
                14 : { "name" : "beam_a1", "component_file" : 0, "solids" : [5] },
                15 : { "name" : "beam_a2", "component_file" : 0, "solids" : [12] },
                16 : { "name" : "beam_a3", "component_file" : 0, "solids" : [6] },
                17 : { "name" : "ground", "component_file" : 0, "solids" : [0] },
                18 : { "name" : "roof", "component_file" : 0, "solids" : [2] },
                19 : { "name" : "sky", "component_file" : 0, "solids" : [18] },
                20 : { "name" : "t_spawn", "component_file" : 1, "solids" : [] },
                21 : { "name" : "ct_spawn", "component_file" : 2, "solids" : [] },
                22 : { "name" : "door_a0", "component_file" : 3, "solids" : [6, 7, 8], "entities" : [] },
                23 : { "name" : "door_a1", "component_file" : 3, "solids" : [9, 10, 11], "entities" : [] },
                24 : { "name" : "door_a2", "component_file" : 3, "solids" : [3, 4, 5], "entities" : [] },
                25 : { "name" : "door_a3", "component_file" : 3, "solids" : [0, 1, 2], "entities" : [] },
                26 : { "name" : "door_a5", "component_file" : 3, "solids" : [12, 13, 14, 15], "entities" : [0] },
                27 : { "name" : "window_a1", "component_file" : 4, "solids" : [9, 10, 11] },

            },
            "patterns" : {
                0 : "00000",
                1 : "11111",
                2 : "22221"
            },
            "tile_width" : 192,
            "tile_length" : 192,
            "tile_height" : 160,
            "tiles" : {
                0 : { "name" : "empty", "components" : [], "pattern_id" : 0 },
                1 : { "name" : "t_spawn", "manual" : True, "components" : [20], "pattern_id" : 2 },
                2 : { "name" : "ct_spawn", "manual" : True, "components" : [21], "pattern_id" : 2 }
            }
        }
        self.pattern_id = max(self.tiles_data["patterns"].keys())+1
        self.tile_id = max(self.tiles_data["tiles"].keys())+1

    def generate(self):

        inverse_patterns = { pattern : pattern_id for (pattern_id, pattern) in self.tiles_data["patterns"].items()}

        for tile_faces in itertools.product([FACE_TYPE_WALL, FACE_TYPE_DOOR, FACE_TYPE_INSIDE], repeat=5):
            components = []
            has_top = tile_faces[4] != FACE_TYPE_INSIDE
            if tile_faces[4] == FACE_TYPE_WALL:
                components.append(ROOF_ID)
            elif tile_faces[4] == FACE_TYPE_DOOR:
                components.append(ROOF_DOOR_ID)

            number_of_columns = 0
            for i in range(4):
                if tile_faces[i] == FACE_TYPE_WALL:
                    components.append(WALL_IDS[i])
                elif tile_faces[i] == FACE_TYPE_DOOR:
                    components.append(DOOR_IDS[i])

                if tile_faces[i] != FACE_TYPE_INSIDE or tile_faces[(i-1) % 4] != FACE_TYPE_INSIDE:
                    components.append(COLUMN_IDS[i])
                    number_of_columns += 1

                if tile_faces[i] != FACE_TYPE_INSIDE or has_top:
                    components.append(BEAM_IDS[i])

                if tile_faces[i] != FACE_TYPE_INSIDE or tile_faces[(i-1) % 4] != FACE_TYPE_INSIDE or has_top:
                    components.append(CORNER_IDS[i])
            
            pattern = "".join(map(str, tile_faces))
            if pattern not in inverse_patterns:
                inverse_patterns[pattern] = self.pattern_id
                self.tiles_data["patterns"][self.pattern_id] = pattern
                self.pattern_id += 1
            
            self.tiles_data["tiles"][self.tile_id] = {
                "components" : components,
                "pattern_id" : inverse_patterns[pattern]
            }
            self.tile_id += 1

        print(f"Generated {len(self.tiles_data['tiles'])} tiles.")
        with open(self.tiles_file_path, 'w') as tiles_file:
            json.dump(self.tiles_data, tiles_file)

        return True

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description='Generate a map.')
    parser.add_argument('file_path', help='map config filepath')
    args = parser.parse_args()

    tile_generator = TileGenerator(args.file_path)
    result = tile_generator.generate()
    if not result:
        print("Failed :(")