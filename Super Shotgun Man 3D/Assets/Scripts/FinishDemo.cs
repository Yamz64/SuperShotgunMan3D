using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishDemo : MonoBehaviour
{
    bool started_transition;

    IEnumerator GoToEnd()
    {
        PlayerStats stats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        yield return new WaitUntil(() => stats.FadeFinished == false);
        yield return new WaitUntil(() => stats.FadeFinished == true);
        SceneManager.LoadScene("EndOfDemo");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        if (started_transition)
            return;

        other.GetComponent<PlayerStats>().ToggleFade();
        StartCoroutine(GoToEnd());
        started_transition = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player")
            return;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
    }
}
