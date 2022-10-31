using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private Builder _builder;
    [SerializeField] private string _moneyString = "$";
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private string _livesString = "10 Carrots";
    [SerializeField] private TextMeshProUGUI _livesText;
    [SerializeField] private string _pointsString = "0 Rabbits captured";
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private GameObject _endPanel; 
    #endregion

    public void ChangeBuilderMode(int i)
    {
        //modify which game object the builder is accessing
        _builder.SetBuilderMode(i);
    }

    public void UpdateMoney(int score)
    {
        //update the money string and display text to match
        _moneyString = $"${score}";
        _moneyText.text = _moneyString;
    }

    public void UpdateLives(int lives)
    {
        //update the lives string and display text to match
        _livesString = $"{lives} Carrots";
        _livesText.text = _livesString;
    }

    public void UpdatePoints(int points)
    {
        //update the pooints string and display text to match
        _pointsString = $"{points} Rabbit{(points == 1 ? "" : "s")} captured";
        _pointsText.text = _pointsString;
    }

    public void TriggerEndPanel()
    {
        //activate the end panel
        _endPanel.SetActive(true);
    }
}
