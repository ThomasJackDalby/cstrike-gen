# idea: to build a procedural level generator for css using wave collapse
# doesn't currently cope with failures (i.e. no backtracking). If this becomes an issue, will implement

import random, json, os, math

EMPTY = "0"
CT_SPAWN = "1"
T_SPAWN = "2"

class MapGenerator:
    def __init__(self, map_file_path):
        self.map_file_path = map_file_path

        with open(map_file_path, "r") as map_file:
            self.map_data = json.load(map_file)  

        self.map_data["tiles_file_path"] = os.path.abspath(self.map_data["tiles_file_path"])

        with open(self.map_data["tiles_file_path"], "r") as tiles_file:
            tile_data = json.load(tiles_file)

        self.edge_id = tile_data["edge_id"] 
        self.tiles = tile_data["tiles"]
        self.map_width = self.map_data["map_width"]
        self.map_length = self.map_data["map_length"]

    def get_index(self, x, y):
        return x + y * self.map_width

    def get_xy(self, index):
        x = index % self.map_width
        y = (int)((index - x) / self.map_width) 
        return x, y

    def filter_allowed_tiles(self, nx, ny, edge_id, opposite_edge_id, allowed_tiles):
        if nx < 0 or nx >= self.map_width or ny < 0 or ny >= self.map_length:
            return [tile_id for tile_id in allowed_tiles if self.tiles[tile_id]["faces"][opposite_edge_id] == self.edge_id]

        adjecant_tile_index = self.get_index(nx, ny)
        adjecant_tile_id = self.map_tiles[adjecant_tile_index]

        if adjecant_tile_id == "-1":
            return allowed_tiles

        adjecant_tile = self.tiles[adjecant_tile_id]
        adjecant_edge = adjecant_tile["faces"][edge_id]
        return [tile_id for tile_id in allowed_tiles if self.tiles[tile_id]["faces"][opposite_edge_id] == adjecant_edge] 

    def check_tile_allowed(self, x, y, tile):
        allowed_tiles = [tile]
        allowed_tiles = self.filter_allowed_tiles(x-1,y, 0, 2, allowed_tiles)
        allowed_tiles = self.filter_allowed_tiles(x+1,y, 2, 0, allowed_tiles)
        allowed_tiles = self.filter_allowed_tiles(x,y-1, 1, 3, allowed_tiles)
        allowed_tiles = self.filter_allowed_tiles(x,y+1, 3, 1, allowed_tiles)
        return tile in allowed_tiles

    def generate(self, map_seed=None):
        if map_seed is None: map_seed = random.randint(0, 100000)
        print(f"Generating map from [{self.map_file_path}] using [{map_seed}]...")
        random.seed(map_seed)

        self.map_tiles = ["-1"] * self.map_width * self.map_length
        self.pre_generate_map()
        fixed_tiles = [False if tile == "-1" else True for tile in self.map_tiles]

        index = 0
        tried_tiles = [[] for i in range(len(self.map_tiles))]

        while index < len(self.map_tiles):
            if index < 0:
                return False

            if self.map_tiles[index] == "-1":
                cx, cy = self.get_xy(index)
                allowed_tiles = list(self.tiles.keys())
                allowed_tiles = [tile_id for tile_id in allowed_tiles if tile_id not in tried_tiles[index]]
                allowed_tiles = [tile_id for tile_id in allowed_tiles if "manual" not in self.tiles[tile_id] or self.tiles[tile_id]["manual"] == False]
                allowed_tiles = self.filter_allowed_tiles(cx-1,cy, 0, 2, allowed_tiles)
                allowed_tiles = self.filter_allowed_tiles(cx+1,cy, 2, 0, allowed_tiles)
                allowed_tiles = self.filter_allowed_tiles(cx,cy-1, 1, 3, allowed_tiles)
                allowed_tiles = self.filter_allowed_tiles(cx,cy+1, 3, 1, allowed_tiles)

                if len(allowed_tiles) <= 0:
                    tried_tiles[index] = []
                    index -= 1
                    tried_tiles[index].append(self.map_tiles[index])
                    if fixed_tiles[index]: index -= 1
                    else: self.map_tiles[index] = "-1"
                    continue

                chosen_tile = allowed_tiles[random.randint(0, len(allowed_tiles)-1)]
                self.map_tiles[index] = chosen_tile
            index += 1

        if "maps" not in self.map_data:
            self.map_data["maps"] = {}
        self.map_data["maps"][f"{map_seed}"] = self.map_tiles
        
        with open(self.map_file_path, "w") as map_file:
            json.dump(self.map_data, map_file)

        return True 

    def get_random_index(self, min_x, max_x, min_y, max_y):
        x = random.randint(min_x, max_x)
        y = random.randint(min_y, max_y)
        return self.get_index(x, y)

    def pre_generate_map(self):
        number_of_waypoints = 2

        t_spawn = self.get_random_index(1, self.map_width-2, 1, 1)
        ct_spawn = self.get_random_index(1, self.map_width-2, self.map_length-2, self.map_length-2)
        way_points = [self.get_random_index(2, self.map_width-3, 2, self.map_length-3) for i in range(number_of_waypoints)]
        points = [ct_spawn, t_spawn] + way_points

        def get_step_towards(a, b):
            ax, ay = self.get_xy(a)
            bx, by = self.get_xy(b)
            if ax == bx: ay += int(math.copysign(1, by - ay))
            elif ay == by: ax += int(math.copysign(1, bx - ax))
            else:
                if bool(random.randint(0, 1)): ay += int(math.copysign(1, by - ay))
                else: ax += int(math.copysign(1, bx - ax))
            return self.get_index(ax, ay)

        def connect_points(a, b):
            self.map_tiles[a] = EMPTY
            while a != b:
                a = get_step_towards(a, b)
                self.map_tiles[a] = EMPTY

        # connect_points(t_spawn, ct_spawn)
        # for i in range(len(points)):
        #     j = random.randint(0, len(points)-1)
        #     connect_points(points[i], points[j])

        self.map_tiles[t_spawn] = T_SPAWN
        self.map_tiles[ct_spawn] = CT_SPAWN

        self.display_map()

    def display_map(self):
        for y in range(self.map_length):
            for x in range(self.map_width):
                index = self.get_index(x, y)
                tile = self.map_tiles[index]
                if tile == EMPTY: print(' ', end="")
                elif tile == CT_SPAWN: print('C', end="")
                elif tile == T_SPAWN: print('T', end="")
                else: print('.', end="")
            print("")

if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument('file_path', help='map config filepath')
    parser.add_argument('-s', '--seed', type=int, help='map seed')
    args = parser.parse_args()

    if not os.path.exists(args.file_path):
        print(f"No map file found at [{args.file_path}]. Saving out a template.")
        map_data = {
            "map_width" : 6,
            "map_length" : 6,
            "tiles_file_path" : "./tiles/tiles.json",
        }
        with open(args.file_path, "w") as file:
            json.dump(map_data, file,indent=4, sort_keys=True )
    else:
        map_generator = MapGenerator(args.file_path)
        result = map_generator.generate(args.seed)    
        if not result:
            print("FAILED :(")