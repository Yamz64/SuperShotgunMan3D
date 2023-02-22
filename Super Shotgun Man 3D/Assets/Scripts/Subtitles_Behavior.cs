using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Subtitles_Behavior : MonoBehaviour
{
    TMP_Text sub_text;              //Text to show subtitles in
    Image text_background;          //Background to show text on

    public string cutscene_name;    //Name of the cutscene the subtitles need to sync with

    public bool cutscene;                  //Whether the subtitles are shown in a cutscene

    List<string> script;            //Container to hold the text

    float timer;                    //Used to track current time in cutscene


    void Awake()
    {
        timer = 0;

        //Grab subtitle objects
        GameObject subtitles = this.gameObject; 
        sub_text = subtitles.transform.GetChild(1).GetComponent<TMP_Text>();
        text_background = subtitles.transform.GetChild(0).GetComponent<Image>();

        //Define objects
        script = new List<string>();
        StreamReader input_stream;

        if (cutscene)
        {
            //Grab the subtitles from the relevant file in Resources depending on the scene
            switch (cutscene_name)
            {
                case ("Opening"):
                    input_stream = new StreamReader("Assets/Resources/Subtitle_Text/Cutscenes/OpeningCutscene.txt");
                    break;
                default:
                    input_stream = new StreamReader("");
                    break;
            }

            //Read each line into the subtitles list
            while (!input_stream.EndOfStream)
            {
                string input_line = input_stream.ReadLine();
                script.Add(input_line);
            }

            input_stream.Close();
        }

        //On = Subtitles display, Off = They don't
        string option = PlayerPrefs.GetString("Subtitles", "Off");

        //If this is for a cutscene, display the text
        if (cutscene_name.Length > 0 && option.Equals("On"))
        {
            StartCoroutine(displaySubtitles());
        }
    }

    //Display each line in the subtitle script, with text changes at predetermined points
    //Read from file defined in start/awake function
    //Used for cutscenes
    public IEnumerator displaySubtitles()
    {
        sub_text.gameObject.SetActive(true);
        //text_background.gameObject.SetActive(true);
        for (int i = 0; i < script.Count; i++)
        {
            //Strings to split main string around
            string[] seps = { "---", "_" };

            //Container to hold substrings
            string[] subs = script[i].Split(seps, System.StringSplitOptions.RemoveEmptyEntries);

            //Text to display in subtitles
            sub_text.text = subs[2];

            //Get starting time
            int t1_0;                                       
            string t1_0s = "" + subs[0][0] + subs[0][1];
            int.TryParse(t1_0s, out t1_0);
            int t2_0;
            string t2_0s = "" + subs[0][3] + subs[0][4];
            int.TryParse(t2_0s, out t2_0);
            int t3_0;
            string t3_0s = "" + subs[0][6] + subs[0][7];
            int.TryParse(t3_0s, out t3_0);
            int time_1 = (t1_0 * 3600) + (t2_0 * 60) + t3_0;

            //Get ending time
            int t1_1;
            string t1_1s = "" + subs[1][0] + subs[1][1];
            int.TryParse(t1_1s, out t1_1);
            int t2_1;
            string t2_1s = "" + subs[1][3] + subs[1][4];
            int.TryParse(t2_1s, out t2_1);
            int t3_1;
            string t3_1s = "" + subs[1][6] + subs[1][7];
            int.TryParse(t3_1s, out t3_1);
            int time_2 = (t1_1 * 3600) + (t2_1 * 60) + t3_1;

            //Debug.Log("Time to wait == " + time_2 + " - " + time_1 + " = " + (time_2 - time_1));

            //Display the text for this amount of seconds
            yield return new WaitForSeconds(time_2 - time_1);
        }
        yield return new WaitForSeconds(1.0f);
    }

    //Overload version, display input text for input time
    public IEnumerator displaySubtitles(float time, string text)
    {
        sub_text.gameObject.SetActive(true);
        sub_text.text = text;
        Debug.Log("Time == " + time);
        yield return new WaitForSeconds(time);
        Debug.Log("Turning text off");
        sub_text.text = "";
        Debug.Log("Text has been turned off");
        yield return new WaitForSeconds(0.1f);
        sub_text.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //Display current time for subtitle testing
        timer += Time.deltaTime;
        //Debug.Log(timer);
    }
}
