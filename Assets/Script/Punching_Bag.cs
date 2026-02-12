using UnityEngine;

public class Punching_Bag : MonoBehaviour
{
    [Header("Follower Number")]
    [SerializeField] private int followerNumber = 50;

    [Header("Punch Sound")]
    [SerializeField] private AudioSource punchAudioSource;
    [SerializeField] private AudioClip punchSound;

    // Minimum force required to register as a punch
    public float minPunchForce = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Detect collision with the punching bag
    private void OnCollisionEnter(Collision collision)
    {
        // Calculate the impact force
        float impactForce = collision.relativeVelocity.magnitude;

        // Check if the impact is strong enough to be considered a punch
        if (impactForce >= minPunchForce)
        {
            punchAudioSource.pitch = Random.Range(0.7f, 1.3f);
            punchAudioSource.volume = Random.Range(0.3f, 0.5f);
            punchAudioSource.PlayOneShot(punchSound);

            if (FollowersManager.Instance != null)
            {
                FollowersManager.Instance.AddFollowers(followerNumber);
            }
            else
            {
                Debug.LogWarning("FollowersManager.Instance est null : aucun gestionnaire de followers trouvé dans la scène.");
            }
        }
    }
}
