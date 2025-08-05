using System.ComponentModel.Design;
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
        isWithinThreshold = CheckForPortalCrossing();

        if (isWithinThreshold)
        {
            Debug.Log("Teleported!");
            TeleportObject(playerController.transform);
        }
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

    // Teleport object to the other portal
    private void TeleportObject(Transform objectToTeleport)
    {
        MirrorTransformThroughPortal(objectToTeleport, out Vector3 mirrorPos, out Quaternion mirrorRot);

        // Update the new position and rotation to teleported object
        objectToTeleport.SetPositionAndRotation(
            outTransform.TransformPoint(mirrorPos),
            outTransform.rotation * mirrorRot);

        // Optional: For fail-safe or debugging, reset flag for the next portal crossing check
        isWithinThreshold = false;
    }

    // Update portal camera to mirror the player's view through the portal
    private void UpdateCameraView()
    {
        MirrorTransformThroughPortal(playerCam.transform, out Vector3 mirrorPos, out Quaternion mirrorRot);

        // Update the new position and rotation to out portal camera
        outCamera.transform.SetPositionAndRotation(
            outTransform.TransformPoint(mirrorPos),
            outTransform.rotation * mirrorRot);
    }

    // Mirrors object position and rotation through the portal relative to the player's camera
    private void MirrorTransformThroughPortal(Transform relativeObject, out Vector3 mirrorPos, out Quaternion mirrorRot)
    {
        // Convert the player camera's position from world space into inTransform's local space
        // then reverse the X and Z directions 
        Vector3 relativePos = inTransform.InverseTransformPoint(relativeObject.position);
        mirrorPos = new(-relativePos.x, relativePos.y, -relativePos.z);

        // Convert the player camera’s rotation from world space into inTransform's local space
        // then flip the portal camera 180 degrees around the Y axis
        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * relativeObject.rotation;
        mirrorRot = Quaternion.AngleAxis(180f, Vector3.up) * relativeRot;
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
    private bool CheckForPortalCrossing()
    {
        // Track the current and previous player position relative to inTransform in world space
        Vector3 posToCurrent = playerCam.transform.position - inTransform.position;
        Vector3 posToPrevious = playerController.GetPreviousPos() - inTransform.position;

        // Calculate the player's distance along the normalized portal surface threshold
        currentFrontDst = Vector3.Dot(inTransform.forward, posToCurrent);
        previousFrontDst = Vector3.Dot(inTransform.forward, posToPrevious);
        sideDst = Vector3.Dot(inTransform.right, posToCurrent);
        heightDst = Vector3.Dot(inTransform.up, posToCurrent);

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
