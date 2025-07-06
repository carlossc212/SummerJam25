using System.Collections;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    [SerializeField] private float bumpHeight = -10f;
    [SerializeField] private float bumpDuration = 0.5f;

    private Vector3 initialLocalPosition;
    private Coroutine bumpCoroutine;

    private Renderer _renderer;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        _renderer = GetComponent<Renderer>();
    }

    public void activate()
    {
        SetColor(Color.yellow);
    }

    public void deactivate()
    {
        SetColor(Color.gray);
    }

    public void block()
    {
        SetColor(Color.red);
    }

    public bool guess(bool correct)
    {
        if (correct)
        {
            SetColor(Color.green);
            return true;
        }
        else
        {
            SetColor(Color.red);
            return false;
        }
    }

    private void SetColor(Color newColor)
    {
        if (_renderer.material.color == newColor)
            return;

        _renderer.material.color = newColor;
        PlayBump();
    }

    private void PlayBump()
    {
        if (bumpCoroutine != null)
        {
            StopCoroutine(bumpCoroutine);
        }
        bumpCoroutine = StartCoroutine(BumpTween());
    }

    private IEnumerator BumpTween()
    {
        float elapsed = 0f;
        Vector3 startPos = initialLocalPosition;
        Vector3 peakPos = initialLocalPosition + Vector3.up * bumpHeight;

        while (elapsed < bumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bumpDuration);
            float easedT = EaseOutExpo(t);
            transform.localPosition = Vector3.Lerp(startPos, peakPos, easedT);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < bumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bumpDuration);
            float easedT = EaseOutExpo(t);
            transform.localPosition = Vector3.Lerp(peakPos, startPos, easedT);
            yield return null;
        }

        transform.localPosition = initialLocalPosition;
    }

    private float EaseOutExpo(float t)
    {
        return t >= 1f ? 1f : 1 - Mathf.Pow(2f, -10f * t);
    }
}
