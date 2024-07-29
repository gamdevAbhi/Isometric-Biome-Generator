using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric
{
    [System.Serializable]
    public class IsometricPerspective
    {
        private PerspectiveInfo left = new PerspectiveInfo("Left", new Vector2(1, 0), new Vector2(0, 1), 
        new Vector2Int(1, 1));
        private PerspectiveInfo right = new PerspectiveInfo("Right", new Vector2(-1, 0), new Vector2(0, -1), 
        new Vector2Int(-1, -1));
        private PerspectiveInfo up = new PerspectiveInfo("Up", new Vector2(-1, 0), new Vector2(0, 1), 
        new Vector2Int(1, -1));
        private PerspectiveInfo down = new PerspectiveInfo("Down", new Vector2(1, 0), new Vector2(0, -1), 
        new Vector2Int(-1, 1));

        [SerializeField]
        private PerspectiveInfo currentPerspective;

        public IsometricPerspective()
        {
            left.SetRelation(up, down);
            down.SetRelation(left, right);
            right.SetRelation(down, up);
            up.SetRelation(right, left);

            currentPerspective = left;
        }

        public void SetNextPerspective()
        {
            currentPerspective = currentPerspective.GetNextPerspective();
        }

        public void SetPreviousPerspective()
        {
            currentPerspective = currentPerspective.GetPreviousPerspective();
        }

        public void ArrangeBiomeInIsometric(Biome biome, List<Grid[][]> world)
        {
            SetBiomeInIsometric(biome, this, world);
            SortToIsometric(world, this);
        }

        private static Vector2 IsometricToWorldPosition(float width, float height, float x, float y, 
        IsometricPerspective perspective)
        {
            Vector2 leftVector = perspective.currentPerspective.GetLeftVector();
            Vector2 upVector = perspective.currentPerspective.GetUpVector();

            float widthLength = 5 * width / 4;
            float heightLength = 5 * height / 4;
            float leftXvalue = (leftVector.x - leftVector.y) * x;
            float leftYvalue = (leftVector.x + leftVector.y) * x;
            float upXvalue = (upVector.x - upVector.y) * y;
            float upYvalue = (upVector.x + upVector.y) * y;

            Vector2 worldPos = new Vector2(widthLength * (leftXvalue + upXvalue), 
            heightLength * (leftYvalue + upYvalue));

            return worldPos;
        }

        private static void SetBiomeInIsometric(Biome biome, IsometricPerspective perspective, 
        List<Grid[][]> world)
        {
            float max = Mathf.Max(biome.spriteWidth, biome.spriteHeight);
            float width = biome.spriteWidth / max; 
            float height = biome.spriteHeight / max;

            int column = world[0].Length;
            int row = world[0][0].Length;

            for(int z = 0; z < world.Count; z++)
            {
                for(int y = 0; y < column; y++)
                {
                    for(int x = 0; x < row; x++)
                    {
                        if(world[z][y][x] == null) continue;

                        Vector2 position;

                        if(z > 0)
                        {
                            Grid parent = BiomeGenerator.GetHighestGrid(world, x, y, z - 1);
                            position = parent.GetGameObject().transform.position +
                            new Vector3(0, 
                            (parent.GetEntity() as Terrain).gridAt * 
                            biome.spriteScale);
                        }
                        else
                        {
                            position = IsometricToWorldPosition(width, height, x, y, perspective) * 
                            biome.spriteScale;
                        }

                        GameObject gameObject = world[z][y][x].GetGameObject();

                        gameObject.transform.position = position;
                        gameObject.transform.localScale = new Vector3(biome.spriteScale, biome.spriteScale, 1);
                    }
                }
            }
        }

        private static void SortToIsometric(List<Grid[][]> world, IsometricPerspective perspective)
        {
            int maxOrder = 32767, minOrder = -32768;
            int height = world.Count;
            int column = world[0].Length;
            int row = world[0][0].Length;

            int sortingLayer = (height * column * row - 1 ) / (maxOrder + Mathf.Abs(minOrder));

            int order = Mathf.Min(height * column * row - 1, maxOrder);
            
            Vector2Int sign = perspective.currentPerspective.GetSortSign();

            for(int j = sign[0] > 0? 0 : column - 1; sign[0] > 0? j < column : j >= 0; j += sign[0]) 
            {
                for(int i = sign[1] > 0? 0 : row - 1; sign[1] > 0? i < row : i >= 0; i += sign[1])
                {
                    for(int k = height - 1; k >= 0; k--)
                    {
                        if(world[k][j][i] != null)
                        {
                            world[k][j][i].GetGameObject().GetComponent<SpriteRenderer>().sortingOrder = order;
                            world[k][j][i].GetGameObject().GetComponent<SpriteRenderer>().sortingLayerID = sortingLayer;
                            order--;

                            if(order < minOrder)
                            {
                                order = maxOrder;
                                sortingLayer--;
                            }
                        }
                    }
                }
            }
        }

        [System.Serializable]
        private class PerspectiveInfo
        {
            public string name;
            Vector2 leftVector;
            Vector2 upVector;
            Vector2Int sortSign;

            PerspectiveInfo previous;
            PerspectiveInfo next;

            protected internal PerspectiveInfo(string name, Vector2 leftVector, Vector2 upVector, Vector2Int sortSign)
            {   
                this.name = name;
                this.leftVector = leftVector;
                this.upVector = upVector;
                this.sortSign = sortSign;
            }

            protected internal void SetRelation(PerspectiveInfo previous, PerspectiveInfo next)
            {
                this.previous = previous;
                this.next = next;
            }

            protected internal PerspectiveInfo GetPreviousPerspective()
            {
                return previous;
            }

            protected internal PerspectiveInfo GetNextPerspective()
            {
                return next;
            }

            protected internal Vector2 GetLeftVector()
            {
                return leftVector;
            }

            protected internal Vector2 GetUpVector()
            {
                return upVector;
            }

            protected internal Vector2Int GetSortSign()
            {
                return sortSign;
            }
        }
    }
}