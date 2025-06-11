using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class Card : MonoBehaviour
{
    public int id;
    public bool isMatched = false;

    // True if card is currently face-up and animation is completed
    public bool IsFullyFlipped { get; private set; }

    private Animator animator;
    private GameManager gameManager;
    private SpriteRenderer frontRenderer;
    private SpriteRenderer backRenderer;

    // Events fired on flip animation completion
    public event System.Action<Card> OnFlippedToFace;
    public event System.Action<Card> OnFlippedToBack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError($"Card: Animator not found on {gameObject.name}!");
        else
            Debug.Log($"Card: Animator assigned on {gameObject.name}.");

        // Find GameManager in scene (consider dependency injection if preferred)
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
            Debug.LogError("Card: GameManager not found in scene!");

        // Find child SpriteRenderers named "Front" and "Back"
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            if (r.name == "Front") frontRenderer = r;
            else if (r.name == "Back") backRenderer = r;
        }

        if (frontRenderer == null)
            Debug.LogError("Card: Front SpriteRenderer not found! Ensure child named 'Front' with SpriteRenderer.");
        if (backRenderer == null)
            Debug.LogError("Card: Back SpriteRenderer not found! Ensure child named 'Back' with SpriteRenderer.");
    }

    /// <summary>
    /// Initialize card with ID and sprites for front/back sides.
    /// Starts flipped face-down immediately.
    /// </summary>
    public void Initialize(int cardId, Sprite front, Sprite back)
    {
        id = cardId;

        if (frontRenderer != null) frontRenderer.sprite = front;
        if (backRenderer != null) backRenderer.sprite = back;

        // Flip back after initialization (delayed one frame to ensure animator is ready)
        StartCoroutine(DelayedFlipBackImmediate());
    }

    private IEnumerator DelayedFlipBackImmediate()
    {
        yield return null; // Wait one frame
        FlipBackImmediate();
    }

    /// <summary>
    /// Update the card's ID and front sprite.
    /// Useful for reusing card objects.
    /// </summary>
    public void SetCardType(int newTypeId, Sprite newFrontSprite)
    {
        Debug.Log(
            $"Card {gameObject.name} at sibling index {transform.GetSiblingIndex()}: Changing type from {id} to {newTypeId}");
        id = newTypeId;
        if (frontRenderer != null)
        {
            frontRenderer.sprite = newFrontSprite;
            Debug.Log($"   Updated front sprite to: {newFrontSprite.name}");
        }
        else
        {
            Debug.LogWarning($"Card {gameObject.name}: Front SpriteRenderer is missing, cannot update sprite.");
        }
    }

    private void OnMouseDown()
    {
        // Ignore clicks on matched or locked cards
        if (isMatched || gameManager.IsCardLocked(this))
        {
            Debug.Log("Card click ignored due to matched or locked state.");
            return;
        }

        Debug.Log($"Card clicked: ID {id}");

        if (animator != null)
        {
            animator.ResetTrigger("Flip");
            animator.ResetTrigger("FlipBack");
            animator.SetTrigger("Flip"); // Play flip to face-up animation
        }
        else
        {
            Debug.LogWarning($"Animator missing on {gameObject.name} during OnMouseDown!");
        }

        gameManager.OnCardFlipped(this);
    }

    private void OnMouseOver()
    {
        // Optional: Add hover effects here
    }

    /// <summary>
    /// Animate flipping card back to face-down.
    /// </summary>
    public void FlipBack()
    {
        if (animator != null)
        {
            animator.SetTrigger("FlipBack"); // Play flip to face-down animation
        }
        else
        {
            Debug.LogWarning($"Animator missing on {gameObject.name} in FlipBack()!");
        }
    }

    /// <summary>
    /// Animate flipping card face-up.
    /// </summary>
    public void FlipFront()
    {
        if (animator != null)
        {
            animator.SetTrigger("Flip"); // Play flip to face-up animation
        }
        else
        {
            Debug.LogWarning($"Animator missing on {gameObject.name} in FlipFront()!");
        }
    }

    /// <summary>
    /// Immediately show card face-up without animation.
    /// </summary>
    public void FlipFrontImmediate()
    {
        if (animator != null)
        {
            animator.Play("Card_FaceUp_Idle");
        }
        else
        {
            Debug.LogWarning($"Animator missing on {gameObject.name} in FlipFrontImmediate()!");
        }

        IsFullyFlipped = true;
    }

    /// <summary>
    /// Immediately show card face-down without animation.
    /// </summary>
    public void FlipBackImmediate()
    {
        if (animator != null)
        {
            animator.Play("Card_FaceDown_Idle");
        }
        else
        {
            Debug.LogWarning($"Animator missing on {gameObject.name} in FlipBackImmediate()!");
        }

        IsFullyFlipped = false;
    }

    /// <summary>
    /// Callback from animation event at the end of flip-to-face-up animation.
    /// </summary>
    public void OnFlipFrontAnimationComplete()
    {
        IsFullyFlipped = true;
        Debug.Log($"Flip to front completed for card ID {id}");
        OnFlippedToFace?.Invoke(this);
    }

    /// <summary>
    /// Callback from animation event at the end of flip-to-face-down animation.
    /// </summary>
    public void OnFlipBackAnimationComplete()
    {
        IsFullyFlipped = false;
        Debug.Log($"Flip to back completed for card ID {id}");
        OnFlippedToBack?.Invoke(this);
    }

    /// <summary>
    /// Sets the matched state and updates visual accordingly.
    /// </summary>
    public void SetMatched(bool matched)
    {
        isMatched = matched;

        if (isMatched)
        {
            FlipFrontImmediate(); // Show face-up instantly for matched cards
        }
        else
        {
            FlipBackImmediate(); // Show face-down instantly for unmatched cards
        }
    }

    public void FlipMatchedInstant()
    {
        animator.SetTrigger("Flip");
        isMatched = true;
    }
}