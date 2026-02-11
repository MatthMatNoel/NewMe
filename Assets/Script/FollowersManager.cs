using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Assure-toi d'avoir le package Input System installé et configuré dans ton projet.

/// <summary>
/// Gestionnaire central du nombre de followers.
/// Stocke le score et expose un événement lorsqu'il change.
/// </summary>
public class FollowersManager : MonoBehaviour
{
    public static FollowersManager Instance { get; private set; }

    [Header("Followers de départ")]
    [SerializeField] private int initialFollowers = 0;

    [Header("Audio paliers followers")]
    [SerializeField] private AudioSource milestoneAudioSource;
    [SerializeField] private AudioClip hundredFollowersClip;

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
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
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

        // Récupère un AudioSource sur le même GameObject si non assigné.
        if (milestoneAudioSource == null)
        {
            milestoneAudioSource = GetComponent<AudioSource>();
        }

        // Optionnel : informer les éventuels listeners de la valeur initiale.
        OnFollowersChanged?.Invoke(FollowersCount);
    }

    /// <summary>
    /// Ajoute un certain nombre de followers (par exemple suite à une pompe réussie).
    /// </summary>
    public void AddFollowers(int amount)
    {
        if (amount <= 0) return;

        int previousFollowers = FollowersCount;
        FollowersCount += amount;

        CheckHundredMilestones(previousFollowers, FollowersCount);

        OnFollowersChanged?.Invoke(FollowersCount);
        Debug.Log($"[FollowersManager] Nouveau total de followers : {FollowersCount}");
    }

    /// <summary>
    /// Permet de fixer directement une valeur (utile si plus tard tu as des sauvegardes / chargements).
    /// </summary>
    public void SetFollowers(int value)
    {
        int previousFollowers = FollowersCount;
        FollowersCount = Mathf.Max(0, value);

        CheckHundredMilestones(previousFollowers, FollowersCount);

        OnFollowersChanged?.Invoke(FollowersCount);
    }

    /// <summary>
    /// Joue un son à chaque fois qu'un palier de 100 followers est franchi : 100, 200, 300, etc.
    /// </summary>
    private void CheckHundredMilestones(int previousCount, int currentCount)
    {
        if (hundredFollowersClip == null || milestoneAudioSource == null)
            return;

        if (currentCount <= 0)
            return;

        int previousMilestone = previousCount / 100;
        int currentMilestone = currentCount / 100;

        // Si on a franchi un ou plusieurs paliers de 100, on joue le son pour chacun.
        if (currentMilestone > previousMilestone)
        {
            for (int milestone = previousMilestone + 1; milestone <= currentMilestone; milestone++)
            {
                milestoneAudioSource.PlayOneShot(hundredFollowersClip);
            }
        }
    }
}
