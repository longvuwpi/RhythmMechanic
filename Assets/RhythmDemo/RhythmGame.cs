using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

// Manage game flow 
public class RhythmGame : MonoBehaviour {
    List<string> attributeNames = new List<string>() { "Money", "Plot", "Salsa"   };
    int[] attributeValues = new int[3];
    public TextMeshProUGUI stat;
    public Canvas pregame;
    public RhythmDemo demoGameplay;
    public RhythmCardDeck cardDeck;

    int currentHighScore = 0;

    void Start () {
        // Set stat values, show main menu
        attributeValues[0] = 0;
        attributeValues[1] = 0;
        attributeValues[2] = 0;
        pregame.gameObject.SetActive(true);
        demoGameplay.stopGame();
    }

    public void updateHighScore(int newScore)
    {
        if (newScore > currentHighScore)
        {
            currentHighScore = newScore;
        }
    }

    public void showPregame()
    {
        pregame.gameObject.SetActive(true);
        cardDeck.DrawCards();
    }

    // Update is called once per frame
    void Update () {
        // Update the high score and stat
        string target = "";
        for (int i = 0; i < attributeNames.Count; i++)
        {
            target += "<sprite name=" + attributeNames[i] + "> " + attributeValues[i] + "\n";
        }
        target += "Highest rhythm score: " + currentHighScore.ToString();
        stat.text = target;
    }

    // When player selects a card, start the rhythm game
    public void CardSelected(RhythmCard card)
    {
        pregame.gameObject.SetActive(false);
        demoGameplay.startGame(card);

    }
}
