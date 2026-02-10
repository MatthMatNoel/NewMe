using UnityEngine;

public class TireFlip : MonoBehaviour
{
    [Header("Tire Flip Detection")]
    [Tooltip("Hauteur minimale pour considérer que le pneu est soulevé (en mètres)")]
    public float liftHeight = 0.5f;

    [Tooltip("Angle minimum de rotation pour considérer que le pneu est renversé (en degrés)")]
    public float flipAngle = 120f;

    [Header("Debug Settings")]
    public bool showDebug = true;

    // State tracking
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isLifted = false;
    private bool isFlipped = false;

    void Start()
    {
        // Mémoriser la position et rotation initiales
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        Debug.Log("Tire Flip - Ready!");
    }

    void Update()
    {
        DetectTireFlip();
    }

    void DetectTireFlip()
    {
        // Calculer la hauteur actuelle par rapport à la position initiale
        float currentHeight = transform.position.y - initialPosition.y;
        
        // Calculer l'angle de rotation par rapport à la rotation initiale
        float rotationAngle = Quaternion.Angle(initialRotation, transform.rotation);

        // Vérifier si le pneu est soulevé
        if (!isLifted && currentHeight >= liftHeight)
        {
            isLifted = true;
            OnTireLifted();
        }

        // Vérifier si le pneu est renversé (soulevé ET rotation suffisante)
        if (isLifted && !isFlipped && rotationAngle >= flipAngle)
        {
            isFlipped = true;
            OnTireFlipped();
        }

        // Réinitialiser si le pneu est reposé au sol et dans sa position initiale
        if (isFlipped && currentHeight < 0.1f && rotationAngle < 30f)
        {
            ResetTire();
        }

        // Debug info
        if (showDebug)
        {
            Debug.DrawLine(initialPosition, transform.position, Color.cyan);
            Debug.Log($"Hauteur: {currentHeight:F2}m | Rotation: {rotationAngle:F1}° | Soulevé: {isLifted} | Renversé: {isFlipped}");
        }
    }

    void OnTireLifted()
    {
        Debug.Log("PNEU SOULEVÉ !");
        // Tu peux ajouter un son ou effet ici
    }

    void OnTireFlipped()
    {
        Debug.Log("PNEU RENVERSÉ - VALIDÉ !");

        // Ajouter un follower quand le pneu est renversé
        if (FollowersManager.Instance != null)
        {
            FollowersManager.Instance.AddFollowers(1);
            Debug.Log("+1 follower !!!!!");
        }
        else
        {
            Debug.LogWarning("FollowersManager.Instance est null : aucun gestionnaire de followers trouvé dans la scène.");
        }
    }

    void ResetTire()
    {
        // Réinitialiser les états pour permettre un nouveau flip
        isLifted = false;
        isFlipped = false;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        Debug.Log("Pneu réinitialisé - Prêt pour un nouveau renversement!");
    }

}
