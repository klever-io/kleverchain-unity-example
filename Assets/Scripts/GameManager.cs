using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    private int score;

    public GameObject playButton;
    public GameObject gameOver;
    public Player player;
    public Text scoreText;

    private void Awake(){
        Application.targetFrameRate = 60;

        Pause();
    }

    private void updateScore(int newScore){
        score = newScore;
        scoreText.text = score.ToString();
    }

    public void Play(){
       updateScore(0);

        gameOver.SetActive(false);
        playButton.SetActive(false);

        Time.timeScale = 1f;
        player.enabled = true;

        Pipes[] pipes = FindObjectsOfType<Pipes>();
        
        for( int i = 0; i < pipes.Length; i++){
            Destroy(pipes[i].gameObject);
        }
    }

    public void Pause(){
        Time.timeScale = 0f;
        player.enabled = false;
    }

    public void GameOver(){
        gameOver.SetActive(true);
        playButton.SetActive(true);

        Pause();
    }

    public void IncreaseScore(){
        updateScore(score++);
    }


}
