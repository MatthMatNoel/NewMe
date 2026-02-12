using UnityEngine;

public class Logo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
