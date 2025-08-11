using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;
using TMPro;
using UnityEngine;

namespace _WHY.Domains.Utilities.UI.Scripts
{
    public class EndScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject winBoard;
        [SerializeField] private GameObject loseBoard;
        [SerializeField] private TextMeshProUGUI defeatedText;
        [SerializeField] private TextMeshProUGUI timeOverText;
        [SerializeField] private TextMeshProUGUI winPressAgainText;
        [SerializeField] private TextMeshProUGUI losePressAgainText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [SerializeField] private float blinkInterval = 0.5f;

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
                    winBoard.SetActive(true);
                    scoreText.text = $"SCORE: {score}";
                    winPressAgainText.enabled = true;                 // known start state
                    _winBlinkCo = StartCoroutine(BlinkText(winPressAgainText, blinkInterval));
                    SoundManager.Instance.PlaySound("Win", transform);
                    break;

                case GameState.TimeOver:
                    loseBoard.SetActive(true);
                    timeOverText.gameObject.SetActive(true);
                    losePressAgainText.enabled = true;               // known start state
                    _loseBlinkCo = StartCoroutine(BlinkText(losePressAgainText, blinkInterval));
                    SoundManager.Instance.PlaySound("Lose", transform);
                    break;

                case GameState.Defeated:
                    loseBoard.SetActive(true);
                    defeatedText.gameObject.SetActive(true);
                    losePressAgainText.enabled = true;               // known start state
                    _loseBlinkCo = StartCoroutine(BlinkText(losePressAgainText, blinkInterval));
                    SoundManager.Instance.PlaySound("Lose", transform);
                    break;
            }
        }

        private void ResetUI()
        {
            // stop any previous blinking
            StopAllBlinking();

            // hide both boards and helper texts
            winBoard.SetActive(false);
            loseBoard.SetActive(false);

            defeatedText.gameObject.SetActive(false);
            timeOverText.gameObject.SetActive(false);

            // ensure press-again texts are active objects, but we control visibility via 'enabled'
            if (winPressAgainText != null)
            {
                winPressAgainText.gameObject.SetActive(true);
                winPressAgainText.enabled = false;
            }
            if (losePressAgainText != null)
            {
                losePressAgainText.gameObject.SetActive(true);
                losePressAgainText.enabled = false;
            }
        }

        private void StopAllBlinking()
        {
            if (_winBlinkCo != null) { StopCoroutine(_winBlinkCo); _winBlinkCo = null; }
            if (_loseBlinkCo != null) { StopCoroutine(_loseBlinkCo); _loseBlinkCo = null; }

            // Make sure the texts end in a predictable state
            if (winPressAgainText != null) winPressAgainText.enabled = false;
            if (losePressAgainText != null) losePressAgainText.enabled = false;
        }

        private IEnumerator BlinkText(TextMeshProUGUI text, float interval)
        {
            // safety: ensure it's visible to start
            text.enabled = true;

            while (true)
            {
                yield return new WaitForSeconds(interval);
                text.enabled = !text.enabled;
            }
        }
    }
}
