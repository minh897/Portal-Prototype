using UnityEngine;

public class PortalRenderer : MonoBehaviour
{
    [Header("Portal Cameras")]
    [SerializeField] private Camera inCamera;
    [SerializeField] private Camera outCamera;

    [Header("Portal Renders")]
    [SerializeField] private Transform inTransform;
    [SerializeField] private Transform outTransform;

    private Camera playerCam;

    void Awake()
    {
        playerCam = Camera.main;
    }

    void LateUpdate()
    {
        HandleCameraPosition();
        HandleNearClipPlane();
    }

    // Check if the player is near the portal surface
    private bool IsNearPortal()
    {
        // Track the current player position relative to inTransform in world space
        // Then calculate the player's distance along the normalized forward direction
        // Return true if the player is in front within the distance threshold
        Vector3 portalToPlayer = playerCam.transform.position - inTransform.position;
        float frontDistance = Vector3.Dot(inTransform.forward.normalized, portalToPlayer);
        return frontDistance >= -0.2f && frontDistance < 0.2f;
    }

    // Handle portal camera position and rotation, mirroring the player camera movement
    private void HandleCameraPosition()
    {
        // Convert the world space position of the player camera into local position of the in portal 
        // then mirror the position so the other portal camera is place behind of the other portal
        Vector3 relativePos = inTransform.InverseTransformPoint(playerCam.transform.position);
        Vector3 mirrorPos = new(-relativePos.x, relativePos.y, -relativePos.z);

        // Convert the player camera’s rotation from world space into the in portal's local space
        // then flip the portal camera 180 degrees around the Y axis
        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * playerCam.transform.rotation;
        Quaternion mirrorRot = Quaternion.AngleAxis(180f, Vector3.up);

        // Apply the flip rotation to the relative rotation
        relativeRot = mirrorRot * relativeRot;

        outCamera.transform.SetPositionAndRotation(outTransform.TransformPoint(mirrorPos), outTransform.rotation * relativeRot);
    }

    // Handle drawing a new near-clip plane using oblique projection matrix
    private void HandleNearClipPlane()
    {
        if (IsNearPortal())
        {
            outCamera.ResetProjectionMatrix();
            return;
        }

        // Build a clip plane in world space (normalized forward vector for safety)
        // with a constant d so the plane passes through outTransform.position
        Vector3 normal = outTransform.forward.normalized;
        float d = Vector3.Dot(outTransform.position, outTransform.forward.normalized);
        Vector4 clipPlaneWorldSpace = new(normal.x, normal.y, normal.z, -d);

        // Transform the plane into camera space
        Vector4 planeToCameraSpace = outCamera.cameraToWorldMatrix.transpose * clipPlaneWorldSpace;

        // Update the projection matrix based on the new near-clip plane from the player camera perspective
        outCamera.projectionMatrix = playerCam.CalculateObliqueMatrix(planeToCameraSpace);
    }
}
