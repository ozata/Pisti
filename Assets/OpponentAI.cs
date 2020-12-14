using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentAI : MonoBehaviour
{
    // This might be better as a Hashmap<String,int>
    public List<GameObject> opponentList;

    // This is the core AI function of the opponent
    public IEnumerator Play(){
        GameObject card = opponentList[0];
        // Remove card from the opponentList List.
        opponentList.Remove(card);
        // Wait for a second to simulate human thought process
        yield return new WaitForSeconds(1F);
        // Opponent Card will be closed so open the card UI
        DeckManager.Instance.CloseOrOpenCard(card);
        // Play the card
        GameSystem.Instance.PlayGame(card);
    }


    void ChooseCardToPlay(){
        
    }
}
