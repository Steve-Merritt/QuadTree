using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree : MonoBehaviour
{
    [SerializeField] float nodeRadius = 10;
    [SerializeField] int nodeCount = 1000;
    [SerializeField] int spawnerChance = 10;
    [SerializeField] int maxRange = 20;

    const int width = 1500;
    const int height = 1080;

    Tree root;
    static int treeCapacity = 1;

    private List<Node> nodes = new List<Node>();
    private List<Path> paths = new List<Path>();

    //
    // Button Callbacks
    //

    public void OnGenerate()
    {
        Clear();
        CreateNodes();
        BuildTree();
        UpdatePaths();
        DrawNodes();
    }

    public void OnDrawTree()
    {
        if (root != null)
        {
            root.Draw();
        }
    }

    //
    // Private methods
    //

    private void Clear()
    {
        foreach (Node node in nodes)
        {
            node.Reset();
        }
        nodes.Clear();

        foreach (Path path in paths)
        {
            path.Destroy();
        }
        paths.Clear();
    }

    private void CreateNodes()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            bool isSpawner = Random.Range(0, spawnerChance) == 1;

            Node node = new Node(new Vector2(x, y), nodeRadius, isSpawner);
            nodes.Add(node);
        }
    }

    private void BuildTree()
    {
        Vector2 tl = new Vector2(0, height);
        Vector2 br = new Vector2(width, 0);
        root = new Tree(tl, br);
        foreach (Node node in nodes)
        {
            root.Insert(node);
        }
    }

    private void UpdatePaths()
    {
        List<Node> nodesInRange = new List<Node>();
        foreach (Node node in nodes)
        {
            if (node.isSpawner)
            {
                nodesInRange.AddRange(GetNodesInPathRange(node));
            }
        }

        foreach (Node node in nodesInRange)
        {
            if (!node.isSpawner)
            {
                node.SetColor(Color.blue);
            }
        }
    }

    private void DrawNodes()
    {
        foreach (Node node in nodes)
        {
            node.Draw();
        }
    }

    List<Node> GetNodesInPathRange(Node _node)
    {
        _node.visited = true;

        AABB range = new AABB(_node.position, new Vector2(maxRange, maxRange));
        //range.Draw();

        List<Node> nodesInRange = new List<Node>();

        // Check if there are any more unvisited nodes within range
        List<Node> tempRange = root.QueryRange(range);
        if (tempRange.Count > 0)
        {
            foreach (Node n in tempRange)
            {
                Path path = new Path(_node.position, n.position, Color.green);
                path.Draw();
                paths.Add(path);

                if (n.visited == true)
                    continue; // skip nodes already checked

                nodesInRange.AddRange(GetNodesInPathRange(n));
            }
        }

        nodesInRange.Add(_node); // add myself

        return nodesInRange;
    }

    private bool NodeInRange(Node n1, Node n2, int d)
    {
        return ((n2.position.x - n1.position.x) * (n2.position.x - n1.position.x) + (n2.position.y - n1.position.y) * (n2.position.y - n1.position.y)) < d * d;
    }  

    // The main quadtree class
    class Tree
    {
        // Hold details of the boundary of this node
        AABB bounds;

        // Contains details of node
        List<Node> nodes = new List<Node>();

        // Children of this tree
        Tree northWest;
        Tree northEast;
        Tree southWest;
        Tree southEast;

        public Tree(Vector2 topL, Vector2 botR)
        {
            Vector2 halfDim = new Vector2((botR.x - topL.x) / 2, (topL.y - botR.y) / 2);
            Vector2 center = new Vector2(topL.x + halfDim.x, topL.y - halfDim.y);
            bounds = new AABB(center, halfDim);
        }

        // Insert a node into the quadtree
        public bool Insert(Node node)
        {
            // Ignore objects that do not belong in this quad tree
            if (!bounds.ContainsPoint(node.position))
            {
                return false;
            }

            // If there is space in this quad tree, add the object here
            if (nodes.Count < treeCapacity)
            {
                nodes.Add(node);
                return true;
            }

            // Otherwise, subdivide and then add the point to whichever node will accept it
            if (northWest == null)
            {
                Subdivide();
            }

            if (northWest.Insert(node)) return true;
            if (northEast.Insert(node)) return true;
            if (southWest.Insert(node)) return true;
            if (southEast.Insert(node)) return true;

            // Otherwise, the point cannot be inserted for some unknown reason (this should never happen)
            return false;
        }

        private void Subdivide()
        {
            northWest = new Tree(new Vector2(bounds.topLeft.x, bounds.topLeft.y), new Vector2(bounds.center.x, bounds.center.y));
            northEast = new Tree(new Vector2(bounds.center.x, bounds.topLeft.y), new Vector2(bounds.botRight.x, bounds.center.y));
            southWest = new Tree(new Vector2(bounds.topLeft.x, bounds.center.y), new Vector2(bounds.center.x, bounds.botRight.y));
            southEast = new Tree(new Vector2(bounds.center.x, bounds.center.y), new Vector2(bounds.botRight.x, bounds.botRight.y));
        }

        // Find all nodes that appear within a range
        public List<Node> QueryRange(AABB range)
        {
            // Prepare an array of results
            List<Node> nodesInRange = new List<Node>();

            // Automatically abort if the range does not intersect this quad
            if (!bounds.IntersectsAABB(range))
                return nodesInRange; // empty list

            // Check objects at this quad level
            foreach (Node n in nodes)
            {
                if (range.ContainsPoint(n.position))
                    nodesInRange.Add(n);
            }

            if (northWest == null)
                return nodesInRange;

            nodesInRange.AddRange(northWest.QueryRange(range));
            nodesInRange.AddRange(northEast.QueryRange(range));
            nodesInRange.AddRange(southWest.QueryRange(range));
            nodesInRange.AddRange(southEast.QueryRange(range));

            return nodesInRange;
        }

        public void Draw()
        {
            // draw myself
            bounds.Draw();

            // bail if not subdivided
            if (northWest == null) return;

            // draw children
            northWest.Draw();
            northEast.Draw();
            southWest.Draw();
            southEast.Draw();
        }
    }
}

/*
// Find a node in a quadtree
public Node Search(Vector2 p)
{
    // Current quad cannot contain it
    if (!InBoundary(p))
        return null;

    // We are at a quad of unit length
    // We cannot subdivide this quad further
    if (nodes != null)
        return nodes;

    if ((topLeft.x + botRight.x) / 2 >= p.x)
    {
        // Indicates topLeftTree
        if ((topLeft.y + botRight.y) / 2 >= p.y)
        {
            if (northWest == null)
                return null;
            return northWest.Search(p);
        }

        // Indicates botLeftTree
        else
        {
            if (southWest == null)
                return null;
            return southWest.Search(p);
        }
    }
    else
    {
        // Indicates topRightTree
        if ((topLeft.y + botRight.y) / 2 >= p.y)
        {
            if (northEast == null)
                return null;
            return northEast.Search(p);
        }

        // Indicates botRightTree
        else
        {
            if (southEast == null)
                return null;
            return southEast.Search(p);
        }
    }
}
*/