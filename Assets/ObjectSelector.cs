using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    public Texture texture;
    
    private Vector2 _rectStartPos;
    private Vector2 _rectEndPos;
    public List<Vector2[]> SelectedList { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        if (Camera.main != null)
            Camera.main.depthTextureMode = DepthTextureMode.Depth;
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        // When mouse is down, either it's the first click or dragging
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Called on the first update where the user has pressed the mouse button.
            if (Input.GetKeyDown(KeyCode.Mouse0))
                _rectStartPos = Input.mousePosition;
            else  // Else we must be in "drag" mode.
                _rectEndPos = Input.mousePosition;  
        }
        // When mouse is released, record rectangle
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            var diagonalPoints = new[] {_rectStartPos, _rectEndPos};
            SelectedList.Add(diagonalPoints);
        }
    }

    private void OnGUI()
    {
        // Draw existing selections.
        foreach (var rect in SelectedList.Select(
            diagonalPoints => MakeRectInScreenCoordinate(diagonalPoints[0], diagonalPoints[1]))
        )
        {
            GUI.DrawTexture(rect, texture);
        }

        // Draw current selection if we are in the middle of a selection.
        if (_rectStartPos != Vector2.zero && _rectEndPos != Vector2.zero)
        {
            var rect = MakeRectInScreenCoordinate(_rectStartPos, _rectEndPos);
            // Draw the texture.
            GUI.DrawTexture(rect, texture);
        }
    }

    // Create a rectangle object out of the start and end position while transforming it to the screen's coordinates.
    private static Rect MakeRectInScreenCoordinate(Vector2 startPos, Vector2 endPos)
    {
        return new Rect(startPos.x, Screen.height - startPos.y,
            endPos.x - startPos.x,
            -1 * (endPos.y - startPos.y));
    }

    public void Reset()
    {
        _rectStartPos = Vector2.zero;
        _rectEndPos = Vector2.zero;
        SelectedList = new List<Vector2[]>();
    }
}
