using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Builder _builder;
    [SerializeField] private string score = "$";
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private string lives = "10 Carrots";
    [SerializeField] private TextMeshProUGUI _livesText;

    public void ChangeBuilderMode(int i)
    {
        _builder.SetBuilderMode(i);
    }
}
