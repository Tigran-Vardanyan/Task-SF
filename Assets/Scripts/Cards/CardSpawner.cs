using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    private Vector2Int gridSize;
    private float hSpacing, vSpacing;
    private GameObject cardPrefab;
    private CardData cardData;

    [SerializeField] private BoardFitter boardFitter;

    public void Initialize(Vector2Int gridSize, float hSpacing, float vSpacing, GameObject cardPrefab, CardData cardData)
    {
        this.gridSize = gridSize;
        this.hSpacing = hSpacing;
        this.vSpacing = vSpacing;
        this.cardPrefab = cardPrefab;
        this.cardData = cardData;
    }

    public void SpawnCards()
    {
        Vector3 boardSize = boardFitter.GetBoardSize();
        Vector3 origin = boardFitter.GetBoardOrigin();

        List<int> ids = GenerateShuffledCardIDs();

        Renderer prefabRenderer = cardPrefab.GetComponentInChildren<Renderer>();
        Vector3 baseCardSize = prefabRenderer.bounds.size;
        float cellWidth = (boardSize.x - hSpacing * (gridSize.x - 1)) / gridSize.x;
        float cellHeight = (boardSize.z - vSpacing * (gridSize.y - 1)) / gridSize.y;
        float scale = Mathf.Min(cellWidth / baseCardSize.x, cellHeight / baseCardSize.z);

        int index = 0;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (index >= ids.Count) return;

                float px = x * (cellWidth + hSpacing);
                float pz = y * (cellHeight + vSpacing);
                Vector3 pos = origin + new Vector3(px + cellWidth / 2f, 0f, pz + cellHeight / 2f);

                GameObject cardObj = Instantiate(cardPrefab, pos, Quaternion.identity, boardFitter.GetBoardTransform());
                cardObj.transform.localScale = Vector3.one * scale;

                Card card = cardObj.GetComponent<Card>();
                card.Initialize(ids[index], cardData.GetCardFace(ids[index]), cardData.cardBack);
                index++;
            }
        }
    }

    private List<int> GenerateShuffledCardIDs()
    {
        int total = gridSize.x * gridSize.y;
        List<int> ids = new();
        for (int i = 0; i < total / 2; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }
        for (int i = 0; i < ids.Count; i++)
        {
            int rand = Random.Range(i, ids.Count);
            (ids[i], ids[rand]) = (ids[rand], ids[i]);
        }
        return ids;
    }
}