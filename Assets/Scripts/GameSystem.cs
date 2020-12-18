using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState {START, PLAYERTURN, OPPONENTTURN, PLAYERWON, OPPONENTWON}
public enum LastHandWinner {PLAYER, OPPONENT}

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
        } else {
            instance = this;
        }
        cardValue = new int[2];
    }

    public UnityEvent pisti;

    void OnEnable(){
        if (pisti == null){
            pisti = new UnityEvent();
        }
        pisti.AddListener(ScoreManager.Instance.AddPisti);
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
        // Add the card to gameList, game cards on the table
        DeckManager.Instance.addCard.Invoke(lastPlayedCard, CardList.GAMELIST);
        SoundManager.Instance.PlayPlayCard();
        if(GameSystem.Instance.state == GameState.PLAYERTURN) {
            DeckManager.Instance.playerList.Remove(lastPlayedCard);
            PlayerPlay();
        } else if(GameSystem.Instance.state == GameState.OPPONENTTURN) {
            DeckManager.Instance.opponentList.Remove(lastPlayedCard);
            StartCoroutine(OpponentPlay());
        }
        CheckDealCards();
        CheckGameOver();
        if(DeckManager.Instance.gameList.Count > 0)
            DeckManager.Instance.CardOnTop = DeckManager.Instance.gameList[DeckManager.Instance.gameList.Count - 1];

    }


    void CheckGameOver() {
        if(DeckManager.Instance.playerList.Count == 0 && DeckManager.Instance.closedList.Count == 0 && DeckManager.Instance.opponentList.Count == 0){
            DeckManager.Instance.LastHandWon(lastHandWinner);
        }
    }

    #region Play functions
    void PlayerPlay(){
        bool jackWin = false;
        if(cardValue[1] == DeckManager.JACK && DeckManager.Instance.GetGameListCount() > 1) jackWin = true;
        if(jackWin || cardValue[1] == DeckManager.Instance.GetCardOnTopValue()[1]){
            SoundManager.Instance.PlayCardWin();
            lastHandWinner = LastHandWinner.PLAYER;
            if(DeckManager.Instance.GetGameListCount() == 2) {
                print("PLAYER PİŞTİ YAPTI");
                pisti.Invoke();
            }
            DeckManager.Instance.AddCardsToWinningList(CardList.PLAYERWINLIST);
        }
        GameSystem.Instance.state = GameState.OPPONENTTURN;
    }

    IEnumerator OpponentPlay(){
        bool jackWin = false;
        if(cardValue[1] == DeckManager.JACK && DeckManager.Instance.GetGameListCount() > 1) jackWin = true;
        if(jackWin || cardValue[1] == DeckManager.Instance.GetCardOnTopValue()[1]){
            lastHandWinner = LastHandWinner.OPPONENT;
            yield return new WaitForSeconds(0.5f);
            SoundManager.Instance.PlayCardWin();
            if(DeckManager.Instance.GetGameListCount() == 2){
                print("OPPONENT PİŞTİ YAPTI");
                pisti.Invoke();
            }
            DeckManager.Instance.AddCardsToWinningList(CardList.OPPONENTWINLIST);
        }
        GameSystem.Instance.state = GameState.PLAYERTURN;
    }
    #endregion

    void CheckDealCards(){
        dealCards++;
        if(dealCards == DeckManager.START_CARD_NUMBER * DeckManager.NUMBER_OF_PLAYERS) {
            DealCardsToPlayers();
        }
    }

    void DealCardsToPlayers() {
        DeckManager.Instance.DealCards();
        dealCards = 0;
    }

    void OnDisable() {
        if (pisti != null){
            pisti.RemoveListener(ScoreManager.Instance.AddPisti);
        }
    }
}
