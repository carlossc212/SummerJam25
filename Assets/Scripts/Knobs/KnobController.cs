using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class KnobController : MonoBehaviour
{
    private Transform actualColorTransform, askedColorTransform, actualColorPivotTransform;
    private int actualColor, askedColor;
    [SerializeField] EventReference clickSound, matchClickSound, breakClickSound;
    private GameController gameController;


    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();

        int startColor = Random.Range(0, 3);

        actualColor = startColor;
        askedColor = startColor;

        // Make sure these child objects exist and are correctly named
        actualColorPivotTransform = this.transform.Find("actualColorPivot");
        actualColorTransform = actualColorPivotTransform.transform.Find("actualColor");
        askedColorTransform = this.transform.Find("askedColor");

        // Optional: Check if transforms were found, as a good practice
        if (actualColorTransform == null)
            Debug.LogError("Child object 'actualColor' not found!", this);
        if (actualColorPivotTransform == null)
            Debug.LogError("Child object 'actualColorPivot' not found!", this);
        if (askedColorTransform == null)
            Debug.LogError("Child object 'askedColor' not found!", this);

        // Initialize colors when the script starts
        updateTransformColor(actualColorTransform, actualColor);
        updateTransformColor(askedColorTransform, askedColor);
        UpdatePivotRotation(actualColorPivotTransform, actualColor);
    }


    private void OnMouseDown()
    {
        if (!gameController.gameActive) return;
        updateActualColor();
    }

    public void updateActualColor() {
        if (actualColor == 2)
        {
            actualColor = 0;
        }
        else {
            actualColor++;
        }

        updateTransformColor(actualColorTransform, actualColor);
        UpdatePivotRotation(actualColorPivotTransform, actualColor);

        if (actualColor == askedColor)
        {
            RuntimeManager.PlayOneShot(matchClickSound);
        }
        else {
            RuntimeManager.PlayOneShot(clickSound);
        }

    }

    public void updateAskedColor() {
        int randomValue = Random.Range(0, 3);

        if (randomValue == actualColor) { 
            updateAskedColor();
            return;
        }

        updateTransformColor(askedColorTransform, randomValue);
        RuntimeManager.PlayOneShot(breakClickSound);
        askedColor = randomValue;
    }

    private void updateTransformColor(Transform targetTransform, int colorId)
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("Cannot update color: targetTransform is null.");
            return;
        }

        // Get the Renderer component attached to the target GameObject
        Renderer targetRenderer = targetTransform.GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError($"No Renderer found on GameObject '{targetTransform.name}'. Cannot change color.", targetTransform.gameObject);
            return;
        }

        // Access the material through the renderer
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

        float targetRotationY = 0f; // Por defecto, 0 grados

        switch (colorId)
        {
            case 0:
                targetRotationY = -60f; // Rojo
                break;
            case 1:
                targetRotationY = 0f;    // Amarillo
                break;
            case 2:
                targetRotationY = 60f;   // Verde
                break;
        }

        // Establecer la rotación del pivote.
        // Quaternion.Euler crea una rotación a partir de ángulos de Euler.
        // Mantenemos X y Z en 0, y solo cambiamos Y.
        pivotTransform.localRotation = Quaternion.Euler(0f, targetRotationY, 0f);
    }

    public bool IsCorrectlyColored()
    {
        return actualColor == askedColor;
    }
}
