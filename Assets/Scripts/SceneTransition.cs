using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField]
    Text timerText;
    private float totalTime = 4;
    private int seconds;
    private bool isSpacePressed = false;
    // Start is called before the first frame update
    void Start()
    {
        timerText = transform.Find("StartGuidance").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            isSpacePressed = true;
        }
        if (isSpacePressed)
        {
            timerText.text = seconds.ToString();
            totalTime -= Time.deltaTime;
            seconds = (int)totalTime;
            if (seconds == 0)
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
