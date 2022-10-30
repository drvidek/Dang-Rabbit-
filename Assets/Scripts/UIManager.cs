using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Builder _builder;
    [SerializeField] private string _scoreString = "$";
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private string _livesString = "10 Carrots";
    [SerializeField] private TextMeshProUGUI _livesText;
    [SerializeField] private string _pointsString = "0 Rabbits captured";
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private GameObject _endPanel;

    public void ChangeBuilderMode(int i)
    {
        _builder.SetBuilderMode(i);
    }

    public void UpdateScore(int score)
    {
        _scoreString = $"${score}";
        _scoreText.text = _scoreString;
    }

    public void UpdateLives(int lives)
    {
        _livesString = $"{lives} Carrots";
        _livesText.text = _livesString;
    }

    public void UpdatePoints(int points)
    {
        _pointsString = $"{points} Rabbit{(points == 1 ? "" : "s")} captured";
        _pointsText.text = _pointsString;
    }

    public void TriggerEndPanel()
    {
        _endPanel.SetActive(true);
    }
}
