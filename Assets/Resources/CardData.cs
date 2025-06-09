using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Memory/CardData")]
public class CardData : ScriptableObject
{
    public List<Sprite> faceImages;
    public Sprite cardBack;

    public Sprite GetCardFace(int id)
    {
        return faceImages[id % faceImages.Count];
    }
}