using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager Instance;
    
    private bool playerWon;
    private bool hasWon = false;

    [SerializeField] private GameObject winConditionObject;
    [SerializeField] private string endSceneName = "EndScene";

    private void Awake()
    {
        // Singleton + persist
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.PlayerWon += OnPlayerWon;
        GameEvents.PlayerDefeated += OnPlayerDefeated;
    }

    private void OnDisable()
    {
        GameEvents.PlayerWon -= OnPlayerWon;
        GameEvents.PlayerDefeated -= OnPlayerDefeated;
    }
    
    public void ResetState(GameObject newWinCondition)
    {
        hasWon = false;
        playerWon = false;
        winConditionObject = newWinCondition;
    }
    
    void Update()
    {
        if (hasWon) return;

        // Unity sets destroyed objects to null
        if (winConditionObject == null)
        {
            hasWon = true;
            GameEvents.PlayerWon?.Invoke();
        }
    }


    void OnPlayerWon()
    {
        playerWon = true;
        SceneManager.LoadScene(endSceneName);
    }

    void OnPlayerDefeated()
    {
        playerWon = false;
        SceneManager.LoadScene(endSceneName);
    }

    public bool DidPlayerWin()
    {
        return playerWon;
    }
}