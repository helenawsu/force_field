// Perlin3D.cs
// --------------------
// Improved Perlin noise in 3D by Ken Perlin (2002).
// Usage: Perlin3D.Evaluate(x,y,z) returns noise in [–1,1].
using UnityEngine;

public static class Perlin3D
{
    // Permutation table; any random shuffle of 0..255 is fine
    private static readonly int[] perm = {
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,
        140,36,103,30,69,142,8,99,37,240,21,10,23,190, 6,148,
        247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,
        57,177,33,88,237,149,56,87,174,20,125,136,171,168, 68,175,
        74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
        60,211,133,230,220,105, 92,41,55,46,245,40,244,102,143,54,
        65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,
        200,196,135,130,116,188,159,86,164,100,109,198,173,186, 3,
        64,52,217,226,250,124,123, 5,202,38,147,118,126,255,82,85,
        212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,
        213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,
        172,9,129,22,39,253, 19,98,108,110,79,113,224,232,178,185,
        112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,
        162,241, 81,51,145,235,249,14,239,107, 49,192,214, 31,181,199,
        106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,138,236,
        205, 93,222,114, 67,29,24,72,243,141,128,195,78,66,215,61,
        156,180
    };

    private static readonly Vector3[] grad3 = {
        new Vector3(1,1,0),new Vector3(-1,1,0),new Vector3(1,-1,0),new Vector3(-1,-1,0),
        new Vector3(1,0,1),new Vector3(-1,0,1),new Vector3(1,0,-1),new Vector3(-1,0,-1),
        new Vector3(0,1,1),new Vector3(0,-1,1),new Vector3(0,1,-1),new Vector3(0,-1,-1),
    };

    private static int FastFloor(float x) => x > 0 ? (int)x : (int)x - 1;

    private static float Dot(Vector3 g, float x, float y, float z) => g.x*x + g.y*y + g.z*z;

    public static float Evaluate(float x, float y, float z)
{
    // Find unit grid cell containing point
    int X = FastFloor(x) & 255;
    int Y = FastFloor(y) & 255;
    int Z = FastFloor(z) & 255;
    int X1 = (X + 1) & 255;
    int Y1 = (Y + 1) & 255;
    int Z1 = (Z + 1) & 255;

    // Relative xyz coordinates within cell
    x -= FastFloor(x);
    y -= FastFloor(y);
    z -= FastFloor(z);

    // Compute fade curves for each coordinate
    float u = Fade(x);
    float v = Fade(y);
    float w = Fade(z);

    // Hash coordinates of the 8 cube corners (with wrapping)
    int A  = (perm[X] + Y) & 255;
    int AA = (perm[A] + Z) & 255;
    int AB = (perm[A] + Z1) & 255;
    int B  = (perm[X1] + Y) & 255;
    int BA = (perm[B] + Z) & 255;
    int BB = (perm[B] + Z1) & 255;

    // And add blended results from corners
    float res = Lerp(w,
        Lerp(v,
            Lerp(u, Grad(perm[AA],   x,   y,   z),
                     Grad(perm[BA], x-1,   y,   z)),
            Lerp(u, Grad(perm[AB],   x, y-1,   z),
                     Grad(perm[BB], x-1, y-1,   z))
        ),
        Lerp(v,
            Lerp(u, Grad(perm[AA+1 & 255],   x,   y,   z-1),
                     Grad(perm[BA+1 & 255], x-1,   y,   z-1)),
            Lerp(u, Grad(perm[AB+1 & 255],   x, y-1,   z-1),
                     Grad(perm[BB+1 & 255], x-1, y-1,   z-1))
        )
    );

    return res;
}


    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float Lerp(float t, float a, float b) => a + t * (b - a);
private static float Grad(int hash, float x, float y, float z)
{
    // Wrap the hash into [0..grad3.Length)
    Vector3 g = grad3[hash % grad3.Length];
    return Dot(g, x, y, z);
}

}
