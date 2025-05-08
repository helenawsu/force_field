using UnityEngine;

public class ForceField : MonoBehaviour
{
    [Header("Setup")]
    public GameObject geometricPrefab;
    public int count = 50;
    public float spawnRadius = 2f;

    [Header("Curl Noise Settings")]
    [Tooltip("Scale of the noise input.")]
    public float noiseScale = 0.5f;
    [Tooltip("Small delta for finite differences.")]
    public float curlEpsilon = 0.1f;
    [Tooltip("Overall strength of the curl force.")]
    public float curlStrength = 10f;

    [Header("Damping")]
    [Range(0f, 1f)]
    public float damping = 0.98f;

    private Transform userCam;
    public Rigidbody[] bodies;

    void Start()
    {
        userCam = Camera.main.transform;
        bodies = new Rigidbody[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            Vector3 pos = userCam.position + dir * spawnRadius;
            var go = Instantiate(geometricPrefab, pos, Random.rotation);
            var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 0.1f;
            bodies[i] = rb;
        }
    }

    void FixedUpdate()
    {
        foreach (var rb in bodies)
        {
            Vector3 p = rb.position * noiseScale;

            // Sample noise at neighbouring points
            float nX0 = Noise3D(p.x - curlEpsilon, p.y,             p.z);
            float nX1 = Noise3D(p.x + curlEpsilon, p.y,             p.z);
            float nY0 = Noise3D(p.x,             p.y - curlEpsilon, p.z);
            float nY1 = Noise3D(p.x,             p.y + curlEpsilon, p.z);
            float nZ0 = Noise3D(p.x,             p.y,             p.z - curlEpsilon);
            float nZ1 = Noise3D(p.x,             p.y,             p.z + curlEpsilon);

            // Finite differences (partial derivatives)
            float dndy_z = (nZ1 - nZ0) / (2f * curlEpsilon);
            float dndz_y = (nY1 - nY0) / (2f * curlEpsilon);
            float dndz_x = (nX1 - nX0) / (2f * curlEpsilon);
            float dndx_z = (nZ1 - nZ0) / (2f * curlEpsilon);
            float dndx_y = (nY1 - nY0) / (2f * curlEpsilon);
            float dndy_x = (nX1 - nX0) / (2f * curlEpsilon);

            // Curl components: ( ∂N_z/∂y - ∂N_y/∂z, ∂N_x/∂z - ∂N_z/∂x, ∂N_y/∂x - ∂N_x/∂y )
            Vector3 curl = new Vector3(
                dndy_z - dndz_y,
                dndz_x - dndx_z,
                dndx_y - dndy_x
            );

            // Apply the curl as a force
            rb.AddForce(curl * curlStrength, ForceMode.Acceleration);

            // Dampen velocity so it stays controlled
            rb.linearVelocity *= damping;
        }
    }

    // Simple 3D noise approximation by mixing 2D PerlinNoise slices
    private float Noise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float zx = Mathf.PerlinNoise(z, x);
        return (xy + yz + zx) * 0.3333f;
    }
}
