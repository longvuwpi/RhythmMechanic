using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Initiates the slowdown effect during the tutorial.
/// </summary>
public class TutorialController : MonoBehaviour
{
    [SerializeField]
    private MusicButton musicButton;

    [SerializeField]
    private Text description;

    [SerializeField]
    private Image arrow;

    [SerializeField]
    private AudioSource musicSource;

    private float desiredPitch = 1f;

    private void Start()
    {
        description.gameObject.SetActive(false);
        arrow.gameObject.SetActive(false);
    }

    public void SetButton(MusicButton firstButton)
    {
        musicButton = firstButton;
    }

    private void Update()
    {
        // Figure out the right time for the first button
        if(musicSource.isPlaying)
        {
            if(GetComponentInParent<RhythmDemo>().GetTotalSpawns() == 0)
            {
                if(musicSource.time > (GetComponentInParent<RhythmDemo>().TimeEvents[0] - 0.4f))
                {
                    if(musicButton != null)
                    {
                        StartCoroutine(ShowTutorial());
                    }
                }
            }
            else
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

        float timing = GetComponentInParent<RhythmDemo>().TimeEvents[0];
        while(musicSource.time < timing)
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

        while(musicSource.pitch < desiredPitch)
        {
            musicSource.pitch += 0.1f;
            yield return null;
        }

        //Time.timeScale = desiredPitch;

        musicSource.pitch = desiredPitch;
    }
}
