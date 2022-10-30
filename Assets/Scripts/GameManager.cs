using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _isPlaying = true;
    public bool IsPlaying { get => _isPlaying; }
    [SerializeField] private int money = 300;
    [SerializeField] private int lives = 10;
    [SerializeField] private int points = 0;

    private int _rabbitWorth = 6;
    public int RabbitWorth { get => _rabbitWorth; }

    private static GameManager _singleton;
    public static GameManager Singleton
    {
        //Property Read is the instance, public by default
        get => _singleton;
        //private means only this instance of the class can access set
        private set
        {
            //set the instance to the value if the instance is null
            if (_singleton == null)
            {
                _singleton = value;
            }
            //if it is not null, check if the value is stored as the static instance
            else if (_singleton != value)
            {
                //if not, throw a warning and destroy that instance

                //$ is to identify the string as containing an interpolated value
                Debug.LogWarning($"{nameof(GameManager)} instance already exists, destroy duplicate!");
                Destroy(value);
            }
        }
    }
    private UIManager _uiManager;

    private void Awake()
    {
        //when the object that this is attached to in game initialises, try to set the instance to this
        Singleton = this;
        //Object.DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        _uiManager ??= GetComponent<UIManager>();
        _uiManager.UpdateScore(money);
        _uiManager.UpdateLives(lives);
        _uiManager.UpdatePoints(points);
    }

    public void ChangeMoney(int change)
    {
        if (!_isPlaying)
            return;

        money += change;
        _uiManager.UpdateScore(money);
    }

    public bool CheckMoney(int value)
    {
        return (money - value >= 0);
    }    

    public void ChangeLives(int change)
    {
        if (!_isPlaying)
            return;

        lives += change;
        _uiManager.UpdateLives(lives);
        if (lives == 0)
        {
            _uiManager.TriggerEndPanel();
            _isPlaying = false;
        }
    }

    public void ChangePoints(int change)
    {
        if (!_isPlaying)
            return;

        points += change;
        _uiManager.UpdatePoints(points);
    }

    public void RestartScene()
    {
        money = 300;
        lives = 10;
        points = 0;
        _isPlaying = true;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
