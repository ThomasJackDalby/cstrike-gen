import json, copy, os
import vmf_parser, vmf_formatter

def process_map(map_file_path, map_seed=None):
    global map_data, tiles_data, cached_tile_vmfs

    map_file_name = os.path.basename(map_file_path)
    map_name = os.path.splitext(map_file_name)[0]
    print(f"Processing [{map_name}]...")

    map_data = {}
    tiles_data = {}
    cached_tile_vmfs = {}

    with open(map_file_path, "r") as map_file:
        map_data = json.load(map_file)

    with open(map_data["tiles_file_path"], "r") as tiles_file:
        tiles_data = json.load(tiles_file)

    map_width = map_data["map_width"]
    map_length = map_data["map_length"]

    def get_index(x, y):
        return x + y * map_width

    map_seeds = []
    if map_seed is None:
        map_seeds = map_data["maps"].keys()
    else:
        map_seeds.append(map_seed)

    for map_seed in map_seeds:
        print(f"> [{map_seed}]...")
        map_tiles = map_data["maps"][map_seed]

        map_vmf = { 
            "world": [{
                "solid" : []
            }],
            "entity": []
        }

        for y in range(map_length):
            for x in range(map_width):
                index = get_index(x, y)
                tile_id = map_tiles[index]
                tile_vmf = get_tile_vmf(tile_id)
                tile_vmf = translate(tile_vmf, x * 256, y * 256)
                merge_vmf(map_vmf, tile_vmf)

        solid_id = 0
        side_id = 0
        entity_id = 0
        for solid in map_vmf["world"][0]["solid"]:
            solid["id"] = solid_id
            solid_id += 1
            for side in solid["side"]:
                side["id"] = side_id
                side_id += 1
        for entity in map_vmf["entity"]:
            entity["id"] = entity_id
            entity_id += 1
            if "solid" in entity:
                for solid in entity["solid"]:
                    solid["id"] = solid_id
                    solid_id += 1
                    for side in solid["side"]:
                        side["id"] = side_id
                        side_id += 1

        formatter = vmf_formatter.VmfFormatter()
        formatter.format(f"{map_name}_{map_seed}.vmf", map_vmf)

def translate(tile_vmf, x, y):
    tile_vmf = copy.deepcopy(tile_vmf)

    if "solid" in tile_vmf["world"][0]:
        for solid in tile_vmf["world"][0]["solid"]:
            translate_solid(solid, x, y)
    
    if "entity" in tile_vmf:
        for entity in tile_vmf["entity"]:
            if "solid" in entity:
                translate_solid(entity["solid"][0], x, y)
            if "origin" in entity:
                origin_bits = entity["origin"].split(" ")
                ox = float(origin_bits[0])
                oy = float(origin_bits[1])
                oz = float(origin_bits[2])
                origin = [ox, oy, oz]

                origin[0] += x
                origin[1] += y

                entity["origin"] = f"{origin[0]} {origin[1]} {origin[2]}"

    return tile_vmf

def translate_solid(solid, x, y):
    for side in solid["side"]:
        plane = parse_plane(side["plane"])
        for point in plane:
            point[0] += x
            point[1] += y
        side["plane"] = format_plane(plane)

def parse_plane(plane_data):
    bits = plane_data.split(" ")
    points = []
    for i in range(3):
        x = float(bits[i * 3 + 0][1:])
        y = float(bits[i * 3 + 1])
        z = float(bits[i * 3 + 2][:-1])
        points.append([x, y, z])
    return points

def format_plane(plane):
    return " ".join((f"({point[0]} {point[1]} {point[2]})" for point in plane))

def merge_vmf(a, b):
    if "solid" in b["world"][0]:
        for solid in b["world"][0]["solid"]:
            a["world"][0]["solid"].append(solid)
    if "entity" in b:
        for entity in b["entity"]:
            a["entity"].append(entity)

def get_tile_vmf(tile_id):
    global tiles_data, cached_tile_vmfs

    if tile_id not in cached_tile_vmfs:
        tile = tiles_data["tiles"][tile_id]
        tile_file_path = tile["file_path"]
        parser = vmf_parser.VmfParser()
        tile_vmf = parser.parse(tile_file_path)
        cached_tile_vmfs[tile_id] = tile_vmf
    return cached_tile_vmfs[tile_id]

if __name__ == "__main__":
    process_map("solan.json")