using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioVictory : MonoBehaviour
{
    public AudioSource victory;
    // Start is called before the first frame update
    void Start()
    {
        victory.Play();
        Invoke("StartMenu", 5);
    }

    void StartMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
}
