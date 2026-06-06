# Data Structure Examples

A collection of hand-built data structure demos in **C# / .NET 10**.
Every operation prints each step to the console so you can follow exactly what the code is doing.

## Projects

| Project | Data Structure | Key concepts |
|---|---|---|
| `ds.array.example` | Dynamic Array | Resize, bubble/selection/merge sort |
| `ds.linkedlist.example` | Linked List | Insert, remove, traverse |
| `ds.stack.queue.example` | Stack & Queue | LIFO stack, FIFO queue |
| `ds.hashtable.example` | Hash Table | Separate chaining, load factor, rehash |
| `ds.binarytree.example` | Binary Search Tree | Insert, remove (3 cases), 4 traversals, height |
| `ds.binaryheap.example` | Binary Heap | MinHeap / MaxHeap, bubble-up/down, heap sort |
| `ds.avltree.example` | AVL Tree | LL / RR / LR / RL rotations with balance-factor traces |
| `ds.redblacktree.example` | Red-Black Tree | Color-based balancing, insert/delete fixup cases |
| `ds.graph.example` | Graph | BFS, DFS, cycle detection, topological sort, Dijkstra |

## Running a demo

```bash
dotnet run --project ds.array.example
dotnet run --project ds.graph.example
# etc.
```

## Windows launcher (`ds.launcher.windows`)

A WinForms GUI that lists all demos as clickable cards.
Clicking a card opens the chosen demo in a new console window.

```bash
dotnet run --project ds.launcher.windows
```

> **Non-Windows users:** `ds.launcher.windows` targets `net10.0-windows` and uses WinForms,
> so it will not build on Linux or macOS. All other projects target cross-platform `net10.0`
> and build normally on any OS.
>
> If you get a build error for this project, remove it from `datastructureexamples.slnx`
> or simply run each demo directly with `dotnet run --project <name>` as shown above.

## Design principles

- Implementations **do not use BCL collections** (no `List<T>`, `Dictionary`, `SortedSet`, etc.)
  for the data structure itself — the goal is to see every pointer move and every comparison.
- Algorithmic helper containers (queues for BFS, dictionaries for visited sets, etc.)
  are BCL because the learning target is the algorithm, not reimplementing those helpers.
- Every operation prints what it is doing: comparisons, swaps, rotations, recolors —
  so execution is fully visible.
