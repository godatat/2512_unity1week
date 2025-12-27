using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ExplainDialog : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _callText;
    [SerializeField] private TextMeshProUGUI _explainText;
    [SerializeField] private Image _timerImage;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private float _timerDuration = 3f;

    private Action _onTimerComplete;

    public void Show(string callText, string explainText, float elapsedTime, int totalTypedChars, float accuracy, Action onTimerComplete = null)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 0;
        _callText.text = callText;
        _explainText.text = explainText;
        _statsText.text = $"時間: {elapsedTime:F2}秒  正確性: {accuracy:F2}%";
        _canvasGroup.DOFade(1, 0.5f);

        _onTimerComplete = onTimerComplete;

        _timerImage.fillAmount = 1;
        _timerText.text = "0";
        _timerImage.DOFillAmount(0, _timerDuration).OnUpdate(() => {
            float remaining = _timerImage.fillAmount * _timerDuration;
            _timerText.text = ((int)Mathf.Ceil(remaining)).ToString();
        }).OnComplete(() => {
            if (_onTimerComplete != null)
            {
                _onTimerComplete();
            }
            Hide();
        });
    }

    public void Hide()
    {
        _canvasGroup.DOFade(0, 0.5f).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
}
