using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages Save and Load UI panels, user interactions, and communicates user actions through events.
/// GameManager or other systems subscribe to events to perform actual saving/loading.
/// </summary>
public class SaveLoadUIManager : MonoBehaviour
{
    [Header("Save Panel UI")]
    [SerializeField] private GameObject savePanel;
    [SerializeField] private TMP_InputField saveFileNameInputField;
    [SerializeField] private Button confirmSaveButton;
    [SerializeField] private Button cancelSaveButton;

    [Header("Load Panel UI")]
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private Transform loadContentParent;
    [SerializeField] private GameObject saveGameEntryPrefab;
    [SerializeField] private Button loadSelectedButton;
    [SerializeField] private Button cancelLoadButton;
    [SerializeField] private TextMeshProUGUI noSaveFilesText;

    /// <summary> Event invoked when user confirms save with a filename </summary>
    public event Action<string> OnConfirmSave;

    /// <summary> Event invoked when user confirms load with a selected filename </summary>
    public event Action<string> OnConfirmLoad;

    private string selectedLoadFileName = string.Empty;
    private readonly List<SaveGameEntry> currentLoadEntries = new List<SaveGameEntry>();

    private void Awake()
    {
        // Initially hide both panels
        savePanel.SetActive(false);
        loadPanel.SetActive(false);

        // Setup UI button listeners
        confirmSaveButton.onClick.AddListener(OnConfirmSaveClicked);
        cancelSaveButton.onClick.AddListener(HideSavePanel);

        loadSelectedButton.onClick.AddListener(OnConfirmLoadClicked);
        cancelLoadButton.onClick.AddListener(HideLoadPanel);
    }

    #region Save Panel Methods

    /// <summary>
    /// Shows the Save Panel with a default file name and prepares input and button states.
    /// </summary>
    public void ShowSavePanel()
    {
        savePanel.SetActive(true);

        string defaultFileName = $"SaveGame_{DateTime.Now:yyyyMMdd_HHmmss}";
        saveFileNameInputField.text = defaultFileName;

        saveFileNameInputField.Select();
        saveFileNameInputField.ActivateInputField();

        confirmSaveButton.interactable = !string.IsNullOrWhiteSpace(defaultFileName);

        // Remove existing listeners to avoid duplicates
        saveFileNameInputField.onValueChanged.RemoveAllListeners();
        saveFileNameInputField.onValueChanged.AddListener(OnSaveFileNameChanged);
    }

    /// <summary>
    /// Hides the Save Panel and cleans up input listeners.
    /// </summary>
    public void HideSavePanel()
    {
        savePanel.SetActive(false);
        saveFileNameInputField.onValueChanged.RemoveAllListeners();
    }

    private void OnSaveFileNameChanged(string newText)
    {
        // Enable or disable confirm button depending on input validity
        confirmSaveButton.interactable = !string.IsNullOrWhiteSpace(newText);
    }

    private void OnConfirmSaveClicked()
    {
        string fileName = saveFileNameInputField.text.Trim();

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            OnConfirmSave?.Invoke(fileName);
            HideSavePanel();
        }
        else
        {
            Debug.LogWarning("Save file name cannot be empty!");
            // Optional: Show UI feedback here
        }
    }

    #endregion

    #region Load Panel Methods

    /// <summary>
    /// Shows the Load Panel and populates the save files list.
    /// </summary>
    public void ShowLoadPanel()
    {
        loadPanel.SetActive(true);
        PopulateLoadPanel();
        loadSelectedButton.interactable = false; // Disable until a save is selected
    }

    /// <summary>
    /// Hides the Load Panel and clears dynamically created entries.
    /// </summary>
    public void HideLoadPanel()
    {
        loadPanel.SetActive(false);
        ClearLoadEntries();
        selectedLoadFileName = string.Empty;
    }

    /// <summary>
    /// Clears the currently displayed save entries.
    /// </summary>
    private void ClearLoadEntries()
    {
        foreach (var entry in currentLoadEntries)
        {
            if (entry != null && entry.gameObject != null)
                Destroy(entry.gameObject);
        }
        currentLoadEntries.Clear();
    }

    /// <summary>
    /// Retrieves save file names and creates UI entries for them.
    /// </summary>
    private void PopulateLoadPanel()
    {
        ClearLoadEntries();
        selectedLoadFileName = string.Empty;
        loadSelectedButton.interactable = false;

        List<string> saveFileNames = GamePersistenceManager.GetAllSaveFileNames();

        if (saveFileNames.Count == 0)
        {
            noSaveFilesText?.gameObject.SetActive(true);
            return;
        }
        noSaveFilesText?.gameObject.SetActive(false);

        foreach (var fileName in saveFileNames)
        {
            GameObject entryObj = Instantiate(saveGameEntryPrefab, loadContentParent);
            SaveGameEntry entry = entryObj.GetComponent<SaveGameEntry>();
            if (entry == null)
            {
                Debug.LogError("SaveGameEntryPrefab missing SaveGameEntry script.");
                Destroy(entryObj);
                continue;
            }

            // Initialize entry with callbacks for selection and deletion
            entry.Initialize(fileName, OnLoadFileSelected, OnDeleteSaveFile);
            currentLoadEntries.Add(entry);
        }
    }

    /// <summary>
    /// Callback when a save file entry is selected.
    /// Updates selection state and enables load button.
    /// </summary>
    /// <param name="fileName">Selected save file name</param>
    private void OnLoadFileSelected(string fileName)
    {
        selectedLoadFileName = fileName;
        loadSelectedButton.interactable = true;

        // Highlight the selected entry, deselect others
        foreach (var entry in currentLoadEntries)
        {
            entry.SetSelected(entry.FileName == fileName);
        }

        Debug.Log($"Selected save file: {fileName}");
    }

    /// <summary>
    /// Callback to delete a save file and refresh the list.
    /// </summary>
    /// <param name="fileName">Name of the save file to delete</param>
    private void OnDeleteSaveFile(string fileName)
    {
        Debug.Log($"Deleting save file: {fileName}");

        // Optionally add confirmation dialog here

        GamePersistenceManager.DeleteSaveGame(fileName);
        PopulateLoadPanel(); // Refresh the list after deletion
    }

    private void OnConfirmLoadClicked()
    {
        if (!string.IsNullOrWhiteSpace(selectedLoadFileName))
        {
            OnConfirmLoad?.Invoke(selectedLoadFileName);
            HideLoadPanel();
        }
        else
        {
            Debug.LogWarning("No save file selected to load!");
            // Optional: Show UI feedback here
        }
    }

    #endregion
}