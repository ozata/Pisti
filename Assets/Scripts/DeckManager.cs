using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DeckManager : MonoBehaviour {

    const int START_CARD_NUMBER = 4;

    AddGameObjectToListEvent addCard;

    private static DeckManager _instance;
    public static DeckManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public GameObject cardPrefab;
    public GameObject playerArea;
    public GameObject opponentArea;
    public GameObject gameArea;

    [HideInInspector] public List<GameObject> deck;
    [HideInInspector] public List<GameObject> playerList;
    [HideInInspector] public List<GameObject> opponentList;
    [HideInInspector] public List<GameObject> gameList;

    public Sprite[] sprites;
    // A backside of a card
    public Sprite closedCard;

    void Start() {

        if (addCard == null)
            addCard = new AddGameObjectToListEvent();

        addCard.AddListener(AddCardObjectToList);
        addCard.AddListener(MoveCardToAreaUI);


        CreateDeck();
        FisherYatesCardDeckShuffle();
        InitFirstFourCards();
    }

    // listId: 0 = gameList, 1 = playerList, 2 = opponentList
    void AddCardObjectToList(GameObject card, int listId) {
        if(listId == 0){
            gameList.Add(card);
        } else if(listId == 1){
            playerList.Add(card);
        } else if (listId == 2) {
            opponentList.Add(card);
        }
    }

    void MoveCardToAreaUI(GameObject card, int listId) {
        if(listId == 0){
            card.transform.SetParent(gameArea.transform, false);
        } else if(listId == 1){
            card.transform.SetParent(playerArea.transform, false);
        } else if (listId == 2) {
            card.transform.SetParent(opponentArea.transform, false);
        }
    }

    public void OnClickCard() {
        addCard.Invoke(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0);
    }

    void InitFirstFourCards(){
        for(int i = 0 ; i < START_CARD_NUMBER; i++) {
            addCard.Invoke(deck[i] , 0);
            addCard.Invoke(deck[i+START_CARD_NUMBER] , 2);
            addCard.Invoke(deck[i+(START_CARD_NUMBER*2)] , 1);
        }
    }

    // Initialize cards
    // Club = 1, Spade = 2, Diamond = 3, Heart = 4
    public void CreateDeck() {
        GameObject card;
        for(int i = 1; i < 5 ; i++) {
            for(int j = 1 ; j < 14 ; j++){
                card = Instantiate(cardPrefab, new Vector3(0,0,0), Quaternion.identity);
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

    #region Events
    [System.Serializable]
    public class AddGameObjectToListEvent : UnityEvent<GameObject, int>
    {
    }
    #endregion
}