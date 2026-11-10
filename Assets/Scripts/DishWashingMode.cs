using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Screen transition only: no scrubbing, scoring or dish completion yet.
/// Call Open(interactor) from a washing station. Close returns to gameplay.
/// Created on demand so existing scenes need no manual wiring.
/// </summary>
public sealed class DishWashingMode : MonoBehaviour
{
    private static DishWashingMode instance;
    public static bool IsOpen => instance != null && instance.open;

    private bool open;
    private Player movement;
    private PlayerInteractor interactor;
    private PlayerInput input;
    private bool movementWasEnabled, interactorWasEnabled, inputWasEnabled;
    private CursorLockMode previousLock;
    private bool previousCursorVisible;
    private Texture2D plate;
    private GUIStyle titleStyle, bodyStyle, buttonStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() { instance = null; }

    public static void Open(PlayerInteractor player)
    {
        if (player == null || IsOpen) return;
        if (instance == null)
            instance = new GameObject("DishWashingMode").AddComponent<DishWashingMode>();
        instance.Begin(player);
    }

    private void Begin(PlayerInteractor player)
    {
        interactor = player;
        movement = player.GetComponent<Player>();
        input = player.GetComponent<PlayerInput>();
        movementWasEnabled = movement != null && movement.enabled;
        interactorWasEnabled = player.enabled;
        inputWasEnabled = input != null && input.enabled;
        if (input != null) input.enabled = false;
        if (movement != null) movement.enabled = false;
        player.enabled = false;
        var body = player.GetComponent<Rigidbody2D>();
        if (body != null) body.linearVelocity = Vector2.zero;
        var animator = player.GetComponentInChildren<Animator>();
        if (animator != null) animator.SetBool("isWalking", false);
        previousLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        open = true;
        // Time.timeScale is intentionally untouched: the shift timer continues.
    }

    public void Close()
    {
        if (!open) return;
        open = false;
        if (movement != null) movement.enabled = movementWasEnabled;
        if (interactor != null) interactor.enabled = interactorWasEnabled;
        if (input != null) input.enabled = inputWasEnabled;
        Cursor.lockState = previousLock;
        Cursor.visible = previousCursorVisible;
    }

    private void Update()
    {
        if (open && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void OnDisable() { Close(); }
    private void OnDestroy()
    {
        Close();
        if (plate != null) Destroy(plate);
        if (instance == this) instance = null;
    }

    private void PrepareGraphics()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 34, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = new Color32(52, 64, 59, 255);
        bodyStyle = new GUIStyle(titleStyle) { fontSize = 19, fontStyle = FontStyle.Normal, wordWrap = true };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
        plate = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        plate.filterMode = FilterMode.Point;
        for (int y = 0; y < 256; y++)
        for (int x = 0; x < 256; x++)
        {
            float radius = Vector2.Distance(new Vector2(x, y), new Vector2(127.5f, 127.5f));
            Color c = Color.clear;
            if (radius < 122) c = new Color32(74, 117, 136, 255);
            if (radius < 116) c = new Color32(225, 241, 235, 255);
            if (radius < 94) c = new Color32(160, 199, 202, 255);
            if (radius < 88) c = new Color32(245, 244, 218, 255);
            if (radius < 85 && (Vector2.Distance(new Vector2(x,y), new Vector2(100,105)) < 25 ||
                Vector2.Distance(new Vector2(x,y), new Vector2(161,145)) < 31 ||
                Vector2.Distance(new Vector2(x,y), new Vector2(104,166)) < 14))
                c = new Color32(142, 103, 64, 255);
            plate.SetPixel(x,y,c);
        }
        plate.Apply();
    }

    private static void Fill(Rect rect, Color color)
    {
        Color before = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = before;
    }

    private void OnGUI()
    {
        if (!open) return;
        PrepareGraphics();
        GUI.depth = -1000;
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
        Fill(new Rect(0,0,1280,720), new Color32(38,63,68,255));
        Fill(new Rect(180,40,920,640), new Color32(246,233,202,255));
        GUI.Label(new Rect(220,65,840,60), "DISH WASHING", titleStyle);
        GUI.Label(new Rect(240,130,800,50), "Washing mode is open. Your shift timer keeps running.", bodyStyle);
        Fill(new Rect(330,195,620,335), new Color32(177,215,217,255));
        GUI.DrawTexture(new Rect(500,225,280,280), plate, ScaleMode.ScaleToFit, true);
        GUI.Label(new Rect(240,540,800,45), "Dirty dish loaded. Scrubbing will be added in a later task.", bodyStyle);
        if (GUI.Button(new Rect(455,600,370,50), "BACK TO RESTAURANT  [Esc]", buttonStyle)) Close();
        // Consume mouse events to prevent clicks reaching gameplay underneath.
        if (Event.current.isMouse && Event.current.type != EventType.Used) Event.current.Use();
        GUI.matrix = previous;
    }
}
