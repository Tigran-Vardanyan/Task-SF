using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int id;
    public bool isMatched = false;

    private Animator animator;
    private GameManager gameManager;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OnClick()
    {
        if (isMatched || gameManager.IsCardLocked(this)) return;

        animator.SetTrigger("Flip");
        gameManager.OnCardFlipped(this);
    }

    public void FlipBack()
    {
        animator.SetTrigger("FlipBack");
    }
}
