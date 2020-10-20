import os
import json

def process_tiles(folder_path):
    # generates an available tiles list
    # creates a tiles.json file in a tiles folder

    next_face_id = 0
    faces = {}
    tiles = {}
    next_tile_id = 0
    for file_name in os.listdir(folder_path):    
        if not file_name.endswith(".vmf"):
            continue
        print(f"Processing [{file_name}]")
        file_path = os.path.abspath(os.path.join(folder_path, file_name))

        bits = os.path.splitext(file_name)[0].split("#")
        tile_name = bits[0]
        tile_faces = bits[1].split("_")

        tile_face_ids = []
        for tile_face in tile_faces:
            if tile_face in faces:
                tile_face_id = faces[tile_face]
            else:
                tile_face_id = next_face_id
                faces[tile_face] = tile_face_id
                next_face_id += 1
            tile_face_ids.append(tile_face_id)

        tiles[next_tile_id] = {
            "name" : tile_name,
            "faces" : tile_face_ids,
            "file_path" : file_path
        }
        next_tile_id += 1
    data = {
        "faces": faces,
        "tiles": tiles,
    }
    with open(os.path.join(folder_path,'tiles.json'), 'w') as file:
        json.dump(data, file)

if __name__ == "__main__":
    process_tiles("tiles")