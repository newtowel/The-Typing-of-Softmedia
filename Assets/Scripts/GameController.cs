using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;
public class GameController : MonoBehaviour
{
    [SerializeField]
    Text ProblemKana;
    [SerializeField]
    Text ProblemText;
    //入力した文字を表示するText
    [SerializeField]
    Text CorrectRomaji;
    //入力を受け付けてよいか
    private bool isInputValid { get; set; }
    //文字を入力し始めてからの経過時間
    private float firstCharInputTime { get; set; }
    //その文字が最初の人文字目であるかのチェック
    private bool isFirstInput { get; set; }
    //変換辞書をもとに生成された入力候補リスト
    private static List<List<string>> AnswerList { get; set; }
    //出題文字列を文字ごとに区切ったリスト
    private List<string> CharList { get; set; }
    //ローマ字仮名対応辞書
    private Dictionary<string, string[]> RomanMap { get; set; }
    //上の文字が追加された時刻（平均秒速打数算出用？）
    private Queue<float> timeQueue = new Queue<float>();
    private readonly string romajiKanaMapPath = Application.streamingAssetsPath + "/roman_map.json";
    //データベース名・テーブル名。問題取得時に用いる。暫定版
    private readonly string tableName = "trend_words";
    private readonly string dbPath = Application.streamingAssetsPath + "/jp_sentence.db";
    private static int spellIndex = 0;
    private static int kanaIndex = 0;
    private static bool isMark = false;


    // Start is called before the first frame update
    void Start()
    {
        ProblemText = transform.Find("ProblemText").GetComponent<Text>();
        ProblemKana = transform.Find("ProblemKana").GetComponent<Text>();
        CorrectRomaji = transform.Find("CorrectRomaji").GetComponent<Text>();
        OutputQ();
    }

    void OutputQ()
    {
        //問題のセット
        var ag = new AnswerGenerator(romajiKanaMapPath, dbPath, tableName);
        ProblemKana.text = ag.QuestionKanaSpelling;
        ProblemText.text = ag.QuestionText;
        AnswerList = ag.AnswerRomajiInputSpellingList;
        CharList = ag.CharList;
        RomanMap = ag.RomajiKanaMap;
        CorrectRomaji.text = "";
        isInputValid = true;
        /*
        foreach (var item in AnswerList)
        {
            foreach (var item2 in item)
            {
                Debug.Log(item2 + "");
            }    
        }
        */

    }

    void OnGUI()

    {
        Event e = Event.current;

        //キー入力が有効な状態でキーが押下され（キーが上がった瞬間も除外）、何らかの（既知の）キーコードが認識
        if (isInputValid && e.type == EventType.KeyDown && e.type != EventType.KeyUp && e.keyCode != KeyCode.None && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            var keyCode = e.keyCode.ToString();

            //新しい問題文が表示されてから1文字目を打つまでのレイテンシは計測時間に含めない
            if (isFirstInput)
            {
                firstCharInputTime = Time.realtimeSinceStartup;
                isFirstInput = false;
            }
            //inputQueue.Enqueue(e.keyCode);
            timeQueue.Enqueue(Time.realtimeSinceStartup);
            Judge(keyCode); 
        }
    }

    
    void Judge(string input)
    {
        //正解のローマ字入力候補のいずれかが入力ローマ字と一致する
        if (AnswerList[kanaIndex].Count(IsMatchWithInputPeek) != 0)
        {
            //現在の入力に一致しない正解候補リストを排除
            AnswerList[kanaIndex].RemoveAll(IsNOTMatchWithInputPeek);

            foreach (string charCandidate in AnswerList[kanaIndex])
            {
                //正解したローマ字を出力
                //記号の場合、入力が特殊なので入力をそのまま出力できない
                if (isMark)
                {
                    //ローマ字仮名変換表の中で入力を要素に持つキーを出力
                    CorrectRomaji.text += RomanMap.First(x => x.Value.Contains(input)).Key;
                    //判定(実際に入力される)文字列は1文字でないので、見るインデックスを飛ばし、次巡で次のかなのチェックに行くように
                    spellIndex = charCandidate.Length;
                    isMark = false;
                }
                else
                {
                    CorrectRomaji.text += input;
                    //検証する入力ローマ字及び正解ローマ字を次へ
                    spellIndex++;
                }

                //一文字分チェックし終われば次の文字へ
                if (spellIndex == charCandidate.Length)
                {
                    spellIndex = 0;
                    kanaIndex++;
                    //出題文の末尾まで回答を終えたら次の問題を出題
                    if (kanaIndex == AnswerList.Count)
                    {
                        kanaIndex = 0;
                        //Debug.Log("次の問題は"+);
                        OutputQ();
                    }

                }
                break;
            }
        }
        else
        {

            Debug.Log("ミスタイプ : " + input);
            
        }
        
        //正解ローマ字入力候補が入力と一致するか
        bool IsMatchWithInputPeek(string s)
        {
            //次の候補文字列の1文字目と一致→かなである
            if (input == s[spellIndex].ToString())
            {
                Debug.Log(input+"はかな");
                return true;
            }
            //1文字目と一致せず、かつ全体と一致→記号である
            else if (input == s)
            {
                Debug.Log(input+"は記号");
                isMark = true;
                return true;
            }
            //いづれとも一致しない→ミスタイプ
            else
            {
                return false;
            }
        }
        //入力と不一致か
        bool IsNOTMatchWithInputPeek(string s)
        {
            return !IsMatchWithInputPeek(s);
        }
    }


    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      UnityEngine.Application.Quit();
#endif
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) Quit();
    }
}
