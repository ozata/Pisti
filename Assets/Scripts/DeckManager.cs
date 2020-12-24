using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System;


using Random = UnityEngine.Random;

public enum CardList { GAMELIST, PLAYERLIST, OPPONENTLIST, CLOSEDLIST, PLAYERWINLIST, OPPONENTWINLIST };


// Club = 1, Spade = 2, Diamond = 3, Heart = 4
public class DeckManager : MonoBehaviour
{

    // add cards on table to the last one player
    private bool playerWonLastHand = false;

    // TODO: Beware static variables, find another way it it's not good
    public static int MAX_CARD_ON_DECK = 52;
    public static int START_CARD_NUMBER = 4;
    public static int NUMBER_OF_PLAYERS = 2;

    #region Values
    public static int ACE = 1;
    public static int JACK = 11;
    public static int QUEEN = 12;
    public static int KING = 13;
    #endregion

    #region Ranks
    public static int CLUB = 1;
    public static int SPADES = 2;
    public static int DIAMOND = 3;
    public static int HEART = 4;
    #endregion

    // TODO: Check if there's need for events
    public AddGameObjectToListEvent addCard;

    private static DeckManager instance;
    public static DeckManager Instance { get { return instance; } }
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

    public GameObject deckArea;
    public GameObject cardPrefab;
    public GameObject playerArea;
    public GameObject opponentArea;
    public GameObject closedCardsArea;
    public GameObject activeCardArea;
    public GameObject gameArea;

    public GameObject playerWinArea;
    public GameObject opponentWinArea;

    public GameObject canvas;

    public OpponentAI opponentAI;

    // Starting deck of cards
    [HideInInspector] public List<GameObject> deck;
    // Cards in player's hand in current time
    [HideInInspector] public List<GameObject> playerList;
    // Cards that player won
    [HideInInspector] public List<GameObject> playerWinList;
    // Cards in opponent's hand in current time
    [HideInInspector] public List<GameObject> opponentList;
    // Cards that opponent won
    [HideInInspector] public List<GameObject> opponentWinList;

    // TODO: Check if this is better as Stack rather than List
    // Cards that is on game table in current time
    [HideInInspector] public List<GameObject> gameList;

    // Cards that are not opened yet
    [HideInInspector] public List<GameObject> closedList;

    // Cards that are left the table, OpponentAI will use this list to trackCards
    [HideInInspector] public List<GameObject> playedList;

    // The card that is currently on top
    [HideInInspector]
    private GameObject cardOnTop;
    public GameObject CardOnTop
    {
        get; set;
    }

    // If first hand is already finished, Opponent AI can use the first three cards since they'll be open
    [HideInInspector]
    private bool firstHandWon;
    public bool FirstHandWon
    {
        get; set;
    }

    // If first hand is won by Opponent. 3 Cards Will be Added to playedList so
    // Opponent can see what they are and count the cards
    [HideInInspector]
    private bool firstHandWonByOpponent;
    public bool FirstHandWonByOpponent
    {
        get; set;
    }

    // Current Card that is being played
    [SerializeField]
    private GameObject currentCard;
    public GameObject CurrentCard
    {
        get { return currentCard; }
        set { currentCard = value; }
    }
    public Sprite[] sprites;
    // Sprite of backside of a card
    public Sprite closedCard;

    public float speed;
    public Transform target;

    private bool moveCard;

    [HideInInspector]
    private bool gameInitialized;
    public bool GameInitialized
    {
        get; set;
    }

    void OnEnable()
    {
        if (addCard == null)
        {
            addCard = new AddGameObjectToListEvent();
        }
        addCard.AddListener(AddCardObjectToList);
        addCard.AddListener(MoveCardToAreaUI);
    }

    private bool opponentPlay = false;
    void Start()
    {
        CreateDeck();
        InitDecks();

        canvas = GameObject.Find("Canvas");
        moveCard = false;
        target = gameArea.transform;
        speed = 4000F;
    }

    void Update()
    {
        // This code is for animating
        if (moveCard)
        {
            float step = speed * Time.deltaTime;
            CurrentCard.transform.position = Vector3.MoveTowards(CurrentCard.transform.position, target.position, step);
            if (CurrentCard.transform.position == target.position)
            {
                CurrentCard.transform.SetParent(activeCardArea.transform, false);
                CurrentCard.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(120, 180), Random.Range(-250, -320));
                GameSystem.Instance.PlayGame(CurrentCard);
                StartCoroutine(OpponentAI.Instance.Play());
                moveCard = false;
            }
        }

    }

    // Add card to active object list
    public void OnClickCard()
    {
        if (GameSystem.Instance.state == GameState.PLAYERTURN)
        {
            CurrentCard = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            moveCard = true;
        }
    }

    public void LastHandWon(LastHandWinner winner)
    {
        if (winner == LastHandWinner.PLAYER)
        {
            AddCardsToWinningList(CardList.PLAYERWINLIST);
        }
        else if (winner == LastHandWinner.OPPONENT)
        {
            AddCardsToWinningList(CardList.OPPONENTWINLIST);
        }
        ScoreManager.Instance.FinalScore();
        GameManager.Instance.ShowButtons();
    }

    public void InitDecks()
    {
        FisherYatesCardDeckShuffle();
        InitCards();
        CloseThreeOfFirstFourCards();
        OpponentAI.Instance.UpdateOpponentCardValues();
        // Assign Card On Top as CardOnTop
        CardOnTop = gameList[START_CARD_NUMBER - 1];
    }

    public void AddOneCardToGameList()
    {
        CardOnTop = closedList[0];
        addCard.Invoke(CardOnTop, CardList.GAMELIST);
    }

    public int[] GetCardOnTopValue()
    {
        return GetCardValue(CardOnTop);
    }

    public int[] GetCardValue(GameObject card)
    {
        int[] cardValue = new int[2];
        if (card.transform.name.Contains(" "))
        {
            string[] values = card.transform.name.Split(' ');
            cardValue[0] = Int32.Parse(values[0]);
            cardValue[1] = Int32.Parse(values[1]);
        }
        return cardValue;
    }

    void AddCardObjectToList(GameObject card, CardList cardList)
    {
        if (cardList == CardList.GAMELIST)
        {
            gameList.Add(card);
            // Played List will hold every card that is played and OpponentAI will track the played cards and act according to that
            // Check if gameInitialized so don't add first 3 closed cards because Opponent AI can't see that.
            if (GameInitialized && !opponentList.Contains(card))
            {
                playedList.Add(card);
            }

            closedList.Remove(card);
            card.GetComponent<Button>().enabled = false;
        }
        else if (cardList == CardList.PLAYERLIST)
        {
            playerList.Add(card);
            closedList.Remove(card);
            card.GetComponent<Button>().enabled = true;
            OpenCard(card);
        }
        else if (cardList == CardList.OPPONENTLIST)
        {
            opponentList.Add(card);

            playedList.Add(card);

            closedList.Remove(card);

            CloseCard(card);
            card.GetComponent<Button>().enabled = false;
        }
        else if (cardList == CardList.CLOSEDLIST)
        {
            closedList.Add(card);
        }
        else if (cardList == CardList.PLAYERWINLIST)
        {
            playerWinList.Add(card);
        }
        else if (cardList == CardList.OPPONENTWINLIST)
        {
            opponentWinList.Add(card);
        }
    }

    void MoveCardToAreaUI(GameObject card, CardList cardList)
    {
        if (cardList == CardList.GAMELIST)
        {
            card.transform.SetParent(activeCardArea.transform, false);
            card.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(100, 200), Random.Range(-280, -320));
        }
        else if (cardList == CardList.PLAYERLIST)
        {
            MoveToList(card, cardList);
        }
        else if (cardList == CardList.OPPONENTLIST)
        {
            MoveToList(card, cardList);
        }
        else if (cardList == CardList.CLOSEDLIST)
        {
            card.transform.SetParent(closedCardsArea.transform, false);
        }
        else if (cardList == CardList.PLAYERWINLIST)
        {
            card.transform.SetParent(playerWinArea.transform, false);
        }
        else if (cardList == CardList.OPPONENTWINLIST)
        {
            card.transform.SetParent(opponentWinArea.transform, false);
        }
    }

    void MoveToList(GameObject card, CardList cardList)
    {
        Transform[] points;
        if (cardList == CardList.PLAYERLIST)
        {
            points = playerArea.GetComponentsInChildren<Transform>();
        }
        else if (cardList == CardList.OPPONENTLIST)
        {
            points = opponentArea.GetComponentsInChildren<Transform>();
        }
        else
        {
            points = null;
        }
        foreach (Transform point in points)
        {
            if (point.transform.childCount == 0 && point.transform.gameObject.name.Contains("point"))
            {
                card.transform.SetParent(point.transform, false);
                card.transform.localPosition = new Vector3(0, 0, 0);
                break;
            }
        }
    }


    public void AddCardsToWinningList(CardList list)
    {
        for (int i = 0; i < GetGameListCount(); i++)
        {
            addCard.Invoke(gameList[i], list);
        }
        ClearGameList();
    }

    public void RemoveAllCardsFromGameList()
    {
        for (int i = 0; i < GetGameListCount(); i++)
        {
            gameList.Remove(gameList[i]);
        }
    }

    public void ClearGameList()
    {
        gameList.Clear();
    }


    public void InitCards()
    {
        for (int i = 0; i < START_CARD_NUMBER; i++)
        {
            addCard.Invoke(deck[i], CardList.GAMELIST);
            addCard.Invoke(deck[i + START_CARD_NUMBER], CardList.OPPONENTLIST);
            addCard.Invoke(deck[i + (START_CARD_NUMBER * 2)], CardList.PLAYERLIST);
        }
        // Add rest of the cards to the closed cards list, which are yet to be opened
        for (int i = START_CARD_NUMBER * 3; i < MAX_CARD_ON_DECK; i++)
        {
            addCard.Invoke(deck[i], CardList.CLOSEDLIST);
        }

        // Add the last card of the initialized list to the playedList, because OpponentAI knows this card since it's open
        playedList.Add(gameList[3]);
        GameInitialized = true;
    }

    // Deal cards after no card left in the hands of Player
    public void DealCards()
    {
        for (int i = 0; i < START_CARD_NUMBER; i++)
        {
            if (closedList.Count > 0)
            {
                addCard.Invoke(closedList[0], CardList.OPPONENTLIST);
                addCard.Invoke(closedList[0], CardList.PLAYERLIST);
            }
        }

        if (GetGameListCount() == 0)
        {
            AddOneCardToGameList();
        };


    }

    void CloseThreeOfFirstFourCards()
    {
        for (int i = 0; i < START_CARD_NUMBER - 1; i++)
        {
            CloseCard(gameList[i]);
        }
    }

    public void CloseCard(GameObject card)
    {
        if (!card.transform.GetChild(1).gameObject.active)
        {
            card.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void OpenCard(GameObject card)
    {
        if (card.transform.GetChild(1).gameObject.active)
        {
            card.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    // Initialize cards
    // Club = 1, Spades = 2, Diamond = 3, Heart = 4
    public void CreateDeck()
    {
        GameObject card;
        for (int i = 1; i < 5; i++)
        {
            for (int j = 1; j < 14; j++)
            {
                card = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                card.gameObject.name = i + " " + j;
                card.GetComponent<Card>().Suit = i;
                card.GetComponent<Card>().Rank = j;
                if (i == DeckManager.CLUB)
                {
                    card.GetComponent<Image>().sprite = sprites[0];
                }
                else if (i == DeckManager.SPADES)
                {
                    card.GetComponent<Image>().sprite = sprites[1];
                }
                else if (i == DeckManager.DIAMOND)
                {
                    card.GetComponent<Image>().sprite = sprites[2];
                }
                else if (i == DeckManager.HEART)
                {
                    card.GetComponent<Image>().sprite = sprites[3];
                }

                // Change the text of the numbered cards' values
                if (j < 11)
                    card.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = j.ToString();

                // Jacks
                if (i == 1 && j == 11) card.GetComponent<Image>().sprite = sprites[4];
                if (i == 2 && j == 11) card.GetComponent<Image>().sprite = sprites[5];
                if (i == 3 && j == 11) card.GetComponent<Image>().sprite = sprites[6];
                if (i == 4 && j == 11) card.GetComponent<Image>().sprite = sprites[7];

                // Queens
                if (i == 1 && j == 12) card.GetComponent<Image>().sprite = sprites[8];
                if (i == 2 && j == 12) card.GetComponent<Image>().sprite = sprites[9];
                if (i == 3 && j == 12) card.GetComponent<Image>().sprite = sprites[10];
                if (i == 4 && j == 12) card.GetComponent<Image>().sprite = sprites[11];

                // Kings
                if (i == 1 && j == 13) card.GetComponent<Image>().sprite = sprites[12];
                if (i == 2 && j == 13) card.GetComponent<Image>().sprite = sprites[13];
                if (i == 3 && j == 13) card.GetComponent<Image>().sprite = sprites[14];
                if (i == 4 && j == 13) card.GetComponent<Image>().sprite = sprites[15];

                deck.Add(card);
                card.transform.SetParent(deckArea.transform, false);
            }
        }
    }

    public int GetGameListCount()
    {
        return gameList.Count;
    }

    // Taken From: https://answers.unity.com/questions/486626/how-can-i-shuffle-alist.html
    void FisherYatesCardDeckShuffle()
    {
        System.Random _random = new System.Random();
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


    void OnDisable()
    {
        if (addCard != null)
        {
            addCard.RemoveListener(AddCardObjectToList);
            addCard.RemoveListener(MoveCardToAreaUI);
        }
    }

    #region Events
    [System.Serializable]
    public class AddGameObjectToListEvent : UnityEvent<GameObject, CardList> { }
    #endregion
}