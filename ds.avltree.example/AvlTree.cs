using ds.shared;

namespace ds.avltree.example;

// ── Node ─────────────────────────────────────────────────────────────────────

/// <summary>
/// AVL node: same as a BST node but also stores the HEIGHT of its subtree.
/// Height is used to compute the balance factor without walking the tree each time.
/// </summary>
public class AvlNode<T>
{
    public T         Value;
    public AvlNode<T>? Left;
    public AvlNode<T>? Right;
    public int       Height; // height of subtree rooted here (leaf = 0)

    public AvlNode(T value) { Value = value; Height = 0; }
    public override string ToString() => $"({Value}|h={Height})";
}

// ── AVL Tree ──────────────────────────────────────────────────────────────────

/// <summary>
/// AVL Tree — a self-balancing BST invented by Adelson-Velsky and Landis (1962).
///
/// BALANCE FACTOR of a node = height(left subtree) - height(right subtree)
///   -1, 0, or +1  →  balanced (no action needed)
///    +2            →  left-heavy  → right rotation (or left-right double rotation)
///    -2            →  right-heavy → left rotation  (or right-left double rotation)
///
/// After every Insert or Remove, the tree walks back up to the root and
/// rebalances any node whose balance factor falls outside [-1, 1].
///
/// This guarantees height ≤ 1.44 log₂(n) always — making all key operations
/// O(log n) in the WORST case, unlike a plain BST which can degrade to O(n).
///
/// 4 rotation cases:
///
///   LL (inserted into LEFT  of LEFT  child)  → single RotateRight
///   RR (inserted into RIGHT of RIGHT child)  → single RotateLeft
///   LR (inserted into RIGHT of LEFT  child)  → RotateLeft(child) then RotateRight(root)
///   RL (inserted into LEFT  of RIGHT child)  → RotateRight(child) then RotateLeft(root)
/// </summary>
public class AvlTree<T> where T : IComparable<T>
{
    private AvlNode<T>? _root;
    public int  Count   { get; private set; }
    public bool IsEmpty => _root is null;

    // ── Height helpers ────────────────────────────────────────────────────────

    private static int H(AvlNode<T>? n)  => n?.Height ?? -1;   // null node has height -1
    private static int BF(AvlNode<T>? n) => n is null ? 0 : H(n.Left) - H(n.Right);
    private static void UpdateHeight(AvlNode<T> n) =>
        n.Height = 1 + Math.Max(H(n.Left), H(n.Right));

    // ── Rotations ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Right rotation around <paramref name="y"/> — used when left-heavy (LL case).
    ///
    ///      y                  x
    ///     / \               /   \
    ///    x   C    →        A     y
    ///   / \                     / \
    ///  A   B                   B   C
    /// </summary>
    private AvlNode<T> RotateRight(AvlNode<T> y)
    {
        var x = y.Left!;
        var B = x.Right;

        Console.WriteLine($"         RotateRight around {y.Value}: {x.Value} rises, {y.Value} drops right.");

        x.Right = y;
        y.Left  = B;

        UpdateHeight(y);
        UpdateHeight(x);

        return x; // new root of this subtree
    }

    /// <summary>
    /// Left rotation around <paramref name="x"/> — used when right-heavy (RR case).
    ///
    ///    x                    y
    ///   / \                 /   \
    ///  A   y    →         x     C
    ///     / \            / \
    ///    B   C          A   B
    /// </summary>
    private AvlNode<T> RotateLeft(AvlNode<T> x)
    {
        var y = x.Right!;
        var B = y.Left;

        Console.WriteLine($"         RotateLeft around {x.Value}: {y.Value} rises, {x.Value} drops left.");

        y.Left  = x;
        x.Right = B;

        UpdateHeight(x);
        UpdateHeight(y);

        return y; // new root of this subtree
    }

    /// <summary>
    /// Checks balance factor of <paramref name="node"/> and applies the correct
    /// rotation(s) if the node is unbalanced. Returns the (possibly new) subtree root.
    /// </summary>
    private AvlNode<T> Rebalance(AvlNode<T> node)
    {
        UpdateHeight(node);
        int bf = BF(node);

        Console.WriteLine($"         Balance check at {node.Value}: BF={bf:+#;-#;0}, " +
                          $"L-height={H(node.Left)}, R-height={H(node.Right)}");

        if (bf > 1)
        {
            // Left-heavy
            if (BF(node.Left) < 0)
            {
                // LR case: left child is right-heavy → double rotation
                Console.WriteLine($"         → LR case: RotateLeft on left child first.");
                node.Left = RotateLeft(node.Left!);
            }
            Console.WriteLine($"         → LL case: RotateRight on {node.Value}.");
            return RotateRight(node);
        }

        if (bf < -1)
        {
            // Right-heavy
            if (BF(node.Right) > 0)
            {
                // RL case: right child is left-heavy → double rotation
                Console.WriteLine($"         → RL case: RotateRight on right child first.");
                node.Right = RotateRight(node.Right!);
            }
            Console.WriteLine($"         → RR case: RotateLeft on {node.Value}.");
            return RotateLeft(node);
        }

        Console.WriteLine($"         → Balanced. No rotation needed.");
        return node;
    }

    // ── Insert ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a value, then rebalances on the way back up the recursion stack.
    /// O(log n) guaranteed — the tree height is always ≤ 1.44 log₂(n).
    /// </summary>
    public void Insert(T value)
    {
        BigO.Print("O(log n) guaranteed",
            "Height is always ≤ 1.44 log₂(n) — the AVL invariant is maintained after every insert. " +
            "At most O(log n) rotations are performed on the way back up the recursion.");

        Console.WriteLine($"\n[Insert] '{value}'");
        _root = InsertRecursive(_root, value);
        PrintTree("After Insert");
    }

    private AvlNode<T> InsertRecursive(AvlNode<T>? node, T value)
    {
        // Standard BST insert
        if (node is null)
        {
            Count++;
            Console.WriteLine($"         Placed '{value}' as a new leaf.");
            return new AvlNode<T>(value);
        }

        int cmp = value.CompareTo(node.Value);
        if (cmp < 0)
        {
            Console.WriteLine($"         '{value}' < '{node.Value}' → go LEFT");
            node.Left  = InsertRecursive(node.Left, value);
        }
        else if (cmp > 0)
        {
            Console.WriteLine($"         '{value}' > '{node.Value}' → go RIGHT");
            node.Right = InsertRecursive(node.Right, value);
        }
        else
        {
            Console.WriteLine($"         '{value}' already exists — no duplicates.");
            return node;
        }

        // Rebalance on the way back up
        return Rebalance(node);
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes a value and rebalances on the way back up.
    /// O(log n) guaranteed.
    /// </summary>
    public bool Remove(T value)
    {
        BigO.Print("O(log n) guaranteed",
            "Same as BST remove (3 cases) plus at most O(log n) rebalance steps walking back to the root. " +
            "AVL guarantees the height stays bounded so every step is O(log n).");

        Console.WriteLine($"\n[Remove] '{value}'");
        bool found = false;
        _root = RemoveRecursive(_root, value, ref found);
        if (!found) Console.WriteLine($"         '{value}' not found.");
        else { Console.WriteLine($"         Done. Count={Count}."); PrintTree("After Remove"); }
        return found;
    }

    private AvlNode<T>? RemoveRecursive(AvlNode<T>? node, T value, ref bool found)
    {
        if (node is null) return null;

        int cmp = value.CompareTo(node.Value);
        if (cmp < 0)
        {
            Console.WriteLine($"         '{value}' < '{node.Value}' → search LEFT");
            node.Left  = RemoveRecursive(node.Left, value, ref found);
        }
        else if (cmp > 0)
        {
            Console.WriteLine($"         '{value}' > '{node.Value}' → search RIGHT");
            node.Right = RemoveRecursive(node.Right, value, ref found);
        }
        else
        {
            found = true;
            Console.WriteLine($"         Found {node}!");

            if (node.Left is null)  { Count--; return node.Right; }
            if (node.Right is null) { Count--; return node.Left;  }

            // Two children: replace with in-order successor
            var successor = MinNode(node.Right);
            Console.WriteLine($"         Two children — in-order successor is {successor.Value}.");
            node.Value = successor.Value;
            node.Right = RemoveRecursive(node.Right, successor.Value, ref found);
            found = true; // RemoveRecursive decrements count; restore the flag
        }

        return Rebalance(node);
    }

    // ── Contains ─────────────────────────────────────────────────────────────

    /// <summary>Searches the tree — O(log n) guaranteed.</summary>
    public bool Contains(T value)
    {
        BigO.Print("O(log n) guaranteed",
            "BST property guides the search; AVL balance guarantee means the height is always logarithmic.");

        Console.WriteLine($"\n[Contains] '{value}'");
        var node = _root;
        while (node is not null)
        {
            int cmp = value.CompareTo(node.Value);
            if      (cmp == 0) { Console.WriteLine($"           {node} → MATCH ✓"); return true; }
            else if (cmp <  0) { Console.WriteLine($"           {node} → go LEFT");  node = node.Left;  }
            else               { Console.WriteLine($"           {node} → go RIGHT"); node = node.Right; }
        }
        Console.WriteLine($"           '{value}' not found.");
        return false;
    }

    // ── Traversals ────────────────────────────────────────────────────────────

    public void InOrder()
    {
        BigO.Print("O(n)", "Every node visited once. InOrder on a BST always yields sorted output.");
        Console.Write("\n[InOrder] ");
        InOrderRec(_root);
        Console.WriteLine();
    }

    private void InOrderRec(AvlNode<T>? n)
    {
        if (n is null) return;
        InOrderRec(n.Left);
        Console.Write($"{n.Value}  ");
        InOrderRec(n.Right);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    public int Height()
    {
        BigO.Print("O(1)",
            "Each AVL node stores its subtree height — updated in O(1) by UpdateHeight() after every " +
            "rotation. Reading the height is simply _root.Height, not a tree traversal. " +
            "(A plain BST without stored heights would need O(n) to compute this.)");
        int h = H(_root);
        Console.WriteLine($"\n[Height] = {h}  (a balanced tree of {Count} nodes ideally has height ≈ {(int)Math.Ceiling(Math.Log2(Count + 1)) - 1})");
        return h;
    }

    public void Clear()
    {
        BigO.Print("O(1)", "Drop root reference — GC collects the entire tree.");
        Console.WriteLine($"\n[Clear] Releasing root. GC collects all {Count} node(s).");
        _root = null; Count = 0;
        Console.WriteLine("[Clear] Done.");
    }

    private static AvlNode<T> MinNode(AvlNode<T> n)
    {
        while (n.Left is not null) n = n.Left;
        return n;
    }

    // ── Visual printer ────────────────────────────────────────────────────────

    /// <summary>Prints the tree rotated 90° CCW with balance factors at each node.</summary>
    public void PrintTree(string label = "AVL Tree")
    {
        Console.WriteLine($"\n  ── {label} (Count={Count}) ──");
        if (_root is null) { Console.WriteLine("  (empty)"); return; }
        PrintRec(_root, "", isRight: true);
        Console.WriteLine();
    }

    private static void PrintRec(AvlNode<T>? node, string indent, bool isRight)
    {
        if (node is null) return;
        PrintRec(node.Right, indent + (isRight ? "        " : "  │     "), isRight: true);
        string bf   = BF(node) switch { > 0 => $"+{BF(node)}", var x => $"{x}" };
        Console.WriteLine($"{indent}{(isRight ? "  ┌─── " : "  └─── ")}{node.Value}  [BF={bf}]");
        PrintRec(node.Left, indent + (isRight ? "  │     " : "        "), isRight: false);
    }
}
