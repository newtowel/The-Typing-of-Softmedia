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

public class GameController : MonoBehaviour
{
    public Text ProblemKana;
    public Text ProblemText;
    //入力した文字を表示するText
    public Text CorrectRomaji;
    //入力を受け付けてよいか
    private bool isInputValid;
    //文字を入力し始めてからの経過時間
    private float firstCharInputTime;
    //その文字が最初の人文字目であるかのチェック
    private bool isFirstInput;
    //変換辞書をもとに生成された入力候補リスト
    private static List<List<string>> AnswerList;
    
    //入力されたローマ字を保持するキュー
    private Queue<KeyCode> inputQueue = new Queue<KeyCode>();
    //上の文字が追加された時刻（平均秒速打数算出用？）
    private Queue<float> timeQueue = new Queue<float>();
    private readonly string romajiKanaMapPath = Application.streamingAssetsPath + "/roman_map.json";
    //データベース名・テーブル名。問題取得時に用いる。暫定版
    private readonly string tableName = "trend_words";
    private readonly string dbPath = Application.streamingAssetsPath + "/jp_sentence.db";
    private static int spellIndex = 0;
    private static int kanaIndex = 0;



    // Start is called before the first frame update
    void Start()
    {
        ProblemText = transform.Find("ProblemText").GetComponent<Text>();
        ProblemKana = transform.Find("ProblemKana").GetComponent<Text>();
        OutputQ();
    }

    void OutputQ()
    {
        //問題のセット
        var ag = new AnswerGenerator(romajiKanaMapPath, dbPath, tableName);
        ProblemKana.text = ag.QuestionKanaSpelling;
        ProblemText.text = ag.QuestionText;
        AnswerList = ag.AnswerRomajiInputSpellingList;
        CorrectRomaji.text = "";
        isInputValid = true;
        /*確認用
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
            //var keyCode = e.keyCode;

            //新しい問題文が表示されてから1文字目を打つまでのレイテンシは計測時間に含めない
            if (isFirstInput)
            {
                firstCharInputTime = Time.realtimeSinceStartup;
                isFirstInput = false;
            }
            inputQueue.Enqueue(e.keyCode);
            timeQueue.Enqueue(Time.realtimeSinceStartup);
            Judge(); 
        }
    }

    
    void Judge()
    {
        //正解のローマ字入力候補のいずれかが入力ローマ字と一致する
        if (AnswerList[kanaIndex].Count(IsMatchWithInputPeek) != 0)
        {
            //正解したローマ字を出力
            CorrectRomaji.text += inputQueue.Peek().ToString();
            
            //現在の入力に一致しない正解候補リストを排除
            AnswerList[kanaIndex].RemoveAll(IsNOTMatchWithInputPeek);

            //検証する入力ローマ字及び正解ローマ字を次へ
            inputQueue.Dequeue();
            try
            {
                spellIndex++;

            }
            //
            catch (IndexOutOfRangeException)
            {
                kanaIndex++;
                spellIndex = 0;
            }
        }
        else
        {

            Debug.Log("ミスタイプ");

        }

        //正解ローマ字入力候補が入力と一致するか
        bool IsMatchWithInputPeek(string s)
        {
            return s[spellIndex].ToString() == inputQueue.Peek().ToString();
        }
        //入力と不一致か
        bool IsNOTMatchWithInputPeek(string s)
        {
            return s[spellIndex].ToString() != inputQueue.Peek().ToString();

        }
    }

    
}
