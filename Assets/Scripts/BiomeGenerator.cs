using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Isometric
{
    public static class BiomeGenerator
    {
        public static List<Grid[][]> GenerateBiome(Biome biome, Vector3Int grid)
        {
            List<Grid[][]> world = new List<Grid[][]>();

            Dictionary<Entity, EigenState> terrainsState = GenerateEigenstates(biome.terrains);
            Dictionary<Entity, EigenState> entitiesState = GenerateEigenstates(biome.entities);

            int[][] heightGrid = CreateMountains(grid, biome);

            for(int i = 0; i < grid.z; i++)
            {
                Grid[][] grids = new Grid[grid.y][];

                for(int j = 0; j < grids.Length; j++) grids[j] = new Grid[grid.x];

                world.Add(grids);

                GenerateTerrainGrid(terrainsState, heightGrid, world);
                NoiseSmoothing(world, biome.terrainNoiseFactor);
            }

            GenerateEntityGrid(entitiesState, world);
            // NoiseSmoothing(world, biome.entitiesNoiseFactor);

            return world;
        }

        private static int[][] CreateMountains(Vector3Int grid, Biome biome)
        {
            int[][] heightGrid = new int[grid.y][];

            for(int i = 0; i < grid.y; i++) heightGrid[i] = new int[grid.x];
            
            int mountains = 0;

            while(mountains < biome.mountainFactor)
            {
                if(Random.Range(0, 101) > biome.mountainProbablity) 
                {
                    mountains++;
                    continue;
                }

                int z = Random.Range(1, grid.z);
                int x = Random.Range(0, heightGrid[0].Length);
                int y = Random.Range(0, heightGrid.Length);
                
                bool[][] check = new bool[heightGrid.Length][];

                for(int i = 0; i < heightGrid.Length; i++) check[i] = new bool[heightGrid[0].Length];

                CreateMountainGrid(x, y, z, heightGrid, check);

                mountains++;
            }

            return heightGrid;
        }

        private static void CreateMountainGrid(int x, int y, int z, int[][] grid, bool[][] check)
        {
            Vector2Int[] coord = new Vector2Int[]{
                new Vector2Int(0, 1) + new Vector2Int(x, y),
                new Vector2Int(0, -1) + new Vector2Int(x, y),
                new Vector2Int(-1, 0) + new Vector2Int(x, y),
                new Vector2Int(1, 0) + new Vector2Int(x, y),
            };

            if(z == 0) return;

            grid[y][x] = Mathf.Max(grid[y][x], z);
            check[y][x] = true;

            if(Random.Range(1, 100) > 50)
            { 
                z -= 1;
            }
            else if(Random.Range(1, 100) > 50)
            {
                z = Random.Range(1, z);
            }

            foreach(Vector2Int pos in coord)
            {
                if(pos.x < 0 || pos.x >= grid[0].Length) continue;
                if(pos.y < 0 || pos.y >= grid.Length) continue;
                if(check[pos.y][pos.x]) continue;

                CreateMountainGrid(pos.x, pos.y, z, grid, check);
            }
        }

        private static Dictionary<Entity, EigenState> GenerateEigenstates(Entity[] entities)
        {
            Dictionary<Entity, EigenState> entries = new Dictionary<Entity, EigenState>();

            for(int i = 0; i < entities.Length; i++)
            {
                entries.Add(entities[i], new EigenState(entities[i].probablity, entities[i]));
            }

            return entries;
        }

        private static void GenerateTerrainGrid(Dictionary<Entity, EigenState> states, int[][] heightGrid,
        List<Grid[][]> world)
        {
            Grid[][] grids = world[world.Count - 1];
            Grid[][] previousGrid = (world.Count > 1)? world[world.Count - 2] : null;

            for(int y = 0; y < grids.Length; y++)
            {
                for(int x = 0; x < grids[y].Length; x++)
                {
                    if(previousGrid != null && previousGrid[y][x] == null) continue;

                    if(world.Count - 1 >= heightGrid[y][x] + 1) continue;

                    Terrain parent = (previousGrid != null)? previousGrid[y][x].GetEntity() as Terrain : null;

                    Entity entity = GetEntity(GetNeighbours(world, x, y), states, parent);
                    
                    grids[y][x] = (entity != null)? 
                    new Grid("Tile[" + x.ToString() + "," + y.ToString() + "]", entity) : null;
                }
            }
        }

        public static Grid GetHighestGrid(List<Grid[][]> world, int x, int y, int z)
        {
            for(int height = z; height >= 0; height--)
            {
                if(world[height][y][x] != null)
                {
                    return world[height][y][x];
                }
            }

            return null;
        }

        private static void GenerateEntityGrid(Dictionary<Entity, EigenState> states,
        List<Grid[][]> world)
        {
            Grid[][] grids = new Grid[world[0].Length][];

            for(int i = 0; i < grids.Length; i++) grids[i] = new Grid[world[0][0].Length];

            for(int y = 0; y < grids.Length; y++)
            {
                for(int x = 0; x < grids[y].Length; x++)
                {
                    Grid parent = GetHighestGrid(world, x, y, world.Count - 1);

                    if(parent == null) continue;

                    Entity entity = GetEntity(GetNeighbours(world, x, y), states, parent.GetEntity() as Terrain);
                    
                    grids[y][x] = (entity != null)? 
                    new Grid("Tile[" + x.ToString() + "," + y.ToString() + "]", entity) : null;
                }
            }

            world.Add(grids);
        }

        private static List<Grid> GetNeighbours(List<Grid[][]> world, int x, int y)
        {
            List<Grid> neighbours = new List<Grid>();

            Vector2Int[] coord = new Vector2Int[]{
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, -1),
                new Vector2Int(1, -1)
            };

            for(int i = 0; i < coord.Length; i++)
            {
                if(y + coord[i].y < 0 || y + coord[i].y >= world[0].Length) continue;
                if(x + coord[i].x < 0 || x + coord[i].x >= world[0][0].Length) continue;

                for(int height = world.Count - 1; height >= 0; height--)
                {
                    if(world[height][y + coord[i].y][x + coord[i].x] != null)
                    {
                        neighbours.Add(world[height][y + coord[i].y][x + coord[i].x]);
                        break;
                    }
                }
            }

            return neighbours;
        }

        private static Entity GetEntity(List<Grid> neighbours, Dictionary<Entity, EigenState> states, 
        Terrain parent)
        {
            Dictionary<Entity, int> neighboursCount = new Dictionary<Entity, int>();
            Dictionary<Entity, int> entitesProbablity = new Dictionary<Entity, int>();

            for(int i = 0; i < neighbours.Count; i++)
            {
                if(!neighboursCount.ContainsKey(neighbours[i].GetEntity())) 
                    neighboursCount[neighbours[i].GetEntity()] = 1;
                else 
                    neighboursCount[neighbours[i].GetEntity()] += 1;
            }

            int minProbablity = int.MaxValue;

            foreach(KeyValuePair<Entity, EigenState> pair in states)
            {
                if(parent != null && parent.IsEntityAllowed(pair.Key) == false) continue;
                if(!neighboursCount.ContainsKey(pair.Key)) neighboursCount[pair.Key] = 0;

                entitesProbablity[pair.Key] = states[pair.Key].GetProbablity(neighboursCount[pair.Key]);

                minProbablity = (entitesProbablity[pair.Key] <= minProbablity)? entitesProbablity[pair.Key] : minProbablity;
            }

            entitesProbablity = entitesProbablity.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);


            Entity previousEntity = null;
            Dictionary<Entity, int> result = new Dictionary<Entity, int>();

            foreach(KeyValuePair<Entity, int> pair in entitesProbablity)
            {
                result[pair.Key] = entitesProbablity[pair.Key] + Mathf.Abs(minProbablity) + 1 +
                ((previousEntity != null)? result[previousEntity] : 0);

                previousEntity = pair.Key;
            }

            if(result.Count == 0) return null;

            int totalProbablity = result.Values.Last() + 1;

            int value = Random.Range(0, totalProbablity);
            Entity entity = previousEntity;

            foreach(KeyValuePair<Entity, int> pair in result)
            {
                if(result[pair.Key] >= value)
                {
                    entity = pair.Key;
                    break;
                }
            }

            return entity;
        }

        private static void NoiseSmoothing(List<Grid[][]> world, float noiseFactor)
        {
            bool[][] check = new bool[world[0].Length][];

            for(int i = 0; i < world[0].Length; i++) check[i] = new bool[world[0][0].Length];

            for(int y = 0; y < world[0].Length; y++)
            {
                for(int x = 0; x < world[0][0].Length; x++)
                {
                    if(world[world.Count - 1][y][x] == null || check[y][x]) continue;

                    Connector connect = new Connector(x, y, world, check);
                    if(connect.IsNoise(noiseFactor)) connect.Convert();
                }
            }
        }
    }

    public class Connector
    {
        private List<Grid> sameGrids;
        private List<Grid> visitedNeighbours;
        private Grid parent;
        private bool isTerrainAllowed;

        protected internal Connector(int x, int y, List<Grid[][]> world, bool[][] check)
        {
            this.sameGrids = new List<Grid>();
            this.isTerrainAllowed = world[world.Count - 1][y][x].GetEntity() is Terrain;

            visitedNeighbours = new List<Grid>();

            if(world.Count - 1 > 0) parent = world[world.Count - 2][y][x];  

            FindConnection(new Vector3Int(x, y, world.Count - 1), world, check, visitedNeighbours);
        }

        protected internal bool IsNoise(float noiseFactor)
        {
            int count = sameGrids.Count;
            
            return ((float)count / (count + visitedNeighbours.Count)) < noiseFactor;
        }

        protected internal void Convert()
        {
            Dictionary<Entity, int> entityCount = new Dictionary<Entity, int>();

            for(int i = 0; i < sameGrids.Count;)
            {
                if(sameGrids[i].GetEntity() is Terrain && !isTerrainAllowed) sameGrids.RemoveAt(i);
                else i++;
            }

            for(int i = 0; i < visitedNeighbours.Count;)
            {
                if(visitedNeighbours[i].GetEntity() is Terrain && !isTerrainAllowed) visitedNeighbours.RemoveAt(i);
                else i++;
            }

            for(int i = 0; i < sameGrids.Count; i++)
            {
                if(!entityCount.ContainsKey(sameGrids[i].GetEntity())) entityCount[sameGrids[i].GetEntity()] = 1;
                else entityCount[sameGrids[i].GetEntity()] += 1;
            }

            for(int i = 0; i < visitedNeighbours.Count; i++)
            {
                if(!entityCount.ContainsKey(visitedNeighbours[i].GetEntity())) 
                    entityCount[visitedNeighbours[i].GetEntity()] = 1;
                else 
                    entityCount[visitedNeighbours[i].GetEntity()] += 1;
            }

            entityCount = entityCount.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            
            Entity choiceEntity = entityCount.Keys.Last();
            Terrain parentTerrain = (parent != null)? parent.GetEntity() as Terrain : null;

            if(parentTerrain != null) 
            {
                foreach(Entity entity in entityCount.Keys.Reverse()) 
                {
                    if(parentTerrain.IsEntityAllowed(entity))
                    {
                        choiceEntity = entity;
                        break;
                    }
                }
            }

            foreach(Grid connection in sameGrids) connection.ChangeEntity(choiceEntity);
        }

        private void FindConnection(Vector3Int coord, List<Grid[][]> world, bool[][] check, List<Grid> visitedNeighbours)
        {
            Grid grid = world[coord.z][coord.y][coord.x];
            check[coord.y][coord.x] = true;

            sameGrids.Add(grid);

            List<Vector3Int> neighboursCoord = GetNeighboursCoord(world, coord.x, coord.y);

            foreach(Vector3Int nCoord in neighboursCoord)
            {
                Grid neighbour = world[nCoord.z][nCoord.y][nCoord.x];

                if(grid.GetEntity() == neighbour.GetEntity() && IsValidConnection(coord, nCoord) &&
                sameGrids.Contains(neighbour) == false) FindConnection(nCoord, world, check, visitedNeighbours);
                else if(visitedNeighbours.Contains(neighbour) == false &&
                grid.GetEntity() != neighbour.GetEntity()) visitedNeighbours.Add(neighbour);
            }
        }

        private bool IsValidConnection(Vector3Int gridCoord, Vector3Int connectorCoord)
        {
            Vector2Int[] coord = new Vector2Int[]{
                new Vector2Int(0, 1) + new Vector2Int(gridCoord.x, gridCoord.y),
                new Vector2Int(0, -1) + new Vector2Int(gridCoord.x, gridCoord.y),
                new Vector2Int(-1, 0) + new Vector2Int(gridCoord.x, gridCoord.y),
                new Vector2Int(1, 0) + new Vector2Int(gridCoord.x, gridCoord.y),
            };

            Vector2Int resultCoord = new Vector2Int(connectorCoord.x, connectorCoord.y);

            foreach(Vector2Int validCoord in coord)
            { 
                if(resultCoord == validCoord) return true;
            }

            return false;
        }

        private List<Vector3Int> GetNeighboursCoord(List<Grid[][]> world, int x, int y)
        {
            List<Vector3Int> neighbours = new List<Vector3Int>();

            Vector2Int[] coord = new Vector2Int[]{
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, -1),
                new Vector2Int(1, -1)
            };

            for(int i = 0; i < coord.Length; i++)
            {
                if(y + coord[i].y < 0 || y + coord[i].y >= world[0].Length) continue;
                if(x + coord[i].x < 0 || x + coord[i].x >= world[0][0].Length) continue;

                for(int height = world.Count - 1; height >= 0; height--)
                {
                    if(world[height][y + coord[i].y][x + coord[i].x] != null)
                    {
                        neighbours.Add(new Vector3Int(x + coord[i].x, y + coord[i].y, height));
                        break;
                    }
                }
            }

            return neighbours;
        }
    }

    public class Grid
    {
        private Entity entity;
        private GameObject gameObject;

        protected internal Grid(string name, Entity entity)
        {
            this.gameObject = new GameObject(name);
            this.gameObject.AddComponent<SpriteRenderer>();
            this.entity = entity;
            
            ChangeSprite();
        }
        
        protected internal GameObject GetGameObject()
        {
            return gameObject;
        }

        protected internal Entity GetEntity()
        {
            return entity;
        }

        protected internal void ChangeEntity(Entity entity)
        {
            this.entity = entity;
            ChangeSprite();
        }

        protected internal void ChangeSprite()
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = entity.sprite;
        }
    }

    public class EigenState
    {
        private int[] probablity;

        protected internal EigenState(int probablity, Entity entity)
        {
            this.probablity = new int[9];
            this.probablity[0] = probablity;

            int max = entity.positiveBias;
            int min = -entity.negetiveBias;

            for(int i = 1; i < this.probablity.Length; i++)
            {
                this.probablity[i] = this.probablity[i - 1] + Random.Range(min, max);
            }
        }

        protected internal int GetProbablity(int neighbours)
        {
            return probablity[neighbours];
        }
    }
}
