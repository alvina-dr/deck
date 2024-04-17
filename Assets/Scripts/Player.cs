using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int PlayerIndex;
    public int CurrentHealth;
    public int CurrentMana;
    public int MaxMana;
    public int TurnIndex;
    public List<Card> deckModel = new List<Card>();
    public Deck Deck;
    public Deck Hand;
    public Deck Invoked;

    public void SetupPlayer(int index)
    {
        PlayerIndex = index;
        name = "Player " + PlayerIndex;
        Deck.SetupDeck();
        deckModel = new List<Card>(Deck.CardList);
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        CurrentHealth = GPCtrl.Instance.StartHealth;
        MaxMana = 0;
        TurnIndex = 0;
        Deck.CardList = new List<Card>(deckModel);
        Hand.CardList.Clear();
        Invoked.CardList.Clear();
        for (int i = 0; i < GPCtrl.Instance.StartHandCardNum; i++)
        {
            Hand.CardList.Add(Deck.PickRandomCard());
        }
    }

    public void StartTurn()
    {
        MaxMana++;
        CurrentMana = MaxMana;
        Hand.CardList.Add(Deck.PickRandomCard());
        while (CurrentMana > 0 && Hand.CardList.Count > 0 && Hand.CanPlayCard(CurrentMana)) //tant qu'on peut invoquer des cartes on en invoque
        {
            //order to find most costly
            //for now just get oldest one
            //Debug.Log("mana left : " + CurrentMana);
            Card card = Hand.GetOldestPlayableCard(CurrentMana);
            PlayCard(card);
            if (CurrentMana == 0) break;
        }
        InflictDamage();
        if (!GPCtrl.Instance.IsGameOver)
            EndTurn();
    }

    public void PlayCard(Card card)
    {
        CurrentMana -= card.Cost;
        Invoked.CardList.Add(Hand.PickCard(card));
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
        //Debug.Log(GetOtherPlayer() + "take damage from " + name);
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
}
