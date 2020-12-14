using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;


public class Card : MonoBehaviour
{

    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite {
        get; set;
    }

    private bool closed;
    public bool Closed {
        get; set;
    }

    [SerializeField]
    [Range(1, 12)]
    private int suit;
    public int Suit {
        get; set;
    }

    [SerializeField]
    [Range(1, 12)]
    private int rank;
    public int Rank {
        get; set;
    }

}