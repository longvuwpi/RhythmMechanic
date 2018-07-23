using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Responsible for the rhythm gameplay
// Probably needs nonserialized tags on things that are not set in the inspector, since it's a mess now
public class RhythmDemo : MonoBehaviour {
    // game music, sound effect for cool, perfect, miss
    public AudioSource audioSource, coolSound, perfectSound, missSound;
    
    // text display for score, and the story
    public TextMeshProUGUI score, actionText;
    
    // it is what it is
    // hit it when rhythm game's over to go back to main screen
    public Button okayButton;

    // shrinking bounding box 
    public GameObject boundingBox;

    // Perfect! || Cool! || Miss
    public TextMeshProUGUI clickResult;

    public GameObject buttonPrefab;

    // Not really explosion but some silly image
    public Image explosion;

    // particle effects
    public UIParticleSystem[] flashes;

    // to compare to the current play time to see if the track just looped
    float lastPlaytime = 0;
    // how many time the song looped
    int loopTime = 0;

    // messed up naming convention from somewhere, it's the player's score as an int
    int iScore = 0;

    string actionString = "";
    string targetActionString = "";

    System.Random rd = new System.Random();

    //Timings
    public float timing_before = 0.6f;
    public float timing_after = 0.3f;
    public float perfect_window_half = 0.05f;
    public float result_duration = 0.2f; //  Perfect! || Cool! || Miss
    public float button_drift_duration = 0.8f;
    public float score_lerp_duration = 0.3f;
    public float[] eventTimes = new float[] { 0.744f, 2.222f, 3.743f, 5.247f, 6.743f, 8.246f, 9.762f, 11.215f };
    //public float[] eventsTime = new float[] { 0.75f, 2.25f, 3.75f, 5.25f, 6.75f, 8.25f, 9.75f, 11.25f,
    //                                          12.75f, 14.25f, 15.75f, 17.25f, 18.75f, 20.25f, 21.75f, 23.25f,
    //                                          24.75f, 26.25f, 27.75f, 29.25f, 30.75f, 32.25f, 33.75f, 35.25f};

    //To run through eventsTime
    int eventIndex = 0;

    public float currentPerfectTime = 0;

    //Phrases for each button
    List<string> content = new List<string>();
    int contentIndex = 0;

    // Score related
    public int currentPerfect = 0;
    public int totalPerfects;
    public int totalCools;
    public int totalMisses;
    public int coolScore = 5;
    public int perfectScore = 10;

    //
    public bool optionClicked = false;

    #region R4
    // Tracks whether the current thing to click is multiple choice group right now or not.
    public bool isMultipleChoice;
    private int correctChoiceCount = 0;
    private int incorrectChoiceCount = 0;
    #endregion

    // How many times buttons spawned
    public int getTotalSpawns()
    {
        return (totalCools + totalMisses + totalPerfects);
    }

    // Get the story of the event, split it, put it in the array, reset variables
    public void setContent(string eventDescription)
    {
        string[] contentSplit = eventDescription.Split(null);
        List<string> newContent = new List<string>(contentSplit);

        content.AddRange(newContent);
        contentIndex = 0;
        currentPerfect = 0;
        totalPerfects = 0;
        totalCools = 0;
        totalMisses = 0;
        optionClicked = false;

        resetChoiceCounts();
    }

    // empty the content
    public void removeContent()
    {
        content = new List<string>();
    }

    // Score for miss
    // It doesn't reset Perfect combo because
    public void ButtonMissed()
    {
        totalMisses++;
    }

    // Score for perfect
    public void ButtonScoredPerfect()
    {
        totalPerfects++;

        //Increment perfect combo
        currentPerfect++;

        addScore(perfectScore * currentPerfect);
    }

    // Score for cool
    public void ButtonScoredCool()
    {
        totalCools++;

        addScore(coolScore);

        // Reset perfect combo
        currentPerfect = 0;
    }

    // Update is called once per frame
    // Gotta put this logic into Update() with the way this works
    void Update()
    {
        if (audioSource.isPlaying)
        {
            float playTime = audioSource.time;

            //Figure out if the song just looped
            if (playTime < lastPlaytime)
            {
                loopTime++;
            }

            lastPlaytime = playTime;

            //Display the score
            score.text = iScore.ToString();

            // Stop the game if
            if (loopTime > 4)
            {
                // put down the volume, remove everything, show the okay button, show score
                audioSource.volume = 0.6f;

                RemoveSpawnedButtons();

                removeContent();
                okayButton.gameObject.SetActive(true);
                score.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                // for what
                float length = audioSource.clip.length;

                //Debug.Log("event index " + eventIndex);
                float currentEvent = eventTimes[eventIndex];

                // if it's getting near the current event time, removed spawned buttons and spawn new ones
                if ((playTime > (currentEvent - timing_before)) && (playTime < currentEvent))
                {
                    RemoveSpawnedButtons();

                    currentPerfectTime = currentEvent;

                    // move to next event in the song (not next content)
                    if (eventIndex >= (eventTimes.Length - 1))
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
    void RemoveSpawnedButtons()
    {
        GameObject[] spawned = GameObject.FindGameObjectsWithTag("SpawnedButton");
        if (spawned.Length > 0)
        {
            foreach (GameObject eachSpawned in spawned)
            {
                Destroy(eachSpawned);
            }
        }
    }

    // Also does what it says
    void SpawnButtons()
    {
        //Reset this attribute. Means that no button is clicked yet
        optionClicked = false;

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
        if (content[contentIndex].Contains("/"))
        {
            string[] options = content[contentIndex].Split('/');

            // first choice is correct
            realChoices.Add(options[0]);

            // other choices are incorrect
            foreach (string option in options)
            {
                if (!realChoices.Contains(option))
                {
                    fakeContent.Add(option);
                }
            }

            // have to change the currentPerfectTime and increment the event index, since if it's a multiple-choice it lasts twice as long
            currentPerfectTime = eventTimes[eventIndex];
            
            if (eventIndex >= (eventTimes.Length - 1))
            {
                eventIndex = 0;
            } else
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
        if (numFakes > 0)
        {
            for (int i = 1; i <= numFakes; i++)
            {
                // fake choices also have randomized positions, but has to be far enough from all the previous choices
                bool farEnough = false;
                int fakeRdX, fakeRdY;
                Vector3 fakePosition = new Vector3(0,0,0);
                
                while (!farEnough)
                {
                    farEnough = true;

                    fakeRdX = rd.Next(-380, 380);
                    fakeRdY = rd.Next(-180, 180);
                    fakePosition = new Vector3(fakeRdX, fakeRdY, 0);

                    foreach (Vector3 eachPos in buttonPositions)
                    {
                        farEnough &= (Vector3.Distance(eachPos, fakePosition) > 280);
                    }
                }

                // if it's far enough from all other positions, add it to the positions list
                buttonPositions.Add(fakePosition);
            }
        }

        // Create all buttons
        for (int i = 0; i < buttonPositions.Count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(gameObject.transform);
            newButton.transform.localPosition = buttonPositions[i];
            newButton.SetActive(true);
            newButton.tag = "SpawnedButton";
            // the button keeps track of its parent
            newButton.GetComponent<MusicButton>().SetRhythmDemo(this);

            // real choice
            if (i < numReals)
            {
                newButton.GetComponent<MusicButton>().SetTerm(realChoices[i], true);
                // this is only used for the first button
                GetComponentInChildren<TutorialController>().setButton(newButton.GetComponent<MusicButton>());
                Debug.Log("Spawned " + realChoices[i]);
            }
            // fake choice
            else
            {
                int next = i - numReals;
                newButton.GetComponent<MusicButton>().SetTerm(fakeContent[next], false);
                Debug.Log("Spawned " + fakeContent[next]);
            }

            isMultipleChoice = fakeContent.Count > 0;


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
        foreach (GameObject clone in GameObject.FindGameObjectsWithTag("Clone"))
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
        foreach (GameObject clone in GameObject.FindGameObjectsWithTag("Clone"))
        {
            Destroy(clone);
        }

        // set up more shitty particle effects
        foreach (UIParticleSystem flash in flashes)
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
        float startTime = audioSource.time;
        float totalTime = result_duration;
        missSound.Play();
        float perc = (audioSource.time - startTime) / totalTime;
        while (perc < 1)
        {
            Vector3 currentScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1), perc);
            clickResult.gameObject.transform.localScale = currentScale;
            perc = (audioSource.time - startTime) / totalTime;
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
        if (currentPerfect > 1)
        {
            target += "<color=\"red\">x" + currentPerfect.ToString();
        }

        // Higher the combo, higher the pitch of the sound
        float newPitch = Mathf.Clamp(1 + (0.06f * (currentPerfect - 1)), 1, 1.8f);
        perfectSound.pitch = newPitch;

        clickResult.text = target;
        clickResult.color = new Color(1, 0, 0.59f);

        // It also lerps
        float startTime = audioSource.time;
        float totalTime = result_duration;
        perfectSound.Play();
        float perc = (audioSource.time - startTime) / totalTime;

        while (perc < 1)
        {
            Vector3 currentScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1), perc);
            clickResult.gameObject.transform.localScale = currentScale;

            explosion.color = Color.Lerp(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 1), perc);
            explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(1.5f, 1.5f, 1.5f), perc);
            //explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1.5f), perc);

            perc = (audioSource.time - startTime) / totalTime;
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
        float startTime = audioSource.time;
        float totalTime = result_duration;
        coolSound.Play();
        float perc = (audioSource.time - startTime) / totalTime;

        while (perc < 1)
        {
            Vector3 currentScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), perc);
            clickResult.gameObject.transform.localScale = currentScale;

            explosion.color = Color.Lerp(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 1), perc);
            explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(1.5f, 1.5f, 1.5f), perc);
            //explosion.gameObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1.5f, 1.5f, 1.5f), perc);

            perc = (audioSource.time - startTime) / totalTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        explosion.gameObject.SetActive(false);
        clickResult.gameObject.SetActive(false);
    }

    // Add the clicked text to the constructed story
    public void updateAction(string newToken, bool isReal)
    {
        string[] stringSplit = actionString.Trim().Split(null);

        if (!(stringSplit.Length == content.Count))
        {
            actionString += newToken + " ";
            if (isReal)
            {
                actionText.text += newToken + " ";
            }
            else
            {
                actionText.text += "<color=\"red\">" + newToken + "</color> ";
            }
        }

        if (contentIndex < (content.Count - 1))
        {
            contentIndex++;
        }
        else
        {
            contentIndex = 0;
        }
    }

    #region r4
    /// <summary>
    /// Handle adding clicked text to the constructed story - individual button words and
    /// words that were part of choices.
    /// Wrong choices show as red. Correct choices show as green.
    /// Also update the UI to show how many user got wrong and how many got right.
    /// </summary>
    /// <param name="newToken">String to be added</param>
    /// <param name="isReal">Whether the choice is correct or not</param>
    /// <param name="wasChoice">Whether the text is part of a group</param>
    public void updateActionWithChoice(string newToken, bool isReal, bool wasChoice)
    {
        string[] stringSplit = actionString.Trim().Split(null);

        if(!(stringSplit.Length == content.Count))
        {
            actionString += newToken + " ";
            if(isReal)
            {
                if(wasChoice)
                {
                    actionText.text += "<color=\"blue\">" + newToken + "</color> ";
                    setCorrectChoiceCount(correctChoiceCount + 1);
                }
                else
                {
                    actionText.text += newToken + " ";
                }
            }
            else
            {
                actionText.text += "<color=\"red\">" + newToken + "</color> ";
                setIncorrectChoiceCount(incorrectChoiceCount + 1);
            }
            updateChoiceCountUI();
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
    #endregion

    // Add delta to current player score
    public void addScore(int delta)
    {
        StartCoroutine(LerpScore(delta));
    }

    // It has to lerp
    IEnumerator LerpScore(int delta)
    {
        float startTime = audioSource.time;

        float length = 0;

        int oldScore = iScore;
        int targetScore = iScore + delta;

        score.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        while (length < score_lerp_duration)
        {
            length = audioSource.time - startTime;
            float perc = length / score_lerp_duration;
            int currentScore = (int)Mathf.Lerp(oldScore, targetScore, perc);
            iScore = currentScore;

            score.gameObject.transform.localScale = Vector3.Lerp(new Vector3(1.5f, 1.5f, 1), new Vector3(2, 2, 1), perc);
            yield return null;
        }

        iScore = targetScore;

        yield return new WaitForSeconds(0.5f);
        score.gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    // Stop song, reset some variables, show main menu
    public void stopGame()
    {
        gameObject.SetActive(false);
        loopTime = 0;
        audioSource.Stop();
        GetComponentInParent<RhythmGame>().updateHighScore(iScore);
        GetComponentInParent<RhythmGame>().showPregame();
    }

    // Get the story, move the score to a hardcoded position on the corner (because I'm lazy), activate the gameplay, play music
    
public void startGame(RhythmCard card)
    {
        iScore = 0;

        // reset the correct and incorrect choice counters
        resetChoiceCounts();

        targetActionString = card.getDescription();
        actionString = "";
        actionText.text = "";

        score.gameObject.transform.localPosition = new Vector3(-547, 288, 0);

        okayButton.gameObject.SetActive(false);

        gameObject.SetActive(true);

        setContent(targetActionString);

        audioSource.volume = 1;
        audioSource.Play();

    }

    #region r4
    /// <summary>
    /// Set the correct choice counter
    /// </summary>
    /// <param name="newCount">The new count of correct choices</param>
    private void setCorrectChoiceCount(int newCount)
    {
        correctChoiceCount = newCount;
    }

    /// <summary>
    /// Set the correct choice counter
    /// </summary>
    /// <param name="newCount">The new count of incorrect choices</param>
    private void setIncorrectChoiceCount(int newCount)
    {
        incorrectChoiceCount = newCount;
    }

    /// <summary>
    /// Make the Correct/Incorrect choice counter UI show the current counts
    /// </summary>
    /// <param name="newCount">The count of how many to set</param>
    private void updateChoiceCountUI()
    {
        GameObject.FindGameObjectWithTag("CorrectUI").GetComponent<TextMeshProUGUI>().text =
            "Correct: " + correctChoiceCount;
        GameObject.FindGameObjectWithTag("IncorrectUI").GetComponent<TextMeshProUGUI>().text =
            "Incorrect: " + incorrectChoiceCount;
    }
   
    /// <summary>
    /// Set the correct incorrect choice counters to 0.
    /// </summary>
    public void resetChoiceCounts()
    {
        correctChoiceCount = 0;
        incorrectChoiceCount = 0;
    }
    #endregion
}
