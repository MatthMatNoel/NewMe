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

    [Header("Audio changement followers")]
    [SerializeField] private AudioSource followersAudioSource;
    [SerializeField] private AudioClip followersChangeClip; // son "de base" pour 1 à 99

    [Header("Sons paliers spéciaux")]
    [SerializeField] private AudioClip hundredMilestoneClip;    // 100, 200, 300, ...
    [SerializeField] private AudioClip fiveHundredMilestoneClip; // 500, 1000, 1500, ...
    [SerializeField] private AudioClip thousandMilestoneClip;    // 1000, 2000, ...

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
        if (followersAudioSource == null)
        {
            followersAudioSource = GetComponent<AudioSource>();
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

        PlayFollowersChangeSound();
        PlayMilestoneSound(previousFollowers, FollowersCount);

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

        PlayFollowersChangeSound();
        PlayMilestoneSound(previousFollowers, FollowersCount);

        OnFollowersChanged?.Invoke(FollowersCount);
    }

    /// <summary>
    /// Joue un son à chaque fois que le nombre de followers change.
    /// </summary>
    private void PlayFollowersChangeSound()
    {
        if (followersAudioSource == null || followersChangeClip == null)
            return;

        followersAudioSource.PlayOneShot(followersChangeClip);
    }

    /// <summary>
    /// Joue des sons spéciaux pour les paliers 100 / 500 / 1000.
    /// </summary>
    private void PlayMilestoneSound(int previousCount, int currentCount)
    {
        if (followersAudioSource == null)
            return;

        if (currentCount <= 0)
            return;

        // On regarde seulement les paliers atteints dans ce changement.
        // Si plusieurs paliers sont franchis d'un coup, on joue le plus important (1000 > 500 > 100).

        // 1000, 2000, 3000, ...
        int thousandStep = 1000;
        int fiveHundredStep = 500;
        int hundredStep = 100;

        bool crossedThousand = CrossedMultiple(previousCount, currentCount, thousandStep);
        bool crossedFiveHundred = CrossedMultiple(previousCount, currentCount, fiveHundredStep);
        bool crossedHundred = CrossedMultiple(previousCount, currentCount, hundredStep);

        if (crossedThousand && thousandMilestoneClip != null)
        {
            followersAudioSource.PlayOneShot(thousandMilestoneClip);
        }
        else if (crossedFiveHundred && fiveHundredMilestoneClip != null)
        {
            followersAudioSource.PlayOneShot(fiveHundredMilestoneClip);
        }
        else if (crossedHundred && hundredMilestoneClip != null)
        {
            followersAudioSource.PlayOneShot(hundredMilestoneClip);
        }
    }

    /// <summary>
    /// Retourne vrai si on a franchi un multiple de "step" entre previous et current (en montée).
    /// </summary>
    private bool CrossedMultiple(int previous, int current, int step)
    {
        if (current <= previous)
            return false;

        int prevMultiple = previous / step;
        int currMultiple = current / step;

        return currMultiple > prevMultiple;
    }
}
