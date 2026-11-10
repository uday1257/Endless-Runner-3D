using System.Collections.Generic;
using UnityEngine;

// Input wrapper: works with the legacy Input Manager (as in the lab PDF) and
// falls back to IMGUI key events if only the new Input System is active.
public static class Inp
{
#if ENABLE_LEGACY_INPUT_MANAGER
    public static float Horizontal()
    {
        float x = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        return x;
    }
    public static bool Up() { return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow); }
    public static bool Down() { return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow); }
    public static bool JumpDown() { return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W); }
    public static bool LeftDown() { return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A); }
    public static bool RightDown() { return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D); }
    public static bool ConfirmDown()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }
    public static bool RestartDown()
    {
        return Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }
    public static bool PauseDown() { return Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape); }
    public static bool KeyDown(KeyCode k) { return Input.GetKeyDown(k); }
    public static void Feed(Event e) { }
    public static void EndFrame() { }
#else
    static readonly HashSet<KeyCode> held = new HashSet<KeyCode>();
    static readonly HashSet<KeyCode> pressed = new HashSet<KeyCode>();

    public static void Feed(Event e)
    {
        if (e == null || !e.isKey || e.keyCode == KeyCode.None) return;
        if (e.type == EventType.KeyDown)
        {
            if (!held.Contains(e.keyCode)) pressed.Add(e.keyCode);
            held.Add(e.keyCode);
        }
        else if (e.type == EventType.KeyUp)
        {
            held.Remove(e.keyCode);
        }
    }
    public static void EndFrame() { pressed.Clear(); }

    static bool Held(KeyCode k) { return held.Contains(k); }
    static bool Down1(KeyCode k) { return pressed.Contains(k); }

    public static float Horizontal()
    {
        float x = 0f;
        if (Held(KeyCode.A) || Held(KeyCode.LeftArrow)) x -= 1f;
        if (Held(KeyCode.D) || Held(KeyCode.RightArrow)) x += 1f;
        return x;
    }
    public static bool Up() { return Held(KeyCode.W) || Held(KeyCode.UpArrow); }
    public static bool Down() { return Held(KeyCode.S) || Held(KeyCode.DownArrow); }
    public static bool JumpDown() { return Down1(KeyCode.Space) || Down1(KeyCode.UpArrow) || Down1(KeyCode.W); }
    public static bool LeftDown() { return Down1(KeyCode.LeftArrow) || Down1(KeyCode.A); }
    public static bool RightDown() { return Down1(KeyCode.RightArrow) || Down1(KeyCode.D); }
    public static bool ConfirmDown() { return Down1(KeyCode.Space) || Down1(KeyCode.Return) || Down1(KeyCode.KeypadEnter); }
    public static bool RestartDown() { return Down1(KeyCode.R) || Down1(KeyCode.Return) || Down1(KeyCode.KeypadEnter); }
    public static bool PauseDown() { return Down1(KeyCode.P) || Down1(KeyCode.Escape); }
    public static bool KeyDown(KeyCode k) { return Down1(k); }
#endif
}
