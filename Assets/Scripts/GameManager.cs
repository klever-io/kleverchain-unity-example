using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using kleversdk.core;
using kleversdk.provider;
using kleversdk.provider.Dto;



public class GameManager : MonoBehaviour
{

    // Blockchain Params

    // Add your private key and the address you want to send klv to test the game.
    private KleverProvider kleverProvider;
    private string privateKey = "";
    private string toAddress = "";



    // Game Params

    private string highScoreKey = "Highscore";
    private int score;
    public GameObject playButton;
    public GameObject sendButton;
    public GameObject gameOver;
    public Player player;
    public Text scoreText;
    public Text highScoreText;


    // Blockchain Methods

    // Derive Address from the private key
    private Wallet DeriveAddressByPK(){
        return new Wallet(privateKey);
    }

    // Send Transaction with Memo
    async public void Send(){
        var wallet = DeriveAddressByPK();
        Account acc =  wallet.GetAccount();
        try { 
            await acc.Sync(this.kleverProvider);
        }catch(Exception e) {
            Debug.Log(e.ToString());
        }

        string message = string.Format("Highscore of {0}: {1}",acc.Address.Bech32,highScoreText.text);

        var tx = await this.kleverProvider.SendWithMessage(acc.Address.Bech32,acc.Nonce,toAddress,0.000001f,message);
        var decoded = await this.kleverProvider.Decode(tx);
        var signature = wallet.SignHex(decoded.Hash);
        tx.AddSignature(signature);

        var broadcastResult = await this.kleverProvider.Broadcast(tx);
        
        Debug.Log(broadcastResult.String());
    }

    // Game Methods

    private void Awake(){
        Application.targetFrameRate = 60;

        // comment if you want keep the highscore.
        PlayerPrefs.DeleteAll();

        // need to instantiate the kleverProvider to use the kleverchain sdk
        kleverProvider = new KleverProvider(new NetworkConfig(kleversdk.provider.Network.TestNet));

        if (PlayerPrefs.HasKey(highScoreKey)){
            highScoreText.text = PlayerPrefs.GetString(highScoreKey);
        }    

        Pause();    
    }

    private void updateHighScore(string newScore){
        highScoreText.text = newScore;
        PlayerPrefs.SetString(highScoreKey,newScore);
    }

    private void updateScore(int newScore){
        score = newScore;
        scoreText.text = score.ToString();
    }

    public void Play(){
        updateScore(0);

        gameOver.SetActive(false);
        playButton.SetActive(false);
        scoreText.gameObject.SetActive(true);
        highScoreText.gameObject.SetActive(false);
        sendButton.SetActive(false);


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
    
        if (PlayerPrefs.HasKey(highScoreKey)){
            var highScore = PlayerPrefs.GetString(highScoreKey);
            if (int.Parse(scoreText.text) >  int.Parse(highScore)){
               sendButton.SetActive(true);
               updateHighScore(scoreText.text);
            }
        } else {
            PlayerPrefs.SetString(highScoreKey,scoreText.text);
        }



        gameOver.SetActive(true);
        playButton.SetActive(true);
        highScoreText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(false);

        Pause();
    }

    public void IncreaseScore(){
        updateScore(score + 1);
    }


}
