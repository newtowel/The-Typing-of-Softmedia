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
    Text MIPS;
    [SerializeField]
    Text Combo;
    //タイトルに戻す案内
    [SerializeField]
    Text ReturnGuide;

    private float TotalTime = 4;
    private int Seconds { get; set; }
    private bool IsSpacePressed = false;
    //文字を点滅させる周期
    private readonly float Interval = 0.5f;
    private float NextTime { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        NextTime = Time.time;
        int correct = GameController.CorrectNum;
        int miss = GameController.MissNum;
        Correct.text += correct + "回";
        Wrong.text += miss + "回";
        Combo.text += GameController.MaxCombo + "回";
        
        float acc = (float)correct / (correct + miss);
        Accuracy.text += Math.Round(acc, 2, MidpointRounding.AwayFromZero);

        //キー入力時刻キューを読み込み
        List<float> inputTimes = GameController.TimeQueue.ToList<float>();
        //隣り合った入力時刻の差分をとることで得られる、1文字当たりの入力時間リスト
        List<float> deltas = new List<float>();
        for (int i = 0; i < inputTimes.Count - 1; i++)
        {
            deltas.Add(inputTimes[i + 1] - inputTimes[i]);
        }
        float mips = 1f / deltas.Average();
        MIPS.text += Math.Round(mips, 2, MidpointRounding.AwayFromZero) + "回/秒";
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
        if (Time.time > NextTime)
        {
            float alpha = ReturnGuide.GetComponent<CanvasRenderer>().GetAlpha();
            if (alpha == 1)
            {
                ReturnGuide.GetComponent<CanvasRenderer>().SetAlpha(0);
            }
            else
            {
                ReturnGuide.GetComponent<CanvasRenderer>().SetAlpha(1);
            }
            NextTime += Interval;
        }
        if (Input.GetKey(KeyCode.Return))
        {
            ReturnGuide.GetComponent<CanvasRenderer>().SetAlpha(1);
            SceneManager.LoadScene("Title");
        }

        if (Input.GetKey(KeyCode.Escape)) Quit();
    }


}
