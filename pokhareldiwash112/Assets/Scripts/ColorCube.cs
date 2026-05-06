using System.Collections;
using UnityEngine;

/// <summary>
/// ColorCube: Grabbable colored cube.
/// Displays protanopia-simulated color during gameplay.
/// Reveals true color and emits glow on final reveal.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ColorCube : MonoBehaviour
{
    public CubeColor CubeType { get; private set; }
    public bool IsCorrectlyPlaced { get; private set; }

    private Material _mat;
    private Rigidbody _rb;
    private bool _placed;
    private bool _autoMoving; // true when cube is moving automatically

    public void SetAutoMoving(bool value) => _autoMoving = value;

    public void Initialize(CubeColor type, Material mat)
    {
        CubeType = type;
        _mat = mat;
        _rb = GetComponent<Rigidbody>();
    }

    // Call this when player grabs the cube (from OVRGrabbable or XR Grab)
    public void OnGrabbed()
    {
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
    }

    // Works with both collision and trigger
    void OnCollisionEnter(Collision col)
    {
        CheckPedestalContact(col.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        CheckPedestalContact(other.gameObject);
    }

    void CheckPedestalContact(GameObject other)
    {
        if (_placed) return;
        if (_autoMoving) return; // ignore collisions during auto movement

        var pedestal = other.GetComponent<Pedestal>();
        if (pedestal == null) return;
        if (pedestal.IsSolved) return; // pedestal already full

        bool correct = pedestal.PedestalType == CubeType;

        if (correct)
        {
            _placed = true;
            IsCorrectlyPlaced = true;
            pedestal.MarkSolved(this);
            // Pedestal.MarkSolved handles the absorption/destruction
        }

        GameManager.Instance.OnCubePlaced(CubeType, pedestal.PedestalType, correct);
    }

    public void ApplyProtanopiaColor()
    {
        if (_mat == null) return;
        _mat.color = GameManager.Instance.GetProtanopiaColor(CubeType);
    }

    public void RevealTrueColor()
    {
        if (_mat == null) return;
        StartCoroutine(LerpColor(_mat.color, GameManager.Instance.GetTrueColor(CubeType), 1.2f));
    }

    public void EmitGlow()
    {
        if (_mat == null) return;
        _mat.EnableKeyword("_EMISSION");
        Color glowColor = GameManager.Instance.GetTrueColor(CubeType) * 2.5f;
        StartCoroutine(PulseGlow(glowColor));
    }

    IEnumerator LerpColor(Color from, Color to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _mat.color = Color.Lerp(from, to, t / duration);
            yield return null;
        }
        _mat.color = to;
    }

    IEnumerator PulseGlow(Color glowColor)
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * 1.5f;
            float intensity = (Mathf.Sin(t) + 1f) * 0.5f;
            _mat.SetColor("_EmissionColor", glowColor * intensity);
            yield return null;
        }
    }

    // Reset if player drops it in wrong place
    public void ResetPlacement()
    {
        _placed = false;
        IsCorrectlyPlaced = false;
        _rb.isKinematic = false;
    }
}
