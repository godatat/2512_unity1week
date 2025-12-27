using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Character : MonoBehaviour
{
    public float amplitude = 0.05f;
    public float speed = 1f;
    public float shakeAmount = 10f;
    public float shakeDuration = 0.3f;
    public GameManager gameManager;
    public float maxSpeedMultiplier = 3f;
    public float maxRotationSpeed = 360f; // 最大回転速度（度/秒）
    public GameObject[] cloneObjects; // 分身用のGameObject配列
    public float cloneSpawnInterval = 0.2f; // 分身を1体ずつ表示する間隔

    protected Vector2 baseAnchoredPos;
    protected RectTransform rectTransform;
    protected float timeOffset = 0f;
    private bool isLastTwoQuestions = false;

    protected virtual void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        baseAnchoredPos = rectTransform.anchoredPosition;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected virtual void Update()
    {
        // ラスト2問かどうかを判定
        isLastTwoQuestions = gameManager != null && gameManager.RemainingQuestions <= 2;

        float ageAgeDoRatio = gameManager != null ? gameManager.AgeAgeDo / 100f : 0f;
        float currentSpeed = speed * (1f + ageAgeDoRatio * (maxSpeedMultiplier - 1f));
        float y = Mathf.Sin(Time.time * currentSpeed + timeOffset) * amplitude;
        rectTransform.anchoredPosition = baseAnchoredPos + new Vector2(0f, y);

        // ラスト2問でない場合のみ回転
        if (!isLastTwoQuestions && gameManager != null && gameManager.AgeAgeDo > 50f)
        {
            float rotationRatio = (gameManager.AgeAgeDo - 50f) / 50f; // 50-100の範囲を0-1に正規化
            float rotationSpeed = rotationRatio * maxRotationSpeed;
            rectTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        else if (isLastTwoQuestions)
        {
            // ラスト2問の場合は回転を停止
            rectTransform.rotation = Quaternion.identity;
        }
    }

    public void ShowClones()
    {
        StartCoroutine(ShowClonesCoroutine());
    }

    IEnumerator ShowClonesCoroutine()
    {
        if (cloneObjects != null)
        {
            foreach (var clone in cloneObjects)
            {
                if (clone != null)
                {
                    clone.SetActive(true);
                    yield return new WaitForSeconds(cloneSpawnInterval);
                }
            }
        }
    }

    public void HideClones()
    {
        if (cloneObjects != null)
        {
            foreach (var clone in cloneObjects)
            {
                if (clone != null)
                {
                    clone.SetActive(false);
                }
            }
        }
    }

    public void Shake()
    {
        rectTransform.DOKill();
        rectTransform.DOShakeAnchorPos(shakeDuration, shakeAmount, 10, 90f, false, true);
    }
}
