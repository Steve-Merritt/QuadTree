using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree : MonoBehaviour
{

    Tree root;

    // The objects that we want stored in the quadtree
    public class Node
    {
        public Vector2 pos;
        public bool isSpawner;
        public bool visited;
        public Node(Vector2 _pos, bool _isSpawner)
        {
            pos = _pos;
            isSpawner = _isSpawner;
            visited = false;
        }
    };

    private List<Node> myNodes = new List<Node>();

    // Axis-aligned bounding box with half dimension and center
    class AABB
    {
        public Vector2 center;
        public Vector2 halfDimension;

        public AABB(Vector2 _center, Vector2 _halfDimension)
        {
            center = _center;
            halfDimension = _halfDimension;
        }

        public bool ContainsPoint(Vector2 _pt)
        {
            if (_pt.x > center.x + halfDimension.x)
                return false;

            if (_pt.x < center.x - halfDimension.x)
                return false;

            if (_pt.y > center.y + halfDimension.y)
                return false;

            if (_pt.y < center.y - halfDimension.y)
                return false;

            return true;
        }

        public bool IntersectsAABB(AABB _other)
        {
            if (_other.center.x - _other.halfDimension.x > center.x + halfDimension.x)
                return false;

            if (_other.center.x + _other.halfDimension.x < center.x - halfDimension.x)
                return false;

            if (_other.center.y - _other.halfDimension.y > center.y + halfDimension.y)
                return false;

            if (_other.center.y + _other.halfDimension.y < center.y - halfDimension.y)
                return false;

            return true;
        }

        public void Draw()
        {
            // Todo: DrawBox(new Vector2(center.x - halfDimension.x, center.y - halfDimension.y), new Vector2(center.x + halfDimension.x, center.y + halfDimension.y));
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGenerate()
    {
        myNodes.Clear();
        const int NUM_NODES = 500;
        for (int i = 0; i < NUM_NODES; i++)
        {
            int x = Random.Range(10, 790);
            int y = Random.Range(10, 600);
            bool isSpawner = Random.Range(0, 10) == 1;

            Node node = new Node(new Vector2(x, y), isSpawner);
            myNodes.Add(node);
        }

        // build quadtree
        Vector2 tl = new Vector2(0, 0);
        Vector2 br = new Vector2(800, 600);
        root = new Tree(tl, br);
        foreach (Node node in myNodes)
        {
            root.Insert(node);
        }

        List<Node> nodesInRange = new List<Node>();
        foreach (Node node in myNodes)
        {
            if (node.isSpawner)
            {
                nodesInRange.AddRange(GetNodesInPathRange(node));
            }
        }

        // draw points
        foreach (Node node in myNodes)
        {
            if (node.isSpawner)
            {
                DrawPoint(node.pos, Color.red);
            }
            else
            {
                DrawPoint(node.pos);
            }
        }

        // draw traversable nodes
        foreach (Node node in nodesInRange)
        {
            if (!node.isSpawner)
            {
                DrawPoint(node.pos, Color.green);
            }
        }
    }
    List<Node> GetNodesInPathRange(Node _node)
    {
        _node.visited = true;

        AABB range = new AABB(_node.pos, new Vector2(20, 20));

        List<Node> nodesInRange = new List<Node>();

        // Check if there are any more unvisited nodes within range
        List<Node> tempRange = root.QueryRange(range);
        if (tempRange.Count > 0)
        {
            foreach (Node node in tempRange)
            {
                DrawLine(_node.pos, node.pos);

                if (node.visited == true)
                    continue; // skip nodes already checked

                nodesInRange.AddRange(GetNodesInPathRange(node));
            }
        }

        nodesInRange.Add(_node); // add myself

        return nodesInRange;
    }

    private bool NodeInRange(Node n1, Node n2, int d)
    {
        return ((n2.pos.x - n1.pos.x) * (n2.pos.x - n1.pos.x) + (n2.pos.y - n1.pos.y) * (n2.pos.y - n1.pos.y)) < d * d;
    }

    public void DrawBox(Vector2 topLeft, Vector2 bottomRight)
    {
        Vector2 tl = topLeft;
        Vector2 tr = new Vector2(bottomRight.x, topLeft.y);
        Vector2 bl = new Vector2(topLeft.x, bottomRight.y);
        Vector2 br = bottomRight;

        DrawLine(tl, tr); // top
        DrawLine(bl, br); // bottom
        DrawLine(tl, bl); // left
        DrawLine(tr, br); // right            
    }

    private void DrawLine(Vector2 p1, Vector2 p2)
    {
        // Todo: Line Renderer
    }

    private void DrawPoint(Vector2 p)
    {
        DrawPoint(p, Color.gray);
    }

    private void DrawPoint(Vector2 p, Color drawColor)
    {
        float size = 6;
        float x = p.x - size / 2;
        float y = p.y - size / 2;

        // Todo: Draw sphere with color
    }

    // The main quadtree class
    class Tree
    {
        // Hold details of the boundary of this node
        Vector2 topLeft;
        Vector2 botRight;
        AABB boundary;

        // Contains details of node
        Node n;

        // Children of this tree
        Tree topLeftTree;
        Tree topRightTree;
        Tree botLeftTree;
        Tree botRightTree;

        public Tree()
        {
            topLeft = new Vector2(0, 0);
            botRight = new Vector2(0, 0);
            boundary = new AABB(new Vector2(0, 0), new Vector2(0, 0));
        }

        public Tree(Vector2 topL, Vector2 botR)
        {
            topLeft = topL;
            botRight = botR;
            Vector2 halfDim = new Vector2((botR.x - topL.x) / 2, (botR.y - topL.y) / 2);
            Vector2 center = new Vector2(topL.x + halfDim.x, topL.y + halfDim.y);
            boundary = new AABB(center, halfDim);
        }

        // Insert a node into the quadtree
        public void Insert(Node node)
        {
            // Current quad cannot contain it
            if (!InBoundary(node.pos))
                return;

            // We are at a quad of unit area
            // We cannot subdivide this quad further
            if (Mathf.Abs(topLeft.x - botRight.x) <= 1 &&
                Mathf.Abs(topLeft.y - botRight.y) <= 1)
            {
                n = node;
                return;
            }

            if ((topLeft.x + botRight.x) / 2 >= node.pos.x)
            {
                // Indicates topLeftTree
                if ((topLeft.y + botRight.y) / 2 >= node.pos.y)
                {
                    if (topLeftTree == null)
                    {
                        topLeftTree = new Tree(
                            new Vector2(topLeft.x, topLeft.y),
                            new Vector2((topLeft.x + botRight.x) / 2, (topLeft.y + botRight.y) / 2));
                    }

                    topLeftTree.Insert(node);
                }

                // Indicates botLeftTree
                else
                {
                    if (botLeftTree == null)
                    {
                        botLeftTree = new Tree(
                            new Vector2(topLeft.x, (topLeft.y + botRight.y) / 2),
                            new Vector2((topLeft.x + botRight.x) / 2, botRight.y));
                    }

                    botLeftTree.Insert(node);
                }
            }
            else
            {
                // Indicates topRightTree
                if ((topLeft.y + botRight.y) / 2 >= node.pos.y)
                {
                    if (topRightTree == null)
                    {
                        topRightTree = new Tree(
                            new Vector2((topLeft.x + botRight.x) / 2, topLeft.y),
                            new Vector2(botRight.x, (topLeft.y + botRight.y) / 2));
                    }

                    topRightTree.Insert(node);
                }

                // Indicates botRightTree
                else
                {
                    if (botRightTree == null)
                    {
                        botRightTree = new Tree(
                            new Vector2((topLeft.x + botRight.x) / 2, (topLeft.y + botRight.y) / 2),
                            new Vector2(botRight.x, botRight.y));
                    }

                    botRightTree.Insert(node);
                }
            }
        }

        // Find a node in a quadtree
        public Node Search(Vector2 p)
        {
            // Current quad cannot contain it
            if (!InBoundary(p))
                return null;

            // We are at a quad of unit length
            // We cannot subdivide this quad further
            if (n != null)
                return n;

            if ((topLeft.x + botRight.x) / 2 >= p.x)
            {
                // Indicates topLeftTree
                if ((topLeft.y + botRight.y) / 2 >= p.y)
                {
                    if (topLeftTree == null)
                        return null;
                    return topLeftTree.Search(p);
                }

                // Indicates botLeftTree
                else
                {
                    if (botLeftTree == null)
                        return null;
                    return botLeftTree.Search(p);
                }
            }
            else
            {
                // Indicates topRightTree
                if ((topLeft.y + botRight.y) / 2 >= p.y)
                {
                    if (topRightTree == null)
                        return null;
                    return topRightTree.Search(p);
                }

                // Indicates botRightTree
                else
                {
                    if (botRightTree == null)
                        return null;
                    return botRightTree.Search(p);
                }
            }
        }

        public void Draw()
        {
            // draw myself
            boundary.Draw();

            // draw children
            if (topLeftTree != null)
                topLeftTree.Draw();

            if (topRightTree != null)
                topRightTree.Draw();

            if (botLeftTree != null)
                botLeftTree.Draw();

            if (botRightTree != null)
                botRightTree.Draw();
        }

        // Check if current quadtree contains the Vector2
        bool InBoundary(Vector2 p)
        {
            return (p.x >= topLeft.x &&
                p.x <= botRight.x &&
                p.y >= topLeft.y &&
                p.y <= botRight.y);
        }

        // Find all nodes that appear within a range
        public List<Node> QueryRange(AABB range)
        {
            // Prepare an array of results
            List<Node> nodesInRange = new List<Node>();

            // Automatically abort if the range does not intersect this quad
            if (!boundary.IntersectsAABB(range))
                return nodesInRange; // empty list

            // Check objects at this quad level
            if (n != null && range.ContainsPoint(n.pos))
                nodesInRange.Add(n);

            // Terminate here, if there are no children
            if (topLeftTree != null)
                nodesInRange.AddRange(topLeftTree.QueryRange(range));

            if (topRightTree != null)
                nodesInRange.AddRange(topRightTree.QueryRange(range));

            if (botLeftTree != null)
                nodesInRange.AddRange(botLeftTree.QueryRange(range));

            if (botRightTree != null)
                nodesInRange.AddRange(botRightTree.QueryRange(range));

            return nodesInRange;
        }
    }

    private void OnDrawTree()
    {
        if (root != null)
        {
            root.Draw();
        }
    }
}
