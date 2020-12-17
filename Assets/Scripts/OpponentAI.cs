using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentAI : MonoBehaviour
{
    // Possible BUG: If I put Q(or any other card) on table, and win the hand and then opponent puts Q, opponent wins when there is 1 card on table
    public IEnumerator Play(){
        yield return new WaitForSeconds(0.1F);
        DeckManager.Instance.OpenCard(DeckManager.Instance.opponentList[0]);
        GameSystem.Instance.PlayGame(DeckManager.Instance.opponentList[0]);
    }

    void ChooseCardToPlay(){

    }
}
