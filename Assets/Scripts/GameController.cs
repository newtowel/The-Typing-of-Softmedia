using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
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
    [SerializeField]
    Text Timer;

    public static int MaxCombo { get; private set; }
    public static int CorrectNum { get; private set; }
    public static int MissNum { get; private set; }
    public static Queue<float> TimeQueue { get; private set; }
    //入力を受け付けてよいか
    public static bool IsInputValid { get; private set; }

    //文字を入力し始めてからの経過時間
    private float FirstCharInputTime { get; set; }
    //その文字が最初の人文字目であるかのチェック
    private bool IsFirstInput { get; set; }
    //変換辞書をもとに生成された入力候補リスト
    private List<List<string>> AnswerList { get; set; }
    //半角スペルの位置
    private int SpellIndex { get; set; }
    //何文字目のかなについて打っているか
    private int KanaIndex { get; set; }
    //上の文字が追加された時刻（平均秒速打数算出用？）
    private readonly string romajiKanaMapPath = Application.streamingAssetsPath + "/roman_map.json";
    //データベース名・テーブル名。問題取得時に用いる。暫定版
    private readonly string tableName = "another_list";
    private readonly string dbPath = Application.streamingAssetsPath + "/jp_sentence.db";
    private int ComboNum { get; set; }

    //制限時間カウントダウン用
    private float TotalTime { get; set; }
    private int Seconds { get; set; }
    
    //「ん」の例外処理用
    private bool AcceptSingleN { get; set; }
    //nでもよい「ん」にて2回目のnを入力したか
    private bool IsInput2ndN { get; set; }
    
    // Start is called before the first frame update
    void Start()
    {
        AcceptSingleN = false;
        IsInput2ndN = false;
        IsInputValid = false;
        TotalTime = 60;
        TimeQueue = new Queue<float>();
        SpellIndex = 0;
        KanaIndex = 0;
        ComboNum = 0;
        MaxCombo = 0;
        CorrectNum = 0;
        MissNum = 0;
        ProblemText = transform.Find("ProblemText").GetComponent<Text>();
        ProblemKana = transform.Find("ProblemKana").GetComponent<Text>();
        CorrectRomaji = transform.Find("CorrectRomaji").GetComponent<Text>();
        Combo = transform.Find("Combo").GetComponent<Text>();
        MissEffect = transform.Find("MissEffect").GetComponent<Image>();
        Timer = transform.Find("Timer").GetComponent<Text>();
        OutputQ();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) Quit();

        TotalTime -= Time.deltaTime;
        Seconds = (int)TotalTime;
        Timer.text = Seconds.ToString();
        if (Seconds == 0)
        {
            SceneManager.LoadScene("Result");
        }

    }

    //入力キューに入れるのは随時入力があったとき、判定はフレームごと
    void OnGUI()
    {
        Event e = Event.current;
        //キー入力が有効な状態でキーが押下され（キーが上がった瞬間も除外）、何らかの（既知の）キーコードが認識
        if (IsInputValid && e.type == EventType.KeyDown && e.type != EventType.KeyUp && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            char inputChar = e.character;

            //新しい問題文が表示されてから1文字目を打つまでのレイテンシは計測時間に含めない
            if (IsFirstInput)
            {
                FirstCharInputTime = Time.realtimeSinceStartup;
                IsFirstInput = false;
            }
            if (inputChar != '\0')
            {
                
                TimeQueue.Enqueue(Time.realtimeSinceStartup);
                Judge(inputChar);
            }
        }
    }

    void Judge(char input)
    {
        //正解のローマ字入力候補のいずれかが入力ローマ字と一致する
        if (AnswerList[KanaIndex].Count(IsMatchWithInputPeek) != 0)
        {
            CorrectNum++;
            ComboNum++;
            Combo.text = ComboNum + " Combo!";
            if (ComboNum > MaxCombo)
            if (ComboNum > MaxCombo)
            {
                MaxCombo = ComboNum;
            }

            AnswerList[KanaIndex].RemoveAll(IsNOTMatchWithInputPeek);

            foreach (string charCandidate in AnswerList[KanaIndex])
            {
                CorrectRomaji.text += input;
                SpellIndex++;

                //一文字分チェックし終われば次の文字へ

                if (SpellIndex == charCandidate.Length)
                {

                    //nでもよい「ん」の場合に入力がnで、まだnnと入力されてないとき
                    if (charCandidate == "n" && input == 'n' && !IsInput2ndN)
                    {
                        //nnのための二番目nを受け入れる準備
                        AcceptSingleN = true;
                        
                    }
                    //次の仮名部分へ
                    SpellIndex = 0;
                    KanaIndex++;
                    if (IsInput2ndN)
                    {
                        IsInput2ndN = false;
                    }
                    //出題文の末尾まで解答を終えたら次の問題を出題
                    if (KanaIndex == AnswerList.Count)
                    {
                        KanaIndex = 0;
                        OutputQ();
                    }
                }
                break;
            }
        }
        else
        {
            ComboNum = 0;
            MissNum++;
            StartCoroutine(FlashOnMiss());
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
                    SpellIndex = 0;
                    return true;
                }
                return input == s[SpellIndex];

            }
            return input == s[SpellIndex];
        }
        //入力と不一致か
        bool IsNOTMatchWithInputPeek(string s)
        {
            return !IsMatchWithInputPeek(s);
        }
    }

    void OutputQ()
    {
        //問題のセット
        var ag = new AnswerGenerator(romajiKanaMapPath, dbPath, tableName);
        ProblemKana.text = ag.QuestionKanaSpelling;
        ProblemText.text = ag.QuestionText;
        AnswerList = ag.AnswerRomajiInputSpellingList;
        CorrectRomaji.text = "";
        IsInputValid = true;
        IsFirstInput = true;
    }


    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      UnityEngine.Application.Quit();
#endif
    }

    
    IEnumerator FlashOnMiss()
    {
        MissEffect.enabled = true;
        yield return new WaitForSeconds(0.2f);
        MissEffect.enabled = false;
    }
}
