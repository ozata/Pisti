using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    public GameObject buttons;

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

    public void ShowButtons()
    {
        buttons.SetActive(true);
    }

    public void RestartGame()
    {
        DeckManager.Instance.playerList.Clear();
        DeckManager.Instance.opponentList.Clear();
        DeckManager.Instance.playerWinList.Clear();
        DeckManager.Instance.opponentWinList.Clear();
        DeckManager.Instance.closedList.Clear();
        DeckManager.Instance.gameList.Clear();
        DeckManager.Instance.InitDecks();

        buttons.SetActive(false);
        ScoreManager.Instance.CloseScoreText();
        GameSystem.Instance.state = GameState.PLAYERTURN;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
