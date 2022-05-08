using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Object pooling for performance -> Reduced garbage collection / memory allocation
public class RandomInstancePool {
    public Instance[] instances;
    protected GameObject rootGameObject;

    private int poolSize;

    /// Properties ///
    public GameObject RootGameObject { get { return rootGameObject; } }

    public int PoolSize { get { return poolSize; } }

    public Instance[] AllInstances { get { return instances; } }
    /// 


    public RandomInstancePool(GameObject[] prefabs, string rootObjectName, int _poolSize) {
        int countPerSample = Mathf.CeilToInt(((float)_poolSize) / prefabs.Length);
        poolSize = countPerSample * prefabs.Length;

        rootGameObject = new GameObject();
        rootGameObject.name = rootObjectName + " : Pool";

        instances = new Instance[poolSize];
        for (int i = 0; i < poolSize; i++) {
            instances[i] = new Instance();
        }

        // Pre spawn instances
        for (int i = 0; i < prefabs.Length; i++) {
            for (int j = 0; j < countPerSample; j++) {
                GameObject newInstance = GameObject.Instantiate(prefabs[i]) as GameObject;
                newInstance.transform.SetParent(rootGameObject.transform);
                instances[(i * countPerSample) + j].AssignGameObject(newInstance);
            }
        }
    }

    // Get random innactive instance
    public Instance GetInnactiveInstance() {
        List<Instance> innactiveInstances = new List<Instance>();
        foreach (Instance i in instances) {
            if (!i.Active)
                innactiveInstances.Add(i);
        }
        if (innactiveInstances.Count > 0) {
            return innactiveInstances[Random.Range(0, innactiveInstances.Count)];
        }
        return null;
    }


    
    public class Instance {
        private GameObject instance;
        private bool active;

        /// Properties ///
        public GameObject GOInstance { get { return instance; } }
        public bool Active { get { return active; } }
        /// 

        public Instance() {
            active = false;
        }

        public void AssignGameObject(GameObject newObject) {
            instance = newObject;
            instance.name = newObject.name + " : Instance";
            instance.SetActive(false);
        }

        public void SetActive(bool state) {
            instance.SetActive(state);
            active = state;
        }
    }
}