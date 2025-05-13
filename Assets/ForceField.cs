using UnityEngine;

public class ForceField : MonoBehaviour
{
    [Header("Setup")]
    public GameObject geometricPrefab;
    public int    count        = 50;
    [Tooltip("Radius of the sphere around the camera")]
    public float  spawnRadius  = 1f;

    [Header("Wander")]
    [Tooltip("How hard they randomly accelerate each frame")]
    public float wanderStrength = 5f;

    [Header("Damping")]
    [Range(0f, 1f)]
    public float damping       = 0.9f;

    private Transform userCam;
    public Rigidbody[] bodies;

    void Start()
    {
        userCam = Camera.main.transform;
        bodies  = new Rigidbody[count];

        for (int i = 0; i < count; i++)
        {
            // spawn uniformly inside the sphere
            Vector3 pos = userCam.position + Random.insideUnitSphere * spawnRadius;
            var go = Instantiate(geometricPrefab, pos, Random.rotation);
            var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass       = 0.1f;
            bodies[i]     = rb;
        }
    }

    void FixedUpdate()
    {
        Vector3 camPos = userCam.position;

        foreach (var rb in bodies)
        {
            // 1) random wander acceleration
            Vector3 randomAccel = Random.insideUnitSphere * wanderStrength;
            rb.AddForce(randomAccel, ForceMode.Acceleration);

            // 2) gentle damping
            rb.linearVelocity *= damping;

            // 3) clamp inside sphere
            Vector3 offset = rb.position - camPos;
            if (offset.sqrMagnitude > spawnRadius * spawnRadius)
            {
                Vector3 normal = offset.normalized;
                // snap back to surface
                rb.position = camPos + normal * spawnRadius;
                // reflect velocity so it bounces inward
                rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, normal);
            }
        }
    }
}
