using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ranking : MonoBehaviour
{
    private readonly int Point = TypingSystem.CorrectNum;
    private string[] Rank { get; } = { "1位", "2位", "3位", "4位", "5位" };
    private int[] RankingValue { get; set; } = new int[5];
    [SerializeField]
    Text RankingText;
    [SerializeField]
    Text ReturnGuidance;

    // Start is called before the first frame update
    void Start()
    {
        GetRanking();
        SetRanking(Point);
        for (int i = 0; i < Rank.Length; i++)
        {
            RankingText.text += "\n \n " + RankingValue[i].ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        TToSUtils.QuitOnEsc();
        TToSUtils.BlinkForSceneTransition(ReturnGuidance, "Title");

    }

    private void GetRanking(){
        for (int i = 0; i < Rank.Length; i++)
        {
            RankingValue[i] = PlayerPrefs.GetInt(Rank[i]);
        }
    }

    //今のスコアがランキング入りしていれば、ランキングを更新
    private void SetRanking(int value)
    {
        for (int i = 0; i < Rank.Length; i++)
        {
            if (value > RankingValue[i])
            {
                var tmp = RankingValue[i];
                RankingValue[i] = value;
                value = tmp;
            }
        }

        for (int i = 0; i < Rank.Length; i++)
        {
            PlayerPrefs.SetInt(Rank[i], RankingValue[i]);
        }
    }
}
