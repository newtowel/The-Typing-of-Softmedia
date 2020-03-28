using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    [SerializeField]
    Text StartText;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > NextTime)
        {
            float alpha = StartText.GetComponent<CanvasRenderer>().GetAlpha();
            if (alpha == 1)
            {
                StartText.GetComponent<CanvasRenderer>().SetAlpha(0);
            }
            else
            {
                StartText.GetComponent<CanvasRenderer>().SetAlpha(1);
            }
            NextTime += Interval;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            IsSpacePressed = true;
        }
        if (IsSpacePressed)
        {
            StartText.GetComponent<CanvasRenderer>().SetAlpha(1);
            StartText.text = Seconds.ToString();
            TotalTime -= Time.deltaTime;
            Seconds = (int)TotalTime;
            if (Seconds == 0)
            {
                SceneManager.LoadScene("Solution");
            }
        }

        if (Input.GetKey(KeyCode.Escape)) Quit();

    }
    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      UnityEngine.Application.Quit();
#endif
    }

}
