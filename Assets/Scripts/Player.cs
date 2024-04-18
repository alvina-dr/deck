using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int PlayerIndex;
    public int CurrentHealth;
    public int CurrentMana;
    public int MaxMana;
    public int TurnIndex;
    public List<Card> DeckModel = new List<Card>();
    public Deck Deck;
    public Deck Hand;
    public Deck Invoked;

    public void SetupPlayer(int index)
    {
        PlayerIndex = index;
        name = "Player " + PlayerIndex;
        Deck = new Deck();
        Hand = new Deck();
        Invoked = new Deck();
        SetupDeck(Deck);
        DeckModel = new List<Card>(Deck.CardList);
        GPCtrl.Instance.formerDeck = new List<Card>(DeckModel);
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        CurrentHealth = GPCtrl.Instance.StartHealth;
        MaxMana = 0;
        TurnIndex = 0;
        Deck.CardList = new List<Card>(DeckModel);
        Hand.CardList.Clear();
        Invoked.CardList.Clear();
        for (int i = 0; i < GPCtrl.Instance.StartHandCardNum; i++)
        {
            Hand.CardList.Add(PickRandomCard(Deck));
        }
    }

    public void StartTurn()
    {
        MaxMana++;
        CurrentMana = MaxMana;
        if (Deck.CardList.Count == 0)
        {
            Debug.Log("GAME OVER NO CARDS LEFT");
            GPCtrl.Instance.GameOver(GetOtherPlayer());
            return;
        }
        Hand.CardList.Add(PickRandomCard(Deck));
        while (CurrentMana > 0 && Hand.CardList.Count > 0 && CanPlayCard(Hand)) //tant qu'on peut invoquer des cartes on en invoque
        {
            List<Card> cardList = GetPlayableCards(Hand);
            int higherPrice = 0;
            for (int i = 0; i < cardList.Count; i++)
            {
                if (cardList[i].Cost > higherPrice) higherPrice = cardList[i].Cost;
            }

            Card card = cardList.FindAll(x => x.Cost == higherPrice)[0];
            if (card != null) PlayCard(card);
            else break;
        }
        InflictDamage();
        if (!GPCtrl.Instance.IsGameOver)
            EndTurn();
    }

    public void PlayCard(Card card)
    {
        CurrentMana -= card.Cost;
        Invoked.CardList.Add(PickCard(Hand, card));
    }

    public void InflictDamage()
    {
        for (int i = 0; i < Invoked.CardList.Count; i++)
        {
            Player enemyPlayer = GPCtrl.Instance.PlayerList[GetOtherPlayer()];
            enemyPlayer.Damage(Invoked.CardList[i].Attack);
        }
    }

    public void Damage(int damage)
    {
        if (GPCtrl.Instance.IsGameOver) return;
        CurrentHealth -= damage;
        if (CurrentHealth <= 0) Death();
    }

    public void Death()
    {
        GPCtrl.Instance.GameOver(GetOtherPlayer());
    }

    public void EndTurn()
    {
        TurnIndex++;
        GPCtrl.Instance.PlayerList[GetOtherPlayer()].StartTurn();
    }

    public int GetOtherPlayer()
    {
        if (PlayerIndex == 1) return 0;
        else return 1;
    }

    #region DeckManagement
    public void SetupDeck(Deck deck)
    {
        for (int i = 0; i < GPCtrl.Instance.DeckSize; i++)
        {
            Card card = GPCtrl.Instance.SetList[Random.Range(0, GPCtrl.Instance.SetList.Count)];
            if (deck.CardList.FindAll(x => x == card).Count < GPCtrl.Instance.sameCardInDeck)
            {
                deck.CardList.Add(card);
                //Debug.Log(PlayerIndex + " - attack : " + card.Attack + " - health : " + card.Health);
            } else
            {
                i--;
            }
        }
    }

    public Card PickRandomCard(Deck deck)
    {
        Card card = deck.CardList[Random.Range(0, deck.CardList.Count)];
        deck.CardList.Remove(card);
        return card;
    }

    public Card PickCard(Deck deck, Card card)
    {
        deck.CardList.Remove(card);
        return card;
    }

    public bool CanPlayCard(Deck deck)
    {
        bool canPlay = false;
        for (int i = 0; i < deck.CardList.Count; i++)
        {
            if (deck.CardList[i].Cost <= CurrentMana) canPlay = true;
        }
        return canPlay;
    }


    public List<Card> GetPlayableCards(Deck deck)
    {
        List<Card> cardList = new List<Card>();
        for (int i = 0; i < deck.CardList.Count; i++)
        {
            if (deck.CardList[i].Cost <= CurrentMana) cardList.Add(deck.CardList[i]);
        }
        return cardList;
    }

    public Card GetOldestPlayableCard(Deck deck)
    {
        for (int i = 0; i < deck.CardList.Count; i++)
        {
            if (deck.CardList[i].Cost <= CurrentMana) return deck.CardList[i];
        }
        return null;
    }
    #endregion
}
