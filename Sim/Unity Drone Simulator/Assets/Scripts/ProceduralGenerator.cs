using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerator : MonoBehaviour {
    public GameObject[] spawnPiecePrefabs;

    private GameObject[] instances;

    private bool[,] instanceBitMap = new bool[GRID_DIMENSION, GRID_DIMENSION];

    private List<Vector3> availableSpawnPositions = new List<Vector3>();

    /// Static ///
    private static ProceduralGenerator staticInstance;
    public static ProceduralGenerator StaticInstance {
        get { return staticInstance; }
    }
    /// Static ///

    /// Constants ///
    private const int GRID_DIMENSION = 5;
    private const float GRID_CELL_SIZE = 7;

    private const int TOTAL_INSTANCE_COUNT = 50;
    private const int ACTIVE_INSTANCE_COUNT = 23;
    /// Constants ///

    private void Start() {
        staticInstance = this;

        SpawnInstances();

        CreateMap();
    }

    private void SpawnInstances() {
        instances = new GameObject[TOTAL_INSTANCE_COUNT];
        for (int i = 0; i < TOTAL_INSTANCE_COUNT; i++) {
            GameObject go = GameObject.Instantiate(spawnPiecePrefabs[Random.Range(0, spawnPiecePrefabs.Length)]);
            go.transform.SetParent(transform);
            go.SetActive(false);
            instances[i] = go;
        }
    }

    public void CreateMap() {
        DestroyMap();

        List<Coordinate> availablePositions = GetInactivePositions();
        List<GameObject> availableInstances = GetInactiveInstances();

        Vector3 bottomLeft = new Vector3(
                -((GRID_CELL_SIZE * GRID_DIMENSION) * 0.5f),
                0,
                -((GRID_CELL_SIZE * GRID_DIMENSION) * 0.5f));

        for (int i = 0; i < ACTIVE_INSTANCE_COUNT; i++) {
            int targetCoordIndex = Random.Range(0, availablePositions.Count);
            Coordinate targetCoord = availablePositions[targetCoordIndex];
            availablePositions.RemoveAt(targetCoordIndex);

            Vector3 targetPosition = bottomLeft + new Vector3(
                    targetCoord.x * GRID_CELL_SIZE,
                    0f,
                    targetCoord.y * GRID_CELL_SIZE);

            int targetInstanceIndex = Random.Range(0, availableInstances.Count);
            GameObject targetInstance = availableInstances[i];
            availableInstances.RemoveAt(targetInstanceIndex);

            targetInstance.SetActive(true);
            targetInstance.transform.position = targetPosition;
            targetInstance.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f);

            Transform spawnRoot = targetInstance.transform.Find("Spawn");
            foreach (Transform t in spawnRoot.GetComponentsInChildren<Transform>()) {
                if (t == spawnRoot) {
                    continue;
                }

                availableSpawnPositions.Add(t.position);
            }
        }
    }

    private void DestroyMap() {
        foreach (GameObject go in instances) {
            go.SetActive(false);
        }
        availableSpawnPositions.Clear();
    }

    private List<Coordinate> GetInactivePositions() {
        List<Coordinate> output = new List<Coordinate>();
        for (int x = 0; x < instanceBitMap.GetLength(0); x++) {
            for (int y = 0; y < instanceBitMap.GetLength(1); y++) {
                if (!instanceBitMap[x, y]) {
                    output.Add(new Coordinate(x, y));
                }
            }
        }
        return output;
    }

    private List<GameObject> GetInactiveInstances() {
        List<GameObject> output = new List<GameObject>();
        foreach (GameObject go in instances) {
            if (!go.activeSelf) {
                output.Add(go);
            }
        }
        return output;
    }

    public static Vector3 GetRandomSpawnPosition() {
        int spawnIndex = Random.Range(0, staticInstance.availableSpawnPositions.Count);
        Vector3 spawnPos = staticInstance.availableSpawnPositions[spawnIndex];
        staticInstance.availableSpawnPositions.RemoveAt(spawnIndex);
        return spawnPos;
    }
}
