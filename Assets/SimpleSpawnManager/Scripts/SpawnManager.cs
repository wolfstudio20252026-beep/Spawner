using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages random object spawning within customizable positions, rotations, and performance limits.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct MinMaxRange
    {
        public float min;
        public float max;
    }

    [System.Serializable]
    public struct SpawnPosition
    {
        public MinMaxRange x;
        public MinMaxRange y;
        public MinMaxRange z;
    }

    [System.Serializable]
    public struct SpawnRotation
    {
        public MinMaxRange x;
        public MinMaxRange y;
        public MinMaxRange z;
    }

    [Header("<size=13><b><color=#FF5733>■ PREFABS SETTINGS</color></b></size>")]
    [Tooltip("List of prefabs available for random spawning.")]
    [SerializeField] private List<GameObject> prefabs;

    [Header("<size=13><b><color=#33FF57>■ SPAWN ZONE</color></b></size>")]
    [Tooltip("Minimum and maximum boundaries for spawning on X, Y, and Z axes.")]
    [SerializeField]
    private SpawnPosition position = new SpawnPosition
    {
        x = new MinMaxRange { min = -10f, max = 10f },
        y = new MinMaxRange { min = -10f, max = 10f },
        z = new MinMaxRange { min = -10f, max = 10f }
    };

    [Tooltip("Minimum and maximum rotation values (Euler angles) applied upon spawning.")]
    [SerializeField]
    private SpawnRotation rotation = new SpawnRotation
    {
        x = new MinMaxRange { min = 0f, max = 360f },
        y = new MinMaxRange { min = 0f, max = 360f },
        z = new MinMaxRange { min = 0f, max = 360f }
    };

    [Header("<size=13><b><color=#3357FF>■ TIMING & HIERARCHY</color></b></size>")]
    [Tooltip("Delay between spawn iterations in seconds.")]
    [SerializeField] private float spawnDelay = 1f;
    [Tooltip("Optional parent transform to organize spawned instances in the hierarchy.")]
    [SerializeField] private Transform parentContainer;

    [Header("<size=13><b><color=#F1C40F>■ PERFORMANCE LIMITS</color></b></size>")]
    [Tooltip("If enabled, spawned objects will automatically be destroyed after a certain time.")]
    [SerializeField] private bool useDestroyTime = true;
    [Tooltip("Lifetime of spawned objects in seconds.")]
    [SerializeField] private float destroyTime = 5f;
    [Space]
    [Tooltip("If enabled, restricts the total number of concurrently active spawned objects.")]
    [SerializeField] private bool useMaxCount = false;
    [Tooltip("Maximum allowed active objects on the scene.")]
    [SerializeField] private int maxObjectsCount = 20;

    [Header("<size=13><b><color=#FF3333>■ ACTIONS</color></b></size>")]
    [Tooltip("Click this toggle to instantly restore all default configurations.")]
    [SerializeField] private bool clickToResetAll;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void OnValidate()
    {
        // Intercepts the inspector toggle to trigger a parameters reset in edit mode.
        if (clickToResetAll)
        {
            clickToResetAll = false;
            ResetToDefault();
        }
    }

    private void Start() => StartCoroutine(SpawnRoutine());

    /// <summary>
    /// Infinite loop handling timed initialization and instantiation of prefabs.
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        // Safe check to avoid runtime null-reference exceptions if the prefab list is missing.
        if (prefabs == null || prefabs.Count == 0) yield break;

        while (true)
        {
            // Enforces the population cap if the corresponding performance limit is active.
            if (useMaxCount)
            {
                spawnedObjects.RemoveAll(item => item == null);
                if (spawnedObjects.Count >= maxObjectsCount)
                {
                    yield return new WaitForSeconds(spawnDelay);
                    continue;
                }
            }

            // Picks a random object from the user-defined list.
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];

            // Calculates a randomized coordinate offset.
            Vector3 spawnPos = new Vector3(
                Random.Range(position.x.min, position.x.max),
                Random.Range(position.y.min, position.y.max),
                Random.Range(position.z.min, position.z.max)
            );

            // Converts randomized Euler angles into a valid mathematical Quaternion rotation.
            Quaternion spawnRot = Quaternion.Euler(
                Random.Range(rotation.x.min, rotation.x.max),
                Random.Range(rotation.y.min, rotation.y.max),
                Random.Range(rotation.z.min, rotation.z.max)
            );

            // Handles instant placement either directly into the root or onto a custom container.
            GameObject newObj = parentContainer == null
                ? Instantiate(prefab, spawnPos, spawnRot)
                : Instantiate(prefab, spawnPos, spawnRot, parentContainer);

            // Registers performance constraints to the newly created instance.
            if (useMaxCount) spawnedObjects.Add(newObj);
            if (useDestroyTime) Destroy(newObj, destroyTime);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    /// <summary>
    /// Reverts all inspector values to their balanced initial states.
    /// </summary>
    private void ResetToDefault()
    {
        position = new SpawnPosition
        {
            x = new MinMaxRange { min = -10f, max = 10f },
            y = new MinMaxRange { min = -10f, max = 10f },
            z = new MinMaxRange { min = -10f, max = 10f }
        };

        rotation = new SpawnRotation
        {
            x = new MinMaxRange { min = 0f, max = 360f },
            y = new MinMaxRange { min = 0f, max = 360f },
            z = new MinMaxRange { min = 0f, max = 360f }
        };

        spawnDelay = 1f;
        parentContainer = null;
        useDestroyTime = true;
        destroyTime = 5f;
        useMaxCount = false;
        maxObjectsCount = 20;
    }
}
