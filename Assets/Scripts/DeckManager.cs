using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;


public enum CardList {GAMELIST, PLAYERLIST, OPPONENTLIST, CLOSEDLIST, PLAYERWINLIST, OPPONENTWINLIST};

public class DeckManager : MonoBehaviour {

    const int START_CARD_NUMBER = 4;
    const int NUMBER_OF_PLAYERS = 2;

    public AddGameObjectToListEvent addCard;
    public UnityEvent handWin;

    private static DeckManager instance;
    public static DeckManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }

    public GameObject deckArea;
    public GameObject cardPrefab;
    public GameObject playerArea;
    public GameObject opponentArea;
    public GameObject closedCardsArea;
    public GameObject activeCardArea;
    public GameObject gameArea;

    public OpponentAI opponentAI;

    // Starting deck of cards
    [HideInInspector] public List<GameObject> deck;
    // Cards in player's hand in current time
    [HideInInspector] public List<GameObject> playerList;
    // Cards that player won
    [HideInInspector] public List<GameObject> playerWonList;
    // Cards in opponent's hand in current time
    [HideInInspector] public List<GameObject> opponentList;
    // Cards that opponent won
    [HideInInspector] public List<GameObject> opponentWonList;
    // Cards in middle in current time
    [HideInInspector] public List<GameObject> gameList;
    // Cards that are not opened yet
    [HideInInspector] public List<GameObject> closedList;

    // The card that is currently on top
    [HideInInspector]
    private GameObject cardOnTop;
    public GameObject CardOnTop {
        get; set;
    }

    public Sprite[] sprites;
    // Sprite of backside of a card
    public Sprite closedCard;

    void OnEnable() {
        if (addCard == null){
            addCard = new AddGameObjectToListEvent();
            handWin = new UnityEvent();
        }

        addCard.AddListener(AddCardObjectToList);
        addCard.AddListener(MoveCardToAreaUI);
        handWin.AddListener(AddWinsToWinner);
    }

    void Start() {
        CreateDeck();
        FisherYatesCardDeckShuffle();
        InitCards();
        CloseThreeOfFirstFourCards();
        CardOnTop = gameList[START_CARD_NUMBER - 1];
    }


    public void AddOneCardToGameList(){
        if(closedList.Count > 0 ){
            GameObject card = closedList[0];
            Debug.Log(closedList[0].GetComponent<Image>().sprite);
            closedList.Remove(card);
            Debug.Log(closedList[0].GetComponent<Image>().sprite);
        }
    }

    public int GetCardOnTopValue(){

        return GetCardValue(CardOnTop);
    }

    public int[] GetCardValue(GameObject card){
        int[] cardValue = new int[2];
        if(card.transform.name.Contains(" ")){
            string[] values = card.transform.name.Split (' ');
            cardValue[0] = Int32.Parse(values[0]);
            cardValue[1] = Int32.Parse(values[1]);
        }
        return cardValue;
    }

    void AddCardObjectToList(GameObject card, CardList cardListType) {
        if(cardListType == CardList.GAMELIST){
            gameList.Add(card);
        } else if(cardListType == CardList.PLAYERLIST){
            playerList.Add(card);
            card.GetComponent<Button>().enabled = true;
        } else if (cardListType == CardList.OPPONENTLIST) {
            card.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().color = new Color32(0,0,0,255);
            CloseOrOpenCard(card);
            opponentAI.opponentList.Add(card);
        } else if(cardListType == CardList.CLOSEDLIST){
            closedList.Add(card);
        } else if(cardListType == CardList.PLAYERWINLIST){
            playerWinList.Add(card);
        } else if(cardListType == CardList.OPPONENTWINLIST){
            opponentWinList.Add(card);
        }
    }


    void AddWinsToWinner(){

    }


    void MoveCardToAreaUI(GameObject card, CardList cardListType) {
        if(cardListType == CardList.GAMELIST){
            card.transform.SetParent(activeCardArea.transform, false);
            card.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(100,200),Random.Range(-280,-320));
        } else if(cardListType == CardList.PLAYERLIST){
            card.transform.SetParent(playerArea.transform, true);
        } else if (cardListType == CardList.OPPONENTLIST) {
            card.transform.SetParent(opponentArea.transform, false);
        } else if (cardListType == CardList.CLOSEDLIST) {
            //card.transform.SetParent(opponentArea.transform, false);
        } else if (cardListType == CardList.PLAYERWINLIST) {
            //card.transform.SetParent(opponentArea.transform, false);
        } else if (cardListType == CardList.OPPONENTWINLIST) {

        }
    }

    // Add card to active object list
    public void OnClickCard() {
        //GameSystem.Instance.CheckGameStatus(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        if(GameSystem.Instance.state == GameState.PLAYERTURN){
            GameObject card = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            addCard.Invoke(card, 0);
            GameSystem.Instance.state = GameState.OPPONENTTURN;
            if(GetCardValue(CardOnTop) == GetCardValue(card)){
                print("player won current hand");
                AddCardsToWinningList(1);
                CardOnTop = null;

            }else{
                CardOnTop = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            }

            StartCoroutine(opponentAI.Play());
        }
    }

    public void AddCardsToWinningList(int listId){
        for(int i = 0; i < gameList.Count ; i++){
            addCard.Invoke(gameList[i], 1);
            Debug.Log(gameList.Count);
        }
        gameList.Clear();
    }

    void InitCards(){
        for(int i = 0 ; i < START_CARD_NUMBER; i++) {
            addCard.Invoke(deck[i] , 0);
            addCard.Invoke(deck[i+START_CARD_NUMBER] , 2);
            addCard.Invoke(deck[i+(START_CARD_NUMBER*2)] , 1);
        }
    }

    void CloseThreeOfFirstFourCards(){
        for(int i = 0 ; i < START_CARD_NUMBER - 1 ; i++){
            CloseOrOpenCard(gameList[i]);
        }
    }

    public void CloseOrOpenCard(GameObject card) {
        if(card.transform.GetChild(1).gameObject.active){
            print("open card");
            card.transform.GetChild(1).gameObject.SetActive(false);
        } else {
            print("close card");
            card.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    // Initialize cards
    // Club = 1, Spade = 2, Diamond = 3, Heart = 4
    public void CreateDeck() {
        GameObject card;
        for(int i = 1; i < 5 ; i++) {
            for(int j = 1 ; j < 14 ; j++){
                card = Instantiate(cardPrefab, new Vector3(0,0,0), Quaternion.identity);
                card.gameObject.name = i + " " + j;
                card.GetComponent<Card>().Suit = i;
                card.GetComponent<Card>().Rank = j;
                if(i == 1) {
                    card.GetComponent<Image>().sprite = sprites[0];
                } else if(i == 2) {
                    card.GetComponent<Image>().sprite = sprites[1];
                } else if(i == 3) {
                    card.GetComponent<Image>().sprite = sprites[2];
                } else if(i == 4) {
                    card.GetComponent<Image>().sprite = sprites[3];
                }

                // Change the text of the numbered cards' values
                if(j < 11)
                    card.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = j.ToString();

                // Jacks
                if(i == 1 && j == 11) card.GetComponent<Image>().sprite = sprites[4];
                if(i == 2 && j == 11) card.GetComponent<Image>().sprite = sprites[5];
                if(i == 3 && j == 11) card.GetComponent<Image>().sprite = sprites[6];
                if(i == 4 && j == 11) card.GetComponent<Image>().sprite = sprites[7];

                // Queens
                if(i == 1 && j == 12) card.GetComponent<Image>().sprite = sprites[8];
                if(i == 2 && j == 12) card.GetComponent<Image>().sprite = sprites[9];
                if(i == 3 && j == 12) card.GetComponent<Image>().sprite = sprites[10];
                if(i == 4 && j == 12) card.GetComponent<Image>().sprite = sprites[11];

                // Kings
                if(i == 1 && j == 13) card.GetComponent<Image>().sprite = sprites[12];
                if(i == 2 && j == 13) card.GetComponent<Image>().sprite = sprites[13];
                if(i == 3 && j == 13) card.GetComponent<Image>().sprite = sprites[14];
                if(i == 4 && j == 13) card.GetComponent<Image>().sprite = sprites[15];

                deck.Add(card);
                card.transform.SetParent(deckArea.transform, false);
            }
        }
    }

    // Taken From: https://answers.unity.com/questions/486626/how-can-i-shuffle-alist.html
    void FisherYatesCardDeckShuffle () {
        System.Random _random = new System.Random ();
        GameObject card;
        int n = deck.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + (int)(_random.NextDouble() * (n - i));
            card = deck[r];
            deck[r] = deck[i];
            deck[i] = card;
        }
    }



    void OnDisable() {
        if (addCard == null){
            addCard.RemoveListener(AddCardObjectToList);
            addCard.RemoveListener(MoveCardToAreaUI);
            handWin.RemoveListener(AddWinsToWinner);
        }
    }

    #region Events
    [System.Serializable]
    public class AddGameObjectToListEvent : UnityEvent<GameObject, int>{ }
    #endregion
}