using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PaletteView : MonoBehaviour
{
    public Palette palette = new Palette();
    public GameObject goImagePreviewPrefab;
    public Transform transGridParent;
    public GridLayoutGroup gridLayout;

    private List<ImagePreview> liImagePreviews = new List<ImagePreview>();


    public void Set(Palette _palette)
    {
        palette = _palette;
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        // remove all that aren't there anymore
        List<ImagePreview> liPreviewsRemoved = liImagePreviews.Where(imgPreview => !palette.liImages.Any(x => imgPreview.imgDisplayed.strGUID == x.strGUID)).ToList();
        liImagePreviews.RemoveAll(x => liPreviewsRemoved.Contains(x));
        liPreviewsRemoved.ForEach(x => Destroy(x.gameObject));
        liPreviewsRemoved.Clear();

        foreach (ImageInfo img in palette.liImages)
        {
            ImagePreview imagePreview = liImagePreviews.FirstOrDefault(x => x.imgDisplayed.strGUID == img.strGUID);
            if (imagePreview == default)
            {
                GameObject goNew = Instantiate(goImagePreviewPrefab, transGridParent);
                imagePreview = goNew.GetComponent<ImagePreview>();
                liImagePreviews.Add(imagePreview);
                imagePreview.eventDraggedOnEmpty.AddListener(Remove);
                imagePreview.DisplayImage(img);
            }
            imagePreview.transform.SetAsLastSibling();
        }
    }

    public void OnDrop(ImageInfo _img)
    {
        ImagePreview imagePreviewHovering = liImagePreviews.FirstOrDefault(x => x.bMouseCursorHovers);

        if (palette.liImages.Contains(_img))
            palette.liImages.Remove(_img);

        if (imagePreviewHovering == default)
            palette.liImages.Add(_img);
        else
            palette.liImages.Insert(palette.liImages.IndexOf(imagePreviewHovering.imgDisplayed), _img);

        UpdateDisplay();
    }

    public void Remove(ImagePreview _imagePreview)
    {
        palette.liImages.Remove(_imagePreview.imgDisplayed);
        UpdateDisplay();
    }
}
