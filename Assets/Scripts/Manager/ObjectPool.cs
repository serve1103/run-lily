using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
    }

    public List<Pool> pools;
    Dictionary<string, Queue<GameObject>> poolDict;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        poolDict = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDict[pool.tag] = queue;
        }
    }

    public GameObject Get(string tag, Vector3 position)
    {
        if (!poolDict.ContainsKey(tag)) return null;

        Queue<GameObject> queue = poolDict[tag];

        // 풀에 남은 게 없으면 새로 생성
        GameObject obj;
        if (queue.Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            obj = Instantiate(pool.prefab, transform);
        }
        else
        {
            obj = queue.Dequeue();
        }

        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void Return(string tag, GameObject obj)
    {
        obj.SetActive(false);
        if (poolDict.ContainsKey(tag))
        {
            poolDict[tag].Enqueue(obj);
        }
    }
}
