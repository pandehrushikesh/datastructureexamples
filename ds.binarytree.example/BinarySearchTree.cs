using ds.shared;

namespace ds.binarytree.example;

// ── Node ─────────────────────────────────────────────────────────────────────

/// <summary>One node in the BST — holds a value, a left child, and a right child.</summary>
public class BstNode<T>
{
    public T Value;
    public BstNode<T>? Left;
    public BstNode<T>? Right;

    public BstNode(T value) { Value = value; }
    public override string ToString() => $"({Value})";
}

// ── Binary Search Tree ────────────────────────────────────────────────────────

/// <summary>
/// Binary Search Tree (BST) built from scratch.
/// No SortedSet&lt;T&gt; or any BCL collection — the goal is to see every pointer move.
///
/// BST PROPERTY (maintained at all times):
///   For every node N:
///     • every value in N's LEFT  subtree is LESS    THAN N.Value
///     • every value in N's RIGHT subtree is GREATER THAN N.Value
///
/// This ordering is what makes search O(log n) on average — at each node
/// we eliminate roughly half the remaining tree.
///
/// Visual example (inserted order: 50, 30, 70, 20, 40, 60, 80):
///
///              (50)          ← root
///             /    \
///          (30)    (70)
///          /  \    /  \
///        (20)(40)(60)(80)
///
/// Traversal orders from this tree:
///   InOrder   (L→N→R): 20 30 40 50 60 70 80  ← always sorted!
///   PreOrder  (N→L→R): 50 30 20 40 70 60 80
///   PostOrder (L→R→N): 20 40 30 60 80 70 50
///   LevelOrder (BFS) : 50 30 70 20 40 60 80
/// </summary>
public class BinarySearchTree<T> where T : IComparable<T>
{
    private BstNode<T>? _root;
    public int  Count   { get; private set; }
    public bool IsEmpty => _root is null;

    // ── Insert ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a value, maintaining the BST property.
    /// Average O(log n) — each comparison halves the search space.
    /// Worst case O(n) when the tree degenerates (e.g. inserting sorted data).
    /// </summary>
    public void Insert(T value)
    {
        BigO.Print("O(log n) average  /  O(n) worst case",
            "At each node we compare and descend left or right, halving the candidates each step. " +
            "Worst case is a degenerate tree (all values inserted in sorted order) which becomes a linked list.");

        Console.WriteLine($"\n[Insert] Inserting '{value}'...");

        if (_root is null)
        {
            _root = new BstNode<T>(value);
            Count++;
            Console.WriteLine($"         Tree was empty — '{value}' becomes the root.");
            PrintTree();
            return;
        }

        var current = _root;
        while (true)
        {
            int cmp = value.CompareTo(current.Value);
            if (cmp == 0)
            {
                Console.WriteLine($"         '{value}' already exists — duplicates not allowed. No change.");
                return;
            }
            else if (cmp < 0)
            {
                Console.WriteLine($"         '{value}' < '{current.Value}' → go LEFT");
                if (current.Left is null)
                {
                    current.Left = new BstNode<T>(value);
                    Count++;
                    Console.WriteLine($"         Left child of '{current.Value}' is null — inserted here.");
                    break;
                }
                current = current.Left;
            }
            else
            {
                Console.WriteLine($"         '{value}' > '{current.Value}' → go RIGHT");
                if (current.Right is null)
                {
                    current.Right = new BstNode<T>(value);
                    Count++;
                    Console.WriteLine($"         Right child of '{current.Value}' is null — inserted here.");
                    break;
                }
                current = current.Right;
            }
        }
        PrintTree();
    }

    // ── Contains ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the value exists in the tree.
    /// Average O(log n) — the BST property lets us eliminate half the tree at each step.
    /// </summary>
    public bool Contains(T value)
    {
        BigO.Print("O(log n) average  /  O(n) worst case",
            "The BST property guides us: go left if smaller, right if larger. " +
            "Each comparison eliminates an entire subtree — no need to look there.");

        Console.WriteLine($"\n[Contains] Searching for '{value}'...");
        var current = _root;
        while (current is not null)
        {
            int cmp = value.CompareTo(current.Value);
            if (cmp == 0)
            {
                Console.WriteLine($"           Visited {current} → MATCH ✓");
                return true;
            }
            else if (cmp < 0)
            {
                Console.WriteLine($"           Visited {current} → '{value}' < '{current.Value}', go LEFT");
                current = current.Left;
            }
            else
            {
                Console.WriteLine($"           Visited {current} → '{value}' > '{current.Value}', go RIGHT");
                current = current.Right;
            }
        }
        Console.WriteLine($"           Reached null — '{value}' not found.");
        return false;
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes the node with the given value. Three cases:
    ///   Case 1 — Leaf node         : simply unlink it.
    ///   Case 2 — One child         : replace node with its only child.
    ///   Case 3 — Two children      : replace value with in-order successor
    ///                                (smallest value in the right subtree),
    ///                                then delete that successor node.
    /// Average O(log n).
    /// </summary>
    public bool Remove(T value)
    {
        BigO.Print("O(log n) average  /  O(n) worst case",
            "Finding the node is O(log n). Removal itself is O(1) pointer work for cases 1 & 2. " +
            "Case 3 adds one more O(log n) descent to find the in-order successor.");

        Console.WriteLine($"\n[Remove] Removing '{value}'...");
        bool found = false;
        _root = RemoveRecursive(_root, value, ref found);
        if (!found) Console.WriteLine($"         '{value}' not found — nothing removed.");
        else { Console.WriteLine($"         Done. Count={Count}."); PrintTree(); }
        return found;
    }

    private BstNode<T>? RemoveRecursive(BstNode<T>? node, T value, ref bool found)
    {
        if (node is null) return null;

        int cmp = value.CompareTo(node.Value);

        if (cmp < 0)
        {
            Console.WriteLine($"         '{value}' < '{node.Value}' → search LEFT subtree");
            node.Left = RemoveRecursive(node.Left, value, ref found);
        }
        else if (cmp > 0)
        {
            Console.WriteLine($"         '{value}' > '{node.Value}' → search RIGHT subtree");
            node.Right = RemoveRecursive(node.Right, value, ref found);
        }
        else
        {
            found = true;
            Console.WriteLine($"         Found {node}!");

            // Case 1: leaf
            if (node.Left is null && node.Right is null)
            {
                Console.WriteLine($"         Case 1 — Leaf node. Simply unlink.");
                Count--;
                return null;
            }
            // Case 2a: only right child
            if (node.Left is null)
            {
                Console.WriteLine($"         Case 2 — Only RIGHT child {node.Right}. Replace node with it.");
                Count--;
                return node.Right;
            }
            // Case 2b: only left child
            if (node.Right is null)
            {
                Console.WriteLine($"         Case 2 — Only LEFT child {node.Left}. Replace node with it.");
                Count--;
                return node.Left;
            }
            // Case 3: two children — find in-order successor (min of right subtree)
            Console.WriteLine($"         Case 3 — Two children. Finding in-order successor (min of right subtree)...");
            var successor = FindMin(node.Right);
            Console.WriteLine($"         In-order successor = {successor}. Copying its value into this node.");
            node.Value = successor.Value;
            Console.WriteLine($"         Now deleting the successor {successor} from the right subtree.");
            node.Right = RemoveRecursive(node.Right, successor.Value, ref found);
            // count was already decremented inside the recursive call above
        }
        return node;
    }

    // ── Min / Max ─────────────────────────────────────────────────────────────

    /// <summary>Returns the minimum value — walk left until null. O(log n) avg.</summary>
    public T Min()
    {
        if (_root is null) throw new InvalidOperationException("Tree is empty.");
        BigO.Print("O(log n) average  /  O(n) worst case",
            "The minimum is always the leftmost node. Walk left until there is no left child.");

        Console.WriteLine("\n[Min] Walking left to find minimum...");
        return FindMin(_root).Value;
    }

    /// <summary>Returns the maximum value — walk right until null. O(log n) avg.</summary>
    public T Max()
    {
        if (_root is null) throw new InvalidOperationException("Tree is empty.");
        BigO.Print("O(log n) average  /  O(n) worst case",
            "The maximum is always the rightmost node. Walk right until there is no right child.");

        Console.WriteLine("\n[Max] Walking right to find maximum...");
        var current = _root;
        while (current.Right is not null)
        {
            Console.WriteLine($"      {current} → go right");
            current = current.Right;
        }
        Console.WriteLine($"      Rightmost node = {current} ✓");
        return current.Value;
    }

    private BstNode<T> FindMin(BstNode<T> node)
    {
        while (node.Left is not null)
        {
            Console.WriteLine($"      {node} → go left");
            node = node.Left;
        }
        Console.WriteLine($"      Leftmost node = {node} ✓");
        return node;
    }

    // ── Height ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the height of the tree (longest root-to-leaf path, 0 for a single node).
    /// Must visit every node — O(n).
    /// </summary>
    public int Height()
    {
        BigO.Print("O(n)",
            "Height requires visiting every node to compare left and right subtree depths. " +
            "There is no shortcut — every path must be measured.");

        int h = HeightRecursive(_root);
        Console.WriteLine($"\n[Height] Tree height = {h}");
        return h;
    }

    private static int HeightRecursive(BstNode<T>? node)
    {
        if (node is null) return -1;
        int left  = HeightRecursive(node.Left);
        int right = HeightRecursive(node.Right);
        return 1 + Math.Max(left, right);
    }

    // ── Traversals ────────────────────────────────────────────────────────────

    /// <summary>
    /// In-order: Left → Node → Right.
    /// KEY INSIGHT: in-order traversal of a BST always yields values in sorted order.
    /// O(n) — every node visited exactly once.
    /// </summary>
    public void InOrder()
    {
        BigO.Print("O(n)",
            "Every node is visited exactly once. " +
            "InOrder (Left→Node→Right) on a BST always produces sorted output — this is its defining property.");

        Console.Write("\n[InOrder]   ");
        InOrderRecursive(_root);
        Console.WriteLine();
    }

    private void InOrderRecursive(BstNode<T>? node)
    {
        if (node is null) return;
        InOrderRecursive(node.Left);
        Console.Write($"{node.Value}  ");
        InOrderRecursive(node.Right);
    }

    /// <summary>
    /// Pre-order: Node → Left → Right.
    /// Useful for serialising / copying the tree — the root always comes first.
    /// O(n).
    /// </summary>
    public void PreOrder()
    {
        BigO.Print("O(n)",
            "Every node is visited exactly once. " +
            "PreOrder (Node→Left→Right) visits the root first — useful for tree serialisation and copying.");

        Console.Write("\n[PreOrder]  ");
        PreOrderRecursive(_root);
        Console.WriteLine();
    }

    private void PreOrderRecursive(BstNode<T>? node)
    {
        if (node is null) return;
        Console.Write($"{node.Value}  ");
        PreOrderRecursive(node.Left);
        PreOrderRecursive(node.Right);
    }

    /// <summary>
    /// Post-order: Left → Right → Node.
    /// Useful for deletion (children before parent) and expression-tree evaluation.
    /// O(n).
    /// </summary>
    public void PostOrder()
    {
        BigO.Print("O(n)",
            "Every node is visited exactly once. " +
            "PostOrder (Left→Right→Node) processes children before parents — used in tree deletion and expression evaluation.");

        Console.Write("\n[PostOrder] ");
        PostOrderRecursive(_root);
        Console.WriteLine();
    }

    private void PostOrderRecursive(BstNode<T>? node)
    {
        if (node is null) return;
        PostOrderRecursive(node.Left);
        PostOrderRecursive(node.Right);
        Console.Write($"{node.Value}  ");
    }

    /// <summary>
    /// Level-order (BFS): visits nodes row by row, top to bottom.
    /// Uses an internal queue. O(n).
    /// </summary>
    public void LevelOrder()
    {
        BigO.Print("O(n)",
            "Every node is enqueued and dequeued exactly once. " +
            "LevelOrder (BFS) visits the tree row by row — useful for finding the shortest path or printing by level.");

        Console.WriteLine("\n[LevelOrder] Row by row (BFS):");
        if (_root is null) { Console.WriteLine("  (empty)"); return; }

        // Use Queue<BstNode<T>> from BCL — the point here is the traversal algorithm,
        // not reimplementing a queue. The BST itself is built from scratch.
        var queue = new Queue<BstNode<T>>();
        queue.Enqueue(_root);
        int level = 0;

        while (queue.Count > 0)
        {
            int levelSize = queue.Count;
            Console.Write($"  Level {level}: ");
            for (int i = 0; i < levelSize; i++)
            {
                var node = queue.Dequeue();
                Console.Write($"{node.Value}  ");
                if (node.Left  is not null) queue.Enqueue(node.Left);
                if (node.Right is not null) queue.Enqueue(node.Right);
            }
            Console.WriteLine();
            level++;
        }
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Drops the root reference — O(1). GC collects the entire tree.
    /// In C++ you would post-order traverse and delete every node manually.
    /// </summary>
    public void Clear()
    {
        BigO.Print("O(1)",
            "Setting the root to null makes the entire tree unreachable. " +
            "The GC collects every node automatically — in C++ you'd need a post-order walk to delete each node.");

        Console.WriteLine($"\n[Clear] Dropping root reference. GC will collect all {Count} node(s).");
        _root  = null;
        Count  = 0;
        Console.WriteLine("[Clear] Tree is now empty.");
    }

    // ── Visual tree printer ───────────────────────────────────────────────────

    /// <summary>
    /// Prints the tree rotated 90° counter-clockwise (right subtree at top).
    /// Each level is indented by 4 spaces so the structure is visible in the console.
    /// </summary>
    public void PrintTree(string label = "Tree")
    {
        Console.WriteLine($"\n  ── {label} (Count={Count}) ──");
        if (_root is null) { Console.WriteLine("  (empty)"); return; }
        PrintTreeRecursive(_root, "", isRight: true);
    }

    private static void PrintTreeRecursive(BstNode<T>? node, string indent, bool isRight)
    {
        if (node is null) return;

        // Print right subtree first (it appears at the top when rotated)
        PrintTreeRecursive(node.Right, indent + (isRight ? "        " : "  │     "), isRight: true);

        Console.WriteLine($"{indent}{(isRight ? "  ┌─── " : "  └─── ")}{node.Value}");

        // Print left subtree
        PrintTreeRecursive(node.Left, indent + (isRight ? "  │     " : "        "), isRight: false);
    }
}
