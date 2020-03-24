using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField]
    Text Correct;

    [SerializeField]
    Text Wrong;

    [SerializeField]
    Text Acc;
    // Start is called before the first frame update
    void Start()
    {
        Correct = transform.Find("Correct").GetComponent<Text>();
        Wrong = transform.Find("Wrong").GetComponent<Text>();
        Acc = transform.Find("CorrectRate").GetComponent<Text>();

        int correct = GameController.CorrectNum;
        int miss = GameController.MissNum;
        Correct.text += correct;
        Wrong.text += miss;
        float acc = (float)correct / (correct + miss);
        Debug.Log(acc);
        Acc.text += acc.ToString();
    }
}
