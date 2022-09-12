using UnityEngine;
using UnityEngine.UI;

public class DrawSettingController : MonoBehaviour {
    public Slider lineWidthSlider;
    public Text lineWidthText;
    public Slider transparencySlider;
    public Text transparencyText;
    public InputField drawColorInputField;

    public DrawViewController drawViewController;

    void Start() {
        lineWidthSlider.value = drawViewController.GetDrawLineWidth();
        lineWidthSlider.onValueChanged.AddListener(delegate { LineWidthChanged(); });
        transparencySlider.value = drawViewController.GetDrawTransparency();
        transparencySlider.onValueChanged.AddListener(delegate { TransparencyChanged(); });
        drawColorInputField.text = "#" + ColorUtility.ToHtmlStringRGB(drawViewController.GetDrawColor());
        drawColorInputField.onEndEdit.AddListener(delegate { DrawColorChanged(); });
    }

    void LineWidthChanged() {
        lineWidthText.text = lineWidthSlider.value.ToString();
        drawViewController.SetDrawLineWidth((int)lineWidthSlider.value);
    }

    void TransparencyChanged() {
        transparencyText.text = transparencySlider.value.ToString();
        drawViewController.SetDrawTransparency(transparencySlider.value);
    }

    void DrawColorChanged() {
        Color colorToSet;
        if (ColorUtility.TryParseHtmlString(drawColorInputField.text, out colorToSet)) {
            drawViewController.SetDrawColor(colorToSet);
        } else {
            //Do some error handling
            Debug.Log("invalid color input");
        }
    }
}
