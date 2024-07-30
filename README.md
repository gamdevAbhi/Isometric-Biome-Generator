# Isometric-Biome-Generator
 
Game Engine - Unity Game Engine 2018 LTS
Softwares Used - Photoshop, VS Code.
Available On - Windows, Android

How It Procedurally Generate Biomes

- At first It create a height grid using jagged array (multidimensional array can also be used) for creating mountains (tweaking can be done through biome scriptable object)

- Then it creates the world using my modified version of wave function collapse algorithm based on the terrain relations. at this point it contains noises.

- then each height noise smoothing is applied to generate a much pleasing biome.

- after that entities such as trees, grasses etc are placed and noise smoothing applied just like before.

- at this point the world generation is finished but all objects are in the same place. as it is a isometric view we can't just place the assets like normal way.

- for that isometric perspective is applied and each objects are placed to make them look like isometric.

- as it is 2d sorting is important so for that sorting is done.

- I have also created different perspective view for isometric where each shows the world in different angle in real-time. but it can not be accessed the build file. To access it please download the unity project.
