using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawWindow : MonoBehaviour
{
    public RawImage rawimage;

    private Texture2D texDrawTexture;

    public void OpenImage(ImageInfo _img)
    {
        texDrawTexture = new Texture2D(_img.texGet().width, _img.texGet().height);
        texDrawTexture.SetPixels32(_img.texGet().GetPixels32());

        // apply picture to drawing space
        //rawimage.texture = texDrawTexture;
        rawimage.texture = _img.texGet();

        gameObject.SetActive(true);
    }

    public void CloseImage()
    {
        gameObject.SetActive(false);
    }
}
