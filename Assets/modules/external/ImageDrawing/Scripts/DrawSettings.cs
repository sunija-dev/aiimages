using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class DrawSettings 
{
    // This is the draw color
    public Color drawColor = Color.red;
    // This is the radius in pixels in which we color the image
    public int lineWidth = 3;

    // The transparency of the current draw color
    public float transparency = 1;

    public Texture2D texBrush;

    // These are the stacks used to manage undo/redo
    public Stack<Color32[]> undos;
    public Stack<Color32[]> redos;

    public DrawSettings() {
        undos = new Stack<Color32[]>();
        redos = new Stack<Color32[]>();
    }

    public void SetDrawColour(Color new_color) {
        drawColor = new_color;
    }

    public void SetLineWidth(int new_width) {
        lineWidth = new_width;
    }

    public void SetAlpha(float amount) {
        Color c = drawColor;
        c.a = amount;
        drawColor = c;
        transparency = amount;
    }

    public void AddUndo(Color32[] undo) {
        undos.Push(undo);
        redos.Clear();
    }

    public Color32[] Undo(Color32[] newState) {
        Color32[] undoToGet = undos.Pop();
        redos.Push(newState);
        return undoToGet;
    }

    public bool CanUndo() {
        return undos.Count > 0;
    }

    public Color32[] Redo(Color32[] newState) {
        Color32[] redoToGet = redos.Pop();
        undos.Push(newState);
        return redoToGet;
    }

    public bool CanRedo() {
        return redos.Count > 0;
    }

}