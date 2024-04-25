using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;

public class GPCtrl : MonoBehaviour
{
    public List<Player> PlayerList;
    public List<Card> SetList = new List<Card>();
    public Player PlayerPrefab;
    public bool IsGameOver = false;

    [Header("GAME - a game")]
    [ReadOnly]
    public int GameNum = 0;
    public int GameNumMax;
    [ReadOnly]
    public int GameNumForFrame = 0;
    [ReadOnly]
    public int GameWonByPlayer1 = 0;

    [Header("TOURNAMENT - a set of X game")]
    [ReadOnly]
    public int TournamentNum = 0;
    public int TournamentNumMax;

    [Header("STATS")]
    public int StartHandCardNum;
    public int DeckSize;
    public int StartHealth;
    public int sameCardInDeck;

    [Header("COMPARISON")]
    [ReadOnly]
    public float formerWinRate = 0;
    [ReadOnly]
    public List<Card> formerDeck = new List<Card>();
    public string JSONPath;
    public TextAsset DeckFile;
    [ReadOnly]
    public List<float> bestwinRateList;
    [ReadOnly]
    public List<float> trueWinRateList;
    public List<float> turnNumList;
    public List<float> turnNumAverageList;

    [Header("PERFORMANCE")]
    public int turnToNextFrame;

    private float startupTime;
    private float testStartupTime;

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

    [Button]
    public void LaunchGame()
    {
        startupTime = Time.realtimeSinceStartup;
        testStartupTime = Time.realtimeSinceStartup;
        SetList = GenerateCards();
        for (int i = 0; i < 2; i++)
        {
            Player player = Instantiate(PlayerPrefab);
            player.SetupPlayer(i);
            PlayerList.Add(player);
        }
        if (DeckFile != null) ImportDeckFromJSON();
        PlayerList[0].StartTurn();
    }

    public void GameOver(int winnerIndex)
    {
        if (winnerIndex == 0) GameWonByPlayer1++;
        IsGameOver = true;

        GameNum++;
        GameNumForFrame++;
        if (GameNumForFrame >= turnToNextFrame) StartCoroutine(NextFrame());
        else
        {
            CheckNewGame();
        }
    }

    public IEnumerator NextFrame()
    {
        GameNumForFrame = 0;
        yield return new WaitForEndOfFrame();
        CheckNewGame();
    }

    public void CheckNewGame()
    {
        turnNumList.Add(PlayerList[0].TurnIndex);
        if (GameNum < GameNumMax)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                PlayerList[i].ResetPlayer();
            }
            IsGameOver = false;
            if (GameNum < GameNumMax / 2.0f)
                PlayerList[0].StartTurn();
            else PlayerList[1].StartTurn();
        }
        else
        {
            float total = 0;
            for (int i = 0; i < turnNumList.Count; i++)
            {
                total += turnNumList[i];
            }
            float average = total / turnNumList.Count;
            turnNumAverageList.Add(average);
            turnNumList.Clear();
            TournamentNum++;
            if (TournamentNum < TournamentNumMax)
                ChangeDeck();
            else
            {
                Debug.Log("TEST OVER, took : " + (Time.realtimeSinceStartup - testStartupTime));
                ExportDeckToJSON();
                ExportWinRateToCSV();
                ExportTurnNumToCSV();
            }
        }
    }

    [Button]
    public void PlayAgain()
    {
        startupTime = Time.realtimeSinceStartup;
        ResetGame();
        if (GameNum < GameNumMax / 2.0f)
            PlayerList[0].StartTurn();
        else PlayerList[1].StartTurn();
    }

    public void ResetGame()
    {
        GameNum = 0;
        GameNumForFrame = 0;
        GameWonByPlayer1 = 0;
        IsGameOver = false;
        for (int i = 0; i < PlayerList.Count; i++)
        {
            PlayerList[i].ResetPlayer();
        }
    }

    public void ChangeDeck()
    {
        GameNumForFrame = turnToNextFrame;
        float newWinRate = GameWonByPlayer1 * 1.0f / GameNum;
        trueWinRateList.Add(newWinRate);
        if (newWinRate < formerWinRate)
        {
            PlayerList[0].DeckModel = new List<Card>(formerDeck);
        } else
        {
            formerWinRate = newWinRate;
            formerDeck = new List<Card>(PlayerList[0].DeckModel);
        }
        bestwinRateList.Add(formerWinRate);

        //CHANGE DECK MODEL
        PlayerList[0].DeckModel.RemoveAt(Random.Range(0, PlayerList[0].DeckModel.Count));

        for (int i = 0; i < 1; i++)
        {
            Card card = SetList[Random.Range(0, SetList.Count)];
            if (PlayerList[0].DeckModel.FindAll(x => x == card).Count < sameCardInDeck)
            {
                PlayerList[0].DeckModel.Add(SetList[Random.Range(0, SetList.Count)]);
            }
            else
            {
                i--;
            }
        }

        //for (int i = 0; i < PlayerList[0].DeckModel.Count; i++)
        //{
        //    Debug.Log("0" + " - attack : " + PlayerList[0].DeckModel[i].Attack + " - health : " + PlayerList[0].DeckModel[i].Health);
        //}

        ResetGame();

        if (GameNum < GameNumMax / 2.0f)
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

    public void ExportDeckToJSON()
    {
        Deck deck = new Deck();
        deck.CardList = new List<Card>(PlayerList[0].DeckModel);
        string exportString = JsonUtility.ToJson(deck);
        System.IO.File.WriteAllText(JSONPath + "/DeckModel.json", exportString);
    }

    public void ExportWinRateToCSV()
    {
        string filePath = JSONPath;

        StreamWriter writer = new StreamWriter(filePath + "/winRate.csv");

        for (int i = 0; i < bestwinRateList.Count; ++i)
        {
            writer.WriteLine(trueWinRateList[i] + ";" + bestwinRateList[i]);
        }
        writer.Flush();
        writer.Close();
    }

    public void ExportTurnNumToCSV()
    {
        string filePath = JSONPath;

        StreamWriter writer = new StreamWriter(filePath + "/turnNum.csv");

        for (int i = 0; i < turnNumAverageList.Count; ++i)
        {
            writer.WriteLine(turnNumAverageList[i]);
        }
        writer.Flush();
        writer.Close();
    }

    [Button]
    public void ImportDeckFromJSON()
    {
        Deck deck = JsonUtility.FromJson<Deck>(DeckFile.text);
        PlayerList[1].Deck.CardList = new List<Card>(deck.CardList);
        PlayerList[1].DeckModel = new List<Card>(deck.CardList);
    }
}