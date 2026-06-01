using ds.shared;

namespace ds.redblacktree.example;

// ── Color ─────────────────────────────────────────────────────────────────────

public enum RbColor { Red, Black }

// ── Node ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Red-Black node — same as BST but adds Color and a Parent pointer.
/// Parent pointer is needed for the fixup algorithms (uncle lookup, etc.).
/// </summary>
public class RbNode<T>
{
    public T         Value;
    public RbColor   Color;
    public RbNode<T> Left   = null!;
    public RbNode<T> Right  = null!;
    public RbNode<T> Parent = null!;

    public RbNode(T value, RbColor color) { Value = value; Color = color; }

    public override string ToString() =>
        $"({Value}:{(Color == RbColor.Red ? "R" : "B")})";
}

// ── Red-Black Tree ─────────────────────────────────────────────────────────────

/// <summary>
/// Red-Black Tree — a self-balancing BST using node colours to enforce a height bound.
///
/// 5 RED-BLACK RULES (must hold at all times):
///   1. Every node is Red or Black.
///   2. The root is Black.
///   3. Every null leaf (represented here by the sentinel NIL) is Black.
///   4. If a node is Red, both its children are Black (no two consecutive reds).
///   5. All simple paths from any node to a descendant NIL have the same
///      number of Black nodes (the "black-height").
///
/// These rules together guarantee:  height ≤ 2 log₂(n + 1)
///
/// WHY A SENTINEL NIL NODE?
///   Instead of nullable pointers we use one shared Black NIL node.
///   Every leaf's Left and Right point to NIL; the root's parent is NIL.
///   This removes countless null-checks inside rotations and fixups.
///
/// INSERT FIXUP — 3 cases (mirrored for left/right):
///   Case 1: Uncle is Red      → recolour parent + uncle Black, grandparent Red; move up
///   Case 2: Uncle is Black, node is inside (zigzag) → rotate to make it outside
///   Case 3: Uncle is Black, node is outside (straight) → rotate + recolour
///
/// DELETE FIXUP — 4 cases (mirrored for left/right):
///   Case 1: Sibling is Red   → rotate to make sibling Black; fall through
///   Case 2: Sibling is Black, both nephews Black → recolour sibling; move problem up
///   Case 3: Sibling is Black, near nephew Red, far nephew Black → rotate sibling; fall through
///   Case 4: Sibling is Black, far nephew Red → rotate + recolour; done
/// </summary>
public class RedBlackTree<T> where T : IComparable<T>
{
    // Sentinel NIL — a shared Black node that acts as every null leaf.
    private readonly RbNode<T> NIL;
    private RbNode<T> _root;

    public int  Count   { get; private set; }
    public bool IsEmpty => _root == NIL;

    public RedBlackTree()
    {
        NIL   = new RbNode<T>(default!, RbColor.Black);
        NIL.Left = NIL.Right = NIL.Parent = NIL;
        _root = NIL;
        Console.WriteLine("[RedBlackTree] Created. NIL sentinel (Black) shared by all null leaves.");
    }

    // ── Rotations ─────────────────────────────────────────────────────────────

    private void RotateLeft(RbNode<T> x)
    {
        var y = x.Right;
        Console.WriteLine($"         RotateLeft({x.Value}): {y.Value} rises, {x.Value} drops left.");

        x.Right = y.Left;
        if (y.Left != NIL) y.Left.Parent = x;

        y.Parent = x.Parent;
        if      (x.Parent == NIL)          _root        = y;
        else if (x == x.Parent.Left)  x.Parent.Left  = y;
        else                           x.Parent.Right = y;

        y.Left   = x;
        x.Parent = y;
    }

    private void RotateRight(RbNode<T> y)
    {
        var x = y.Left;
        Console.WriteLine($"         RotateRight({y.Value}): {x.Value} rises, {y.Value} drops right.");

        y.Left = x.Right;
        if (x.Right != NIL) x.Right.Parent = y;

        x.Parent = y.Parent;
        if      (y.Parent == NIL)          _root        = x;
        else if (y == y.Parent.Right) y.Parent.Right = x;
        else                           y.Parent.Left  = x;

        x.Right  = y;
        y.Parent = x;
    }

    // ── Insert ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new Red node using standard BST insert, then calls InsertFixup
    /// to restore the Red-Black rules. O(log n) guaranteed.
    /// </summary>
    public void Insert(T value)
    {
        BigO.Print("O(log n) guaranteed",
            "Height ≤ 2 log₂(n+1) always. Insert adds a Red node, then fixup does at most " +
            "O(log n) recolourings and at most 2 rotations to restore all 5 rules.");

        Console.WriteLine($"\n[Insert] '{value}' — new node starts RED (rule: always insert Red).");

        var z = new RbNode<T>(value, RbColor.Red) { Left = NIL, Right = NIL, Parent = NIL };

        // Standard BST insert
        var y = NIL;
        var x = _root;
        while (x != NIL)
        {
            y = x;
            int cmp = value.CompareTo(x.Value);
            if      (cmp == 0) { Console.WriteLine($"         '{value}' already exists — duplicates not allowed."); return; }
            else if (cmp  < 0) { Console.WriteLine($"         '{value}' < '{x.Value}' → go LEFT");  x = x.Left;  }
            else               { Console.WriteLine($"         '{value}' > '{x.Value}' → go RIGHT"); x = x.Right; }
        }

        z.Parent = y;
        if      (y == NIL)              _root    = z;
        else if (value.CompareTo(y.Value) < 0) y.Left  = z;
        else                            y.Right = z;

        Count++;
        Console.WriteLine($"         Placed {z} as leaf.");

        InsertFixup(z);
        PrintTree("After Insert");
    }

    private void InsertFixup(RbNode<T> z)
    {
        Console.WriteLine($"         InsertFixup starting at {z}...");
        while (z.Parent.Color == RbColor.Red)
        {
            Console.WriteLine($"           z={z}, parent={z.Parent} is Red — violation! Checking uncle...");

            if (z.Parent == z.Parent.Parent.Left)
            {
                var uncle = z.Parent.Parent.Right;
                Console.WriteLine($"           Parent is LEFT child. Uncle = {uncle}");

                if (uncle.Color == RbColor.Red)
                {
                    // Case 1: uncle is Red → recolour, move z up to grandparent
                    Console.WriteLine($"           Case 1: Uncle is Red → recolour parent+uncle Black, grandparent Red. Move up.");
                    z.Parent.Color         = RbColor.Black;
                    uncle.Color            = RbColor.Black;
                    z.Parent.Parent.Color  = RbColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Right)
                    {
                        // Case 2: uncle Black, z is inside (right child) → rotate left to convert to Case 3
                        Console.WriteLine($"           Case 2: Uncle Black, z is inside (right child) → RotateLeft(parent) → becomes Case 3.");
                        z = z.Parent;
                        RotateLeft(z);
                    }
                    // Case 3: uncle Black, z is outside (left child) → rotate + recolour
                    Console.WriteLine($"           Case 3: Uncle Black, z is outside → recolour + RotateRight(grandparent).");
                    z.Parent.Color        = RbColor.Black;
                    z.Parent.Parent.Color = RbColor.Red;
                    RotateRight(z.Parent.Parent);
                }
            }
            else
            {
                // Mirror: parent is RIGHT child
                var uncle = z.Parent.Parent.Left;
                Console.WriteLine($"           Parent is RIGHT child. Uncle = {uncle}");

                if (uncle.Color == RbColor.Red)
                {
                    Console.WriteLine($"           Case 1 (mirror): Uncle is Red → recolour parent+uncle Black, grandparent Red. Move up.");
                    z.Parent.Color        = RbColor.Black;
                    uncle.Color           = RbColor.Black;
                    z.Parent.Parent.Color = RbColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Left)
                    {
                        Console.WriteLine($"           Case 2 (mirror): Uncle Black, z is inside (left child) → RotateRight(parent).");
                        z = z.Parent;
                        RotateRight(z);
                    }
                    Console.WriteLine($"           Case 3 (mirror): Uncle Black, z is outside → recolour + RotateLeft(grandparent).");
                    z.Parent.Color        = RbColor.Black;
                    z.Parent.Parent.Color = RbColor.Red;
                    RotateLeft(z.Parent.Parent);
                }
            }
        }
        // Rule 2: root is always Black
        if (_root.Color == RbColor.Red)
        {
            Console.WriteLine($"           Root {_root} was Red → forced Black (Rule 2).");
            _root.Color = RbColor.Black;
        }
        Console.WriteLine($"         InsertFixup complete.");
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes a node using transplant + the standard 3-case BST deletion,
    /// then calls DeleteFixup if a Black node was removed (which can violate Rule 5).
    /// O(log n) guaranteed.
    /// </summary>
    public bool Remove(T value)
    {
        BigO.Print("O(log n) guaranteed",
            "Locating the node is O(log n). Fixup does at most O(log n) recolourings " +
            "and at most 3 rotations. The height bound ensures every step is logarithmic.");

        Console.WriteLine($"\n[Remove] '{value}'");
        var z = FindNode(_root, value);
        if (z == NIL) { Console.WriteLine($"         '{value}' not found."); return false; }

        Console.WriteLine($"         Found {z}. Removing...");
        Delete(z);
        Count--;
        Console.WriteLine($"         Done. Count={Count}.");
        PrintTree("After Remove");
        return true;
    }

    private void Delete(RbNode<T> z)
    {
        var y         = z;
        var yOrigColor = y.Color;
        RbNode<T> x;

        if (z.Left == NIL)
        {
            Console.WriteLine($"         Case: no left child → transplant right child.");
            x = z.Right;
            Transplant(z, z.Right);
        }
        else if (z.Right == NIL)
        {
            Console.WriteLine($"         Case: no right child → transplant left child.");
            x = z.Left;
            Transplant(z, z.Left);
        }
        else
        {
            // Two children: find in-order successor (minimum of right subtree)
            y = TreeMinimum(z.Right);
            Console.WriteLine($"         Case: two children. In-order successor = {y}.");
            yOrigColor = y.Color;
            x          = y.Right;

            if (y.Parent == z)
            {
                x.Parent = y;
            }
            else
            {
                Transplant(y, y.Right);
                y.Right        = z.Right;
                y.Right.Parent = y;
            }
            Transplant(z, y);
            y.Left        = z.Left;
            y.Left.Parent = y;
            y.Color       = z.Color;
            Console.WriteLine($"         Successor {y} moved into place with colour {z.Color}.");
        }

        if (yOrigColor == RbColor.Black)
        {
            Console.WriteLine($"         Removed node was Black — Rule 5 may be violated. Running DeleteFixup...");
            DeleteFixup(x);
        }
        else
        {
            Console.WriteLine($"         Removed node was Red — no fixup needed (Black-height unchanged).");
        }
    }

    private void DeleteFixup(RbNode<T> x)
    {
        while (x != _root && x.Color == RbColor.Black)
        {
            Console.WriteLine($"           DeleteFixup: x={x}, checking sibling...");
            if (x == x.Parent.Left)
            {
                var w = x.Parent.Right; // sibling
                Console.WriteLine($"           x is LEFT child. Sibling w={w}.");

                if (w.Color == RbColor.Red)
                {
                    // Case 1: sibling Red → rotate to make sibling Black
                    Console.WriteLine($"           Case 1: Sibling Red → recolour + RotateLeft(parent). Now sibling is Black.");
                    w.Color        = RbColor.Black;
                    x.Parent.Color = RbColor.Red;
                    RotateLeft(x.Parent);
                    w = x.Parent.Right;
                }
                if (w.Left.Color == RbColor.Black && w.Right.Color == RbColor.Black)
                {
                    // Case 2: sibling Black, both nephews Black → recolour sibling, move x up
                    Console.WriteLine($"           Case 2: Sibling Black, both nephews Black → recolour sibling Red, move x up.");
                    w.Color = RbColor.Red;
                    x       = x.Parent;
                }
                else
                {
                    if (w.Right.Color == RbColor.Black)
                    {
                        // Case 3: sibling Black, far nephew Black, near nephew Red → rotate sibling
                        Console.WriteLine($"           Case 3: Far nephew Black, near nephew Red → recolour + RotateRight(sibling) → Case 4.");
                        w.Left.Color = RbColor.Black;
                        w.Color      = RbColor.Red;
                        RotateRight(w);
                        w = x.Parent.Right;
                    }
                    // Case 4: sibling Black, far nephew Red → rotate parent, done
                    Console.WriteLine($"           Case 4: Far nephew Red → recolour + RotateLeft(parent). Fixed.");
                    w.Color        = x.Parent.Color;
                    x.Parent.Color = RbColor.Black;
                    w.Right.Color  = RbColor.Black;
                    RotateLeft(x.Parent);
                    x = _root; // done
                }
            }
            else
            {
                // Mirror
                var w = x.Parent.Left;
                Console.WriteLine($"           x is RIGHT child. Sibling w={w}.");

                if (w.Color == RbColor.Red)
                {
                    Console.WriteLine($"           Case 1 (mirror): Sibling Red → recolour + RotateRight(parent).");
                    w.Color        = RbColor.Black;
                    x.Parent.Color = RbColor.Red;
                    RotateRight(x.Parent);
                    w = x.Parent.Left;
                }
                if (w.Right.Color == RbColor.Black && w.Left.Color == RbColor.Black)
                {
                    Console.WriteLine($"           Case 2 (mirror): Both nephews Black → recolour sibling Red, move x up.");
                    w.Color = RbColor.Red;
                    x       = x.Parent;
                }
                else
                {
                    if (w.Left.Color == RbColor.Black)
                    {
                        Console.WriteLine($"           Case 3 (mirror): → RotateLeft(sibling).");
                        w.Right.Color = RbColor.Black;
                        w.Color       = RbColor.Red;
                        RotateLeft(w);
                        w = x.Parent.Left;
                    }
                    Console.WriteLine($"           Case 4 (mirror): → recolour + RotateRight(parent). Fixed.");
                    w.Color        = x.Parent.Color;
                    x.Parent.Color = RbColor.Black;
                    w.Left.Color   = RbColor.Black;
                    RotateRight(x.Parent);
                    x = _root;
                }
            }
        }
        x.Color = RbColor.Black;
    }

    private void Transplant(RbNode<T> u, RbNode<T> v)
    {
        if      (u.Parent == NIL)          _root            = v;
        else if (u == u.Parent.Left)  u.Parent.Left  = v;
        else                           u.Parent.Right = v;
        v.Parent = u.Parent;
    }

    private RbNode<T> TreeMinimum(RbNode<T> x)
    {
        while (x.Left != NIL) x = x.Left;
        return x;
    }

    // ── Contains ─────────────────────────────────────────────────────────────

    public bool Contains(T value)
    {
        BigO.Print("O(log n) guaranteed",
            "BST property guides the search. Height ≤ 2 log₂(n+1) ensures worst case is always O(log n).");

        Console.WriteLine($"\n[Contains] '{value}'");
        var node = FindNode(_root, value);
        if (node != NIL)
        {
            Console.WriteLine($"           Found {node} ✓");
            return true;
        }
        Console.WriteLine($"           '{value}' not found.");
        return false;
    }

    private RbNode<T> FindNode(RbNode<T> node, T value)
    {
        while (node != NIL)
        {
            int cmp = value.CompareTo(node.Value);
            if      (cmp == 0) return node;
            else if (cmp  < 0) { Console.WriteLine($"           {node} → go LEFT");  node = node.Left;  }
            else               { Console.WriteLine($"           {node} → go RIGHT"); node = node.Right; }
        }
        return NIL;
    }

    // ── Traversal ─────────────────────────────────────────────────────────────

    public void InOrder()
    {
        BigO.Print("O(n)", "Every node visited once. Produces sorted output (BST property).");
        Console.Write("\n[InOrder] ");
        InOrderRec(_root);
        Console.WriteLine();
    }

    private void InOrderRec(RbNode<T> n)
    {
        if (n == NIL) return;
        InOrderRec(n.Left);
        Console.Write($"{n.Value}({(n.Color == RbColor.Red ? "R" : "B")})  ");
        InOrderRec(n.Right);
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    public void Clear()
    {
        BigO.Print("O(1)", "Drop root reference — GC collects every node. NIL sentinel is reused.");
        Console.WriteLine($"\n[Clear] Releasing root. GC collects all {Count} node(s).");
        _root = NIL; Count = 0;
        Console.WriteLine("[Clear] Done.");
    }

    // ── Visual printer ────────────────────────────────────────────────────────

    /// <summary>Prints tree rotated 90° CCW. R = Red, B = Black.</summary>
    public void PrintTree(string label = "Red-Black Tree")
    {
        Console.WriteLine($"\n  ── {label} (Count={Count}) ──");
        if (_root == NIL) { Console.WriteLine("  (empty)"); return; }
        PrintRec(_root, "", isRight: true);
        Console.WriteLine();
    }

    private void PrintRec(RbNode<T> node, string indent, bool isRight)
    {
        if (node == NIL) return;
        PrintRec(node.Right, indent + (isRight ? "        " : "  │     "), isRight: true);
        string colour = node.Color == RbColor.Red ? "🔴" : "⚫";
        Console.WriteLine($"{indent}{(isRight ? "  ┌─── " : "  └─── ")}{colour} {node.Value}");
        PrintRec(node.Left, indent + (isRight ? "  │     " : "        "), isRight: false);
    }
}
