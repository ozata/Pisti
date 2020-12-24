using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState { START, PLAYERTURN, OPPONENTTURN, PLAYERWON, OPPONENTWON }
public enum LastHandWinner { PLAYER, OPPONENT }

public class GameSystem : MonoBehaviour
{
    // Deal cards check when cards on players are 0
    private int dealCards = 0;
    int[] cardValue;

    private static GameSystem instance;
    public static GameSystem Instance { get { return instance; } }

    // winner of last hand takes the cards on the table
    private LastHandWinner lastHandWinner;
    int lastTwoCardsPlayed = 0;

    private bool lastHand = false;

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
        cardValue = new int[2];
    }

    public GameState state;

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.START;
        SetupGame();
    }

    void SetupGame()
    {
        state = GameState.PLAYERTURN;
    }

    public void PlayGame(GameObject lastPlayedCard)
    {
        cardValue = DeckManager.Instance.GetCardValue(lastPlayedCard);
        // Add the card to gameList, game cards on the table
        DeckManager.Instance.addCard.Invoke(lastPlayedCard, CardList.GAMELIST);
        SoundManager.Instance.PlayPlayCard();
        if (GameSystem.Instance.state == GameState.PLAYERTURN)
        {
            DeckManager.Instance.playerList.Remove(lastPlayedCard);
            PlayerPlay();
        }
        else if (GameSystem.Instance.state == GameState.OPPONENTTURN)
        {
            DeckManager.Instance.opponentList.Remove(lastPlayedCard);
            StartCoroutine(OpponentPlay());
        }
        CheckDealCards();
        CheckGameOver();
        if (DeckManager.Instance.gameList.Count > 0)
            DeckManager.Instance.CardOnTop = DeckManager.Instance.gameList[DeckManager.Instance.gameList.Count - 1];

    }


    void CheckGameOver()
    {
        if (DeckManager.Instance.playerList.Count == 0 && DeckManager.Instance.closedList.Count == 0 && DeckManager.Instance.opponentList.Count == 0)
        {
            DeckManager.Instance.LastHandWon(lastHandWinner);
        }
    }

    #region Play functions
    void PlayerPlay()
    {
        bool jackWin = false;
        if (DeckManager.Instance.GetGameListCount() > 1)
        {
            if (cardValue[1] == DeckManager.JACK) jackWin = true;

            // Win
            if (jackWin || (cardValue[1] == DeckManager.Instance.GetCardOnTopValue()[1]))
            {
                SoundManager.Instance.PlayCardWin();

                if (!DeckManager.Instance.FirstHandWon)
                {
                    DeckManager.Instance.FirstHandWonByOpponent = false;
                    DeckManager.Instance.FirstHandWon = true;
                }

                lastHandWinner = LastHandWinner.PLAYER;

                if (DeckManager.Instance.GetGameListCount() == 2)
                {
                    if (jackWin && DeckManager.Instance.GetCardOnTopValue()[1] == DeckManager.JACK)
                    {
                        ScoreManager.Instance.AddPisti();
                    }
                    else if (!jackWin)
                    {
                        ScoreManager.Instance.AddPisti();
                    }
                }
                DeckManager.Instance.AddCardsToWinningList(CardList.PLAYERWINLIST);
            }
        }
        GameSystem.Instance.state = GameState.OPPONENTTURN;
    }

    IEnumerator OpponentPlay()
    {
        bool jackWin = false;
        if (DeckManager.Instance.GetGameListCount() > 1)
        {
            if (cardValue[1] == DeckManager.JACK) jackWin = true;

            // Win
            if (jackWin || cardValue[1] == DeckManager.Instance.GetCardOnTopValue()[1])
            {
                yield return new WaitForSeconds(0.3f);
                SoundManager.Instance.PlayCardWin();

                // If first hand won by Opponent add them to playedList
                if (!DeckManager.Instance.FirstHandWon)
                {
                    DeckManager.Instance.FirstHandWonByOpponent = true;
                    DeckManager.Instance.FirstHandWon = true;
                    // Add the first 3 closed cards to the game list
                    for (int i = 0; i < 3; i++)
                    {
                        if (!DeckManager.Instance.playedList.Contains(DeckManager.Instance.gameList[i]))
                        {
                            DeckManager.Instance.playedList.Add(DeckManager.Instance.gameList[i]);
                        }
                    }
                }

                lastHandWinner = LastHandWinner.OPPONENT;
                if (DeckManager.Instance.GetGameListCount() == 2)
                {
                    if (jackWin && DeckManager.Instance.GetCardOnTopValue()[1] == DeckManager.JACK)
                    {
                        ScoreManager.Instance.AddPisti();
                    }
                    else if (!jackWin)
                    {
                        ScoreManager.Instance.AddPisti();
                    }
                }
                DeckManager.Instance.AddCardsToWinningList(CardList.OPPONENTWINLIST);
            }
        }

        GameSystem.Instance.state = GameState.PLAYERTURN;
    }
    #endregion

    void CheckDealCards()
    {
        dealCards++;
        if (dealCards == DeckManager.START_CARD_NUMBER * DeckManager.NUMBER_OF_PLAYERS)
        {
            DealCardsToPlayers();
        }
    }

    void DealCardsToPlayers()
    {
        DeckManager.Instance.DealCards();
        dealCards = 0;
    }
}
