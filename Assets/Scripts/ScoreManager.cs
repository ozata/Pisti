﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    public static ScoreManager Instance { get { return instance; } }

    public int playerScore = 0;
    public int opponentScore = 0;

    public GameObject resultTexts;
    public GameObject winnerText;
    public GameObject playerScoreText;
    public GameObject opponentScoreText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void CloseScoreText()
    {
        resultTexts.SetActive(false);
    }

    // TODO: Jack does not give 20 points when makes a pisti, check with rules.
    public void AddPisti()
    {
        if (GameSystem.Instance.state == GameState.PLAYERTURN)
        {
            playerScore += 10;
        }
        else if (GameSystem.Instance.state == GameState.OPPONENTTURN)
        {
            opponentScore += 10;
        }
    }


    public void FinalScore()
    {
        string winner;
        playerScore += CalculateScore(DeckManager.Instance.playerWinList);
        opponentScore += CalculateScore(DeckManager.Instance.opponentWinList);

        if (DeckManager.Instance.playerWinList.Count > DeckManager.Instance.opponentWinList.Count)
        {
            playerScore += 3;
        }
        else
        {
            opponentScore += 3;
        }

        if (playerScore > opponentScore)
        {
            GameSystem.Instance.state = GameState.PLAYERWON;
            winner = "You";
        }
        else
        {
            GameSystem.Instance.state = GameState.OPPONENTWON;
            winner = "Opponent";
        }
        resultTexts.SetActive(true);
        winnerText.GetComponent<TMPro.TextMeshProUGUI>().text = winner + " Won! ";
        playerScoreText.GetComponent<TMPro.TextMeshProUGUI>().text = "Player Score: " + playerScore;
        opponentScoreText.GetComponent<TMPro.TextMeshProUGUI>().text = "Opponent Score: " + opponentScore;

        // Reset Scores
        playerScore = 0;
        opponentScore = 0;
    }

    int CalculateScore(List<GameObject> scoreList)
    {
        int score = 0;
        int[] cardValue;
        for (int i = 0; i < scoreList.Count; i++)
        {
            cardValue = DeckManager.Instance.GetCardValue(scoreList[i]);
            // If Jack, 1 point added
            if (cardValue[1] == DeckManager.JACK)
            {

                score += 1;
            }
            // Ace, 1 point added
            else if (cardValue[1] == DeckManager.ACE)
            {
                score += 1;
            }
            // Club of 2s, 2 points added
            else if (cardValue[0] == DeckManager.CLUB && cardValue[1] == 2)
            {
                score += 2;
            }
            // Diamond of 10s, 3 points added
            else if (cardValue[0] == DeckManager.DIAMOND && cardValue[1] == 10)
            {
                score += 3;
            }
        }
        return score;
    }





}
