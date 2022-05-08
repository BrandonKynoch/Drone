using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Object pooling for performance -> Reduced garbage collection / memory allocation
public class InstancePool {
    public GameObject poolPrefab;

    public Instance[] instances;
    protected GameObject rootGameObject;

    private int poolSize;

    /// Properties ///
    public int PoolSize { get { return poolSize; } }

    public int SpawnedCount {
        get {
            int count = 0;
            foreach (Instance i in instances) {
                if (i.Assigned)
                    count++;
            }
            return count;
        }
    }
    /// 


    public InstancePool(GameObject _poolPrefab, string rootObjectName, int _poolSize) {
        poolPrefab = _poolPrefab;
        poolSize = _poolSize;

        rootGameObject = new GameObject();
        rootGameObject.name = rootObjectName + " : Pool";

        instances = new Instance[poolSize];
        for (int i = 0; i < poolSize; i++) {
            instances[i] = new Instance();
        }
    }

    public Instance GetInnactiveInstance() {
        foreach (Instance i in instances) {
            if (i.Assigned && !i.Active)
                return i;
        }
        // Not enough instances --> spawn and return new instance
        foreach (Instance i in instances) {
            if (!i.Assigned) {
                GameObject newInstance = GameObject.Instantiate(poolPrefab) as GameObject;
                newInstance.transform.SetParent(rootGameObject.transform);
                i.AssignGameObject(newInstance);
                return i;
            }
        }
        return null;
    }



    public class Instance {
        private GameObject instance;
        private bool active;
        private bool assigned;

        /// Properties ///
        public GameObject GOInstance { get { return instance; } }
        public bool Active { get { return active; } }
        public bool Assigned { get { return assigned; } }
        /// 

        public Instance() {
            active = false;
            assigned = false;
        }

        public void AssignGameObject(GameObject newObject) {
            instance = newObject;
            instance.name = newObject.name + " : Instance";
            instance.SetActive(false);
            assigned = true;
        }

        public void SetActive(bool state) {
            if (!assigned)
                return;

            instance.SetActive(state);
            active = state;
        }
    }
}