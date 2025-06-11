using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMatcher : MonoBehaviour
{
    // Events for interaction locking, matches, and turn completion
    public event Action OnRequestInteractionLock;
    public event Action OnRequestInteractionUnlock;
    public event Action<int> OnMatchFound;      // Invoked with number of matches found
    public event Action<int> OnTurnCompleted;   // Invoked with number of turns taken

    private List<Card> currentlyFlippedCards = new();
    private HashSet<Card> lockedCardsForInteraction = new();

    private int matchesFound = 0;
    private int turnsTaken = 0;
    private int totalPairs;
    private GameManager _gameManager;

    private AudioSource audioSource;
    private AudioClip flipClip, matchClip, mismatchClip, gameOverClip;

    [SerializeField] private BoardFitter boardFitter;

    /// <summary>
    /// Initialize matcher with board size and audio clips.
    /// </summary>
    public void Initialize(Vector2Int gridSize, AudioSource audioSource, AudioClip flip, AudioClip match, AudioClip mismatch, AudioClip gameOver,GameManager gameManager)
    {
        this.audioSource = audioSource;
        _gameManager = gameManager;
        this.flipClip = flip;
        this.matchClip = match;
        this.mismatchClip = mismatch;
        this.gameOverClip = gameOver;
        totalPairs = (gridSize.x * gridSize.y) / 2;
    }

    /// <summary>
    /// Called when a card is flipped by the player.
    /// </summary>
    public void HandleCardFlipped(Card card)
    {
        // Ignore clicks on already matched or locked cards
        if (card.isMatched || lockedCardsForInteraction.Contains(card))
        {
            Debug.Log($"Ignored click for card {card.id}. Already matched or locked.");
            return;
        }

        // Play flip sound and lock card to prevent double-clicking
        audioSource.PlayOneShot(flipClip);
        lockedCardsForInteraction.Add(card);

        card.OnFlippedToFace += OnCardFlippedFaceUpComplete;
        currentlyFlippedCards.Add(card);

        // If two cards flipped, process matching
        if (currentlyFlippedCards.Count == 2)
        {
            Card card1 = currentlyFlippedCards[0];
            Card card2 = currentlyFlippedCards[1];

            currentlyFlippedCards.Clear();

            turnsTaken++;
            OnTurnCompleted?.Invoke(turnsTaken);

            StartCoroutine(ProcessCardPair(card1, card2));
        }
    }

    /// <summary>
    /// Callback when card flip animation to face up completes.
    /// Unsubscribes event to avoid leaks.
    /// </summary>
    private void OnCardFlippedFaceUpComplete(Card card)
    {
        card.OnFlippedToFace -= OnCardFlippedFaceUpComplete;
    }

    /// <summary>
    /// Coroutine that processes two flipped cards: checks for match or mismatch.
    /// </summary>
    private IEnumerator ProcessCardPair(Card card1, Card card2)
    {
        Debug.Log($"Processing pair: Card {card1.id} and Card {card2.id}");

        // Wait until both cards have fully flipped face up
        yield return new WaitUntil(() => card1.IsFullyFlipped && card2.IsFullyFlipped);

        // Small delay before checking
        yield return new WaitForSeconds(0.7f);

        if (card1.id == card2.id)
        {
            // Cards matched
            audioSource.PlayOneShot(matchClip);

            card1.isMatched = true;
            card2.isMatched = true;

            matchesFound++;
            OnMatchFound?.Invoke(matchesFound);

            if (matchesFound >= totalPairs)
            {
                // Game over condition
                audioSource.PlayOneShot(gameOverClip);
                OnRequestInteractionLock?.Invoke();
                _gameManager.ToggleHUD();
            }
        }
        else
        {
            // Cards do not match, play mismatch sound and flip back after delay
            audioSource.PlayOneShot(mismatchClip);
            yield return new WaitForSeconds(0.7f);

            card1.FlipBack();
            card2.FlipBack();

            // Wait until cards are fully flipped back down
            yield return new WaitUntil(() => !card1.IsFullyFlipped && !card2.IsFullyFlipped);
        }

        // Unlock cards so they can be interacted with again
        lockedCardsForInteraction.Remove(card1);
        lockedCardsForInteraction.Remove(card2);

        Debug.Log($"Finished processing pair: Card {card1.id} and Card {card2.id}. Cards unlocked.");
    }

    /// <summary>
    /// Coroutine to temporarily reveal all cards for a short time (e.g., preview phase).
    /// Locks all card interactions during the preview.
    /// </summary>
    public IEnumerator ShowAllCardsTemporarily()
    {
        OnRequestInteractionLock?.Invoke();

        List<Card> allCards = new List<Card>();
        foreach (Transform child in boardFitter.GetBoardTransform())
        {
            Card card = child.GetComponent<Card>();
            if (card != null)
            {
                allCards.Add(card);
                lockedCardsForInteraction.Add(card);
                card.FlipFront();
            }
        }

        yield return new WaitForSeconds(1.5f);

        foreach (Card card in allCards)
        {
            if (!card.isMatched)
            {
                card.FlipBack();
            }
        }

        yield return new WaitForSeconds(1.0f);

        foreach (Card card in allCards)
        {
            if (!card.isMatched)
            {
                lockedCardsForInteraction.Remove(card);
            }
        }

        OnRequestInteractionUnlock?.Invoke();
    }

    /// <summary>
    /// Checks if a card is currently locked for interaction.
    /// </summary>
    public bool IsCardLocked(Card card)
    {
        return lockedCardsForInteraction.Contains(card);
    }
}