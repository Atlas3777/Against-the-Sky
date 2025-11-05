using UnityEngine;

public class SnowController : MonoBehaviour
{
    [SerializeField] private GameObject SnowParticleSystem;
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private LayerMask roofLayers;

    void Update()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.up;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, raycastDistance, roofLayers))
        {
            if (SnowParticleSystem != null)
            {
                SnowParticleSystem.SetActive(false);
            }
        }
        else
        {
            if (SnowParticleSystem != null)
            {
                SnowParticleSystem.SetActive(true);
            }
        }
    }
}