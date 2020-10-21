import itertools, json

tile_id = 0
face_index = 0
tiles_data = {
    "faces" : {},
    "component_files" : {
        0 : "C:/Users/Tom/source/repos/thomasjackdalby/cstrike_gen/components/walls.vmf"
    },
    "components" : {
        1 : { "name" : "column_a0", "component_file" : 0, "solids" : [11] },
        2 : { "name" : "column_a1", "component_file" : 0, "solids" : [7] },
        3 : { "name" : "column_a2", "component_file" : 0, "solids" : [9] },
        4 : { "name" : "column_a3", "component_file" : 0, "solids" : [10] },
        5 : { "name" : "wall_a0", "component_file" : 0, "solids" : [3] },
        6 : { "name" : "wall_a1", "component_file" : 0, "solids" : [8] },
        7 : { "name" : "wall_a2", "component_file" : 0, "solids" : [1] },
        8 : { "name" : "wall_a3", "component_file" : 0, "solids" : [26] },
        9 : { "name" : "corner_a0", "component_file" : 0, "solids" : [33] },
        10 : { "name" : "corner_a1", "component_file" : 0, "solids" : [32] },
        11 : { "name" : "corner_a2", "component_file" : 0, "solids" : [31] },
        12 : { "name" : "corner_a3", "component_file" : 0, "solids" : [34] },
        13 : { "name" : "beam_a0", "component_file" : 0, "solids" : [4] },
        14 : { "name" : "beam_a1", "component_file" : 0, "solids" : [5] },
        15 : { "name" : "beam_a2", "component_file" : 0, "solids" : [12] },
        16 : { "name" : "beam_a3", "component_file" : 0, "solids" : [6] },
        17 : { "name" : "column_b0", "component_file" : 0, "solids" : [24] },
        18 : { "name" : "column_b1", "component_file" : 0, "solids" : [19] },
        19 : { "name" : "column_b2", "component_file" : 0, "solids" : [21] },
        20 : { "name" : "column_b3", "component_file" : 0, "solids" : [22] },
        21 : { "name" : "wall_b0", "component_file" : 0, "solids" : [15] },
        22 : { "name" : "wall_b1", "component_file" : 0, "solids" : [20] },
        23 : { "name" : "wall_b2", "component_file" : 0, "solids" : [13] },
        24 : { "name" : "wall_b3", "component_file" : 0, "solids" : [23] },
        25 : { "name" : "corner_b0", "component_file" : 0, "solids" : [27] },
        26 : { "name" : "corner_b1", "component_file" : 0, "solids" : [30] },
        27 : { "name" : "corner_b2", "component_file" : 0, "solids" : [29] },
        28 : { "name" : "corner_b3", "component_file" : 0, "solids" : [28] },
        29 : { "name" : "beam_b0", "component_file" : 0, "solids" : [16] },
        30 : { "name" : "beam_b1", "component_file" : 0, "solids" : [17] },
        31 : { "name" : "beam_b2", "component_file" : 0, "solids" : [25] },
        32 : { "name" : "beam_b3", "component_file" : 0, "solids" : [18] },
        33 : { "name" : "floor", "component_file" : 0, "solids" : [0] },
        34 : { "name" : "walkway", "component_file" : 0, "solids" : [2] },
        35 : { "name" : "roof", "component_file" : 0, "solids" : [14] },
    },
    "tiles" : {}
}

base_column_ids = [1, 2, 3, 4]
base_wall_ids = [5, 6, 7, 8]
base_corner_ids = [9, 10, 11, 12]
base_beam_ids = [13, 14, 15, 16]
top_column_ids = [17, 18, 19, 20]
top_wall_ids = [21, 22, 23, 24]
top_corner_ids = [25, 26, 27, 28]
top_beam_ids = [29, 30, 31, 32]
floor_id = 33
walkway_id = 34
roof_id = 35

def merge_faces(base_tile, top_tile=None, force_empty=True):
    global face_index
    faces = []
    for i in range(4):
        base_face = base_tile["raw_faces"][i]
        if top_tile is not None:
            top_face = top_tile["raw_faces"][i]
            face = f"{base_face}{top_face}"
        else:
            if force_empty:
                face = f"{base_face}0000"
            else:
                face = base_face
        if face not in tiles_data["faces"]:
            tiles_data["faces"][face] = face_index
            face_index += 1
        face_id = tiles_data["faces"][face]
        faces.append(face_id)
    return faces

def merge_tile_sets(base_tiles, top_tiles, force_empty=True):
    global tile_id, tiles_data
    for base_tile in base_tiles:
        for top_tile in top_tiles:
            components = list(set(base_tile["components"] + top_tile["components"]))
            components.append(floor_id)
            faces = merge_faces(base_tile, top_tile, force_empty)
            tiles_data["tiles"][tile_id] = {
                "name" : "",
                "components" : components,
                "faces" : faces,
            }
            tile_id += 1

def generate_tiles():
    global tiles_data, tile_id
    base_walkway_tiles = generate_part_tiles(True, True)
    base_no_walkway_tiles = generate_part_tiles(True, False)
    base_no_walkway_full_height_tiles = generate_part_tiles(True, False, True)

    top_roof_tiles = generate_part_tiles(False, True)
    top_no_roof_tiles = generate_part_tiles(False, False)

    print(f"Full height base tiles [{tile_id}]")
    for base_tile in base_no_walkway_full_height_tiles:
        components = list(base_tile["components"])
        components.append(floor_id)
        faces = merge_faces(base_tile, None, False)
        tiles_data["tiles"][tile_id] = {
            "name" : "",
            "components" : components,
            "faces" : faces,
        }
        tile_id += 1

    print(f"Base tiles [{tile_id}]")
    for base_tile in base_walkway_tiles + base_no_walkway_tiles:
        components = list(base_tile["components"])
        components.append(floor_id)
        faces = merge_faces(base_tile)
        tiles_data["tiles"][tile_id] = {
            "name" : "",
            "components" : components,
            "faces" : faces,
        }
        tile_id += 1
    
    print(f"Top Roof tiles [{tile_id}]")
    merge_tile_sets(base_walkway_tiles, top_roof_tiles)
    print(f"Top No Roof tiles [{tile_id}]")
    merge_tile_sets(base_walkway_tiles, top_no_roof_tiles)

    # base walkway x top no roof
    # base walkway x top roof
    # base walkway
    # base no walkway
    # base no walkway full height
    print(f"Generated {len(tiles_data['tiles'])} tiles.")

    with open('tiles_generate.json', 'w') as file:
        json.dump(tiles_data, file)

def generate_part_tiles(is_base, has_top=False, full_height=False):
    if is_base:
        column_ids = base_column_ids
        wall_ids = base_wall_ids
        corner_ids = base_corner_ids
        beam_ids = base_beam_ids
        top_id = walkway_id
    else:
        column_ids = top_column_ids
        wall_ids = top_wall_ids
        corner_ids = top_corner_ids
        beam_ids = top_beam_ids
        top_id = roof_id

    part_tiles = []
    for combination in itertools.product([True,False],repeat=5):
        wall_combination = combination[0:4]

        if sum(wall_combination) >= 3:
            continue

        has_doors = combination[4]
        raw_faces = [0] * 4
        components = []
        if has_top:
            if full_height:
                components.append(roof_id)
            else:
                components.append(top_id)

        for i in range(4):
            if wall_combination[i]:
                components.append(wall_ids[i])
                if full_height:
                    components.append(top_wall_ids[i])
            if has_doors or wall_combination[i] or wall_combination[(i-1) % 4]:
                components.append(column_ids[i])
                if full_height:
                    components.append(top_column_ids[i])
            if has_doors or wall_combination[i] or has_top:
                components.append(beam_ids[i])
                if full_height:
                    components.append(top_beam_ids[i])
            if has_doors or wall_combination[i] or wall_combination[(i-1) % 4] or has_top:
                components.append(corner_ids[i])
                if full_height:
                    components.append(top_corner_ids[i])

            w = int(wall_combination[i])
            lc = int(has_doors or wall_combination[i] or wall_combination[(i-1) % 4])
            rc = int(has_doors or wall_combination[i] or wall_combination[(i+1) % 4])
            t = int(has_doors or wall_combination[i] or has_top)

            if i == 0 or i == 1: raw_face = f"{w}{lc}{rc}{t}"
            else: raw_face = f"{w}{rc}{lc}{t}"
            if full_height:
                raw_face += raw_face
            raw_faces[i] = raw_face

        part_tiles.append({
            "name" : "generated_tile",
            "components" : components,
            "raw_faces" : raw_faces,
        })
    return part_tiles

if __name__ == "__main__":
    generate_tiles()