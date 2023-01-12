using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinishGame : MonoBehaviour
{
    public float fade_vignette_increment;
    private bool fade_direction, finished_fade;
    private Text final_time_text;
    private Image fade_vignette;

    public void ToggleFade() { fade_direction = !fade_direction; }

    IEnumerator RestartGame()
    {
        fade_direction = true;
        yield return new WaitUntil(() => finished_fade == false);
        yield return new WaitUntil(() => finished_fade == true);
        SceneManager.LoadScene("Level1");
    }

    void Actions()
    {
        if (!finished_fade)
            return;

        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(RestartGame());

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    void Fade()
    {
        //fade in
        if (!fade_direction)
        {
            if (fade_vignette.color.a == 0.0f)
            {
                finished_fade = true;
                return;
            }
            float progress = fade_vignette.color.a;
            if (progress > 0.0f)
                progress -= Time.deltaTime * fade_vignette_increment;
            else
                progress = 0.0f;
            fade_vignette.color = new Color(0.0f, 0.0f, 0.0f, progress);
            finished_fade = false;
        }
        //fade out
        else
        {
            if (fade_vignette.color.a == 1.0f)
            {
                finished_fade = true;
                return;
            }
            float progress = fade_vignette.color.a;
            if (progress < 1.0f)
                progress += Time.deltaTime * fade_vignette_increment;
            else
                progress = 1.0f;
            fade_vignette.color = new Color(0.0f, 0.0f, 0.0f, progress);
            finished_fade = false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        final_time_text = transform.GetChild(1).GetComponent<Text>();
        fade_vignette = transform.GetChild(3).GetComponent<Image>();

        float time = PlayerPrefs.GetFloat("Time", 0.0f);
        int time_hours = (int)(time / 3600.0f);
        int time_minutes = (int)((time - time_hours * 3600.0f) / 60.0f);
        int time_seconds = (int)(time % 60.0f);
        final_time_text.text = $"Your final time:\n{time_hours.ToString("00")}:{time_minutes.ToString("00")}:{time_seconds.ToString("00")}";
    }

    // Update is called once per frame
    void Update()
    {
        Fade();
        Actions();
    }
}
