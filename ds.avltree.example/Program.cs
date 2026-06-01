using ds.avltree.example;

Banner("AVL Tree — Data Structure Demo");
Console.WriteLine("""
  An AVL tree is a self-balancing BST. After every Insert or Remove it
  checks the BALANCE FACTOR (BF) at each node on the path back to the root:

    BF = height(left subtree) - height(right subtree)

    BF ∈ {-1, 0, +1} → balanced    (no action)
    BF = +2           → left-heavy  → RotateRight  (or Left-Right double)
    BF = -2           → right-heavy → RotateLeft   (or Right-Left double)

  4 rotation cases:
    LL  BF=+2, left child  BF≥0 → RotateRight
    RR  BF=-2, right child BF≤0 → RotateLeft
    LR  BF=+2, left child  BF<0 → RotateLeft(child) then RotateRight(root)
    RL  BF=-2, right child BF>0 → RotateRight(child) then RotateLeft(root)

  Tree nodes are shown as:  value  [BF=balance factor]
""");

var avl = new AvlTree<int>();

// ────────────────────────────────────────────────────────────────────────────
Section("1. Insert in sorted order — triggers rotations (BST would degenerate)");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("  Inserting: 10, 20, 30  →  triggers RR (RotateLeft)\n");
avl.Insert(10);
avl.Insert(20);
avl.Insert(30);   // BF at 10 becomes -2 → RotateLeft → 20 becomes root

// ────────────────────────────────────────────────────────────────────────────
Section("2. More inserts — LR and RL double rotations");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  To trigger an LR double rotation we need:
    BF at some node = +2  AND  its left child has BF < 0.
  Inserting 5 then 7:
    5 goes left of 10  → BF(10)=+1 (balanced)
    7 goes right of 5  → BF(5)=-1, BF(10) becomes +2  ← LR case!
    → RotateLeft(5) then RotateRight(10)

  To trigger an RL double rotation we need:
    BF at some node = -2  AND  its right child has BF > 0.
  Inserting 35 then 32:
    35 goes right of 30 → BF(30)=-1 (balanced)
    32 goes left of 35  → BF(35)=+1, BF(30) becomes -2  ← RL case!
    → RotateRight(35) then RotateLeft(30)
""");
avl.Insert(5);
avl.Insert(7);    // LR case at node 10
avl.Insert(35);
avl.Insert(32);   // RL case at node 30

// ────────────────────────────────────────────────────────────────────────────
Section("3. Full balanced tree after 7 inserts");
// ────────────────────────────────────────────────────────────────────────────
avl.PrintTree("State after all inserts");
avl.Height();
avl.InOrder();

// ────────────────────────────────────────────────────────────────────────────
Section("4. Contains — O(log n) guaranteed");
// ────────────────────────────────────────────────────────────────────────────
avl.Contains(15);
avl.Contains(99);

// ────────────────────────────────────────────────────────────────────────────
Section("5. Remove — rebalances on the way back up");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("  Remove 5 (leaf):");
avl.Remove(5);

Console.WriteLine("\n  Remove 20 (root — two children, successor is 25):");
avl.Remove(20);

Console.WriteLine("\n  Remove 99 (not present):");
avl.Remove(99);

// ────────────────────────────────────────────────────────────────────────────
Section("6. AVL vs plain BST — height comparison");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  Inserting 1..7 in sorted order into a plain BST gives height = 6 (a list).
  An AVL tree rebalances on every insert — final height stays at 2.

  Plain BST:        AVL Tree:
    1                   4
     \                /   \
      2              2     6
       \            / \   / \
        3          1   3 5   7
         \
          4
           \
            5
             \
              6
               \
                7
""");
var avlVsBst = new AvlTree<int>();
foreach (var v in new[] { 1, 2, 3, 4, 5, 6, 7 })
    avlVsBst.Insert(v);

avlVsBst.Height();
avlVsBst.PrintTree("AVL after sorted inserts 1..7");

// ────────────────────────────────────────────────────────────────────────────
Banner("Big O Summary — AVL Tree");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  ┌────────────────┬───────────────┬──────────────────────────────────────────┐
  │ Operation      │ Complexity    │ Why                                      │
  ├────────────────┼───────────────┼──────────────────────────────────────────┤
  │ Insert         │ O(log n)      │ Height ≤ 1.44 log₂(n) always; ≤1 rotation│
  │ Remove         │ O(log n)      │ Same bound; O(log n) rebalance steps     │
  │ Contains       │ O(log n)      │ Bounded height eliminates worst case     │
  │ InOrder        │ O(n)          │ Every node visited once                  │
  │ Height         │ O(1)          │ Stored at each node — just read _root.Height│
  └────────────────┴───────────────┴──────────────────────────────────────────┘

  vs plain BST:
    BST worst case is O(n) (sorted input → linked list).
    AVL worst case is O(log n) always — guaranteed by rotations.

  vs Red-Black Tree (next):
    AVL is MORE strictly balanced (BF ≤ 1 vs RB's looser constraint).
    → AVL has faster lookups (shorter tree).
    → Red-Black has fewer rotations on insert/delete (faster writes).
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
