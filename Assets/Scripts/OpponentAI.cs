using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OpponentAI : MonoBehaviour
{

    public HashSet<int> opponentCardsValues = new HashSet<int>();

    private static OpponentAI instance;
    public static OpponentAI Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }

    // Possible BUG: If I put Q(or any other card) on table, and win the hand and then opponent puts Q, opponent wins when there is 1 card on table
    public IEnumerator Play(){
        ChooseCardToPlay();
        yield return new WaitForSeconds(0.8F);
        int cardIndex = ChooseCardToPlay();
        DeckManager.Instance.OpenCard(DeckManager.Instance.opponentList[cardIndex]);
        GameSystem.Instance.PlayGame(DeckManager.Instance.opponentList[cardIndex]);
        UpdateOpponentCardValues();
    }

    public void UpdateOpponentCardValues() {
        opponentCardsValues.Clear();
        for(int i = 0 ; i < DeckManager.Instance.opponentList.Count ; i++){
            opponentCardsValues.Add(DeckManager.Instance.GetCardValue(DeckManager.Instance.opponentList[i])[1]);
        }
    }


    // TODO: AI shoudl have more Intelligence and this code should be refactored
    int ChooseCardToPlay(){
        //opponentCardsValues.ToList<int>().ForEach(x => print(x));
        if(opponentCardsValues.Contains(DeckManager.Instance.GetCardOnTopValue()[1]) && DeckManager.Instance.gameList.Count == 1){
            print("pişti play");
            return ValueToCard(DeckManager.Instance.GetCardOnTopValue()[1]);
        } else if(opponentCardsValues.Contains(DeckManager.JACK) && DeckManager.Instance.opponentList.Count > 1){
            int jackIndex = ValueToCard(DeckManager.JACK);
            if(DeckManager.Instance.opponentList.Count == 2 && jackIndex == 0){
                return 1;
            } else if(DeckManager.Instance.opponentList.Count == 3 && jackIndex == 0 &&
            DeckManager.Instance.GetCardValue(DeckManager.Instance.opponentList[2])[1] != DeckManager.JACK) {
                return 2;
            }
        }
        return 0;
    }

    // TODO: This should return an array of integers for duplicate values, and maybe should be in DeckManager and more abstract.
    // Return the first card that has the given value as parameter
    int ValueToCard(int value) {
        for(int i = 0 ; i < DeckManager.Instance.opponentList.Count ; i++){
            if( value == DeckManager.Instance.GetCardValue(DeckManager.Instance.opponentList[i])[1]){
                return i;
            }
        }
        return 0;
    }

}
