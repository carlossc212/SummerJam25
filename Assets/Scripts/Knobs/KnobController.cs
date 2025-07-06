using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class KnobController : MonoBehaviour
{
    [SerializeField] private Transform actualColorTransform, askedColorTransform, actualColorPivotTransform;
    private int actualColor, askedColor;
    [SerializeField] EventReference clickSound, matchClickSound, breakClickSound;
    private GameController gameController;

    [SerializeField] private float bumpHeight = 0.1f, bumpScaleHeight = 0.2f, bumpScaleDuration = 0.1f; // Controla desplazamiento y escala
    private float rotationDuration = 0.2f;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();

        int startColor = Random.Range(0, 3);

        actualColor = startColor;
        askedColor = startColor;


        if (actualColorTransform == null)
            Debug.LogError("Child object 'actualColor' not found!", this);
        if (actualColorPivotTransform == null)
            Debug.LogError("Child object 'actualColorPivot' not found!", this);
        if (askedColorTransform == null)
            Debug.LogError("Child object 'askedColor' not found!", this);

        updateTransformColor(actualColorTransform, actualColor);
        updateTransformColor(askedColorTransform, askedColor);
        UpdatePivotRotation(actualColorPivotTransform, actualColor);
    }

    private void OnMouseDown()
    {
        if (!gameController.gameActive) return;
        updateActualColor();
    }

    public void updateActualColor()
    {
        actualColor = (actualColor + 1) % 3;

        updateTransformColor(actualColorTransform, actualColor);
        UpdatePivotRotation(actualColorPivotTransform, actualColor);

        if (actualColor == askedColor)
        {
            RuntimeManager.PlayOneShot(matchClickSound);
        }
        else
        {
            RuntimeManager.PlayOneShot(clickSound);
        }
    }

    public void updateAskedColor()
    {
        int randomValue = Random.Range(0, 3);

        if (randomValue == actualColor)
        {
            updateAskedColor();
            return;
        }

        updateTransformColor(askedColorTransform, randomValue);
        RuntimeManager.PlayOneShot(breakClickSound);
        askedColor = randomValue;

        // Bump de escala en el pivote al cambiar el color pedido
        StartCoroutine(ScaleBump(askedColorTransform, bumpScaleDuration));
    }

    private void updateTransformColor(Transform targetTransform, int colorId)
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("Cannot update color: targetTransform is null.");
            return;
        }

        Renderer targetRenderer = targetTransform.GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError($"No Renderer found on GameObject '{targetTransform.name}'. Cannot change color.", targetTransform.gameObject);
            return;
        }

        switch (colorId)
        {
            case 0:
                targetRenderer.material.color = Color.red;
                break;
            case 1:
                targetRenderer.material.color = Color.yellow;
                break;
            case 2:
                targetRenderer.material.color = Color.green;
                break;
        }
    }

    private void UpdatePivotRotation(Transform pivotTransform, int colorId)
    {
        if (pivotTransform == null)
        {
            Debug.LogWarning("Cannot update pivot rotation: pivotTransform is null.");
            return;
        }

        float targetRotationY = 0f;

        switch (colorId)
        {
            case 0: targetRotationY = -60f; break; // Rojo
            case 1: targetRotationY = 0f; break; // Amarillo
            case 2: targetRotationY = 60f; break; // Verde
        }

        // Inicia la rotación con easing y bump vertical
        StartCoroutine(RotateTo(pivotTransform, Quaternion.Euler(0f, targetRotationY, 0f), rotationDuration));
    }

    private IEnumerator RotateTo(Transform target, Quaternion targetRotation, float duration)
    {
        Quaternion startRotation = target.localRotation;

        Vector3 startLocalPos = transform.localPosition;
        Vector3 peakLocalPos = startLocalPos + new Vector3(0f, bumpHeight, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            float easedT = t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
            target.localRotation = Quaternion.Slerp(startRotation, targetRotation, easedT);

            float yCurve = Mathf.Sin(easedT * Mathf.PI); // Subida y bajada
            transform.localPosition = Vector3.Lerp(startLocalPos, peakLocalPos, yCurve);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localRotation = targetRotation;
        transform.localPosition = startLocalPos;
    }

    private IEnumerator ScaleBump(Transform target, float duration)
    {
        Vector3 originalScale = target.localScale;
        Vector3 peakScale = originalScale * (1f + bumpScaleHeight);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
            float curve = Mathf.Sin(easedT * Mathf.PI); // 0 → 1 → 0

            target.localScale = Vector3.Lerp(originalScale, peakScale, curve);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale;
    }

    public bool IsCorrectlyColored()
    {
        return actualColor == askedColor;
    }
}
