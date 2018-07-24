using UnityEngine;
using TMPro;

/// <summary>
/// Controls card objects in the main menu.
/// </summary>
public class RhythmCard : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI eventText, outcomeText;
    private string eventString;
    private string outcomeString;
    private RhythmGame rhythmGame;
    private string descriptionString;

    private void Start()
    {
        rhythmGame = FindObjectOfType<RhythmGame>();
    }

    private void SetEventText()
    {
        eventText.text = eventString;
    }

    public string GetEventString()
    {
        return eventString;
    }

    public string GetOutcomeString()
    {
        return outcomeString;
    }

    public string GetDescription()
    {
        return descriptionString;
    }

    private void SetOutcomeText()
    {
        string target = "";

        string[] outcomeSplit = outcomeString.Split(null);

        //Debug.Log("length: " + outcomeSplit.Length);

        for(int i = 0; i < outcomeSplit.Length; i++)
        {
            //Debug.Log(outcomeSplit[i]);
            string[] eachOutcome = outcomeSplit[i].Split('+');
            target += "<sprite name=" + eachOutcome[0] + ">+" + eachOutcome[1] + " ";
        }

        outcomeText.text = target;
    }

    public void SetContent(string content)
    {
        string[] contentSplit = content.Split(',');

        eventString = contentSplit[0].Trim();
        SetEventText();

        outcomeString = contentSplit[1].Trim();
        SetOutcomeText();

        descriptionString = contentSplit[2].Trim();
    }

    // called when a card is clicked
    public void CardSelected()
    {
        rhythmGame.CardSelected(this);
    }
}
