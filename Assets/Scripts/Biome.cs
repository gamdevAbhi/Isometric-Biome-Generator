using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric
{
    [CreateAssetMenu(fileName = "Biome", menuName = "Isometric/Biome")]
    public class Biome : ScriptableObject
    {
        public int spriteWidth;
        public int spriteHeight;
        public float spriteScale;
        [Range(0.0f, 1.0f)]
        public float terrainNoiseFactor;
        [Range(0.0f, 1.0f)]
        public float entitiesNoiseFactor;
        public int mountainFactor;
        [Range(0, 100)]
        public int mountainProbablity;

        public Terrain[] terrains;
        public Entity[] entities;

        public void OnValidate()
        {
            foreach(Terrain terrain in terrains) terrain.Update(this);
        }
    }

    [System.Serializable]
    public class Entity
    {
        public string name;
        public Sprite sprite;
        [Range(1, 100)]
        public int probablity;
        [Range(1, 100)]
        public int positiveBias;
        [Range(1, 100)]
        public int negetiveBias;
    }

    [System.Serializable]
    public class Terrain : Entity
    {
        public float gridAt;
        public List<Tick> terrainBehaviour;
        public List<Tick> entitiesBehaviour;

        public void Update(Biome biome)
        {
            terrainBehaviour = ChangeTick(terrainBehaviour, biome.terrains);
            entitiesBehaviour = ChangeTick(entitiesBehaviour, biome.entities);
        }

        private List<Tick> ChangeTick(List<Tick> list, Entity[] entities)
        {
            List<Tick> newList = new List<Tick>();
            
            for(int i = 0; i < entities.Length; i++)
            {
                if(i >= list.Count || list[i].name != entities[i].name) newList.Add(new Tick(entities[i].name));
                else newList.Add(list[i]);
            }

            return newList;
        }

        public bool IsEntityAllowed(Entity entity)
        {
            foreach(Tick tick in terrainBehaviour) if(tick.name == entity.name) return tick.isAllowed;
            foreach(Tick tick in entitiesBehaviour) if(tick.name == entity.name) return tick.isAllowed;
            Debug.Log(entity.name);
            return false;
        }
    }

    [System.Serializable]
    public class Tick
    {
        public string name;
        public bool isAllowed = false;

        public Tick(string name)
        {
            this.name = name;
        }
    }
}
