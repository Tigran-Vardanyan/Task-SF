using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    private Vector2Int gridSize;
    private float hSpacing, vSpacing;
    private GameObject cardPrefab;
    private CardData cardData;

    [SerializeField] private BoardFitter boardFitter;

    /// <summary>
    /// Initializes the spawner with required parameters.
    /// </summary>
    public void Initialize(Vector2Int gridSize, float hSpacing, float vSpacing, GameObject cardPrefab, CardData cardData)
    {
        this.gridSize = gridSize;
        this.hSpacing = hSpacing;
        this.vSpacing = vSpacing;
        this.cardPrefab = cardPrefab;
        this.cardData = cardData;

        if (boardFitter == null)
        {
            Debug.LogError("CardSpawner: BoardFitter reference is not assigned! Please assign it in the Inspector.");
        }
    }

    /// <summary>
    /// Spawns cards on the board.
    /// If savedCardStates provided, loads saved game state.
    /// Otherwise generates a new shuffled set of cards.
    /// </summary>
    public void SpawnCards(List<GamePersistenceManager.CardState> savedCardStates = null)
    {
        // Clear existing cards
        foreach (Transform child in boardFitter.GetBoardTransform())
        {
            Destroy(child.gameObject);
        }

        Vector3 boardSize = boardFitter.GetBoardSize();
        Vector3 origin = boardFitter.GetBoardOrigin();

        int totalCardsExpected = gridSize.x * gridSize.y;
        List<GamePersistenceManager.CardState> cardsToAssign;

        // Use saved data if provided and valid
        if (savedCardStates != null && savedCardStates.Count == totalCardsExpected)
        {
            cardsToAssign = savedCardStates;
            Debug.Log($"CardSpawner: Spawning {totalCardsExpected} cards from provided saved states.");
        }
        else
        {
            // Generate new shuffled card IDs
            List<int> ids = GenerateShuffledCardIDs();
            cardsToAssign = new List<GamePersistenceManager.CardState>();
            for (int i = 0; i < ids.Count; i++)
            {
                cardsToAssign.Add(new GamePersistenceManager.CardState(i, ids[i], false));
            }
            Debug.Log($"CardSpawner: Spawning {cardsToAssign.Count} cards with new shuffled states.");
        }

        // Calculate scaling for cards
        Renderer prefabRenderer = cardPrefab.GetComponentInChildren<Renderer>();
        if (prefabRenderer == null)
        {
            Debug.LogError("CardSpawner: Card prefab or its children must have a Renderer component to calculate size!");
            return;
        }
        Vector3 baseCardSize = prefabRenderer.bounds.size;

        float cellWidth = (boardSize.x - hSpacing * (gridSize.x - 1)) / gridSize.x;
        float cellHeight = (boardSize.z - vSpacing * (gridSize.y - 1)) / gridSize.y;
        float scale = Mathf.Min(cellWidth / baseCardSize.x, cellHeight / baseCardSize.z);

        int index = 0;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (index >= cardsToAssign.Count) return;

                float px = x * (cellWidth + hSpacing);
                float pz = y * (cellHeight + vSpacing);
                Vector3 pos = origin + new Vector3(px + cellWidth / 2f, 0f, pz + cellHeight / 2f);

                GameObject cardObj = Instantiate(cardPrefab, pos, Quaternion.identity, boardFitter.GetBoardTransform());
                cardObj.transform.localScale = Vector3.one * scale;

                Card card = cardObj.GetComponent<Card>();
                if (card != null)
                {
                    var cardState = cardsToAssign[index];
                    Sprite faceSprite = cardData.GetSpriteById(cardState.typeId);
                    card.Initialize(cardState.typeId, faceSprite, cardData.cardBack);
                    card.SetMatched(cardState.isMatched);

                    if (cardState.isMatched)
                    {
                        card.FlipMatchedInstant();
                    }
                }
                else
                {
                    Debug.LogError($"CardSpawner: Card prefab '{cardPrefab.name}' is missing a 'Card' component!");
                }
                index++;
            }
        }
    }

    /// <summary>
    /// Generates a perfectly paired shuffled list of card IDs.
    /// </summary>
    private List<int> GenerateShuffledCardIDs()
    {
        int totalCards = gridSize.x * gridSize.y;

        // Ensure totalCards is even (safety check)
        if (totalCards % 2 != 0)
        {
            Debug.LogWarning("Grid size is odd; one card will not have a pair. Reducing total cards by one.");
            totalCards -= 1;
        }

        int pairsCount = totalCards / 2;
        List<int> ids = new List<int>();

        // Select random unique IDs for pairs
        List<int> uniqueIds = new List<int>();
        for (int i = 0; i < cardData.faceImages.Count; i++)
        {
            uniqueIds.Add(i);
        }

        // Shuffle available unique IDs
        for (int i = 0; i < uniqueIds.Count; i++)
        {
            int randomIndex = Random.Range(i, uniqueIds.Count);
            (uniqueIds[i], uniqueIds[randomIndex]) = (uniqueIds[randomIndex], uniqueIds[i]);
        }

        // Select required number of unique IDs
        for (int i = 0; i < pairsCount; i++)
        {
            int id = uniqueIds[i % uniqueIds.Count];
            ids.Add(id);
            ids.Add(id);
        }

        // Shuffle final list of paired IDs
        for (int i = 0; i < ids.Count; i++)
        {
            int randomIndex = Random.Range(i, ids.Count);
            (ids[i], ids[randomIndex]) = (ids[randomIndex], ids[i]);
        }

        return ids;
    }
}