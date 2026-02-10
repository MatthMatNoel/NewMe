using System;
using UnityEngine;

/// <summary>
/// Gestionnaire central du nombre de followers.
/// Stocke le score et expose un événement lorsqu'il change.
/// </summary>
public class FollowersManager : MonoBehaviour
{
    public static FollowersManager Instance { get; private set; }

    [Header("Followers de départ")]
    [SerializeField] private int initialFollowers = 0;

    /// <summary>
    /// Nombre total actuel de followers.
    /// </summary>
    public int FollowersCount { get; private set; }

    /// <summary>
    /// Événement appelé à chaque fois que le nombre de followers change.
    /// int = nouveau total.
    /// </summary>
    public event Action<int> OnFollowersChanged;

    private void Update()
    {
        // DEBUG TEMPORAIRE : appuyer sur ESPACE pour ajouter 1 follower
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddFollowers(1);
            Debug.Log($"[FollowersManager][DEBUG] Espace pressé -> +1 follower, total = {FollowersCount}");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // On s'assure qu'il n'existe qu'un seul gestionnaire dans la scène.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        FollowersCount = initialFollowers;

        // Optionnel : informer les éventuels listeners de la valeur initiale.
        OnFollowersChanged?.Invoke(FollowersCount);
    }

    /// <summary>
    /// Ajoute un certain nombre de followers (par exemple suite à une pompe réussie).
    /// </summary>
    public void AddFollowers(int amount)
    {
        if (amount <= 0) return;

        FollowersCount += amount;
        OnFollowersChanged?.Invoke(FollowersCount);
        Debug.Log($"[FollowersManager] Nouveau total de followers : {FollowersCount}");
    }

    /// <summary>
    /// Permet de fixer directement une valeur (utile si plus tard tu as des sauvegardes / chargements).
    /// </summary>
    public void SetFollowers(int value)
    {
        FollowersCount = Mathf.Max(0, value);
        OnFollowersChanged?.Invoke(FollowersCount);
    }
}
