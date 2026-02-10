using UnityEngine;
using TMPro;

/// <summary>
/// Affichage du nombre de followers sur une montre (Canvas en World Space).
/// Ce script NE MODIFIE PAS le score, il fait seulement l'affichage.
/// </summary>
public class FollowersWatchDisplay : MonoBehaviour
{
    [Header("Référence vers le TextMeshPro de la montre")]
    [SerializeField] private TMP_Text followersText;

    [Header("Texte avant le nombre")]
    [SerializeField] private string prefix = "Followers : ";

    private void OnEnable()
    {
        if (FollowersManager.Instance != null)
        {
            FollowersManager.Instance.OnFollowersChanged += HandleFollowersChanged;

            // Initialisation de l'affichage avec la valeur actuelle.
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
    /// Callback appelé par le FollowersManager quand le nombre de followers change.
    /// </summary>
    private void HandleFollowersChanged(int newCount)
    {
        if (followersText == null)
            return;

        // Gestion du singulier/pluriel : 1 follower / X followers
        string label = newCount == 1 ? " follower" : " followers";
        followersText.text = prefix + newCount.ToString() + label;
    }
}
