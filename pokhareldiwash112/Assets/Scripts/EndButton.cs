using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// EndButton: Generic button interaction handler.
/// Works with OVR Ray Interactor, hand poke, and mouse click.
/// </summary>
public class EndButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image _img;
    private Color _normalColor;
    private Color _hoverColor;
    private Color _pressColor;
    private System.Action _onClick;
    private bool _pressed;

    [Tooltip("If true, button can be pressed multiple times")]
    public bool repeatable = false;

    public void Initialize(Image img, System.Action onClick)
    {
        _img = img;
        _onClick = onClick;
        _normalColor = img.color;
        _hoverColor  = Color.Lerp(img.color, Color.white, 0.3f);
        _pressColor  = Color.Lerp(img.color, Color.black, 0.2f);
    }

    public void SetRepeatable(bool value) => repeatable = value;

    public void OnPointerClick(PointerEventData eventData) => TriggerPress();
    public void OnPointerEnter(PointerEventData eventData) { if (!_pressed && _img) _img.color = _hoverColor; }
    public void OnPointerExit(PointerEventData eventData)  { if (!_pressed && _img) _img.color = _normalColor; }

    void OnTriggerEnter(Collider other) => TriggerPress();
    void OnMouseDown()  => TriggerPress();
    void OnMouseEnter() { if (!_pressed && _img) _img.color = _hoverColor; }
    void OnMouseExit()  { if (!_pressed && _img) _img.color = _normalColor; }

    void Update()
    {
        if (_pressed && !repeatable) return;
        #if UNITY_ANDROID
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (IsBeingGazedAt()) TriggerPress();
        }
        #endif
    }

    bool IsBeingGazedAt()
    {
        if (Camera.main == null) return false;
        var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        var col = GetComponent<Collider>();
        return col != null && col.bounds.IntersectRay(ray);
    }

    void TriggerPress()
    {
        if (_pressed && !repeatable) return;
        if (!repeatable) _pressed = true;
        if (_img) _img.color = _pressColor;
        _onClick?.Invoke();
        if (repeatable) StartCoroutine(ResetColor());
    }

    System.Collections.IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(0.15f);
        if (_img) _img.color = _normalColor;
    }
}
