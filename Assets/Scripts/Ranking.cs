using System;
using System.Collections.Generic;
using TypingSystem;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

/// <summary>
/// 結果スコアの歴代ランキング処理。
/// </summary>
public class Ranking : MonoBehaviour
{
    //今とった正解数スコアとその他のスコアの文字列
    [Serializable]
    private class Scores
    {
        public int _Correct;
        [SerializeField]
        public Dictionary<string, string> _ReferenceScores;
    }

    //ランキングを初期化する場合の初期値
    private readonly Scores _InitialScores = new Scores()
    {
        _Correct = 0,
        _ReferenceScores = new Dictionary<string, string> {
               { "Typo.", "0回" },
               { "Acc.", "0%" },
               { "KPS", "0回/秒" },
               { "Combo", "0回" },
               { "IT", "0秒" }
            }
    };

    //現状のランクインスコアたち
    [SerializeField]
    private Scores[] _RankingScores  = new Scores[5];
    private static readonly string[] _RankingLabels = { "1位", "2位", "3位", "4位", "5位" };
    [SerializeField]
    Text RankingText;
    [SerializeField]
    Text ReturnGuidance;

    // Start is called before the first frame update
    void Start()
    {
        //ResetRanking();
        Scores currentScore = new Scores() { _Correct = Solution.Correct, _ReferenceScores = Result.ReferenceScores };
        //RankingLabelsに沿って保存した現在ランクインしているスコアたちを読み込み
        GetObject();
        //今とったスコアと合わせてRankingScoresを更新
        SetObject(currentScore);

		for (int i = 0; i < _RankingLabels.Length; i++)
        {
            RankingText.text += "\n\n" + (i + 1) + "位：";
            //今とったスコアがランクインした場合、それはオレンジで示す
            if (_RankingScores[i] == currentScore) RankingText.text += "<color=orange>";
            RankingText.text += _RankingScores[i]._Correct;
            RankingText.text += "\n(";
            foreach (KeyValuePair<string, string> referenceScore in _RankingScores[i]._ReferenceScores)
            {
                RankingText.text += referenceScore.Key + "：" + referenceScore.Value + " ";
            }
            RankingText.text += ")";
            if (_RankingScores[i] == currentScore) RankingText.text += "</color>";
        }

    }

	// Update is called once per frame
	void Update()
    {
        TToSUtils.QuitOnEsc();
        TToSUtils.BlinkForSceneTransition(ReturnGuidance, "Title");

    }

    /// <summary>
    /// 指定されたオブジェクトの情報を読み込みます
    /// </summary>
    private void GetObject()
    {
        
        //JSONで保持されているスコアたちをScores型にデシリアライズ・RankingScoresに代入
        for (int i = 0; i < _RankingLabels.Length; i++)
        {
            //セーブされている暫定ランクデータを取得・データがなければ、初期化直後用のすべて0のデータを取得
            string json = PlayerPrefs.GetString(_RankingLabels[i], JsonConvert.SerializeObject(_InitialScores));
            _RankingScores[i] = JsonConvert.DeserializeObject<Scores>(json);
            Debug.Log("暫定ランク：" + json);
        }
    }


    /// <summary>
    /// 指定されたオブジェクトの情報を保存します
    /// </summary>
    private void SetObject(Scores score)
    {    
       
        //今とった正解数スコアを現在ランクインしているスコア群のうちの正解数を1位から順に比較していき、今とったスコアが上回ったら順次入れ替えてRankingScoresを更新   
        for (int i = 0; i < _RankingScores.Length; i++)
        {
            if (score._Correct > _RankingScores[i]._Correct)
            {
                var tmp = _RankingScores[i];
                _RankingScores[i] = score;
                score = tmp;
            }
        }

        
         //RankingScoresをJSONにして保存
        for (int i = 0; i < _RankingScores.Length; i++)
        {
            var json = JsonConvert.SerializeObject(_RankingScores[i]);
            Debug.Log("更新済み・保存：" + json);
            PlayerPrefs.SetString(_RankingLabels[i], json);
            PlayerPrefs.Save();
    
        }
    }

    public static void ResetRanking()
    {
        foreach (var item in _RankingLabels)
        {
            PlayerPrefs.DeleteKey(item);
        }
    }
}
