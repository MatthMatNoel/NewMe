using UnityEngine;
using System.Collections.Generic;

public class PosterSpawner : MonoBehaviour
{
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
        // Charger toutes les sprites du dossier
        Sprite[] posters = Resources.LoadAll<Sprite>(posterFolderPath);

        if (posters.Length == 0)
        {
            Debug.LogWarning($"Aucune affiche trouvée dans Resources/{posterFolderPath}");
            return;
        }

        Debug.Log($"Trouvé {posters.Length} affiche(s)");

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
            for (int i = 0; i < postersPerWall; i++)
            {
                // Choisir une affiche aléatoire
                Sprite randomPoster = posters[Random.Range(0, posters.Length)];

                // Générer une position aléatoire sur le mur
                Vector3 randomPosition = GetRandomPositionOnWall(wall);

                // Créer et placer l'affiche
                CreatePoster(randomPoster, randomPosition, wall.transform);
            }
        }
    }

    Vector3 GetRandomPositionOnWall(GameObject wall)
    {
        Collider wallCollider = wall.GetComponent<Collider>();

        if (wallCollider == null)
        {
            Debug.LogWarning($"Le mur {wall.name} n'a pas de Collider");
            return wall.transform.position;
        }

        // Obtenir les limites du collider
        Bounds bounds = wallCollider.bounds;

        // Générer une position aléatoire sur le mur (haut et côté aléatoire)
        float randomX = Random.Range(bounds.min.x + margin, bounds.max.x - margin);
        float randomY = Random.Range(bounds.min.y + margin, bounds.max.y - margin);
        float randomZ = bounds.center.z; // Z reste contre le mur

        return new Vector3(randomX, randomY, randomZ);
    }

    void CreatePoster(Sprite sprite, Vector3 position, Transform wallParent)
    {
        // Créer un objet pour l'affiche
        GameObject posterObj = new GameObject($"Poster_{sprite.name}");
        posterObj.transform.position = position;
        posterObj.transform.parent = wallParent;

        // Ajouter un SpriteRenderer pour afficher l'image
        SpriteRenderer spriteRenderer = posterObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        // Ajuster la taille
        posterObj.transform.localScale = new Vector3(posterSize.x, posterSize.y, 1f);

        Debug.Log($"Affiche '{sprite.name}' placée à {position}");
    }
}
