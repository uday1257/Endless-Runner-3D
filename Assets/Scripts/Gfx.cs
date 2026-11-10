using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Procedural graphics helpers: meshes, materials (Built-in or URP), noise.
public static class Gfx
{
    static Shader litShader;
    static Mesh cubeMesh, sphereMesh, cylinderMesh, capsuleMesh, coneMesh;
    static Material spriteMat;
    static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();
    static readonly Dictionary<string, Mesh> quadCache = new Dictionary<string, Mesh>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        litShader = null;
        cubeMesh = null; sphereMesh = null; cylinderMesh = null; capsuleMesh = null; coneMesh = null;
        spriteMat = null;
        matCache.Clear();
        quadCache.Clear();
    }

    // ---------------- shaders / materials ----------------
    public static Shader LitShader
    {
        get
        {
            if (litShader != null) return litShader;
            if (GraphicsSettings.currentRenderPipeline != null)
                litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Sprites/Default");
            return litShader;
        }
    }

    public static Material NewMat(Color c, float smooth, float metal)
    {
        Material m = new Material(LitShader);
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal);
        return m;
    }

    public static void SetEmission(Material m, Color c, float k)
    {
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * k);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }

    public static Material Mat(Color c, float smooth = 0.2f, float metal = 0f, float emission = 0f)
    {
        string k = ColorUtility.ToHtmlStringRGB(c) + "|" + smooth.ToString("0.00") + "|" + metal.ToString("0.00") + "|" + emission.ToString("0.00");
        Material m;
        if (matCache.TryGetValue(k, out m) && m != null) return m;
        m = NewMat(c, smooth, metal);
        if (emission > 0f) SetEmission(m, c, emission);
        matCache[k] = m;
        return m;
    }

    public static Material TexMat(Texture2D tex, Color tint, float smooth)
    {
        Material m = NewMat(tint, smooth, 0f);
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
        return m;
    }

    public static Material SpriteMat
    {
        get
        {
            if (spriteMat != null) return spriteMat;
            Shader s = Shader.Find("Sprites/Default");
            if (s == null) s = LitShader;
            spriteMat = new Material(s);
            return spriteMat;
        }
    }

    // ---------------- meshes ----------------
    static Mesh GetPrim(PrimitiveType t)
    {
        GameObject g = GameObject.CreatePrimitive(t);
        Mesh m = g.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(g);
        return m;
    }
    public static Mesh Cube { get { if (cubeMesh == null) cubeMesh = GetPrim(PrimitiveType.Cube); return cubeMesh; } }
    public static Mesh Sphere { get { if (sphereMesh == null) sphereMesh = GetPrim(PrimitiveType.Sphere); return sphereMesh; } }
    public static Mesh Cylinder { get { if (cylinderMesh == null) cylinderMesh = GetPrim(PrimitiveType.Cylinder); return cylinderMesh; } }
    public static Mesh Capsule { get { if (capsuleMesh == null) capsuleMesh = GetPrim(PrimitiveType.Capsule); return capsuleMesh; } }
    public static Mesh Cone { get { if (coneMesh == null) coneMesh = BuildCone(18); return coneMesh; } }

    // Cone: base radius 0.5 at y=0, apex at y=1.
    static Mesh BuildCone(int seg)
    {
        List<Vector3> v = new List<Vector3>();
        List<Vector3> n = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> t = new List<int>();
        for (int i = 0; i < seg; i++)
        {
            float a0 = (float)i / seg * Mathf.PI * 2f;
            float a1 = (float)(i + 1) / seg * Mathf.PI * 2f;
            float am = (a0 + a1) * 0.5f;
            int b = v.Count;
            v.Add(new Vector3(Mathf.Cos(a0) * 0.5f, 0f, Mathf.Sin(a0) * 0.5f));
            v.Add(new Vector3(0f, 1f, 0f));
            v.Add(new Vector3(Mathf.Cos(a1) * 0.5f, 0f, Mathf.Sin(a1) * 0.5f));
            n.Add(new Vector3(Mathf.Cos(a0), 0.5f, Mathf.Sin(a0)).normalized);
            n.Add(new Vector3(Mathf.Cos(am), 0.5f, Mathf.Sin(am)).normalized);
            n.Add(new Vector3(Mathf.Cos(a1), 0.5f, Mathf.Sin(a1)).normalized);
            uv.Add(new Vector2((float)i / seg, 0f));
            uv.Add(new Vector2(((float)i + 0.5f) / seg, 1f));
            uv.Add(new Vector2((float)(i + 1) / seg, 0f));
            t.Add(b); t.Add(b + 1); t.Add(b + 2);
        }
        Mesh m = new Mesh();
        m.name = "Cone";
        m.SetVertices(v);
        m.SetNormals(n);
        m.SetUVs(0, uv);
        m.SetTriangles(t, 0);
        m.RecalculateBounds();
        return m;
    }

    // Horizontal quad (normal up), centered on origin, with controllable UV repeat.
    public static Mesh Ground(float w, float l, float uRep, float vRep)
    {
        string key = w.ToString("0.##") + "_" + l.ToString("0.##") + "_" + uRep.ToString("0.##") + "_" + vRep.ToString("0.##");
        Mesh m;
        if (quadCache.TryGetValue(key, out m) && m != null) return m;
        m = new Mesh();
        m.name = "GroundQuad";
        m.vertices = new Vector3[]
        {
            new Vector3(-w * 0.5f, 0f, -l * 0.5f), new Vector3(-w * 0.5f, 0f, l * 0.5f),
            new Vector3(w * 0.5f, 0f, l * 0.5f), new Vector3(w * 0.5f, 0f, -l * 0.5f)
        };
        m.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, vRep), new Vector2(uRep, vRep), new Vector2(uRep, 0) };
        m.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateBounds();
        quadCache[key] = m;
        return m;
    }

    // ---------------- part builders ----------------
    public static GameObject Part(Transform parent, Mesh mesh, Material mat, Vector3 pos, Vector3 scale, Vector3 euler, bool shadow = true)
    {
        GameObject g = new GameObject("p");
        Transform t = g.transform;
        t.SetParent(parent, false);
        t.localPosition = pos;
        t.localEulerAngles = euler;
        t.localScale = scale;
        g.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer r = g.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (!shadow) r.shadowCastingMode = ShadowCastingMode.Off;
        return g;
    }

    public static GameObject Box(Transform p, Vector3 center, Vector3 size, Material m, bool shadow = true)
    {
        return Part(p, Cube, m, center, size, Vector3.zero, shadow);
    }
    public static GameObject BoxR(Transform p, Vector3 center, Vector3 size, Vector3 euler, Material m, bool shadow = true)
    {
        return Part(p, Cube, m, center, size, euler, shadow);
    }
    public static GameObject Ball(Transform p, Vector3 center, Vector3 size, Material m, bool shadow = true)
    {
        return Part(p, Sphere, m, center, size, Vector3.zero, shadow);
    }
    // Upright cylinder whose BASE sits at basePos.
    public static GameObject Cyl(Transform p, Vector3 basePos, float r, float h, Material m, bool shadow = true)
    {
        return Part(p, Cylinder, m, basePos + Vector3.up * (h * 0.5f), new Vector3(r * 2f, h * 0.5f, r * 2f), Vector3.zero, shadow);
    }
    // Cylinder centered at 'center', length 'len' along its local Y, rotated by euler.
    public static GameObject CylR(Transform p, Vector3 center, float r, float len, Vector3 euler, Material m, bool shadow = true)
    {
        return Part(p, Cylinder, m, center, new Vector3(r * 2f, len * 0.5f, r * 2f), euler, shadow);
    }
    // Upright cone whose BASE sits at basePos.
    public static GameObject ConeAt(Transform p, Vector3 basePos, float r, float h, Material m, bool shadow = true)
    {
        return Part(p, Cone, m, basePos, new Vector3(r * 2f, h, r * 2f), Vector3.zero, shadow);
    }

    // ---------------- misc helpers ----------------
    public static Color Shade(Color c, float k)
    {
        return new Color(Mathf.Clamp01(c.r * k), Mathf.Clamp01(c.g * k), Mathf.Clamp01(c.b * k), 1f);
    }
    public static float R(System.Random r, float a, float b)
    {
        return a + (float)r.NextDouble() * (b - a);
    }

    public static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263 + seed * 1442695041;
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0xFFFF) / 65535f;
        }
    }

    // Tileable value noise (u,v in 0..1 repeat), freq = lattice cells per tile.
    public static float Noise(float u, float v, int freq, int seed)
    {
        u = u - Mathf.Floor(u);
        v = v - Mathf.Floor(v);
        float fx = u * freq, fy = v * freq;
        int ix = (int)fx, iy = (int)fy;
        float tx = fx - ix, ty = fy - iy;
        tx = tx * tx * (3f - 2f * tx);
        ty = ty * ty * (3f - 2f * ty);
        int x0 = ix % freq, x1 = (ix + 1) % freq, y0 = iy % freq, y1 = (iy + 1) % freq;
        float a = Hash(x0, y0, seed), b = Hash(x1, y0, seed), c = Hash(x0, y1, seed), d = Hash(x1, y1, seed);
        return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
    }
}
