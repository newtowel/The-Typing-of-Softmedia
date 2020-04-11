using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class TypingSystem : MonoBehaviour
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
    Image MistakeEffect;
    [SerializeField]
    Text Timer;

    public static int MaxCombo { get; private set; }
    public static int CorrectNum { get; private set; }
    public static int MissNum { get; private set; }
    public static Queue<float> TimeQueue { get; private set; }
    //入力を受け付けてよいか
    public static bool IsInputValid { get; private set; }
    //新しい問題が表示された時刻
    public static List<float> ProblemShownTime { get; private set; }
    //その問題の1文字目を入力した時刻
    public static List<float> FirstCharInputTime { get; private set; }
    //苦手キー
    public static List<char> WeakKeys { get; private set; }
    //その文字が問題の１文字目であるかのチェック
    private bool IsFirstInput { get; set; } = true;
    //変換辞書をもとに生成された入力候補リスト
    private List<List<string>> AnswerList { get; set; }
    //半角スペルの位置
    private int SpellingIndex { get; set; } = 0;
    //何文字目のかなについて打っているか
    private int KanaIndex { get; set; } = 0;
    //上の文字が追加された時刻（平均秒速打数算出用？）
    private string RomajiKanaMapPath { get; } = Application.streamingAssetsPath + "/roman_map.json";
    //データベース名・テーブル名。問題取得時に用いる。暫定版
    private string TableName { get; } = "trend_words";
    private string DbPath { get; } = Application.streamingAssetsPath + "/jp_sentence.db";
    private int ComboNum { get; set; } = 0;

    //制限時間カウントダウン用
    private float TotalTime = 60;
    private int Seconds;

    //「ん」の例外処理用
    private bool AcceptSingleN { get; set; } = false;
    //nでもよい「ん」にて2回目のnを入力したか
    private bool IsInput2ndN { get; set; } = false;
    
    // Start is called before the first frame update
    void Start()
    {
        MaxCombo = 0;
        CorrectNum = 0;
        MissNum = 0;
        TimeQueue = new Queue<float>();
        IsInputValid = false;
        ProblemShownTime = new List<float>();
        FirstCharInputTime = new List<float>();
        WeakKeys = new List<char>();
        OutputQ();
    }

    void Update()
    {
        TToSUtils.QuitOnEsc();
        TToSUtils.CountDownToSceneTransition(Timer,ref Seconds, ref TotalTime, "Result");
    }

    private void OutputQ()
    {
        //問題のセット
        var ag = new AnswerGenerator(RomajiKanaMapPath, DbPath, TableName);
        ProblemKana.text = ag.QuestionKanaSpelling;
        ProblemText.text = ag.QuestionText;
        AnswerList = ag.AnswerRomajiInputSpellingList;
        CorrectRomaji.text = "";
        //入力補助用アルファベット表示
        foreach (List<string> item in AnswerList)
        {
            CorrectRomaji.text += item[0];
        }
        IsInputValid = true;
        IsFirstInput = true;
        ProblemShownTime.Add(Time.realtimeSinceStartup);
    }

    void OnGUI()
    {
        Event e = Event.current;
        
        if (IsInputValid && e.type == EventType.KeyDown && e.type != EventType.KeyUp && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            char inputChar = e.character;

            if (IsFirstInput)
            {
                FirstCharInputTime.Add(Time.realtimeSinceStartup);
                IsFirstInput = false;
            }
            if (inputChar != '\0')
            {
                
                TimeQueue.Enqueue(Time.realtimeSinceStartup);
                Judge(inputChar);
            }
        }
    }

    private void Judge(char input)
    {
        //正解のローマ字入力候補のいずれかが入力ローマ字と一致する
        if (AnswerList[KanaIndex].Count(IsMatchWithInputPeek) != 0)
        {

            CorrectNum++;
            ComboNum++;
            Combo.text = ComboNum + " Combo!";
            if (ComboNum > MaxCombo)
            {
                MaxCombo = ComboNum;
            }
            
            //入力に一致しない候補を削除
            AnswerList[KanaIndex].RemoveAll(IsNOTMatchWithInputPeek);

            //入力補助用アルファベット更新
            EraseCorrectSpellings();
       
            foreach (string charCandidate in AnswerList[KanaIndex])
            {
                SpellingIndex++;
                
                //かな一文字分チェックし終われば次の文字へ
                if (SpellingIndex == charCandidate.Length)
                {

                    //nでもよい「ん」の場合に入力がnで、まだnnと入力されてないとき
                    if (charCandidate == "n" && input == 'n' && !IsInput2ndN)
                    {
                        //nnのための二番目nを受け入れる準備
                        AcceptSingleN = true;
                        
                    }
                    //次の仮名部分へ
                    SpellingIndex = 0;
                    KanaIndex++;
                    if (IsInput2ndN)
                    {
                        IsInput2ndN = false;
                    }
                    //出題文の末尾まで解答を終えたら次の問題を出題
                    if (KanaIndex == AnswerList.Count)
                    {
                        KanaIndex = 0;
                        IsFirstInput = true;
                        OutputQ();
                    }
                }
                break;
            }
        }
        else
        {
            Debug.Log("ミスタイプ。正解は" + AnswerList[KanaIndex][0][SpellingIndex]);
            WeakKeys.Add(AnswerList[KanaIndex][0][SpellingIndex]);
            ComboNum = 0;
            MissNum++;
            StartCoroutine(FlashOnMistake());
            Combo.text = "";
            
        }

        //正解ローマ字入力候補が入力と一致するか
        bool IsMatchWithInputPeek(string s)
        {
            if (AcceptSingleN)
            {
                AcceptSingleN = false;

                //nでもよい「ん」の2回目のnが入力されたとき
                if (input == 'n')
                {
                    IsInput2ndN = true;
                    //次の仮名を見ているインデックスを前の「ん」のnに戻す
                    KanaIndex--;
                    SpellingIndex = 0;
                    return true;
                }
                return input == s[SpellingIndex];

            }
            return input == s[SpellingIndex];
        }
        //入力と不一致か
        bool IsNOTMatchWithInputPeek(string s)
        {
            return !IsMatchWithInputPeek(s);
        }
    }

    //入力補助用アルファベット表示：正解した部分を消していく
    private void EraseCorrectSpellings()
    {
        CorrectRomaji.text = "";
        for (int i = 0; i < AnswerList.Count; i++)
        {
            for (int j = 0; j < AnswerList[i][0].Length; j++)
            {
                if (i < KanaIndex || (i == KanaIndex && j <= SpellingIndex))
                {
                    continue;
                }
                CorrectRomaji.text += AnswerList[i][0][j];
                
            }
        }
    }

    //0.2秒画面を赤くする
    private IEnumerator FlashOnMistake()
    {
        MistakeEffect.enabled = true;
        yield return new WaitForSeconds(0.2f);
        MistakeEffect.enabled = false;
    }
}
