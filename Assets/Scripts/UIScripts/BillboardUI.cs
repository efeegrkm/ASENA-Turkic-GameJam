using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCam != null)
        {
            // UI'ýn sürekli kameranýn baktýðý yöne (oyuncunun gözüne) dönmesini saðlar
            transform.LookAt(transform.position + mainCam.transform.forward);
        }
    }
}