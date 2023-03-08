using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    public ObjectPool<GameObject> _shotPool;

    private GameObject _shot = default;

    void Awake()
    {
        _shotPool = new ObjectPool<GameObject>(InstantiateObjects, GetObjectFromPool, ReleaseObjectToPool);
    }

    
    private GameObject InstantiateObjects()
    {
        Instantiate(_shot,this.gameObject.transform);
        return _shot;
    }

    private void GetObjectFromPool(GameObject _shot)
    {
        _shot.SetActive(true);
    }

    public void ReleaseObjectToPool(GameObject _shot)
    {
        _shot.SetActive(false);
    }

    public GameObject GetGameObject(GameObject _shotPrefab, Vector2 position, Quaternion quaternion)
    {
        _shot = _shotPrefab;
        GameObject _shotObj = _shotPool.Get();
        Transform transform = _shotObj.transform;
        transform.position = position;
        transform.rotation = quaternion;

        return _shotObj;
    }

    void Shot()
    {

    }
}
