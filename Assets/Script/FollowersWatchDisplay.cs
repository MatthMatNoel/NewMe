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

    [Header("(Optionnel) Texte avant le nombre")]
    [SerializeField] private string prefix = "";

    private void OnEnable()
    {
        if (FollowersManager.Instance != null)
        {
            FollowersManager.Instance.OnFollowersChanged += HandleFollowersChanged;

            // Initialisation de l'affichage avec la valeur actuelle.
            HandleFollowersChanged(FollowersManager.Instance.FollowersCount);
        }
        else if (followersText != null)
        {
            // Si pas de manager, on commence à 0 par défaut.
            followersText.text = prefix + "0";
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

        // Affiche uniquement le nombre (éventuellement précédé d'un petit préfixe vide par défaut).
        followersText.text = prefix + newCount.ToString();
    }
}
