using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum GameState {START, PLAYERTURN, OPPONENTTURN, WON, LOST}

public class GameSystem : MonoBehaviour
{
    // One hand won
    private bool currentHandWon;
    // if this is 4 this means all cards in the hand of player is gone and run the dealCards function
    private int dealCards = 0;

    private static GameSystem instance;
    public static GameSystem Instance { get { return instance; } }

    // Someone won the hand
    public UnityEvent handWon;

    int[] cardValue;

    void OnEnable(){
        if (handWon == null){
            handWon = new UnityEvent();
        }
        //handWon.AddListener(DeckManager.Instance.AddOneCardToGameList);

    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else {
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

    void SetupGame() {
        state = GameState.PLAYERTURN;
    }

    public void PlayGame(GameObject lastPlayedCard) {
        cardValue = DeckManager.Instance.GetCardValue(lastPlayedCard);
        // Add the card to gameList, meaning, current game cards on the table
        DeckManager.Instance.addCard.Invoke(lastPlayedCard, CardList.GAMELIST);

        if(GameSystem.Instance.state == GameState.PLAYERTURN) {
            if(cardValue[1] == DeckManager.Instance.GetCardOnTopValue()[1]){
                Debug.Log("Player Win this turn!");
                DeckManager.Instance.AddCardsToWinningList(CardList.PLAYERWINLIST);
            }
            GameSystem.Instance.state = GameState.OPPONENTTURN;
        } else if(GameSystem.Instance.state == GameState.OPPONENTTURN) {
           StartCoroutine(OpponentPlay());
        }

        DeckManager.Instance.CardOnTop = lastPlayedCard;
        dealCards++;
        if(dealCards == DeckManager.START_CARD_NUMBER * DeckManager.NUMBER_OF_PLAYERS) {
            DealCardsToPlayers();
        }
    }


    IEnumerator OpponentPlay(){
         if(cardValue[1] == DeckManager.Instance.GetCardOnTopValue()[1]){
                yield return new WaitForSeconds(0.5f);
                Debug.Log("Opponent Win this turn!");
                DeckManager.Instance.AddCardsToWinningList(CardList.OPPONENTWINLIST);
            }
        GameSystem.Instance.state = GameState.PLAYERTURN;
    }

    void DealCardsToPlayers() {
        DeckManager.Instance.DealCards();
        dealCards = 0;
    }


}
