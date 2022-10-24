using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private static int score = 300;
    [SerializeField] private static int lives = 10;

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
        Object.DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        _uiManager ??= GetComponent<UIManager>();
    }

    public void ChangeScore(int change)
    {
        score += change;
        _uiManager.UpdateScore(score);
    }

    public void ChangeLives(int change)
    {
        lives += change;
        _uiManager.UpdateLives(lives);
    }

}
