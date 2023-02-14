using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuBehavior : MonoBehaviour
{
    public bool paused;
    private GameObject pause_menu;
    private GameObject options_menu;

    void ActivatePauseMenu()
    {
        if (paused)
        {
            Time.timeScale = 0.0f;
            if (!pause_menu.activeInHierarchy)
            {
                pause_menu.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
        else
        {
            Time.timeScale = 1.0f;
            if (options_menu.activeInHierarchy)
                CloseOptionsMenu();
            if (pause_menu.activeInHierarchy)
            {
                pause_menu.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void OpenOptionsMenu()
    {
        //get every child that isn't the options menu and disable them
        for(int i=0; i<pause_menu.transform.childCount - 1; i++)
        {
            pause_menu.transform.GetChild(i).gameObject.SetActive(false);
        }

        //enable the options menu
        options_menu.SetActive(true);
    }

    public void CloseOptionsMenu()
    {
        //get every child that isn't the options menu and enable them
        for (int i = 0; i < pause_menu.transform.childCount - 1; i++)
        {
            pause_menu.transform.GetChild(i).gameObject.SetActive(true);
        }

        //disable the options menu
        options_menu.SetActive(false);
    }

    void SetupSliders()
    {
        //for each slider set their value to the last player preference position
        for(int i=0; i<3; i++)
        {
            Slider slider = options_menu.transform.GetChild(i).GetComponent<Slider>();

            switch (i)
            {
                case 0:
                    slider.value = PlayerPrefs.GetFloat("Mouse Sensitivity", 0.5f);
                    break;
                case 1:
                    slider.value = PlayerPrefs.GetFloat("Music Volume", 0.5f);
                    break;
                case 2:
                    slider.value = PlayerPrefs.GetFloat("SFX Volume", 0.5f);
                    break;
            }
            //update their text
            OnSliderUpdate(i);
        }
    }

    public void OnSliderUpdate(int slider_index)
    {
        if(slider_index < 0)
        {
            Debug.LogWarning("Slider index is less than 0, clamping!");
            slider_index = 0;
        }
        if(slider_index > 2)
        {
            Debug.LogWarning("Slider index is greater than the number of provided sliders, clamping!");
            slider_index = 2;
        }

        //get the current bar's value
        float slider_progress = options_menu.transform.GetChild(slider_index).GetComponent<Slider>().value;

        //update the progress bar's text value
        TextMeshProUGUI text_o = options_menu.transform.GetChild(slider_index).GetChild(1).GetComponent<TextMeshProUGUI>();

        //mouse sensitivity measured differently
        if (slider_index == 0)
        {
            if (slider_progress * 500.0f < 10)
                text_o.text = $"0{(int)(slider_progress * 500.0f)}";
            else
                text_o.text = $"{(int)(slider_progress * 500.0f)}";
        }
        else
        {
            if (slider_progress * 100 < 10)
                text_o.text = $"0{(int)(slider_progress * 100)}%";
            else
                text_o.text = $"{(int)(slider_progress * 100)}%";
        }

        //update the value in player preferences accordingly
        switch (slider_index)
        {
            case 0:
                PlayerPrefs.SetFloat("Mouse Sensitivity", slider_progress);
                break;
            case 1:
                PlayerPrefs.SetFloat("Music Volume", slider_progress);
                AudioUtils.UpdateMusicVolume();
                break;
            case 2:
                PlayerPrefs.SetFloat("SFX Volume", slider_progress);
                AudioUtils.UpdateSFXVolume();
                break;
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {
        pause_menu = transform.GetChild(6).gameObject;
        pause_menu.SetActive(false);
        options_menu = pause_menu.transform.GetChild(4).gameObject;
        options_menu.SetActive(false);

        SetupSliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<DebugController>().ShowConsole)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            paused = !paused;

        ActivatePauseMenu();
    }
}
