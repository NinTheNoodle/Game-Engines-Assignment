C12722625 Shane Farrell
=======================

Infinite terrain generator

Controls:

WASD + SPACE

SPACE is the primary method of propulsion and will launch you at high speeds downwards. The trick is to press space on the side of a hill and use it as a ramp to gain large amounts of horisontal velocity.


The terrain generator creates terrain cells. Each terrain cell has its own heightmap, where the heightmap data is generated using perlin noise dictating various attributes such as moisture levels, height and snow amount. The heightmap is stored as an array on the terrain cell scope and in a dictionary on the scope of the entire map. The use of the dictionary is what allows the terrain to be infinite. Snow particle generators are created and destroyed with the terrain cells. Terrain cells are created and destroyed by the terrain generator to fill a circle around the camera. The camera's far draw distance is set to be smaller than this circle to hide the edges without having to resort to fog. The snow has physics.