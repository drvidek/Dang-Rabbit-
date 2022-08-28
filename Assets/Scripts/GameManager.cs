using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static List<Rabbit> rabbits = new List<Rabbit>();

    public static void RepathRabbits()
    {
        foreach (Rabbit rabbit in rabbits)
        {
            rabbit.RepathPending = true;
        }
    }
}
