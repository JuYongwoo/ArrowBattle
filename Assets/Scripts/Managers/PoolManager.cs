// Path suggestion: Scripts/Pool/PoolManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    private Dictionary<GameObject, Queue<GameObject>> _pools = new();

    public void CleanPool() { _pools.Clear(); } //씬오브젝트의 Start() 내에서 실행 권장
    public void CreatePool(GameObject prefab, int initialSize = 1)
    {
        if (prefab == null) return;
        if (_pools.ContainsKey(prefab)) return;

        var q = new Queue<GameObject>();
        for (int i = 0; i < initialSize; i++)
        {
            var go = Object.Instantiate(prefab);
            go.SetActive(false);

            var pooled = go.GetComponent<PooledObject>();
            if (pooled == null) pooled = go.AddComponent<PooledObject>();
            pooled.SetOriginPrefab(prefab);

            q.Enqueue(go);
        }
        _pools[prefab] = q;
    }

    public GameObject Spawn(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var q))
        {
            CreatePool(prefab, 0);
            q = _pools[prefab];
        }

        GameObject instance = (q.Count > 0) ? q.Dequeue() : Object.Instantiate(prefab);
        instance.SetActive(true);

        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null) pooled = instance.AddComponent<PooledObject>();
        pooled.SetOriginPrefab(prefab);

        return instance;

    }

    public GameObject Spawn(GameObject prefab, Transform parent)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var q))
        {
            CreatePool(prefab, 0);
            q = _pools[prefab];
        }

        GameObject instance = (q.Count > 0) ? q.Dequeue() : Object.Instantiate(prefab);
        instance.transform.SetParent(parent, false);
        instance.SetActive(true);

        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null) pooled = instance.AddComponent<PooledObject>();
        pooled.SetOriginPrefab(prefab);

        return instance;

    }
    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var q))
        {
            CreatePool(prefab, 0);
            q = _pools[prefab];
        }

        GameObject instance = (q.Count > 0) ? q.Dequeue() : Object.Instantiate(prefab);
        instance.transform.SetPositionAndRotation(pos, rot);
        instance.SetActive(true);

        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null) pooled = instance.AddComponent<PooledObject>();
        pooled.SetOriginPrefab(prefab);

        return instance;
    }

    public void ReturnToPool(GameObject originPrefab, GameObject instance)
    {
        if (originPrefab == null || instance == null)
        {
            Object.Destroy(instance);
            return;
        }

        instance.transform.SetParent(null, false); //어딘가의 자식 오브젝트로 계속 존재하면 이벤트가 실행되는 경우 존재, 바깥으로 빼낸다.
        instance.SetActive(false);
        if (!_pools.TryGetValue(originPrefab, out var q))
            _pools[originPrefab] = q = new Queue<GameObject>();

        q.Enqueue(instance);
    }

    public void DestroyPooled(GameObject instance)
    {
        if (instance == null) return;

        var pooled = instance.GetComponent<PooledObject>();
        if (pooled != null && pooled.originPrefab != null)
            ReturnToPool(pooled.originPrefab, instance);
        else
            Object.Destroy(instance);
    }


}
public class PooledObject : MonoBehaviour
{
    [HideInInspector] public GameObject originPrefab;

    public void SetOriginPrefab(GameObject prefab) => originPrefab = prefab;

}
