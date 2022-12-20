using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinishGame : MonoBehaviour
{
    public float fade_vignette_increment;
    private bool fade_direction, finished_fade;
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
        fade_vignette = transform.GetChild(2).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        Fade();
        Actions();
    }
}
