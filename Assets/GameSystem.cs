using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum GameState {START, PLAYERTURN, OPPONENTTURN, WON, LOST}

public class GameSystem : MonoBehaviour
{
    // One hand won
    private bool currentHandWon;

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

    void CheckGameStatus(GameObject lastPlayedCard) {
        cardValue = DeckManager.Instance.GetCardValue(lastPlayedCard);
        // Add the card to gameList, meaning, current game cards on the table
        DeckManager.Instance.addCard.Invoke(card, CardList.GAMELIST);
        if(GameSystem.Instance.state == GameState.PLAYERTURN) {
            if(cardValue == DeckManager.Instance.GetCardOnTopValue()){
                DeckManager.Instance.AddCardsToWinningList(CardList.OPPONENTWINLIST);
            }
            GameSystem.Instance.state = GameState.OPPONENTTURN;
        } else if(GameSystem.Instance.state == GameState.OPPONENTTURN) {
            if(cardValue == DeckManager.Instance.GetCardOnTopValue()){
                DeckManager.Instance.AddCardsToWinningList(Cardlist.PLAYERWINLIST);
            }
            GameSystem.Instance.state = GameState.PLAYERTURN;
        }
    }


}
