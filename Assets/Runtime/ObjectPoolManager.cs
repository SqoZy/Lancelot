using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace Miner.Assets.Runtime.ObjectPooling
{
    public class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField] private bool addToDontDestroyOnLoad = false; //for music and sound effects, if we want to keep them alive across scenes

        private GameObject emptyHolder;

        // Static references to empty GameObjects for organizing pooled objects, you can add more to organize different types of objects if needed
        private static GameObject particleSystemEmpty;
        private static GameObject gameObjectEmpty;
        private static GameObject enemyEmpty;
        private static GameObject soundFXEmpty;

        private static Dictionary<GameObject, ObjectPool<GameObject>> objectPools;
        private static Dictionary<GameObject, GameObject> cloneToPrefabMap;

        public enum EPoolType
        {
            ParticleSystem,
            GameObject,
            Enemy,
            SoundFX
        }

        public static EPoolType PoolingType;

        private void Awake()
        {
            objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
            cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

            SetupEmptyHolders();
        }

        private void SetupEmptyHolders()
        {
            emptyHolder = new GameObject("Object Pools");

            particleSystemEmpty = new GameObject("Particle Effects");
            particleSystemEmpty.transform.SetParent(emptyHolder.transform);

            gameObjectEmpty = new GameObject("Game Objects");
            gameObjectEmpty.transform.SetParent(emptyHolder.transform);

            enemyEmpty = new GameObject("Enemies");
            enemyEmpty.transform.SetParent(emptyHolder.transform);

            soundFXEmpty = new GameObject("Sound FX");
            soundFXEmpty.transform.SetParent(emptyHolder.transform);

            if (addToDontDestroyOnLoad)
                DontDestroyOnLoad(particleSystemEmpty.transform.root);
        }

        private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, EPoolType poolType = EPoolType.GameObject) // default pooltype is GameObject
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateObject(prefab, pos, rot, poolType),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: OnDestroyObject
            );

            objectPools.Add(prefab, pool);
        }

        private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, EPoolType poolType = EPoolType.GameObject)
        {
            prefab.SetActive(false);

            GameObject obj = Instantiate(prefab, pos, rot);

            prefab.SetActive(true);

            GameObject parentObject = SetParentObject(poolType);
            obj.transform.SetParent(parentObject.transform);

            return obj;
        }

        private static void OnGetObject(GameObject obj)
        {
            // optional logic when an object gets pulled from the pool not needed for now
        }

        private static void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void OnDestroyObject(GameObject obj)
        {
            cloneToPrefabMap.Remove(obj);
        }

        private static GameObject SetParentObject(EPoolType poolType)
        {
            switch (poolType)
            {
                case EPoolType.ParticleSystem:
                    return particleSystemEmpty;
                case EPoolType.GameObject:
                    return gameObjectEmpty;
                case EPoolType.Enemy:
                    return enemyEmpty;
                case EPoolType.SoundFX:
                    return soundFXEmpty;
                default:
                    Debug.LogError("Invalid pool type specified.");
                    return null;
            }
        }

        private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRot, EPoolType poolType = EPoolType.GameObject) where T : Object
        {
            if (!objectPools.ContainsKey(objectToSpawn))
                CreatePool(objectToSpawn, spawnPos, spawnRot, poolType);

            GameObject obj = objectPools[objectToSpawn].Get();

            if (obj != null)
            {
                cloneToPrefabMap.TryAdd(obj, objectToSpawn);

                obj.transform.position = spawnPos;
                obj.transform.rotation = spawnRot;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                    return obj as T;

                T component = obj.GetComponent<T>();
                if (component == null) { Debug.LogError($"The spawned object does not have a component of type {typeof(T).Name}"); return null; }

                return component;
            }
            return null;
        }

        public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRot, EPoolType poolType = EPoolType.GameObject) where T : Component // Load any component type from the prefab, not just GameObjects
        {
            return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRot, poolType);
        }

        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRot, EPoolType poolType = EPoolType.GameObject) // Load a GameObject from the prefab
        {
            return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRot, poolType);
        }

        public static void ReturnObjectToPool(GameObject obj, EPoolType poolType = EPoolType.GameObject)
        {
            if (cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
            {
                GameObject parentObject = SetParentObject(poolType);

                if (obj.transform.parent != parentObject.transform)
                    obj.transform.SetParent(parentObject.transform);
                if (objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
                    pool.Release(obj);
            }
            else
                Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name);
        }
    }
}
