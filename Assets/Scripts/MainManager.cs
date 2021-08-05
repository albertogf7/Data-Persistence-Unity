using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainManager : MonoBehaviour
{
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public GameObject GameOverText;
    
    private bool m_Started = false;
    private int m_Points;
    
    private bool m_GameOver = false;

    public static MainManager Instance;
    public string UserName;
    public string HighScoreUser;
    public Text HighScoreText;
    public int m_Highscore;
    public bool newHighScoreSet;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        UserName = ScoresTracker.Instance.playerName;
        LoadHighScore();

        m_Highscore = PlayerPrefs.GetInt("HighScore");
        HighScoreText.text = $"{HighScoreUser}'s High Score : {m_Highscore}";
        ScoreText.text = $"{UserName} : {m_Points}";
    }

        void Start()
    {
        const float step = 0.6f;
        int perLine = Mathf.FloorToInt(4.0f / step);
        
        int[] pointCountArray = new [] {1,1,2,2,5,5};
        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                brick.PointValue = pointCountArray[i];
                brick.onDestroyed.AddListener(AddPoint);
            }
        }
    }

    private void Update()
    {
        if (!m_Started)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Started = true;
                float randomDirection = Random.Range(-1.0f, 1.0f);
                Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                forceDir.Normalize();

                Ball.transform.SetParent(null);
                Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
            }
        }
        else if (m_GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void AddPoint(int point)
    {
        m_Points += point;
        ScoreText.text = $"{UserName} : {m_Points}";
        UpdateHighScore();
    }

    public void UpdateHighScore()
    {
        if (m_Points > m_Highscore)
        {
            HighScoreUser = UserName;
            m_Highscore = m_Points;
            HighScoreText.text = $"{UserName} 's High Score : {m_Points}";

            PlayerPrefs.SetInt("HighScore", m_Highscore);
        }
    }

    public void GameOver()
    {
        m_GameOver = true;
        GameOverText.SetActive(true);
        SaveplayerName();
    }

    [System.Serializable]
    class SaveData
    {
        public string playerName;
        public int highScore;
        public string newHighPlayer;
    }

    public void SaveplayerName()
    {
        SaveData data = new SaveData();
        data.highScore = m_Highscore;
        data.newHighPlayer = HighScoreUser;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public void LoadHighScore()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            m_Highscore = data.highScore;
            HighScoreUser = data.newHighPlayer;
        }
    }
}
