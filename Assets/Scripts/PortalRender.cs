using UnityEngine;

public class PortalRender : MonoBehaviour
{
    [Header("Portal Cameras")]
    [SerializeField] private Camera inPortalCamera;
    [SerializeField] private Camera outPortalCamera;

    [Header("Portal Renders")]
    [SerializeField] private Transform inPortalTransform;
    [SerializeField] private Transform outPortalTransform;

    private Camera playerCam;

    void Awake()
    {
        playerCam = Camera.main;
    }

    void LateUpdate()
    {
        HandleCameraPosition();
    }

    // Handle portal camera position and rotation, mirroring the player camera movement
    private void HandleCameraPosition()
    {
        // Convert the world space position of the player camera into local position of the in portal 
        // then mirror the position so the other portal camera is place behind of the other portal
        Vector3 relativePos = inPortalTransform.InverseTransformPoint(playerCam.transform.position);
        Vector3 mirrorPos = new(-relativePos.x, relativePos.y, -relativePos.z);

        // Convert the player camera’s rotation from world space into the in portal's local space
        // then flip the portal camera 180 degrees around the Y axis
        Quaternion relativeRot = Quaternion.Inverse(inPortalTransform.rotation) * playerCam.transform.rotation;
        Quaternion mirrorRot = Quaternion.AngleAxis(180f, Vector3.up);

        // Apply the flip rotation to the relative rotation
        relativeRot = mirrorRot * relativeRot;

        outPortalCamera.transform.SetPositionAndRotation(outPortalTransform.TransformPoint(mirrorPos), outPortalTransform.rotation * relativeRot);
    }
}
