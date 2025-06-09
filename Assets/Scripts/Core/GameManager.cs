using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public BoardFitter boardFitter;
    public CardSpawner cardSpawner;
    public CardMatcher cardMatcher;

    [Header("Settings")]
    public Vector2Int gridSize = new(4, 3);
    public float horizontalSpacing = 0.01f;
    public float verticalSpacing = 0.05f;
    public GameObject cardPrefab;
    public CardData cardData;

    [Header("Audio")]
    public AudioClip flipClip, matchClip, mismatchClip, gameOverClip;
    public AudioSource audioSource;

    private bool interactionLocked = true;

    private void Start()
    {
        boardFitter.FitToCamera();

        cardSpawner.Initialize(gridSize, horizontalSpacing, verticalSpacing, cardPrefab, cardData);
        cardSpawner.SpawnCards();

        cardMatcher.Initialize(gridSize, audioSource, flipClip, matchClip, mismatchClip, gameOverClip);
        cardMatcher.OnRequestInteractionLock += LockInteraction;
        cardMatcher.OnRequestInteractionUnlock += UnlockInteraction;

        StartCoroutine(ShowCardsAndUnlockInteraction());
    }

    private System.Collections.IEnumerator ShowCardsAndUnlockInteraction()
    {
        yield return StartCoroutine(cardMatcher.ShowAllCardsTemporarily());
        interactionLocked = false;
    }

    private void LockInteraction() => interactionLocked = true;
    private void UnlockInteraction() => interactionLocked = false;

    public void OnCardFlipped(Card card)
    {
        if (interactionLocked) return;
        cardMatcher.HandleCardFlipped(card);
    }

    public bool IsCardLocked(Card card) => interactionLocked || cardMatcher.IsCardLocked(card);
}