import json, copy, os
from cstrikegen.vmf_parser import VmfParser
from cstrikegen.vmf_formatter import VmfFormatter

class MapProcessor:
    def __init__(self, map_file_path):
        self.map_file_path = map_file_path
        with open(map_file_path, "r") as map_file:
            self.map_data = json.load(map_file)  
        self.map_width = self.map_data["map_width"]
        self.map_length = self.map_data["map_length"]
        self.map_height = self.map_data["map_height"]
        
        with open(self.map_data["tiles_file_path"], "r") as tiles_file:
            self.tiles_data = json.load(tiles_file)
        self.tile_width = self.tiles_data["tile_width"]
        self.tile_length = self.tiles_data["tile_length"]
        self.tile_height = self.tiles_data["tile_height"]
        self.cached_component_file_vmfs = {}

    def get_index(self, x, y, z):
        return x + y * self.map_width + z * self.map_width * self.map_length

    def get_xyz(self, index):
        x = int(index % self.map_width)
        y = int((index / self.map_width) % self.map_length)
        z = int(index / (self.map_width * self.map_length))
        return x, y, z

    def process_map(self, map_seed=None):
        map_file_name = os.path.basename(self.map_file_path)
        map_name = os.path.splitext(map_file_name)[0]
        print(f"Processing [{map_name}]...")

        map_seeds = []
        if map_seed is None:
            map_seeds = self.map_data["maps"].keys()
        else:
            map_seeds.append(map_seed)

        for map_seed in map_seeds:
            print(f"> [{map_seed}]")
            map_tiles = self.map_data["maps"][map_seed]

            # process tiles
            map_vmf = get_blank_vmf()
            for y in range(self.map_length):
                for x in range(self.map_width):
                    for z in range(self.map_height):
                        index = self.get_index(x, y, z)
                        tile_id = map_tiles[index]
                        tile_vmf = self.get_tile_vmf(tile_id)
                        tile_vmf = translate(tile_vmf, x * self.tile_width, y * self.tile_length, z * self.tile_height)
                        merge_vmf(map_vmf, tile_vmf)

            FLOOR_ID = 17
            SKY_ID = 19

            # wrap map
            for y in range(self.map_length):
                for x in range(self.map_width):
                    for z in range(self.map_height):
                        if z == 0:
                            vmf = self.get_component_vmf(FLOOR_ID)
                            vmf = translate(vmf, x * self.tile_width, y * self.tile_length, z * self.tile_height)
                            merge_vmf(map_vmf, vmf)
                        if z == self.map_height - 1:
                            vmf = self.get_component_vmf(SKY_ID)
                            vmf = translate(vmf, x * self.tile_width, y * self.tile_length, z * self.tile_height)
                            merge_vmf(map_vmf, vmf)
            
            # re-order ids
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

            formatter= VmfFormatter()
            formatter.format(f"{map_name}_{map_seed}.vmf", map_vmf)

    def get_tile_vmf(self, tile_id):
        tile = self.tiles_data["tiles"][tile_id]
        tile_vmf = get_blank_vmf()
        for component_id in tile["components"]:
            component_vmf = self.get_component_vmf(component_id)
            merge_vmf(tile_vmf, component_vmf)
        return tile_vmf

    def get_component_vmf(self, component_id):
        component = self.tiles_data["components"][str(component_id)]
        component_file_id = component["component_file"]
        if component_file_id not in self.cached_component_file_vmfs:
            component_file_path = self.tiles_data["component_files"][str(component_file_id)]
            parser = VmfParser()
            component_file_vmf = parser.parse(component_file_path)
            self.cached_component_file_vmfs[component_file_id] = component_file_vmf
        else:
            component_file_vmf = self.cached_component_file_vmfs[component_file_id]

        component_vmf = get_blank_vmf()

        if "solids" in component: component_solids = component["solids"]
        elif "solid" in component_file_vmf["world"][0]: component_solids = range(0, len(component_file_vmf["world"][0]["solid"]))
        else: component_solids = None

        if "entities" in component: component_entities = component["entities"]
        elif "entity" in component_file_vmf: component_entities =  range(0, len(component_file_vmf["entity"]))
        else: component_entities = None
        merge_vmf(component_vmf, component_file_vmf, solid_ids=component_solids, entity_ids=component_entities)
        
        return component_vmf

def translate(tile_vmf, x, y, z):
    tile_vmf = copy.deepcopy(tile_vmf)

    if "solid" in tile_vmf["world"][0]:
        for solid in tile_vmf["world"][0]["solid"]:
            translate_solid(solid, x, y, z)
    
    if "entity" in tile_vmf:
        for entity in tile_vmf["entity"]:
            if "solid" in entity:
                translate_solid(entity["solid"][0], x, y, z)
            if "origin" in entity:
                origin_bits = entity["origin"].split(" ")
                ox = float(origin_bits[0])
                oy = float(origin_bits[1])
                oz = float(origin_bits[2])
                origin = [ox, oy, oz]

                origin[0] += x
                origin[1] += y
                origin[2] += z

                entity["origin"] = f"{origin[0]} {origin[1]} {origin[2]}"

    return tile_vmf

def translate_solid(solid, x, y, z):
    for side in solid["side"]:
        plane = parse_plane(side["plane"])
        for point in plane:
            point[0] += x
            point[1] += y
            point[2] += z
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
        if solid_ids is not None:
            for solid_id in solid_ids:
                solid = b["world"][0]["solid"][solid_id]
                a["world"][0]["solid"].append(solid)
        else:
            for solid in b["world"][0]["solid"]:
                a["world"][0]["solid"].append(solid)

    if "entity" in b:
        if entity_ids is not None:
            for entity_id in entity_ids:
                entity = b["entity"][entity_id]
                a["entity"].append(entity)
        else:
            for entity in b["entity"]:
                a["entity"].append(entity)

def get_blank_vmf():
    # TODO: Need to update this to have required contents
    return { 
                "versioninfo":[
                {
                    "editorversion": "400",
                    "editorbuild": "8357",
                    "mapversion": "21",
                    "formatversion": "100",
                    "prefab": "0"
                }],
                "visgroups":[],
                "viewsettings":[
                    {
                        "bSnapToGrid": "1",
                        "bShowGrid": "1",
                        "bShowLogicalGrid": "0",
                        "nGridSpacing": "8w",
                        "bShow3DGrid": "0"
                    }
                ],
                "world":[
                {
                    "id": "1",
                    "mapversion": "21",
                    "classname": "worldspawn",
                    "detailmaterial": "detail/detailsprites",
                    "detailvbsp": "detail.vbsp",
                    "maxpropscreenwidth": "-1",
                    "skyname": "sky_day01_01",
                    "solid" : []
                }],
                "entity": [],
                "cameras": [
                    {
                        "activecamera": "-1"
                    }
                ],
                "cordon":[
                    {
                        "mins": "(-1024 -1024 -1024)",
                        "maxs": "(1024 1024 1024)",
                        "active": "0"
                    }
                ]
            }

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument('file_path', help='map config filepath')
    args = parser.parse_args()

    if not os.path.exists(args.file_path):
        print(f"No map file at [{args.file_path}]")
    else:
        map_processor = MapProcessor(args.file_path)
        map_processor.process_map()

    #process_tiles("tiles_generate.json")