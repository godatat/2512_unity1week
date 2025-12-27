using UnityEngine;
using TMPro;

public class ResultScene : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultText;

    void Start()
    {
        if (_resultText != null)
        {
            _resultText.text = $"入力時間: {GameResultData.TotalTime:F2}秒\n\n\n正確性: {GameResultData.Accuracy:F2}%";
        }
    }

    public void OnClickReturnTitle()
    {
        SceneTransition.LoadScene("TitleScene");
    }
}

