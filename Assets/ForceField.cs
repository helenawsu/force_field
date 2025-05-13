using System;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    [Header("Setup")]
    public GameObject geometricPrefab;
    public int    count       = 50;
    [Tooltip("Radius of the sphere around the camera")]
    public float  spawnRadius = 0.2f;

    [Header("Curl Noise Settings")]
    public float noiseScale   = 0.03f;
    public float curlStrength = 2f;
    public float curlEpsilon  = 0.1f;

    [Header("Damping")]
    [Range(0f, 1f)]
    public float damping      = 0.99f;

    private Transform userCam;
    public Rigidbody[] bodies;
    public float divergenceNoiseScale = 0.5f;
    [Tooltip("Overall strength of this added divergence")]
    public float divergenceStrength   = 1f;
    public float  SampleDivergence(Vector3 p) => Divergence(p);
    public Vector3 SampleCurl     (Vector3 p) => Curl(p);

    void Start()
    {
        userCam = Camera.main.transform;
        bodies  = new Rigidbody[count];
        Vector3 center = new Vector3(0f, 1.5f, 0f);

        for (int i = 0; i < count; i++)
        {
Vector3 pos = center + UnityEngine.Random.insideUnitSphere * spawnRadius;           
var go = Instantiate(
    geometricPrefab,
    pos,
    UnityEngine.Random.rotation
);
            var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass       = 100f;
            bodies[i]     = rb;
        }
    }
void FixedUpdate()
{
    Vector3 center = userCam.position;

    foreach (var rb in bodies)
    {
        // 1) sample the curl‐noise field
        Vector3 F = ForceFieldFunction(rb.position);

        // 2) attraction to center
        Vector3 toCenter = (center - rb.position);
        F += toCenter * 1.0f;    // the “1.0f” is your attraction strength

        // apply forces
        rb.AddForce(F, ForceMode.Acceleration);

        // 3) gentle damping
        rb.linearVelocity *= damping;

        // 4) compute & print divergence & curl
        float div  = SampleDivergence(rb.position);
        Vector3 curl = SampleCurl(rb.position);
        Debug.Log($"Div = {div:F4}   |   Curl = ({curl.x:F4}, {curl.y:F4}, {curl.z:F4})   |   |Curl| = {curl.magnitude:F4}");
    }
}


    // —————————————————————————
    // Actual force‐field definition
    // —————————————————————————
    Vector3 ForceFieldFunction(Vector3 p)
    {
        // build the “base” 3D noise vector
        float nx = Perlin3D.Evaluate(p.y * noiseScale, p.z * noiseScale, p.x * noiseScale);
        float ny = Perlin3D.Evaluate(p.z * noiseScale, p.x * noiseScale, p.y * noiseScale);
        float nz = Perlin3D.Evaluate(p.x * noiseScale, p.y * noiseScale, p.z * noiseScale);

        // curl that noise to make a divergence‐free swirl
        Vector3 curl = CurlOfVectorField(q =>
            new Vector3(
                Perlin3D.Evaluate(q.y * noiseScale, q.z * noiseScale, q.x * noiseScale),
                Perlin3D.Evaluate(q.z * noiseScale, q.x * noiseScale, q.y * noiseScale),
                Perlin3D.Evaluate(q.x * noiseScale, q.y * noiseScale, q.z * noiseScale)
            ),
            p
        );

        // Vector3 baseF = ForceFieldFunction(p);

        // B) add a little divergence: sample Perlin in 1D for a scalar noise
        float dNoise = (Mathf.PerlinNoise(p.x * divergenceNoiseScale, p.y * divergenceNoiseScale)
                      - 0.5f) * 2f;  // in range [-1,1]

        // radial direction
        Vector3 radial = (p - userCam.position).normalized;

        return curl * curlStrength + radial * dNoise * divergenceStrength;
    }

    // used by Divergence & Curl below
    Vector3 SampleField(Vector3 p) => ForceFieldFunction(p);

    // —————————————————————————
    // Finite‐difference operators
    // —————————————————————————
    float Divergence(Vector3 p)
    {
        float h = curlEpsilon;
        float dFx_dx = (SampleField(p + Vector3.right * h).x
                      - SampleField(p - Vector3.right * h).x) / (2*h);
        float dFy_dy = (SampleField(p + Vector3.up    * h).y
                      - SampleField(p - Vector3.up    * h).y) / (2*h);
        float dFz_dz = (SampleField(p + Vector3.forward * h).z
                      - SampleField(p - Vector3.forward * h).z) / (2*h);
        return dFx_dx + dFy_dy + dFz_dz;
    }

    Vector3 Curl(Vector3 p)
    {
        float h = curlEpsilon;
        Vector3 fxp = SampleField(p + Vector3.right   * h);
        Vector3 fxm = SampleField(p - Vector3.right   * h);
        Vector3 fyp = SampleField(p + Vector3.up      * h);
        Vector3 fym = SampleField(p - Vector3.up      * h);
        Vector3 fzp = SampleField(p + Vector3.forward * h);
        Vector3 fzm = SampleField(p - Vector3.forward * h);

        float dFz_dy = (fzp.y - fzm.y)/(2*h);
        float dFy_dz = (fyp.z - fym.z)/(2*h);
        float dFx_dz = (fxp.z - fxm.z)/(2*h);
        float dFz_dx = (fzp.x - fzm.x)/(2*h);
        float dFy_dx = (fyp.x - fym.x)/(2*h);
        float dFx_dy = (fxp.y - fxm.y)/(2*h);

        return new Vector3(
            dFz_dy - dFy_dz,
            dFx_dz - dFz_dx,
            dFy_dx - dFx_dy
        );
    }

    // numerically compute curl of any Vector3→Vector3 function
    Vector3 CurlOfVectorField(Func<Vector3, Vector3> field, Vector3 p)
    {
        float h = curlEpsilon;
        Vector3 fxp = field(p + Vector3.right   * h);
        Vector3 fxm = field(p - Vector3.right   * h);
        Vector3 fyp = field(p + Vector3.up      * h);
        Vector3 fym = field(p - Vector3.up      * h);
        Vector3 fzp = field(p + Vector3.forward * h);
        Vector3 fzm = field(p - Vector3.forward * h);

        float dFz_dy = (fzp.y - fzm.y)/(2*h);
        float dFy_dz = (fyp.z - fym.z)/(2*h);
        float dFx_dz = (fxp.z - fxm.z)/(2*h);
        float dFz_dx = (fzp.x - fzm.x)/(2*h);
        float dFy_dx = (fyp.x - fym.x)/(2*h);
        float dFx_dy = (fxp.y - fxm.y)/(2*h);

        return new Vector3(
            dFz_dy - dFy_dz,
            dFx_dz - dFz_dx,
            dFy_dx - dFx_dy
        );
    }
}
