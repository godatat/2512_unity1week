using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ExplainDialog : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _callText;

    public void Show(string text)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 0;
        _callText.text = text;
        _canvasGroup.DOFade(1, 0.5f);
    }

    public void Hide()
    {
        _canvasGroup.DOFade(0, 0.5f).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
}
