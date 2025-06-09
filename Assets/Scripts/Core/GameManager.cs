using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private List<Card> flippedCards = new();
    private int matchesFound = 0;
    private int totalPairs;
    private bool isChecking = false;

    public AudioClip flipClip, matchClip, mismatchClip, gameOverClip;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //LoadGame(); // Optional
    }

    public void OnCardFlipped(Card card)
    {
        if (flippedCards.Contains(card)) return;
        audioSource.PlayOneShot(flipClip);
        flippedCards.Add(card);

        if (flippedCards.Count == 2 && !isChecking)
        {
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        isChecking = true;
        yield return new WaitForSeconds(0.5f);

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
            foreach (var c in flippedCards) c.FlipBack();
        }

        flippedCards.Clear();
        isChecking = false;
    }

    public bool IsCardLocked(Card card) => flippedCards.Contains(card) || isChecking;
}
