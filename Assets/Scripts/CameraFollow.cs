using UnityEngine;

// Attached to the Main Camera. Follows the player at a fixed distance (see lab Step 6).
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 5.2f, -8f);
    public float lookAhead = 6f;
    public bool menuOrbit = true;

    Camera cam;
    PlayerController pc;
    float shake;

    void Awake() { cam = GetComponent<Camera>(); }

    public void Shake(float amount) { shake = amount; }

    public void Snap() { Place(0f); }

    void LateUpdate() { Place(Time.unscaledDeltaTime); }

    void Place(float dt)
    {
        if (player == null) return;
        if (pc == null) pc = player.GetComponent<PlayerController>();
        Vector3 pp = player.position;
        float py = Mathf.Max(pp.y, 0f);
        Vector3 pos;
        Vector3 look;

        if (menuOrbit)
        {
            Vector3 o = Quaternion.Euler(0f, Mathf.Sin(Time.unscaledTime * 0.4f) * 30f, 0f) * new Vector3(0f, 0f, -9f);
            pos = new Vector3(pp.x + o.x, 4.2f, pp.z + o.z);
            look = new Vector3(pp.x, 1.3f, pp.z + 3f);
        }
        else
        {
            pos = new Vector3(pp.x * 0.7f + offset.x, offset.y + 1f + Mathf.Max(0f, py - 1f) * 0.4f, pp.z + offset.z);
            look = new Vector3(pp.x * 0.9f, 1.2f + Mathf.Max(0f, py - 1f) * 0.5f, pp.z + lookAhead);
        }

        if (shake > 0f)
        {
            pos += Random.insideUnitSphere * shake * 0.4f;
            shake = Mathf.MoveTowards(shake, 0f, dt * 1.5f);
        }

        transform.position = pos;
        transform.LookAt(look);

        if (cam != null)
        {
            float f = (pc != null) ? pc.SpeedNormalized : 0f;
            float target = 60f + f * 14f;
            cam.fieldOfView = (dt <= 0f) ? target : Mathf.Lerp(cam.fieldOfView, target, dt * 2f);
        }
    }
}
