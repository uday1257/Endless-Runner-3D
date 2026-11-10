using System.Collections.Generic;
using UnityEngine;

// Streams endless 30-unit segments ahead of the player and removes them behind.
// Biomes cycle: Grassland -> Jungle -> Desert -> Ice World -> City -> ...
public class WorldGenerator : MonoBehaviour
{
    public const float SegLen = 30f;
    public const float TrackWidth = 6f;
    public const int SegsPerBiome = 8;
    public static readonly float[] LaneX = { -2f, 0f, 2f };

    // -1 = all biomes cycle; 0..4 = stay in that single biome.
    public static int Selected = -1;

    public Transform player;
    public int segmentsAhead = 7;
    public int segmentsBehind = 2;

    class Seg { public int index; public GameObject go; }
    readonly List<Seg> segs = new List<Seg>();
    int nextIndex;
    float nextRowZ, lastRowZ, lastFreeX;

    public static int BiomeIndex(int segIndex)
    {
        if (Selected >= 0) return Selected;
        if (segIndex < 0) return 0;
        return (segIndex / SegsPerBiome) % Biomes.Count;
    }

    public static int BiomeIndexAt(float z)
    {
        return BiomeIndex(Mathf.FloorToInt(z / SegLen));
    }

    // Blend info used for sky / fog / light transitions.
    public static void EnvAt(float z, out BiomeStyle a, out BiomeStyle b, out float t)
    {
        float len = SegLen * SegsPerBiome;
        float f = z / len;
        int bi = Mathf.FloorToInt(f);
        float frac = f - bi;
        a = Biomes.Get(BiomeIndex(Mathf.Max(0, bi) * SegsPerBiome));
        b = Biomes.Get(BiomeIndex((Mathf.Max(0, bi) + 1) * SegsPerBiome));
        float w = 25f / len;
        t = 0f;
        if (Selected < 0 && z > 0f && frac > 1f - w)
        {
            float k = (frac - (1f - w)) / w;
            t = k * k * (3f - 2f * k);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < segs.Count; i++)
            if (segs[i].go != null) Destroy(segs[i].go);
        segs.Clear();
    }

    public void Rebuild()
    {
        Clear();
        nextIndex = -1;
        nextRowZ = 2f * SegLen + 10f;
        lastRowZ = 20f;
        lastFreeX = 0f;
        SpawnUpTo(segmentsAhead * SegLen);
    }

    void Update()
    {
        if (player == null) return;
        SpawnUpTo(player.position.z + segmentsAhead * SegLen);
        while (segs.Count > 0 && (segs[0].index + 1) * SegLen < player.position.z - segmentsBehind * SegLen - 10f)
        {
            if (segs[0].go != null) Destroy(segs[0].go);
            segs.RemoveAt(0);
        }
    }

    void SpawnUpTo(float zLimit)
    {
        while (nextIndex * SegLen < zLimit)
        {
            Spawn(nextIndex);
            nextIndex++;
        }
    }

    void Spawn(int idx)
    {
        BiomeStyle s = Biomes.Get(BiomeIndex(idx));
        GameObject go = new GameObject("Segment_" + idx + "_" + s.name);
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(0f, 0f, idx * SegLen);
        System.Random r = new System.Random(idx * 7919 + 17);

        Decor.BuildGround(s, go.transform, SegLen);
        Decor.BuildDecor(s, go.transform, r, SegLen);
        if (Selected < 0 && idx > 0 && idx % SegsPerBiome == 0) Decor.BuildGate(s, go.transform);
        PlaceRows(idx, s, go.transform, r);

        Seg sg = new Seg();
        sg.index = idx;
        sg.go = go;
        segs.Add(sg);
    }

    void PlaceRows(int idx, BiomeStyle s, Transform seg, System.Random r)
    {
        float segStart = idx * SegLen;
        float segEnd = segStart + SegLen;
        while (nextRowZ < segEnd)
        {
            float rz = nextRowZ;
            float lz = rz - segStart;
            float freeX;
            bool arc = false;

            int pat = PickPattern(r);
            switch (pat)
            {
                case 0: // one tall obstacle
                {
                    int a = r.Next(3);
                    Decor.MakeObstacle(s, seg, LaneX[a], lz, true, 1.8f, r);
                    freeX = LaneX[(a + 1 + r.Next(2)) % 3];
                    break;
                }
                case 1: // two tall obstacles, one free lane
                {
                    int free = r.Next(3);
                    for (int l = 0; l < 3; l++)
                        if (l != free) Decor.MakeObstacle(s, seg, LaneX[l], lz, true, 1.8f, r);
                    freeX = LaneX[free];
                    break;
                }
                case 2: // full-width low barrier -> jump over it
                {
                    Decor.MakeObstacle(s, seg, 0f, lz, false, TrackWidth, r);
                    freeX = 0f;
                    arc = true;
                    break;
                }
                case 3: // one low obstacle
                {
                    int a = r.Next(3);
                    Decor.MakeObstacle(s, seg, LaneX[a], lz, false, 1.8f, r);
                    freeX = LaneX[(a + 1 + r.Next(2)) % 3];
                    break;
                }
                default: // tall + low, one free lane
                {
                    int a = r.Next(3);
                    int b = (a + 1 + r.Next(2)) % 3;
                    int free = 3 - a - b;
                    Decor.MakeObstacle(s, seg, LaneX[a], lz, true, 1.8f, r);
                    Decor.MakeObstacle(s, seg, LaneX[b], lz, false, 1.8f, r);
                    freeX = LaneX[free];
                    break;
                }
            }

            // coins guiding the player from the previous free lane to this one
            float z1 = lastRowZ + 4f, z2 = rz - 3.5f;
            if (z2 > z1)
            {
                int n = Mathf.FloorToInt((z2 - z1) / 1.5f) + 1;
                for (int i = 0; i < n; i++)
                {
                    float k = (n == 1) ? 0f : i / (float)(n - 1);
                    float sm = k * k * (3f - 2f * k);
                    Decor.MakeCoin(seg, new Vector3(Mathf.Lerp(lastFreeX, freeX, sm), 1.0f, z1 + i * 1.5f - segStart));
                }
            }
            // jump arc over full-width barriers
            if (arc)
            {
                for (int i = 0; i < 5; i++)
                {
                    float y = 1.0f + 1.9f * Mathf.Sin(Mathf.PI * (i / 4f));
                    Decor.MakeCoin(seg, new Vector3(0f, y, lz - 3f + i * 1.5f));
                }
            }

            lastRowZ = rz + (arc ? 3f : 0f);
            lastFreeX = freeX;
            float gap = Gfx.R(r, 11f, 14f) - Mathf.Min(3f, idx * 0.08f);
            nextRowZ = rz + Mathf.Max(9f, gap);
        }
    }

    static int PickPattern(System.Random r)
    {
        int v = r.Next(100);
        if (v < 30) return 0;
        if (v < 55) return 1;
        if (v < 75) return 2;
        if (v < 90) return 3;
        return 4;
    }
}
