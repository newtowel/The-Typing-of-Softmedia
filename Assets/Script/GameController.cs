using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Diagnostics.Eventing.Reader;

public class GameController : MonoBehaviour
{
    public Text ProblemKana;
    public InputField InputField;
    //入力した文字を表示するText
    public Text correctRomaji;
    //入力を受け付けてよいか
    private bool isInputValid;
    //文字を入力し始めてからの経過時間
    private float firstCharInputTime;
    //その文字が最初の人文字目であるかのチェック
    private bool isFirstInput;
    //変換辞書をもとに生成された入力候補リスト
    private List<List<string>> AnswerList;
    private string CorrectAnswerString;

    //入力されたローマ字を保持するキュー
    private Queue<KeyCode> inputQueue;
    //上の文字が追加された時刻（平均秒速打数算出用？）
    private Queue<float> timeQueue;
    private readonly string romajiKanaMapPath = Application.streamingAssetsPath + "/roman_map.json";
    //データベース名・テーブル名。問題取得時に用いる。暫定版
    private readonly string tableName = "trend_words";
    private readonly string dbPath = Application.streamingAssetsPath + "/jp_sentence.db";

	public Text ProblemText { get;private set; }


	// Start is called before the first frame update
	void Start()
    {
        ProblemText = transform.Find("Canvas/ProblemText").GetComponent<Text>();
        ProblemKana = transform.Find("Canvas/ProblemKana").GetComponent<Text>();
        InputField = GetComponent<InputField>();
        OutputQ();
    }

    
    //※Updateは、一定時刻ごとにイベントをチェックするため、その感覚よりタイピングが速いとタイプが検出されない→OnGUIはイベントドリブン
    void OnGUI()
    {
        Event e = Event.current;

        //キー入力が有効な状態でキーが押下され（キーが上がった瞬間も除外）、何らかの（既知の）キーコードが認識されており、マウスも押下されていない
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

    void OutputQ()
    {
        var ag = new AnswerGenerator(romajiKanaMapPath, dbPath, tableName);
        ProblemKana.text = ag.QuestionKanaSpelling;
        ProblemText.text = ag.QuestionText;
        AnswerList = ag.AnswerRomajiInputSpellingList;
        ProblemText.text = "";
        ProblemKana.text = "";
        InputField.text = "";
        //inputField.ActivateInputField();
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

    void Judge()
    {
        //正解入力候補リスト中の各仮名の入力候補について順次
        foreach (List<string> characterVariety in AnswerList)
        {
            int idx = 0;
            //入力したローマ字順列を正解にもつ入力方法のみ残す
            foreach (string character in characterVariety)
            {

                if (character[i] == ))
                {

                }
            }

        }

    }

}
