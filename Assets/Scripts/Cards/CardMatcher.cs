using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMatcher : MonoBehaviour
{
    public event Action OnRequestInteractionLock;
    public event Action OnRequestInteractionUnlock;

    private List<Card> flippedCards = new();
    private int matchesFound = 0;
    private int totalPairs;
    private bool isChecking = false;

    private AudioSource audioSource;
    private AudioClip flipClip, matchClip, mismatchClip, gameOverClip;

    [SerializeField] private BoardFitter boardFitter;

    public void Initialize(Vector2Int gridSize, AudioSource audioSource, AudioClip flip, AudioClip match, AudioClip mismatch, AudioClip gameOver)
    {
        this.audioSource = audioSource;
        this.flipClip = flip;
        this.matchClip = match;
        this.mismatchClip = mismatch;
        this.gameOverClip = gameOver;
        totalPairs = (gridSize.x * gridSize.y) / 2;
    }

    public void HandleCardFlipped(Card card)
    {
        if (isChecking || flippedCards.Contains(card)) return;

        audioSource.PlayOneShot(flipClip);
        flippedCards.Add(card);

        // If second card is flipped, check match
        if (flippedCards.Count == 2)
        {
            OnRequestInteractionLock?.Invoke(); // lock interaction during match check
            StartCoroutine(CheckMatch());
        }
    }
    

    public IEnumerator ShowAllCardsTemporarily()
    {
        OnRequestInteractionLock?.Invoke();

        yield return new WaitForSeconds(0.2f);
        foreach (Transform child in boardFitter.GetBoardTransform())
        {
            Card card = child.GetComponent<Card>();
            card.FlipFront();
        }

        yield return new WaitForSeconds(1f);
        foreach (Transform child in boardFitter.GetBoardTransform())
        {
            Card card = child.GetComponent<Card>();
            if (!card.isMatched)
                card.FlipBack();
        }

        OnRequestInteractionUnlock?.Invoke();
    }
    private IEnumerator CheckMatch()
    {
        isChecking = true;

        // Wait until the second card has fully flipped
        yield return new WaitUntil(() => flippedCards[1].IsFullyFlipped);

        yield return new WaitForSeconds(0.25f); // brief delay to see both cards

        if (flippedCards[0].id == flippedCards[1].id)
        {
            audioSource.PlayOneShot(matchClip);
            foreach (var c in flippedCards) c.isMatched = true;
            matchesFound++;
            if (matchesFound >= totalPairs)
                audioSource.PlayOneShot(gameOverClip);
        }
        else
        {
            audioSource.PlayOneShot(mismatchClip);
            yield return new WaitForSeconds(0.5f);
            foreach (var c in flippedCards) c.FlipBack();
        }

        flippedCards.Clear();
        isChecking = false;
    }

    public bool IsCardLocked(Card card) => flippedCards.Contains(card) || isChecking;
}