using UnityEngine;

public abstract class BillboardObject : MonoBehaviour
{
    protected Camera cam;

    protected virtual void Awake()
    {
        cam = Camera.main;
    }

    protected virtual void LateUpdate()
    {
        transform.forward = cam.transform.forward;
    }
}