using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Title : MonoBehaviour
{
    [SerializeField]
    Text StartText;
    private float _TotalTime = 4;
    private int _CurrentSeconds;
    //カウントダウンしている間点滅しないように。GetKeyは押した瞬間しかtrueにならないので、それ以降は別途フラグを立てていなければならない
    private bool IsSpacePressed { get; set; } = false;

    // Update is called once per frame
    void Update()
    {
        //S, Yの同時押しでランキングを初期化
        if (Input.GetKeyDown(KeyCode.S) && Input.GetKeyDown(KeyCode.Y))
        {
            StartCoroutine(DisplayResetGuidance());
        }

        if (Input.GetKey(KeyCode.Space))
        {
            IsSpacePressed = true;
        }
        //スペースキーが押されるまでは案内文字の点滅・押されたら遷移へのカウントダウン
        if (!IsSpacePressed)
        {
            TToSUtils.BlinkText(StartText);
        }
        else
        {
            //点滅をやめたタイミングによっては字が薄くなっている恐れがあるので調整。
            StartText.GetComponent<CanvasRenderer>().SetAlpha(1);
            TToSUtils.CountDownToSceneTransition(StartText, ref _CurrentSeconds, ref _TotalTime, "Solution");
        }
        TToSUtils.QuitOnEsc();
    }
    private IEnumerator DisplayResetGuidance()
    {
        Ranking.ResetRanking();
        Debug.Log("ランキングを初期化");
        StartText.text = "ランキングを初期化しました";
        yield return new WaitForSeconds(2);
        StartText.text = "スペースキーを押してスタート";
    }
}
