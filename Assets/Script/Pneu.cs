using UnityEngine;

public class TirePush : MonoBehaviour
{
    [Header("Tire Push Detection")]
    [Tooltip("Distance minimale à parcourir pour valider (en mètres)")]
    public float requiredDistance = 3f;

    [Header("Debug Settings")]
    public bool showDebug = true;

    // State tracking
    private Vector3 startPosition;
    private bool isCompleted = false;

    void Start()
    {
        // Mémoriser la position de départ
        startPosition = transform.position;
        
        Debug.Log("Tire Push - Ready!");
    }

    void Update()
    {
        if (!isCompleted)
        {
            DetectTirePush();
        }
    }

    void DetectTirePush()
    {
        // Calculer la distance parcourue depuis le départ
        float distanceMoved = Vector3.Distance(startPosition, transform.position);

        // Vérifier si le pneu a été poussé assez loin
        if (distanceMoved >= requiredDistance)
        {
            isCompleted = true;
            OnTirePushed();
        }

        // Debug info
        if (showDebug)
        {
            Debug.DrawLine(startPosition, transform.position, Color.green);
            Debug.Log($"Distance parcourue: {distanceMoved:F2}m / {requiredDistance}m");
        }
    }

    void OnTirePushed()
    {
        Debug.Log("PNEU POUSSÉ - VALIDÉ !");

        // Ajouter un follower quand le pneu est poussé assez loin
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

    // Fonction pour réinitialiser manuellement si besoin
    public void ResetTire()
    {
        startPosition = transform.position;
        isCompleted = false;
        Debug.Log("Pneu réinitialisé - Prêt pour un nouveau push!");
    }

}
