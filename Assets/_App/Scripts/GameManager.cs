using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI targetWordText;
    public TextMeshProUGUI typedText;
    public bool autoStart = true;
    public AudioSource sePlayer;
    public AudioClip correctSE;
    public AudioClip incorrectSE;
    public AudioClip clearSE;
    public float slideDuration = 0.3f;
    public Zunda zunda;
    public Metan metan;
    public ExplainDialog explainDialog;
    public AgeGauge ageGauge;
    public TextMeshProUGUI countdownText;
    public CanvasGroup _coreCanvasGroup;
    public CanvasGroup _countdownCanvasGroup;

    private List<string> japaneseWords = new List<string>();
    private List<string> romajiWords = new List<string>();
    private List<int> wordLineNumbers = new List<int>();
    private ExplainData[] explainDataList;
    private string currentWord = "";
    private string currentRomaji = "";
    private int currentIndex = 0;
    private int currentLineNumber = -1;
    private bool isPlaying = false;
    private string typedBuffer = "";
    private float originalTextX = 0f;
    private float questionStartTime = 0f;
    private int totalTypedChars = 0;
    private int missCount = 0;
    private List<float> accuracyHistory = new List<float>();
    private int lastCompletedLineNumber = -1;
    private float excitementLevel = 0f;
    public float missPenalty = 2f;
    private float totalInputTime = 0f;

    // アゲアゲ度（0-100の値）
    public float AgeAgeDo
    {
        get { return excitementLevel; }
        private set { excitementLevel = Mathf.Clamp(value, 0f, 100f); }
    }

    // 残りの問題数を取得
    public int RemainingQuestions
    {
        get
        {
            if (explainDataList == null || lastCompletedLineNumber < 0)
            {
                return explainDataList != null ? explainDataList.Length : 0;
            }
            int totalQuestions = explainDataList.Length;
            int currentQuestion = lastCompletedLineNumber + 1;
            return Mathf.Max(0, totalQuestions - currentQuestion);
        }
    }

    [Serializable]
    private class ExplainData
    {
        public string call;
        public string explain;
    }

    void Start()
    {
        LoadWords();
        LoadExplains();
        
        // 初期状態: カウントダウンを非表示、ゲームUIを非表示
        if (_countdownCanvasGroup != null)
        {
            _countdownCanvasGroup.alpha = 0f;
            _countdownCanvasGroup.gameObject.SetActive(true);
        }
        if (_coreCanvasGroup != null)
        {
            _coreCanvasGroup.alpha = 0f;
            _coreCanvasGroup.gameObject.SetActive(true);
        }
        
        if (autoStart)
        {
            StartCountdown();
        }
    }

    private void LoadExplains()
    {
        var ta = Resources.Load<TextAsset>("explains");
        if (ta != null)
        {
            string json = "{\"items\":" + ta.text + "}";
            explainDataList = JsonUtility.FromJson<ExplainDataArray>(json).items;
        }
    }

    [Serializable]
    private class ExplainDataArray
    {
        public ExplainData[] items;
    }

    private void LoadWords()
    {
        var ta = Resources.Load<TextAsset>("words");
        var text = ta.text;
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int lineNumber = 0;
        foreach (var l in lines)
        {
            var line = l.Trim();
            var two = line.Split(new[] { ',' }, 2);
            var japSegment = two[0].Trim();
            var romSegment = two[1].Trim();
            var jItems = japSegment.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var rItems = romSegment.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var max = System.Math.Max(jItems.Length, rItems.Length);
            for (int i = 0; i < max; i++)
            {
                var j = jItems[i].Trim();
                var r = i < rItems.Length ? rItems[i].Trim() : "";
                japaneseWords.Add(j);
                romajiWords.Add(r);
                wordLineNumbers.Add(lineNumber);
            }
            lineNumber++;
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        var input = Input.inputString;
        foreach (var c in input)
        {
            if (c == '\b')
            {
                typedBuffer = typedBuffer.Substring(0, Math.Max(0, typedBuffer.Length - 1));
            }
            else if (c == '\n' || c == '\r')
            {
                if (string.Equals(NormalizeForCompare(typedBuffer), NormalizeForCompare(currentRomaji), StringComparison.OrdinalIgnoreCase)) OnCorrectWord();
            }
            else
            {
                totalTypedChars++;
                var newBuffer = typedBuffer + c;
                if (NormalizeForCompare(currentRomaji).StartsWith(NormalizeForCompare(newBuffer), StringComparison.OrdinalIgnoreCase))
                {
                    typedBuffer = newBuffer;
                }
                else
                {
                    missCount++;
                    Debug.Log($"missCount: {missCount}, excitementLevel: {excitementLevel}");
                    /*
                    excitementLevel = Mathf.Max(0f, excitementLevel - missPenalty);
                    if (ageGauge != null)
                    {
                        ageGauge.SetValue(excitementLevel / 100f);
                    }
                    */
                    PlayIncorrectSE();
                    if (zunda != null)
                    {
                        zunda.ChangeFacial(Zunda.Facial.Miss);
                        //zunda.Shake();
                    }
                    if (metan != null)
                    {
                        metan.ChangeFacial(Metan.Facial.Miss);
                    }
                    Invoke("ResetCharacters", 0.5f);
                }
            }
        }

        UpdateTypedUI();

        if (string.Equals(NormalizeForCompare(typedBuffer), NormalizeForCompare(currentRomaji), StringComparison.OrdinalIgnoreCase)) OnCorrectWord();
    }

    void StartCountdown()
    {
        // カウントダウンUIをフェードイン
        if (_countdownCanvasGroup != null)
        {
            _countdownCanvasGroup.alpha = 0f;
            _countdownCanvasGroup.DOFade(1f, 0.3f);
        }
        
        // ゲームUIを非表示
        if (_coreCanvasGroup != null)
        {
            _coreCanvasGroup.alpha = 0f;
        }
        
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }
            yield return new WaitForSeconds(1f);
        }
        
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
        }
        
        // カウントダウンUIをフェードアウト
        if (_countdownCanvasGroup != null)
        {
            _countdownCanvasGroup.DOFade(0f, 0.3f);
        }
        
        yield return new WaitForSeconds(0.3f);
        
        StartGame();
    }

    void StartGame()
    {
        // ゲームUIをフェードイン
        if (_coreCanvasGroup != null)
        {
            _coreCanvasGroup.alpha = 0f;
            _coreCanvasGroup.DOFade(1f, 0.3f);
        }
        
        currentIndex = 0;
        accuracyHistory.Clear();
        excitementLevel = 0f;
        totalInputTime = 0f;
        GameResultData.Reset();
        PickNextWord();
        isPlaying = true;
        typedBuffer = "";
        UpdateTypedUI();
    }

    void UpdateExcitementLevel()
    {
        if (accuracyHistory.Count == 0)
        {
            excitementLevel = 0f;
            if (ageGauge != null)
            {
                ageGauge.SetValue(0f);
            }
            return;
        }

        float sum = 0f;
        foreach (float acc in accuracyHistory)
        {
            sum += acc;
        }
        float averageAccuracy = sum / accuracyHistory.Count;
        float accuracyRate = averageAccuracy / 100f;

        int totalQuestions = explainDataList != null ? explainDataList.Length : 0;
        if (totalQuestions == 0)
        {
            totalQuestions = 1;
        }

        float maxValue = totalQuestions * accuracyRate;
        float currentValue = accuracyHistory.Count * accuracyRate;
        
        excitementLevel = maxValue > 0 ? (currentValue / maxValue * 100f) : 0f;
        excitementLevel = Mathf.Clamp(excitementLevel, 0f, 100f);
    }

    void EndGame()
    {
        isPlaying = false;
        targetWordText.text = "Game Over";
        typedBuffer = "";
        UpdateTypedUI();
    }

    void PickNextWord()
    {
        if (currentIndex >= japaneseWords.Count) currentIndex = 0;
        string newWord = japaneseWords[currentIndex];
        string newRomaji = romajiWords[currentIndex];
        currentIndex++;

        bool hasCurrentWord = !string.IsNullOrEmpty(currentWord);
        
        if (hasCurrentWord)
        {
            // 既存の文字をスライドアウト
            RectTransform rectTransform = targetWordText.rectTransform;
            Vector2 currentPos = rectTransform.anchoredPosition;
            rectTransform.DOAnchorPosX(1000f, slideDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => { ShowNewWord(newWord, newRomaji); });
        }
        else
        {
            // 最初の文字はスライドインのみ
            ShowNewWord(newWord, newRomaji);
        }
    }

    void ShowNewWord(string word, string romaji)
    {
        currentWord = word;
        currentRomaji = romaji;

        RectTransform rectTransform = targetWordText.rectTransform;
        Vector2 currentPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector2(-1000f, currentPos.y);
        
        targetWordText.text = word;
        typedText.text = EscapeRichText(romaji);
        typedBuffer = "";
        UpdateTypedUI();

        // スライドイン
        rectTransform.DOAnchorPosX(0, slideDuration)
            .SetEase(Ease.OutQuad);
    }

    void OnCorrectWord()
    {
        PlayCorrectSE();
        
        bool isLineComplete = false;
        int completedLineNumber = -1;
        
        if (currentIndex > 0)
        {
            int currentWordLine = wordLineNumbers[currentIndex - 1];
            
            // 次の単語がある場合
            if (currentIndex < wordLineNumbers.Count)
            {
                int nextWordLine = wordLineNumbers[currentIndex];
                
                if (currentWordLine != nextWordLine)
                {
                    PlayClearSE();
                    isLineComplete = true;
                    completedLineNumber = currentWordLine;
                }
            }
            // 最後の単語を正解した場合（次の単語がない）
            else if (currentIndex == wordLineNumbers.Count)
            {
                PlayClearSE();
                isLineComplete = true;
                completedLineNumber = currentWordLine;
            }
        }
        
        if (zunda != null)
        {
            zunda.ChangeFacial(Zunda.Facial.Happy);
        }
        if (metan != null)
        {
            metan.ChangeFacial(Metan.Facial.Happy);
        }
        Invoke("ResetCharacters", 0.5f);
        typedBuffer = "";
        UpdateTypedUI();
        
        if (isLineComplete && explainDialog != null)
        {
            isPlaying = false;
            string callText = "";
            string explainText = "";
            
            if (explainDataList != null && completedLineNumber >= 0 && completedLineNumber < explainDataList.Length)
            {
                callText = explainDataList[completedLineNumber].call;
                explainText = explainDataList[completedLineNumber].explain;
            }
            
            // 完了した行番号を保存
            lastCompletedLineNumber = completedLineNumber;
            
            float elapsedTime = Time.time - questionStartTime;
            totalInputTime += elapsedTime; // 各問題の入力時間を累積
            float accuracy = totalTypedChars > 0 ? ((float)(totalTypedChars - missCount) / totalTypedChars * 100f) : 100f;

            Debug.Log($"totalTypedChars: {totalTypedChars}, missCount: {missCount}, accuracy: {accuracy}");

            excitementLevel = Mathf.Max(0f, excitementLevel + accuracy);
            accuracyHistory.Add(accuracy);
            UpdateExcitementLevel();
            explainDialog.Show(callText, explainText, elapsedTime, totalTypedChars, accuracy, CloseExplainDialogAndContinue);
        }
        else
        {
            PickNextWord();
        }
    }

    void ResetCharacters()
    {
        if (zunda != null)
        {
            zunda.ChangeFacial(Zunda.Facial.Normal);
        }
        if (metan != null)
        {
            metan.ChangeFacial(Metan.Facial.Normal);
        }
    }

    void CloseExplainDialogAndContinue()
    {
        if (explainDialog != null)
        {
            explainDialog.Hide();
        }
        
        // 最後の問題だったらタイトルに戻る
        if (explainDataList != null && lastCompletedLineNumber >= 0)
        {
            bool isLastLine = (lastCompletedLineNumber == explainDataList.Length - 1);
            if (isLastLine)
            {
                ReturnToTitle();
                return;
            }
        }
        
        // 設問の間に「one more call！」を表示
        ShowQuestionInterval();
    }

    void ShowQuestionInterval()
    {
        // カウントダウンUIをフェードイン
        if (_countdownCanvasGroup != null)
        {
            _countdownCanvasGroup.alpha = 0f;
            _countdownCanvasGroup.DOFade(1f, 0.3f);
        }
        
        // ゲームUIを非表示
        if (_coreCanvasGroup != null)
        {
            _coreCanvasGroup.alpha = 0f;
        }
        
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            
            // 残りの問題数を計算
            string questionCountText = "";
            if (explainDataList != null && lastCompletedLineNumber >= 0)
            {
                int currentQuestion = lastCompletedLineNumber + 1; // 現在の問題番号（1ベース）
                int totalQuestions = explainDataList.Length;
                int remainingQuestions = currentQuestion + 1;
                questionCountText = $" {remainingQuestions}/{totalQuestions}";
            }
            
            countdownText.text = $"one more call！ ({questionCountText})";
        }
        
        StartCoroutine(QuestionIntervalCoroutine());
    }

    IEnumerator QuestionIntervalCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        
        // カウントダウンUIをフェードアウト
        if (_countdownCanvasGroup != null)
        {
            _countdownCanvasGroup.DOFade(0f, 0.3f);
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // 7問目以降の場合、キャラクターの分身を表示
        if (RemainingQuestions <= 7)
        {
            if (zunda != null)
            {
                zunda.ShowClones();
            }
            if (metan != null)
            {
                metan.ShowClones();
            }
        }
        
        missCount = 0;
        totalTypedChars = 0;
        questionStartTime = Time.time;
        
        if (ageGauge != null)
        {
            ageGauge.SetValue(excitementLevel / 100f);
        }
        
        // ゲームUIをフェードイン
        if (_coreCanvasGroup != null)
        {
            _coreCanvasGroup.alpha = 0f;
            _coreCanvasGroup.DOFade(1f, 0.3f);
        }
        
        isPlaying = true;
        PickNextWord();
    }

    void ReturnToTitle()
    {
        // 正確性の平均を計算
        float averageAccuracy = 0f;
        if (accuracyHistory.Count > 0)
        {
            float sum = 0f;
            foreach (float acc in accuracyHistory)
            {
                sum += acc;
            }
            averageAccuracy = sum / accuracyHistory.Count;
        }
        
        // 総時間（各問題の入力時間の合計）と総スコア、正確性を記録
        float totalScore = excitementLevel; // アゲアゲ度をスコアとして使用
        GameResultData.SetResult(totalInputTime, totalScore, averageAccuracy);
        
        // 最後の問題の解説が終わったらリザルトシーンに遷移
        SceneTransition.LoadScene("ResultScene");
    }

    void PlayCorrectSE()
    {
        if (sePlayer != null && correctSE != null)
        {
            sePlayer.PlayOneShot(correctSE);
        }
    }

    void PlayIncorrectSE()
    {
        if (sePlayer != null && incorrectSE != null)
        {
            sePlayer.PlayOneShot(incorrectSE);
        }
    }

    void PlayClearSE()
    {
        if (sePlayer != null && clearSE != null)
        {
            sePlayer.PlayOneShot(clearSE);
        }
    }

    void UpdateTypedUI()
    {
        var expected = currentRomaji;
        var expectedNorm = NormalizeForCompare(expected);
        var typedNorm = NormalizeForCompare(typedBuffer);

        int correctCount = 0;
        for (int i = 0; i < typedNorm.Length && i < expectedNorm.Length; i++)
        {
            if (typedNorm[i] == expectedNorm[i]) correctCount++;
            else break;
        }

        // Map correctCount (count of matching non-space chars) back to the original expected string
        int charsToInclude = 0;
        int seen = 0;
        while (charsToInclude < expected.Length && seen < correctCount)
        {
            if (expected[charsToInclude] != ' ') seen++;
            charsToInclude++;
        }

        var correctPart = EscapeRichText(expected.Substring(0, charsToInclude));
        var restPart = EscapeRichText(expected.Substring(charsToInclude));

        if (correctCount > 0)
            typedText.text = $"<color=#ff4444>{correctPart}</color>{restPart}";
        else
            typedText.text = restPart;
    }

    private string EscapeRichText(string s)
    {
        return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }

    private string NormalizeForCompare(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace(" ", "").ToLowerInvariant();
    }

    public void Restart()
    {
        StartGame();
    }
}
