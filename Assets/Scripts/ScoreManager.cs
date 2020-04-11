using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    //正解数
    [SerializeField]
    Text Correct;
    //ミスタイプ数
    [SerializeField]
    Text Wrong;
    //正確率
    [SerializeField]
    Text Accuracy;
    //平均秒速キータイプ数
    [SerializeField]
    Text MKPS;
    [SerializeField]
    Text Combo;
    //平均初速
    [SerializeField]
    Text InitialSpeed;
    //苦手キー
    [SerializeField]
    Text WeakKeys;
    //ランキング表示シーンへの案内
    [SerializeField]
    Text GuidanceToRanking;
    // Start is called before the first frame update
    void Start()
    {
        int correct = TypingSystem.CorrectNum;
        int miss = TypingSystem.MissNum;
        float acc, mips;
        //キー入力時刻キューを読み込み
        List<float> inputTimes = TypingSystem.TimeQueue.ToList<float>();
        //隣り合った入力時刻の差分をとることで得られる、1文字当たりの入力時間リスト
        List<float> deltas = new List<float>();
        List<float> firstInputTimes = TypingSystem.FirstCharInputTime;
        List<float> problemShownTimes = TypingSystem.ProblemShownTime;
        List<float> initialTimes = new List<float>();
        List<char> weakKeys = TypingSystem.WeakKeys;
        Dictionary<char, int> weakKeyRank = new Dictionary<char, int>();

        Correct.text += correct + "回";
        Wrong.text += miss + "回";
        Combo.text += TypingSystem.MaxCombo + "回";
        
        acc = (float)correct / (correct + miss);
        Accuracy.text += Math.Round(acc, 3, MidpointRounding.AwayFromZero) * 100.0 + "％";


        for (int i = 0; i < inputTimes.Count - 1; i++)
        {
            deltas.Add(inputTimes[i + 1] - inputTimes[i]);
        }
        mips = 1f / deltas.Average();
        MKPS.text += Math.Round(mips, 2, MidpointRounding.AwayFromZero) + "回/秒";

        for (int i = 0; i < firstInputTimes.Count; i++)
        {
            initialTimes.Add(firstInputTimes[i] - problemShownTimes[i]);
        }
        InitialSpeed.text += Math.Round(initialTimes.Average(), 2, MidpointRounding.AwayFromZero ) + "秒";

        foreach (char key in weakKeys)
        {
            //以前に間違えたことがある（すでに検知されている）キーならその回数を増加
            if (weakKeyRank.ContainsKey(key))
            {
                weakKeyRank[key]++;
            }
            else
            {
                //そのキーが初めて検知されたとき、辞書に1回として追加
                weakKeyRank.Add(key, 1);
            }
        }
        //ミス回数が3回以上のキーをミスタイプ回数が多い順に5種類抽出
        var sortedRank = weakKeyRank.OrderByDescending(x => x.Value).Where(x => x.Value >= 3).Take(5);
        foreach (var item in sortedRank)
        {
            WeakKeys.text += item.Key + "(" + item.Value + ") ";
        }
    }
    void Update()
    {
        TToSUtils.QuitOnEsc();
        TToSUtils.BlinkForSceneTransition(GuidanceToRanking, "Ranking");
    }


}
