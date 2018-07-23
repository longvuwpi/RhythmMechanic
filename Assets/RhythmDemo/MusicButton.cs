using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicButton : MonoBehaviour {
    RhythmDemo rhythmDemo;
    AudioSource audioSource;
    TextMeshProUGUI clickResult;
    GameObject newBoundingBox;

    Vector3 originalLocation;

    System.Random rd = new System.Random();

    bool missed = false;

    //Hard mode
    //float[] eventTime = new float[] { 0.744f, 1.5f, 2.222f, 2.993f, 3.743f, 4.513f, 5.247f, 5.993f, 6.743f, 7.5f, 8.246f, 8.981f, 9.762f, 10.48f, 11.215f };
    //float timing_before = 0.4f;
    //float timing_after = 0.2f;
    //float perfect_window_half = 0.04f;
    //float result_duration = 0.05f;

    bool isRealOption = false;

    public void SetRhythmDemo(RhythmDemo newGameplayController)
    {
        rhythmDemo = newGameplayController;
        audioSource = rhythmDemo.audioSource;
        clickResult = rhythmDemo.clickResult;
    }

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }

    public void StartAnimation()
    {
        StartCoroutine(flashButton());
    }

    // animate things
    IEnumerator flashButton()
    {
        // instantiate a new bounding box
        newBoundingBox = Instantiate(rhythmDemo.boundingBox);
        newBoundingBox.transform.SetParent(gameObject.transform.parent);
        newBoundingBox.transform.SetAsFirstSibling();
        newBoundingBox.tag = "SpawnedButton";
        newBoundingBox.SetActive(true);


        float startTime = rhythmDemo.audioSource.time;

        Vector3 buttonScale = new Vector3(3, 3, 1);
        gameObject.transform.localScale = buttonScale;

        // bounding box lerps from scale (7,7,1) to scale of button
        Vector3 boundingScale = new Vector3(7, 7, 1);
        // position of bounding box is randomized
        int rdBoxX = rd.Next(-200, 200);
        int rdBoxY = rd.Next(-200, 200);
        // offset from the button's position
        Vector3 boundingPos = gameObject.transform.localPosition + (new Vector3(rdBoxX, rdBoxY, 0));

        float currentPerfectTime = rhythmDemo.currentPerfectTime;
        float animDuration = 0;
        float currentAnimDuration = 0;
        float timeUntilPerfect = 0;
        float clipLength = rhythmDemo.audioSource.clip.length;

        if (currentPerfectTime > startTime) 
        {
            timeUntilPerfect = currentPerfectTime - startTime;
        }
        else // if the song just looped
        {
            timeUntilPerfect = clipLength - startTime + currentPerfectTime;
        }
        animDuration = timeUntilPerfect + rhythmDemo.timing_after;

        float lastUpdate = startTime;

        // bounding box gets smaller and smaller until it's same size as the button. also moves until it's where the button is
        while (currentAnimDuration < animDuration)
        {
            float perc = currentAnimDuration / timeUntilPerfect;
            Vector3 currentScale = Vector3.Lerp(boundingScale, buttonScale, perc);
            Color currentColor = Color.Lerp(Color.black, Color.green, perc);
            Vector3 currentPos = Vector3.Lerp(boundingPos, gameObject.transform.localPosition, perc);

            newBoundingBox.transform.localScale = currentScale;
            newBoundingBox.GetComponent<Image>().color = currentColor;
            newBoundingBox.transform.localPosition = currentPos;

            float currentUpdate = rhythmDemo.audioSource.time;
            if (currentUpdate >= lastUpdate)
            {
                currentAnimDuration += (currentUpdate - lastUpdate);
            } else {
                currentAnimDuration += currentUpdate + (clipLength - lastUpdate);
            }
            lastUpdate = currentUpdate;

            yield return null;
        }

        // when it finishes moving, if nothing is clicked yet then it's a miss
        if (!rhythmDemo.optionClicked)
        {
            missed = true;
            StartCoroutine(FadeButton(false));
            rhythmDemo.Miss(this);
        } else // if something was clicked then not a miss
        {
            DestroyBox();
            Destroy(this.gameObject);
        }
    }

    // set the word for the button and whether it's a correct choice
    public void SetTerm(string newTerm, bool isReal)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = newTerm;
        isRealOption = isReal;
    }

    // called when clicked on
    public void OnMouseDown()
    {
        // only counts if nothing is clicked on yet
        if ((!rhythmDemo.optionClicked) && (!missed))
        {
            rhythmDemo.optionClicked = true;

            float clickTime = rhythmDemo.audioSource.time;
            float currentPerfectTime = rhythmDemo.currentPerfectTime;

            // put the word on the screen
            //rhythmDemo.updateAction(GetComponentInChildren<TextMeshProUGUI>().text, isRealOption);

            rhythmDemo.updateActionWithChoice(GetComponentInChildren<TextMeshProUGUI>().text, isRealOption, rhythmDemo.isMultipleChoice);

            // Figure out timing, whether it's cool or perfect
            if (((currentPerfectTime - rhythmDemo.perfect_window_half) <= clickTime) && (clickTime <= (currentPerfectTime + rhythmDemo.perfect_window_half)))
            {
                rhythmDemo.Perfect(this);
                StopAllCoroutines();
                StartCoroutine(FadeButton(true));
            }
            else
            {
                rhythmDemo.Cool(this);
                StopAllCoroutines();
                StartCoroutine(FadeButton(false));
            }
        }
    }

    // Animate button when clicked
    IEnumerator FadeButton(bool isPerfect)
    {
        float clickTime = rhythmDemo.audioSource.time;
        float duration = rhythmDemo.button_drift_duration;
        float currentDuration = 0;
        float deltaScale = 0;

        // if it's perfect, bounding box snaps to button
        if (isPerfect)
        {
            newBoundingBox.transform.localScale = gameObject.transform.localScale;
            deltaScale = 0.15f;
        }
        else
        {
            deltaScale = 0.03f;
        }

        float currentAlpha = 1;

        while (currentDuration < duration)
        {
            //Adjust clones
            foreach (GameObject clone in GameObject.FindGameObjectsWithTag("Clone"))
            {
                clone.transform.localPosition = gameObject.transform.localPosition;
            }

            //Adjust button itself
            gameObject.transform.localPosition += new Vector3(1.5f, 0, 0);
            gameObject.transform.localScale += new Vector3(deltaScale, deltaScale, 0);

            //Adjust background color of button
            Color currentColor = GetComponent<Image>().color;
            GetComponent<Image>().color = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);

            //Adjust color and position of bounding box
            Color currentBoundingColor = newBoundingBox.GetComponent<Image>().color;
            newBoundingBox.GetComponent<Image>().color = new Color(currentBoundingColor.r, currentBoundingColor.g, currentBoundingColor.b, currentAlpha);
            newBoundingBox.transform.localPosition = gameObject.transform.localPosition;
            newBoundingBox.transform.localScale += new Vector3(deltaScale, deltaScale, 0);

            //Adjust button text color
            Color currentTextColor = GetComponentInChildren<TextMeshProUGUI>().color;
            //GetComponentInChildren<TextMeshProUGUI>().color = new Color(currentTextColor.r, currentTextColor.g, currentTextColor.b, currentAlpha);

            currentDuration = (rhythmDemo.audioSource.time - clickTime);
            currentAlpha -= 0.02f;
            yield return null;
        }

        DestroyBox();
        Destroy(this.gameObject);
    }

    public void DestroyBox()
    {
        Destroy(newBoundingBox);
    }
}
