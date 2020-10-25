import json, copy, os
from vmf_parser import VmfParser
from vmf_formatter import VmfFormatter

tiles_data = {}
cached_component_file_vmfs = {}

def process_tiles(tiles_file_path):
    global tiles_data, cached_component_file_vmfs
    with open(tiles_file_path, "r") as tiles_file:
        tiles_data = json.load(tiles_file)

    for tile_id in tiles_data["tiles"]:
        tile_vmf = get_tile_vmf(tile_id)
        formatter = VmfFormatter()
        formatter.format(f"tiles/tile_{tile_id}.vmf", tile_vmf)

def process_map(map_file_path, map_seed=None):
    global map_data, tiles_data, cached_component_file_vmfs

    map_file_name = os.path.basename(map_file_path)
    map_name = os.path.splitext(map_file_name)[0]
    print(f"Processing [{map_name}]...")

    map_data = {}

    with open(map_file_path, "r") as map_file:
        map_data = json.load(map_file)

    with open(map_data["tiles_file_path"], "r") as tiles_file:
        tiles_data = json.load(tiles_file)

    tile_width = 192
    tile_height = 192
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
        print(f"> [{map_seed}]")
        map_tiles = map_data["maps"][map_seed]

        map_vmf = get_blank_vmf()

        for y in range(map_length):
            for x in range(map_width):
                index = get_index(x, y)
                tile_id = map_tiles[index]
                tile_vmf = get_tile_vmf(tile_id)
                tile_vmf = translate(tile_vmf, x * tile_width, y * tile_height)
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

        formatter = VmfFormatter()
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

def merge_vmf(a, b, solid_ids=None, entity_ids=None,debug=False):
    if "solid" in b["world"][0]:
        if solid_ids:
            for solid_id in solid_ids:
                solid = b["world"][0]["solid"][solid_id]
                a["world"][0]["solid"].append(solid)
        else:
            for solid in b["world"][0]["solid"]:
                a["world"][0]["solid"].append(solid)
    if "entity" in b:
        for entity in b["entity"]:
            if entity_ids and entity["id"] not in entity_ids:
                continue
            a["entity"].append(entity)

def get_tile_vmf(tile_id):
    global tiles_data
    tile = tiles_data["tiles"][tile_id]
    tile_vmf = get_blank_vmf()
    for component_id in tile["components"]:
        component_vmf = get_component_vmf(component_id)
        merge_vmf(tile_vmf, component_vmf)
    return tile_vmf

def get_blank_vmf():
    # TODO: Need to update this to have required contents
    return { 
            "world": [{
                "solid" : []
            }],
            "entity": []
        }

def get_component_vmf(component_id):
    global tiles_data, cached_component_file_vmfs

    component = tiles_data["components"][str(component_id)]
    component_file_id = component["component_file"]
    if component_file_id not in cached_component_file_vmfs:
        component_file_path = tiles_data["component_files"][str(component_file_id)]
        parser = VmfParser()
        component_file_vmf = parser.parse(component_file_path)
        cached_component_file_vmfs[component_file_id] = component_file_vmf
    else:
        component_file_vmf = cached_component_file_vmfs[component_file_id]

    debug = True if component_id == 33 else False
    component_vmf = get_blank_vmf()

    if "solids" in component: component_solids = component["solids"]
    elif "solid" in component_file_vmf["world"][0]: component_solids = component_file_vmf["world"][0]["solid"]
    else: component_entities = None

    if "entities" in component: component_entities = component["entities"]
    elif "entity" in component_file_vmf: component_entities = [entity["id"] for entity in component_file_vmf["entity"]]
    else: component_entities = None
    merge_vmf(component_vmf, component_file_vmf, solid_ids=component_solids, entity_ids=component_entities, debug=debug)
    
    return component_vmf

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument('file_path', help='map config filepath')
    args = parser.parse_args()


    if not os.path.exists(args.file_path):
        print(f"No map file at [{args.file_path}]")
    else:
        process_map(args.file_path)


    #process_tiles("tiles_generate.json")