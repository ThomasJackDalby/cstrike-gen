
# CSS-Level-Gen

Tiles edge lengths are powers of 2, i.e. 
1, 2, 4, 

A tile is an integer matrix, where each integer refers to the material
The level generator will work out the best way to form components (volumes of the same material)

## Algorithm

The algorithm chooses an edge from the current edge list, and assigns a tile to it that matches (based on a distribution)
If there are no tiles that fit, then the algorithm unwinds one step and chooses a different tile.
- Therefore the tile selection is done via a shuffled list, such that the next can be popped.

## Loading Tiles
Tiles are stored/defined as bitmap with a seperate config file
The .bmp files store "slices", typically xy





# Plan

- if all cells are the same size, we can do overlap.
    + can control macro layout with variable probability
    + simpler
    + might lead to more triangles without  post-processing
    
- if we allow different sizes, must be divisible etc.
    + we can specifically build certain rooms etc
    + terrain is on a macro scale rather than minor
    - more complex to code
    - might not converge as often (unknown until testing)


