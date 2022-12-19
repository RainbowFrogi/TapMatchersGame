using UnityEngine;
using UnityEngine.SceneManagement;

public class ESCMenu : MonoBehaviour
{
    [SerializeField] GameObject ESCPanel;

    public static ESCMenu Instance;

    [HideInInspector] public bool inMenu = false;

    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Check if user pressed ESC and ESCPanel was not active
        if (Input.GetKeyDown(KeyCode.Escape) && !ESCPanel.activeSelf)
        {
            GameManager.Instance.canClick = false;
            ESCPanel.SetActive(true);
            inMenu = true;
        }

        //Check if user pressed ESC and ESCPanel was active
        else if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.Instance.canClick && ESCPanel.activeSelf)
        {
            GameManager.Instance.canClick = true;
            ESCPanel.SetActive(false);
            inMenu = false;
        }
    }

    //Load scene from previous buildIndex
    public void BackToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void CloseEscMenu()
    {
        GameManager.Instance.canClick = true;
        ESCPanel.SetActive(false);
        inMenu = false;
    }
}
