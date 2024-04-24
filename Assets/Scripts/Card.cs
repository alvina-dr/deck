using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    public int Attack;
    public int Defense;
    public int Cost;
    public bool HasTaunt;
    public bool HasTrample;
    public bool HasDistortion;
    public bool HasFirstStrike;
    [System.NonSerialized] public int CurrentHealth;

    public Card(int attack, int defense, bool hasTaunt, bool hasTrample, bool hasDistortion, bool hasFirstStrike)
    {
        Attack = attack;
        Defense = defense;
        HasTaunt = hasTaunt;
        HasTrample = hasTrample;
        HasDistortion = hasDistortion;
        HasFirstStrike = hasFirstStrike;
        CurrentHealth = Defense;
        Cost = Mathf.CeilToInt((Attack + Defense) / 2f + (HasTaunt ? 1.5f : 0) + (HasTrample ? 1f : 0) + (HasDistortion ? 1f : 0) + (HasFirstStrike ? 1f : 0));
    }
}