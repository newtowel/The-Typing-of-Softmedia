using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TypingSystem
{
    /// <summary>
    /// 挑戦結果スコア一覧の表示。
    /// </summary>
    public class Result : MonoBehaviour
    {
        /// <summary>
        /// 挑戦結果のうち正解数以外の結果の文字列
        /// </summary>
        public static Dictionary<string, string> ReferenceScores { get; private set; } = new Dictionary<string, string> {
                {"Typo.", ""},
                {"Acc.", ""},
                {"KPS", ""},
                {"Combo",""},
                {"IT", ""}
        };
        
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
        //連続正解入力数
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
            
            ReferenceScores["Typo."] = Solution.Mistake + "回";
            ReferenceScores["Combo"] = Solution.MaxCombo + "回";

            //キー入力時刻キューを読み込み
            List<float> inputTimes = Solution.TimeQueue.ToList();
            //隣り合った入力時刻の差分をとることで得られる、1文字当たりの入力時間リスト
            List<float> deltas = new List<float>();
            List<float> initialTimes = new List<float>();

            Correct.text += Solution.Correct;
            Wrong.text += ReferenceScores["Typo."];
            Combo.text += ReferenceScores["Combo"];

            ReferenceScores["Acc."] = Math.Round((double)Solution.Correct / (Solution.Correct + Solution.Mistake), 3, MidpointRounding.AwayFromZero) * 100 + "%";
            Accuracy.text += ReferenceScores["Acc."];

            //現状レイテンシの考慮の可能性から各入力時刻の差をとっているが、レイテンシを考慮しない場合 本来、打鍵数/制限時間でよい
            for (int i = 0; i < inputTimes.Count - 1; i++)
            {
                deltas.Add(inputTimes[i + 1] - inputTimes[i]);
            }
            ReferenceScores["KPS"] = CalculateMeanValueOfList(deltas) + "回/秒";
            MKPS.text += ReferenceScores["KPS"];

            for (int i = 0; i < Solution.FirstCharInputTime.Count; i++)
            {
                initialTimes.Add(Solution.FirstCharInputTime[i] - Solution.ProblemShownTime[i]);
            }
            ReferenceScores["IT"] = CalculateMeanValueOfList(initialTimes) + "秒";
            InitialSpeed.text += ReferenceScores["IT"];

            CalculateWeakKeysRank(Solution.WeakKeys);
        }
        void Update()
        {
            TToSUtils.QuitOnEsc();
            TToSUtils.BlinkForSceneTransition(GuidanceToRanking, "Ranking");
        }

        private void CalculateWeakKeysRank(List<char> weakKeys)
        {
            var weakKeyRank = new Dictionary<char, int>();
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

        private float CalculateMeanValueOfList(List<float> list)
        {
            return (float)Math.Round(1f / list.Average(), 2, MidpointRounding.AwayFromZero);
        }
    }

}