using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class GameController : MonoBehaviour
{
    [SerializeField] private EventReference gameOverSound, winSound;
    private Music music;
    private int lives = 0;
    [SerializeField] private Transform[] lifesFX;
    [SerializeField] private Transform gameOverScreen, playScreen, winScreen;
    private bool isGameOver, isGameStarted = false;
    public bool gameActive = false;

    private void Start()
    {
        gameActive = false;
        gameOverScreen.gameObject.SetActive(false);
        playScreen.gameObject.SetActive(true);
        winScreen.gameObject.SetActive(false);

        music = FindObjectOfType<Music>();
        if (music == null)
        {
            Debug.LogError("GameController: Music component not found in the scene! Cannot subscribe to OnWin.");
            enabled = false; // Disable this script if Music isn't found
            return;
        }

        lives = lifesFX.Length;
        music.OnWin += winGG;
        Debug.Log("GameController: Subscribed to Music.OnWin event.");
    }

    private void Update()
    {
        // Restart logic
        if ((isGameOver || winScreen.gameObject.activeSelf) && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("GameController: Restarting scene.");
            Bus masterBus = RuntimeManager.GetBus("bus:/");
            masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        // Start game logic
        if (!isGameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("GameController: Starting game and music.");
            playScreen.gameObject.SetActive(false);
            music.StartMusic();
            isGameStarted = true;
            gameActive = true;
        }
    }

    public void loseLive()
    {
        if (lives > 0)
        {
            int index = lives - 1;
            if (index >= 0 && index < lifesFX.Length && lifesFX[index] != null) // Safety check
            {
                lifesFX[index].gameObject.SetActive(false);
                Debug.Log($"GameController: Lost a life. Remaining lives: {lives - 1}");
            }
            lives--;
        }
        else
        {
            gameOver();
        }
    }

    private void gameOver()
    {
        Debug.Log("GameController: Game Over initiated.");
        gameActive = false;

        if (music != null)
        {
            music.stopAndClear();
        }
        else
        {
            Debug.LogWarning("GameController: Music reference is null during gameOver. Cannot stop music.");
        }

        RuntimeManager.PlayOneShot(gameOverSound);

        if (gameOverScreen != null) // Safety check
        {
            gameOverScreen.gameObject.SetActive(true);
            Debug.Log("GameController: Game Over screen activated.");
        }
        else
        {
            Debug.LogError("GameController: Game Over Screen Transform is not assigned or is null!");
        }
        isGameOver = true;
    }

    private void winGG(bool b)
    {
        // THIS IS THE DEBUG.LOG YOU WERE LOOKING FOR. If it doesn't appear, the problem is before here.
        Debug.Log("GameController: winGG() method called. Setting win screen active.");
        gameActive = false;

        if (winScreen != null) // Safety check
        {
            winScreen.gameObject.SetActive(true);
            Debug.Log("GameController: Win screen activated.");
        }
        else
        {
            Debug.LogError("GameController: Win Screen Transform is not assigned or is null!");
        }

        RuntimeManager.PlayOneShot(winSound);
        Debug.Log("GameController: winGG() finished.");
    }

    private void OnDestroy()
    {
        if (music != null)
        {
            music.OnWin -= winGG;
            Debug.Log("GameController: Unsubscribed from Music.OnWin in OnDestroy.");
        }
    }
}