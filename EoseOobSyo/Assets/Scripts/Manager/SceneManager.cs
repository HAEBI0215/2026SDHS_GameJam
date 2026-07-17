using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public void Start()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGame");
    }
    
    public void Exit()
    {
        Application.Quit();
    }
}
