using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2 position;
    float scale;
    Color color;

    public bool isSpawner;
    public bool visited;

    GameObject mesh;

    public Node(Vector2 _position, float _radius, bool _isSpawner)
    {
        position = _position;
        scale = _radius/2;
        color = Color.gray;

        isSpawner = _isSpawner;
        visited = false;
    }

    public void UpdatePosition(Vector2 _position)
    {
        position = _position;
    }

    public void SetColor(Color _color)
    {
        color = _color;
    }

    public void Draw()
    {
        if (mesh == null)
        {
            mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        mesh.transform.position = new Vector3(position.x, position.y);
        mesh.transform.localScale = new Vector3(scale, scale, scale);

        if (isSpawner)
        {
            mesh.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            mesh.GetComponent<Renderer>().material.color = color;
        }
    }

    public void Reset()
    {
        position = Vector2.zero;
        scale = 0.0f;
        isSpawner = false;
        visited = false;

        if (mesh)
        {
            Object.Destroy(mesh);
        }
    }
}
