using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DrawWindow : MonoBehaviour
{
    public RawImage rawimage;
    public DrawViewController drawView;

    private Texture2D texDrawTexture;
    private ImageInfo img;
    private System.Action<string> actionFinished = null;

    public void OpenImage(ImageInfo _img, System.Action<string> _actionFinished)
    {
        img = _img;
        actionFinished = _actionFinished;
        gameObject.SetActive(true);
        texDrawTexture = texDuplicateTexture(_img.texGet());

        // apply picture to drawing space
        rawimage.texture = texDrawTexture;

        drawView.Initialize();
    }

    public void CloseImage()
    {
        gameObject.SetActive(false);

        // save image, set as input
        string strGUID = System.Guid.NewGuid().ToString();
        string strPath = Path.Combine(ToolManager.s_settings.strInputDirectory, $"{Path.GetFileNameWithoutExtension(img.strFilePathRelative)}_{strGUID.Replace("-", "_")}.png");

        File.WriteAllBytes(strPath, texDrawTexture.EncodeToPNG());

        if (actionFinished != null)
            actionFinished.Invoke(strPath);
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
