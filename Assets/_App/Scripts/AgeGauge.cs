using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AgeGauge : MonoBehaviour
{
    [SerializeField] private RectTransform _gaugeRectTransform;
    [SerializeField] private float _maxWidth = 100f;
    [SerializeField] private float _animationDuration = 0.3f;

    private float _baseWidth = 0f;

    void Start()
    {
        _baseWidth = GetComponent<RectTransform>().sizeDelta.x;
    }

    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        Debug.Log("value: " + value);
        float targetWidth = _baseWidth * value;
        _gaugeRectTransform.DOKill();
        _gaugeRectTransform.DOSizeDelta(new Vector2(targetWidth, _gaugeRectTransform.sizeDelta.y), _animationDuration).SetEase(Ease.OutQuad);
    }
}
