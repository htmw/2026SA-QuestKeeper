using UnityEngine;

public class Main_Battle_Camera : MonoBehaviour
{
    /* TARGETS
     * These are the two fighters the camera tracks; player vs opponent
     */
    [Header("Targets")]
    [Tooltip("Player")]
    public Transform player;

    [Tooltip("Opponent")]
    public Transform opponent;

    /* STAGE LIMITS
     * Prevents camera from showing too far past the stage
     * Parameters
     * minX = farthest left the camera can go
     * maxX = farther right the camera can go
     * fixedY = Fixed Vertical Position
     */
    [Space(10)]
    [Header("Stage Limits")]
    [Tooltip("Leftmost X Camera position")]
    public float minX = -7f;

    [Tooltip("Rightmost X Camera position")]
    public float maxX = 7f;

    [Tooltip("Fixed Vertical Camera Position")]
    public float fixedY = 1.5f;

    /* ZOOM SETTINGS
     * minZoom = zoomed in value
     * maxZoom = zoomed out value
     * zoomLimiter = how quickly zoom changes based on fighter distance
     * zoomSmoothTime = how fast/smoothly zoom changes
     * zoomDeadZone = ignores small distance changes to reduce twitching effect
    */
    [Space(10)]
    [Header("Zoom Settings")]
    [Tooltip("Closest zoom level. Smaller number means more zoomed in.")]
    public float minZoom = 3.5f;

    [Tooltip("Farthest zoom level. Larger number means more zoomed out.")]
    public float maxZoom = 6f;

    [Tooltip("Controls how much horizontal distance affects zoom.")]
    public float zoomLimiter = 8f;

    [Tooltip("How smoothly the camera transitions between zoom levels.")]
    public float zoomSmoothTime = 0.15f;

    [Tooltip("Ignores very small distance changes so the camera does not feel twitchy.")]
    public float zoomDeadZone = 1.5f;

    /* CAMERA FOLLOW SETTINGS
    */
    [Space(10)]
    [Header("Camera Follow Settings")]
    public float followSmoothTime = 0.12f; // Controls how quickly the camera moves to its target position

    /* DEBUG
     * showWarnings = warns you if player/opponent were not assigned
    */
    [Space(10)]
    [Header("Debug")]
    [Tooltip("Logs a warning if player or opponent references are missing.")]
    public bool showWarnings = true;

    // Internal references and smoothing helpers
    private Camera cam;
    private Vector3 moveVelocity = Vector3.zero;
    private float zoomVelocity = 0f;


 
    private void Awake()
    {
        cam = GetComponent<Camera>();

        // Checks if camera is using 2D orthographic projection
        if (cam != null )
        {
            cam.orthographic = true;
        }
    }

    private void LateUpdate()
    {
        // Stops if either target (Player or Opponent) is missing
        if (player == null || opponent == null)
        {
            if (showWarnings)
            {
                Debug.LogWarning("Camera is missing player and/or opponent references");
            }

            return;
        }

        MoveCamera();
        ZoomCamera(); 
    }

    private void MoveCamera()
    {
        // Find the center point between both fighters on the X axis.
        float midpointX = (player.position.x + opponent.position.x) * 0.5f;

        // Keep the camera inside the stage boundaries.
        float clampedX = Mathf.Clamp(midpointX, minX, maxX);

        // ADDED: Adjust the Y position based on zoom level to keep the fighters in view and add some dynamic movement
        float midpointY = (player.position.y + opponent.position.y) * 0.5f;
        float targetY = Mathf.Max(fixedY, midpointY + 0.5f);

        // Slightly lowers the camera when zoomed in
        float zoomOffsetY = Mathf.Lerp(-0.5f, 0f, Mathf.InverseLerp(minZoom, maxZoom, cam.orthographicSize));
        targetY += zoomOffsetY;

        // Builds final target position.
        Vector3 targetPosition = new Vector3(clampedX, targetY, -10f);

        // Smoothly move toward that position.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref moveVelocity,
            followSmoothTime
        );
    }

    private void ZoomCamera()
    {
        // Measure left/right separation
        float horizontalDistance = Mathf.Abs(player.position.x - opponent.position.x);

        // Measure up/down sepration
        float verticalDistance = Mathf.Abs(player.position.y - opponent.position.y);

        // Multiply vertical distance by 1.5 to account for the shorter verticle field of view
        float largestDistance = Mathf.Max(horizontalDistance, verticalDistance * 2f);

        // Apply dead zone so tiny changes do not constantly adjust zoom.
        float adjustedDistance = Mathf.Max(0f, largestDistance - zoomDeadZone);

        // Convert  distance into a 0 to 1 interpolation value
        float t = Mathf.Clamp01(adjustedDistance / zoomLimiter);

        // Calculate zoom between min & max.
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, t);

        // Smoothly change camera zoom.
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }
}
