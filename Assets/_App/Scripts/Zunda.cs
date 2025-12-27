using UnityEngine;
using UnityEngine.UI;

public class Zunda : Character
{
    public enum Facial
    {
        Normal,
        Miss,
        Happy
    }

    [SerializeField] private Image _image;
    [SerializeField] private Sprite[] facialSprites;

    public Facial facial = Facial.Normal;

    public void ChangeFacial(Facial facial)
    {
        _image.sprite = facialSprites[(int)facial];
    }
}
