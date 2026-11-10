using UnityEngine;

// Auto-runs forward; left/right arrows switch lane (one press = one lane), Up or Space jumps (lab Step 4).
// Builds its own Rigidbody, CapsuleCollider and a procedural explorer model if missing.
public class PlayerController : MonoBehaviour
{
    public float speed = 11f;          // starting forward speed
    public float maxSpeed = 24f;
    public float acceleration = 0.18f; // speed gained per second
    public float sideSpeed = 8f;
    public float jumpForce = 11f;
    public float trackLimit = 2.5f;

    Rigidbody rb;
    CapsuleCollider col;
    bool grounded, running, dead;
    float boost = 1f, vx, currentSpeed;
    float jumpPressTime = -10f, lastGroundTime = -10f;
    int beginFrame = -1;
    float invulUntil = -10f;
    int lane = 1;                      // 0 = left, 1 = middle, 2 = right
    public float laneSpeed = 26f;      // units/sec while snapping to the next lane

    Transform model, armL, armR, legL, legR;

    public bool IsDead { get { return dead; } }
    public bool IsRunning { get { return running; } }
    public bool IsInvulnerable { get { return Time.time < invulUntil; } }
    public float SpeedNormalized { get { return running ? Mathf.Clamp01((currentSpeed - speed) / Mathf.Max(1f, maxSpeed - speed)) : 0f; } }

    Vector3 Vel
    {
        get
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }
        set
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = value;
#else
            rb.velocity = value;
#endif
        }
    }

    void Awake()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false; // hide the plain capsule, we draw a character instead

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        col = GetComponent<CapsuleCollider>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider>();
        col.radius = 0.4f;
        col.height = 2f;
        col.center = Vector3.zero;

        BuildModel();
        currentSpeed = speed;
    }

    public void ResetPlayer()
    {
        dead = false;
        running = false;
        grounded = true;
        invulUntil = -10f;
        lane = 1;
        if (model != null) model.gameObject.SetActive(true);
        vx = 0f;
        currentSpeed = speed;
        jumpPressTime = -10f;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        Vel = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = new Vector3(0f, 1.05f, 0f);
        rb.rotation = Quaternion.identity;
        transform.position = new Vector3(0f, 1.05f, 0f);
        transform.rotation = Quaternion.identity;
        if (model != null) { model.localPosition = Vector3.zero; model.localRotation = Quaternion.identity; }
        rb.isKinematic = true; // parked until Begin()
    }

    public void Begin()
    {
        rb.isKinematic = false;
        running = true;
        dead = false;
        beginFrame = Time.frameCount;
        jumpPressTime = -10f;
    }

    void Update()
    {
        Animate();
        if (!running || dead) return;

        // One key press = exactly one lane change. Holding the key does nothing extra.
        if (Inp.LeftDown()) lane = Mathf.Max(0, lane - 1);
        if (Inp.RightDown()) lane = Mathf.Min(2, lane + 1);
        boost = 1f;
        if (Inp.JumpDown() && Time.frameCount != beginFrame) jumpPressTime = Time.time;
        currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (!running || dead) return;

        grounded = Physics.Raycast(rb.position, Vector3.down, 1.15f, ~0, QueryTriggerInteraction.Ignore);
        if (grounded) lastGroundTime = Time.time;

        // Slide straight to the centre of the chosen lane (lands exactly, no drifting).
        float dx = WorldGenerator.LaneX[lane] - rb.position.x;
        vx = Mathf.Clamp(dx / Time.fixedDeltaTime, -laneSpeed, laneSpeed);

        Vector3 v = Vel;
        float vy = v.y;
        if (Time.time - jumpPressTime < 0.12f && Time.time - lastGroundTime < 0.1f)
        {
            vy = jumpForce;
            jumpPressTime = -10f;
            lastGroundTime = -10f;
            grounded = false;
            if (GameManager.Instance != null) GameManager.Instance.PlayJump();
        }
        Vel = new Vector3(vx, vy, currentSpeed * boost);

        Vector3 p = rb.position;
        if (Mathf.Abs(p.x) > trackLimit + 0.3f)
        {
            p.x = Mathf.Clamp(p.x, -trackLimit - 0.3f, trackLimit + 0.3f);
            rb.position = p;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (dead || !running) return;
        if (collision.gameObject.GetComponent<Obstacle>() == null) return;
        if (Time.time < invulUntil) return;

        GameManager gm = GameManager.Instance;
        if (gm != null && gm.LoseLife())
        {
            // still alive: brief invulnerability, smash through the obstacle
            invulUntil = Time.time + 2f;
            currentSpeed = Mathf.Max(speed, currentSpeed * 0.8f);
            Collider oc = collision.collider;
            if (oc != null) oc.enabled = false;
            Vel = new Vector3(0f, Mathf.Max(Vel.y, 4f), currentSpeed);
            return;
        }
        Die();
    }

    void Die()
    {
        dead = true;
        running = false;
        Debug.Log("GAME OVER");
        rb.constraints = RigidbodyConstraints.None;
        Vel = new Vector3(Random.Range(-2f, 2f), 6f, -7f);
        rb.angularVelocity = new Vector3(Random.Range(-8f, 8f), Random.Range(-4f, 4f), Random.Range(-8f, 8f));
        if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
    }

    // ---------------- procedural character ----------------
    void BuildModel()
    {
        GameObject m = new GameObject("Model");
        m.transform.SetParent(transform, false);
        model = m.transform;

        Material skin = Gfx.Mat(new Color(0.96f, 0.76f, 0.60f), 0.2f);
        Material shirt = Gfx.Mat(new Color(0.95f, 0.25f, 0.20f), 0.2f);
        Material shorts = Gfx.Mat(new Color(0.82f, 0.66f, 0.35f), 0.1f);
        Material boot = Gfx.Mat(new Color(0.30f, 0.18f, 0.10f), 0.2f);
        Material hat = Gfx.Mat(new Color(0.20f, 0.60f, 0.28f), 0.1f);
        Material band = Gfx.Mat(new Color(0.95f, 0.80f, 0.15f), 0.1f);
        Material pack = Gfx.Mat(new Color(0.15f, 0.62f, 0.72f), 0.15f);
        Material black = Gfx.Mat(new Color(0.05f, 0.05f, 0.05f), 0.3f);

        // torso, backpack, head, hat
        Gfx.Box(model, new Vector3(0f, 0.33f, 0f), new Vector3(0.52f, 0.66f, 0.30f), shirt);
        Gfx.Box(model, new Vector3(0f, 0.35f, -0.24f), new Vector3(0.40f, 0.50f, 0.20f), pack);
        Gfx.Ball(model, new Vector3(0f, 0.95f, 0f), new Vector3(0.50f, 0.50f, 0.50f), skin);
        Gfx.Ball(model, new Vector3(-0.10f, 0.98f, 0.22f), new Vector3(0.08f, 0.08f, 0.08f), black, false);
        Gfx.Ball(model, new Vector3(0.10f, 0.98f, 0.22f), new Vector3(0.08f, 0.08f, 0.08f), black, false);
        Gfx.Cyl(model, new Vector3(0f, 1.12f, 0f), 0.34f, 0.07f, hat);
        Gfx.Cyl(model, new Vector3(0f, 1.17f, 0f), 0.22f, 0.17f, hat);
        Gfx.Cyl(model, new Vector3(0f, 1.18f, 0f), 0.225f, 0.04f, band);

        legL = MakeLeg(-0.15f, shorts, skin, boot);
        legR = MakeLeg(0.15f, shorts, skin, boot);
        armL = MakeArm(-0.34f, shirt, skin);
        armR = MakeArm(0.34f, shirt, skin);
    }

    Transform MakeLeg(float x, Material shorts, Material skin, Material boot)
    {
        GameObject p = new GameObject("Leg");
        p.transform.SetParent(model, false);
        p.transform.localPosition = new Vector3(x, 0f, 0f);
        Gfx.Cyl(p.transform, new Vector3(0f, -0.45f, 0f), 0.14f, 0.45f, shorts);
        Gfx.Cyl(p.transform, new Vector3(0f, -0.85f, 0f), 0.11f, 0.40f, skin);
        Gfx.Box(p.transform, new Vector3(0f, -0.93f, 0.05f), new Vector3(0.26f, 0.14f, 0.38f), boot);
        return p.transform;
    }

    Transform MakeArm(float x, Material shirt, Material skin)
    {
        GameObject p = new GameObject("Arm");
        p.transform.SetParent(model, false);
        p.transform.localPosition = new Vector3(x, 0.58f, 0f);
        Gfx.Cyl(p.transform, new Vector3(0f, -0.25f, 0f), 0.10f, 0.25f, shirt);
        Gfx.Cyl(p.transform, new Vector3(0f, -0.50f, 0f), 0.08f, 0.25f, skin);
        Gfx.Ball(p.transform, new Vector3(0f, -0.55f, 0f), new Vector3(0.18f, 0.18f, 0.18f), skin);
        return p.transform;
    }

    void Animate()
    {
        if (model == null || dead) return;
        bool blinkOn = !(Time.time < invulUntil) || ((int)(Time.time * 12f) % 2 == 0);
        if (model.gameObject.activeSelf != blinkOn) model.gameObject.SetActive(blinkOn);
        if (!running)
        {
            float b = Mathf.Sin(Time.unscaledTime * 2f) * 0.03f;
            model.localPosition = new Vector3(0f, b, 0f);
            legL.localRotation = Quaternion.identity; legR.localRotation = Quaternion.identity;
            armL.localRotation = Quaternion.identity; armR.localRotation = Quaternion.identity;
            return;
        }
        if (!grounded)
        {
            legL.localRotation = Quaternion.Euler(-35f, 0f, 0f);
            legR.localRotation = Quaternion.Euler(30f, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-150f, 0f, -10f);
            armR.localRotation = Quaternion.Euler(-150f, 0f, 10f);
            model.localPosition = Vector3.zero;
        }
        else
        {
            float t = Time.time * (6f + currentSpeed * 0.35f);
            float sw = Mathf.Sin(t) * 55f;
            legL.localRotation = Quaternion.Euler(sw, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-sw, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-sw * 0.9f, 0f, 0f);
            armR.localRotation = Quaternion.Euler(sw * 0.9f, 0f, 0f);
            model.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Cos(t)) * 0.08f, 0f);
        }
        model.localRotation = Quaternion.Euler(8f, vx * 1.5f, -vx * 2.5f);
    }
}
