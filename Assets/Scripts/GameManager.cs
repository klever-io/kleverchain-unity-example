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

    // Create the Transfer transaction with a message
    async private Task<kleversdk.provider.Dto.Transaction> Transfer(string fromAddress, string toAddress, float amount, long nonce, string kda, string message){
        long precision = 6;
        bool isNFT = false;
        if (kda.Contains("/")) {
            isNFT = true;
            precision = 0;
        }

        if (!isNFT && kda.Length > 0 && kda != "KLV" && kda != "KFI")
        {
            try
            {
                var asset = await kleverProvider.GetAsset(kda);
                precision = asset.Precision;
            }catch(Exception e)
            {
                throw e;
            }
        }

        List<IContract> contracts = new List<IContract>();

        long parsedAmount = Convert.ToInt64(amount * (Math.Pow(10, precision)));
        contracts.Add(new TransferContract(toAddress, parsedAmount, kda));

        var encondedMessage = this.EncodeMessage(message);

        // Build Transaction
            var data = new SendRequest
            {
                Type = 0,
                Sender = fromAddress,
                Nonce = nonce,
                Data = encondedMessage,
                Contracts = contracts,
            };

        return await kleverProvider.PrepareTransaction(data);
    }

    // Encode Message
    private byte[][] EncodeMessage(string message){

        byte[][] encodedMessage = new byte[1][];

        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(message);;

        encodedMessage[0] = bytes;

        return encodedMessage;
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

        var tx = await this.Transfer(acc.Address.Bech32,toAddress,0.000001f,acc.Nonce,"KLV",message);    
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
