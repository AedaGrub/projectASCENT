using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class objectPoolManager : MonoBehaviour
{
    public static List<pooledObjectInfo> objectPools = new List<pooledObjectInfo>();

    private GameObject objectPoolEmptyHolder;

    private static GameObject particleSystemsEmpty;
    private static GameObject gameObjectsEmpty;

    public enum PoolType
    {
        ParticleSystem,
        GameObject,
        None
    }
    public static PoolType PoolingType;

    private void Awake()
    {
        SetupEmpties();
    }

    private void SetupEmpties()
    {
        objectPoolEmptyHolder = new GameObject("Pooled Objects");

        particleSystemsEmpty = new GameObject("Particle Effects");
        particleSystemsEmpty.transform.SetParent(objectPoolEmptyHolder.transform);

        gameObjectsEmpty = new GameObject("GameObjects");
        gameObjectsEmpty.transform.SetParent(objectPoolEmptyHolder.transform);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRot, PoolType poolType = PoolType.None)
    {
        pooledObjectInfo pool = objectPools.Find(p => p.lookupString == objectToSpawn.name);

        //IF POOL DOESN'T EXIST, CREATE IT
        if (pool == null)
        {
            pool = new pooledObjectInfo() { lookupString = objectToSpawn.name };
            objectPools.Add(pool);
        }

        //CHECK IF ANY INACTIVATE USABLE OBJECTS IN POOL
        GameObject spawnableObject = pool.inactiveObjects.FirstOrDefault();

        if (spawnableObject == null)
        {
            //FIND PARENT OF EMPTY OBJECT
            GameObject parentObject = SetParentObject(poolType);

            //IF NO INACTIVE OBJECTS, CREATE A NEW ONE
            spawnableObject = Instantiate(objectToSpawn, spawnPos, spawnRot);

            if (parentObject != null)
            {
                spawnableObject.transform.SetParent(parentObject.transform);
            }
        }
        else
        {
            //IF THERE IS AN INACTIVE OBJECT, REACTIVE IT
            spawnableObject.transform.position = spawnPos;
            spawnableObject.transform.rotation = spawnRot;
            pool.inactiveObjects.Remove(spawnableObject);
            spawnableObject.SetActive(true);
        }

        return spawnableObject;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        //REMOVE "(clone)" FROM GAMEOBJECT
        string goName = obj.name.Substring(0, obj.name.Length - 7);

        pooledObjectInfo pool = objectPools.Find(p => p.lookupString == goName);

        if (pool == null)
        {
            Debug.LogWarning("TRYING TO RELEASE AN OBJECT THAT IS NOT POOLED: " + obj.name);
        }
        else
        {
            obj.SetActive(false);
            pool.inactiveObjects.Add(obj);
        }
    }

    private static GameObject SetParentObject(PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.ParticleSystem: 
                return particleSystemsEmpty;

            case PoolType.GameObject: 
                return gameObjectsEmpty;

            case PoolType.None: 
                return null;

            default:
                return null;
        }
    }
}

public class pooledObjectInfo
{
    public string lookupString;
    public List<GameObject> inactiveObjects = new List<GameObject> ();
}