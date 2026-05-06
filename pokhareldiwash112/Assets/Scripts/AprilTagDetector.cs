using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// AprilTagDetector: Uses AR Foundation image tracking to detect Tag.png.
/// When detected, shows the StartScreenUI panel.
/// 
/// SETUP:
/// 1. Add this script to a GameObject called "TagDetector"
/// 2. Add ARTrackedImageManager to the same GameObject
/// 3. Assign TagImageLibrary to ARTrackedImageManager.referenceLibrary
/// 4. Assign [BuildingBlock] Camera Rig to cameraRig field
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class AprilTagDetector : MonoBehaviour
{
    [Tooltip("Drag your [BuildingBlock] Camera Rig here")]
    public Transform cameraRig;

    private ARTrackedImageManager _imageManager;
    private bool _tagFound;

    void Awake()
    {
        _imageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        _imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        _imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void Start()
    {
        ShowWaitingPrompt();
    }

    void ShowWaitingPrompt()
    {
        var canvasGo = new GameObject("WaitingCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();

        var rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600f, 200f);
        canvasGo.transform.localScale = Vector3.one * 0.001f;

        var textGo = new GameObject("WaitingText");
        textGo.transform.SetParent(canvasGo.transform, false);
        var tmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "Hold up the\nActivation Card to begin";
        tmp.fontSize = 48f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(580f, 180f);
        textRt.anchoredPosition = Vector2.zero;

        StartCoroutine(FollowPlayerUntilFound(canvasGo.transform, tmp));
    }

    IEnumerator FollowPlayerUntilFound(Transform label, TMPro.TextMeshProUGUI tmp)
    {
        float t = 0f;
        while (!_tagFound)
        {
            t += Time.deltaTime * 1.5f;
            tmp.color = new Color(1f, 1f, 1f, (Mathf.Sin(t) + 1f) * 0.5f);

            Transform cam = Camera.main?.transform ?? cameraRig;
            if (cam != null)
            {
                Vector3 fwd = cam.forward;
                fwd.y = 0f;
                if (fwd.magnitude < 0.01f) fwd = Vector3.forward;
                fwd.Normalize();
                label.position = cam.position + fwd * 1.0f + Vector3.up * 0.1f;
                label.rotation = Quaternion.LookRotation(fwd);
            }
            yield return null;
        }
        Destroy(label.gameObject);
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
            HandleTagDetected(trackedImage);

        foreach (var trackedImage in args.updated)
            if (trackedImage.trackingState == TrackingState.Tracking)
                HandleTagDetected(trackedImage);
    }

    void HandleTagDetected(ARTrackedImage trackedImage)
    {
        if (_tagFound) return;
        _tagFound = true;

        Debug.Log($"[AprilTagDetector] Tag detected: {trackedImage.referenceImage.name}");

        if (GameManager.Instance != null && cameraRig != null)
            GameManager.Instance.ovrCameraRig = cameraRig;

        StartCoroutine(ShowStartScreen());
    }

    IEnumerator ShowStartScreen()
    {
        yield return new WaitForSeconds(0.4f);

        Transform camTransform = Camera.main?.transform ?? cameraRig;
        var startGo = new GameObject("StartScreenUI");
        var startScreen = startGo.AddComponent<StartScreenUI>();
        startScreen.Initialize(camTransform);

        _imageManager.enabled = false;
    }
}
