using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject bgPanel;
    public GameObject victoryPanel;
    public GameObject losePanel;

    public int goal;
    public int moves;
    public int points;

    public bool IsGameEnded;

    public TMP_Text pointsTxt;
    public TMP_Text movesTxt;
    public TMP_Text goalTxt;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int _moves,int _goal)
    {
        moves = _moves;
        goal = _goal;
    }

    // Update is called once per frame
    void Update()
    {
        pointsTxt.text = "Points:" + points.ToString();
        movesTxt.text = "Moves:" + moves.ToString();
        goalTxt.text = "Goal:" + goal.ToString();
    }

    public void ProcessTurn(int _pointsToGain,bool _substractMoves)
    {
        points += _pointsToGain;
        if(_substractMoves)
        {
            moves--;
        }

        if(points>=goal)
        {
            //you've won the game
            IsGameEnded = true;
            //Display a victory screen
            bgPanel.SetActive(true);
            victoryPanel.SetActive(true);
            return;
        }
        if(moves==0)
        {
            //loose the game
            IsGameEnded = true;
            bgPanel.SetActive(true);
            losePanel.SetActive(true);
            return;
        }
    }

    public void WinGame()
    {
        SceneManager.LoadScene(0);
    }
    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }
}
