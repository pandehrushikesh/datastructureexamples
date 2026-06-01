using ds.redblacktree.example;

Banner("Red-Black Tree — Data Structure Demo");
Console.WriteLine("""
  A Red-Black tree is a self-balancing BST where every node is coloured
  Red (🔴) or Black (⚫). Five rules maintain a height bound of 2 log₂(n+1):

    Rule 1: Every node is Red or Black.
    Rule 2: The ROOT is always Black.
    Rule 3: Every null leaf (NIL sentinel) is Black.
    Rule 4: A Red node's children must both be Black (no consecutive reds).
    Rule 5: Every path from a node to a descendant NIL has the same
            number of Black nodes (the "black-height").

  INSERT FIXUP — 3 cases (+ their left/right mirrors):
    Case 1: Uncle 🔴  → recolour parent+uncle ⚫, grandparent 🔴, move up
    Case 2: Uncle ⚫, node inside (zigzag) → rotate to become Case 3
    Case 3: Uncle ⚫, node outside (straight) → rotate + recolour, done

  DELETE FIXUP — 4 cases (+ mirrors):
    Case 1: Sibling 🔴 → rotate to make sibling ⚫, continue
    Case 2: Sibling ⚫, both nephews ⚫ → recolour sibling 🔴, move up
    Case 3: Sibling ⚫, far nephew ⚫ → rotate sibling, continue
    Case 4: Sibling ⚫, far nephew 🔴 → rotate + recolour, done

  WHY A NIL SENTINEL?
    A shared Black NIL node replaces all null pointers. Every leaf's children
    and the root's parent point to NIL. This eliminates null-checks inside
    every rotation and fixup branch — the same code handles leaf and non-leaf.
""");

var rbt = new RedBlackTree<int>();

// ────────────────────────────────────────────────────────────────────────────
Section("1. Insert in sorted order — triggers fixup cases");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("  Inserting: 10, 20, 30 → triggers recolour + rotation\n");
rbt.Insert(10);
rbt.Insert(20);
rbt.Insert(30);   // Case 3: uncle Black, node outside → RotateLeft + recolour

// ────────────────────────────────────────────────────────────────────────────
Section("2. More inserts — Case 1 (recolour) and Case 2/3");
// ────────────────────────────────────────────────────────────────────────────
rbt.Insert(15);
rbt.Insert(25);
rbt.Insert(5);
rbt.Insert(1);

rbt.PrintTree("State after 7 inserts");
rbt.InOrder();

// ────────────────────────────────────────────────────────────────────────────
Section("3. Contains — O(log n) guided search");
// ────────────────────────────────────────────────────────────────────────────
rbt.Contains(15);
rbt.Contains(99);

// ────────────────────────────────────────────────────────────────────────────
Section("4. Remove — triggers DeleteFixup");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("  Remove 1 (Red leaf — no fixup needed):");
rbt.Remove(1);

Console.WriteLine("\n  Remove 5 (may require fixup):");
rbt.Remove(5);

Console.WriteLine("\n  Remove 20 (root area — in-order successor + fixup):");
rbt.Remove(20);

Console.WriteLine("\n  Remove 99 (not present):");
rbt.Remove(99);

// ────────────────────────────────────────────────────────────────────────────
Section("5. Sorted insert stress test — stays balanced");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("  Inserting 1..10 in sorted order (would make a plain BST height=9):\n");
var rbt2 = new RedBlackTree<int>();
foreach (var v in Enumerable.Range(1, 10))
    rbt2.Insert(v);

rbt2.PrintTree("RBT after sorted inserts 1..10");
rbt2.InOrder();

// ────────────────────────────────────────────────────────────────────────────
Section("6. Clear");
// ────────────────────────────────────────────────────────────────────────────
rbt.Clear();
Console.WriteLine($"  IsEmpty={rbt.IsEmpty}   Count={rbt.Count}");

// ────────────────────────────────────────────────────────────────────────────
Banner("AVL vs Red-Black — Head to Head");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  ┌──────────────────────┬───────────────────────┬───────────────────────────┐
  │                      │ AVL Tree              │ Red-Black Tree            │
  ├──────────────────────┼───────────────────────┼───────────────────────────┤
  │ Balance guarantee    │ Strict: BF ∈ {-1,0,1} │ Looser: h ≤ 2 log₂(n+1) │
  │ Height               │ ≤ 1.44 log₂(n)        │ ≤ 2 log₂(n+1)            │
  │ Insert rotations     │ At most 2              │ At most 2                 │
  │ Delete rotations     │ O(log n)               │ At most 3                 │
  │ Lookup speed         │ Faster (shorter tree)  │ Slightly slower           │
  │ Write speed          │ Slightly slower        │ Faster (fewer rotations)  │
  │ Used in              │ Databases, filesystems │ Linux kernel, Java TreeMap│
  │                      │ (read-heavy workloads) │ (write-heavy workloads)   │
  └──────────────────────┴───────────────────────┴───────────────────────────┘

  Rule of thumb:
    Read-heavy  → prefer AVL   (shorter height = fewer comparisons per lookup)
    Write-heavy → prefer RBT   (bounded rotations = cheaper insert/delete)
""");

Banner("Big O Summary — Red-Black Tree");
Console.WriteLine("""
  ┌────────────────┬─────────────┬────────────────────────────────────────────┐
  │ Operation      │ Complexity  │ Why                                        │
  ├────────────────┼─────────────┼────────────────────────────────────────────┤
  │ Insert         │ O(log n)    │ h ≤ 2 log₂(n+1); ≤ 2 rotations; O(log n) recolours│
  │ Remove         │ O(log n)    │ Same height bound; ≤ 3 rotations total     │
  │ Contains       │ O(log n)    │ Height bound guarantees worst case         │
  │ InOrder        │ O(n)        │ Every node visited once                    │
  │ Clear          │ O(1)        │ Drop root; NIL sentinel stays alive        │
  └────────────────┴─────────────┴────────────────────────────────────────────┘
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
