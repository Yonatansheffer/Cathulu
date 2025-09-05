using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using _WHY.Domains.Utilities.Sound.Scripts;
using TMPro;
using UnityEngine;

namespace _WHY.Domains.Utilities.UI.Scripts
{
    public class EndScreenUI : MonoBehaviour
    {
        [Header("Boards")]
        [SerializeField, Tooltip("Win board root GameObject")] private GameObject winBoard;
        [SerializeField, Tooltip("Lose board root GameObject")] private GameObject loseBoard;

        [Header("Texts")]
        [SerializeField, Tooltip("Shown when player is defeated by damage")] private TextMeshProUGUI defeatedText;
        [SerializeField, Tooltip("Shown when time runs out")] private TextMeshProUGUI timeOverText;
        [SerializeField, Tooltip("Blinking 'press again' text on win")] private TextMeshProUGUI winPressAgainText;
        [SerializeField, Tooltip("Blinking 'press again' text on lose")] private TextMeshProUGUI losePressAgainText;
        [SerializeField, Tooltip("Final score text")] private TextMeshProUGUI scoreText;

        [Header("Blink")]
        [SerializeField, Tooltip("Seconds between blink toggles")] private float blinkInterval = 0.5f;

        private Coroutine _winBlinkCo;
        private Coroutine _loseBlinkCo;

        private void OnEnable()
        {
            GameEvents.GameOverUI += HandleGameOverScreen;
        }

        private void OnDisable()
        {
            GameEvents.GameOverUI -= HandleGameOverScreen;
            StopAllBlinking();
        }

        private void HandleGameOverScreen(GameState gameState, int score)
        {
            ResetUI();

            switch (gameState)
            {
                case GameState.PlayerWon:
                {
                    if (winBoard) winBoard.SetActive(true);
                    if (scoreText) scoreText.text = $"SCORE: {score}";
                    if (winPressAgainText)
                    {
                        winPressAgainText.enabled = true;
                        _winBlinkCo = StartCoroutine(BlinkText(winPressAgainText, blinkInterval));
                    }
                    SoundManager.Instance.PlaySound("Win", transform);
                    break;
                }
                case GameState.TimeOver:
                {
                    if (loseBoard) loseBoard.SetActive(true);
                    if (timeOverText) timeOverText.gameObject.SetActive(true);
                    if (losePressAgainText)
                    {
                        losePressAgainText.enabled = true;
                        _loseBlinkCo = StartCoroutine(BlinkText(losePressAgainText, blinkInterval));
                    }
                    SoundManager.Instance.PlaySound("Lose", transform);
                    break;
                }
                case GameState.Defeated:
                {
                    if (loseBoard) loseBoard.SetActive(true);
                    if (defeatedText) defeatedText.gameObject.SetActive(true);
                    if (losePressAgainText)
                    {
                        losePressAgainText.enabled = true;
                        _loseBlinkCo = StartCoroutine(BlinkText(losePressAgainText, blinkInterval));
                    }
                    SoundManager.Instance.PlaySound("Lose", transform);
                    break;
                }
            }
        }

        private void ResetUI()
        {
            StopAllBlinking();

            if (winBoard) winBoard.SetActive(false);
            if (loseBoard) loseBoard.SetActive(false);

            if (defeatedText) defeatedText.gameObject.SetActive(false);
            if (timeOverText) timeOverText.gameObject.SetActive(false);

            if (winPressAgainText)
            {
                winPressAgainText.gameObject.SetActive(true);
                winPressAgainText.enabled = false;
            }
            if (losePressAgainText)
            {
                losePressAgainText.gameObject.SetActive(true);
                losePressAgainText.enabled = false;
            }
        }

        private void StopAllBlinking()
        {
            if (_winBlinkCo != null) { StopCoroutine(_winBlinkCo); _winBlinkCo = null; }
            if (_loseBlinkCo != null) { StopCoroutine(_loseBlinkCo); _loseBlinkCo = null; }
            if (winPressAgainText) winPressAgainText.enabled = false;
            if (losePressAgainText) losePressAgainText.enabled = false;
        }

        private IEnumerator BlinkText(TextMeshProUGUI text, float interval)
        {
            text.enabled = true;
            while (true)
            {
                yield return new WaitForSeconds(interval);
                text.enabled = !text.enabled;
            }
        }
    }
}
