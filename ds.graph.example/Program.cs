using ds.graph.example;

Banner("Graph — Data Structure Demo");
Console.WriteLine("""
  A Graph is a collection of vertices (nodes) connected by edges.
  Unlike trees, graphs can have cycles and multiple paths between nodes.

  Key terms:
    • Vertex (V)  — a node in the graph
    • Edge   (E)  — a connection between two vertices; may carry a weight
    • Directed    — edges have direction (A→B ≠ B→A)
    • Undirected  — edges are bidirectional (A—B means both A→B and B→A)
    • Path        — a sequence of vertices where each consecutive pair is connected
    • Cycle       — a path that starts and ends at the same vertex
    • DAG         — Directed Acyclic Graph (directed, no cycles)

  Representation used here: Adjacency List
    Each vertex stores a list of its neighbours.
    Space: O(V + E) — only existing edges are stored.
    This is efficient for sparse graphs (most real-world graphs where E << V²).
    Alternative: Adjacency Matrix — O(V²) space, O(1) edge lookup; good for dense graphs.
""");

// ────────────────────────────────────────────────────────────────────────────
Section("1. Build an undirected graph and inspect its adjacency list");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  Building this undirected graph (a tree — no cycles yet):

       A ── B ── D
       |    |
       C    E ── G
       |
       F

  Edges: A-B, A-C, B-D, B-E, C-F, E-G
""");
var g = new DsGraph<string>(directed: false);
g.AddEdge("A", "B");
g.AddEdge("A", "C");
g.AddEdge("B", "D");
g.AddEdge("B", "E");
g.AddEdge("C", "F");
g.AddEdge("E", "G");
g.PrintAdjacencyList();

// ────────────────────────────────────────────────────────────────────────────
Section("2. BFS — Breadth-First Search (shortest hop-count distances)");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  BFS uses a QUEUE — it visits all neighbours at the current distance before
  moving further away. This "level by level" expansion guarantees that the first
  time we reach a vertex, it is via the shortest hop-count path.

  Expected levels from 'A':
    Level 0: A
    Level 1: B, C          (direct neighbours)
    Level 2: D, E, F       (neighbours of B and C)
    Level 3: G             (neighbour of E)
""");
g.BFS("A");

// ────────────────────────────────────────────────────────────────────────────
Section("3. DFS — Depth-First Search (full traversal, one branch at a time)");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  DFS uses a STACK — it follows one branch as deep as possible before backtracking
  to try the next branch. The explicit stack here mirrors the implicit call stack
  in the recursive version, making every push and pop visible.

  Key difference from BFS:
    BFS → queue → nearest nodes first → shortest hop-count paths
    DFS → stack → deepest nodes first → full branch exploration
""");
g.DFS("A");

// ────────────────────────────────────────────────────────────────────────────
Section("4. Cycle Detection — Undirected Graph");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  In an undirected DFS every edge is traversed in both directions, so we always
  see the node we just came from (our "parent"). That is fine — it's not a cycle.
  A cycle exists only when we reach an already-visited node that is NOT the parent.

  Test 1: the tree above (A-B, A-C, B-D, B-E, C-F, E-G) — no cycle.
""");
g.HasCycleUndirected();

Console.WriteLine("""

  Test 2: add edge D-G — now D-B-E-G-D forms a cycle.
""");
var gCycle = new DsGraph<string>(directed: false);
foreach (var (a, b) in new[] { ("A","B"),("A","C"),("B","D"),("B","E"),("C","F"),("E","G"),("D","G") })
    gCycle.AddEdge(a, b);
gCycle.HasCycleUndirected();

// ────────────────────────────────────────────────────────────────────────────
Section("5. Directed Graph — Topological Sort");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  A DAG (Directed Acyclic Graph) models dependencies — "A must happen before B."
  Topological sort produces a linear ordering where every vertex comes before
  all vertices it has an edge to. More than one valid ordering may exist.

  Algorithm — DFS finish-time stack:
    When we fully finish a vertex (all paths from it are explored), push it.
    The reverse of the finish order is a valid topological ordering.

  Course prerequisites:
    Math     ──► Calculus
    Math     ──► DS
    CS101    ──► DS
    DS       ──► Algorithms
    Calculus ──► Algorithms

  One valid order: Math → CS101 → Calculus → DS → Algorithms
  (Math and CS101 are sources — no prerequisites — so they come first)
""");
var dag = new DsGraph<string>(directed: true);
dag.AddEdge("Math",     "Calculus");
dag.AddEdge("Math",     "DS");
dag.AddEdge("CS101",    "DS");
dag.AddEdge("DS",       "Algorithms");
dag.AddEdge("Calculus", "Algorithms");
dag.PrintAdjacencyList();

Console.WriteLine("\n  First verify there is no cycle (it must be a DAG for topo sort to be valid):");
dag.HasCycleDirected();

dag.TopologicalSort();

// ────────────────────────────────────────────────────────────────────────────
Section("6. Directed Cycle Detection — 3-Color DFS");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  The undirected "parent check" does not work for directed graphs.
  Consider A→B, B→C, C→A — this is a cycle, but B's parent is A, so when C
  sees A it would wrongly think "that's my grandparent, not an issue."

  Instead we use three colors to track DFS state:
    White — not yet visited
    Gray  — currently on the DFS call stack (in-progress path)
    Black — fully finished (all outgoing paths explored)

  If we ever try to follow an edge to a GRAY vertex, we found a back edge —
  we have looped back to an ancestor on our current path — that is a cycle.

  Graph: A→B, B→C, C→A  (triangle cycle)
""");
var gDir = new DsGraph<string>(directed: true);
gDir.AddEdge("A", "B");
gDir.AddEdge("B", "C");
gDir.AddEdge("C", "A");
gDir.HasCycleDirected();

// ────────────────────────────────────────────────────────────────────────────
Section("7. Dijkstra's Shortest Path — Weighted Directed Graph");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  Dijkstra finds the shortest weighted path from a source to every other vertex.

  Core idea — RELAXATION:
    For each neighbour nb of the current vertex u:
      if  dist[u] + weight(u→nb)  <  dist[nb]
      then update dist[nb] (we found a cheaper path through u).

  Greedy choice: always process the unvisited vertex with the smallest known
  distance next. This is safe because all edge weights are ≥ 0 — a later path
  can only be equal or more expensive, never cheaper.

  Graph:
    A ──4──► B
    A ──2──► C
    C ──1──► B     ← going A→C→B (cost 2+1=3) beats direct A→B (cost 4)
    B ──5──► D
    C ──8──► D     ← going A→C→B→D (cost 3+5=8) beats A→C→D (cost 2+8=10)
    D ──3──► E

  Expected shortest distances from A:
    A =  0  (source)
    C =  2  (A→C direct)
    B =  3  (A→C→B: 2+1, improves over direct A→B=4)
    D =  8  (A→C→B→D: 3+5, improves over A→C→D=10)
    E = 11  (A→C→B→D→E: 8+3)
""");
var wg = new DsGraph<string>(directed: true);
wg.AddEdge("A", "B", weight: 4);
wg.AddEdge("A", "C", weight: 2);
wg.AddEdge("C", "B", weight: 1);
wg.AddEdge("B", "D", weight: 5);
wg.AddEdge("C", "D", weight: 8);
wg.AddEdge("D", "E", weight: 3);
wg.PrintAdjacencyList();
wg.Dijkstra("A");

// ────────────────────────────────────────────────────────────────────────────
Banner("Big O Summary");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  ┌──────────────────────────┬─────────────────────┬──────────────────────────────────────────────────────────┐
  │ Algorithm                │ Complexity           │ Why                                                      │
  ├──────────────────────────┼─────────────────────┼──────────────────────────────────────────────────────────┤
  │ AddVertex / AddEdge      │ O(1) amortized       │ Dictionary insert + list append                          │
  │ BFS                      │ O(V + E)             │ Each vertex enqueued once; each edge inspected once      │
  │ DFS                      │ O(V + E)             │ Each vertex pushed once; each edge inspected once        │
  │ Cycle detection          │ O(V + E)             │ DFS with parent tracking or 3-color — same traversal     │
  │ Topological sort         │ O(V + E)             │ DFS finish-stack — same traversal cost                   │
  │ Dijkstra (linear scan)   │ O(V²)                │ V rounds × O(V) scan per round to find minimum          │
  │ Dijkstra (binary heap)   │ O((V+E) log V)       │ Min-heap replaces O(V) scan with O(log V) extract-min   │
  └──────────────────────────┴─────────────────────┴──────────────────────────────────────────────────────────┘

  Space: O(V + E) for the adjacency list.
  Adjacency Matrix uses O(V²) — fine for dense graphs, wasteful when E << V².

  When to use which traversal:
    BFS       → shortest hop-count path; level-order exploration; proximity queries
    DFS       → cycle detection; topological sort; connected components; maze solving
    Dijkstra  → shortest WEIGHTED path (all weights ≥ 0)
    Bellman-Ford (not shown) → handles negative weights; detects negative-weight cycles
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
