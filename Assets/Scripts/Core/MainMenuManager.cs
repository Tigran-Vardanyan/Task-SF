using System.Collections.Generic; // For List<T>
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // For TextMeshPro UI elements

public class MainMenuManager : MonoBehaviour
{
    // === UI References ===
    [Header("UI References")]
    public Button startGame;       // Button to start a new game
    public Button loadGame;        // Button to open load game panel
    public Button quitGame;        // Button to quit the game

    // === Load Panel UI Elements ===
    [Header("Main Menu Load Panel")]
    public GameObject loadGamePanel;         // Load panel GameObject (initially hidden)
    public Transform loadContentParent;      // Parent transform (Content area of ScrollView) for save entries
    public GameObject saveGameEntryPrefab;   // Prefab for a single save entry UI element
    public Button loadSelectedButton;        // Button to confirm loading selected save
    public Button cancelLoadButton;          // Button to cancel/hide load panel
    public TextMeshProUGUI noSaveFilesText;  // Text shown when no saves exist
    
    [Header("Board Size Panel")]
    public GameObject boardSizePanel;       
    public Transform boardSizeButtonParent; 
    public GameObject boardSizeButtonPrefab;

    // === Internal state ===
    private string selectedLoadFileName = "";              // Currently selected save file name
    private List<SaveGameEntry> currentLoadEntries = new List<SaveGameEntry>(); // Active save entry UI instances

    // Called once when the script instance is being loaded
    private void Awake()
    {
        // Setup button listeners
        startGame?.onClick.AddListener(NewGame);
        loadGame?.onClick.AddListener(ShowLoadPanel);
        quitGame?.onClick.AddListener(QuitGame);

        loadSelectedButton?.onClick.AddListener(HandleConfirmLoadFromMenu);
        cancelLoadButton?.onClick.AddListener(HideLoadPanel);

        // Ensure the load panel is hidden initially
        loadGamePanel?.SetActive(false);
        boardSizePanel?.SetActive(false);
    }

    // Called on first frame update
    private void Start()
    {
        // Enable or disable the Load Game button depending on save availability
        UpdateLoadButtonState();
    }

    // Enables or disables the Load Game button based on presence of save files
    private void UpdateLoadButtonState()
    {
        loadGame.interactable = GamePersistenceManager.GetAllSaveFileNames().Count > 0;
    }

    // Starts a new game by clearing any load flags and loading the main game scene
    public void NewGame()
    {
        Debug.Log("Starting New Game...");

        boardSizePanel?.SetActive(true);
        GenerateBoardSizeButtons();
    }
    private void GenerateBoardSizeButtons()
    {
       
        foreach (Transform child in boardSizeButtonParent)
        {
            Destroy(child.gameObject);
        }

       
        for (int cols = 2; cols <= 5; cols++)
        {
            for (int rows = 2; rows <= 6; rows++)
            {
                int totalCards = cols * rows;
                if (totalCards % 2 != 0) continue; 

                GameObject buttonObj = Instantiate(boardSizeButtonPrefab, boardSizeButtonParent);
                Button btn = buttonObj.GetComponent<Button>();
                TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null)
                    label.text = $"{cols} x {rows}";

                Vector2Int size = new Vector2Int(cols, rows);

                btn.onClick.AddListener(() => OnBoardSizeSelected(size));
            }
        }
    }
    private void OnBoardSizeSelected(Vector2Int size)
    {
        Debug.Log($"Selected board size: {size.x} x {size.y}");

        PlayerPrefs.SetInt("GridColumns", size.x);
        PlayerPrefs.SetInt("GridRows", size.y);
        PlayerPrefs.DeleteKey("LoadGameOnStart");
        PlayerPrefs.DeleteKey("LoadFileName");
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameScene");
    }

    // Shows the load game panel and populates it with save entries
    public void ShowLoadPanel()
    {
        loadGamePanel?.SetActive(true);
        PopulateLoadPanel();
        loadSelectedButton.interactable = false; // Disable load button until user selects a save
    }

    // Hides the load game panel and cleans up dynamically created UI entries
    public void HideLoadPanel()
    {
        loadGamePanel?.SetActive(false);

        // Destroy all current save entry UI elements
        foreach (var entry in currentLoadEntries)
        {
            if (entry != null && entry.gameObject != null)
                Destroy(entry.gameObject);
        }
        currentLoadEntries.Clear();

        selectedLoadFileName = ""; // Clear any previous selection

        UpdateLoadButtonState(); // Refresh Load Game button state
    }

    // Populates the load panel's scroll view with all available save files
    private void PopulateLoadPanel()
    {
        // First, clear any existing entries (defensive cleanup)
        foreach (var entry in currentLoadEntries)
        {
            if (entry != null && entry.gameObject != null)
                Destroy(entry.gameObject);
        }
        currentLoadEntries.Clear();
        selectedLoadFileName = "";
        loadSelectedButton.interactable = false;

        // Get all saved game filenames from persistence manager
        List<string> saveFileNames = GamePersistenceManager.GetAllSaveFileNames();

        if (saveFileNames.Count == 0)
        {
            // Show 'No saves found' message if there are no save files
            noSaveFilesText?.gameObject.SetActive(true);
            return;
        }
        else
        {
            noSaveFilesText?.gameObject.SetActive(false);
        }

        // Create UI entry for each save file
        foreach (string fileName in saveFileNames)
        {
            GameObject entryObj = Instantiate(saveGameEntryPrefab, loadContentParent);
            SaveGameEntry entry = entryObj.GetComponent<SaveGameEntry>();

            if (entry != null)
            {
                // Initialize entry with filename and callbacks for selection & deletion
                entry.Initialize(fileName, SelectLoadFile, DeleteSaveFile);
                currentLoadEntries.Add(entry);
            }
            else
            {
                Debug.LogError("SaveGameEntryPrefab is missing the 'SaveGameEntry' script!");
                Destroy(entryObj);
            }
        }

        UpdateLoadButtonState(); // Update button state again (in case saves changed)
    }

    /// <summary>
    /// Deletes a save file and refreshes the load panel UI.
    /// </summary>
    /// <param name="fileName">Save file name to delete</param>
    private void DeleteSaveFile(string fileName)
    {
        Debug.Log($"Attempting to delete save file from Main Menu: {fileName}");
        GamePersistenceManager.DeleteSaveGame(fileName);

        // Refresh the UI after deletion
        PopulateLoadPanel();
        UpdateLoadButtonState();
    }

    // Called when a user selects a save file entry
    private void SelectLoadFile(string fileName)
    {
        selectedLoadFileName = fileName;
        loadSelectedButton.interactable = true;

        // Provide visual feedback by highlighting the selected save entry
        foreach (var entry in currentLoadEntries)
        {
            entry.SetSelected(entry.FileName == fileName);
        }
        Debug.Log($"Selected save file: {fileName}");
    }

    // Called when user confirms loading the selected save game
    private void HandleConfirmLoadFromMenu()
    {
        if (!string.IsNullOrWhiteSpace(selectedLoadFileName))
        {
            // Save to PlayerPrefs which file to load at game start
            PlayerPrefs.SetInt("LoadGameOnStart", 1);
            PlayerPrefs.SetString("LoadFileName", selectedLoadFileName);
            PlayerPrefs.Save();

            // Load the main game scene which will read PlayerPrefs to load the saved game
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogWarning("No save file selected!");
        }
    }

    // Quits the application or stops play mode in the Unity Editor
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in Editor
#else
        Application.Quit(); // Quit standalone build
#endif
    }
}