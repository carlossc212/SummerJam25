using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class GameController : MonoBehaviour
{
    [SerializeField] private EventReference gameOverSound;
    private Music music;
    private int lives = 0;
    [SerializeField] private Transform[] lifesFX;
    [SerializeField] private Transform gameOverScreen;
    [HideInInspector] public bool isGameOver = false;

    private void Start()
    {
        gameOverScreen.gameObject.SetActive(false);
        music = FindObjectOfType<Music>();
        lives = lifesFX.Length;
    }

    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Space)) {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
    }

    public void loseLive() {
        if (lives > 0)
        {
            int index = lives - 1;
            lifesFX[index].gameObject.SetActive(false);
            lives--;

        }
        else {
            gameOver();
        }
    }

    private void gameOver() {
        music.stopAndClear();
        RuntimeManager.PlayOneShot(gameOverSound);
        gameOverScreen.gameObject.SetActive(true);
        isGameOver = true;
    }

}
