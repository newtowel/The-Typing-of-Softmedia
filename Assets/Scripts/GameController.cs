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

    public int SpellIndex { get; private set; }
    public int KanaIndex { get; private set; }
    public static int MaxCombo { get; private set; }
    public static int CorrectNum { get; private set; }
    public static int MissNum { get; private set; }
    public static Queue<float> timeQueue { get; private set; }

    //入力を受け付けてよいか
    private bool IsInputValid { get; set; }
    //文字を入力し始めてからの経過時間
    private float FirstCharInputTime { get; set; }
    //その文字が最初の人文字目であるかのチェック
    private bool IsFirstInput { get; set; }
    //変換辞書をもとに生成された入力候補リスト
    private List<List<string>> AnswerList { get; set; }
    //上の文字が追加された時刻（平均秒速打数算出用？）
    private readonly string romajiKanaMapPath = Application.streamingAssetsPath + "/roman_map.json";
    //データベース名・テーブル名。問題取得時に用いる。暫定版
    private readonly string tableName = "another_list";
    private readonly string dbPath = Application.streamingAssetsPath + "/jp_sentence.db";
    private int ComboNum { get; set; }

    //制限時間カウントダウン用
    private float totalTime = 50;
    private int seconds;

    // Start is called before the first frame update
    void Start()
    {
        timeQueue = new Queue<float>();
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

    void OutputQ()
    {
        //問題のセット
        var ag = new AnswerGenerator(romajiKanaMapPath, dbPath, tableName);
        ProblemKana.text = ag.QuestionKanaSpelling;
        ProblemText.text = ag.QuestionText;
        AnswerList = ag.AnswerRomajiInputSpellingList;
        CorrectRomaji.text = "";
        IsInputValid = true;
        
    }

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
            //inputQueue.Enqueue(e.keyCode);
            Debug.Log(Time.realtimeSinceStartup);
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
        if (AnswerList[KanaIndex].Count(IsMatchWithInputPeek) != 0)
        {
            CorrectNum++;
            ComboNum++;
            Combo.text = ComboNum + " Combo!";
            if (ComboNum > MaxCombo)
            {
                MaxCombo = ComboNum;
            }
            //現在の入力に一致しない正解候補リストを排除
            AnswerList[KanaIndex].RemoveAll(IsNOTMatchWithInputPeek);

            foreach (string charCandidate in AnswerList[KanaIndex])
            {
                CorrectRomaji.text += input;
                SpellIndex++;

                //一文字分チェックし終われば次の文字へ
                if (SpellIndex == charCandidate.Length)
                {
                    SpellIndex = 0;
                    KanaIndex++;
                    //出題文の末尾まで回答を終えたら次の問題を出題
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
            Debug.Log("ミスタイプ : " + input);
            
        }
        
        //正解ローマ字入力候補が入力と一致するか
        bool IsMatchWithInputPeek(string s)
        {
            return input == s[SpellIndex];
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

        totalTime -= Time.deltaTime;
        seconds = (int)totalTime;
        Timer.text = seconds.ToString();
        if (seconds == 0)
        {
            SceneManager.LoadScene("Result");
        }
    }

    IEnumerator FlashOnMiss()
    {
        MissEffect.enabled = true;
        yield return new WaitForSeconds(0.2f);
        MissEffect.enabled = false;
    }
}
