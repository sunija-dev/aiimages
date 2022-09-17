using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawViewController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    //This is the data model used to store our draw settings
    public DrawSettings drawSettings;

    // The image we are going to edit at runtime
    RawImage drawImage;

    // The texture of the drawSprite, the actual .png you are editing
    Texture2D drawTexture;

    //The position of the mouse in the last frame
    Vector2 previousDragPosition;

    //The array used to reset the image to be empty
    Color[] resetColorsArray;
    //The color filled into the "resetColorsArray"
    Color resetColor;

    //The color array of the changes to be applied
    Color32[] currentColors;

    //The rectTransform of the GameObject this script is attached to
    RectTransform rectTransform;

    void Awake() 
    {
        rectTransform = GetComponent<RectTransform>();
        drawImage = GetComponent<RawImage>();

        resetColor = new Color(0, 0, 0, 0);

        Initialize();

        //Toggle this off to prevent the texture from getting cleared
        ResetTexture();
    }

    //Call this whenever the image this script is attached has changed
    //most uses can probably simply call initalize at the beginning
    public void Initialize() {
        drawTexture = (Texture2D)drawImage.texture;

        // fill the array with our reset color so it can be easily reset later on
        resetColorsArray = new Color[(int)drawImage.texture.width * (int)drawImage.texture.height];
        for (int x = 0; x < resetColorsArray.Length; x++)
            resetColorsArray[x] = resetColor;
    }

    void Update() {
        KeyboardInput();
    }

    void KeyboardInput() {
        // We have different undo/redo controls in the editor,
        // so that way you don't accidentally undo something in the scene
#if UNITY_EDITOR
        bool isShiftHeldDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isZHeldDown = Input.GetKeyDown(KeyCode.Z);
        bool isYHeldDown = Input.GetKeyDown(KeyCode.Y);

        if (isShiftHeldDown &&
            isZHeldDown &&
            drawSettings.CanUndo()) {
            // if there's something to undo, pull the last state off of the stack, and apply those changes
            currentColors = drawSettings.Undo(drawTexture.GetPixels32());
            ApplyCurrentColors();
        }

        if (isShiftHeldDown &&
            isYHeldDown &&
            drawSettings.CanRedo()) {
            currentColors = drawSettings.Redo(drawTexture.GetPixels32());
            ApplyCurrentColors();
        }
        //These controls only take effect if we build the game! See: Platform dependent compilation
#else
        bool isControlHeldDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool isZHeldDown = Input.GetKeyDown(KeyCode.Z);
        if (isControlHeldDown && isZHeldDown) {
            if (Input.GetKey(KeyCode.LeftShift) && 
                drawSettings.CanRedo()) {
                currentColors = drawSettings.Redo(drawTexture.GetPixels32());
                ApplyCurrentColors();
            } else if (drawSettings.CanUndo()) {
                currentColors = drawSettings.Undo(drawTexture.GetPixels32());
                ApplyCurrentColors();
            }
        }
#endif
    }


    // Pass in a point in PIXEL coordinates
    // Changes the surrounding pixels of the pixelPosition to the drawSetting.drawColor
    public void Paint(Vector2 pixelPosition) {
        //grab the current image state
        currentColors = drawTexture.GetPixels32();

        if (previousDragPosition == Vector2.zero) {
            // If this is the first frame in a drag, color the pixels around the mouse
            MarkPixelsToColour(pixelPosition);
        } else {
            // Color between where we are this frame, and where our mouse was last frame
            ColorBetween(previousDragPosition, pixelPosition);
        }
        ApplyCurrentColors();

        previousDragPosition = pixelPosition;
    }

    //Color the pixels around the centerPoint
    public void MarkPixelsToColour(Vector2 centerPixel) {
        int centerX = (int)centerPixel.x;
        int centerY = (int)centerPixel.y;

        for (int x = centerX - drawSettings.lineWidth; x <= centerX + drawSettings.lineWidth; x++) {
            // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
            if (x >= (int)drawImage.texture.width || x < 0)
                continue;

            for (int y = centerY - drawSettings.lineWidth; y <= centerY + drawSettings.lineWidth; y++) {
                MarkPixelToChange(x, y);
            }
        }
    }

    // Mark the pixels to be changed from startPoint to endPoint
    public void ColorBetween(Vector2 startPoint, Vector2 endPoint) {
        // Get the distance from start to finish
        float distance = Vector2.Distance(startPoint, endPoint);

        Vector2 cur_position = startPoint;

        // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps) {
            cur_position = Vector2.Lerp(startPoint, endPoint, lerp);
            MarkPixelsToColour(cur_position);
        }
    }

    public void MarkPixelToChange(int x, int y) {
        // Need to transform x and y coordinates to flat coordinates of array
        int arrayPosition = (y * (int)drawImage.texture.width) + x;

        // Check if this is a valid position
        if (arrayPosition > currentColors.Length || arrayPosition < 0) {
            return;
        }

        currentColors[arrayPosition] = drawSettings.drawColor;
    }

    public void ApplyCurrentColors() {
        drawTexture.SetPixels32(currentColors);
        drawTexture.Apply();
    }

    // Changes every pixel to be the reset colour
    public void ResetTexture() {
        drawTexture.SetPixels(resetColorsArray);
        drawTexture.Apply();
    }

    //We started a new drag, save the current state so we can go back to this state
    public void OnBeginDrag(PointerEventData eventData) {
        drawSettings.AddUndo(drawTexture.GetPixels32());
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 localCursor = Vector2.zero;
        //This method transforms the mouse position, to a position relative to the image's pivot
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localCursor)) {
            return;
        }

        //Check if the cursor is over the image
        if (localCursor.x < rectTransform.rect.width &&
            localCursor.y < rectTransform.rect.height &&
            localCursor.x > 0 &&
            localCursor.y > 0) {
            float rectToPixelScale = drawImage.rectTransform.rect.width / rectTransform.rect.width;
            localCursor = new Vector2(localCursor.x * rectToPixelScale, localCursor.y * rectToPixelScale);
            Paint(localCursor);
            previousDragPosition = localCursor;
        } else {
            previousDragPosition = Vector2.zero;
        }


    }

    //Reset the previosDragPosition so that our brush knows the next drag is a new line
    public void OnEndDrag(PointerEventData eventData) {
        previousDragPosition = Vector2.zero;
    }

    #region DrawSetting exposed variables
    public void SetDrawColor(Color color) {
        drawSettings.SetDrawColour(color);
    }

    public Color GetDrawColor() {
        return drawSettings.drawColor;
    }

    public void SetDrawLineWidth(int width) {
        drawSettings.SetLineWidth(width);
    }

    public int GetDrawLineWidth() {
        return drawSettings.lineWidth;
    }

    public void SetDrawTransparency(float transparency) {
        drawSettings.SetAlpha(transparency);
    }

    public float GetDrawTransparency() {
        return drawSettings.transparency;
    }
    #endregion
}