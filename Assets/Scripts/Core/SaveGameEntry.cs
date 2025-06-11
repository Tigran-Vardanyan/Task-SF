using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a single save game entry in the load panel list.
/// Displays the save file name and handles user interactions for selecting or deleting the save.
/// </summary>
public class SaveGameEntry : MonoBehaviour
{
    // UI references to display filename and handle user actions
    public TMP_Text fileNameText;  // Text component showing the save file name
    public Button selectButton;    // Button to select this save for loading
    public Button deleteButton;    // Button to delete this save file

    private string _fileName; // Internal storage of the save file name
    public string FileName => _fileName; // Public getter for external access

    // Callbacks to notify when this entry is selected or deleted
    private System.Action<string> _selectCallback;
    private System.Action<string> _deleteCallback;

    /// <summary>
    /// Initializes the entry with the save file name and callback functions.
    /// Sets up button listeners.
    /// </summary>
    /// <param name="fileName">The save file name to display</param>
    /// <param name="onSelect">Callback when select button is clicked</param>
    /// <param name="onDelete">Callback when delete button is clicked</param>
    public void Initialize(string fileName, System.Action<string> onSelect, System.Action<string> onDelete)
    {
        _fileName = fileName;
        _selectCallback = onSelect;
        _deleteCallback = onDelete;

        if (fileNameText != null)
            fileNameText.text = fileName;

        // Remove existing listeners and add new ones for buttons
        selectButton?.onClick.RemoveAllListeners();
        selectButton?.onClick.AddListener(OnSelectClicked);

        deleteButton?.onClick.RemoveAllListeners();
        deleteButton?.onClick.AddListener(OnDeleteClicked);
    }

    // Called when the select button is clicked
    private void OnSelectClicked()
    {
        _selectCallback?.Invoke(_fileName);
    }

    // Called when the delete button is clicked
    private void OnDeleteClicked()
    {
        _deleteCallback?.Invoke(_fileName);
    }

    /// <summary>
    /// Visually marks this entry as selected or not.
    /// Changes the select button's normal color as feedback.
    /// </summary>
    /// <param name="isSelected">True if selected, false otherwise</param>
    public void SetSelected(bool isSelected)
    {
        if (selectButton != null)
        {
            ColorBlock colors = selectButton.colors;
            colors.normalColor = isSelected ? Color.cyan : Color.white; // Change color based on selection state
            selectButton.colors = colors;
        }
    }
}