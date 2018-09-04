using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    Vector3 start;
    Vector3 end;
    Color color;
    int width;

    GameObject line;

    public Path(Vector3 _start, Vector3 _end, Color _color, int _width = 2)
    {
        start = _start;
        end = _end;
        color = _color;
        width = _width;
    }

    public void Draw()
    {
        if (line == null)
        {
            line = new GameObject();
        }
        
        line.transform.position = start;
        line.AddComponent<LineRenderer>();
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    public void Destroy()
    {
        if (line)
        {
            Object.Destroy(line);
        }
    }
}
