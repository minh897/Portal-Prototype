using UnityEngine;

public class PortalRender : MonoBehaviour
{
    [Header("Portal Cameras")]
    [SerializeField] private Camera inPortalCamera;
    [SerializeField] private Camera outPortalCamera;

    [Header("Portal Renders")]
    [SerializeField] private Transform inPortalRender;
    [SerializeField] private Transform outPortalRender;

    private Camera playerCam;

    void Awake()
    {
        playerCam = Camera.main;
    }

    void LateUpdate()
    {
        HandleCameraPosition();
    }

    private void HandleCameraPosition()
    {
        // Convert the world space position of the player camera into local position of the in portal 
        // then mirror the position so the other portal camera is place behind of the other portal
        Vector3 relativePos = inPortalRender.InverseTransformPoint(playerCam.transform.position);
        Vector3 mirrorPos = new(-relativePos.x, relativePos.y, -relativePos.z);

        // Converting the player camera’s rotation from world space into the in portal's local space
        // then flip the portal camera 180 degrees around the Y axis
        Quaternion relativeRot = Quaternion.Inverse(inPortalRender.rotation) * playerCam.transform.rotation;
        Quaternion mirrorRot = Quaternion.AngleAxis(180f, Vector3.up);
        relativeRot = mirrorRot * relativeRot;

        outPortalCamera.transform.SetPositionAndRotation(outPortalRender.TransformPoint(mirrorPos), outPortalRender.rotation * relativeRot);
    }
}
