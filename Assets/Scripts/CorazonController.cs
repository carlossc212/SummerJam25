using System.Collections;
using UnityEngine;

public class CorazonController : MonoBehaviour
{
    public float scaleFactor = 1.2f;
    public float scaleDuration = 0.2f;

    private Vector3 originalScale;
    private Coroutine scaleRoutine;

    private Music music;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        music = FindObjectOfType<Music>();

        music.OnBeat += DoScale;
    }

    public void DoScale(int i)
    {
        if (!gameObject.activeSelf) return;

        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
        }
        scaleRoutine = StartCoroutine(ScalePulse());
    }

    private IEnumerator ScalePulse()
    {
        Vector3 targetScale = originalScale * scaleFactor;

        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        scaleRoutine = null;
    }
}
