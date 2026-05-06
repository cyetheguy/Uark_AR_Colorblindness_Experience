using System.Collections;
using UnityEngine;

/// <summary>
/// Pedestal: Accepts multiple cubes of matching color.
/// Each cube is silently absorbed (fade + shrink).
/// When required count reached, pedestal is solved.
/// </summary>
public class Pedestal : MonoBehaviour
{
    public CubeColor PedestalType { get; private set; }
    public bool IsSolved { get; private set; }

    private Material _mat;
    private int _required;
    private int _count;

    public void Initialize(CubeColor type, Material mat, int required = 1)
    {
        PedestalType = type;
        _mat = mat;
        _required = required;
        _count = 0;

        // Trigger collider
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Large trigger zone above pedestal
        var trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size   = new Vector3(1.5f, 3f, 1.5f);
        trigger.center = new Vector3(0f, 1f, 0f);
    }

    public void MarkSolved(ColorCube cube)
    {
        if (IsSolved) return;
        _count++;

        // Set IsSolved BEFORE starting coroutine so OnCubePlaced check is accurate
        if (_count >= _required)
            IsSolved = true;

        // Absorb cube with fade
        StartCoroutine(AbsorbCube(cube.gameObject));

        if (IsSolved)
            StartCoroutine(SolvedPulse());
    }

    IEnumerator AbsorbCube(GameObject cube)
    {
        if (cube == null) yield break;

        // Disable grab while absorbing
        var rb = cube.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        var rend = cube.GetComponent<Renderer>();
        Vector3 startScale = cube.transform.localScale;
        Vector3 targetPos  = transform.position;
        Color startColor   = rend != null ? rend.material.color : Color.white;

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            cube.transform.position   = Vector3.Lerp(cube.transform.position, targetPos, t);
            cube.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 1f - t;
                rend.material.color = c;
            }

            yield return null;
        }

        Destroy(cube);
    }

    IEnumerator SolvedPulse()
    {
        Vector3 original = transform.localScale;
        transform.localScale = original * 1.3f;
        yield return new WaitForSeconds(0.15f);
        transform.localScale = original;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = original * 1.15f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = original;
    }

    public void ApplyProtanopiaColor()
    {
        if (_mat == null) return;
        _mat.color = GameManager.Instance.GetProtanopiaColor(PedestalType) * 0.7f;
    }

    public void RevealTrueColor()
    {
        if (_mat == null) return;
        Color trueColor = GameManager.Instance.GetTrueColor(PedestalType);
        StartCoroutine(LerpColor(_mat.color, trueColor * 0.7f, 1.2f));
        _mat.EnableKeyword("_EMISSION");
        _mat.SetColor("_EmissionColor", trueColor * 1.2f);
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
}
