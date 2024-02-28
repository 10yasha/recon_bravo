using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour{
    public string currentScene; // to return to this screen if reset

    public GameObject loseScreen;
    public GameObject winScreen;
    bool gameIsGG;

    void Start(){
        Guard.OnPlayerSpotted += ShowLose;
        FindObjectOfType<Player>().OnLevelClear += ShowWin;
    }

    void Update(){
        if (gameIsGG){
            if (Input.GetKeyDown(KeyCode.Space)){
                gameIsGG = false; // might be unnecessary?
                SceneManager.LoadScene(currentScene);
            }
            if (Input.GetKeyDown(KeyCode.Q)){
                gameIsGG = false;
                SceneManager.LoadScene(0);
            }
        }
    }

    void ShowLose(){
        OnGG(loseScreen);
    }

    void ShowWin(){
        OnGG(winScreen);
    }

    void OnGG(GameObject resultantUI){
        gameIsGG = true;
        Guard.OnPlayerSpotted -= ShowLose;
        FindObjectOfType<Player>().OnLevelClear -= ShowWin;

        resultantUI.SetActive(true); // show endgame screen
    }
}
