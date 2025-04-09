using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class fruit_spawner : MonoBehaviour
{
    [SerializeField] private fruit fruitToObserve;

    public GameObject attachedFruit;

    private void OnThingHappened()
    {
        // any logic that responds to event goes here
        Debug.Log("Spawn another fruit");
    }

    private void Awake()
    {
        if (fruitToObserve != null)
        {
            fruitToObserve.hasBeenPicked += OnThingHappened;
        }
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
        attachedFruit = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
           
    }
    
}
