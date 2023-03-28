using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Pool 
{
    public string tag;
    public ObjectPool prefab;
    public int size;
}

public class Pooler : Singleton<Pooler>
{
    public List<Pool> pools;
    public Dictionary<string, List<ObjectPool>> poolDictionary;
    private Dictionary<string, GameObject> m_PoolHandlers;

    protected virtual void Initialize()
    {
        poolDictionary = new Dictionary<string, List<ObjectPool>>();
        m_PoolHandlers = new Dictionary<string, GameObject>();
    }
    
    void Start()
    {
        poolDictionary = new Dictionary<string, List<ObjectPool>>();
        m_PoolHandlers = new Dictionary<string, GameObject>();
        FillPool();
    }

    public void FillPool()
    {
        foreach(Pool pool in pools)
        {
            List<ObjectPool> objectPool = new List<ObjectPool>();
            GameObject parent = new GameObject("pooler " + pool.tag);
            parent.transform.SetParent(gameObject.transform);

            for (int i = 0; i < pool.size; i++)
            {
                ObjectPool obj = Instantiate<ObjectPool>(pool.prefab);
                obj.poolName = pool.tag;
                obj.gameObject.name = pool.tag + i;
                obj.gameObject.SetActive(false);
                obj.gameObject.transform.SetParent(parent.transform);
                objectPool.Add(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            m_PoolHandlers.Add(pool.tag, parent);
        }
    }

    public void AttachObject(string tag, ObjectPool gameObject)
    {
        if(!poolDictionary.ContainsKey(tag)) return;
        gameObject.gameObject.transform.SetParent(m_PoolHandlers[tag].transform);
    }

    public GameObject GetObject(string tag) 
    {
        if(!poolDictionary.ContainsKey(tag)) return null;

        foreach (var objectPool in poolDictionary[tag])
        {
            if (!objectPool.gameObject.activeInHierarchy)
                return objectPool.gameObject;
        }

        return null;
    }

    public T GameObject<T>(string tag) where T : ObjectPool
    {
        if (!poolDictionary.ContainsKey(tag)) return null;

        foreach (var objectPool in poolDictionary[tag])
        {
            if (!objectPool.gameObject.activeInHierarchy && objectPool is T)
                return objectPool as T;
        }

        return null;
    }

    public virtual GameObject Spawn(string tag, Vector3 position, Transform parent = null)
    {
        GameObject obj = GetObject(tag);

        if (obj)
        {
            obj.SetActive(true);
            obj.transform.SetParent(parent);
            obj.transform.position = position;
        }

        return obj;
    }

    public virtual T Spawn<T>(string tag, Vector3 position, Transform parent = null) where T : ObjectPool
    {
        return Spawn(tag, position, parent)?.GetComponent<T>();
    }

    public virtual GameObject Spawn(string tag, Vector3 position, Quaternion rotation , Vector3 scale, Transform parent = null)
    {
        GameObject obj = GetObject(tag);

        if(obj)
        {
            obj.SetActive(true);
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = scale;
        }

        return obj;
    }

    public virtual T Spawn<T>(string tag, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null) where T : ObjectPool
    {
        return Spawn(tag, position, rotation, scale, parent)?.GetComponent<T>();
    }

    public void Recycle()
    {
        foreach (var pool in poolDictionary.Values)
        {
            foreach (var objPool in pool)
            {
                if (objPool.gameObject.activeInHierarchy)
                    objPool.Recycle();
            }
        }
    }
}