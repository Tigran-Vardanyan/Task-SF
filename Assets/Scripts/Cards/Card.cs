using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Card : MonoBehaviour
{
    public int id;
    public bool isMatched = false;

    private Animator animator;
    private GameManager gameManager;
    private SpriteRenderer frontRenderer;
    private SpriteRenderer backRenderer;

    private bool isFlipping = false;
    public bool IsFullyFlipped => !isFlipping;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            if (r.name == "Front") frontRenderer = r;
            else if (r.name == "Back") backRenderer = r;
        }
    }

    public void Initialize(int cardId, Sprite front, Sprite back)
    {
        id = cardId;
        frontRenderer.sprite = front;
        backRenderer.sprite = back;
    }

    private void OnMouseDown()
    {
        if (isMatched || gameManager.IsCardLocked(this) || isFlipping) return;

        animator.SetTrigger("Flip");
        isFlipping = true;
        gameManager.OnCardFlipped(this);
    }

    public void FlipBack()
    {
        animator.SetTrigger("FlipBack");
        isFlipping = true;
    }

    public void FlipFront()
    {
        animator.SetTrigger("Flip");
        isFlipping = true;
    }

    // Called at the end of Flip and FlipBack animations via Animation Event
    public void OnFlipComplete()
    {
        isFlipping = false;
    }
}