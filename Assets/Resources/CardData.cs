using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Memory/CardData")]
public class CardData : ScriptableObject
{
    [Tooltip("List of sprites used for the faces of the cards.")]
    public List<Sprite> faceImages;

    [Tooltip("Sprite used for the back of all cards.")]
    public Sprite cardBack;

    public Sprite GetSpriteById(int id)
    {
        if (faceImages == null || faceImages.Count == 0)
        {
            Debug.LogError("CardData: faceImages list is empty or null!");
            return null;
        }

        int index = id % faceImages.Count;
        return faceImages[index];
    }
}