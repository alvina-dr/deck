using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System.Linq;

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
    public TextAsset Player0DeckFile;
    public TextAsset Player1DeckFile;
    [ReadOnly]
    public List<float> bestwinRateList;
    [ReadOnly]
    public List<float> trueWinRateList;

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
        if (Player0DeckFile != null) ImportDeckFromJSON(0);
        if (Player1DeckFile != null) ImportDeckFromJSON(1);
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
            TournamentNum++;
            if (TournamentNum < TournamentNumMax)
                ChangeDeck();
            else
            {
                Debug.Log("TEST OVER, took : " + (Time.realtimeSinceStartup - testStartupTime));
                ExportDeckToJSON();
                ExportWinRateToCSV();
                ExportStatsToCSV();
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
        for (int atk = 0; atk <= 16; atk++)
        {
            for (int def = 1; def <= 16; def++)
            {
                Card card = new Card(atk, def, false, false, false, false);
                Card cardTaunt = new Card(atk, def, true, false, false, false);
                Card cardTrample = new Card(atk, def, false, true, false, false);
                Card cardTauntTrample = new Card(atk, def, true, true, false, false);
                Card cardDistortion = new Card(atk, def, false, false, true, false);
                Card cardTauntDistortion = new Card(atk, def, true, false, true, false);
                Card cardTrampleDistortion = new Card(atk, def, false, true, true, false);
                Card cardTauntTrampleDistortion = new Card(atk, def, true, true, true, false);
                Card cardFirstStrike = new Card(atk, def, false, false, false, true);
                Card cardTauntFirstStrike = new Card(atk, def, true, false, false, true);
                Card cardTrampleFirstStrike = new Card(atk, def, false, true, false, true);
                Card cardDistortionFirstStrike = new Card(atk, def, false, false, true, true);
                Card cardTauntTrampleFirstStrike = new Card(atk, def, true, true, false, true);
                Card cardTauntDistortionFirstStrike = new Card(atk, def, true, false, true, true);
                Card cardTrampleDistortionFirstStrike = new Card(atk, def, false, true, true, true);
                Card cardAll = new Card(atk, def, true, true, true, true);
                if (card.Cost <= 8) cards.Add(card);
                if (cardTaunt.Cost <= 8) cards.Add(cardTaunt);
                if (cardTrample.Cost <= 8) cards.Add(cardTrample);
                if (cardTauntTrample.Cost <= 8) cards.Add(cardTauntTrample);
                if (cardDistortion.Cost <= 8) cards.Add(cardDistortion);
                if (cardTauntDistortion.Cost <= 8) cards.Add(cardTauntDistortion);
                if (cardTrampleDistortion.Cost <= 8) cards.Add(cardTrampleDistortion);
                if (cardTauntTrampleDistortion.Cost <= 8) cards.Add(cardTauntTrampleDistortion);
                if (cardFirstStrike.Cost <= 8) cards.Add(cardFirstStrike);
                if (cardTauntFirstStrike.Cost <= 8) cards.Add(cardTauntFirstStrike);
                if (cardTrampleFirstStrike.Cost <= 8) cards.Add(cardTrampleFirstStrike);
                if (cardDistortionFirstStrike.Cost <= 8) cards.Add(cardDistortionFirstStrike);
                if (cardTauntTrampleFirstStrike.Cost <= 8) cards.Add(cardTauntTrampleFirstStrike);
                if (cardTauntDistortionFirstStrike.Cost <= 8) cards.Add(cardTauntDistortionFirstStrike);
                if (cardTrampleDistortionFirstStrike.Cost <= 8) cards.Add(cardTrampleDistortionFirstStrike);
                if (cardAll.Cost <= 8) cards.Add(cardAll);
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

    public void ExportStatsToCSV()
    {
        string filePath = JSONPath;

        StreamWriter writer = new StreamWriter(filePath + "/stats.csv");

        //writer.WriteLine("HasTaunt; HasTrample; HasDistortion; HasFirstStrike");
        List<string> stringList = new List<string>();
        for (int i = 0; i < PlayerList[0].DeckModel.Count; ++i)
        {
            string value = PlayerList[0].DeckModel[i].HasTaunt ? "Taunt" : "";
            value += (PlayerList[0].DeckModel[i].HasTrample ? "Trample" : "");
            value += (PlayerList[0].DeckModel[i].HasDistortion ? "Distortion" : "");
            value += (PlayerList[0].DeckModel[i].HasFirstStrike ? "FirstStrike" : "") ;
            if (value == "") value = "No power";
            stringList.Add(value);
            //writer.WriteLine(value);
        }

        var a = stringList.GroupBy(x => x);

        foreach(var y in a )
        {
            writer.WriteLine(y.Key + ";" + y.Count());
        }
        writer.Flush();
        writer.Close();
    }

    [Button]
    public void ImportDeckFromJSON(int playerIndex)
    {
        Deck deck = JsonUtility.FromJson<Deck>(Player1DeckFile.text);
        PlayerList[playerIndex].Deck.CardList = new List<Card>(deck.CardList);
        PlayerList[playerIndex].DeckModel = new List<Card>(deck.CardList);
    }
}