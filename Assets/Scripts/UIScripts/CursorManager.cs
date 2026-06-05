using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnSetCursorState += HandleCursorState;
    }

    private void OnDisable()
    {
        GameEvents.OnSetCursorState -= HandleCursorState;
    }

    private void Start()
    {
        HandleCursorState(true);
    }

    private void HandleCursorState(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}