using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// GameManager: Master state machine for "The Hidden Signal"
/// Spawns all objects procedurally, manages protanopia filter,
/// tracks puzzle state, and triggers the final color reveal.
/// 
/// SETUP: Attach to an empty GameObject in SampleScene.
/// Assign ovrCameraRig (your [BuildingBlock] Camera Rig transform).
/// Everything else is spawned at runtime.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Reference")]
    [Tooltip("Drag your [BuildingBlock] Camera Rig here")]
    public Transform ovrCameraRig;

    [Header("Prefabs")]
    [Tooltip("Drag your GrabbableCube prefab here")]
    public GameObject grabbableCubePrefab;

    [Header("Spawn Settings")]
    public float spawnRadius = 0.5f;
    public float spawnHeight = 1.2f;
    public float pedestalDistance = 0.5f;

    // ── Internal state ──────────────────────────────────────────────
    private ProtanopiaFilterController _filter;
    private List<ColorCube> _cubes = new();
    private List<Pedestal>  _pedestals = new();
    private TMPro.TextMeshProUGUI _hintLabel;
    private Transform        _hintCanvasTransform;
    private TMPro.TextMeshProUGUI _revealLabel;
    private bool            _gameComplete;

    [Header("Pedestal Unlock Counts")]
    public int redPedestalCount   = 3;
    public int greenPedestalCount = 2;
    public int bluePedestalCount  = 1;

    // The final output code revealed when all cubes land correctly
    private string FinalCode => $"R{redPedestalCount}-G{greenPedestalCount}-B{bluePedestalCount}";

    // ── Colors ───────────────────────────────────────────────────────
    // True colors (visible after reveal)
    // True colors — shown before Let's Go (no LUT yet)
    public static readonly Color TrueRed   = new Color(0.8f,  0.15f, 0.1f);
    public static readonly Color TrueGreen = new Color(0.1f,  0.65f, 0.15f);
    public static readonly Color TrueBlue  = new Color(0.15f, 0.4f,  0.85f);

    // Protanopia colors — similar but slightly distinguishable under the filter
    public static readonly Color ProtoRed   = new Color(0.58f, 0.50f, 0.08f);
    public static readonly Color ProtoGreen = new Color(0.50f, 0.56f, 0.18f);
    public static readonly Color ProtoBlue  = new Color(0.15f, 0.40f, 0.85f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (ovrCameraRig == null)
            ovrCameraRig = Camera.main?.transform.parent ?? Camera.main?.transform;

        // Check for tag detector — if present, wait for tag
        // Otherwise show start screen immediately (editor fallback)
        var tagDetector = FindObjectOfType<AprilTagDetector>();
        if (tagDetector == null)
        {
            Debug.Log("[GameManager] No tag detector — showing start screen directly.");
            Transform camTransform = Camera.main?.transform ?? ovrCameraRig;
            var startGo = new GameObject("StartScreenUI");
            var startScreen = startGo.AddComponent<StartScreenUI>();
            startScreen.Initialize(camTransform);
        }
        else
        {
            Debug.Log("[GameManager] Waiting for marker detection...");
        }
    }

    /// <summary>
    /// Called by StartScreenUI when the player presses "Let's Go".
    /// </summary>
    public void BeginGame()
    {
        _filter = gameObject.AddComponent<ProtanopiaFilterController>();
        _filter.Initialize();
        StartCoroutine(BeginSequence());
    }

    IEnumerator BeginSequence()
    {
        // Apply filter immediately
        _filter.ApplyImmediate();
        yield return null;
        StartCoroutine(InitSequence());
    }

    // Called after cubes spawn to apply protanopia colors
    public void ApplyProtanopiaColors()
    {
        foreach (var c in _cubes)
            c.ApplyProtanopiaColor();
        foreach (var p in _pedestals)
            p.ApplyProtanopiaColor();
    }

    IEnumerator InitSequence()
    {
        yield return new WaitForSeconds(0.5f);

        ShowHint("Grab the cubes and place them on\nthe matching pedestals.\nCan you tell them apart?");
        SpawnPedestals();
        SpawnCubes();
        ApplyProtanopiaColors();

        yield return new WaitForSeconds(3.5f);
        HideHint();
    }

    // ── Spawning ────────────────────────────────────────────────────

    void SpawnPedestals()
    {
        // Get camera XZ position and forward direction
        Transform cam = Camera.main?.transform;
        Vector3 camPos = cam != null ? cam.position : Vector3.zero;
        Vector3 fwd = cam != null ? cam.forward : Vector3.forward;
        fwd.y = 0f; fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        CubeColor[] types = { CubeColor.Red, CubeColor.Green, CubeColor.Blue };
        float spacing = 0.25f;
        float dist = 0.5f;
        float height = 0.7f;

        for (int i = 0; i < 3; i++)
        {
            float offset = (i - 1) * spacing; // -0.25, 0, +0.25
            Vector3 pos = new Vector3(camPos.x, 0f, camPos.z)
                        + fwd * dist
                        + right * offset
                        + Vector3.up * height;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"Pedestal_{types[i]}";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.18f, 0.04f, 0.18f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = GetTrueColor(types[i]) * 0.7f;
            go.GetComponent<Renderer>().material = mat;

            var pedestal = go.AddComponent<Pedestal>();
            int required = types[i] switch {
                CubeColor.Red   => redPedestalCount,
                CubeColor.Green => greenPedestalCount,
                CubeColor.Blue  => bluePedestalCount,
                _ => 1
            };
            pedestal.Initialize(types[i], mat, required);
            _pedestals.Add(pedestal);
        }
    }

    void SpawnCubes()
    {
        Transform cam = Camera.main?.transform;
        Vector3 camPos = cam != null ? cam.position : Vector3.zero;
        Vector3 fwd = cam != null ? cam.forward : Vector3.forward;
        fwd.y = 0f; fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        // Build cube list based on pedestal counts, then shuffle
        var cubeList = new System.Collections.Generic.List<CubeColor>();
        for (int i = 0; i < redPedestalCount;   i++) cubeList.Add(CubeColor.Red);
        for (int i = 0; i < greenPedestalCount; i++) cubeList.Add(CubeColor.Green);
        for (int i = 0; i < bluePedestalCount;  i++) cubeList.Add(CubeColor.Blue);

        // Fisher-Yates shuffle
        for (int i = cubeList.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = cubeList[i]; cubeList[i] = cubeList[j]; cubeList[j] = tmp;
        }

        int total = cubeList.Count;
        float spacing = 0.22f;
        float dist    = 0.5f;
        float height  = 0.95f;

        for (int i = 0; i < total; i++)
        {
            // Evenly distribute around a circle
            float angle = (i / (float)total) * 2f * Mathf.PI;
            Vector3 orbitCenter = new Vector3(camPos.x, 0f, camPos.z) + fwd * dist;
            float radius = spacing * total * 0.18f;

            Vector3 pos = new Vector3(
                orbitCenter.x + Mathf.Sin(angle) * radius,
                0f,
                orbitCenter.z + Mathf.Cos(angle) * radius
            ) + Vector3.up * 0.95f;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = GetTrueColor(cubeList[i]);

            GameObject go;
            if (grabbableCubePrefab != null)
            {
                go = Instantiate(grabbableCubePrefab, pos, Quaternion.identity);
                go.name = $"Cube_{cubeList[i]}_{i}";
                go.transform.localScale = Vector3.one * 0.1f;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Cube_{cubeList[i]}_{i}";
                go.transform.position = pos;
                go.transform.localScale = Vector3.one * 0.1f;
                var rb = go.AddComponent<Rigidbody>();
                rb.mass = 0.3f; rb.useGravity = false; rb.isKinematic = true;
            }

            var mainRend = go.GetComponent<Renderer>();
            if (mainRend != null) mainRend.material = mat;

            var cube = go.AddComponent<ColorCube>();
            cube.Initialize(cubeList[i], mat);
            _cubes.Add(cube);

            // Add rotation and revolution
            var motion = go.AddComponent<CubeMotion>();
            Vector3 orbitCenterPos = new Vector3(camPos.x, 0f, camPos.z) + fwd * dist;
            motion.orbitCenter = new Vector3(orbitCenterPos.x, 0.95f, orbitCenterPos.z);
            motion.orbitRadius = spacing * total * 0.18f;
            motion.spinSpeed   = UnityEngine.Random.Range(45f, 90f);
            motion.orbitSpeed  = UnityEngine.Random.Range(15f, 25f);
        }
    }

    void AddShapeMarker(Transform parent, CubeColor type)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "ShapeMarker";
        quad.transform.SetParent(parent);
        quad.transform.localPosition = new Vector3(0, 0, -0.52f);
        quad.transform.localScale = Vector3.one * 0.7f;
        Destroy(quad.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.black;
        quad.GetComponent<Renderer>().material = mat;

        // TMP label on quad
        var labelGo = new GameObject("MarkerLabel");
        labelGo.transform.SetParent(quad.transform);
        labelGo.transform.localPosition = new Vector3(0, 0, -0.01f);
        labelGo.transform.localScale = Vector3.one * 0.08f;

        var tmp = labelGo.AddComponent<TextMeshPro>();
        tmp.text = type switch {
            CubeColor.Red   => "▲",
            CubeColor.Green => "■",
            CubeColor.Blue  => "●",
            _ => "?"
        };
        tmp.fontSize = 8;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    void TryAddGrabbable(GameObject go)
    {
        bool added = false;

        // Try all possible grabbable types in order
        string[] types = new string[]
        {
            "OVRGrabbable, Assembly-CSharp",
            "Oculus.Interaction.Grabbable, Oculus.Interaction",
            "Oculus.Interaction.HandGrab.HandGrabInteractable, Oculus.Interaction",
            "OVRGrabbable, Oculus.VR",
        };

        foreach (var typeName in types)
        {
            var t = System.Type.GetType(typeName);
            if (t != null)
            {
                try
                {
                    go.AddComponent(t);
                    Debug.Log($"[GameManager] Added {typeName} to {go.name}");
                    added = true;

                    // Add companion components needed by Oculus.Interaction.Grabbable
                    if (typeName.Contains("Oculus.Interaction.Grabbable"))
                    {
                        var rb = go.GetComponent<Rigidbody>();

                        // GrabInteractable — needed for controller grab
                        var grabType = System.Type.GetType("Oculus.Interaction.GrabInteractable, Oculus.Interaction");
                        if (grabType != null)
                        {
                            try
                            {
                                var grabComp = go.AddComponent(grabType);
                                // Assign rigidbody via reflection
                                var rbField = grabType.GetField("_rigidbody",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (rbField != null) rbField.SetValue(grabComp, rb);
                                var rbProp = grabType.GetProperty("Rigidbody");
                                if (rbProp != null) rbProp.SetValue(grabComp, rb);
                            }
                            catch (System.Exception e) { Debug.LogWarning($"GrabInteractable: {e.Message}"); }
                        }

                        // HandGrabInteractable — needed for hand grab/pinch
                        var handGrabType = System.Type.GetType("Oculus.Interaction.HandGrab.HandGrabInteractable, Oculus.Interaction");
                        if (handGrabType != null)
                        {
                            try
                            {
                                var handComp = go.AddComponent(handGrabType);
                                var rbField = handGrabType.GetField("_rigidbody",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (rbField != null) rbField.SetValue(handComp, rb);
                                var rbProp = handGrabType.GetProperty("Rigidbody");
                                if (rbProp != null) rbProp.SetValue(handComp, rb);
                            }
                            catch (System.Exception e) { Debug.LogWarning($"HandGrabInteractable: {e.Message}"); }
                        }
                    }
                    break;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[GameManager] Failed to add {typeName}: {e.Message}");
                }
            }
        }

        if (!added)
        {
            // Enable physics so cube can be knocked onto pedestal
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            Debug.LogWarning($"[GameManager] No grab component found for {go.name} - using physics");

            #if UNITY_EDITOR
            go.AddComponent<SimpleDragInteractable>();
            #endif
        }
    }

    // ── Puzzle logic ─────────────────────────────────────────────────

    public void OnCubePlaced(CubeColor cubeColor, CubeColor pedestalColor, bool correct)
    {
        if (_gameComplete) return;

        if (!correct)
        {
            ShowHint("Wrong pedestal! Try again.");
            return;
        }

        // Check if all pedestals are fully unlocked
        bool allUnlocked = true;
        foreach (var p in _pedestals)
            if (!p.IsSolved) { allUnlocked = false; break; }

        if (allUnlocked)
            StartCoroutine(TriggerFinalReveal());
    }

    IEnumerator TriggerFinalReveal()
    {
        _gameComplete = true;
        HideHint();

        ShowHint("All placed! Something is shifting...");
        yield return new WaitForSeconds(1.5f);
        HideHint();

        // Reveal pedestal colors (cubes are already absorbed/destroyed)
        foreach (var p in _pedestals)
            if (p != null) p.RevealTrueColor();

        // Fade filter
        if (_filter != null)
            yield return _filter.FadeOut(1.8f);
        else
            yield return new WaitForSeconds(1.8f);

        yield return new WaitForSeconds(0.5f);

        ShowFinalCode();
    }

    void ShowFinalCode()
    {
        var canvasGo = new GameObject("RevealCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // EventSystem if not present
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 600f);
        canvasGo.transform.localScale = Vector3.one * 0.001f;

        Transform cam = Camera.main?.transform;
        if (cam != null)
        {
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            if (fwd.magnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            canvasGo.transform.position = cam.position + fwd * 1.0f + Vector3.up * 0.1f;
            canvasGo.transform.rotation = Quaternion.LookRotation(fwd);
        }

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(canvasGo.transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Title
        CreateCanvasText(canvasGo.transform, "SIGNAL DECODED", new Vector2(0, 200), 55,
            new Color(1f, 0.85f, 0.2f), TMPro.FontStyles.Bold);

        // Code
        CreateCanvasText(canvasGo.transform, FinalCode, new Vector2(0, 110), 75,
            new Color(1f, 0.85f, 0.2f), TMPro.FontStyles.Bold);

        // Message
        CreateCanvasText(canvasGo.transform,
            "Now you know what protanopia feels like.",
            new Vector2(0, 20), 26, Color.white);

        // Play Again button
        CreateEndButton(canvasGo.transform, "PLAY AGAIN",
            new Vector2(-195f, -120f), new Color(0.2f, 0.6f, 0.3f), () =>
        {
            Destroy(canvasGo);
            RestartGame();
        });

        // Full Colorblind Experience button
        CreateEndButton(canvasGo.transform, "COLORBLIND\nEXPERIENCE",
            new Vector2(195f, -120f), new Color(0.2f, 0.3f, 0.7f), () =>
        {
            Destroy(canvasGo);
            StartColorblindExperience();
        });

        StartCoroutine(ScaleIn(canvasGo.transform));
    }

    void CreateEndButton(Transform parent, string label, Vector2 pos, Color color, System.Action onClick)
    {
        var btnGo = new GameObject("Button");
        btnGo.transform.SetParent(parent, false);

        var img = btnGo.AddComponent<UnityEngine.UI.Image>();
        img.color = color;

        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchoredPosition = pos;
        btnRt.sizeDelta = new Vector2(330f, 110f);

        var btn = btnGo.AddComponent<UnityEngine.UI.Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());

        // Collider for poke
        var col = btnGo.AddComponent<BoxCollider>();
        col.size = new Vector3(330f, 110f, 80f);

        // Add EndButton for OVR pointer/poke support
        var endBtn = btnGo.AddComponent<EndButton>();
        endBtn.Initialize(img, onClick);

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        var tmp = labelGo.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28f;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
    }

    void RestartGame()
    {
        // Clean up current game objects
        foreach (var c in _cubes)     if (c != null) Destroy(c.gameObject);
        foreach (var p in _pedestals) if (p != null) Destroy(p.gameObject);
        _cubes.Clear();
        _pedestals.Clear();
        _gameComplete = false;

        // Restart filter and game
        if (_filter != null) Destroy(_filter);
        _filter = gameObject.AddComponent<ProtanopiaFilterController>();
        _filter.Initialize();
        StartCoroutine(BeginSequence());
    }

    void StartColorblindExperience()
    {
        // Clean up game objects
        foreach (var c in _cubes)     if (c != null) Destroy(c.gameObject);
        foreach (var p in _pedestals) if (p != null) Destroy(p.gameObject);
        _cubes.Clear();
        _pedestals.Clear();
        _gameComplete = false;

        // Disable main game filter before launching colorblind experience
        if (_filter != null)
        {
            Destroy(_filter);
            _filter = null;
        }
        // Also disable the passthrough colormap cleanly
        var pt = FindObjectOfType<OVRPassthroughLayer>();
        if (pt != null) { try { pt.DisableColorMap(); } catch {} }

        // Launch colorblind experience
        var cbExp = gameObject.AddComponent<ColorblindExperience>();
        cbExp.Initialize(ovrCameraRig);
    }

    void CreateCanvasText(Transform parent, string text, Vector2 pos, float fontSize,
        Color color, TMPro.FontStyles style = TMPro.FontStyles.Normal)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(750, 150);
    }

    IEnumerator ScaleIn(Transform t)
    {
        Vector3 targetScale = t.localScale == Vector3.zero ? Vector3.one * 0.001f : t.localScale;
        t.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, elapsed / 0.6f);
            t.localScale = targetScale * s;
            yield return null;
        }
        t.localScale = targetScale;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    public Color GetProtanopiaColor(CubeColor type) => type switch {
        CubeColor.Red   => ProtoRed,
        CubeColor.Green => ProtoGreen,
        CubeColor.Blue  => ProtoBlue,
        _ => Color.white
    };

    public Color GetTrueColor(CubeColor type) => type switch {
        CubeColor.Red   => TrueRed,
        CubeColor.Green => TrueGreen,
        CubeColor.Blue  => TrueBlue,
        _ => Color.white
    };

    void ShowHint(string msg)
    {
        if (_hintLabel == null)
        {
            // Use World Space Canvas — same as start/end panel
            var canvasGo = new GameObject("HintCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            var rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800f, 200f);
            canvasGo.transform.localScale = Vector3.one * 0.001f;

            var textGo = new GameObject("HintText");
            textGo.transform.SetParent(canvasGo.transform, false);
            _hintLabel = textGo.AddComponent<TMPro.TextMeshProUGUI>();
            _hintLabel.fontSize = 40f;
            _hintLabel.alignment = TextAlignmentOptions.Center;
            _hintLabel.color = Color.white;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(780f, 180f);
            textRt.anchoredPosition = Vector2.zero;

            _hintCanvasTransform = canvasGo.transform;
        }

        // Position same as start/end panel
        Transform cam = Camera.main?.transform;
        if (cam != null)
        {
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            if (fwd.magnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            _hintCanvasTransform.position = cam.position + fwd * 1.0f + Vector3.up * 0.4f;
            _hintCanvasTransform.rotation = Quaternion.LookRotation(fwd);
        }

        _hintLabel.text = msg;
        _hintCanvasTransform.gameObject.SetActive(true);
    }

    void HideHint()
    {
        if (_hintCanvasTransform != null)
            _hintCanvasTransform.gameObject.SetActive(false);
    }



    TextMeshPro CreateWorldLabel(string text, Vector3 pos, float fontSize, Color color)
    {
        var go = new GameObject("WorldLabel");
        go.transform.position = pos;
        if (ovrCameraRig != null)
            go.transform.rotation = Quaternion.LookRotation(pos - ovrCameraRig.position);

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        return tmp;
    }
}

public enum CubeColor { Red, Green, Blue }
