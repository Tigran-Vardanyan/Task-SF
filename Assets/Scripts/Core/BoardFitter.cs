using UnityEngine;

public class BoardFitter : MonoBehaviour
{
    [SerializeField] private Transform boardTransform;
    [SerializeField] private BoxCollider boardCollider;

    public void FitToCamera()
    {
        Camera cam = Camera.main;
        if (!cam) return;

        float distance = Mathf.Abs(boardTransform.position.y - cam.transform.position.y);
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0.15f, distance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 0.95f, distance));

        Vector3 size = topRight - bottomLeft;
        size.y = 0.1f;
        boardCollider.size = size;
        boardCollider.center = Vector3.zero;
        boardTransform.position = (bottomLeft + topRight) / 2f;
    }

    public Vector3 GetBoardOrigin() => boardTransform.position - boardCollider.size / 2f;

    public Vector3 GetBoardSize() => boardCollider.size;

    public Transform GetBoardTransform() => boardTransform;
}