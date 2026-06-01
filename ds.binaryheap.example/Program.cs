using ds.binaryheap.example;

// ════════════════════════════════════════════════════════════════════════════
//  PART 1 — MIN HEAP
// ════════════════════════════════════════════════════════════════════════════
Banner("Binary Heap — Data Structure Demo");
Console.WriteLine("""
  A Binary Heap is a COMPLETE BINARY TREE stored in a FLAT ARRAY.
  No node pointers — parent/child relationships are pure index math:

    Parent    of i  →  (i - 1) / 2
    Left child  of i  →  2 * i + 1
    Right child of i  →  2 * i + 2

  HEAP PROPERTY:
    MinHeap → every parent ≤ its children  (root = global minimum)
    MaxHeap → every parent ≥ its children  (root = global maximum)

  Siblings have NO ordering — this is NOT a sorted structure.
  The only guarantee is parent-vs-children.

  KEY ADVANTAGE over BST:
    Peek (see min/max)  →  O(1)  — always at index 0
    Insert / Extract    →  O(log n)
    BST's Peek would be O(log n) — you must walk to the leftmost node.
""");

// ────────────────────────────────────────────────────────────────────────────
Section("PART 1 — MinHeap");
// ────────────────────────────────────────────────────────────────────────────
var minHeap = new DsHeap<int>(HeapType.Min);

Section("1a. Insert — place at end then bubble UP");
foreach (var v in new[] { 50, 30, 70, 10, 40, 20, 60 })
    minHeap.Insert(v);

// ────────────────────────────────────────────────────────────────────────────
Section("1b. Peek — O(1) root read");
// ────────────────────────────────────────────────────────────────────────────
var top = minHeap.Peek();
Console.WriteLine($"  Peek returned: {top}  (always the minimum)");

// ────────────────────────────────────────────────────────────────────────────
Section("1c. ExtractTop — remove root, move last to root, bubble DOWN");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine($"  Extracted: {minHeap.ExtractTop()}");
Console.WriteLine($"  Extracted: {minHeap.ExtractTop()}");

// ────────────────────────────────────────────────────────────────────────────
Section("1d. Contains — O(n) full scan (siblings are unordered)");
// ────────────────────────────────────────────────────────────────────────────
minHeap.Contains(40);
minHeap.Contains(99);

// ────────────────────────────────────────────────────────────────────────────
Section("1e. DrainSorted — extract all → ascending order (heap sort)");
// ────────────────────────────────────────────────────────────────────────────
var minSorted = minHeap.DrainSorted();
Console.WriteLine($"  Result: [ {string.Join(", ", minSorted)} ]  ← ascending ✓");

// ════════════════════════════════════════════════════════════════════════════
//  PART 2 — MAX HEAP
// ════════════════════════════════════════════════════════════════════════════
Section("PART 2 — MaxHeap (same structure, comparison flipped)");
var maxHeap = new DsHeap<int>(HeapType.Max);

Section("2a. Insert");
foreach (var v in new[] { 50, 30, 70, 10, 40, 20, 60 })
    maxHeap.Insert(v);

Section("2b. Peek");
Console.WriteLine($"  Peek returned: {maxHeap.Peek()}  (always the maximum)");

Section("2c. ExtractTop");
Console.WriteLine($"  Extracted: {maxHeap.ExtractTop()}");
Console.WriteLine($"  Extracted: {maxHeap.ExtractTop()}");

Section("2d. DrainSorted — extract all → descending order");
var maxSorted = maxHeap.DrainSorted();
Console.WriteLine($"  Result: [ {string.Join(", ", maxSorted)} ]  ← descending ✓");

// ════════════════════════════════════════════════════════════════════════════
//  PART 3 — REAL-WORLD: PRIORITY QUEUE
// ════════════════════════════════════════════════════════════════════════════
Section("PART 3 — Real-world: task priority queue");
Console.WriteLine("""
  A MinHeap is a natural priority queue — the task with the
  LOWEST priority number (= highest urgency) is always served first.
""");

var pq = new DsHeap<int>(HeapType.Min);
Console.WriteLine("  Adding tasks with priority: 5 (low), 1 (critical), 3 (medium), 2 (high)");
pq.Insert(5);
pq.Insert(1);
pq.Insert(3);
pq.Insert(2);

Console.WriteLine("\n  Processing tasks in priority order:");
while (!pq.IsEmpty)
{
    int priority = pq.ExtractTop();
    string label = priority switch { 1 => "CRITICAL", 2 => "HIGH", 3 => "MEDIUM", _ => "LOW" };
    Console.WriteLine($"    → Priority {priority} ({label})");
}

// ════════════════════════════════════════════════════════════════════════════
//  PART 4 — BST vs HEAP comparison
// ════════════════════════════════════════════════════════════════════════════
Banner("BST vs Heap — When to use which");
Console.WriteLine("""
  ┌──────────────────────┬─────────────────┬──────────────────────────────────┐
  │ Need                 │ Use             │ Why                              │
  ├──────────────────────┼─────────────────┼──────────────────────────────────┤
  │ Get min or max fast  │ Heap            │ O(1) peek at root                │
  │ Remove min or max    │ Heap            │ O(log n) ExtractTop              │
  │ Search for a value   │ BST             │ O(log n) guided; Heap is O(n)    │
  │ In-order iteration   │ BST             │ InOrder traversal = sorted       │
  │ Priority queue       │ MinHeap         │ Always serves lowest priority    │
  │ Heap sort            │ MaxHeap         │ Drain gives descending order     │
  │ Cache-friendly store │ Heap            │ Flat array — no pointer chasing  │
  └──────────────────────┴─────────────────┴──────────────────────────────────┘
""");

// ════════════════════════════════════════════════════════════════════════════
//  Big O Summary
// ════════════════════════════════════════════════════════════════════════════
Banner("Big O Summary");
Console.WriteLine("""
  ┌───────────────┬───────────┬──────────────────────────────────────────────┐
  │ Operation     │ Time      │ Why                                          │
  ├───────────────┼───────────┼──────────────────────────────────────────────┤
  │ Insert        │ O(log n)  │ Place at end O(1), bubble up ≤ log₂(n) swaps│
  │ ExtractTop    │ O(log n)  │ Move last to root O(1), bubble down log₂(n) │
  │ Peek          │ O(1)      │ Root is always at index 0                    │
  │ Contains      │ O(n)      │ Siblings unordered — must scan everything    │
  │ Clear         │ O(n)      │ Null array slots to release references       │
  │ DrainSorted   │ O(n log n)│ n × ExtractTop, each O(log n)               │
  │ Build from n  │ O(n)      │ Floyd's heapify (not shown) — bottom-up      │
  └───────────────┴───────────┴──────────────────────────────────────────────┘

  Space: O(n) — one flat array, no per-node pointer overhead.
  Compare BST: O(n) space but 2 pointers per node (left + right) vs zero here.
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
