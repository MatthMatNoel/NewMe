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

    [Tooltip("Source dédiée pour le son de changement de followers (pour gérer le pitch indépendamment)")]
    [SerializeField] private AudioSource followersChangeAudioSource;

    [Header("Pitch aléatoire du son de changement")]
    [SerializeField] private float minChangePitch = 0.9f;
    [SerializeField] private float maxChangePitch = 1.1f;

    [Header("Sons paliers spéciaux")]
    [SerializeField] private AudioClip hundredMilestoneClip;    // 100, 200, 300, ...
    [SerializeField] private AudioClip fiveHundredMilestoneClip; // 500, 1000, 1500, ...
    [SerializeField] private AudioClip thousandMilestoneClip;    // 1000, 2000, ...

    [Header("Sons de motivation (aléatoires)")]
    [Tooltip("Liste de sons de motivation joués parfois quand on gagne des followers")]
    [SerializeField] private AudioClip[] motivationalClips;

    [Tooltip("Probabilité (0-1) de jouer un son de motivation à chaque gain de followers")]
    [Range(0f, 1f)]
    [SerializeField] private float motivationChancePerGain = 0.3f;

    [Tooltip("Temps minimum en secondes entre deux sons de motivation")]
    [SerializeField] private float motivationCooldown = 3f;

    private float _lastMotivationTime = -999f;

    [Header("Particles lors du gain de followers")]
    [SerializeField] private ParticleSystem followersParticleSystem;
    [Tooltip("Nombre de particules émises par follower gagné")] 
    [SerializeField] private float particlesPerFollower = 10f;
    [Tooltip("Nombre maximum de particules émises en une fois (0 = pas de limite)")]
    [SerializeField] private int maxParticlesPerBurst = 200;
    [Tooltip("Durée maximale en secondes pendant laquelle les particules vont être émises séquentiellement")]
    [SerializeField] private float burstDuration = 1.5f;

    private Coroutine _particlesCoroutine;

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

        // Si aucune source dédiée n'est assignée pour le son de changement,
        // on en crée une nouvelle en copiant les réglages principaux de la source existante.
        if (followersChangeAudioSource == null)
        {
            if (followersAudioSource != null)
            {
                followersChangeAudioSource = gameObject.AddComponent<AudioSource>();
                followersChangeAudioSource.playOnAwake = false;
                followersChangeAudioSource.spatialBlend = followersAudioSource.spatialBlend;
                followersChangeAudioSource.volume = followersAudioSource.volume;
                followersChangeAudioSource.outputAudioMixerGroup = followersAudioSource.outputAudioMixerGroup;
            }
            else
            {
                followersChangeAudioSource = GetComponent<AudioSource>();
            }
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
        TryPlayMotivationalSound();
        PlayMilestoneSound(previousFollowers, FollowersCount);
        PlayFollowersParticles(amount);

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
        TryPlayMotivationalSound();
        PlayMilestoneSound(previousFollowers, FollowersCount);

        OnFollowersChanged?.Invoke(FollowersCount);
    }

    /// <summary>
    /// Joue un son à chaque fois que le nombre de followers change.
    /// </summary>
    private void PlayFollowersChangeSound()
    {
        AudioSource src = followersChangeAudioSource != null ? followersChangeAudioSource : followersAudioSource;

        if (src == null || followersChangeClip == null)
            return;

        // Assure qu'on a un intervalle valide
        float pitchMin = Mathf.Min(minChangePitch, maxChangePitch);
        float pitchMax = Mathf.Max(minChangePitch, maxChangePitch);

        float randomPitch = UnityEngine.Random.Range(pitchMin, pitchMax);
        src.pitch = randomPitch;
        src.PlayOneShot(followersChangeClip);

        Debug.Log($"[FollowersManager] followersChangeClip joué avec pitch = {randomPitch:F2}");
    }

    /// <summary>
    /// Tente de jouer un son de motivation de façon aléatoire lors d'un gain de followers.
    /// </summary>
    private void TryPlayMotivationalSound()
    {
        if (followersAudioSource == null || motivationalClips == null || motivationalClips.Length == 0)
            return;

        // Cooldown pour éviter le spam.
        if (Time.time - _lastMotivationTime < motivationCooldown)
            return;

        // Chance de jouer un son de motivation.
        if (UnityEngine.Random.value > motivationChancePerGain)
            return;

        int index = UnityEngine.Random.Range(0, motivationalClips.Length);
        AudioClip clip = motivationalClips[index];
        if (clip == null)
            return;

        followersAudioSource.PlayOneShot(clip);
        _lastMotivationTime = Time.time;
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

    /// <summary>
    /// Joue un effet de particules dont l'intensité dépend du nombre de followers gagnés.
    /// </summary>
    private void PlayFollowersParticles(int amount)
    {
        if (followersParticleSystem == null || amount <= 0)
            return;

        // Nombre désiré de particules = followers gagnés * multiplicateur.
        int desiredParticles = Mathf.RoundToInt(amount * Mathf.Max(0f, particlesPerFollower));

        // Si tu veux exactement 1 particule par follower, mets particlesPerFollower = 1.
        int particleCount = desiredParticles;

        if (maxParticlesPerBurst > 0)
        {
            particleCount = Mathf.Min(particleCount, maxParticlesPerBurst);
        }

        if (particleCount <= 0)
            return;

        // Désactive toute émission continue pour garder un contrôle "au particle près".
        var emission = followersParticleSystem.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 0f;

        if (_particlesCoroutine != null)
        {
            StopCoroutine(_particlesCoroutine);
        }

        Debug.Log($"[FollowersManager] Gagné {amount} followers -> émission de {particleCount} particules");

        _particlesCoroutine = StartCoroutine(EmitParticlesSequentially(particleCount));
    }

    /// <summary>
    /// Émet les particules une par une sur une courte durée.
    /// </summary>
    private IEnumerator EmitParticlesSequentially(int particleCount)
    {
        if (followersParticleSystem == null || particleCount <= 0)
            yield break;

        // S'assurer que le GameObject est actif pour voir les particules.
        if (!followersParticleSystem.gameObject.activeInHierarchy)
        {
            followersParticleSystem.gameObject.SetActive(true);
        }

        if (!followersParticleSystem.isPlaying)
        {
            followersParticleSystem.Play();
        }

        float totalDuration = Mathf.Max(0f, burstDuration);
        float delay = 0f;

        if (particleCount > 1 && totalDuration > 0f)
        {
            // On répartit les particules sur la durée souhaitée.
            delay = totalDuration / particleCount;
        }

        for (int i = 0; i < particleCount; i++)
        {
            followersParticleSystem.Emit(1);

            if (delay > 0f && i < particleCount - 1)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null; // au moins une frame entre chaque particule
            }
        }

        _particlesCoroutine = null;
    }
}
