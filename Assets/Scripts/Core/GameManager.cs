using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq; // For OrderBy
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("References")] public BoardFitter boardFitter;
    public CardSpawner cardSpawner;
    public CardMatcher cardMatcher;
    public Animator animatorHUD;
    public SaveLoadUIManager uiManager;

    [Header("Settings")] public Vector2Int gridSize = new(4, 3);
    public float horizontalSpacing = 0.01f;
    public float verticalSpacing = 0.05f;
    public GameObject cardPrefab;
    public CardData cardData;
    public bool fitToCamera = true;

    [Header("Audio")] public AudioClip flipClip, matchClip, mismatchClip, gameOverClip;
    public AudioSource audioSource;

    [Header("Scoring")] public TextMeshProUGUI matchesText;
    public TextMeshProUGUI turnsText;

    [Header("Buttons")] public Button restartButton;
    public Button loadGameButton;
    public Button saveGameButton;
    public Button backToMenuButton;
    public Button arrowHudButton;

    private bool globalInteractionLock = true;
    private int currentMatches = 0;
    private int currentTurns = 0;
    private bool HUDisVisible = false;

    private void Awake()
    {
        AddButtonListeners();

        if (uiManager != null)
        {
            uiManager.OnConfirmSave += SaveCurrentGame;
            uiManager.OnConfirmLoad += ExecuteLoadGame;
        }
        else
        {
            Debug.LogError("SaveLoadUIManager reference is missing on GameManager! Please assign it in the Inspector.");
        }
    }

    private void OnDestroy()
    {
        if (uiManager != null)
        {
            uiManager.OnConfirmSave -= SaveCurrentGame;
            uiManager.OnConfirmLoad -= ExecuteLoadGame;
        }
    }

    // Setup all UI button click listeners here
    private void AddButtonListeners()
    {
        restartButton?.onClick.AddListener(RestartGame);
        loadGameButton?.onClick.AddListener(uiManager.ShowLoadPanel);
        saveGameButton?.onClick.AddListener(uiManager.ShowSavePanel);
        backToMenuButton?.onClick.AddListener(BackToMainMenu);
        arrowHudButton?.onClick.AddListener(ToggleHUD);
    }

    private void Start()
    {
        if (fitToCamera)
            boardFitter.FitToCamera();

        int savedCols = PlayerPrefs.GetInt("GridColumns", gridSize.x);
        int savedRows = PlayerPrefs.GetInt("GridRows", gridSize.y);
        gridSize = new Vector2Int(savedCols, savedRows);

        cardSpawner.Initialize(gridSize, horizontalSpacing, verticalSpacing, cardPrefab, cardData);

        cardMatcher.Initialize(gridSize, audioSource, flipClip, matchClip, mismatchClip, gameOverClip, this);
        cardMatcher.OnRequestInteractionLock += LockGlobalInteraction;
        cardMatcher.OnRequestInteractionUnlock += UnlockGlobalInteraction;
        cardMatcher.OnMatchFound += UpdateMatchesDisplay;
        cardMatcher.OnTurnCompleted += UpdateTurnsDisplay;

        UpdateMatchesDisplay(0);
        UpdateTurnsDisplay(0);

        bool shouldLoadGame = PlayerPrefs.GetInt("LoadGameOnStart", 0) == 1;
        string fileNameToLoad = PlayerPrefs.GetString("LoadFileName", "");

        if (shouldLoadGame && !string.IsNullOrEmpty(fileNameToLoad))
        {
            Debug.Log($"GameManager: Starting with load game flag for {fileNameToLoad}");
            PlayerPrefs.DeleteKey("LoadGameOnStart");
            PlayerPrefs.DeleteKey("LoadFileName");
            PlayerPrefs.Save();
            ExecuteLoadGame(fileNameToLoad);
        }
        else
        {
            Debug.Log("GameManager: Starting a new game.");
            cardSpawner.SpawnCards();
            StartCoroutine(ShowCardsAndUnlockInteraction());
        }
    }

    // Show all cards briefly then unlock interaction
    private System.Collections.IEnumerator ShowCardsAndUnlockInteraction()
    {
        yield return StartCoroutine(cardMatcher.ShowAllCardsTemporarily());
        globalInteractionLock = false;
    }

    private void LockGlobalInteraction()
    {
        globalInteractionLock = true;

        Debug.Log("Global Interaction Locked (e.g., during game over or intro)");
    }

    private void UnlockGlobalInteraction()
    {
        globalInteractionLock = false;
        Debug.Log("Global Interaction Unlocked");
    }

    // Called from Card when flipped; forward to CardMatcher if allowed
    public void OnCardFlipped(Card card)
    {
        if (globalInteractionLock)
        {
            Debug.Log($"GameManager: Cannot flip card {card.id} due to global lock.");
            return;
        }

        cardMatcher.HandleCardFlipped(card);
    }

    // Query if a card is currently locked for interaction
    public bool IsCardLocked(Card card)
    {
        if (globalInteractionLock) return true;
        return cardMatcher.IsCardLocked(card);
    }

    // Update the displayed number of matches found
    private void UpdateMatchesDisplay(int matches)
    {
        currentMatches = matches;
        Debug.Log($"Matches: {currentMatches}");
        if (matchesText != null)
            matchesText.text = $"Matches: {currentMatches}";
    }

    // Update the displayed number of turns taken
    private void UpdateTurnsDisplay(int turns)
    {
        currentTurns = turns;
        Debug.Log($"Turns: {currentTurns}");
        if (turnsText != null)
            turnsText.text = $"Turns: {currentTurns}";
    }

    // Reload the current scene to restart the game
    public void RestartGame()
    {
        Debug.Log("Restarting Game");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Save the current game state to a file

    public void SaveCurrentGame(string saveFileName)
    {
        GamePersistenceManager.GameState gameState = new GamePersistenceManager.GameState();
        gameState.gridSizeX = gridSize.x;
        gameState.gridSizeY = gridSize.y;
        gameState.turnsTaken = currentTurns;
        gameState.matchesFound = currentMatches;

        gameState.cardStates = new List<GamePersistenceManager.CardState>();

        Transform boardTransform = boardFitter.GetBoardTransform();

        for (int i = 0; i < boardTransform.childCount; i++)
        {
            Card card = boardTransform.GetChild(i).GetComponent<Card>();
            gameState.cardStates.Add(new GamePersistenceManager.CardState(i, card.id, card.isMatched));
        }

        GamePersistenceManager.SaveGame(gameState, saveFileName);
    }

    // Load game state from a file and apply it
    private void ExecuteLoadGame(string fileName)
    {
        Debug.Log($"GameManager: Executing in-game Load from: {fileName}");
        GamePersistenceManager.GameState gs = GamePersistenceManager.LoadGame(fileName);

        if (gs == null)
        {
            Debug.LogWarning($"GameManager: Load failed for '{fileName}' or file not found. Starting a new game.");
            if (boardFitter.GetBoardTransform().childCount == 0)
            {
                cardSpawner.SpawnCards();
                StartCoroutine(ShowCardsAndUnlockInteraction());
            }

            return;
        }

        // Clear existing cards on board
        foreach (Transform child in boardFitter.GetBoardTransform())
            Destroy(child.gameObject);

        // Prepare saved card IDs ordered by board index for correct spawning
        List<int> savedTypeIDs = gs.cardStates.OrderBy(s => s.boardIndex)
            .Select(s => s.typeId)
            .ToList();
        Debug.Log($"GameManager: Prepared {savedTypeIDs.Count} type IDs for spawning.");

        // Update grid size and spawn cards using saved data
        gridSize = new Vector2Int(gs.gridSizeX, gs.gridSizeY);
        cardSpawner.Initialize(gridSize, horizontalSpacing, verticalSpacing, cardPrefab, cardData);
        var loadedGame = GamePersistenceManager.LoadGame(fileName);
        cardSpawner.SpawnCards(loadedGame.cardStates);

        // Map spawned cards by their board index for state application
        Dictionary<int, Card> spawnedCardsByIndex = new();
        Transform boardTransform = boardFitter.GetBoardTransform();

        for (int i = 0; i < boardTransform.childCount; i++)
        {
            Card card = boardTransform.GetChild(i).GetComponent<Card>();
            if (card != null)
                spawnedCardsByIndex.Add(i, card);
        }

        Debug.Log($"GameManager: Retrieved {spawnedCardsByIndex.Count} newly spawned cards.");

        // Apply matched state to spawned cards based on saved data
        foreach (var savedCardState in gs.cardStates.OrderBy(s => s.boardIndex))
        {
            if (spawnedCardsByIndex.TryGetValue(savedCardState.boardIndex, out Card card))
                card.SetMatched(savedCardState.isMatched);
            else
                Debug.LogWarning(
                    $"Card at board index {savedCardState.boardIndex} not found after spawning. Possible data mismatch.");
        }

        currentMatches = gs.matchesFound;
        currentTurns = gs.turnsTaken;
        UpdateMatchesDisplay(currentMatches);
        UpdateTurnsDisplay(currentTurns);

        cardMatcher.Initialize(gridSize, audioSource, flipClip, matchClip, mismatchClip, gameOverClip, this);

        globalInteractionLock = false;
    }

    // Toggle HUD visibility animation and state
    public void ToggleHUD()
    {
        if (HUDisVisible)
        {
            animatorHUD.SetTrigger("CloseHUD");
            HUDisVisible = false;
        }
        else
        {
            animatorHUD.SetTrigger("OpenHUD");
            HUDisVisible = true;
        }
    }

    public void BackToMainMenu()
    {
        Debug.Log("Returning to Main Menu");
        SceneManager.LoadScene("MainMenu");
    }
}