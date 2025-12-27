using UnityEngine;
using DG.Tweening;

public class Character : MonoBehaviour
{
    public float amplitude = 0.05f;
    public float speed = 1f;
    public float shakeAmount = 10f;
    public float shakeDuration = 0.3f;

    protected Vector2 baseAnchoredPos;
    protected RectTransform rectTransform;
    protected float timeOffset = 0f;

    protected virtual void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        baseAnchoredPos = rectTransform.anchoredPosition;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected virtual void Update()
    {
        float y = Mathf.Sin(Time.time * speed + timeOffset) * amplitude;
        rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(0f, y);
    }

    public void Shake()
    {
        rectTransform.DOKill();
        rectTransform.DOShakeAnchorPos(shakeDuration, shakeAmount, 10, 90f, false, true);
    }
}
