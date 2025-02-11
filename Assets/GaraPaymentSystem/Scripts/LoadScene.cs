using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{


    private void Start()
    {
    }

    public void Load_GaraPayment_scene()
    {
        //here load gara payment scene
        SceneManager.LoadScene("TestPayment");
    }

    public void BackToGame()
    {
        //here load your game scene, to make a feedback system
        SceneManager.LoadScene("Game");
    }

    public void Reload_Actual_scene()
    {
        //Current scene reloading system.  
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
