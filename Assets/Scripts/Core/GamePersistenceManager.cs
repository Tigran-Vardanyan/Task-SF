using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

/// <summary>
/// Handles all game save/load/delete operations using JSON serialization.
/// Saves are stored in Application.persistentDataPath/SaveGames with .json extension.
/// </summary>
public static class GamePersistenceManager
{
    // Directory path where save files are stored
    private static string saveDirectoryPath;
    private const string SAVE_FILE_EXTENSION = ".json";

    // Static constructor to initialize the save directory path and create folder if missing
    static GamePersistenceManager()
    {
        saveDirectoryPath = Path.Combine(Application.persistentDataPath, "SaveGames");
        Debug.Log($"Game Save Directory: {saveDirectoryPath}");

        if (!Directory.Exists(saveDirectoryPath))
        {
            Directory.CreateDirectory(saveDirectoryPath);
        }
    }

    /// <summary>
    /// Builds a full valid file path from a given save file name.
    /// Sanitizes filename to remove invalid characters.
    /// </summary>
    /// <param name="fileName">Save file name without extension</param>
    /// <returns>Full path to the save file</returns>
    private static string GetFullSavePath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("Save filename cannot be empty or null.");
            return null;
        }

        // Replace invalid filename characters with underscores
        string sanitizedFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(saveDirectoryPath, sanitizedFileName + SAVE_FILE_EXTENSION);
    }

    /// <summary>
    /// Saves the current game state to a JSON file.
    /// </summary>
    /// <param name="gameState">GameState object containing all data to save</param>
    /// <param name="fileName">Name of the save file (without extension)</param>
    public static void SaveGame(GameState gameState, string fileName)
    {
        string fullPath = GetFullSavePath(fileName);
        if (fullPath == null) return;

        try
        {
            // Convert game state to pretty JSON and write to file
            string json = JsonUtility.ToJson(gameState, true);
            File.WriteAllText(fullPath, json);
            Debug.Log($"Game Saved Successfully to: {fileName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game to {fileName}: {e.Message}");
        }
    }

    /// <summary>
    /// Loads game state from a specified save file.
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>Loaded GameState object or null if failed</returns>
    public static GameState LoadGame(string fileName)
    {
        string fullPath = GetFullSavePath(fileName);
        if (fullPath == null || !File.Exists(fullPath))
        {
            Debug.LogWarning($"Save file not found: {fileName}");
            return null;
        }

        try
        {
            // Read JSON from file and deserialize to GameState
            string json = File.ReadAllText(fullPath);
            GameState loadedState = JsonUtility.FromJson<GameState>(json);
            Debug.Log($"Game Loaded Successfully from: {fileName}");
            return loadedState;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game from {fileName}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deletes a specified save game file.
    /// </summary>
    /// <param name="fileName">The name of the save file to delete (without extension).</param>
    public static void DeleteSaveGame(string fileName)
    {
        string fullPath = GetFullSavePath(fileName);
        if (fullPath == null) return;

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                Debug.Log($"Save game '{fileName}' deleted successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save game '{fileName}': {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to delete non-existent save file: {fileName}");
        }
    }

    /// <summary>
    /// Returns a list of all save file names (without extensions) currently stored.
    /// </summary>
    /// <returns>Sorted list of save file names</returns>
    public static List<string> GetAllSaveFileNames()
    {
        List<string> fileNames = new List<string>();
        try
        {
            string[] files = Directory.GetFiles(saveDirectoryPath, "*" + SAVE_FILE_EXTENSION);
            foreach (string filePath in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                fileNames.Add(fileNameWithoutExtension);
            }

            fileNames.Sort();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get save files: {e.Message}");
        }

        return fileNames;
    }

    /// <summary>
    /// Checks if a save file with the given name exists.
    /// </summary>
    /// <param name="fileName">Save file name without extension</param>
    /// <returns>True if save file exists, false otherwise</returns>
    public static bool HasSaveGame(string fileName)
    {
        return File.Exists(GetFullSavePath(fileName));
    }

    /// <summary>
    /// Checks if there are any save files present.
    /// </summary>
    /// <returns>True if at least one save file exists</returns>
    public static bool HasAnySaveGames()
    {
        return Directory.Exists(saveDirectoryPath) &&
               Directory.GetFiles(saveDirectoryPath, "*" + SAVE_FILE_EXTENSION).Length > 0;
    }

    /// <summary>
    /// Data class representing the full game state to save/load.
    /// </summary>
    [Serializable]
    public class GameState
    {
        public int matchesFound;
        public int turnsTaken;
        public int gridSizeX;
        public int gridSizeY;
        public List<CardState> cardStates; // List of states for each card on the board
    }

    /// <summary>
    /// Data class representing individual card state within the saved game.
    /// </summary>
    [Serializable]
    public class CardState
    {
        public int boardIndex; // Position or identifier of card on the board
        public int typeId; // Card type or image ID
        public bool isMatched; // Whether the card has been matched

        public CardState(int boardIndex, int typeId, bool isMatched)
        {
            this.boardIndex = boardIndex;
            this.typeId = typeId;
            this.isMatched = isMatched;
        }
    }
}