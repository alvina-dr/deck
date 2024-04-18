using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    public List<Card> CardList = new List<Card>();
    public int MaxCards;
    public int MaxSameCards;
}
