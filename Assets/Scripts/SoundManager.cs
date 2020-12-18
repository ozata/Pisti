using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    private static SoundManager instance;
    public static SoundManager Instance { get { return instance; } }

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

    public AudioSource cardPlay;
    public AudioSource cardWin;

    public void PlayPlayCard()
    {
        cardPlay.Play();
    }

    public void PlayCardWin()
    {
        cardWin.Play();
    }

}
