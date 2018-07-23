using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Makes the slowing down at first happen
public class TutorialController : MonoBehaviour {
    public MusicButton musicButton;
    public Text description;
    public Image arrow;
    public AudioSource musicSource;

    float desiredPitch = 1f;

	void Start () {
        description.gameObject.SetActive(false);
        arrow.gameObject.SetActive(false);
	}
	
    public void setButton(MusicButton firstButton)
    {
        musicButton = firstButton;
    }

	// Update is called once per frame
	void Update () {
        // Figure out the right time for the first button
        if (musicSource.isPlaying)
        {
            if (GetComponentInParent<RhythmDemo>().getTotalSpawns() == 0)
            {
                if (musicSource.time > (GetComponentInParent<RhythmDemo>().eventTimes[0]-0.4f))
                {
                    if (musicButton != null)
                    {
                        StartCoroutine(ShowTutorial());
                    }
                }
            } else
            {
                StopAllCoroutines();
                StartCoroutine(StopTutorial());
            }
        }
	}

    // Slow the pitch down
    IEnumerator ShowTutorial()
    {
        musicSource.pitch = desiredPitch;

        description.gameObject.SetActive(true);
        arrow.gameObject.SetActive(true);
        gameObject.transform.localPosition = musicButton.transform.localPosition;

        float timing = GetComponentInParent<RhythmDemo>().eventTimes[0];
        while (musicSource.time < timing)
        {
            float perc = Mathf.Pow((musicSource.time / timing), 4);
            musicSource.pitch = Mathf.Lerp(desiredPitch, 0.000001f, perc);
            yield return null;
        }
    }

    // Get the pitch back to normal
    IEnumerator StopTutorial()
    {
        description.gameObject.SetActive(false);
        arrow.gameObject.SetActive(false);

        while (musicSource.pitch < desiredPitch)
        {
            musicSource.pitch += 0.1f;
            yield return null;
        }

        //Time.timeScale = desiredPitch;

        musicSource.pitch = desiredPitch;
    }
}
