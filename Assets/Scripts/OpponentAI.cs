using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OpponentAI : MonoBehaviour
{
    private bool moveCard;
    private int cardIndex;
    public HashSet<int> opponentCardsValues = new HashSet<int>();

    private int[] playedCards;
    // Opponent AI uses this variable to check if it should play Jack and get all the cards on the table.
    private int jackThreshold;

    private static OpponentAI instance;
    public static OpponentAI Instance { get { return instance; } }
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

    void Start()
    {
        moveCard = false;
        jackThreshold = 3;
        playedCards = new int[13];
    }

    void Update()
    {
        // Move Card Animation and PlayCard Logic
        if (moveCard)
        {
            float step = DeckManager.Instance.speed * Time.deltaTime;
            DeckManager.Instance.CurrentCard.transform.position = Vector3.MoveTowards(DeckManager.Instance.CurrentCard.transform.position, DeckManager.Instance.target.position, step);
            if (DeckManager.Instance.CurrentCard.transform.position == DeckManager.Instance.target.position)
            {
                moveCard = false;
                DeckManager.Instance.OpenCard(DeckManager.Instance.opponentList[cardIndex]);
                GameSystem.Instance.PlayGame(DeckManager.Instance.opponentList[cardIndex]);
                UpdateOpponentCardValues();
            }
        }
    }

    public IEnumerator Play()
    {
        yield return new WaitForSeconds(0.1F);
        cardIndex = ChooseCardToPlay();
        DeckManager.Instance.CurrentCard = DeckManager.Instance.opponentList[cardIndex];
        moveCard = true;
    }

    // Use the rank of the cards as value to find if Opponent has same card on table
    public void UpdateOpponentCardValues()
    {
        opponentCardsValues.Clear();
        for (int i = 0; i < DeckManager.Instance.opponentList.Count; i++)
        {
            opponentCardsValues.Add(DeckManager.Instance.GetCardValue(DeckManager.Instance.opponentList[i])[1]);
        }
    }


    // This function chooses which card to play:
    // It evaluates the situations from the most important to least important
    // If best possible card to play is not available, function goes to another case
    // And tries to play that case
    int ChooseCardToPlay()
    {
        int iterator = 0;

        // Opponent can't know first three cards that's on the table because they'll be closed
        if (DeckManager.Instance.FirstHandWon)
        {
            iterator = 0;
        }
        else if (!DeckManager.Instance.FirstHandWon)
        {
            iterator = 3;
        }

        // If opponent has the card on the table, take it
        if (opponentCardsValues.Contains(DeckManager.Instance.GetCardOnTopValue()[1]))
        {
            return ValueToCard(DeckManager.Instance.GetCardOnTopValue()[1]);
        }

        // If there's Two of Clubs or Ten of Diamonds and Opponent has Jack, take it with jack.
        for (int i = iterator; i < DeckManager.Instance.GetGameListCount(); i++)
        {
            // Ten of Diamonds
            if ((DeckManager.Instance.gameList[i].GetComponent<Card>().Suit == DeckManager.DIAMOND && DeckManager.Instance.gameList[i].GetComponent<Card>().Rank == 10)
            ||
            (DeckManager.Instance.gameList[i].GetComponent<Card>().Suit == DeckManager.CLUB && DeckManager.Instance.gameList[i].GetComponent<Card>().Rank == 2)
            ||
            (DeckManager.Instance.gameList[i].GetComponent<Card>().Rank == DeckManager.ACE)
            )
            {
                if (opponentCardsValues.Contains(DeckManager.JACK))
                {
                    return ValueToCard(DeckManager.JACK);
                }
            }
        }

        // If there are more than jackThreshold cards and has the card on top or Jack, Opponent takes it.
        if (DeckManager.Instance.GetGameListCount() > jackThreshold)
        {
            if (opponentCardsValues.Contains(DeckManager.JACK))
            {
                return ValueToCard(DeckManager.JACK);
            }
        }

        // If Jack is first card at hand, try not to play first card
        // Extreme case
        if (ValueToCard(DeckManager.JACK) == 0)
        {
            int o = DeckManager.Instance.opponentList.Count;
            if (o > 1) return 1;
            if (o > 2) return 2;
            if (o > 3) return 3;
        }

        // Opponent AI counts the cards and plays accordingly
        int val = MostPlayedCard();
        int ind = ValueToCard(val);
        return ind;
    }

    // TODO: This should return an array of integers for duplicate values, and maybe should be in DeckManager and more abstract.
    // Return the first card that has the given value as parameter
    int ValueToCard(int value)
    {
        for (int i = 0; i < DeckManager.Instance.opponentList.Count; i++)
        {
            if (value == DeckManager.Instance.GetCardValue(DeckManager.Instance.opponentList[i])[1])
            {
                return i;
            }
        }
        return 0;
    }

    int MostPlayedCard()
    {
        int max = 0;
        int index = 0;
        for (int i = 0; i < DeckManager.Instance.playedList.Count; i++)
        {
            int value = DeckManager.Instance.GetCardValue(DeckManager.Instance.playedList[i])[1];
            // Index shifting
            value -= 1;
            playedCards[value]++;
        }

        for (int i = 0; i < playedCards.Length; i++)
        {
            if (playedCards[i] > max)
            {
                index = i;
            }
        }

        // Clear playedCardsList
        for (int i = 0; i < playedCards.Length; i++)
        {
            playedCards[i] = 0;
        }

        // Index shifting
        return index + 1;
    }

}
