using UnityEngine;

// Particle bursts and procedural audio (no imported assets needed).
public static class Fx
{
    public static void Burst(Vector3 pos, Color c, int count = 14, float speed = 4f, float size = 0.18f, float life = 0.6f, float gravity = 1f)
    {
        GameObject g = new GameObject("FxBurst");
        g.transform.position = pos;
        ParticleSystem ps = g.AddComponent<ParticleSystem>();
        ps.Stop();
        ParticleSystem.MainModule main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = life;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = c;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ParticleSystem.EmissionModule em = ps.emission;
        em.enabled = false;
        ParticleSystem.ShapeModule sh = ps.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = 0.1f;
        ParticleSystem.SizeOverLifetimeModule sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        ParticleSystemRenderer pr = g.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = Gfx.SpriteMat;
        ps.Emit(count);
        Object.Destroy(g, life + 1.5f);
    }
}

public static class Sfx
{
    public static AudioClip Coin, Jump, Crash, Start, Music;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { Coin = null; Jump = null; Crash = null; Start = null; Music = null; }

    public static void Init()
    {
        if (Coin != null) return;
        const float TWO_PI = Mathf.PI * 2f;

        Coin = Make("coin", 0.22f, delegate (float t, float k)
        {
            float f = k < 0.3f ? 988f : 1319f;
            return Mathf.Sin(TWO_PI * f * t) * (1f - k) * (1f - k) * 0.35f;
        });
        Jump = Make("jump", 0.22f, delegate (float t, float k)
        {
            return Mathf.Sin(TWO_PI * (280f * t + 900f * t * t)) * (1f - k) * 0.3f;
        });
        Crash = Make("crash", 0.7f, delegate (float t, float k)
        {
            float noise = Random.value * 2f - 1f;
            return (noise * 0.5f + Mathf.Sin(TWO_PI * 70f * t) * 0.5f) * (1f - k) * (1f - k) * 0.6f;
        });
        Start = Make("start", 0.5f, delegate (float t, float k)
        {
            float f = k < 0.33f ? 523f : (k < 0.66f ? 659f : 784f);
            return Mathf.Sin(TWO_PI * f * t) * (1f - k) * 0.3f;
        });

        float[] scale = { 261.6f, 293.7f, 329.6f, 392f, 440f, 523.3f, 587.3f, 659.3f };
        int[] pattern = { 0, 2, 4, 5, 4, 2, 3, 1, 0, 2, 4, 7, 5, 4, 2, 3 };
        const float step = 0.25f;
        Music = Make("music", step * pattern.Length, delegate (float t, float k)
        {
            int idx = (int)(t / step) % pattern.Length;
            float local = t - Mathf.Floor(t / step) * step;
            float env = Mathf.Exp(-9f * local);
            float f = scale[pattern[idx]];
            float lead = Mathf.Sin(TWO_PI * f * t) * 0.10f * env;
            float bass = (idx % 4 == 0) ? Mathf.Sin(TWO_PI * (f * 0.5f) * t) * 0.12f * Mathf.Exp(-4f * local) : 0f;
            return lead + bass;
        });
    }

    delegate float Gen(float t, float k);

    static AudioClip Make(string name, float dur, Gen gen)
    {
        int rate = 44100;
        int n = (int)(rate * dur);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            d[i] = gen(t, t / dur);
        }
        AudioClip c = AudioClip.Create(name, n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }
}
