using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        if (WinLoseManager.Instance.DidPlayerWin())
            winScreen.SetActive(true);
        else
            loseScreen.SetActive(true);
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}