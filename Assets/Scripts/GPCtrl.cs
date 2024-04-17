using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GPCtrl : MonoBehaviour
{
    public List<Player> PlayerList;
    public List<Card> SetList = new List<Card>();
    public Player PlayerPrefab;
    public bool IsGameOver = false;
    public int GameNum = 0;
    public int GameWonByPlayer1 = 0;

    [Header("STATS")]
    public int StartHandCardNum;
    public int DeckSize;
    public int StartHealth;
    public int GameTestNumMax;

    private float startupTime;

    #region Singleton
    public static GPCtrl Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    private void Start()
    {
    }

    [Button]
    public void LaunchGame()
    {
        startupTime = Time.realtimeSinceStartup;
        SetList = GenerateCards();
        for (int i = 0; i < 2; i++)
        {
            Player player = Instantiate(PlayerPrefab);
            player.SetupPlayer(i);
            PlayerList.Add(player);
        }
        PlayerList[0].StartTurn();
    }

    public void GameOver(int winnerIndex)
    {
        //Debug.Log("WINNER IS " + winnerIndex);
        if (winnerIndex == 0) GameWonByPlayer1++;
        IsGameOver = true;

        GameNum++;
        if (GameNum < GameTestNumMax)
        {
            IsGameOver = false;
            for (int i = 0; i < PlayerList.Count; i++)
            {
                PlayerList[i].ResetPlayer();
            }
            if (GameNum < GameTestNumMax / 2)
                PlayerList[0].StartTurn();
            else PlayerList[1].StartTurn();
        }
        else
        {
            Debug.Log("WIN RATE OF PLAYER 1 IS : " + GameWonByPlayer1 * 1.0f / GameNum * 1.0f);
            Debug.Log("timeout : " + (Time.realtimeSinceStartup - startupTime));
        }
    }

    [Button]
    public void PlayAgain()
    {
        GameNum = 0;
        IsGameOver = false;
        startupTime = Time.realtimeSinceStartup;
        for (int i = 0; i < PlayerList.Count; i++)
        {
            PlayerList[i].ResetPlayer();
        }
        if (GameNum < GameTestNumMax / 2)
            PlayerList[0].StartTurn();
        else PlayerList[1].StartTurn();
    }

    public List<Card> GenerateCards()
    {
        List<Card> cards = new List<Card>();
        for (int atk = 0; atk <= 6; atk++)
        {
            for (int def = 1; def <= 6; def++)
            {
                Card card = new Card(atk, def);
                if (card.Cost <= 6)
                {
                    cards.Add(card);
                }
            }
        }
        return cards;
    }
}
