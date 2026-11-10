using UnityEngine;
using UnityEngine.Rendering;

public enum GameState { Menu, Playing, Paused, GameOver }

// Creates and runs everything: camera, light, player, endless world, HUD, audio.
// Drop this on any empty GameObject (the JungleLevel scene already has it).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State = GameState.Menu;
    public int Coins { get; private set; }
    public float Distance { get; private set; }
    public int Best { get; private set; }
    public const int MaxLives = 3;
    public int Lives { get; private set; }
    int menuChoice = -1;   // last chosen biome (-1 = mix of all)
    public int Score { get { return Coins * 10 + Mathf.FloorToInt(Distance); } }

    Camera cam;
    CameraFollow camFollow;
    Light sun;
    PlayerController player;
    WorldGenerator world;
    AudioSource sfxSrc, musicSrc;
    ParticleSystem ambient;

    float gameOverAt;
    bool newBest;
    int lastBiome = -1;
    string toast = "";
    float toastAt = -10f;
    int combo;
    float lastCoinTime = -10f;

    // GUI
    GUIStyle lbl, btn;

    // ------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Physics.gravity = new Vector3(0f, -28f, 0f);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Best = PlayerPrefs.GetInt("JR_Best", 0);

        Sfx.Init();
        SetupAudio();
        SetupLight();
        SetupCamera();
        SetupPlayer();
        SetupWorld();
        SetupAmbient();

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.ambientMode = AmbientMode.Flat;

        world.Rebuild();
        player.ResetPlayer();
        camFollow.menuOrbit = true;
        camFollow.Snap();
        ApplyEnvironment();
        State = GameState.Menu;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    // ------------------------------------------------------------ setup
    void SetupAudio()
    {
        sfxSrc = gameObject.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        musicSrc = gameObject.AddComponent<AudioSource>();
        musicSrc.clip = Sfx.Music;
        musicSrc.loop = true;
        musicSrc.volume = 0.35f;
        musicSrc.playOnAwake = false;
        musicSrc.Play();
    }

    void SetupLight()
    {
        GameObject existing = GameObject.Find("Directional Light");
        if (existing != null) sun = existing.GetComponent<Light>();
        if (sun == null)
        {
            GameObject g = new GameObject("Directional Light");
            sun = g.AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.shadows = LightShadows.Soft;
        QualitySettings.shadowDistance = 60f;
    }

    void SetupCamera()
    {
        cam = Camera.main;
        if (cam == null)
        {
            GameObject c = new GameObject("Main Camera");
            c.tag = "MainCamera";
            cam = c.AddComponent<Camera>();
            c.AddComponent<AudioListener>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 350f;
        cam.fieldOfView = 60f;
        camFollow = cam.GetComponent<CameraFollow>();
        if (camFollow == null) camFollow = cam.gameObject.AddComponent<CameraFollow>();
    }

    void SetupPlayer()
    {
        GameObject found = GameObject.Find("Player");
        if (found != null) player = found.GetComponent<PlayerController>();
        if (player == null)
        {
            GameObject p = new GameObject("Player");
            p.transform.position = new Vector3(0f, 1.05f, 0f);
            player = p.AddComponent<PlayerController>();
        }
        if (player.GetComponent<CoinCollector>() == null) player.gameObject.AddComponent<CoinCollector>();
        camFollow.player = player.transform;
    }

    void SetupWorld()
    {
        GameObject w = new GameObject("World");
        world = w.AddComponent<WorldGenerator>();
        world.player = player.transform;
    }

    void SetupAmbient()
    {
        GameObject g = new GameObject("AmbientParticles");
        ambient = g.AddComponent<ParticleSystem>();
        ambient.Stop();
        ParticleSystem.MainModule main = ambient.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 7f;
        main.maxParticles = 400;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSize = 0.12f;
        main.startSpeed = 0.4f;
        ParticleSystem.EmissionModule em = ambient.emission;
        em.rateOverTime = 45f;
        ParticleSystem.ShapeModule sh = ambient.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = new Vector3(60f, 18f, 70f);
        g.GetComponent<ParticleSystemRenderer>().sharedMaterial = Gfx.SpriteMat;
        ambient.Play();
    }

    void StyleAmbient(BiomeStyle b)
    {
        ParticleSystem.MainModule main = ambient.main;
        main.startColor = new ParticleSystem.MinMaxGradient(b.particleA, b.particleB);
        main.startSize = b.particleSize;
        main.startSpeed = b.particleSpeed;
        main.gravityModifier = b.particleGravity;
    }

    // ------------------------------------------------------------ game flow
    void StartGame()
    {
        State = GameState.Playing;
        Coins = 0; Distance = 0f; combo = 0; Lives = MaxLives;
        camFollow.menuOrbit = false;
        player.Begin();
        PlaySfx(Sfx.Start, 0.7f);
        lastBiome = -1;
    }

    void Restart()
    {
        Time.timeScale = 1f;
        world.Rebuild();
        player.ResetPlayer();
        camFollow.menuOrbit = false;
        camFollow.Snap();
        StartGame();
    }

    void Pause(bool p)
    {
        Time.timeScale = p ? 0f : 1f;
        State = p ? GameState.Paused : GameState.Playing;
    }

    // Called by the player when an obstacle is hit. Returns true if the player survives.
    public bool LoseLife()
    {
        if (Lives <= 0) return false;
        Lives--;
        camFollow.Shake(0.5f);
        PlaySfx(Sfx.Crash, 0.6f);
        if (player != null) Fx.Burst(player.transform.position + Vector3.up, new Color(1f, 0.35f, 0.25f), 20, 6f, 0.2f, 0.7f, 1f);
        return Lives > 0;
    }

    // Start a run in the chosen biome (-1 = all biomes cycling).
    void StartBiome(int biome)
    {
        menuChoice = biome;
        WorldGenerator.Selected = biome;
        Restart();
    }

    // Back to the biome selection screen.
    void GoHome()
    {
        Time.timeScale = 1f;
        WorldGenerator.Selected = menuChoice;
        world.Rebuild();
        player.ResetPlayer();
        camFollow.menuOrbit = true;
        camFollow.Snap();
        State = GameState.Menu;
        ApplyEnvironment();
    }

    public void AddCoin(Vector3 pos)
    {
        Coins++;
        if (Time.time - lastCoinTime > 1.2f) combo = 0;
        combo++;
        lastCoinTime = Time.time;
        sfxSrc.pitch = 1f + Mathf.Min(combo, 10) * 0.04f;
        sfxSrc.PlayOneShot(Sfx.Coin, 0.8f);
        Fx.Burst(pos, new Color(1f, 0.85f, 0.15f), 10, 3.5f, 0.15f, 0.45f, 0.5f);
    }

    public void PlayJump() { sfxSrc.pitch = 1f; sfxSrc.PlayOneShot(Sfx.Jump, 0.6f); }

    void PlaySfx(AudioClip c, float v) { sfxSrc.pitch = 1f; sfxSrc.PlayOneShot(c, v); }

    public void OnPlayerDied()
    {
        Lives = 0;
        State = GameState.GameOver;
        gameOverAt = Time.unscaledTime;
        newBest = Score > Best;
        if (newBest)
        {
            Best = Score;
            PlayerPrefs.SetInt("JR_Best", Best);
            PlayerPrefs.Save();
        }
        PlaySfx(Sfx.Crash, 1f);
        camFollow.Shake(0.8f);
        Fx.Burst(player.transform.position, new Color(1f, 0.5f, 0.2f), 36, 8f, 0.25f, 0.9f, 1f);
    }

    // ------------------------------------------------------------ per-frame
    void Update()
    {
        switch (State)
        {
            case GameState.Menu:
                if (Inp.ConfirmDown()) StartBiome(menuChoice);
                if (Inp.KeyDown(KeyCode.Alpha1)) StartBiome(0);
                if (Inp.KeyDown(KeyCode.Alpha2)) StartBiome(1);
                if (Inp.KeyDown(KeyCode.Alpha3)) StartBiome(2);
                if (Inp.KeyDown(KeyCode.Alpha4)) StartBiome(3);
                if (Inp.KeyDown(KeyCode.Alpha5)) StartBiome(4);
                if (Inp.KeyDown(KeyCode.Alpha6)) StartBiome(-1);
                break;
            case GameState.Playing:
                Distance = Mathf.Max(Distance, player.transform.position.z);
                int bi = WorldGenerator.BiomeIndexAt(player.transform.position.z);
                if (bi != lastBiome)
                {
                    lastBiome = bi;
                    toast = Biomes.Get(bi).name;
                    toastAt = Time.unscaledTime;
                }
                if (Inp.PauseDown()) Pause(true);
                if (Inp.KeyDown(KeyCode.H)) GoHome();
                break;
            case GameState.Paused:
                if (Inp.PauseDown()) Pause(false);
                if (Inp.KeyDown(KeyCode.H)) GoHome();
                break;
            case GameState.GameOver:
                if (Time.unscaledTime - gameOverAt > 0.8f && (Inp.RestartDown() || Inp.ConfirmDown())) Restart();
                if (Inp.KeyDown(KeyCode.H)) GoHome();
                break;
        }
        ApplyEnvironment();
    }

    void LateUpdate() { Inp.EndFrame(); }

    void ApplyEnvironment()
    {
        float pz = (player != null) ? player.transform.position.z : 0f;
        BiomeStyle a, b;
        float t;
        WorldGenerator.EnvAt(pz + 70f, out a, out b, out t);

        Color sky = Color.Lerp(a.sky, b.sky, t);
        cam.backgroundColor = sky;
        RenderSettings.fogColor = sky;
        RenderSettings.fogDensity = Mathf.Lerp(a.fogDensity, b.fogDensity, t);
        RenderSettings.ambientLight = Color.Lerp(a.ambient, b.ambient, t);
        if (sun != null)
        {
            sun.color = Color.Lerp(a.sun, b.sun, t);
            sun.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
            sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(a.sunAngle), Quaternion.Euler(b.sunAngle), t);
        }
        if (ambient != null)
        {
            BiomeStyle dom = (t > 0.5f) ? b : a;
            if (dom.type != ambientType)
            {
                ambientType = dom.type;
                StyleAmbient(dom);
            }
            ambient.transform.position = new Vector3(0f, 8f, pz + 25f);
        }
    }
    BiomeType ambientType = (BiomeType)(-1);

    // ------------------------------------------------------------ HUD
    void EnsureStyles()
    {
        if (lbl != null) return;
        lbl = new GUIStyle(GUI.skin.label);
        lbl.richText = true;
        lbl.fontStyle = FontStyle.Bold;
        lbl.wordWrap = false;
        btn = new GUIStyle(GUI.skin.button);
        btn.fontStyle = FontStyle.Bold;
        btn.fontSize = 28;
    }

    void Panel(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    void Text(Rect r, string s, int size, Color c, TextAnchor a)
    {
        lbl.fontSize = size;
        lbl.alignment = a;
        lbl.normal.textColor = new Color(0f, 0f, 0f, 0.65f * c.a);
        GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), s, lbl);
        lbl.normal.textColor = c;
        GUI.Label(r, s, lbl);
    }

    bool Btn(Rect r, string label, int size = 26)
    {
        btn.fontSize = size;
        return GUI.Button(r, label, btn);
    }

    void DrawLives(float x, float y)
    {
        for (int i = 0; i < MaxLives; i++)
        {
            bool full = i < Lives;
            Rect r = new Rect(x + i * 40f, y, 32f, 32f);
            Panel(r, full ? new Color(0.95f, 0.15f, 0.25f) : new Color(0.3f, 0.3f, 0.35f, 0.8f));
            if (full) Panel(new Rect(r.x + 6f, r.y + 5f, 9f, 8f), new Color(1f, 0.7f, 0.75f));
        }
    }

    void OnGUI()
    {
        Inp.Feed(Event.current);
        EnsureStyles();
        float s = Screen.height / 720f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1f));
        float W = Screen.width / s, H = 720f;

        BiomeStyle cur = Biomes.Get(Mathf.Max(0, WorldGenerator.BiomeIndexAt(player != null ? player.transform.position.z : 0f)));

        if (State != GameState.Menu)
        {
            // score + lives panel
            Panel(new Rect(16, 14, 330, 132), new Color(0f, 0f, 0f, 0.45f));
            Text(new Rect(28, 14, 310, 50), "SCORE  " + Score, 36, Color.white, TextAnchor.MiddleLeft);
            Panel(new Rect(32, 74, 16, 16), new Color(1f, 0.85f, 0.2f));
            Text(new Rect(56, 62, 280, 36), Coins + "     " + Mathf.FloorToInt(Distance) + " m", 24, Color.white, TextAnchor.MiddleLeft);
            DrawLives(28, 102);
            // biome + best
            Panel(new Rect(W - 276, 14, 260, 70), new Color(0f, 0f, 0f, 0.45f));
            Panel(new Rect(W - 276, 14, 8, 70), cur.hud);
            Text(new Rect(W - 262, 14, 240, 38), cur.name, 26, cur.hud, TextAnchor.MiddleLeft);
            Text(new Rect(W - 262, 48, 240, 32), "BEST  " + Best, 20, new Color(1f, 1f, 1f, 0.9f), TextAnchor.MiddleLeft);
            // pause + home buttons
            if (State == GameState.Playing)
            {
                if (Btn(new Rect(W - 276, 94, 126, 46), "PAUSE", 22)) Pause(true);
                if (Btn(new Rect(W - 142, 94, 126, 46), "HOME", 22)) GoHome();
            }
            // biome toast
            float ta = Time.unscaledTime - toastAt;
            if (State == GameState.Playing && ta < 2.8f && toast.Length > 0)
            {
                float al = Mathf.Clamp01(Mathf.Min(ta * 3f, (2.8f - ta) * 2f));
                Color tc = new Color(cur.hud.r, cur.hud.g, cur.hud.b, al);
                Text(new Rect(0, 170, W, 30), "ENTERING", 22, new Color(1f, 1f, 1f, al), TextAnchor.MiddleCenter);
                Text(new Rect(0, 196, W, 80), toast, 68, tc, TextAnchor.MiddleCenter);
            }
        }

        if (State == GameState.Menu)
        {
            Panel(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.4f));
            Text(new Rect(0, 24, W, 80), "JUNGLE ENDLESS RUNNER", 60, new Color(1f, 0.95f, 0.5f), TextAnchor.MiddleCenter);
            Text(new Rect(0, 100, W, 40), "SELECT YOUR BIOME  (click a card or press 1-6)", 26, Color.white, TextAnchor.MiddleCenter);

            float cw = 270f, ch = 130f, gap = 18f;
            float x0 = W * 0.5f - (cw * 3f + gap * 2f) * 0.5f;
            for (int i = 0; i < 6; i++)
            {
                int biome = (i < 5) ? i : -1;
                Rect r = new Rect(x0 + (i % 3) * (cw + gap), 160f + (i / 3) * (ch + gap), cw, ch);
                Color c = (biome >= 0) ? Biomes.Get(biome).sideBase : new Color(0.55f, 0.35f, 0.9f);
                Color dk = (biome >= 0) ? Biomes.Get(biome).trackBase : new Color(0.2f, 0.12f, 0.4f);
                bool hover = r.Contains(Event.current.mousePosition);
                Panel(new Rect(r.x - 4, r.y - 4, r.width + 8, r.height + 8), (biome == menuChoice) ? Color.white : new Color(0f, 0f, 0f, 0.6f));
                Panel(r, hover ? Color.Lerp(c, Color.white, 0.25f) : c);
                Panel(new Rect(r.x, r.y + r.height - 44f, r.width, 44f), new Color(dk.r, dk.g, dk.b, 0.9f));
                string nm = (biome >= 0) ? Biomes.Get(biome).name : "ALL BIOMES";
                Text(new Rect(r.x, r.y + 14f, r.width, 50f), (i + 1) + "  " + nm, 32, (biome >= 0) ? Color.Lerp(Color.white, dk, 0.1f) : Color.white, TextAnchor.MiddleCenter);
                Text(new Rect(r.x, r.y + r.height - 44f, r.width, 44f), (biome >= 0) ? "Endless run here" : "Mix, changes every 8 tiles", 20, Color.white, TextAnchor.MiddleCenter);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) StartBiome(biome);
            }

            Panel(new Rect(W / 2 - 400, 470, 800, 150), new Color(0f, 0f, 0f, 0.5f));
            Text(new Rect(W / 2 - 380, 478, 760, 140),
                "LEFT / RIGHT arrows: change lane     UP arrow or SPACE: jump\n" +
                "P or ESC: pause     H: home     3 lives per run\n" +
                "Collect coins, avoid rocks, logs and trunks!", 24, Color.white, TextAnchor.MiddleCenter);
            Text(new Rect(0, 640, W, 36), "BEST  " + Best, 26, new Color(1f, 0.9f, 0.4f), TextAnchor.MiddleCenter);
        }
        else if (State == GameState.Paused)
        {
            Panel(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.5f));
            Text(new Rect(0, 230, W, 90), "PAUSED", 72, Color.white, TextAnchor.MiddleCenter);
            if (Btn(new Rect(W / 2 - 150, 340, 300, 64), "RESUME  (P)")) Pause(false);
            if (Btn(new Rect(W / 2 - 150, 420, 300, 64), "HOME  (H)")) GoHome();
        }
        else if (State == GameState.GameOver && Time.unscaledTime - gameOverAt > 0.6f)
        {
            Panel(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.55f));
            Panel(new Rect(W / 2 - 280, 120, 560, 470), new Color(0.05f, 0.05f, 0.1f, 0.85f));
            Text(new Rect(0, 135, W, 90), "GAME OVER", 72, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter);
            Text(new Rect(0, 230, W, 60), "SCORE  " + Score, 46, Color.white, TextAnchor.MiddleCenter);
            Text(new Rect(0, 292, W, 40), "Coins " + Coins + "    Distance " + Mathf.FloorToInt(Distance) + " m", 26, new Color(1f, 0.9f, 0.5f), TextAnchor.MiddleCenter);
            Text(new Rect(0, 335, W, 40), newBest ? "NEW BEST!" : "Best " + Best, 28, newBest ? new Color(0.4f, 1f, 0.5f) : Color.white, TextAnchor.MiddleCenter);
            if (Btn(new Rect(W / 2 - 150, 410, 300, 64), "RESTART  (R)", 28)) Restart();
            if (Btn(new Rect(W / 2 - 150, 490, 300, 64), "HOME  (H)", 28)) GoHome();
        }
        GUI.matrix = Matrix4x4.identity;
    }
}

// Makes the game start even if a scene has no GameManager object yet.
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (GameManager.Instance == null)
        {
            GameObject g = new GameObject("GameManager");
            g.AddComponent<GameManager>();
        }
    }
}
