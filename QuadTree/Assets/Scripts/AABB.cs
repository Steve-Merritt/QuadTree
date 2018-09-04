using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABB
{
    public Vector2 center;
    public Vector2 halfDimension;

    public Vector2 topLeft;
    public Vector2 botRight;

    public AABB(Vector2 _center, Vector2 _halfDimension)
    {
        center = _center;
        halfDimension = _halfDimension;

        topLeft.x = center.x - halfDimension.x;
        topLeft.y = center.y + halfDimension.y;

        botRight.x = center.x + halfDimension.x;
        botRight.y = center.y - halfDimension.y;
    }

    public bool ContainsPoint(Vector2 _pt)
    {
        if (_pt.x < topLeft.x) return false;
        if (_pt.x > botRight.x) return false;
        if (_pt.y > topLeft.y) return false;
        if (_pt.y < botRight.y) return false;

        return true;
    }

    public bool IntersectsAABB(AABB other)
    {
        if (other.topLeft.x > botRight.x) return false; // other is to the right
        if (other.topLeft.y < botRight.y) return false; // other is below
        if (other.botRight.x < topLeft.x) return false; // other is to the left
        if (other.botRight.y > topLeft.y) return false; // other is above

        return true;
    }

    public void Draw()
    {
        DrawingUtils.DrawBox(topLeft, botRight, Color.green);
    }
}
