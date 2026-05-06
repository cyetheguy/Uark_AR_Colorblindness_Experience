using UnityEngine;

/// <summary>
/// CubeMotion: Makes cubes spin on their axis and revolve around a center point.
/// Stops when grabbed by player.
/// </summary>
public class CubeMotion : MonoBehaviour
{
    [Header("Rotation")]
    public float spinSpeed  = 90f;
    public Vector3 spinAxis = new Vector3(1f, 1f, 0.5f);

    [Header("Revolution")]
    public float orbitSpeed  = 30f;
    public Vector3 orbitCenter;
    public float orbitRadius;

    private float _orbitAngle;
    private bool _isGrabbed;
    private ColorCube _colorCube;
    private Vector3 _lastPosition;
    private float _grabCheckTimer;

    void Start()
    {
        _colorCube = GetComponent<ColorCube>();
        Vector3 offset = transform.position - orbitCenter;
        _orbitAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        _lastPosition = transform.position;

        // Subscribe to ISDK grab events if available
        TrySubscribeToGrabEvents();
    }

    void TrySubscribeToGrabEvents()
    {
        // Try Oculus.Interaction.Grabbable
        var grabbable = GetComponent<Oculus.Interaction.Grabbable>();
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;
            return;
        }
    }

    void OnPointerEvent(Oculus.Interaction.PointerEvent evt)
    {
        if (evt.Type == Oculus.Interaction.PointerEventType.Select)
        {
            _isGrabbed = true;
            if (_colorCube != null) _colorCube.SetAutoMoving(false);
        }
        else if (evt.Type == Oculus.Interaction.PointerEventType.Unselect)
        {
            _isGrabbed = false;
        }
    }

    void Update()
    {
        if (_isGrabbed) return;

        if (_colorCube != null)
            _colorCube.SetAutoMoving(true);

        // Self rotation
        transform.Rotate(spinAxis.normalized, spinSpeed * Time.deltaTime, Space.World);

        // Orbit
        _orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = _orbitAngle * Mathf.Deg2Rad;
        float currentHeight = transform.position.y;
        transform.position = new Vector3(
            orbitCenter.x + Mathf.Sin(rad) * orbitRadius,
            currentHeight,
            orbitCenter.z + Mathf.Cos(rad) * orbitRadius
        );
    }

    void OnDestroy()
    {
        var grabbable = GetComponent<Oculus.Interaction.Grabbable>();
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }
}
