using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ColorblindExperience: Pure passthrough experience with buttons
/// to switch between different colorblindness filters in real time.
/// No gameplay — just see the world through different eyes.
/// </summary>
public class ColorblindExperience : MonoBehaviour
{
    private Transform _cameraRig;
    private OVRPassthroughLayer _passthroughLayer;
    private GameObject _canvasGo;
    private string _currentFilter = "None";

    private Texture2D _protanopiaLUT;
    private Texture2D _deuteranopiaLUT;
    private Texture2D _tritanopiaLUT;
    private Texture2D _identityLUT;

    public void Initialize(Transform cameraRig)
    {
        _cameraRig = cameraRig;
        _passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();

        // Debug: print all properties containing "lut" or "color"
        if (_passthroughLayer != null)
        {
            foreach (var prop in _passthroughLayer.GetType().GetProperties())
                if (prop.Name.ToLower().Contains("lut") || prop.Name.ToLower().Contains("color"))
                    Debug.Log($"[CBExp] Property: {prop.Name} ({prop.PropertyType})");
            foreach (var field in _passthroughLayer.GetType().GetFields())
                if (field.Name.ToLower().Contains("lut") || field.Name.ToLower().Contains("color"))
                    Debug.Log($"[CBExp] Field: {field.Name} ({field.FieldType})");
        }

        // Load LUTs from Resources folder
        _protanopiaLUT   = Resources.Load<Texture2D>("ProtanopiaLUT");
        _deuteranopiaLUT = Resources.Load<Texture2D>("DeuteranopiaLUT");
        _tritanopiaLUT   = Resources.Load<Texture2D>("TritanopiaLUT");
        _identityLUT     = Resources.Load<Texture2D>("IdentityLUT");

        // Reset filter to none
        SetFilter("None");

        // Build UI
        StartCoroutine(SpawnAfterDelay(0.5f));
    }

    IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BuildUI();
    }

    void BuildUI()
    {
        _canvasGo = new GameObject("ColorblindCanvas");
        var canvas = _canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        _canvasGo.AddComponent<CanvasScaler>();
        _canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem with OVRInputModule
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var ovrInputType = System.Type.GetType("OVRInputModule, Assembly-CSharp");
            if (ovrInputType != null) esGo.AddComponent(ovrInputType);
            else esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var rt = _canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 600f);
        _canvasGo.transform.localScale = Vector3.one * 0.001f;

        // Position in front of player
        PositionCanvas();

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(_canvasGo.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Title
        CreateText("COLORBLIND EXPERIENCE", new Vector2(0, 220), 42,
            new Color(1f, 0.85f, 0.2f), FontStyles.Bold);

        // Subtitle
        CreateText("See the world through different eyes",
            new Vector2(0, 155), 24, new Color(0.8f, 0.8f, 0.8f));

        // Filter buttons row
        CreateFilterButton("None",         new Vector2(-280f,  50f), new Color(0.3f, 0.3f, 0.3f));
        CreateFilterButton("Protanopia",   new Vector2( -80f,  50f), new Color(0.6f, 0.2f, 0.2f));
        CreateFilterButton("Deuteranopia", new Vector2(  120f, 50f), new Color(0.2f, 0.5f, 0.2f));
        CreateFilterButton("Tritanopia",   new Vector2(  320f, 50f), new Color(0.2f, 0.3f, 0.6f));

        // Description label
        CreateText("Select a filter to experience\ndifferent types of colorblindness",
            new Vector2(0, -80), 24, Color.white);

        // Back button
        CreateActionButton("BACK TO GAME", new Vector2(0, -220),
            new Color(0.4f, 0.4f, 0.4f), () =>
        {
            SetFilter("None");
            Destroy(_canvasGo);
            Destroy(this);
        });

        StartCoroutine(ScaleIn(_canvasGo.transform));
    }

    void PositionCanvas()
    {
        Transform cam = Camera.main?.transform;
        if (cam != null)
        {
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            if (fwd.magnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            _canvasGo.transform.position = cam.position + fwd * 1.0f + Vector3.up * 0.1f;
            _canvasGo.transform.rotation = Quaternion.LookRotation(fwd);
        }
    }

    void CreateFilterButton(string filterName, Vector2 pos, Color color)
    {
        var btnGo = new GameObject($"Btn_{filterName}");
        btnGo.transform.SetParent(_canvasGo.transform, false);

        var img = btnGo.AddComponent<Image>();
        img.color = color;

        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchoredPosition = pos;
        btnRt.sizeDelta = new Vector2(175f, 90f);

        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;
        string captured = filterName;
        btn.onClick.AddListener(() => SetFilter(captured));

        var col = btnGo.AddComponent<BoxCollider>();
        col.size = new Vector3(175f, 90f, 80f);

        var endBtn = btnGo.AddComponent<EndButton>();
        endBtn.Initialize(img, () => SetFilter(captured));
        endBtn.SetRepeatable(true);

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = filterName;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
    }

    void CreateActionButton(string label, Vector2 pos, Color color, System.Action onClick)
    {
        var btnGo = new GameObject("ActionBtn");
        btnGo.transform.SetParent(_canvasGo.transform, false);

        var img = btnGo.AddComponent<Image>();
        img.color = color;

        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchoredPosition = pos;
        btnRt.sizeDelta = new Vector2(300f, 80f);

        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());

        var col = btnGo.AddComponent<BoxCollider>();
        col.size = new Vector3(300f, 80f, 80f);

        var endBtn = btnGo.AddComponent<EndButton>();
        endBtn.Initialize(img, onClick);

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
    }

    void CreateText(string text, Vector2 pos, float fontSize, Color color,
        FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(_canvasGo.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(750f, 120f);
    }

    void SetFilter(string filterName)
    {
        _currentFilter = filterName;

        if (_passthroughLayer == null)
        {
            Debug.LogWarning("[ColorblindExperience] No OVRPassthroughLayer found");
            return;
        }

        // Use the same working LUT (assigned in Inspector) at different weights
        // to simulate different types/severities of colorblindness
        switch (filterName)
        {
            case "None":
                try { _passthroughLayer.DisableColorMap(); }
                catch (System.Exception e) { Debug.LogWarning($"DisableColorMap: {e.Message}"); }
                break;

            case "Protanopia":
                if (_protanopiaLUT != null)
                {
                    var lut = new OVRPassthroughColorLut(_protanopiaLUT, true);
                    _passthroughLayer.SetColorLut(lut, 1.0f);
                }
                break;

            case "Deuteranopia":
                if (_deuteranopiaLUT != null)
                {
                    var lut = new OVRPassthroughColorLut(_deuteranopiaLUT, true);
                    _passthroughLayer.SetColorLut(lut, 1.0f);
                }
                break;

            case "Tritanopia":
                if (_tritanopiaLUT != null)
                {
                    var lut = new OVRPassthroughColorLut(_tritanopiaLUT, true);
                    _passthroughLayer.SetColorLut(lut, 1.0f);
                }
                break;
        }

        Debug.Log($"[ColorblindExperience] Filter: {filterName}");
    }

    void ApplyColorScale(Color redShift, Color greenShift)
    {
        // Use colorScale/colorOffset as fallback
        try
        {
            var scaleProp = _passthroughLayer.GetType().GetProperty("colorScale");
            var offsetProp = _passthroughLayer.GetType().GetProperty("colorOffset");
            if (scaleProp != null)  scaleProp.SetValue(_passthroughLayer, redShift);
            if (offsetProp != null) offsetProp.SetValue(_passthroughLayer, greenShift * 0.1f);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ColorblindExperience] ApplyColorScale failed: {e.Message}");
        }
    }

    void ResetColorScale()
    {
        try
        {
            var scaleProp = _passthroughLayer.GetType().GetProperty("colorScale");
            var offsetProp = _passthroughLayer.GetType().GetProperty("colorOffset");
            if (scaleProp != null)  scaleProp.SetValue(_passthroughLayer, Color.white);
            if (offsetProp != null) offsetProp.SetValue(_passthroughLayer, Color.clear);
        }
        catch { }
    }

    void SetLutTexture(Texture2D lut)
    {
        if (lut == null)
        {
            Debug.LogWarning("[ColorblindExperience] LUT texture is null — check Resources folder");
            return;
        }
        try
        {
            var colorLut = new OVRPassthroughColorLut(lut, true);
            _passthroughLayer.SetColorLut(colorLut, 1f);
            Debug.Log($"[ColorblindExperience] LUT applied: {lut.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ColorblindExperience] SetLutTexture failed: {e.Message}");
        }
    }

    void SetLutWeight(float weight)
    {
        try
        {
            // SetColorLut with weight parameter controls blend
            // To remove LUT, call DisableColorMap
            if (weight <= 0f)
                _passthroughLayer.DisableColorMap();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ColorblindExperience] SetLutWeight failed: {e.Message}");
        }
    }

    IEnumerator ScaleIn(Transform t)
    {
        Vector3 target = t.localScale;
        t.localScale = Vector3.zero;
        float e = 0f;
        while (e < 0.5f)
        {
            e += Time.deltaTime;
            t.localScale = target * Mathf.SmoothStep(0f, 1f, e / 0.5f);
            yield return null;
        }
        t.localScale = target;
    }
}
