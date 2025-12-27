using System;
using System.Collections.Generic;
using UnityEngine;
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

    private List<string> japaneseWords = new List<string>();
    private List<string> romajiWords = new List<string>();
    private List<int> wordLineNumbers = new List<int>();
    private string currentWord = "";
    private string currentRomaji = "";
    private int currentIndex = 0;
    private int currentLineNumber = -1;
    private bool isPlaying = false;
    private string typedBuffer = "";
    private float originalTextX = 0f;

    void Start()
    {
        LoadWords();
        StartGame();
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
                var newBuffer = typedBuffer + c;
                if (NormalizeForCompare(currentRomaji).StartsWith(NormalizeForCompare(newBuffer), StringComparison.OrdinalIgnoreCase))
                {
                    typedBuffer = newBuffer;
                }
                else
                {
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

    void StartGame()
    {
        currentIndex = 0;
        PickNextWord();
        isPlaying = true;
        typedBuffer = "";
        UpdateTypedUI();
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
        
        if (currentIndex > 0 && currentIndex < wordLineNumbers.Count)
        {
            int currentWordLine = wordLineNumbers[currentIndex - 1];
            int nextWordLine = wordLineNumbers[currentIndex];
            
            if (currentWordLine != nextWordLine)
            {
                PlayClearSE();
                isLineComplete = true;
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
            explainDialog.Show("");
            Invoke("CloseExplainDialogAndContinue", 3f);
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
        isPlaying = true;
        PickNextWord();
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
