using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DrawWindow : MonoBehaviour
{
    public RawImage rawimage;
    public DrawViewController drawView;
    public AnimationCurve animBrushSize;
    public Slider sliderBrushSize;
    public Image imageBrushOutline;

    private Texture2D texDrawTexture;
    private ImageInfo img;
    private System.Action<string> actionFinished = null;

    public Vector2 v2MaxSize = Vector2.zero;

    private void Update()
    {
        imageBrushOutline.transform.position = Input.mousePosition;
    }

    public void OpenImage(ImageInfo _img, System.Action<string> _actionFinished)
    {
        if (v2MaxSize == Vector2.zero)
            v2MaxSize = rawimage.rectTransform.GetSize();

        img = _img;
        actionFinished = _actionFinished;
        gameObject.SetActive(true);
        texDrawTexture = texDuplicateTexture(_img.texGet());

        if (texDrawTexture != null)
            Utility.ScaleRectToImage(rawimage.rectTransform, v2MaxSize, new Vector2(texDrawTexture.width, texDrawTexture.height));

        // apply picture to drawing space
        rawimage.texture = texDrawTexture;

        SetBrushSize(sliderBrushSize.value);

        drawView.Initialize();
    }

    public void CloseImage(bool _bApply)
    {
        gameObject.SetActive(false);

        // save image, set as input
        if (_bApply)
        {
            string strGUID = System.Guid.NewGuid().ToString();
            string strPath = Path.Combine(ToolManager.s_settings.strInputDirectory, $"{Path.GetFileNameWithoutExtension(img.strFilePathRelative)}_{strGUID.Replace("-", "_")}_masked.png");

            File.WriteAllBytes(strPath, texDrawTexture.EncodeToPNG());

            if (actionFinished != null)
                actionFinished.Invoke(strPath);
        }
    }

    public void SetDrawingMode(bool _bFill)
    {
        drawView.drawSettings.SetAlpha(_bFill ? 1f : 0f);
        Color colorNew = drawView.drawSettings.drawColor;
        colorNew.a = _bFill ? 1f : 0f;
        drawView.drawSettings.SetDrawColour(colorNew);
    }

    public void SetBrushSize(float _fSize)
    {
        int iValue = (int)animBrushSize.Evaluate(_fSize);
        drawView.drawSettings.SetLineWidth(iValue);
        imageBrushOutline.transform.localScale = Vector3.one * iValue / imageBrushOutline.rectTransform.rect.width * 3f; // don't know why the 3 works
    }


    // from https://answers.unity.com/questions/988174/create-modify-texture2d-to-readwrite-enabled-at-ru.html
    Texture2D texDuplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
