import random

class Tile:
    def __init__(self, id, width, height) -> None:
        self.id = id
        self.size = [width, height]
        self.data = None

class TileInstance:

    def __init__(self, tile, position):
        self.tile = tile
        self.position = position

    def intersects(self, x, y, width, height):
        

def process():
    queued_cells = []
    # list of cells we have (at least) 1 edge for. When new tiles are added, their neighbors are added to the list

    # more complex due to our need/desire to have different sized cells
    # maybe it is edges...

    board_width = 5
    board_depth = 5

    available_tiles = []
    tiles = [] # placed tiles: list of tuples (x, y, tile). in future will add rotation
    queued_cells = []

    # cells of the board are defined. for now, all are set to "must be wall"
    for x in range(board_width):
        queued_cells.append((x, 0))
        queued_cells.append((x, board_depth-1))
    for y in range(board_depth):
        queued_cells.append((0, y))
        queued_cells.append((board_width-1, y))

    def get_sizes_placements(cell, sizes):
        # cell: [x, y]
        # sizes: [[width, depth], ...]
        # return a dictionary of placements for all sizes
        return { size : get_size_placements(cell, size) for size in sizes }

    def get_size_placements(cell, size):
        # cell: [x, y]
        # size: [width, depth]
        cell_x, cell_y = cell
        tile_width, tile_depth = size

        for dx in range(tile_width):
            for dy in range(tile_depth):
                tile_x = cell_x - dx
                tile_y = cell_y - dy

                # check whether tile size fits
                # if any square of the tile overlaps a tile in the map, it can't fit

                check_fits

    while len(queued_cells) > 0: # must be an edge left to join
        
        cell = next(queued_cells)

        # find tiles which can fit
        sizes = set((tile.size for tile in available_tiles))
        available_positions = 
        possible_tiles = [tile for tile in available_tiles if check_tile(tile, cell)]

        # pick one randomly,
        tile = random.choice(possible_tiles)
        tiles.append(  )

        # continue with routine

