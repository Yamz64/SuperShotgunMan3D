using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneSequence : MonoBehaviour
{
    public float skip_text_stay_duration, skip_text_fade_duration;
    public string transition_scene;
    private float m_skip_text_fade_duration;
    private bool faded, skip = false;
    private VideoPlayer player;
    private RawImage image;
    private Text skip_text;

    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => player.isPlaying || skip);
        yield return new WaitUntil(() => !player.isPlaying || skip);
        StartCoroutine(Fade());
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => faded);
        SceneManager.LoadScene(transition_scene);
    }

    IEnumerator Fade(float duration = 1)
    {
        //tween it's alpha value
        float progress = 0;
        while (progress < duration)
        {
            progress += 1 / 24f;
            yield return new WaitForSeconds(1 / 24f);
            image.color = Color.Lerp(new Color(image.color.r, image.color.g, image.color.b, 1.0f), Color.black, progress / duration);
        }
        faded = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_skip_text_fade_duration = skip_text_fade_duration;
        player = GetComponent<VideoPlayer>();
        image = GetComponent<RawImage>();
        skip_text = transform.GetChild(0).GetComponent<Text>();
        StartCoroutine(LateStart());
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (!skip)
            {
                skip = true;
                if (skip_text_fade_duration > 0.0f)
                {
                    m_skip_text_fade_duration = 1.0f;
                    skip_text_fade_duration = 1.0f;
                }
            }
        }
        if(skip_text_stay_duration > 0.0f && !skip)
            skip_text_stay_duration -= Time.deltaTime;
        else
        {
            if (skip_text_fade_duration > 0.0f)
                skip_text_fade_duration -= Time.deltaTime;
            else
                skip_text_fade_duration = 0.0f;
            
            skip_text.color = Color.Lerp(new Color(1.0f, 1.0f, 1.0f, 0.0f), Color.white, skip_text_fade_duration / m_skip_text_fade_duration);
        }
    }
}
