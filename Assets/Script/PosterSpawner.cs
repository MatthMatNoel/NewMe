using UnityEngine;
using System.Collections.Generic;

public class PosterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PosterItem
    {
        [Tooltip("Sprite de l'affiche — glisse directement depuis ton projet pour l'assigner")]
        public Sprite sprite;

        [HideInInspector]
        public bool used = false;
    }

    [Header("Poster Assets")]
    [Tooltip("Tu peux glisser ici jusqu'à plusieurs sprites d'affiche pour les utiliser directement")]
    public PosterItem[] posterItems;

    [Header("Poster Settings")]
    [Tooltip("Dossier contenant les affiches (dans Resources)")]
    public string posterFolderPath = "affiche2d";

    [Tooltip("Tag des murs où placer les affiches")]
    public string wallTag = "Wall";

    [Tooltip("Nombre d'affiches à placer par mur")]
    public int postersPerWall = 6;

    [Tooltip("Taille des affiches")]
    public Vector2 posterSize = new Vector2(2f, 3f);

    [Tooltip("Marges pour éviter les bords")]
    public float margin = 0.5f;

    void Start()
    {
        SpawnPosters();
    }

    void SpawnPosters()
    {
        // Rassembler la liste de sprites disponibles :
        // - On privilégie les sprites assignés dans l'inspector via `posterItems`
        // - Sinon on charge depuis Resources/posterFolderPath
        Sprite[] availablePosters = null;

        if (posterItems != null && posterItems.Length > 0)
        {
            var list = new System.Collections.Generic.List<Sprite>();
            foreach (var pi in posterItems)
            {
                if (pi != null && pi.sprite != null) list.Add(pi.sprite);
            }

            if (list.Count > 0) availablePosters = list.ToArray();
        }

        if (availablePosters == null || availablePosters.Length == 0)
        {
            availablePosters = Resources.LoadAll<Sprite>(posterFolderPath);
        }

        if (availablePosters == null || availablePosters.Length == 0)
        {
            Debug.LogWarning($"Aucune affiche trouvée ni dans les PosterItems ni dans Resources/{posterFolderPath}");
            return;
        }

        Debug.Log($"Trouvé {availablePosters.Length} affiche(s) disponibles");

        // Si des positions de spawn ont été placées dans la scène (taggées "SpawnPoint"),
        // on les utilise en priorité (workflow level-design). Sinon on retombe sur
        // la génération procédurale sur murs / sols.
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Debug.Log($"Trouvé {spawnPoints.Length} SpawnPoint(s) — utilisation des positions placées.");
            foreach (var sp in spawnPoints)
            {
                if (sp == null) continue;
                Sprite randomPoster = availablePosters[Random.Range(0, availablePosters.Length)];
                Quaternion rot = sp.transform.rotation;
                Transform parent = sp.transform.parent != null ? sp.transform.parent : null;
                CreatePoster(randomPoster, sp.transform.position, parent, rot);
            }

            // On a utilisé les spawn points, on s'arrête là.
            return;
        }

        // Trouver tous les murs
        GameObject[] walls = GameObject.FindGameObjectsWithTag(wallTag);

        if (walls.Length == 0)
        {
            Debug.LogWarning($"Aucun objet trouvé avec le tag '{wallTag}'");
            return;
        }

        Debug.Log($"Trouvé {walls.Length} mur(s)");

        // Placer les affiches sur chaque mur
        foreach (GameObject wall in walls)
        {
            Collider wallCollider = wall.GetComponent<Collider>();

            for (int i = 0; i < postersPerWall; i++)
            {
                Sprite randomPoster = posters[Random.Range(0, posters.Length)];
                Vector3 randomPosition = GetRandomPositionOnWall(wall, wallCollider);
                Quaternion rotation = GetWallPosterRotation(wall);
                CreatePoster(randomPoster, randomPosition, wall.transform, rotation);
            }
        }

        // (No ground spawning — posters are only placed on walls or SpawnPoints)
    }

    Vector3 GetRandomPositionOnWall(GameObject wall, Collider wallCollider)
    {
        if (wallCollider == null)
        {
            Debug.LogWarning($"Le mur {wall.name} n'a pas de Collider");
            return wall.transform.position;
        }

        Bounds bounds = wallCollider.bounds;

        // Générer un point aléatoire à l'intérieur des bounds, puis projeter sur la surface
        float rx = Random.Range(bounds.min.x + margin, bounds.max.x - margin);
        float ry = Random.Range(bounds.min.y + margin, bounds.max.y - margin);
        float rz = Random.Range(bounds.min.z + margin, bounds.max.z - margin);

        Vector3 samplePoint = new Vector3(rx, ry, rz);

        // ClosestPoint renvoie le point le plus proche sur la surface du collider
        Vector3 surfacePoint = wallCollider.ClosestPoint(samplePoint);

        // Décaler légèrement vers l'extérieur pour éviter le z-fighting
        Vector3 outward = wall.transform.forward.normalized;
        surfacePoint += outward * 0.01f;

        return surfacePoint;
    }

    Vector3 GetRandomPositionOnGround(GameObject ground, Collider groundCollider)
    {
        if (groundCollider == null)
        {
            Debug.LogWarning($"La surface {ground.name} n'a pas de Collider");
            return ground.transform.position;
        }

        Bounds bounds = groundCollider.bounds;

        float rx = Random.Range(bounds.min.x + margin, bounds.max.x - margin);
        float rz = Random.Range(bounds.min.z + margin, bounds.max.z - margin);

        // Position au-dessus de la surface (y = top of collider + half poster height)
        float y = bounds.max.y + (posterSize.y * 0.5f);

        return new Vector3(rx, y, rz);
    }

    Quaternion GetWallPosterRotation(GameObject wall)
    {
        // Poster should face away from the wall surface and be upright (world up)
        Vector3 normal = wall.transform.forward;
        return Quaternion.LookRotation(normal, Vector3.up);
    }

    Quaternion GetGroundPosterRotation(GameObject ground)
    {
        // Upright poster on ground: up = world up, random Y rotation so they face different directions
        float yRot = Random.Range(0f, 360f);
        return Quaternion.Euler(0f, yRot, 0f);
    }

    void CreatePoster(Sprite sprite, Vector3 position, Transform parent, Quaternion rotation)
    {
        // Créer un objet pour l'affiche
        GameObject posterObj = new GameObject($"Poster_{sprite.name}");
        posterObj.transform.position = position;
        posterObj.transform.rotation = rotation;
        posterObj.transform.parent = parent;

        // Ajouter un SpriteRenderer pour afficher l'image
        SpriteRenderer spriteRenderer = posterObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        // Ajuster la taille
        posterObj.transform.localScale = new Vector3(posterSize.x, posterSize.y, 1f);

        Debug.Log($"Affiche '{sprite.name}' placée à {position}");
    }
}
