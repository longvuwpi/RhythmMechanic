using UnityEngine;
using TMPro;

/// <summary>
/// Manages game flow.
/// </summary>
public class RhythmGame : MonoBehaviour
{
    private string[] attributeNames = new string[] { "Money", "Plot", "Salsa" };
    private int[] attributeValues = new int[3];

    [SerializeField]
    private TextMeshProUGUI stat;

    [SerializeField]
    private Canvas pregame;

    [SerializeField]
    private RhythmDemo demoGameplay;

    [SerializeField]
    private RhythmCardDeck cardDeck;

    private int currentHighScore = 0;

    private void Start()
    {
        // Set stat values, show main menu
        attributeValues[0] = 0;
        attributeValues[1] = 0;
        attributeValues[2] = 0;
        pregame.gameObject.SetActive(true);
        demoGameplay.StopGame();
    }

    public void UpdateHighScore(int newScore)
    {
        if(newScore > currentHighScore)
        {
            currentHighScore = newScore;
        }
    }

    public void ShowPregame()
    {
        pregame.gameObject.SetActive(true);
        cardDeck.DrawCards();
    }

    private void Update()
    {
        // Update the high score and stat
        string target = "";
        for(int i = 0; i < attributeNames.Length; i++)
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
        demoGameplay.StartGame(card);
    }
}
