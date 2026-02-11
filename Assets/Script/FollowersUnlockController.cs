using UnityEngine;

/// <summary>
/// Active des objets dans la scène quand certains paliers de followers sont atteints.
/// À attacher sur un GameObject "Empty" dans la scène.
/// </summary>
public class FollowersUnlockController : MonoBehaviour
{
    [System.Serializable]
    public class UnlockItem
    {
        [Tooltip("Nombre de followers nécessaire pour faire apparaître l'objet")]
        public int requiredFollowers = 0;

        [Tooltip("Objet à faire apparaître (mettre ici un GameObject de la scène, initialement désactivé)")]
        public GameObject objectToActivate;

        [Tooltip("Son joué quand cet élément est débloqué (par exemple début d'un nouvel exercice)")]
        public AudioClip unlockSound;

        [HideInInspector]
        public bool isUnlocked = false; // Évite de réactiver plusieurs fois le même objet
    }

    [Header("Liste des objets à débloquer")]
    [Tooltip("Configure ici les paliers de followers et les objets à activer")]
    public UnlockItem[] unlocks;

    [Header("Audio")]
    [Tooltip("Source audio utilisée pour jouer les sons de déblocage d'étapes")]
    [SerializeField] private AudioSource unlockAudioSource;

    // Index de l'étape courante dans la "state machine"
    // 0 = première étape, 1 = deuxième, etc.
    private int currentStepIndex = 0;

    private void OnEnable()
    {
        if (unlockAudioSource == null)
        {
            unlockAudioSource = GetComponent<AudioSource>();
        }

        if (FollowersManager.Instance != null)
        {
            FollowersManager.Instance.OnFollowersChanged += HandleFollowersChanged;

            // Initialisation avec la valeur actuelle au cas où il y ait déjà des followers
            HandleFollowersChanged(FollowersManager.Instance.FollowersCount);
        }
    }

    private void OnDisable()
    {
        if (FollowersManager.Instance != null)
        {
            FollowersManager.Instance.OnFollowersChanged -= HandleFollowersChanged;
        }
    }

    /// <summary>
    /// Appelé à chaque changement de nombre de followers.
    /// </summary>
    private void HandleFollowersChanged(int newCount)
    {
        Debug.Log($"[FollowersUnlockController] HandleFollowersChanged appelé avec {newCount} followers (currentStepIndex = {currentStepIndex})");

        if (unlocks == null || unlocks.Length == 0)
            return;

        // Fonctionnement type "state machine" :
        // - On suppose que le tableau "unlocks" est rangé par ordre croissant de requiredFollowers.
        // - On ne regarde que l'étape suivante (currentStepIndex), puis la suivante, etc.
        // - On n'essaie jamais de revenir en arrière.

        while (currentStepIndex < unlocks.Length)
        {
            var item = unlocks[currentStepIndex];

            // Si l'élément est mal configuré, on passe à l'étape suivante.
            if (item == null || item.objectToActivate == null)
            {
                currentStepIndex++;
                continue;
            }

            // Si déjà débloqué, on avance à l'étape suivante.
            if (item.isUnlocked)
            {
                currentStepIndex++;
                continue;
            }

            // Condition d'entrée dans l'étape : on a atteint (ou dépassé) le palier.
            // Si tu veux vraiment uniquement à la valeur exacte, tu peux remplacer
            // ">=" par "==", mais attention : si tu sautes ce nombre, l'étape ne sera jamais activée.
            Debug.Log($"[FollowersUnlockController] Étape {currentStepIndex} : palier = {item.requiredFollowers}, followers actuels = {newCount}");

            if (newCount >= item.requiredFollowers)
            {
                // Find all placeholder GameObjects in the scene by tag and instantiate the
                // object at the one nearest to the camera rig. If no placeholders are
                // present, fall back to activating the provided object in-place.
                var placeholderGOs = GameObject.FindGameObjectsWithTag("Placeholder");

                Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
                Vector3 cameraPos = cameraTransform != null ? cameraTransform.position : transform.position;

                if (placeholderGOs != null && placeholderGOs.Length > 0)
                {
                    GameObject nearest = null;
                    float minDist = float.MaxValue;

                    foreach (var ph in placeholderGOs)
                    {
                        if (ph == null) continue;
                        float d = Vector3.SqrMagnitude(ph.transform.position - cameraPos);
                        if (d < minDist)
                        {
                            minDist = d;
                            nearest = ph;
                        }
                    }

                    if (nearest != null)
                    {
                        Instantiate(item.objectToActivate, nearest.transform.position, nearest.transform.rotation);
                        item.isUnlocked = true;
                        PlayUnlockSound(item.unlockSound);
                        Debug.Log($"[FollowersUnlockController] Étape {currentStepIndex} débloquée (instanciée au placeholder) : {item.objectToActivate.name} (palier {item.requiredFollowers}, actuel {newCount})");
                    }
                    else
                    {
                        item.objectToActivate.SetActive(true);
                        item.isUnlocked = true;
                        PlayUnlockSound(item.unlockSound);
                        Debug.Log($"[FollowersUnlockController] Étape {currentStepIndex} débloquée (activation fallback) : {item.objectToActivate.name} (palier {item.requiredFollowers}, actuel {newCount})");
                    }
                }
                else
                {
                    item.objectToActivate.SetActive(true);
                    item.isUnlocked = true;
                    PlayUnlockSound(item.unlockSound);
                    Debug.Log($"[FollowersUnlockController] Étape {currentStepIndex} débloquée (aucun placeholder trouvé) : {item.objectToActivate.name} (palier {item.requiredFollowers}, actuel {newCount})");
                }

                // On passe à l'étape suivante et on regarde si, par exemple après un gros gain de followers,
                // on doit aussi débloquer les étapes suivantes dans la même frame.
                currentStepIndex++;
                continue;
            }

            // Si on n'a pas encore atteint le palier de l'étape courante, on s'arrête là.
            break;
        }
    }

    /// <summary>
    /// Joue le son associé à un élément lorsqu'il est débloqué.
    /// </summary>
    private void PlayUnlockSound(AudioClip clip)
    {
        if (unlockAudioSource == null || clip == null)
            return;

        unlockAudioSource.PlayOneShot(clip);
    }
}
