using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("UI Settings")]
    [SerializeField] private string promptText = "Interact";

    [Header("Outline Settings")]
    [SerializeField] private bool useOutline = true;
    [SerializeField] private Color outlineColor = Color.green;
    [SerializeField][Range(1.01f, 1.2f)] private float outlineThickness = 1.05f;

    [Header("Focus Hold Settings")]
    [Tooltip("Objeye kaç saniye bakýlýrsa yeni event tetiklensin?")]
    [SerializeField] private float focusHoldTime = 2.0f;
    [SerializeField] private UnityEvent onFocusHold;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private UnityEvent onFocus;
    [SerializeField] private UnityEvent onLoseFocus;

    private GameObject outlineMesh;
    private bool isFocused = false;
    private float focusTimer = 0f;
    private bool holdTriggered = false;

    private void Update()
    {
        if (isFocused && !holdTriggered)
        {
            focusTimer += Time.deltaTime;
            if (focusTimer >= focusHoldTime)
            {
                onFocusHold?.Invoke();
                holdTriggered = true;
            }
        }
    }

    public void Interact()
    {
        onInteract?.Invoke();
    }

    public string GetInteractPrompt()
    {
        return promptText;
    }

    public void OnFocus()
    {
        if (useOutline) ShowOutline();

        isFocused = true;
        focusTimer = 0f;
        holdTriggered = false;

        // Ekranda ipucu sistemimiz aracýlýðýyla etkileþim metnini 2 saniyeliðine gösteriyoruz
        GameEvents.OnShowHint(promptText, 2.0f);

        onFocus?.Invoke();
    }

    public void OnLoseFocus()
    {
        if (useOutline) HideOutline();

        isFocused = false;
        focusTimer = 0f;
        holdTriggered = false;

        onLoseFocus?.Invoke();
    }

    private void ShowOutline()
    {
        if (outlineMesh == null) CreateOutlineMesh();
        outlineMesh.SetActive(true);
    }

    private void HideOutline()
    {
        if (outlineMesh != null) outlineMesh.SetActive(false);
    }

    private void CreateOutlineMesh()
    {
        MeshFilter originalMesh = GetComponent<MeshFilter>();
        if (originalMesh == null) return;

        outlineMesh = new GameObject("DynamicOutline");
        outlineMesh.transform.SetParent(transform, false);
        outlineMesh.transform.localPosition = Vector3.zero;
        outlineMesh.transform.localRotation = Quaternion.identity;
        outlineMesh.transform.localScale = Vector3.one * outlineThickness;

        MeshFilter outMf = outlineMesh.AddComponent<MeshFilter>();

        Mesh clonedMesh = Instantiate(originalMesh.mesh);
        int[] triangles = clonedMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int temp = triangles[i];
            triangles[i] = triangles[i + 1];
            triangles[i + 1] = temp;
        }
        clonedMesh.triangles = triangles;
        outMf.mesh = clonedMesh;

        MeshRenderer outMr = outlineMesh.AddComponent<MeshRenderer>();
        Material outlineMat = new Material(Shader.Find("Unlit/Color"));
        outlineMat.color = outlineColor;
        outMr.material = outlineMat;

        outlineMesh.SetActive(false);
    }

    private void OnDestroy()
    {
        if (outlineMesh != null && outlineMesh.GetComponent<MeshFilter>().mesh != null)
        {
            Destroy(outlineMesh.GetComponent<MeshFilter>().mesh);
        }
    }

    private void OnDisable()
    {
        HideOutline();
    }
}