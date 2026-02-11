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

        [HideInInspector]
        public bool isUnlocked = false; // Évite de réactiver plusieurs fois le même objet
    }

    [Header("Liste des objets à débloquer")]
    [Tooltip("Configure ici les paliers de followers et les objets à activer")]
    public UnlockItem[] unlocks;

    // Index de l'étape courante dans la "state machine"
    // 0 = première étape, 1 = deuxième, etc.
    private int currentStepIndex = 0;

    private void OnEnable()
    {
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
                item.objectToActivate.SetActive(true);
                item.isUnlocked = true;
                Debug.Log($"[FollowersUnlockController] Étape {currentStepIndex} débloquée : {item.objectToActivate.name} (palier {item.requiredFollowers}, actuel {newCount})");

                // On passe à l'étape suivante et on regarde si, par exemple après un gros gain de followers,
                // on doit aussi débloquer les étapes suivantes dans la même frame.
                currentStepIndex++;
                continue;
            }

            // Si on n'a pas encore atteint le palier de l'étape courante, on s'arrête là.
            break;
        }
    }
}
