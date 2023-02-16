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

    string cutscene_name;           //Name of the cutscene the subtitles need to sync with

    List<string> script;            //Container to hold the text


    // Start is called before the first frame update
    void Start()
    {
        //Grab subtitle objects
        GameObject subtitles = GameObject.FindGameObjectWithTag("Subtitles");
        sub_text = subtitles.transform.GetChild(1).GetComponent<TMP_Text>();
        text_background = subtitles.transform.GetChild(0).GetComponent<Image>();

        //Define objects
        script = new List<string>();
        StreamReader input_stream;

        //Grab the subtitles from the relevant file in Resources depending on the scene
        switch (cutscene_name)
        {
            case ("Opening"):
                input_stream = new StreamReader("Subtitle_Text/Cutscenes/OpeningCutscene.txt");
                break;
            default:
                input_stream = new StreamReader("Subtitle_Text/Cutscenes/OpeningCutscene.txt");
                break;
        }
        //Read each line into the subtitles list
        while (!input_stream.EndOfStream)
        {
            string input_line = input_stream.ReadLine();
            script.Add(input_line);
        }

        input_stream.Close();

        //If this is for a cutscene, display the text
        if (cutscene_name.Length > 0)
        {
            StartCoroutine(displaySubtitles());
        }
    }

    //Display each line in the subtitle script, with text changes at predetermined points
    //Used for cutscenes
    IEnumerator displaySubtitles()
    {
        yield return new WaitForSeconds(1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
