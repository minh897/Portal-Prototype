using UnityEngine;

public class PortalController : MonoBehaviour
{
    [Header("Portal Cameras")]
    [SerializeField] private Camera outCamera;

    [Header("Portal Renders")]
    [SerializeField] private Transform inTransform;
    [SerializeField] private Transform outTransform;

    private bool isNearPortal = false;
    private bool isWithinThreshold = false;

    private float sideDst;
    private float heightDst;
    private float currentFrontDst;
    private float previousFrontDst;

    private Camera playerCam;
    private PlayerController playerController;

    private const float depthThreshold = 0.001f;
    private const float widthThreshold = 0.6f;
    private const float heightThreshold = 1.2f;

    void Awake()
    {
        playerCam = Camera.main;
        playerController = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {

    }

    void LateUpdate()
    {
        UpdateCameraView();
        UpdateNearClipPlane();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPortal = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPortal = false;
        }
    }

    #region PRIVATE METHODS

    // Update portal camera to mirror the player's view through the portal
    private void UpdateCameraView()
    {
        ApplyMirroredViewThroughPortal(outCamera.transform);
    }

    // Mirrors object position and rotation through the portal relative to the player's camera
    private void ApplyMirroredViewThroughPortal(Transform objectTransform)
    {
        // Convert the player camera’s rotation from world space into inTransform's local space
        // then flip the portal camera 180 degrees around the Y axis
        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * playerCam.transform.rotation;
        Quaternion mirrorRot = Quaternion.AngleAxis(180f, Vector3.up) * relativeRot;

        // Convert the player camera's position from world space into inTransform's local space
        // then reverse the X and Z directions 
        Vector3 relativePos = inTransform.InverseTransformPoint(playerCam.transform.position);
        Vector3 mirrorPos = new(-relativePos.x, relativePos.y, -relativePos.z);

        // Apply new mirror position and rotation to out portal camera
        objectTransform.SetPositionAndRotation(
            outTransform.TransformPoint(mirrorPos),
            outTransform.rotation * mirrorRot);
    }

    // Handle drawing a new near-clip plane using oblique projection matrix
    private void UpdateNearClipPlane()
    {
        if (isNearPortal)
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

    // Check if the player is within the portal threshold for teleportation
    private bool IsWithinPortalThreshold()
    {
        // Track the current and previous player position relative to inTransform in world space
        Vector3 posToCurrent = playerController.GetCurrentPos() - inTransform.position;
        Vector3 posToPrevious = playerController.GetPreviousPos() - inTransform.position;

        // Calculate the player's distance along the normalized portal surface threshold
        currentFrontDst = Vector3.Dot(inTransform.forward.normalized, posToCurrent);
        previousFrontDst = Vector3.Dot(inTransform.forward.normalized, posToPrevious);
        sideDst = Vector3.Dot(inTransform.right.normalized, posToCurrent);
        heightDst = Vector3.Dot(inTransform.up.normalized, posToCurrent);

        // Return true if the player is within the distance threshold
        if (currentFrontDst < depthThreshold
            && previousFrontDst >= -depthThreshold
            && Mathf.Abs(sideDst) <= widthThreshold
            && Mathf.Abs(heightDst) <= heightThreshold)
        {
            return true;
        }

        return false;
    }

    #endregion
}
