using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentAI : MonoBehaviour
{
    public IEnumerator Play(){
        yield return new WaitForSeconds(0.1F);
        DeckManager.Instance.CloseOrOpenCard(DeckManager.Instance.opponentList[0]);
        GameSystem.Instance.PlayGame(DeckManager.Instance.opponentList[0]);
    }

    void ChooseCardToPlay(){

    }
}
