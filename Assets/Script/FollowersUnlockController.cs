using UnityEngine;

/// <summary>
/// Active des objets dans la scene quand certains paliers de followers sont atteints.
/// a attacher sur un GameObject "Empty" dans la scene.
/// </summary>
/// 
[DefaultExecutionOrder(-100)]
public class FollowersUnlockController : MonoBehaviour
{
    [System.Serializable]
    public class UnlockItem
    {
        [Tooltip("Nombre de followers necessaire pour faire apparaître l'objet")]
        public int requiredFollowers = 0;

        [Tooltip("Objet a faire apparaître (mettre ici un GameObject de la scene, initialement desactive)")]
        public GameObject objectToActivate;

        [HideInInspector]
        public bool isUnlocked = false; // evite de reactiver plusieurs fois le même objet
    }

    [Header("Liste des objets a debloquer")]
    [Tooltip("Configure ici les paliers de followers et les objets a activer")]
    public UnlockItem[] unlocks;

    // Index de l'etape courante dans la "state machine"
    // 0 = premiere etape, 1 = deuxieme, etc.
    private int currentStepIndex = 0;

    FollowersManager manager;

    private void OnEnable()
    {
        manager = GetComponent<FollowersManager>();
        if (manager != null)
        {
            manager.OnFollowersChanged += HandleFollowersChanged;

            // Initialisation avec la valeur actuelle au cas où il y ait deja des followers
            HandleFollowersChanged(manager.FollowersCount);
        }
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnFollowersChanged -= HandleFollowersChanged;
        }
    }

    /// <summary>
    /// Appele a chaque changement de nombre de followers.
    /// </summary>
    private void HandleFollowersChanged(int newCount)
    {
        Debug.Log($"[FollowersUnlockController] HandleFollowersChanged appele avec {newCount} followers (currentStepIndex = {currentStepIndex})");

        if (unlocks == null || unlocks.Length == 0)
            return;
        Debug.Log(currentStepIndex);

        // Fonctionnement type "state machine" :
        // - On suppose que le tableau "unlocks" est range par ordre croissant de requiredFollowers.
        // - On ne regarde que l'etape suivante (currentStepIndex), puis la suivante, etc.
        // - On n'essaie jamais de revenir en arriere.

        while (currentStepIndex < unlocks.Length)
        {
            var item = unlocks[currentStepIndex];

            // Si l'element est mal configure, on passe a l'etape suivante.
            if (item == null || item.objectToActivate == null)
            {
                currentStepIndex++;
                continue;
            }

            // Si deja debloque, on avance a l'etape suivante.
            if (item.isUnlocked)
            {
                currentStepIndex++;
                continue;
            }

            // Condition d'entree dans l'etape : on a atteint (ou depasse) le palier.
            // Si tu veux vraiment uniquement a la valeur exacte, tu peux remplacer
            // ">=" par "==", mais attention : si tu sautes ce nombre, l'etape ne sera jamais activee.
            Debug.Log($"[FollowersUnlockController] etape {currentStepIndex} : palier = {item.requiredFollowers}, followers actuels = {newCount}");

            Debug.Log($"debug :{item.requiredFollowers}");
            if (newCount >= item.requiredFollowers)
            {
                item.objectToActivate.SetActive(true);
                item.isUnlocked = true;
                Debug.Log($"[FollowersUnlockController] etape {currentStepIndex} debloquee : {item.objectToActivate.name} (palier {item.requiredFollowers}, actuel {newCount})");

                // On passe a l'etape suivante et on regarde si, par exemple apres un gros gain de followers,
                // on doit aussi debloquer les etapes suivantes dans la même frame.
                currentStepIndex++;
                continue;
            }

            // Si on n'a pas encore atteint le palier de l'etape courante, on s'arrête la.
            break;
        }
    }
}
