using UnityEngine;

public class BoardFitter : MonoBehaviour
{
    [SerializeField] private Transform boardTransform;   // Transform of the board to fit
    [SerializeField] private BoxCollider boardCollider; // Collider used to define the board's bounds

    /// <summary>
    /// Fits the board within the camera's viewport area defined by viewport coordinates (0.3, 0.15) to (0.7, 0.95).
    /// Adjusts the board position and BoxCollider size accordingly.
    /// </summary>
    public void FitToCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        // Calculate distance from camera to board on Y axis
        float distance = Mathf.Abs(boardTransform.position.y - cam.transform.position.y);

        // Convert viewport corners to world points at the board's distance
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0.3f, 0.15f, distance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(0.7f, 0.95f, distance));

        // Calculate the size based on viewport corners
        Vector3 size = topRight - bottomLeft;
        size.y = 0.1f; // Maintain a small height for the collider (thin board)

        // Apply size and center to the board collider
        boardCollider.size = size;
        boardCollider.center = Vector3.zero; // Collider centered relative to boardTransform

        // Position the board in the middle of the viewport area
        boardTransform.position = (bottomLeft + topRight) / 2f;
    }

    /// <summary>
    /// Returns the world position of the bottom-left corner of the board (origin).
    /// </summary>
    public Vector3 GetBoardOrigin()
    {
        Vector3 boardCenter = boardTransform.position + boardCollider.center;
        Vector3 boardSize = boardCollider.size;

        // Calculate bottom-left corner from center and size
        return boardCenter - new Vector3(boardSize.x / 2f, 0f, boardSize.z / 2f);
    }

    /// <summary>
    /// Returns the current size of the board collider.
    /// </summary>
    public Vector3 GetBoardSize() => boardCollider.size;

    /// <summary>
    /// Returns the Transform component of the board.
    /// </summary>
    public Transform GetBoardTransform() => boardTransform;

    /// <summary>
    /// Draws visual gizmos in the Unity Editor to visualize the board collider bounds.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (boardCollider == null || boardTransform == null)
            return;

        // Set semi-transparent cyan color for the filled cube
        Gizmos.color = new Color(0, 1, 1, 0.5f);

        // Save current Gizmos matrix and apply board transform matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = boardTransform.localToWorldMatrix;

        // Draw solid cube for the board collider
        Gizmos.DrawCube(boardCollider.center, boardCollider.size);

        // Draw wireframe cube with opaque cyan for edges
        Gizmos.color = new Color(0, 1, 1, 1f);
        Gizmos.DrawWireCube(boardCollider.center, boardCollider.size);

        // Restore original Gizmos matrix
        Gizmos.matrix = oldMatrix;
    }
}