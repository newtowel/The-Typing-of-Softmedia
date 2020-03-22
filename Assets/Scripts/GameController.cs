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
    [SerializeField]
    Text Combo;
    //ミスタイプ時エフェクト
    [SerializeField]
    Image MissEffect;
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
    private readonly string romajiKanaMapPath = Application.streamingAssetsPath + "/roman_map2.json";
    //データベース名・テーブル名。問題取得時に用いる。暫定版
    private readonly string tableName = "trend_words";
    private readonly string dbPath = Application.streamingAssetsPath + "/jp_sentence.db";
    private static int spellIndex = 0;
    private static int kanaIndex = 0;
    private static int comboNum = 0;
    private static int maxCombo = 0;
    private static int correctNum = 0;
    private static int wrongNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        ProblemText = transform.Find("ProblemText").GetComponent<Text>();
        ProblemKana = transform.Find("ProblemKana").GetComponent<Text>();
        CorrectRomaji = transform.Find("CorrectRomaji").GetComponent<Text>();
        Combo = transform.Find("Combo").GetComponent<Text>();
        MissEffect = transform.Find("MissEffect").GetComponent<Image>();
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
        
    }

    void OnGUI()

    {
        Event e = Event.current;
        //キー入力が有効な状態でキーが押下され（キーが上がった瞬間も除外）、何らかの（既知の）キーコードが認識
        if (isInputValid && e.type == EventType.KeyDown && e.type != EventType.KeyUp && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            char inputChar = e.character;

            //新しい問題文が表示されてから1文字目を打つまでのレイテンシは計測時間に含めない
            if (isFirstInput)
            {
                firstCharInputTime = Time.realtimeSinceStartup;
                isFirstInput = false;
            }
            //inputQueue.Enqueue(e.keyCode);
            timeQueue.Enqueue(Time.realtimeSinceStartup);
            if (inputChar != '\0')
            {
                Judge(inputChar);
            }
        }
    }

    
    void Judge(char input)
    {
        //正解のローマ字入力候補のいずれかが入力ローマ字と一致する
        if (AnswerList[kanaIndex].Count(IsMatchWithInputPeek) != 0)
        {
            correctNum++;
            comboNum++;
            Combo.text = comboNum + " Combo!";
            if (comboNum > maxCombo)
            {
                maxCombo = comboNum;
            }
            //現在の入力に一致しない正解候補リストを排除
            AnswerList[kanaIndex].RemoveAll(IsNOTMatchWithInputPeek);

            foreach (string charCandidate in AnswerList[kanaIndex])
            {
                CorrectRomaji.text += input;
                spellIndex++;

                //一文字分チェックし終われば次の文字へ
                if (spellIndex == charCandidate.Length)
                {
                    spellIndex = 0;
                    kanaIndex++;
                    //出題文の末尾まで回答を終えたら次の問題を出題
                    if (kanaIndex == AnswerList.Count)
                    {
                        kanaIndex = 0;
                        OutputQ();
                    }

                }
                break;
            }
        }
        else
        {
            comboNum = 0;
            wrongNum++;
            StartCoroutine(FlashOnMiss());
            Combo.text = "";
            Debug.Log("ミスタイプ : " + input);
            
        }
        
        //正解ローマ字入力候補が入力と一致するか
        bool IsMatchWithInputPeek(string s)
        {
            return input == s[spellIndex];
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

    IEnumerator FlashOnMiss()
    {
        MissEffect.enabled = true;
        yield return new WaitForSeconds(0.2f);
        MissEffect.enabled = false;
    }
}
