using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

public class fruit_spawner : MonoBehaviour
{
    [SerializeField] private fruit fruitToObserve;

    [SerializeField] private fruit fruitPrefab;

    private IObjectPool<fruit> objectPool;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 50;

    private void OnThingHappened()
    {
        GrowFruit();
    }

    private void Awake()
    {
        if (fruitToObserve != null)
        {
            fruitToObserve.hasBeenPicked += OnThingHappened;
        }
       
        objectPool = new UnityEngine.Pool.ObjectPool<fruit>(SpawnFruit,
        OnGetFromPool, OnReleaseToPool,
        OnDestroyPooledObject, collectionCheck, defaultCapacity, maxSize);
       
    }

    private fruit SpawnFruit()
    {
        fruit fruitInstance = Instantiate(fruitPrefab);
        fruitInstance.ObjectPool = objectPool;
        return fruitInstance;
    }

    private void OnReleaseToPool(fruit pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
    }

    // Invoked when retrieving the next item from the object pool
    private void OnGetFromPool(fruit pooledObject)
    {
        pooledObject.gameObject.SetActive(true);
    }

    // Invoked when we exceed the maximum number of pooled items (i.e. destroy the pooled object)
    private void OnDestroyPooledObject(fruit pooledObject)
    {
        Destroy(pooledObject.gameObject);
    }


    private void OnDestroy()
    {
        if (fruitToObserve != null)
        {
            fruitToObserve.hasBeenPicked -= OnThingHappened;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void GrowFruit()
    {
        fruit spawnedFruit = objectPool.Get();

        if (spawnedFruit == null)
            return;

        spawnedFruit.transform.SetPositionAndRotation(transform.position, transform.rotation);
        fruitToObserve.hasBeenPicked -= OnThingHappened;
        fruitToObserve = spawnedFruit;
        fruitToObserve.hasBeenPicked += OnThingHappened;
    }
    
}
