# idea: to build a procedural level generator for css using wave collapse
# doesn't currently cope with failures (i.e. no backtracking). If this becomes an issue, will implement

import random, json, os

def generate(map_file_path):
    print(f"Generating map using [{map_file_path}]...")

    with open(map_file_path, "r") as map_file:
        map_data = json.load(map_file)  

    map_data["tiles_file_path"] = os.path.abspath(map_data["tiles_file_path"])

    with open(map_data["tiles_file_path"], "r") as tiles_file:
        tile_data = json.load(tiles_file)
    tiles = tile_data["tiles"]

    map_width = map_data["map_width"]
    map_length = map_data["map_length"]

    for map_seed in map_data["map_seeds"]:
        print(f"Using seed [{map_seed}]")
        random.seed(map_seed)

        map_tiles = [-1] * map_width * map_length
        map_tiles_len = len(map_tiles) 
        tried_tiles = [[] for i in range(map_tiles_len)]
        
        def get_index(x, y):
            return x + y * map_width

        def get_xy(index):
            x = index % map_width
            y = (int)((index - x) / map_width) 
            return x, y

        def get_tile_id(name):
            for tile_id in tiles:
                tile = tiles[tile_id]
                if tile["name"] == name:
                    return tile_id
            return None

        # spawn the required tiles first
        manual_tiles = [
            get_tile_id("ct_spawn"),
            get_tile_id("t_spawn")
        ]

        def filter_allowed_tiles(nx, ny, edge_id, opposite_edge_id, allowed_tiles):
            if nx < 0 or nx >= map_width or ny < 0 or ny >= map_length:
                return [tile_id for tile_id in allowed_tiles if tiles[tile_id]["faces"][opposite_edge_id] == 1]

            adjecant_tile_index = get_index(nx, ny)
            adjecant_tile_id = map_tiles[adjecant_tile_index]
            if adjecant_tile_id == -1:
                return allowed_tiles

            adjecant_tile = tiles[adjecant_tile_id]
            adjecant_edge = adjecant_tile["faces"][edge_id]
            return [tile_id for tile_id in allowed_tiles if tiles[tile_id]["faces"][opposite_edge_id] == adjecant_edge] 

        def check_tile_allowed(x, y, tile):
            allowed_tiles = [tile]
            allowed_tiles = filter_allowed_tiles(cx-1,cy, 0, 2, allowed_tiles)
            allowed_tiles = filter_allowed_tiles(cx+1,cy, 2, 0, allowed_tiles)
            allowed_tiles = filter_allowed_tiles(cx,cy-1, 1, 3, allowed_tiles)
            allowed_tiles = filter_allowed_tiles(cx,cy+1, 3, 1, allowed_tiles)
            return tile in allowed_tiles

        for tile in manual_tiles:
            while True:
                index = random.randint(0, map_width * map_length)
                cx, cy = get_xy(index)
                if not check_tile_allowed(cx, cy, tile):
                    continue
                map_tiles[index] = tile
                break

        # wave collapse itself just walks along the map trying to place tiles, and backtracking if it can't
        # in terms of available tiles, long term plan is to have a folder of the rotated tiles, where the names correspond to the edge signatures
        # when checking tiles, in theory only need to check below and to the left, unless at an edge, although this not true for pre-placed tiles. Algorithm should cope with pre-placed tiles
        index = 0
        has_failed = False
        while index < map_tiles_len and not has_failed:
            if index < 0:
                print("Unable to find a viable map layout :(")
                has_failed = True
                break

            if map_tiles[index] == -1:
                cx, cy = get_xy(index)
                allowed_tiles = list(tiles.keys())
                allowed_tiles = [tile_id for tile_id in allowed_tiles if tile_id not in tried_tiles[index]]
                allowed_tiles = [tile_id for tile_id in allowed_tiles if "manual" not in tiles[tile_id] or tiles[tile_id]["manual"] == False]
                allowed_tiles = filter_allowed_tiles(cx-1,cy, 0, 2, allowed_tiles)
                allowed_tiles = filter_allowed_tiles(cx+1,cy, 2, 0, allowed_tiles)
                allowed_tiles = filter_allowed_tiles(cx,cy-1, 1, 3, allowed_tiles)
                allowed_tiles = filter_allowed_tiles(cx,cy+1, 3, 1, allowed_tiles)

                if len(allowed_tiles) <= 0:
                    index -= 1
                    tried_tiles[index].append(map_tiles[index])
                    map_tiles[index] = -1
                    continue

                chosen_tile = allowed_tiles[random.randint(0, len(allowed_tiles)-1)]
                map_tiles[index] = chosen_tile
            index += 1

        if has_failed:
            continue
        else:
            if "maps" not in map_data:
                map_data["maps"] = {}
            map_data["maps"][f"{map_seed}"] = map_tiles
    
    with open(map_file_path, "w") as map_file:
        json.dump(map_data, map_file)

if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument('file_path', help='map config filepath')
    args = parser.parse_args()

    if not os.path.exists(args.file_path):
        print(f"No map file found at [{args.file_path}]. Saving out a template.")
        map_data = {
            "map_seed" : 12345,
            "map_width" : 6,
            "map_length" : 6,
            "tiles_file_path" : "./tiles/tiles.json",
        }
        with open(args.file_path, "w") as file:
            json.dump(map_data, file,indent=4, sort_keys=True )
    else:
        generate(args.file_path)