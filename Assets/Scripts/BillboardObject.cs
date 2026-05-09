using UnityEngine;

public class BillboardObject : MonoBehaviour
{
    private Camera cam => Camera.main;

    private void LateUpdate()
    {
        transform.forward = cam.transform.forward;
    }
}