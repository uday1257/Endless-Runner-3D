using UnityEngine;

public enum BiomeType { Grassland = 0, Jungle = 1, Desert = 2, IceWorld = 3, City = 4 }

public class BiomeStyle
{
    public BiomeType type;
    public string name;
    // environment
    public Color sky, ambient, sun;
    public float sunIntensity = 1.1f, fogDensity = 0.008f;
    public Vector3 sunAngle = new Vector3(50f, -30f, 0f);
    // ground (track = darker running surface, side = bright colourful surroundings)
    public Color trackBase, trackLine, sideBase, sideAccent, curb, hud;
    public float trackGloss = 0.1f;
    // ambient particles
    public Color particleA, particleB;
    public float particleSize = 0.12f, particleGravity = 0f, particleSpeed = 0.4f;
    // runtime materials
    public Material trackMat, sideMat, curbMat;
}

public static class Biomes
{
    public const int Count = 5;
    static BiomeStyle[] all;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { all = null; }

    static Color C(float r, float g, float b, float a = 1f) { return new Color(r, g, b, a); }

    public static BiomeStyle Get(int index)
    {
        if (all == null) Build();
        int i = ((index % Count) + Count) % Count;
        return all[i];
    }

    static void Build()
    {
        BiomeStyle[] a = new BiomeStyle[Count];

        a[0] = new BiomeStyle
        {
            type = BiomeType.Grassland, name = "GRASSLAND",
            sky = C(0.50f, 0.80f, 1.00f), ambient = C(0.70f, 0.78f, 0.90f), sun = C(1f, 0.96f, 0.85f),
            sunIntensity = 1.15f, fogDensity = 0.008f, sunAngle = new Vector3(52f, -30f, 0f),
            trackBase = C(0.13f, 0.34f, 0.12f), trackLine = C(0.34f, 0.62f, 0.25f),
            sideBase = C(0.36f, 0.88f, 0.25f), sideAccent = C(0.62f, 0.97f, 0.30f),
            curb = C(0.62f, 0.45f, 0.28f), hud = C(0.45f, 0.95f, 0.35f),
            particleA = C(1f, 1f, 0.6f, 0.9f), particleB = C(1f, 0.6f, 0.85f, 0.9f),
            particleSize = 0.12f, particleGravity = -0.01f, particleSpeed = 0.4f
        };
        a[1] = new BiomeStyle
        {
            type = BiomeType.Jungle, name = "JUNGLE",
            sky = C(0.42f, 0.86f, 0.70f), ambient = C(0.55f, 0.76f, 0.66f), sun = C(1f, 0.95f, 0.70f),
            sunIntensity = 1.0f, fogDensity = 0.010f, sunAngle = new Vector3(60f, 20f, 0f),
            trackBase = C(0.09f, 0.26f, 0.11f), trackLine = C(0.28f, 0.50f, 0.22f),
            sideBase = C(0.05f, 0.68f, 0.32f), sideAccent = C(0.30f, 0.92f, 0.42f),
            curb = C(0.42f, 0.26f, 0.13f), hud = C(0.20f, 0.95f, 0.55f),
            particleA = C(0.7f, 1f, 0.3f, 0.95f), particleB = C(0.3f, 1f, 0.8f, 0.95f),
            particleSize = 0.15f, particleGravity = -0.01f, particleSpeed = 0.5f
        };
        a[2] = new BiomeStyle
        {
            type = BiomeType.Desert, name = "DESERT",
            sky = C(1.00f, 0.78f, 0.50f), ambient = C(0.92f, 0.76f, 0.60f), sun = C(1f, 0.86f, 0.62f),
            sunIntensity = 1.3f, fogDensity = 0.008f, sunAngle = new Vector3(45f, -50f, 0f),
            trackBase = C(0.46f, 0.30f, 0.14f), trackLine = C(0.70f, 0.50f, 0.28f),
            sideBase = C(1.00f, 0.74f, 0.30f), sideAccent = C(1.00f, 0.58f, 0.20f),
            curb = C(0.78f, 0.40f, 0.20f), hud = C(1.00f, 0.70f, 0.20f),
            particleA = C(1f, 0.85f, 0.5f, 0.5f), particleB = C(1f, 0.7f, 0.4f, 0.5f),
            particleSize = 0.09f, particleGravity = 0f, particleSpeed = 2.5f
        };
        a[3] = new BiomeStyle
        {
            type = BiomeType.IceWorld, name = "ICE WORLD",
            sky = C(0.68f, 0.90f, 1.00f), ambient = C(0.76f, 0.86f, 1.00f), sun = C(0.88f, 0.94f, 1f),
            sunIntensity = 1.1f, fogDensity = 0.009f, sunAngle = new Vector3(40f, 40f, 0f),
            trackBase = C(0.16f, 0.36f, 0.58f), trackLine = C(0.55f, 0.82f, 0.96f),
            sideBase = C(0.90f, 0.97f, 1.00f), sideAccent = C(0.58f, 0.84f, 1.00f),
            curb = C(0.45f, 0.72f, 0.96f), hud = C(0.55f, 0.90f, 1.00f), trackGloss = 0.7f,
            particleA = C(1f, 1f, 1f, 0.95f), particleB = C(0.8f, 0.95f, 1f, 0.95f),
            particleSize = 0.14f, particleGravity = 0.12f, particleSpeed = 0.3f
        };
        a[4] = new BiomeStyle
        {
            type = BiomeType.City, name = "CITY",
            sky = C(0.82f, 0.52f, 0.82f), ambient = C(0.66f, 0.60f, 0.82f), sun = C(1f, 0.72f, 0.62f),
            sunIntensity = 1.0f, fogDensity = 0.007f, sunAngle = new Vector3(35f, -60f, 0f),
            trackBase = C(0.13f, 0.13f, 0.18f), trackLine = C(0.92f, 0.92f, 0.92f),
            sideBase = C(0.52f, 0.58f, 0.76f), sideAccent = C(0.68f, 0.74f, 0.92f),
            curb = C(0.82f, 0.82f, 0.88f), hud = C(1.00f, 0.45f, 0.85f), trackGloss = 0.3f,
            particleA = C(1f, 0.4f, 0.9f, 0.9f), particleB = C(0.3f, 0.9f, 1f, 0.9f),
            particleSize = 0.10f, particleGravity = -0.02f, particleSpeed = 0.5f
        };

        for (int i = 0; i < Count; i++)
        {
            BiomeStyle s = a[i];
            s.trackMat = Gfx.TexMat(MakeTex(s, true), Color.white, s.trackGloss);
            s.sideMat = Gfx.TexMat(MakeTex(s, false), Color.white, 0.08f);
            s.curbMat = Gfx.NewMat(s.curb, 0.2f, 0f);
        }
        all = a;
    }

    // 128x128 tileable ground texture.
    static Texture2D MakeTex(BiomeStyle s, bool track)
    {
        const int N = 128;
        Texture2D tex = new Texture2D(N, N, TextureFormat.RGBA32, true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        tex.anisoLevel = 4;
        Color[] px = new Color[N * N];
        Color baseC = track ? s.trackBase : s.sideBase;
        Color alt = track ? s.trackLine : s.sideAccent;
        int seed = (int)s.type * 31 + (track ? 7 : 11);

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                float u = (x + 0.5f) / N, v = (y + 0.5f) / N;
                float n = Gfx.Noise(u, v, 4, seed) * 0.35f + Gfx.Noise(u, v, 16, seed + 1) * 0.4f + Gfx.Noise(u, v, 32, seed + 2) * 0.25f;
                Color c = Color.Lerp(Gfx.Shade(baseC, 0.82f), Gfx.Shade(baseC, 1.18f), n);
                float h = Gfx.Hash(x, y, seed + 5);

                switch (s.type)
                {
                    case BiomeType.Grassland:
                        c = Gfx.Shade(c, ((y / 32) % 2 == 0) ? 1.07f : 0.93f);
                        if (h > 0.86f) c = Color.Lerp(c, alt, 0.45f);
                        if (!track && h < 0.012f) c = new Color(1f, 0.95f, 0.45f);
                        break;
                    case BiomeType.Jungle:
                    {
                        float dirt = Gfx.Noise(u, v, 6, seed + 9);
                        if (dirt > 0.6f)
                        {
                            Color dc = track ? new Color(0.28f, 0.18f, 0.09f) : new Color(0.10f, 0.45f, 0.20f);
                            c = Color.Lerp(c, dc, Mathf.Clamp01((dirt - 0.6f) * 2.5f));
                        }
                        if (h > 0.93f) c = Color.Lerp(c, alt, 0.6f);
                        break;
                    }
                    case BiomeType.Desert:
                    {
                        float rip = Mathf.Sin(v * Mathf.PI * 2f * 8f + n * 5f);
                        c = Gfx.Shade(c, 1f + rip * 0.07f);
                        if (h > 0.92f) c = Color.Lerp(c, alt, 0.5f);
                        break;
                    }
                    case BiomeType.IceWorld:
                    {
                        float cr = Mathf.Abs(Gfx.Noise(u, v, 5, seed + 7) - 0.5f);
                        if (cr < 0.012f) c = Color.Lerp(c, alt, 0.8f);
                        if (h > 0.985f) c = Color.Lerp(c, Color.white, 0.7f);
                        break;
                    }
                    case BiomeType.City:
                        if (track)
                        {
                            c = Gfx.Shade(c, 0.9f + h * 0.2f);
                        }
                        else
                        {
                            c = Gfx.Shade(c, (((x / 32) + (y / 32)) % 2 == 0) ? 1.05f : 0.95f);
                            if (x % 32 == 0 || y % 32 == 0) c = Gfx.Shade(c, 0.8f);
                        }
                        break;
                }

                if (track)
                {
                    bool dash = (y % 64) < 32;
                    if (dash && (Mathf.Abs(x - 42.67f) < 2f || Mathf.Abs(x - 85.33f) < 2f))
                        c = Color.Lerp(c, alt, 0.9f);
                }
                c.a = 1f;
                px[y * N + x] = c;
            }
        }
        tex.SetPixels(px);
        tex.Apply(true);
        return tex;
    }
}
