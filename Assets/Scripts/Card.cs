using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    public int Attack;
    public int Health;
    public int Cost;

    public Card(int attack, int health)
    {
        Attack = attack;
        Health = health;
        Cost = Mathf.CeilToInt((Attack + Health) / 2f);
    }
}