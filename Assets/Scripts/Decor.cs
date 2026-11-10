using UnityEngine;

// Builds one 30-unit world segment: ground, curbs, scenery, gates, coins, obstacles.
public static partial class Decor
{
    // ---------- tiny helpers ----------
    static Material M(Color c, float s = 0.15f) { return Gfx.Mat(c, s); }
    static float F(System.Random r, float a, float b) { return Gfx.R(r, a, b); }
    static Color Pick(System.Random r, Color[] a) { return a[r.Next(a.Length)]; }
    static Color C(float r, float g, float b) { return new Color(r, g, b, 1f); }

    // ---------- ground ----------
    public static void BuildGround(BiomeStyle s, Transform seg, float L)
    {
        GameObject g = new GameObject("Track");
        g.transform.SetParent(seg, false);
        g.transform.localPosition = new Vector3(0f, 0f, L * 0.5f);
        BoxCollider bc = g.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, -0.5f, 0f);
        bc.size = new Vector3(WorldGenerator.TrackWidth, 1f, L + 0.2f);
        // darker running surface (textured: grass / moss / sand / ice / asphalt)
        Gfx.Part(g.transform, Gfx.Ground(WorldGenerator.TrackWidth, L, 1f, L / 5f), s.trackMat, Vector3.zero, Vector3.one, Vector3.zero, false);

        for (int sd = -1; sd <= 1; sd += 2)
        {
            // bright colourful surroundings
            Gfx.Part(seg, Gfx.Ground(100f, L, 100f / 8f, L / 8f), s.sideMat,
                new Vector3(sd * (3.3f + 50f), -0.12f, L * 0.5f), Vector3.one, Vector3.zero, false);
            // curb
            Gfx.Box(seg, new Vector3(sd * 3.15f, 0.1f, L * 0.5f), new Vector3(0.3f, 0.3f, L), s.curbMat, false);
        }
    }

    // Archway shown at the start of every new biome.
    public static void BuildGate(BiomeStyle s, Transform seg)
    {
        Material pillar = Gfx.Mat(s.hud, 0.4f, 0f, 0.5f);
        Material dark = M(Gfx.Shade(s.hud, 0.45f), 0.3f);
        for (int sd = -1; sd <= 1; sd += 2)
        {
            Gfx.Cyl(seg, new Vector3(sd * 3.9f, 0f, 1f), 0.35f, 8.4f, dark);
            Gfx.Ball(seg, new Vector3(sd * 3.9f, 8.7f, 1f), new Vector3(0.9f, 0.9f, 0.9f), pillar);
        }
        Gfx.Box(seg, new Vector3(0f, 8.6f, 1f), new Vector3(8.2f, 0.7f, 0.7f), pillar);
    }

    // ---------- scenery ----------
    public static void BuildDecor(BiomeStyle s, Transform seg, System.Random r, float L)
    {
        if (s.type == BiomeType.City)
        {
            for (int sd = -1; sd <= 1; sd += 2) CityRow(s, seg, r, L, sd);
            return;
        }
        for (int sd = -1; sd <= 1; sd += 2)
        {
            for (int i = 0; i < 7; i++) Spawn(s, seg, new Vector3(sd * F(r, 4.8f, 13f), 0f, F(r, 0f, L)), r, 0);
            for (int i = 0; i < 4; i++) Spawn(s, seg, new Vector3(sd * F(r, 14f, 32f), 0f, F(r, 0f, L)), r, 1);
            for (int i = 0; i < 2; i++) Spawn(s, seg, new Vector3(sd * F(r, 45f, 95f), 0f, F(r, 0f, L)), r, 2);
            if (s.type == BiomeType.Grassland) Fence(seg, sd, L);
        }
    }

    static void Spawn(BiomeStyle s, Transform seg, Vector3 pos, System.Random r, int zone)
    {
        GameObject root = new GameObject("deco");
        Transform t = root.transform;
        t.SetParent(seg, false);
        t.localPosition = pos;
        t.localEulerAngles = new Vector3(0f, F(r, 0f, 360f), 0f);
        if (zone == 1) t.localScale = Vector3.one * F(r, 1.2f, 1.8f);
        switch (s.type)
        {
            case BiomeType.Grassland: GrassItem(t, r, zone); break;
            case BiomeType.Jungle: JungleItem(t, r, zone); break;
            case BiomeType.Desert: DesertItem(t, r, zone); break;
            case BiomeType.IceWorld: IceItem(t, r, zone); break;
        }
    }

    static void Fence(Transform seg, int sd, float L)
    {
        Material wood = M(C(0.88f, 0.68f, 0.42f), 0.1f);
        for (int i = 0; i < 6; i++)
            Gfx.Cyl(seg, new Vector3(sd * 4.3f, 0f, 2.5f + i * 5f), 0.08f, 1.0f, wood);
        Gfx.Box(seg, new Vector3(sd * 4.3f, 0.78f, L * 0.5f), new Vector3(0.08f, 0.10f, L), wood);
        Gfx.Box(seg, new Vector3(sd * 4.3f, 0.45f, L * 0.5f), new Vector3(0.08f, 0.10f, L), wood);
    }

    // ---------- coins ----------
    static Material coinMat;
    public static void MakeCoin(Transform seg, Vector3 localPos)
    {
        if (coinMat == null) coinMat = Gfx.Mat(C(1f, 0.80f, 0.10f), 0.7f, 0.3f, 0.6f);
        GameObject g = new GameObject("Coin");
        g.transform.SetParent(seg, false);
        g.transform.localPosition = localPos;
        SphereCollider sc = g.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.7f;
        Coin coin = g.AddComponent<Coin>();
        GameObject vis = Gfx.Part(g.transform, Gfx.Cylinder, coinMat, Vector3.zero, new Vector3(0.8f, 0.07f, 0.8f), new Vector3(90f, 0f, 0f), false);
        coin.visual = vis.transform;
    }

    // ---------- obstacles ----------
    // Returns an object with Obstacle + BoxCollider. width>3 => full-width low barrier.
    public static GameObject MakeObstacle(BiomeStyle s, Transform seg, float x, float z, bool tall, float width, System.Random r)
    {
        GameObject g = new GameObject(tall ? "Obstacle_Tall" : "Obstacle_Low");
        g.transform.SetParent(seg, false);
        g.transform.localPosition = new Vector3(x, 0f, z);
        Obstacle ob = g.AddComponent<Obstacle>();
        ob.tall = tall;
        Transform t = g.transform;
        Vector3 size;
        switch (s.type)
        {
            case BiomeType.Grassland: size = tall ? GrassTall(t, r) : GrassLow(t, r, width); break;
            case BiomeType.Jungle: size = tall ? JungleTall(t, r) : JungleLow(t, r, width); break;
            case BiomeType.Desert: size = tall ? DesertTall(t, r) : DesertLow(t, r, width); break;
            case BiomeType.IceWorld: size = tall ? IceTall(t, r) : IceLow(t, r, width); break;
            default: size = tall ? CityTall(t, r) : CityLow(t, r, width); break;
        }
        BoxCollider bc = g.AddComponent<BoxCollider>();
        bc.size = size;
        bc.center = new Vector3(0f, size.y * 0.5f, 0f);
        return g;
    }

    // ----- Grassland -----
    static Vector3 GrassTall(Transform t, System.Random r)
    {
        if (r.NextDouble() < 0.5)
        {
            Gfx.Cyl(t, Vector3.zero, 0.9f, 2.4f, M(C(0.50f, 0.32f, 0.15f)));
            Gfx.Cyl(t, new Vector3(0f, 2.4f, 0f), 0.85f, 0.06f, M(C(0.85f, 0.65f, 0.40f)));
            Gfx.Cyl(t, new Vector3(0.5f, 0f, 0.3f), 0.18f, 1.6f, M(C(0.9f, 0.9f, 0.85f)));
            Gfx.Ball(t, new Vector3(0.5f, 1.7f, 0.3f), new Vector3(0.7f, 0.4f, 0.7f), M(C(0.95f, 0.2f, 0.25f)));
        }
        else
        {
            Gfx.Ball(t, new Vector3(0f, 0.85f, 0f), new Vector3(1.9f, 1.7f, 1.7f), M(C(0.62f, 0.64f, 0.72f)));
            Gfx.Ball(t, new Vector3(0.2f, 1.9f, 0f), new Vector3(1.3f, 1.2f, 1.2f), M(C(0.72f, 0.74f, 0.80f)));
            Gfx.Ball(t, new Vector3(0f, 2.35f, 0f), new Vector3(0.9f, 0.4f, 0.8f), M(C(0.3f, 0.8f, 0.3f)));
        }
        return new Vector3(1.8f, 2.5f, 1.8f);
    }
    static Vector3 GrassLow(Transform t, System.Random r, float width)
    {
        bool full = width > 3f;
        double k = r.NextDouble();
        if (k < 0.5)
        {
            Material log = M(C(0.55f, 0.34f, 0.16f));
            Gfx.CylR(t, new Vector3(0f, 0.5f, 0f), 0.5f, width, new Vector3(0f, 0f, 90f), log);
            Gfx.CylR(t, new Vector3(width * 0.5f + 0.01f, 0.5f, 0f), 0.42f, 0.04f, new Vector3(0f, 0f, 90f), M(C(0.85f, 0.65f, 0.4f)));
            return new Vector3(width, 1f, 1f);
        }
        if (k < 0.8 || full)
        {
            Material wood = M(C(0.9f, 0.7f, 0.45f));
            Material red = M(C(0.9f, 0.25f, 0.25f));
            float hw = width * 0.5f - 0.1f;
            Gfx.Cyl(t, new Vector3(-hw, 0f, 0f), 0.1f, 1f, wood);
            Gfx.Cyl(t, new Vector3(hw, 0f, 0f), 0.1f, 1f, wood);
            Gfx.Box(t, new Vector3(0f, 0.8f, 0f), new Vector3(width, 0.14f, 0.12f), red);
            Gfx.Box(t, new Vector3(0f, 0.45f, 0f), new Vector3(width, 0.14f, 0.12f), wood);
            return new Vector3(width, 1f, 0.4f);
        }
        Gfx.Ball(t, new Vector3(0f, 0.2f, 0f), new Vector3(1.8f, 1.6f, 1.4f), M(C(0.62f, 0.64f, 0.72f)));
        return new Vector3(1.8f, 1f, 1.4f);
    }

    // ----- Jungle -----
    static Vector3 JungleTall(Transform t, System.Random r)
    {
        if (r.NextDouble() < 0.55)
        {
            Gfx.Cyl(t, Vector3.zero, 0.8f, 3.2f, M(C(0.32f, 0.19f, 0.10f)));
            Gfx.Cyl(t, new Vector3(0f, 1.0f, 0f), 0.83f, 0.5f, M(C(0.20f, 0.62f, 0.22f)));
            Gfx.Cyl(t, new Vector3(0.45f, 1.5f, 0.45f), 0.05f, 1.7f, M(C(0.15f, 0.55f, 0.15f)), false);
            Gfx.Ball(t, new Vector3(0f, 3.3f, 0f), new Vector3(1.6f, 0.5f, 1.6f), M(C(0.1f, 0.55f, 0.25f)));
        }
        else
        {
            Gfx.Ball(t, new Vector3(0f, 1.3f, 0f), new Vector3(2.0f, 2.6f, 1.8f), M(C(0.42f, 0.52f, 0.42f)));
            Gfx.Ball(t, new Vector3(0f, 2.5f, 0f), new Vector3(1.5f, 0.7f, 1.3f), M(C(0.18f, 0.72f, 0.28f)));
        }
        return new Vector3(1.8f, 3.2f, 1.8f);
    }
    static Vector3 JungleLow(Transform t, System.Random r, float width)
    {
        if (r.NextDouble() < 0.7 || width > 3f)
        {
            Gfx.CylR(t, new Vector3(0f, 0.5f, 0f), 0.5f, width, new Vector3(0f, 0f, 90f), M(C(0.42f, 0.25f, 0.12f)));
            Gfx.Box(t, new Vector3(0f, 1.0f, 0f), new Vector3(width * 0.4f, 0.1f, 0.55f), M(C(0.25f, 0.72f, 0.2f)));
            Gfx.Ball(t, new Vector3(width * 0.25f, 1.0f, 0.1f), new Vector3(0.4f, 0.4f, 0.4f), M(C(0.95f, 0.2f, 0.6f)));
            return new Vector3(width, 1f, 1f);
        }
        Gfx.Ball(t, new Vector3(0f, 0.1f, 0f), new Vector3(1.8f, 1.8f, 1.5f), M(C(0.45f, 0.52f, 0.45f)));
        Gfx.Ball(t, new Vector3(0f, 0.85f, 0f), new Vector3(1.2f, 0.4f, 1.0f), M(C(0.2f, 0.7f, 0.25f)));
        return new Vector3(1.8f, 1f, 1.5f);
    }

    // ----- Desert -----
    static Vector3 DesertTall(Transform t, System.Random r)
    {
        if (r.NextDouble() < 0.55)
        {
            Material g = M(C(0.20f, 0.66f, 0.30f));
            Gfx.Cyl(t, Vector3.zero, 0.5f, 3f, g);
            Gfx.Ball(t, new Vector3(0f, 3f, 0f), new Vector3(1f, 1f, 1f), g);
            Gfx.CylR(t, new Vector3(-0.75f, 1.6f, 0f), 0.25f, 0.9f, new Vector3(0f, 0f, 90f), g);
            Gfx.Cyl(t, new Vector3(-1.1f, 1.45f, 0f), 0.25f, 1.0f, g);
            Gfx.Ball(t, new Vector3(-1.1f, 2.45f, 0f), new Vector3(0.5f, 0.5f, 0.5f), g);
            Gfx.Ball(t, new Vector3(0f, 3.4f, 0f), new Vector3(0.4f, 0.4f, 0.4f), M(C(1f, 0.4f, 0.7f)));
            return new Vector3(2.0f, 3.2f, 1.2f);
        }
        Gfx.Cyl(t, Vector3.zero, 0.85f, 2.8f, M(C(0.86f, 0.46f, 0.26f)));
        Gfx.Cyl(t, new Vector3(0f, 1.2f, 0f), 0.88f, 0.35f, M(C(0.96f, 0.66f, 0.40f)));
        Gfx.Cyl(t, new Vector3(0f, 2.8f, 0f), 0.95f, 0.15f, M(C(0.95f, 0.55f, 0.30f)));
        return new Vector3(1.8f, 2.9f, 1.8f);
    }
    static Vector3 DesertLow(Transform t, System.Random r, float width)
    {
        if (r.NextDouble() < 0.65 || width > 3f)
        {
            Gfx.Box(t, new Vector3(0f, 0.45f, 0f), new Vector3(width * 0.98f, 0.9f, 1f), M(C(0.86f, 0.50f, 0.30f)));
            Gfx.Box(t, new Vector3(0f, 0.95f, 0f), new Vector3(width, 0.1f, 1.06f), M(C(0.98f, 0.72f, 0.45f)));
            Gfx.Box(t, new Vector3(0f, 0.55f, 0.51f), new Vector3(width * 0.9f, 0.12f, 0.02f), M(C(0.7f, 0.35f, 0.2f)), false);
            return new Vector3(width, 1f, 1f);
        }
        Gfx.CylR(t, new Vector3(0f, 0.45f, 0f), 0.45f, width, new Vector3(0f, 0f, 90f), M(C(0.70f, 0.55f, 0.32f)));
        Gfx.Ball(t, new Vector3(0.3f, 0.95f, 0f), new Vector3(0.5f, 0.5f, 0.5f), M(C(0.2f, 0.66f, 0.3f)));
        return new Vector3(width, 0.95f, 1f);
    }

    // ----- Ice World -----
    static Vector3 IceTall(Transform t, System.Random r)
    {
        if (r.NextDouble() < 0.5)
        {
            Material ice = Gfx.Mat(C(0.45f, 0.88f, 1f), 0.9f, 0.1f, 0.25f);
            Material ice2 = Gfx.Mat(C(0.70f, 0.70f, 1f), 0.9f, 0.1f, 0.25f);
            Gfx.ConeAt(t, Vector3.zero, 0.9f, 2.8f, ice);
            Gfx.ConeAt(t, new Vector3(-0.5f, 0f, 0.2f), 0.55f, 2.0f, ice2);
            Gfx.ConeAt(t, new Vector3(0.5f, 0f, -0.2f), 0.5f, 1.7f, ice);
        }
        else
        {
            Gfx.Box(t, new Vector3(0f, 1.3f, 0f), new Vector3(1.7f, 2.6f, 1.7f), Gfx.Mat(C(0.40f, 0.82f, 1f), 0.95f, 0.1f, 0.15f));
            Gfx.Box(t, new Vector3(0f, 2.7f, 0f), new Vector3(1.8f, 0.2f, 1.8f), M(C(1f, 1f, 1f), 0.1f));
        }
        return new Vector3(1.8f, 2.8f, 1.8f);
    }
    static Vector3 IceLow(Transform t, System.Random r, float width)
    {
        if (r.NextDouble() < 0.6 || width > 3f)
        {
            Gfx.Box(t, new Vector3(0f, 0.45f, 0f), new Vector3(width * 0.98f, 0.9f, 0.9f), Gfx.Mat(C(0.45f, 0.85f, 1f), 0.95f, 0.1f, 0.15f));
            int n = width > 3f ? 6 : 2;
            for (int i = 0; i < n; i++)
            {
                float px = (n == 1) ? 0f : Mathf.Lerp(-width * 0.4f, width * 0.4f, i / (float)(n - 1));
                Gfx.ConeAt(t, new Vector3(px, 0.9f, 0f), 0.18f, 0.45f, M(C(1f, 1f, 1f), 0.4f));
            }
            return new Vector3(width, 1f, 0.9f);
        }
        Gfx.Ball(t, new Vector3(0f, 0f, 0f), new Vector3(1.9f, 1.9f, 1.4f), M(C(0.95f, 0.98f, 1f), 0.3f));
        Gfx.ConeAt(t, new Vector3(0.4f, 0.7f, 0f), 0.25f, 0.5f, Gfx.Mat(C(0.5f, 0.9f, 1f), 0.9f, 0.1f, 0.3f));
        return new Vector3(1.8f, 0.95f, 1.4f);
    }

    // ----- City -----
    static readonly Color[] cityObj =
    {
        new Color(0.95f, 0.50f, 0.15f), new Color(0.10f, 0.60f, 0.70f),
        new Color(0.85f, 0.22f, 0.28f), new Color(0.25f, 0.38f, 0.90f)
    };
    static Vector3 CityTall(Transform t, System.Random r)
    {
        if (r.NextDouble() < 0.5)
        {
            Color c = Pick(r, cityObj);
            Gfx.Box(t, new Vector3(0f, 1.2f, 0f), new Vector3(1.8f, 2.4f, 3.6f), M(c, 0.3f));
            Material dk = M(Gfx.Shade(c, 0.6f), 0.3f);
            for (int i = 0; i < 4; i++) Gfx.Box(t, new Vector3(0f, 1.2f, -1.35f + i * 0.9f), new Vector3(1.86f, 2.4f, 0.08f), dk, false);
            return new Vector3(1.8f, 2.4f, 3.6f);
        }
        Gfx.Box(t, new Vector3(0f, 0.95f, 0f), new Vector3(1.8f, 1.3f, 3.2f), M(C(1f, 0.85f, 0.10f), 0.4f));
        Gfx.Box(t, new Vector3(0f, 1.95f, -0.2f), new Vector3(1.6f, 0.9f, 1.8f), M(C(0.7f, 0.9f, 1f), 0.8f));
        Material tire = M(C(0.05f, 0.05f, 0.05f), 0.2f);
        for (int i = 0; i < 4; i++)
        {
            float wx = (i % 2 == 0) ? -0.9f : 0.9f;
            float wz = (i < 2) ? -1.0f : 1.0f;
            Gfx.CylR(t, new Vector3(wx, 0.35f, wz), 0.35f, 0.25f, new Vector3(0f, 0f, 90f), tire);
        }
        Gfx.Box(t, new Vector3(0f, 2.5f, -0.2f), new Vector3(0.6f, 0.2f, 0.3f), Gfx.Mat(C(1f, 1f, 0.6f), 0.3f, 0f, 1.5f));
        return new Vector3(1.8f, 2.6f, 3.2f);
    }
    static Vector3 CityLow(Transform t, System.Random r, float width)
    {
        double k = r.NextDouble();
        if (k < 0.45 || width > 3f && k < 0.8)
        {
            Material red = M(C(0.92f, 0.2f, 0.2f), 0.3f);
            Material white = M(C(0.97f, 0.97f, 0.97f), 0.3f);
            int n = Mathf.Max(2, Mathf.FloorToInt(width / 0.7f));
            float w = width / n;
            for (int i = 0; i < n; i++)
                Gfx.Box(t, new Vector3(-width * 0.5f + w * (i + 0.5f), 0.75f, 0f), new Vector3(w, 0.35f, 0.2f), (i % 2 == 0) ? red : white);
            Material leg = M(C(0.4f, 0.4f, 0.45f), 0.2f);
            Gfx.Box(t, new Vector3(-width * 0.5f + 0.15f, 0.3f, 0f), new Vector3(0.1f, 0.6f, 0.4f), leg);
            Gfx.Box(t, new Vector3(width * 0.5f - 0.15f, 0.3f, 0f), new Vector3(0.1f, 0.6f, 0.4f), leg);
            return new Vector3(width, 0.95f, 0.4f);
        }
        if (k < 0.8 || width > 3f)
        {
            Material orange = M(C(1f, 0.5f, 0.1f), 0.3f);
            Material white = M(C(1f, 1f, 1f), 0.3f);
            int n = width > 3f ? 6 : 2;
            for (int i = 0; i < n; i++)
            {
                float px = (n == 1) ? 0f : Mathf.Lerp(-width * 0.45f, width * 0.45f, i / (float)(n - 1));
                Gfx.ConeAt(t, new Vector3(px, 0f, 0f), 0.3f, 0.85f, orange);
                Gfx.ConeAt(t, new Vector3(px, 0.4f, 0f), 0.17f, 0.2f, white);
            }
            return new Vector3(width, 0.85f, 0.6f);
        }
        Gfx.Box(t, new Vector3(0f, 0.45f, 0f), new Vector3(width, 0.9f, 0.9f), M(C(0.6f, 0.6f, 0.65f), 0.1f));
        Gfx.Box(t, new Vector3(0f, 0.9f, 0.46f), new Vector3(width * 0.9f, 0.1f, 0.05f), M(C(1f, 0.85f, 0.1f), 0.3f), false);
        return new Vector3(width, 0.9f, 0.9f);
    }
}
