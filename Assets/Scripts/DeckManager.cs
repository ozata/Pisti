using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour {
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
    public List<GameObject> deck;

    public Sprite[] sprites;

    void Start() {
        CreateDeck();
        FisherYatesCardDeckShuffle();
        Debug.Log(deck[0].GetComponent<Card>().Rank);
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
                card.transform.SetParent(playerArea.transform, false);
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

}