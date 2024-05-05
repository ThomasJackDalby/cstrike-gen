
import json
import random
from PIL import Image
# e.g.


class Tile:

    def __init__(self, id, width, height) -> None:
        self.id = id
        self.size = [width, height]
        self.data = None

class Cube:
    def __init__(self, id, width, height, depth) -> None:
        self.id = id
        self.size = [width, height]
        self.data = [0] * width * height

def load_tiles(data_file_path):
    # needs to be a .bmp for the data, and a .json for the metadata
    image = Image.open(data_file_path)
    image_width = image.size[0]
    pixels = list(image.getdata())

    meta_data_file_path = data_file_path + ".json"
    with open(meta_data_file_path, "r") as file:
        data = json.load(file)

    pixel_to_id_map = {tuple(material["rgb"]): material["id"]
                       for material in data["materials"]}

    def get_material_id(pixel):
        return pixel_to_id_map[pixel]

    def get_tile_pixels(x, y, width, height):
        tile_indexes = (x + image_width * (y + j) +
                        i for j in range(height) for i in range(width))
        tile_pixels = (tuple(pixels[index][:3]) for index in tile_indexes)
        return [get_material_id(pixel) for pixel in tile_pixels]

    def load_tile(tile_data):
        tile_id = tile_data["id"]
        tile_width, tile_height = tile_data["size"]
        tile = Tile(tile_id, tile_width, tile_height)
        tile.data = get_tile_pixels(*tile_data["origin"], *tile_data["size"])
        return tile

    return [load_tile(tile_data) for tile_data in data["tiles"]]


def load_cubes(file_path, tile_set_map):
    # tiles is a dictionary of tile_set_id to tile_id to tile
    with open(file_path, "r") as file:
        data = json.load(file)

    def get_cube_voxels(tiles):
        return [value for tile_set_id, tile_id, amount in tiles for _ in amount for value in tile_set_map[tile_set_id][tile_id].data]

    def load_cube(cube_data):
        cube = Cube()
        # add cube size?
        cube.data = get_cube_voxels(cube_data["tiles"])
        return cube

    return [load_cube(cube_data) for cube_data in data["cubes"]]

def print_cube():
    pass




if __name__ == "__main__":
    tiles = load_tiles("tiles.bmp")
    tile_set_map = {"tiles": {tile.id: tile for tile in tiles}}
    cubes = load_cubes("cubes.json", tile_set_map)
