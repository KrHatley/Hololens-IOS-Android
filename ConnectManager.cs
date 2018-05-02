using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Attached to camera as a manager script
/// Used to send and recieve data from the online data table, 
/// Used in junction with leaderboard classes
/// and seperate MVC with EF6 application
/// </summary>
public class ConnectManager : MonoBehaviour {

    public string playername;
    public string playeremail;
    public int playerscore;
    public UnityWebRequest webRequest;
    public DownloadHandlerBuffer DH;
    public byte[] nibbles;
    public WWWForm form;
    public bool isConnected;
    private PlayerManager playerManager;
    [SerializeField] public List<PlayerData> leaderboardScores;



    private void Awake()
    {
        DH = new DownloadHandlerBuffer(); 
        GetNameFromServer();
        playerManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerManager>();
        isConnected = false;
        if (form == null)
        {
            form = new WWWForm();
        }
        playerscore = 0;
        leaderboardScores = new List<PlayerData>();
    }

    void Start()
    {
        StartCoroutine(Connect());
    }

    public IEnumerator ReturnTopScores()
    {
        using ( UnityWebRequest webRequest =  UnityWebRequest.Get("http://iam.colum.edu/students/kevin.hatley/PharmAR/Webapplication2/leaderboard/GetTopHighScores"))
        {
            yield return webRequest.SendWebRequest();
            
            string data = webRequest.downloadHandler.text;
            char spliter = ',';
            string[] substrings = data.Split(spliter); /// 
            for (int i = 0; i < substrings.Length; i+=2)
            {
                int test = i + 1;
                if(test < substrings.Length)
                {
                    PlayerData p = new PlayerData();
                    p.Name = substrings[i];
                    p.Score = int.Parse(substrings[i + 1]);
                    leaderboardScores.Add(p);
                }  
            }
        }
    }

    /// <summary> SendScoreToServerLeaderBoard:
    /// sends the players score back to the leader board by calling the Coroutine function;
    /// </summary>
    public void SendScoreToServerLeaderBoard()
    { 
        if(isConnected)
        { 
            playerscore = playerManager.PlayerScore; // get score from ScoreAmount
            form.AddField("score", playerscore);
            WWW www = new WWW("http://iam.colum.edu/students/kevin.hatley/PharmAR/Webapplication2/playerhighscores/GetLastPlayerHighScore", form);
        }
    }

    public string GetEmailFromServer()
    {
        StartCoroutine(WaitforEmailResponse());
        if(!isConnected)
        {
            playeremail = "";
        }
        return playeremail;
    }

    private IEnumerator WaitforEmailResponse()
    {
        WWW www = new WWW("http://iam.colum.edu/students/kevin.hatley/PharmAR/Webapplication2/playerhighscores/GetLastPlayerEmail");
        yield return www;
        if (www.error == null)
        {
            Debug.Log(www.text);
            playeremail = www.text;
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    public void GetNameFromServer()
    {
        StartCoroutine(WaitforNameResponse());
        if (!isConnected)
        {
            playername = "";
        }
        
    }
    private IEnumerator WaitforNameResponse()
    {
        WWW www = new WWW("http://iam.colum.edu/students/kevin.hatley/PharmAR/Webapplication2/playerhighscores/GetLastPlayerName");
        yield return www;
        if (www.error == null)
        {
            Debug.Log(www.text);
            playername = www.text;
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    private IEnumerator Connect()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://iam.colum.edu/students/kevin.hatley/PharmAR/Webapplication2/playerhighscores/"))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                isConnected = true;
                StartCoroutine(ReturnTopScores());
            }
        }
    }

}
