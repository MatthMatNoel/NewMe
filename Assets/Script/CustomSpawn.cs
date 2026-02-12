using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit
{
    /// <summary>
    /// Enhanced spawn system based on FindSpawnPositions with additional customization options.
    /// Allows spawning multiple different objects with individual spawn counts, custom rotation,
    /// and player exclusion zones.
    /// </summary>
    public class CustomSpawn : MonoBehaviour
    {
        /// <summary>
        /// Data structure to hold information about each spawnable object
        /// </summary>
        [System.Serializable]
        public class SpawnObjectData
        {
            [Tooltip("The prefab or object to spawn")]
            public GameObject Prefab;
            
            [Tooltip("How many instances of this object to spawn")]
            public int SpawnCount = 1;
            
            [Tooltip("Enable custom rotation for this object")]
            public bool UseCustomRotation = false;
            
            [Tooltip("Minimum rotation angles (X, Y, Z)")]
            public Vector3 MinRotation = Vector3.zero;
            
            [Tooltip("Maximum rotation angles (X, Y, Z)")]
            public Vector3 MaxRotation = Vector3.zero;
        }

        /// <summary>
        /// When the scene data is loaded, this controls what room(s) the prefabs will spawn in.
        /// </summary>
        [Tooltip("When the scene data is loaded, this controls what room(s) the prefabs will spawn in.")]
        public MRUK.RoomFilter SpawnOnStart = MRUK.RoomFilter.CurrentRoomOnly;

        /// <summary>
        /// List of objects to spawn with their individual settings
        /// </summary>
        [SerializeField, Tooltip("List of objects to spawn with their individual settings")]
        public List<SpawnObjectData> SpawnObjects = new List<SpawnObjectData>();

        /// <summary>
        /// Randomly pick between different spawn objects
        /// </summary>
        [SerializeField, Tooltip("When enabled, randomly picks between spawn objects instead of spawning all of them")]
        public bool RandomSelection = false;

        /// <summary>
        /// Number of random objects to spawn when RandomSelection is enabled
        /// </summary>
        [SerializeField, Tooltip("Number of random objects to spawn when RandomSelection is enabled")]
        public int RandomSelectionCount = 5;

        /// <summary>
        /// Maximum number of times to attempt spawning/moving an object before giving up.
        /// </summary>
        [SerializeField, Tooltip("Maximum number of times to attempt spawning/moving an object before giving up.")]
        public int MaxIterations = 1000;

        /// <summary>
        /// Defines possible locations where objects can be spawned.
        /// </summary>
        public enum SpawnLocation
        {
            /// Spawn somewhere floating in the free space within the room
            Floating,
            /// Spawn on any surface (i.e. a combination of all 3 options below)
            AnySurface,
            /// Spawn only on vertical surfaces such as walls, windows, wall art, doors, etc...
            VerticalSurfaces,
            /// Spawn on surfaces facing upwards such as ground, top of tables, beds, couches, etc...
            OnTopOfSurfaces,
            /// Spawn on surfaces facing downwards such as the ceiling
            HangingDown
        }

        /// <summary>
        /// Attach content to scene surfaces.
        /// </summary>
        [SerializeField, Tooltip("Attach content to scene surfaces.")]
        public SpawnLocation SpawnLocations = SpawnLocation.Floating;

        /// <summary>
        /// When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.
        /// </summary>
        [SerializeField, Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
        public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;

        /// <summary>
        /// If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.
        /// </summary>
        [SerializeField, Tooltip("If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.")]
        public bool CheckOverlaps = true;

        /// <summary>
        /// Required free space for the object (Set negative to auto-detect using GetPrefabBounds)
        /// default to auto-detect. This value represents the extents of the bounding box
        /// </summary>
        [SerializeField, Tooltip("Required free space for the object (Set negative to auto-detect using GetPrefabBounds)")]
        public float OverrideBounds = -1;

        /// <summary>
        /// Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.
        /// </summary>
        [SerializeField, Tooltip("Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.")]
        public LayerMask LayerMask = -1;

        /// <summary>
        /// The clearance distance required in front of the surface in order for it to be considered a valid spawn position
        /// </summary>
        [SerializeField, Tooltip("The clearance distance required in front of the surface in order for it to be considered a valid spawn position")]
        public float SurfaceClearanceDistance = 0.1f;

        /// <summary>
        /// Enable player exclusion zone to prevent spawning near the player start position
        /// </summary>
        [SerializeField, Tooltip("Enable player exclusion zone to prevent spawning near the player start position")]
        public bool UsePlayerExclusionZone = false;

        /// <summary>
        /// Radius around the player start position where objects cannot spawn
        /// </summary>
        [SerializeField, Tooltip("Radius around the player start position where objects cannot spawn")]
        public float PlayerExclusionRadius = 2.0f;

        /// <summary>
        /// The transform representing the player's start position. If null, uses the camera's position.
        /// </summary>
        [SerializeField, Tooltip("The transform representing the player's start position. If null, uses the camera's position.")]
        public Transform PlayerStartTransform;

        private List<GameObject> _spawnedObjects = new();

        /// <summary>
        /// The list containing all the objects instantiated by this component
        /// </summary>
        public IReadOnlyList<GameObject> SpawnedObjects => _spawnedObjects;

        private Vector3 PlayerStartPosition
        {
            get
            {
                if (PlayerStartTransform != null)
                {
                    return PlayerStartTransform.position;
                }
                
                // Fallback to camera position
                if (Camera.main != null)
                {
                    return Camera.main.transform.position;
                }

                return Vector3.zero;
            }
        }

        private void Start()
        {
            if (MRUK.Instance && SpawnOnStart != MRUK.RoomFilter.None)
            {
                MRUK.Instance.RegisterSceneLoadedCallback(() =>
                {
                    switch (SpawnOnStart)
                    {
                        case MRUK.RoomFilter.AllRooms:
                            StartSpawn();
                            break;
                        case MRUK.RoomFilter.CurrentRoomOnly:
                            StartSpawn(MRUK.Instance.GetCurrentRoom());
                            break;
                    }
                });
            }
        }

        /// <summary>
        /// Starts the spawning process for all rooms.
        /// </summary>
        public void StartSpawn()
        {
            foreach (var room in MRUK.Instance.Rooms)
            {
                StartSpawn(room);
            }
        }

        /// <summary>
        /// Starts the spawning process for a specific room.
        /// </summary>
        /// <param name="room">The room to spawn objects in.</param>
        public void StartSpawn(MRUKRoom room)
        {
            if (SpawnObjects == null || SpawnObjects.Count == 0)
            {
                Debug.LogWarning("No spawn objects defined in CustomSpawn.");
                return;
            }

            if (RandomSelection)
            {
                SpawnRandomSelection(room);
            }
            else
            {
                SpawnAllObjects(room);
            }
        }

        /// <summary>
        /// Spawns all objects according to their individual spawn counts
        /// </summary>
        private void SpawnAllObjects(MRUKRoom room)
        {
            foreach (var spawnData in SpawnObjects)
            {
                if (spawnData.Prefab == null)
                {
                    Debug.LogWarning("Spawn object prefab is null, skipping.");
                    continue;
                }

                for (int i = 0; i < spawnData.SpawnCount; i++)
                {
                    TrySpawnObject(room, spawnData);
                }
            }
        }

        /// <summary>
        /// Randomly selects and spawns objects
        /// </summary>
        private void SpawnRandomSelection(MRUKRoom room)
        {
            var validObjects = new List<SpawnObjectData>();
            foreach (var obj in SpawnObjects)
            {
                if (obj.Prefab != null)
                {
                    validObjects.Add(obj);
                }
            }

            if (validObjects.Count == 0)
            {
                Debug.LogWarning("No valid spawn objects for random selection.");
                return;
            }

            for (int i = 0; i < RandomSelectionCount; i++)
            {
                var randomData = validObjects[Random.Range(0, validObjects.Count)];
                TrySpawnObject(room, randomData);
            }
        }

        /// <summary>
        /// Attempts to spawn a single object
        /// </summary>
        private void TrySpawnObject(MRUKRoom room, SpawnObjectData spawnData)
        {
            var prefabBounds = Utilities.GetPrefabBounds(spawnData.Prefab);
            var minRadius = 0.0f;
            const float clearanceDistance = 0.01f;
            var baseOffset = -prefabBounds?.min.y ?? 0.0f;
            var centerOffset = prefabBounds?.center.y ?? 0.0f;
            Bounds adjustedBounds = new();

            if (prefabBounds.HasValue)
            {
                minRadius = Mathf.Min(-prefabBounds.Value.min.x, -prefabBounds.Value.min.z, 
                    prefabBounds.Value.max.x, prefabBounds.Value.max.z);
                if (minRadius < 0f)
                {
                    minRadius = 0f;
                }

                var min = prefabBounds.Value.min;
                var max = prefabBounds.Value.max;
                min.y += clearanceDistance;
                if (max.y < min.y)
                {
                    max.y = min.y;
                }

                adjustedBounds.SetMinMax(min, max);
                if (OverrideBounds > 0)
                {
                    var center = new Vector3(0f, clearanceDistance, 0f);
                    var size = new Vector3(OverrideBounds * 2f, clearanceDistance * 2f, OverrideBounds * 2f);
                    adjustedBounds = new Bounds(center, size);
                }
            }

            bool foundValidSpawnPosition = false;
            
            for (int j = 0; j < MaxIterations; j++)
            {
                Vector3 spawnPosition = Vector3.zero;
                Vector3 spawnNormal = Vector3.zero;

                if (SpawnLocations == SpawnLocation.Floating)
                {
                    var randomPos = room.GenerateRandomPositionInRoom(minRadius, true);
                    if (!randomPos.HasValue)
                    {
                        break;
                    }

                    spawnPosition = randomPos.Value;
                }
                else
                {
                    MRUK.SurfaceType surfaceType = 0;
                    switch (SpawnLocations)
                    {
                        case SpawnLocation.AnySurface:
                            surfaceType |= MRUK.SurfaceType.FACING_UP;
                            surfaceType |= MRUK.SurfaceType.VERTICAL;
                            surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                            break;
                        case SpawnLocation.VerticalSurfaces:
                            surfaceType |= MRUK.SurfaceType.VERTICAL;
                            break;
                        case SpawnLocation.OnTopOfSurfaces:
                            surfaceType |= MRUK.SurfaceType.FACING_UP;
                            break;
                        case SpawnLocation.HangingDown:
                            surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                            break;
                    }

                    if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, 
                        new LabelFilter(Labels), out var pos, out var normal))
                    {
                        spawnPosition = pos + normal * baseOffset;
                        spawnNormal = normal;
                        var center = spawnPosition + normal * centerOffset;
                        
                        // Check if position is inside the room
                        if (!room.IsPositionInRoom(center))
                        {
                            continue;
                        }

                        // Check if position is inside a scene volume
                        if (room.IsPositionInSceneVolume(center))
                        {
                            continue;
                        }

                        // Check surface clearance
                        if (room.Raycast(new Ray(pos, normal), SurfaceClearanceDistance, out _))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                // Check player exclusion zone
                if (UsePlayerExclusionZone)
                {
                    Vector3 playerPos = PlayerStartPosition;
                    // Check horizontal distance (ignore Y)
                    Vector3 horizontalSpawnPos = new Vector3(spawnPosition.x, playerPos.y, spawnPosition.z);
                    float distanceToPlayer = Vector3.Distance(horizontalSpawnPos, playerPos);
                    
                    if (distanceToPlayer < PlayerExclusionRadius)
                    {
                        continue; // Too close to player, try again
                    }
                }

                // Calculate rotation
                Quaternion spawnRotation;
                if (spawnData.UseCustomRotation)
                {
                    // Generate random rotation within specified ranges
                    float randomX = Random.Range(spawnData.MinRotation.x, spawnData.MaxRotation.x);
                    float randomY = Random.Range(spawnData.MinRotation.y, spawnData.MaxRotation.y);
                    float randomZ = Random.Range(spawnData.MinRotation.z, spawnData.MaxRotation.z);
                    
                    // Combine with surface normal if applicable
                    Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
                    Quaternion customRotation = Quaternion.Euler(randomX, randomY, randomZ);
                    spawnRotation = surfaceRotation * customRotation;
                }
                else
                {
                    spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
                }

                // Check overlaps
                if (CheckOverlaps && prefabBounds.HasValue)
                {
                    if (Physics.CheckBox(spawnPosition + spawnRotation * adjustedBounds.center, 
                        adjustedBounds.extents, spawnRotation, LayerMask, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }
                }

                foundValidSpawnPosition = true;

                // Spawn the object
                if (spawnData.Prefab.scene.path == null)
                {
                    var item = Instantiate(spawnData.Prefab, spawnPosition, spawnRotation, transform);
                    _spawnedObjects.Add(item);
                }
                else
                {
                    spawnData.Prefab.transform.position = spawnPosition;
                    spawnData.Prefab.transform.rotation = spawnRotation;
                }

                break;
            }

            if (!foundValidSpawnPosition)
            {
                Debug.LogWarning($"Failed to find valid spawn position for {spawnData.Prefab.name} after {MaxIterations} iterations.");
            }
        }

        /// <summary>
        /// Destroys all the game objects instantiated and clears the SpawnedObjects list.
        /// </summary>
        public void ClearSpawnedPrefabs()
        {
            for (var i = _spawnedObjects.Count - 1; i >= 0; i--)
            {
                var spawnedObject = _spawnedObjects[i];
                if (spawnedObject)
                {
                    Destroy(spawnedObject);
                }
            }

            _spawnedObjects.Clear();
        }
    }
}
