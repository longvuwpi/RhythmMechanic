using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsible for rhythm-based gameplay.
/// Contains timing settings that are initialized in the script, not the Inspector.
/// </summary>
/// <remarks>
/// The comment above this class indicated that the timing settings should be nonserialized, so I made that change.
/// Note that for some variables, the initializing value in the script differed from the value in the Inspector.
/// In order to avoid changing the gameplay, I changed the values in the script to match those in the Inspector,
/// since the values in the Inspector were overriding the script anyway.
/// --Matt
/// </remarks>
public class RhythmDemo : MonoBehaviour
{
    /// <summary>
    /// AudioSource for music.
    /// </summary>
    public AudioSource audioSourceMusic;

    /// <summary>
    /// AudioSource for sound effect that plays when the player's button-press timing earns a rank of "cool."
    /// </summary>
    public AudioSource audioSourceCool;

    /// <summary>
    /// AudioSource for sound effect that plays when the player's button-press timing earns a rank of "perfect."
    /// </summary>
    public AudioSource audioSourcePerfect;

    /// <summary>
    /// AudioSource for sound effect that plays when the player's button-press timing earns a rank of "miss."
    /// </summary>
    public AudioSource audioSourceMiss;

    /// <summary>
    /// Text object that displays the player's score.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI score;

    /// <summary>
    /// Text object that displays the story.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI actionText;

    /// <summary>
    /// Button that returns the player to the menu.
    /// </summary>
    [SerializeField]
    private Button okayButton;

    /// <summary>
    /// The shrinking bounding box that represents ideal button-press timing.
    /// </summary>
    public GameObject boundingBox;

    /// <summary>
    /// Text object that indicates the accuracy of a player's button-press timing.
    /// Button-press timing accuracy is divided into the following categories: cool, perfect, and miss.
    /// </summary>
    public TextMeshProUGUI clickResult;

    /// <summary>
    /// Prefab for a button that the player must attempt to press with perfect timing.
    /// </summary>
    [SerializeField]
    private GameObject buttonPrefab;

    /// <summary>
    /// Image that serves as a visual effect for when a button is pressed.
    /// </summary>
    [SerializeField]
    private Image explosion;

    /// <summary>
    /// Array of available particle effects.
    /// </summary>
    [SerializeField]
    private UIParticleSystem[] flashes;

    /// <summary>
    /// Used to check if the track just looped.
    /// </summary>
    private float lastPlaytime = 0;

    /// <summary>
    /// The number of times the song has looped.
    /// </summary>
    private int loopTime = 0;

    /// <summary>
    /// The player's current score.
    /// </summary>
    private int playerScore = 0;

    /// <summary>
    /// The string containing the story that the player has assembled by playing the game.
    /// </summary>
    private string actionString = "";

    /// <summary>
    /// The string containing the story that the player must assemble by playing the game.
    /// </summary>
    private string targetActionString = "";

    /// <summary>
    /// A <c>Random</c> object used to generate random values.
    /// </summary>
    private System.Random rd = new System.Random();

    [HideInInspector]
    public float TimeBefore = 0.6f;

    [HideInInspector]
    public float TimeAfter = 0.4f;

    [HideInInspector]
    public float TimePerfectWindowHalf = 0.05f;

    [HideInInspector]
    public float TimeShowResult = 0.2f;

    [HideInInspector]
    public float TimeButtonDrift = 1f;

    [HideInInspector]
    public float TimeScoreLerp = 0.5f;

    [HideInInspector]
    public float[] TimeEvents = new float[] { 0.744f, 2.222f, 3.743f, 5.247f, 6.743f, 8.246f, 9.762f, 11.215f };

    // To run through eventsTime
    private int eventIndex = 0;

    [HideInInspector]
    public float CurrentPerfectTime;

    //Phrases for each button
    private List<string> content = new List<string>();
    private int contentIndex = 0;

    // Score related
    [HideInInspector]
    public int CurrentPerfect;

    [HideInInspector]
    public int TotalPerfects;

    [HideInInspector]
    public int TotalCools;

    [HideInInspector]
    public int TotalMisses;

    [HideInInspector]
    public int CoolScore = 5;

    [HideInInspector]
    public int PerfectScore = 10;

    [HideInInspector]
    public bool OptionClicked = false;

    // How many times buttons spawned
    public int GetTotalSpawns()
    {
        return (TotalCools + TotalMisses + TotalPerfects);
    }

    // Get the story of the event, split it, put it in the array, reset variables
    public void SetContent(string eventDescription)
    {
        string[] contentSplit = eventDescription.Split(null);
        List<string> newContent = new List<string>(contentSplit);

        content.AddRange(newContent);
        contentIndex = 0;
        CurrentPerfect = 0;
        TotalPerfects = 0;
        TotalCools = 0;
        TotalMisses = 0;
        OptionClicked = false;
    }

    // empty the content
    public void RemoveContent()
    {
        content = new List<string>();
    }

    // Score for miss
    // It doesn't reset Perfect combo because
    public void ButtonMissed()
    {
        TotalMisses++;
    }

    // Score for perfect
    public void ButtonScoredPerfect()
    {
        TotalPerfects++;

        //Increment perfect combo
        CurrentPerfect++;

        AddScore(PerfectScore * CurrentPerfect);
    }

    // Score for cool
    public void ButtonScoredCool()
    {
        TotalCools++;

        AddScore(CoolScore);

        // Reset perfect combo
        CurrentPerfect = 0;
    }

    // Update is called once per frame
    // Gotta put this logic into Update() with the way this works
    private void Update()
    {
        if(audioSourceMusic.isPlaying)
        {
            float playTime = audioSourceMusic.time;

            //Figure out if the song just looped
            if(playTime < lastPlaytime)
            {
                loopTime++;
            }

            lastPlaytime = playTime;

            //Display the score
            score.text = playerScore.ToString();

            // Stop the game if
            if(loopTime > 4)
            {
                // put down the volume, remove everything, show the okay button, show score
                audioSourceMusic.volume = 0.6f;

                RemoveSpawnedButtons();

                RemoveContent();
                okayButton.gameObject.SetActive(true);
                score.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                // for what
                float length = audioSourceMusic.clip.length;

                //Debug.Log("event index " + eventIndex);
                float currentEvent = TimeEvents[eventIndex];

                // if it's getting near the current event time, removed spawned buttons and spawn new ones
                if((playTime > (currentEvent - TimeBefore)) && (playTime < currentEvent))
                {
                    RemoveSpawnedButtons();

                    CurrentPerfectTime = currentEvent;

                    // move to next event in the song (not next content)
                    if(eventIndex >= (TimeEvents.Length - 1))
                    {
                        eventIndex = 0;
                    }
                    else
                    {
                        eventIndex++;
                    }

                    SpawnButtons();

                }
            }

        }
    }

    // It does what it says
    private void RemoveSpawnedButtons()
    {
        GameObject[] spawned = GameObject.FindGameObjectsWithTag("SpawnedButton");
        if(spawned.Length > 0)
        {
            foreach(GameObject eachSpawned in spawned)
            {
                Destroy(eachSpawned);
            }
        }
    }

    // Also does what it says
    private void SpawnButtons()
    {
        //Reset this attribute. Means that no button is clicked yet
        OptionClicked = false;

        // To store randomized positions of buttons
        List<Vector3> buttonPositions = new List<Vector3>();

        int rdX = rd.Next(-380, 380);
        int rdY = rd.Next(-180, 180);

        Vector3 firstButtonPosition = new Vector3(rdX, rdY, 0);

        buttonPositions.Add(firstButtonPosition);

        // number of answers
        // was to be changed later to 2 or 3 depends on how many real choices there are
        // but right now there can only be 1 real (correct) choice.
        int numReals = 1;
        List<string> realChoices = new List<string>();
        List<string> fakeContent = new List<string>();

        // multiple choices
        // first choice is correct, others are incorrect (fake)
        // idk what I was smoking when I did all this naming, was a while ago
        if(content[contentIndex].Contains("/"))
        {
            string[] options = content[contentIndex].Split('/');

            // first choice is correct
            realChoices.Add(options[0]);

            // other choices are incorrect
            foreach(string option in options)
            {
                if(!realChoices.Contains(option))
                {
                    fakeContent.Add(option);
                }
            }

            // have to change the currentPerfectTime and increment the event index, since if it's a multiple-choice it lasts twice as long
            CurrentPerfectTime = TimeEvents[eventIndex];

            if(eventIndex >= (TimeEvents.Length - 1))
            {
                eventIndex = 0;
            }
            else
            {
                eventIndex++;
            }
        }
        else
        {
            // else it's just 1 choice - 1 button spawning
            realChoices.Add(content[contentIndex]);
        }

        numReals = realChoices.Count;

        int numFakes = fakeContent.Count;

        // if there are fake choices
        if(numFakes > 0)
        {
            for(int i = 1; i <= numFakes; i++)
            {
                // fake choices also have randomized positions, but has to be far enough from all the previous choices
                bool farEnough = false;
                int fakeRdX, fakeRdY;
                Vector3 fakePosition = new Vector3(0, 0, 0);

                while(!farEnough)
                {
                    farEnough = true;

                    fakeRdX = rd.Next(-380, 380);
                    fakeRdY = rd.Next(-180, 180);
                    fakePosition = new Vector3(fakeRdX, fakeRdY, 0);

                    foreach(Vector3 eachPos in buttonPositions)
                    {
                        farEnough &= (Vector3.Distance(eachPos, fakePosition) > 280);
                    }
                }

                // if it's far enough from all other positions, add it to the positions list
                buttonPositions.Add(fakePosition);
            }
        }

        // Create all buttons
        for(int i = 0; i < buttonPositions.Count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(gameObject.transform);
            newButton.transform.localPosition = buttonPositions[i];
            newButton.SetActive(true);
            newButton.tag = "SpawnedButton";
            // the button keeps track of its parent
            newButton.GetComponent<MusicButton>().SetRhythmDemo(this);

            // real choice
            if(i < numReals)
            {
                newButton.GetComponent<MusicButton>().SetTerm(realChoices[i], true);
                // this is only used for the first button
                GetComponentInChildren<TutorialController>().SetButton(newButton.GetComponent<MusicButton>());
                Debug.Log("Spawned " + realChoices[i]);
            }
            // fake choice
            else
            {
                int next = i - numReals;
                newButton.GetComponent<MusicButton>().SetTerm(fakeContent[next], false);
                Debug.Log("Spawned " + fakeContent[next]);
            }

            newButton.GetComponent<MusicButton>().StartAnimation();
        }

    }

    // called when player misses
    public void Miss(MusicButton musicButton)
    {
        StartCoroutine(MissRoutine(musicButton));
    }

    // called when player cools
    public void Cool(MusicButton musicButton)
    {
        // destroy particle effects if any left
        foreach(GameObject clone in GameObject.FindGameObjectsWithTag("Clone"))
        {
            Destroy(clone);
        }

        //set up some shitty particle effect
        UIParticleSystem flashClone = Instantiate(flashes[1]);
        flashClone.gameObject.transform.SetParent(this.gameObject.transform);
        flashClone.gameObject.SetActive(true);
        flashClone.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition;
        flashClone.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1);
        flashClone.gameObject.transform.SetAsLastSibling();
        flashClone.gameObject.tag = "Clone";
        flashClone.Play();

        // Plays button animation
        StartCoroutine(CoolRoutine(musicButton));
    }

    // called when player perfects
    public void Perfect(MusicButton musicButton)
    {
        foreach(GameObject clone in GameObject.FindGameObjectsWithTag("Clone"))
        {
            Destroy(clone);
        }

        // set up more shitty particle effects
        foreach(UIParticleSystem flash in flashes)
        {
            UIParticleSystem flashClone = Instantiate(flash);
            flashClone.gameObject.transform.SetParent(this.gameObject.transform);
            flashClone.gameObject.SetActive(true);
            flashClone.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition;
            flashClone.gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 1);
            flashClone.Speed = 10f;
            flashClone.gameObject.transform.SetAsLastSibling();
            flashClone.gameObject.tag = "Clone";
            flashClone.Play();
        }

        // animate
        StartCoroutine(PerfectRoutine(musicButton));
    }

    IEnumerator MissRoutine(MusicButton musicButton)
    {
        // score keeping
        ButtonMissed();

        // MISS text
        clickResult.gameObject.SetActive(true);
        clickResult.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition + (new Vector3(30, 30, 0));
        explosion.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition + (new Vector3(30, 30, 0));
        clickResult.gameObject.transform.SetAsLastSibling();
        explosion.gameObject.transform.SetAsLastSibling();
        clickResult.text = "Miss";
        clickResult.color = Color.magenta;

        // It lerps
        float startTime = audioSourceMusic.time;
        float totalTime = TimeShowResult;
        audioSourceMiss.Play();
        float perc = (audioSourceMusic.time - startTime) / totalTime;
        while(perc < 1)
        {
            Vector3 currentScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1), perc);
            clickResult.gameObject.transform.localScale = currentScale;
            perc = (audioSourceMusic.time - startTime) / totalTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        clickResult.gameObject.SetActive(false);
    }

    IEnumerator PerfectRoutine(MusicButton musicButton)
    {
        explosion.gameObject.SetActive(true);

        // Score keeping
        ButtonScoredPerfect();

        // Perfect text
        clickResult.gameObject.SetActive(true);
        clickResult.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition + (new Vector3(30, 100, 0));
        explosion.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition + (new Vector3(30, 100, 0));
        clickResult.gameObject.transform.SetAsLastSibling();
        explosion.gameObject.transform.SetAsLastSibling();
        string target = "Perfect!";
        // Perfect combo
        if(CurrentPerfect > 1)
        {
            target += "<color=\"red\">x" + CurrentPerfect.ToString();
        }

        // Higher the combo, higher the pitch of the sound
        float newPitch = Mathf.Clamp(1 + (0.06f * (CurrentPerfect - 1)), 1, 1.8f);
        audioSourcePerfect.pitch = newPitch;

        clickResult.text = target;
        clickResult.color = new Color(1, 0, 0.59f);

        // It also lerps
        float startTime = audioSourceMusic.time;
        float totalTime = TimeShowResult;
        audioSourcePerfect.Play();
        float perc = (audioSourceMusic.time - startTime) / totalTime;

        while(perc < 1)
        {
            Vector3 currentScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1), perc);
            clickResult.gameObject.transform.localScale = currentScale;

            explosion.color = Color.Lerp(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 1), perc);
            explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(1.5f, 1.5f, 1.5f), perc);
            //explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1.5f), perc);

            perc = (audioSourceMusic.time - startTime) / totalTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        explosion.gameObject.SetActive(false);
        clickResult.gameObject.SetActive(false);
    }

    IEnumerator CoolRoutine(MusicButton musicButton)
    {
        explosion.gameObject.SetActive(true);
        // Score keeping
        ButtonScoredCool();

        // Cool text
        clickResult.gameObject.SetActive(true);
        clickResult.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition + (new Vector3(30, 100, 0));
        explosion.gameObject.transform.localPosition = musicButton.gameObject.transform.localPosition + (new Vector3(30, 100, 0));
        clickResult.gameObject.transform.SetAsLastSibling();
        explosion.gameObject.transform.SetAsLastSibling();

        clickResult.text = "Cool!";
        clickResult.color = Color.green;

        // It also lerps
        float startTime = audioSourceMusic.time;
        float totalTime = TimeShowResult;
        audioSourceCool.Play();
        float perc = (audioSourceMusic.time - startTime) / totalTime;

        while(perc < 1)
        {
            Vector3 currentScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), perc);
            clickResult.gameObject.transform.localScale = currentScale;

            explosion.color = Color.Lerp(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 1), perc);
            explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(1.5f, 1.5f, 1.5f), perc);
            //explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1.5f), perc);

            perc = (audioSourceMusic.time - startTime) / totalTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        explosion.gameObject.SetActive(false);
        clickResult.gameObject.SetActive(false);
    }

    // Add the clicked text to the constructed story
    public void UpdateAction(string newToken, bool isReal)
    {
        string[] stringSplit = actionString.Trim().Split(null);

        if(!(stringSplit.Length == content.Count))
        {
            actionString += newToken + " ";
            if(isReal)
            {
                actionText.text += newToken + " ";
            }
            else
            {
                actionText.text += "<color=\"red\">" + newToken + "</color> ";
            }
        }

        if(contentIndex < (content.Count - 1))
        {
            contentIndex++;
        }
        else
        {
            contentIndex = 0;
        }
    }

    // Add delta to current player score
    public void AddScore(int delta)
    {
        StartCoroutine(LerpScore(delta));
    }

    // It has to lerp
    IEnumerator LerpScore(int delta)
    {
        float startTime = audioSourceMusic.time;

        float length = 0;

        int oldScore = playerScore;
        int targetScore = playerScore + delta;

        score.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        while(length < TimeScoreLerp)
        {
            length = audioSourceMusic.time - startTime;
            float perc = length / TimeScoreLerp;
            int currentScore = (int)Mathf.Lerp(oldScore, targetScore, perc);
            playerScore = currentScore;

            score.gameObject.transform.localScale = Vector3.Lerp(new Vector3(1.5f, 1.5f, 1), new Vector3(2, 2, 1), perc);
            yield return null;
        }

        playerScore = targetScore;

        yield return new WaitForSeconds(0.5f);
        score.gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    // Stop song, reset some variables, show main menu
    public void StopGame()
    {
        gameObject.SetActive(false);
        loopTime = 0;
        audioSourceMusic.Stop();
        GetComponentInParent<RhythmGame>().UpdateHighScore(playerScore);
        GetComponentInParent<RhythmGame>().ShowPregame();
    }

    // Get the story, move the score to a hardcoded position on the corner (because I'm lazy), activate the gameplay, play music
    public void StartGame(RhythmCard card)
    {
        playerScore = 0;
        targetActionString = card.GetDescription();
        actionString = "";
        actionText.text = "";

        score.gameObject.transform.localPosition = new Vector3(-547, 288, 0);

        okayButton.gameObject.SetActive(false);

        gameObject.SetActive(true);

        SetContent(targetActionString);

        audioSourceMusic.volume = 1;
        audioSourceMusic.Play();

    }
}
