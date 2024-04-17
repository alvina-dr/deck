using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> CardList = new List<Card>();

    public void SetupDeck()
    {
        List<Card> setList = new List<Card>(GPCtrl.Instance.SetList);
        for (int i = 0; i < GPCtrl.Instance.DeckSize; i++)
        {
            Card card = setList[Random.Range(0, setList.Count)];
            setList.Remove(card);
            CardList.Add(card);
        }
    }

    public Card PickRandomCard()
    {
        Card card = CardList[Random.Range(0, CardList.Count)];
        CardList.Remove(card);
        return card;
    }

    public Card PickCard(Card card)
    {
        CardList.Remove(card);
        return card;
    }

    public bool CanPlayCard(int currentMana)
    {
        bool canPlay = false;
        for (int i = 0; i < CardList.Count; i++)
        {
            if (CardList[i].Cost <= currentMana) canPlay = true;
        }
        return canPlay;
    }


    public List<Card> GetPlayableCards(int currentMana)
    {
        List<Card> cardList = new List<Card>();
        for (int i = 0; i < CardList.Count; i++)
        {
            if (CardList[i].Cost <= currentMana) cardList.Add(CardList[i]);
        }
        return cardList;
    }

    public Card GetOldestPlayableCard(int currentMana)
    {
        for (int i = 0; i < CardList.Count; i++)
        {
            if (CardList[i].Cost <= currentMana) return CardList[i];
        }
        return null;
    }
}
