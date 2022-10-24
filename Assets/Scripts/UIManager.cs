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
}
