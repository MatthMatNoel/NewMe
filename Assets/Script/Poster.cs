using UnityEngine;

public class Poster : MonoBehaviour
{

    [Header("Liste des prefabs possibles")]
    [Tooltip("Glisse ici les différents prefabs de poster à choisir aléatoirement au démarrage")]
    public GameObject[] possiblePosters;

    [Tooltip("Référence vers l'objet enfant 'Poster' à remplacer (laisse vide pour auto-détection)")]
    [SerializeField] private Transform posterToReplace;

    [Header("Échelle aléatoire")]
    [Tooltip("Facteur d'échelle min et max appliqué par rapport à l'échelle de base du poster")]
    [SerializeField] private float minScaleFactor = 0.9f;
    [SerializeField] private float maxScaleFactor = 1.1f;

    [Header("Décalage Z aléatoire")]
    [Tooltip("Amplitude max du léger décalage sur l'axe Z pour éviter le z-fighting (en unités locales)")]
    [SerializeField] private float maxZOffset = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Oriente le poster comme avant
        transform.rotation = Quaternion.LookRotation(transform.up, Vector3.up);

        // Choix aléatoire d'un prefab parmi la liste, si elle est remplie
        if (possiblePosters != null && possiblePosters.Length > 0)
        {
            // Trouve l'enfant à remplacer :
            // 1) champ assigné dans l'inspector
            // 2) enfant nommé "Poster"
            // 3) sinon, premier enfant
            if (posterToReplace == null)
            {
                Transform found = transform.Find("Poster");
                if (found != null)
                {
                    posterToReplace = found;
                }
                else if (transform.childCount > 0)
                {
                    posterToReplace = transform.GetChild(0);
                }
            }

            if (posterToReplace == null)
            {
                // Rien à remplacer
                return;
            }

            Transform parent = posterToReplace.parent;
            Vector3 localPos = posterToReplace.localPosition;
            Quaternion localRot = posterToReplace.localRotation;
            Vector3 localScale = posterToReplace.localScale;

            // Détruit l'enfant actuel
            Destroy(posterToReplace.gameObject);

            // S'assure que les bornes d'échelle sont cohérentes
            if (maxScaleFactor < minScaleFactor)
            {
                float tmp = minScaleFactor;
                minScaleFactor = maxScaleFactor;
                maxScaleFactor = tmp;
            }

            // Sélectionne un prefab aléatoire
            GameObject chosenPrefab = null;
            int safety = 0;
            while (chosenPrefab == null && safety < 20)
            {
                int index = Random.Range(0, possiblePosters.Length);
                chosenPrefab = possiblePosters[index];
                safety++;
            }

            if (chosenPrefab != null)
            {
                GameObject instance = Instantiate(chosenPrefab, parent);

                // Applique un léger décalage aléatoire sur l'axe Z pour réduire le z-fighting
                float zOffset = 0f;
                if (maxZOffset > 0f)
                {
                    zOffset = Random.Range(-maxZOffset, maxZOffset);
                }

                instance.transform.localPosition = new Vector3(
                    localPos.x,
                    localPos.y,
                    localPos.z + zOffset
                );
                instance.transform.localRotation = localRot;

                // Applique un facteur d'échelle aléatoire dans l'intervalle spécifié
                float scaleFactor = Random.Range(minScaleFactor, maxScaleFactor);
                instance.transform.localScale = localScale * scaleFactor;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}