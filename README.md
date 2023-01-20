# Procedural 2D Dungeon Generation in Unity

## Topic

![End result](https://user-images.githubusercontent.com/25634121/213528108-e8005425-602c-43f2-b87f-3b3080f481e9.png)

Procedural dungeons are a staple of dungeon crawlers. Roaming around in a dungeons whose layout changes every time, making your way to the end.
Below I will write down my thought process on how I went about and created my own implementation of one of the algorithms.

I based my implementation on the Reddit post by the [TinyKeep dev](https://www.reddit.com/r/gamedev/comments/1dlwc4/procedural_dungeon_generation_algorithm_explained/)
Here he offers a step-by-step explanation of how he generates his dungeons.

## Single Room Generation

To keep things simple, I decided to use square tiles to lay out my rooms. Each room prefab contains a script (Generate Room) to arrange the tiles, a box collider and a rigidbody.
The script I wrote has 2 configurable parameters: cell size and cell spacing. Cell size, as the name implies defines the size of each cell (square size only). The cell spacing variable defines the size of the box collider, and how much distance the room wants between itself and other rooms.

## Multiple Room Generation

The Generate Dungeon script generate a variable number of rooms, with minimum and maximum dimensions.

### Part 1: Generate Rooms

The first step is to generate the rooms. This is done in a simple for loop where I instantiate the room prefab game objects, assign them their position, and also calculate the mean width and height of rooms, as I need this for later.
The width and height of the rooms are pseudo-random numbers from a normal distribution using the [Marsaglia polar method](https://en.wikipedia.org/wiki/Marsaglia_polar_method). This way, the numbers don't diverge as heavily as they would with Unity's built-in random numbers.

When all the rooms are placed, I run a physics simulation on them so they spread out (hence the box collision and rigidbody).

![Generated Rooms](https://user-images.githubusercontent.com/25634121/213528184-8fbf437f-0d5a-4b07-a0a9-1a534a947e1e.png)


### Part 2: Select Main Rooms

Next up, I have to select the rooms that I will end up connecting, my main rooms. I compare the width and height of the room agains the mean width/height, compared with the selection threshhold variable. This variable skews the selection: the higher the less likely main rooms will be chosen, the lower the more likely.

Rooms that aren't chosen for a main room are saved in a different list, so they can be deleted later.

![Main Rooms](https://user-images.githubusercontent.com/25634121/213528255-b4ec130b-2f1c-41f1-a3fc-b3f2b5c9f618.png)

The main rooms are highlighted in black.

## The algorithm part: the algorithm part

This part was where I really struggled. I had to combine different algorithms, and find a way to tie their output together.
- First, I pass a list with the center coordinates of the rooms to a function with calculates the convex hull. ([Jarvis-March gift-wrapping algorithm](https://en.wikipedia.org/wiki/Gift_wrapping_algorithm))

![Convex Hull](https://user-images.githubusercontent.com/25634121/213528309-36b5aa1f-18c4-4bce-a00e-c7da467627cd.png)

- Then, I create triangles using the hull vertices

- Thirdly, I had to transform the triangles using the [Delaunay Algorithm](https://en.wikipedia.org/wiki/Delaunay_triangulation). This agorithm is used to avoid long, thin triangles which can otherwise occur by flipping the common edge between 2 triangles. I found an implementation of this algorithm on [Habrador](https://www.habrador.com/tutorials/math/11-delaunay/).

![Triangulation](https://user-images.githubusercontent.com/25634121/213528335-3599d006-f5c8-41e4-b004-ee2db21e0957.png)

- The last part is where I struggled the most. I had to generate an [MST (minimum spanning tree)](https://en.wikipedia.org/wiki/Minimum_spanning_tree) using the list of separate triangles I got from the Delaunay algorithm. I converted the triangles into a graph using the implementation found on [Wikipedia's page on graph cycles](https://en.wikipedia.org/wiki/Cycle_(graph_theory)).

![Corridor + MST](https://user-images.githubusercontent.com/25634121/213528368-06d352c1-3691-4638-a358-67933c188f90.png)


## Corridor Generation

Now that I have edges that connect my rooms, all that's left is to actually connect them. The way I did this is pretty simply and straightforward: I calculate my corner point and start creating tiles until I either reach said corner point, or collide with the other room.

## Future work
This implementation is far from perfect (please don't use this as-is in any project!), but I plan on improving on this as time goes on. Improving my graph so the rooms connect better, adding collision to the walls, rooms, corridors, so it can be turned into something playable. This was my first venture into the realm of procedural generation, a topic I am very interested in, so I want to do more in this field in the future.

## Bibliography

- TinyKeep Dev: https://www.reddit.com/r/gamedev/comments/1dlwc4/procedural_dungeon_generation_algorithm_explained/
- Explanation with gifs & iamges: https://www.gamedeveloper.com/programming/procedural-dungeon-generation-algorithm
- Marsaglia Polar Method: https://en.wikipedia.org/wiki/Marsaglia_polar_method
- Jarvis-March gift-wrapping algorithm: https://en.wikipedia.org/wiki/Gift_wrapping_algorithm
- Delaunay Triangulation: https://en.wikipedia.org/wiki/Delaunay_triangulation
- Minimum Spanning Tree: https://en.wikipedia.org/wiki/Minimum_spanning_tree
- Cycles in graphs: https://en.wikipedia.org/wiki/Cycle_(graph_theory)
