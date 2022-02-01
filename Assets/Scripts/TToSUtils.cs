using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// TToS内全域にわたり使用する汎用機能。
/// </summary>
public static class TToSUtils
{
    /// <summary>
    /// 次に点滅する時刻
    /// </summary>
    private static float NextTime { get; set; } = Time.time;
    /// <summary>
    /// 点滅する周期
    /// </summary>
    private static float Interval { get; } = 0.5f;
    /// <summary>
    /// エスケープキーで終了。
    /// </summary>
    public static void QuitOnEsc()
    {
        if (Input.GetKey(KeyCode.Escape))
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      UnityEngine.Application.Quit();
#endif
        }
    }

    /// <summary>
    /// カウントダウン及びその表示、後にシーン遷移。
    /// </summary>
    /// <param name="secondsText">現在の秒数を表示するUI</param>
    /// <param name="seconds">現在の秒数</param>
    /// <param name="totalTime">残り秒数</param>
    /// <param name="nextScene">遷移する次のシーン</param>
    public static void CountDownToSceneTransition(Text secondsText,ref int seconds,ref float totalTime, string nextScene)
    {
        secondsText.text = seconds.ToString();
        totalTime -= Time.deltaTime;
        seconds = (int)totalTime;
        if (seconds == 0)
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    /// <summary>
    /// Text UIを点滅。
    /// </summary>
    /// <param name="blinkingText">点滅させるText</param>
    public static void BlinkText(Text blinkingText)
    {
        
        //現在時刻が点滅予定時刻を超えていれば、Textの透明度を確認、1なら0に、0なら1に、を繰り返す。
        if (Time.time > NextTime)
        {
            float alpha = blinkingText.GetComponent<CanvasRenderer>().GetAlpha();
            if (alpha == 1)
            {
                blinkingText.GetComponent<CanvasRenderer>().SetAlpha(0);
            }
            else
            {
                blinkingText.GetComponent<CanvasRenderer>().SetAlpha(1);
            }
            NextTime += Interval;
        }

    }
    /// <summary>
    /// Enterキー押下でシーン遷移。及びその旨を示すText表示。
    /// </summary>
    /// <param name="blinkingText">点滅させるText</param>
    /// <param name="nextScene">遷移先のシーン</param>
    public static void BlinkForSceneTransition(Text blinkingText, string nextScene)
    {
        if (Input.GetKey(KeyCode.Return))
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            BlinkText(blinkingText);
        }
    }
}
