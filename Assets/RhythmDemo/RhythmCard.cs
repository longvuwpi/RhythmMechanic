using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

// Attached to card objects on the main menu
public class RhythmCard : MonoBehaviour
{
    public TextMeshProUGUI eventText, outcomeText;
    string eventString;
    public string outcomeString;
    RhythmGame rhythmGame;
    string descriptionString;

    // Use this for initialization
    void Start()
    {
        rhythmGame = FindObjectOfType<RhythmGame>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void setEventText()
    {
        eventText.text = eventString;
    }

    public string getEventString()
    {
        return eventString;
    }

    public string getOutcomeString()
    {
        return outcomeString;
    }

    public string getDescription()
    {
        return descriptionString;
    }

    void setOutcomeText()
    {
        string target = "";

        string[] outcomeSplit = outcomeString.Split(null);

        //Debug.Log("length: " + outcomeSplit.Length);

        for (int i = 0; i < outcomeSplit.Length; i++)
        {
            //Debug.Log(outcomeSplit[i]);
            string[] eachOutcome = outcomeSplit[i].Split('+');
            target += "<sprite name=" + eachOutcome[0] + ">+" + eachOutcome[1] + " ";
        }

        outcomeText.text = target;
    }

    public void setContent(string content)
    {
        string[] contentSplit = content.Split(',');

        eventString = contentSplit[0].Trim();
        setEventText();

        outcomeString = contentSplit[1].Trim();
        setOutcomeText();

        descriptionString = contentSplit[2].Trim();
    }

    // called when a card is clicked
    public void CardSelected()
    {
        rhythmGame.CardSelected(this);
    }
}
