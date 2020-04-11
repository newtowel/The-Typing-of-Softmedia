using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class Title : MonoBehaviour
{
    [SerializeField]
    Text StartText;
    private float TotalTime = 4;
    private int Seconds;
    //カウントダウンしている間点滅しないように。GetKeyは押した瞬間しかtrueにならないので、それ以降は別途フラグを立てていなければならない
    private bool IsSpacePressed { get; set; } = false;

    // Update is called once per frame
    void Update()
    {
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
            TToSUtils.CountDownToSceneTransition(StartText, ref Seconds, ref TotalTime, "Solution");
        }
        TToSUtils.QuitOnEsc();
    }
}
