using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GridBoxDisplay : MonoBehaviour
{
    public GridLayoutGroup grid;
    public GridBoxData gridboxData;

    public List<ImagePreview> liImagePreviews = new List<ImagePreview>();

    public void SetData(GridBoxData _gridboxData)
    {
        gridboxData = _gridboxData;
        gridboxData.oliImgs.Updated += UpdateDisplay;
        UpdateCellSize();
    }

    public void UpdateCellSize()
    {
        grid.cellSize = gridboxData.oliImgs.Count > 0 ? gridboxData.v2iGetElementSize() : Vector2.zero;
        Vector2 v2Spacing = grid.spacing;
        v2Spacing.x = gridboxData.fGetFittingPadding();
        grid.spacing = v2Spacing;
    }

    public void UpdateDisplay()
    {
        for (int i = 0; i < gridboxData.oliImgs.Count; i++)
        {
            ImageInfo output = gridboxData.oliImgs[i]; // gridboxData.oliOutputs.Count - 1 - i

            if (!System.IO.File.Exists(output.strFilePathFull()))
                continue;

            // is there already a preview for that?
            ImagePreview imagePreview = liImagePreviews.FirstOrDefault(x => x.liOutputs.Any(y => y == output));
            if (imagePreview != default) 
            {
                imagePreview.transform.SetAsLastSibling();
                continue;
            }

            // is it just a step variation?
            imagePreview = liImagePreviews.FirstOrDefault(x => x.liOutputs.Any(y => y.prompt.bEqualExceptSteps(output.prompt)));
            if (imagePreview != default) 
            {
                imagePreview.liOutputs.Add(output);
                continue;
            }

            // neither of both? spawn (if it exists)

            GameObject goPreview = Instantiate(ToolManager.Instance.goImagePreviewPrefab, grid.transform);
            imagePreview = goPreview.GetComponent<ImagePreview>();
            imagePreview.DisplayImage(output);

            liImagePreviews.Add(imagePreview);
        }
    }

    private void OnDestroy()
    {
        gridboxData.oliImgs.Updated -= UpdateDisplay;
    }
}
