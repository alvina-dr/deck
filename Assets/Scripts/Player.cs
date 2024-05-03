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
            GPCtrl.Instance.GameOver(GetOtherPlayerIndex());
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
        List<Card> attackedCards = new List<Card>();
        for (int i = 0; i < Invoked.CardList.Count; i++)
        {
            Player enemyPlayer = GetOtherPlayer();
            Card tauntCard = enemyPlayer.Invoked.CardList.Find(x => x.HasTaunt);
            if (tauntCard != null && !(Invoked.CardList[i].HasDistortion && !tauntCard.HasDistortion)) //si le jeu adverse a des cartes avec la capacit� provoc
            {
                if (!attackedCards.Contains(tauntCard)) attackedCards.Add(tauntCard);
                tauntCard.CurrentHealth -= Invoked.CardList[i].Attack;
                Invoked.CardList[i].CurrentHealth -= tauntCard.Attack;
                //Debug.Log(tauntCard.Defense + " health : " + tauntCard.CurrentHealth);
                //Debug.Break();

                if (Invoked.CardList[i].HasFirstStrike && tauntCard.CurrentHealth <= 0)
                {
                    if (Invoked.CardList[i].HasTrample && tauntCard.CurrentHealth < 0) enemyPlayer.Damage(tauntCard.CurrentHealth * -1);
                    enemyPlayer.Invoked.CardList.Remove(tauntCard);
                } else {
                    Debug.Log("in else :  " + Invoked.CardList[i].CurrentHealth);
                    if (tauntCard.CurrentHealth <= 0)
                    {
                        if (Invoked.CardList[i].HasTrample && tauntCard.CurrentHealth < 0) enemyPlayer.Damage(tauntCard.CurrentHealth * -1);
                        enemyPlayer.Invoked.CardList.Remove(tauntCard);
                    }
                    if (Invoked.CardList[i].CurrentHealth <= 0)
                    {
                        Invoked.CardList.Remove(Invoked.CardList[i]);
                        i--;
                        if (i == Invoked.CardList.Count) break;
                    }
                }
            } else
            {
                enemyPlayer.Damage(Invoked.CardList[i].Attack);
            }
        }
        for (int i = 0; i < attackedCards.Count; i++) //reset life of card at end of turn
        {
            attackedCards[i].CurrentHealth = attackedCards[i].Defense; 
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
        GPCtrl.Instance.GameOver(GetOtherPlayerIndex());
    }

    public void EndTurn()
    {
        TurnIndex++;
        GPCtrl.Instance.PlayerList[GetOtherPlayerIndex()].StartTurn();
    }

    public int GetOtherPlayerIndex()
    {
        if (PlayerIndex == 1) return 0;
        else return 1;
    }

    public Player GetOtherPlayer()
    {
        if (PlayerIndex == 1) return GPCtrl.Instance.PlayerList[0];
        else return GPCtrl.Instance.PlayerList[1];
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
