using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Poster : MonoBehaviour
{
    [System.Serializable]
    public class PosterItem
    {
        [Tooltip("Glisser un prefab d'affiche ici (GameObject)")]
        public GameObject prefab;
    }

    [Header("Poster Assets")]
    [Tooltip("Glisser jusqu'à plusieurs prefabs ici. Le script choisira un des 6 premiers aléatoirement.")]
    public PosterItem[] posterItems;

    [Tooltip("Dossier dans Resources contenant les prefabs si aucun PosterItem n'est assigné (ex: 'affiche2d')")]
    public string postersFolder = "poster2d";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Rassembler les prefabs directement assignés dans l'inspector
        var list = new List<GameObject>();
        if (posterItems != null)
        {
            foreach (var pi in posterItems)
            {
                if (pi != null && pi.prefab != null) list.Add(pi.prefab);
            }
        }

        // Si aucun prefab assigné via l'inspector, charger depuis Resources/postersFolder
        if (list.Count == 0)
        {
            GameObject[] loaded = Resources.LoadAll<GameObject>(postersFolder);
            if (loaded != null && loaded.Length > 0)
            {
                list.AddRange(loaded);
            }
        }

#if UNITY_EDITOR
        // Editor fallback: si rien trouvé dans Resources, tenter de charger depuis Assets/<postersFolder>
        if (list.Count == 0)
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath($"Assets/{postersFolder}");
            if (objs != null)
            {
                foreach (var o in objs)
                {
                    if (o is GameObject go) list.Add(go);
                }
            }
        }
#endif

        if (list.Count == 0)
        {
            Debug.LogWarning($"Aucun prefab d'affiche trouvé : vérifie Poster.postersFolder ou remplis posterItems dans l'Inspector.");
            // keep placeholder upright
            transform.rotation = Quaternion.LookRotation(transform.up, Vector3.up);
            return;
        }

        int maxChoices = Mathf.Min(6, list.Count);
        int idx = Random.Range(0, maxChoices);
        GameObject chosenPrefab = list[idx];

        // Instantiate chosen prefab at this transform's position/rotation and parent
        var instance = Instantiate(chosenPrefab, transform.position, transform.rotation, transform.parent);

        // Optionally, you can adjust instance.localScale or other properties here

        // Destroy this placeholder object
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}