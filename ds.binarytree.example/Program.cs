using ds.binarytree.example;

Banner("Binary Search Tree — Data Structure Demo");
Console.WriteLine("""
  A Binary Search Tree stores values so that for every node:
    • LEFT  subtree  →  all values LESS    THAN the node
    • RIGHT subtree  →  all values GREATER THAN the node

  This ordering property lets search, insert, and delete skip half
  the remaining tree at every step — O(log n) average.

  We will insert: 50, 30, 70, 20, 40, 60, 80
  which produces a balanced tree:

              (50)
             /    \
          (30)    (70)
          /  \    /  \
        (20)(40)(60)(80)
""");

// ────────────────────────────────────────────────────────────────────────────
Section("1. Insert — watch the BST property guide each placement");
// ────────────────────────────────────────────────────────────────────────────
var bst = new BinarySearchTree<int>();
foreach (var v in new[] { 50, 30, 70, 20, 40, 60, 80 })
    bst.Insert(v);

// ────────────────────────────────────────────────────────────────────────────
Section("2. Contains — O(log n) guided search");
// ────────────────────────────────────────────────────────────────────────────
bst.Contains(40);   // present — in left subtree
bst.Contains(99);   // absent

// ────────────────────────────────────────────────────────────────────────────
Section("3. Min and Max — walk left / walk right");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine($"  Min = {bst.Min()}");
Console.WriteLine($"  Max = {bst.Max()}");

// ────────────────────────────────────────────────────────────────────────────
Section("4. Height");
// ────────────────────────────────────────────────────────────────────────────
bst.Height();

// ────────────────────────────────────────────────────────────────────────────
Section("5. Traversals — four ways to walk the tree");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  InOrder   (Left → Node → Right) : sorted output — the BST's defining property
  PreOrder  (Node → Left → Right) : root first — good for copying/serialising
  PostOrder (Left → Right → Node) : children before parent — good for deletion
  LevelOrder (BFS, row by row)    : breadth-first — good for shortest path
""");
bst.InOrder();
bst.PreOrder();
bst.PostOrder();
bst.LevelOrder();

// ────────────────────────────────────────────────────────────────────────────
Section("6. Remove — three cases");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  Case 1: Leaf node       — simply unlink
  Case 2: One child       — replace node with its child
  Case 3: Two children    — replace value with in-order successor,
                            then delete the successor from the right subtree
""");

Console.WriteLine("  Removing 20 (Case 1 — leaf):");
bst.Remove(20);

Console.WriteLine("\n  Removing 30 (Case 2 — one child, only right child 40 remains):");
bst.Remove(30);

Console.WriteLine("\n  Removing 50 (Case 3 — two children; in-order successor is 60):");
bst.Remove(50);

Console.WriteLine("\n  Removing 99 (not present):");
bst.Remove(99);

// ────────────────────────────────────────────────────────────────────────────
Section("7. Degenerate tree — the sorted-insert worst case");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  Inserting values in sorted order (10, 20, 30, 40, 50) makes every new
  value go to the right — the tree degenerates into a linked list.
  Height = n-1 instead of log₂(n). Search becomes O(n), not O(log n).
  This is why self-balancing trees (AVL, Red-Black) exist.
""");
var degenerate = new BinarySearchTree<int>();
foreach (var v in new[] { 10, 20, 30, 40, 50 })
    degenerate.Insert(v);

degenerate.Height();
Console.WriteLine($"\n  Height of a balanced tree with 5 nodes would be {(int)Math.Floor(Math.Log2(5))}.");
Console.WriteLine("  Here it is 4 — same as a linked list of 5 nodes.");

// ────────────────────────────────────────────────────────────────────────────
Section("8. Clear");
// ────────────────────────────────────────────────────────────────────────────
bst.Clear();
Console.WriteLine($"  IsEmpty={bst.IsEmpty}   Count={bst.Count}");

// ────────────────────────────────────────────────────────────────────────────
Banner("Big O Summary");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  ┌─────────────────┬──────────────┬───────────┬─────────────────────────────────────────────┐
  │ Operation       │   Average    │   Worst   │ Why                                         │
  ├─────────────────┼──────────────┼───────────┼─────────────────────────────────────────────┤
  │ Insert          │  O(log n)    │   O(n)    │ Halve candidates each step; degenerate = list│
  │ Contains        │  O(log n)    │   O(n)    │ BST property eliminates a subtree each step  │
  │ Remove          │  O(log n)    │   O(n)    │ Find + pointer fixup; successor also O(log n)│
  │ Min / Max       │  O(log n)    │   O(n)    │ Walk all the way left / right                │
  │ Height          │  O(n)        │   O(n)    │ Every node must be visited                   │
  │ InOrder         │  O(n)        │   O(n)    │ Every node visited — output is always sorted │
  │ PreOrder        │  O(n)        │   O(n)    │ Every node visited — root first              │
  │ PostOrder       │  O(n)        │   O(n)    │ Every node visited — children before parent  │
  │ LevelOrder      │  O(n)        │   O(n)    │ Every node enqueued and dequeued once        │
  │ Clear           │  O(1)        │   O(1)    │ Drop root — GC collects everything           │
  └─────────────────┴──────────────┴───────────┴─────────────────────────────────────────────┘

  Average vs worst case:
    A BALANCED tree of n nodes has height ≈ log₂(n), giving O(log n) operations.
    A DEGENERATE tree (sorted insertions) has height n-1, degrading to O(n).
    Self-balancing trees (AVL, Red-Black) guarantee O(log n) in all cases
    by automatically rotating nodes after insert/delete — that's the next step!
""");

Console.WriteLine("══════════════════════════════════════════════════════════");
Console.WriteLine("  Demo complete.");
Console.WriteLine("══════════════════════════════════════════════════════════");

static void Banner(string title)
{
    var line = new string('═', title.Length + 4);
    Console.WriteLine($"\n╔{line}╗");
    Console.WriteLine($"║  {title}  ║");
    Console.WriteLine($"╚{line}╝\n");
}

static void Section(string title)
{
    Console.WriteLine($"\n\n──────────────────────────────────────────");
    Console.WriteLine($"  {title}");
    Console.WriteLine($"──────────────────────────────────────────");
}
