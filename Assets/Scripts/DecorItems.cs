using System.Collections.Generic;
using UnityEngine;

// Scenery pieces for each biome. zone: 0 = near path, 1 = mid distance, 2 = far silhouettes.
public static partial class Decor
{
    static readonly Color[] flowerCols =
    {
        new Color(0.95f, 0.20f, 0.25f), new Color(1f, 0.85f, 0.10f), new Color(1f, 0.40f, 0.70f),
        new Color(0.65f, 0.30f, 0.95f), new Color(1f, 1f, 1f), new Color(1f, 0.55f, 0.10f)
    };
    static readonly Color[] treeGreens =
    {
        new Color(0.20f, 0.80f, 0.25f), new Color(0.55f, 0.90f, 0.20f), new Color(0.10f, 0.70f, 0.35f)
    };
    static readonly Color[] jungleGreens =
    {
        new Color(0.05f, 0.55f, 0.25f), new Color(0.10f, 0.75f, 0.35f), new Color(0.00f, 0.50f, 0.45f), new Color(0.50f, 0.85f, 0.10f)
    };
    static readonly Color[] jungleFlowers =
    {
        new Color(0.95f, 0.10f, 0.60f), new Color(1f, 0.50f, 0.10f), new Color(1f, 0.90f, 0.20f), new Color(0.20f, 0.50f, 1f)
    };
    static readonly Color[] rockGray = { new Color(0.60f, 0.62f, 0.68f), new Color(0.72f, 0.74f, 0.80f) };
    static readonly Color[] sandRocks = { new Color(0.85f, 0.45f, 0.25f), new Color(0.92f, 0.58f, 0.32f) };
    static readonly Color[] iceCols =
    {
        new Color(0.40f, 0.88f, 1f), new Color(0.55f, 0.65f, 1f), new Color(0.80f, 0.60f, 1f), new Color(1f, 0.70f, 0.90f)
    };

    // ---------- shared pieces ----------
    static void Rock(Transform t, System.Random r, Color[] cols)
    {
        Material m = M(Pick(r, cols), 0.1f);
        Gfx.Part(t, Gfx.Sphere, m, new Vector3(0f, 0.3f, 0f), new Vector3(F(r, 1f, 2.2f), F(r, 0.7f, 1.4f), F(r, 1f, 2f)),
            new Vector3(F(r, -10f, 10f), F(r, 0f, 360f), F(r, -10f, 10f)));
        if (r.NextDouble() < 0.5)
            Gfx.Part(t, Gfx.Sphere, m, new Vector3(F(r, 0.8f, 1.5f), 0.15f, F(r, -0.6f, 0.6f)), new Vector3(0.9f, 0.6f, 0.8f), Vector3.zero);
    }

    static void HillBlob(Transform t, System.Random r, Color[] cols)
    {
        float w = F(r, 28f, 50f), h = F(r, 10f, 22f);
        Gfx.Ball(t, new Vector3(0f, -h * 0.3f, 0f), new Vector3(w, h, w), M(Pick(r, cols), 0.05f), false);
    }

    static void Mushroom(Transform t, System.Random r, float scale, Color[] caps)
    {
        float sh = 1.1f * scale;
        Gfx.Cyl(t, Vector3.zero, 0.2f * scale, sh, M(new Color(0.96f, 0.92f, 0.80f)));
        Gfx.Ball(t, new Vector3(0f, sh, 0f), new Vector3(1.8f * scale, 1.0f * scale, 1.8f * scale), M(Pick(r, caps), 0.3f));
        Material dot = M(Color.white, 0.2f);
        for (int i = 0; i < 3; i++)
        {
            float a = i * 2.1f + F(r, 0f, 1f);
            Gfx.Ball(t, new Vector3(Mathf.Cos(a) * 0.55f * scale, sh + 0.35f * scale, Mathf.Sin(a) * 0.55f * scale),
                new Vector3(0.25f * scale, 0.12f * scale, 0.25f * scale), dot, false);
        }
    }

    static void Palm(Transform t, System.Random r, Material leaf, Material trunk)
    {
        float h = F(r, 3f, 5.5f);
        Gfx.Part(t, Gfx.Cylinder, trunk, new Vector3(0f, h * 0.5f, 0f), new Vector3(0.36f, h * 0.5f, 0.36f), new Vector3(0f, 0f, F(r, -6f, 6f)));
        for (int i = 0; i < 6; i++)
        {
            GameObject pv = new GameObject("leaf");
            pv.transform.SetParent(t, false);
            pv.transform.localPosition = new Vector3(0f, h, 0f);
            pv.transform.localEulerAngles = new Vector3(0f, i * 60f, 0f);
            Gfx.Part(pv.transform, Gfx.Cube, leaf, new Vector3(0f, 0f, 1.1f), new Vector3(0.6f, 0.06f, 2.4f), new Vector3(18f, 0f, 0f));
        }
        Gfx.Ball(t, new Vector3(0.15f, h - 0.1f, 0.1f), new Vector3(0.3f, 0.3f, 0.3f), M(new Color(0.45f, 0.28f, 0.12f)), false);
    }

    // ---------- Grassland ----------
    static void GrassItem(Transform t, System.Random r, int zone)
    {
        if (zone == 2)
        {
            HillBlob(t, r, new Color[] { new Color(0.25f, 0.80f, 0.30f), new Color(0.15f, 0.72f, 0.55f), new Color(0.50f, 0.85f, 0.25f) });
            return;
        }
        double k = r.NextDouble();
        if (k < 0.45)
        {
            float h = F(r, 2.2f, 3.6f), tr = F(r, 0.22f, 0.38f);
            Gfx.Cyl(t, Vector3.zero, tr, h, M(new Color(0.45f, 0.28f, 0.12f)));
            float cd = F(r, 3f, 4.6f);
            Gfx.Ball(t, new Vector3(0f, h + cd * 0.35f, 0f), new Vector3(cd, cd * 0.9f, cd), M(Pick(r, treeGreens), 0.2f));
            Gfx.Ball(t, new Vector3(F(r, -1f, 1f), h + cd * 0.2f, F(r, -1f, 1f)), new Vector3(cd * 0.7f, cd * 0.65f, cd * 0.7f), M(Pick(r, treeGreens), 0.2f));
            if (r.NextDouble() < 0.4)
            {
                Material apple = M(new Color(0.95f, 0.15f, 0.15f), 0.5f);
                for (int i = 0; i < 3; i++)
                {
                    Vector3 d = new Vector3(F(r, -1f, 1f), F(r, 0f, 0.6f), F(r, -1f, 1f)).normalized;
                    Gfx.Ball(t, new Vector3(0f, h + cd * 0.35f, 0f) + d * cd * 0.5f, new Vector3(0.4f, 0.4f, 0.4f), apple, false);
                }
            }
        }
        else if (k < 0.62)
        {
            int n = 2 + r.Next(2);
            for (int i = 0; i < n; i++)
                Gfx.Ball(t, new Vector3(F(r, -0.8f, 0.8f), 0.5f, F(r, -0.8f, 0.8f)), Vector3.one * F(r, 1.2f, 1.8f), M(Pick(r, treeGreens), 0.2f));
            Gfx.Ball(t, new Vector3(0.3f, 1.0f, 0.5f), Vector3.one * 0.3f, M(Pick(r, flowerCols), 0.4f), false);
        }
        else if (k < 0.86)
        {
            Material stem = M(new Color(0.20f, 0.65f, 0.20f));
            int n = 4 + r.Next(3);
            for (int i = 0; i < n; i++)
            {
                float ox = F(r, -1.5f, 1.5f), oz = F(r, -1.5f, 1.5f), h = F(r, 0.35f, 0.7f);
                Gfx.Cyl(t, new Vector3(ox, 0f, oz), 0.035f, h, stem, false);
                Gfx.Ball(t, new Vector3(ox, h + 0.05f, oz), Vector3.one * F(r, 0.28f, 0.42f), M(Pick(r, flowerCols), 0.35f), false);
            }
        }
        else if (k < 0.94) Rock(t, r, rockGray);
        else Mushroom(t, r, 1.2f, new Color[] { new Color(0.95f, 0.2f, 0.25f), new Color(1f, 0.5f, 0.1f) });
    }

    // ---------- Jungle ----------
    static void JungleItem(Transform t, System.Random r, int zone)
    {
        if (zone == 2)
        {
            HillBlob(t, r, new Color[] { new Color(0.10f, 0.50f, 0.35f), new Color(0.05f, 0.40f, 0.50f), new Color(0.20f, 0.60f, 0.30f) });
            return;
        }
        double k = r.NextDouble();
        if (k < 0.45)
        {
            float h = F(r, 7f, 12f), tr = F(r, 0.5f, 0.85f);
            Gfx.Cyl(t, Vector3.zero, tr, h, M(new Color(0.35f, 0.20f, 0.10f)));
            for (int i = 0; i < 3; i++)
            {
                float cd = F(r, 5f, 8f);
                Gfx.Ball(t, new Vector3(F(r, -2f, 2f), h + F(r, 0f, 1.5f), F(r, -2f, 2f)), new Vector3(cd, cd * 0.7f, cd), M(Pick(r, jungleGreens), 0.2f));
            }
            Material vine = M(new Color(0.15f, 0.55f, 0.15f));
            for (int i = 0; i < 3; i++)
            {
                float len = F(r, 3f, 5f);
                Gfx.Cyl(t, new Vector3(F(r, -2f, 2f), h - 0.5f - len, F(r, -2f, 2f)), 0.04f, len, vine, false);
            }
            for (int i = 0; i < 4; i++)
                Gfx.Ball(t, new Vector3(F(r, -2.5f, 2.5f), h + F(r, 0.3f, 1.2f), F(r, -2.5f, 2.5f)), Vector3.one * 0.6f, M(Pick(r, jungleFlowers), 0.4f), false);
        }
        else if (k < 0.65)
        {
            Palm(t, r, M(new Color(0.10f, 0.70f, 0.30f)), M(new Color(0.40f, 0.26f, 0.12f)));
        }
        else if (k < 0.75) Mushroom(t, r, 1.8f, new Color[] { new Color(0.95f, 0.20f, 0.25f), new Color(0.70f, 0.30f, 0.95f), new Color(1f, 0.55f, 0.10f) });
        else if (k < 0.90)
        {
            float h = F(r, 1.2f, 2.2f);
            Gfx.Cyl(t, Vector3.zero, 0.05f, h, M(new Color(0.15f, 0.60f, 0.20f)), false);
            Material petal = M(Pick(r, jungleFlowers), 0.4f);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                Gfx.Ball(t, new Vector3(Mathf.Cos(a) * 0.45f, h, Mathf.Sin(a) * 0.45f), new Vector3(0.55f, 0.15f, 0.55f), petal, false);
            }
            Gfx.Ball(t, new Vector3(0f, h + 0.05f, 0f), Vector3.one * 0.4f, M(new Color(1f, 0.9f, 0.2f), 0.4f), false);
        }
        else
        {
            Material leaf = M(Pick(r, jungleGreens), 0.2f);
            for (int i = 0; i < 6; i++)
            {
                GameObject pv = new GameObject("fern");
                pv.transform.SetParent(t, false);
                pv.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                pv.transform.localEulerAngles = new Vector3(0f, i * 60f, 0f);
                Gfx.Part(pv.transform, Gfx.Cube, leaf, new Vector3(0f, 0.3f, 0.9f), new Vector3(0.5f, 0.05f, 1.9f), new Vector3(-30f, 0f, 0f), false);
            }
        }
    }

    // ---------- Desert ----------
    static void DesertItem(Transform t, System.Random r, int zone)
    {
        if (zone == 2)
        {
            if (r.NextDouble() < 0.5)
            {
                float rad = F(r, 7f, 13f), h = F(r, 14f, 26f);
                Gfx.Cyl(t, new Vector3(0f, -3f, 0f), rad, h, M(new Color(0.85f, 0.40f, 0.20f), 0.05f), false);
                Gfx.Cyl(t, new Vector3(0f, -3f + h * 0.45f, 0f), rad + 0.1f, h * 0.12f, M(new Color(0.96f, 0.66f, 0.40f), 0.05f), false);
                Gfx.Cyl(t, new Vector3(0f, -3f + h, 0f), rad * 0.98f, 0.8f, M(new Color(0.98f, 0.78f, 0.50f), 0.05f), false);
            }
            else
            {
                float w = F(r, 18f, 34f), h = F(r, 4f, 8f);
                Gfx.Ball(t, new Vector3(0f, -h * 0.3f, 0f), new Vector3(w, h, w), M(new Color(1f, 0.72f, 0.35f), 0.05f), false);
            }
            return;
        }
        double k = r.NextDouble();
        if (k < 0.35)
        {
            Material g = M(Pick(r, new Color[] { new Color(0.20f, 0.70f, 0.35f), new Color(0.35f, 0.80f, 0.30f) }), 0.15f);
            float h = F(r, 2.5f, 4.5f);
            Gfx.Cyl(t, Vector3.zero, 0.33f, h, g);
            Gfx.Ball(t, new Vector3(0f, h, 0f), Vector3.one * 0.66f, g);
            for (int sd = -1; sd <= 1; sd += 2)
            {
                if (r.NextDouble() < 0.6)
                {
                    float ah = F(r, 0.9f, h * 0.6f);
                    float len = F(r, 0.8f, 1.4f);
                    Gfx.CylR(t, new Vector3(sd * 0.55f, ah, 0f), 0.18f, 0.8f, new Vector3(0f, 0f, 90f), g);
                    Gfx.Cyl(t, new Vector3(sd * 0.9f, ah - 0.18f, 0f), 0.18f, len, g);
                    Gfx.Ball(t, new Vector3(sd * 0.9f, ah - 0.18f + len, 0f), Vector3.one * 0.36f, g);
                }
            }
            if (r.NextDouble() < 0.5) Gfx.Ball(t, new Vector3(0f, h + 0.3f, 0f), Vector3.one * 0.35f, M(new Color(1f, 0.4f, 0.7f), 0.4f), false);
        }
        else if (k < 0.60)
        {
            int n = 2 + r.Next(2);
            for (int i = 0; i < n; i++)
                Gfx.Part(t, Gfx.Sphere, M(Pick(r, sandRocks), 0.1f), new Vector3(F(r, -1f, 1f), 0.5f + i * 0.5f, F(r, -0.8f, 0.8f)),
                    new Vector3(F(r, 1.5f, 2.6f), F(r, 1f, 1.6f), F(r, 1.4f, 2.2f)), new Vector3(0f, F(r, 0f, 360f), 0f));
        }
        else if (k < 0.72)
        {
            for (int i = 0; i < 3; i++)
                Gfx.Ball(t, new Vector3(F(r, -0.7f, 0.7f), 0.3f, F(r, -0.7f, 0.7f)), Vector3.one * F(r, 0.6f, 1.1f), M(new Color(0.75f, 0.65f, 0.25f), 0.05f));
        }
        else if (k < 0.85)
        {
            float w = F(r, 6f, 12f);
            Gfx.Ball(t, new Vector3(0f, -w * 0.12f, 0f), new Vector3(w, w * 0.35f, w), M(new Color(1f, 0.75f, 0.35f), 0.05f), false);
        }
        else if (k < 0.93)
        {
            Palm(t, r, M(new Color(0.25f, 0.75f, 0.30f)), M(new Color(0.60f, 0.42f, 0.22f)));
        }
        else
        {
            float h = F(r, 2f, 4f);
            Gfx.Box(t, new Vector3(0f, 0.15f, 0f), new Vector3(1.6f, 0.3f, 1.6f), M(new Color(0.95f, 0.85f, 0.65f)));
            Gfx.Cyl(t, new Vector3(0f, 0.3f, 0f), 0.6f, h, M(new Color(0.95f, 0.85f, 0.65f)));
        }
    }

    // ---------- Ice World ----------
    static void IceItem(Transform t, System.Random r, int zone)
    {
        Material snow = M(new Color(0.97f, 0.99f, 1f), 0.25f);
        if (zone == 2)
        {
            float rad = F(r, 14f, 24f), h = F(r, 30f, 55f);
            Gfx.ConeAt(t, new Vector3(0f, -3f, 0f), rad, h, M(new Color(0.65f, 0.86f, 1f), 0.2f), false);
            Gfx.ConeAt(t, new Vector3(0f, -3f + h * 0.62f, 0f), rad * 0.38f * 1.02f, h * 0.38f * 1.01f, snow, false);
            return;
        }
        double k = r.NextDouble();
        if (k < 0.35)
        {
            Gfx.Cyl(t, Vector3.zero, 0.2f, 1.0f, M(new Color(0.35f, 0.22f, 0.12f)));
            Material pine = M(Pick(r, new Color[] { new Color(0.10f, 0.45f, 0.50f), new Color(0.12f, 0.55f, 0.55f) }), 0.1f);
            for (int i = 0; i < 3; i++)
            {
                float y0 = 0.8f + i * 1.4f, rr = 1.7f - i * 0.45f, hh = 2.2f - i * 0.25f;
                Gfx.ConeAt(t, new Vector3(0f, y0, 0f), rr, hh, pine);
                Gfx.ConeAt(t, new Vector3(0f, y0 + hh * 0.5f, 0f), rr * 0.52f, hh * 0.5f, snow);
            }
        }
        else if (k < 0.60)
        {
            Material ice = Gfx.Mat(Pick(r, iceCols), 0.9f, 0.1f, 0.3f);
            int n = 3 + r.Next(3);
            for (int i = 0; i < n; i++)
            {
                float rr = F(r, 0.35f, 0.8f), h = F(r, 2.5f, 6f);
                Gfx.Part(t, Gfx.Cone, Gfx.Mat(Pick(r, iceCols), 0.9f, 0.1f, 0.3f), new Vector3(F(r, -1f, 1f), 0f, F(r, -1f, 1f)),
                    new Vector3(rr * 2f, h, rr * 2f), new Vector3(F(r, -12f, 12f), F(r, 0f, 360f), F(r, -12f, 12f)));
            }
            if (ice == null) return;
        }
        else if (k < 0.70)
        {
            Gfx.Ball(t, new Vector3(0f, 0.9f, 0f), Vector3.one * 1.8f, snow);
            Gfx.Ball(t, new Vector3(0f, 2.2f, 0f), Vector3.one * 1.3f, snow);
            Gfx.Ball(t, new Vector3(0f, 3.2f, 0f), Vector3.one * 0.9f, snow);
            Material dark = M(new Color(0.12f, 0.10f, 0.20f), 0.2f);
            Gfx.Cyl(t, new Vector3(0f, 3.6f, 0f), 0.35f, 0.4f, dark);
            Gfx.Cyl(t, new Vector3(0f, 3.6f, 0f), 0.5f, 0.05f, dark);
            Gfx.Part(t, Gfx.Cone, M(new Color(1f, 0.5f, 0.1f), 0.3f), new Vector3(0f, 3.2f, 0.4f), new Vector3(0.16f, 0.55f, 0.16f), new Vector3(90f, 0f, 0f), false);
            Gfx.Ball(t, new Vector3(-0.15f, 3.3f, 0.38f), Vector3.one * 0.08f, dark, false);
            Gfx.Ball(t, new Vector3(0.15f, 3.3f, 0.38f), Vector3.one * 0.08f, dark, false);
            Gfx.Cyl(t, new Vector3(0f, 2.75f, 0f), 0.5f, 0.15f, M(new Color(0.9f, 0.15f, 0.2f), 0.3f));
        }
        else if (k < 0.92)
        {
            Gfx.Ball(t, new Vector3(0f, 0f, 0f), new Vector3(F(r, 3f, 6f), F(r, 1f, 2.2f), F(r, 3f, 6f)), snow, false);
        }
        else
        {
            Gfx.Ball(t, new Vector3(0f, 0f, 0f), new Vector3(4f, 4f, 4f), snow);
            Gfx.Box(t, new Vector3(0f, 0.7f, 1.8f), new Vector3(1.2f, 1.4f, 0.8f), M(new Color(0.15f, 0.2f, 0.35f), 0.2f));
        }
    }

    // ---------- City ----------
    static readonly Color[] cityCols =
    {
        new Color(0.95f, 0.30f, 0.60f), new Color(0.25f, 0.55f, 1f), new Color(1f, 0.60f, 0.15f), new Color(0.60f, 0.35f, 0.95f),
        new Color(0.15f, 0.85f, 0.75f), new Color(0.98f, 0.85f, 0.25f), new Color(0.95f, 0.30f, 0.30f), new Color(0.50f, 0.95f, 0.60f)
    };
    static readonly Color[] neonCols =
    {
        new Color(1f, 0.2f, 0.7f), new Color(0.2f, 0.9f, 1f), new Color(1f, 0.9f, 0.2f), new Color(0.5f, 1f, 0.3f)
    };
    static Texture2D[] windowTex;
    static readonly Dictionary<int, Material> bldMats = new Dictionary<int, Material>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetDecor() { windowTex = null; bldMats.Clear(); coinMat = null; }

    static Texture2D MakeWindowTex(Color wall, int seed)
    {
        const int N = 64;
        Texture2D tex = new Texture2D(N, N, TextureFormat.RGBA32, true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[N * N];
        Color frame = Gfx.Shade(wall, 0.8f);
        Color unlit = Gfx.Shade(wall, 0.35f);
        for (int cy = 0; cy < 4; cy++)
        {
            for (int cx = 0; cx < 4; cx++)
            {
                bool lit = Gfx.Hash(cx, cy, seed) > 0.45f;
                Color wc = lit ? ((Gfx.Hash(cx, cy, seed + 3) > 0.5f) ? new Color(1f, 0.92f, 0.55f) : new Color(0.65f, 0.95f, 1f)) : unlit;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        bool win = x >= 3 && x < 13 && y >= 3 && y < 12;
                        px[(cy * 16 + y) * N + cx * 16 + x] = win ? wc : frame;
                    }
                }
            }
        }
        tex.SetPixels(px);
        tex.Apply(true);
        return tex;
    }

    static Material BuildingMat(int ci, float w, float h)
    {
        if (windowTex == null)
        {
            windowTex = new Texture2D[cityCols.Length];
            for (int i = 0; i < cityCols.Length; i++) windowTex[i] = MakeWindowTex(cityCols[i], 40 + i);
        }
        int uT = Mathf.Max(1, Mathf.RoundToInt(w / 4.8f));
        int vT = Mathf.Clamp(Mathf.RoundToInt(h / 4.8f), 1, 16);
        int key = ci * 1000 + uT * 100 + vT;
        Material m;
        if (bldMats.TryGetValue(key, out m) && m != null) return m;
        m = Gfx.TexMat(windowTex[ci], Color.white, 0.35f);
        m.mainTextureScale = new Vector2(uT, vT);
        bldMats[key] = m;
        return m;
    }

    static void Building(Transform seg, System.Random r, int sd, float x, float z, float w, float d, float h, bool details)
    {
        int ci = r.Next(cityCols.Length);
        Gfx.Box(seg, new Vector3(sd * x, h * 0.5f, z), new Vector3(w, h, d), BuildingMat(ci, w, h), details);
        if (!details) return;
        Material roof = M(new Color(0.30f, 0.30f, 0.38f), 0.2f);
        Gfx.Box(seg, new Vector3(sd * x, h + 0.5f, z), new Vector3(w * 0.5f, 1f, d * 0.5f), roof);
        float ah = F(r, 2f, 5f);
        Gfx.Cyl(seg, new Vector3(sd * x, h + 1f, z), 0.06f, ah, roof, false);
        Gfx.Ball(seg, new Vector3(sd * x, h + 1f + ah, z), Vector3.one * 0.25f, Gfx.Mat(new Color(1f, 0.1f, 0.1f), 0.3f, 0f, 2f), false);
        float sy = F(r, 4f, Mathf.Max(5f, Mathf.Min(h - 2f, 10f)));
        Gfx.Box(seg, new Vector3(sd * (x - w * 0.5f - 0.06f), sy, z), new Vector3(0.12f, F(r, 0.6f, 1.2f), d * 0.7f),
            Gfx.Mat(Pick(r, neonCols), 0.4f, 0f, 1.8f), false);
    }

    static void CityRow(BiomeStyle s, Transform seg, System.Random r, float L, int sd)
    {
        // near buildings (3 slots)
        for (int i = 0; i < 3; i++)
        {
            float w = F(r, 5f, 8f), d = F(r, 5f, 8.5f), h = F(r, 8f, 22f);
            Building(seg, r, sd, 7.5f + w * 0.5f, 5f + i * 10f + F(r, -1f, 1f), w, d, h, true);
        }
        // mid buildings (2 slots)
        for (int i = 0; i < 2; i++)
        {
            float w = F(r, 6f, 10f), d = F(r, 6f, 9f), h = F(r, 15f, 38f);
            Building(seg, r, sd, 18f + w * 0.5f + F(r, 0f, 6f), 8f + i * 14f + F(r, -3f, 3f), w, d, h, true);
        }
        // far skyline
        {
            float w = F(r, 10f, 16f), d = F(r, 10f, 16f), h = F(r, 40f, 80f);
            Building(seg, r, sd, F(r, 38f, 70f), F(r, 0f, L), w, d, h, false);
        }
        // street lamps
        Material pole = M(new Color(0.30f, 0.30f, 0.35f), 0.3f);
        Material glow = Gfx.Mat(new Color(1f, 0.88f, 0.5f), 0.4f, 0f, 2f);
        for (int i = 0; i < 3; i++)
        {
            float z = 5f + i * 10f;
            Gfx.Cyl(seg, new Vector3(sd * 3.9f, 0f, z), 0.07f, 4.6f, pole, false);
            Gfx.Box(seg, new Vector3(sd * 3.4f, 4.6f, z), new Vector3(1.0f, 0.08f, 0.08f), pole, false);
            Gfx.Ball(seg, new Vector3(sd * 2.95f, 4.5f, z), Vector3.one * 0.4f, glow, false);
        }
    }
}
