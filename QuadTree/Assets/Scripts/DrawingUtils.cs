using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class DrawingUtils
{
    public static void DrawBox(Vector2 topLeft, Vector2 bottomRight, Color color)
    {
        Vector2 tl = topLeft;
        Vector2 tr = new Vector2(bottomRight.x, topLeft.y);
        Vector2 bl = new Vector2(topLeft.x, bottomRight.y);
        Vector2 br = bottomRight;

        DrawLine(tl, tr, color); // top
        DrawLine(bl, br, color); // bottom
        DrawLine(tl, bl, color); // left
        DrawLine(tr, br, color); // right            
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, int width = 2)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    public static void DrawPoint(Vector2 position, float radius, Color drawColor)
    {
        float scale = radius / 2;

        // Todo: Draw sphere with color
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(position.x, position.y);
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.GetComponent<Renderer>().material.color = drawColor;
    }
}
