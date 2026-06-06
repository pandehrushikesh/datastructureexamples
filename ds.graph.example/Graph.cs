using ds.shared;

namespace ds.graph.example;

/// <summary>
/// Weighted adjacency-list graph — directed or undirected.
/// The graph structure itself is built from scratch using a dictionary of neighbor lists.
/// Algorithmic helper containers (Queue, Stack, HashSet, Dictionary) are from BCL
/// because the learning target here is graph algorithms, not reimplementing those helpers.
///
/// Supports:
///   • BFS              — shortest hop-count distances from a source
///   • DFS              — depth-first traversal with explicit stack trace
///   • Cycle detection  — undirected (parent-tracking DFS) and directed (3-color DFS)
///   • Topological sort — DAG ordering via DFS finish-time stack
///   • Dijkstra         — shortest weighted paths from a source (O(V²) scan version)
/// </summary>
public class DsGraph<T> where T : notnull, IComparable<T>
{
    // Adjacency list: vertex → list of (neighbor, edge weight)
    private readonly Dictionary<T, List<(T To, int Weight)>> _adj = new();
    public bool IsDirected { get; }
    public int  VertexCount => _adj.Count;
    public int  EdgeCount   { get; private set; }

    public DsGraph(bool directed = false) { IsDirected = directed; }

    // ── Structure ─────────────────────────────────────────────────────────────

    public void AddVertex(T v)
    {
        if (_adj.TryAdd(v, []))
            Console.WriteLine($"  [AddVertex] '{v}'");
    }

    public void AddEdge(T from, T to, int weight = 1)
    {
        AddVertex(from);
        AddVertex(to);
        _adj[from].Add((to, weight));
        EdgeCount++;

        if (!IsDirected)
        {
            _adj[to].Add((from, weight));
            Console.WriteLine($"  [AddEdge]   {from} ──{weight}── {to}  (undirected)");
        }
        else
        {
            Console.WriteLine($"  [AddEdge]   {from} ──{weight}──> {to}  (directed)");
        }
    }

    public void PrintAdjacencyList(string label = "")
    {
        if (label != "") Console.WriteLine($"\n  {label}");
        Console.WriteLine($"  Adjacency list ({(IsDirected ? "directed" : "undirected")}, {VertexCount} vertices, {EdgeCount} edges):");
        foreach (var v in _adj.Keys.Order())
        {
            var edges = _adj[v].Select(n => n.Weight == 1 ? $"{n.To}" : $"{n.To}(w={n.Weight})");
            Console.WriteLine($"    {v}  →  [ {string.Join(", ", edges)} ]");
        }
    }

    // ── BFS ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Breadth-First Search from <paramref name="start"/>.
    /// Visits nodes level by level (nearest first) using a queue.
    /// Returns shortest hop-count distances from the source.
    /// O(V + E).
    /// </summary>
    public Dictionary<T, int> BFS(T start)
    {
        BigO.Print("O(V + E)",
            "Every vertex is enqueued once (O(V)) and every edge is inspected once (O(E)). " +
            "BFS guarantees shortest hop-count distances because it expands level by level.");

        Console.WriteLine($"\n[BFS] Starting from '{start}'");
        var visited  = new HashSet<T>();
        var distance = new Dictionary<T, int>();
        var queue    = new Queue<T>();

        visited.Add(start);
        distance[start] = 0;
        queue.Enqueue(start);
        Console.WriteLine($"  Enqueue '{start}'  |  Queue: [ {QStr(queue)} ]");

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            Console.WriteLine($"\n  Dequeue '{cur}' (dist={distance[cur]})  |  Queue: [ {QStr(queue)} ]");

            foreach (var (nb, _) in _adj[cur])
            {
                if (!visited.Contains(nb))
                {
                    visited.Add(nb);
                    distance[nb] = distance[cur] + 1;
                    queue.Enqueue(nb);
                    Console.WriteLine($"    '{cur}' → '{nb}' unvisited — enqueue  |  Queue: [ {QStr(queue)} ]");
                }
                else
                {
                    Console.WriteLine($"    '{cur}' → '{nb}' already visited — skip");
                }
            }
        }

        Console.WriteLine("\n  BFS result (hop-count distances from source):");
        foreach (var (v, d) in distance.OrderBy(x => x.Value).ThenBy(x => x.Key))
            Console.WriteLine($"    '{start}' → '{v}':  {d} hop{(d == 1 ? "" : "s")}");

        return distance;
    }

    // ── DFS ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Depth-First Search from <paramref name="start"/> — iterative, explicit stack.
    /// Follows one path as deep as possible before backtracking.
    /// O(V + E).
    /// </summary>
    public List<T> DFS(T start)
    {
        BigO.Print("O(V + E)",
            "Every vertex is pushed once (O(V)) and every edge is inspected once (O(E)). " +
            "DFS uses a stack instead of a queue — go deep first, backtrack when stuck.");

        Console.WriteLine($"\n[DFS] Starting from '{start}'");
        var visited = new HashSet<T>();
        var order   = new List<T>();
        var stack   = new Stack<T>();

        stack.Push(start);
        Console.WriteLine($"  Push '{start}'  |  Stack (top→bottom): [ {SStr(stack)} ]");

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (visited.Contains(cur))
            {
                Console.WriteLine($"  Pop  '{cur}' — already visited, skip");
                continue;
            }
            visited.Add(cur);
            order.Add(cur);
            Console.WriteLine($"  Pop  '{cur}' — VISIT  |  Stack: [ {SStr(stack)} ]");

            // Push in reverse order so we pop and process in natural (sorted) order
            foreach (var (nb, _) in _adj[cur].OrderByDescending(n => n.To))
            {
                if (!visited.Contains(nb))
                {
                    stack.Push(nb);
                    Console.WriteLine($"    Push '{nb}'  |  Stack: [ {SStr(stack)} ]");
                }
            }
        }

        Console.WriteLine($"\n  DFS visit order: {string.Join(" → ", order)}");
        return order;
    }

    // ── Cycle Detection ───────────────────────────────────────────────────────

    /// <summary>
    /// Detects a cycle in an UNDIRECTED graph.
    /// DFS with parent tracking: if we reach an already-visited node that is NOT
    /// the node we came from, that is a genuine back edge — a cycle exists.
    /// O(V + E).
    /// </summary>
    public bool HasCycleUndirected()
    {
        if (IsDirected) throw new InvalidOperationException("Use HasCycleDirected() for directed graphs.");
        BigO.Print("O(V + E)",
            "DFS with parent tracking. If a visited neighbour is not the node we came from, " +
            "it is a back edge — we found a cycle.");

        Console.WriteLine("\n[HasCycle - Undirected] DFS with parent tracking...");
        var visited = new HashSet<T>();

        foreach (var v in _adj.Keys)
        {
            if (!visited.Contains(v) && DfsCycleUndirected(v, default, visited, isRoot: true))
            {
                Console.WriteLine("  → Cycle DETECTED.");
                return true;
            }
        }
        Console.WriteLine("  → No cycle found.");
        return false;
    }

    private bool DfsCycleUndirected(T node, T? parent, HashSet<T> visited, bool isRoot)
    {
        visited.Add(node);
        Console.WriteLine($"  Visit '{node}' (parent = {(isRoot ? "none" : $"'{parent}'")})");

        foreach (var (nb, _) in _adj[node])
        {
            // Self-loop is always a cycle regardless of root/parent status
            if (nb.Equals(node))
            {
                Console.WriteLine($"    '{node}' → '{nb}' self-loop → CYCLE!");
                return true;
            }

            if (!visited.Contains(nb))
            {
                if (DfsCycleUndirected(nb, node, visited, isRoot: false)) return true;
            }
            else if (!isRoot && !nb.Equals(parent))
            {
                Console.WriteLine($"    '{node}' → '{nb}' is visited and not the parent → back edge → CYCLE!");
                return true;
            }
            else
            {
                Console.WriteLine($"    '{node}' → '{nb}' visited (parent — skip)");
            }
        }
        return false;
    }

    /// <summary>
    /// Detects a cycle in a DIRECTED graph using 3-color DFS.
    /// White (0) = unvisited, Gray (1) = on the current DFS path, Black (2) = finished.
    /// A gray→gray back edge means we reached an ancestor on our own path — cycle.
    /// O(V + E).
    /// </summary>
    public bool HasCycleDirected()
    {
        if (!IsDirected) throw new InvalidOperationException("Use HasCycleUndirected() for undirected graphs.");
        BigO.Print("O(V + E)",
            "3-color DFS. White = unvisited, Gray = on current path, Black = fully finished. " +
            "A gray→gray edge is a back edge — we reached an ancestor on our own call stack — cycle.");

        Console.WriteLine("\n[HasCycle - Directed] 3-color DFS...");
        Console.WriteLine("  White = not yet visited  |  Gray = in current DFS path  |  Black = fully processed");

        var color = new Dictionary<T, int>(); // 0=white 1=gray 2=black
        foreach (var v in _adj.Keys) color[v] = 0;

        foreach (var v in _adj.Keys)
        {
            if (color[v] == 0 && DfsCycleDirected(v, color))
            {
                Console.WriteLine("  → Cycle DETECTED.");
                return true;
            }
        }
        Console.WriteLine("  → No cycle found. Graph is a DAG.");
        return false;
    }

    private bool DfsCycleDirected(T node, Dictionary<T, int> color)
    {
        color[node] = 1; // gray — on current path
        Console.WriteLine($"  Enter '{node}' → GRAY");

        foreach (var (nb, _) in _adj[node])
        {
            if (color[nb] == 1)
            {
                Console.WriteLine($"    '{node}' → '{nb}' is GRAY — back edge → CYCLE!");
                return true;
            }
            else if (color[nb] == 0)
            {
                Console.WriteLine($"    '{node}' → '{nb}' is WHITE — recurse");
                if (DfsCycleDirected(nb, color)) return true;
            }
            else
            {
                Console.WriteLine($"    '{node}' → '{nb}' is BLACK — already finished, skip");
            }
        }

        color[node] = 2; // black — fully finished
        Console.WriteLine($"  Leave '{node}' → BLACK");
        return false;
    }

    // ── Topological Sort ──────────────────────────────────────────────────────

    /// <summary>
    /// Topological sort of a DAG via DFS finish-time ordering.
    /// A vertex is pushed to the result stack only AFTER all its successors are fully processed.
    /// Reversing (popping) that stack gives a valid topological ordering.
    /// O(V + E). Only valid on a DAG — run HasCycleDirected first if unsure.
    /// </summary>
    public List<T> TopologicalSort()
    {
        if (!IsDirected) throw new InvalidOperationException("Topological sort requires a directed graph.");
        BigO.Print("O(V + E)",
            "DFS-based: a vertex is added to the result only after all vertices it leads to are done. " +
            "Reversing finish order gives a valid topological ordering.");

        Console.WriteLine("\n[TopologicalSort] DFS finish-time stack...");

        var visited = new HashSet<T>();
        var result  = new Stack<T>();

        foreach (var v in _adj.Keys)
        {
            if (!visited.Contains(v))
                TopoSortDfs(v, visited, result);
        }

        var order = result.ToList();
        Console.WriteLine($"\n  Topological order: {string.Join(" → ", order)}");
        return order;
    }

    private void TopoSortDfs(T node, HashSet<T> visited, Stack<T> result)
    {
        visited.Add(node);
        Console.WriteLine($"  Enter '{node}'");

        foreach (var (nb, _) in _adj[node])
        {
            if (!visited.Contains(nb))
            {
                Console.WriteLine($"    '{node}' → '{nb}' — recurse");
                TopoSortDfs(nb, visited, result);
            }
        }

        result.Push(node);
        Console.WriteLine($"  Finish '{node}' — push  |  stack: [ {SStr(result)} ]");
    }

    // ── Dijkstra ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Dijkstra's single-source shortest-path algorithm.
    /// Uses a simple O(V) linear scan each round to find the nearest unvisited vertex.
    /// The O(V²) total cost makes the relaxation logic easy to follow step by step.
    /// All edge weights must be ≥ 0.
    /// </summary>
    public Dictionary<T, int> Dijkstra(T source)
    {
        BigO.Print("O(V²)",
            "V rounds, each scanning all V unvisited vertices to find the minimum distance. " +
            "A binary min-heap reduces this to O((V+E) log V) — the relaxation logic is identical " +
            "either way, so the linear scan makes the algorithm easiest to follow.");

        // Dijkstra requires non-negative weights — negative edges break the greedy lock-in
        foreach (var (_, neighbors) in _adj)
            foreach (var (_, w) in neighbors)
                if (w < 0) throw new InvalidOperationException(
                    $"Dijkstra requires all edge weights ≥ 0. Found weight {w}. Use Bellman-Ford for negative weights.");

        Console.WriteLine($"\n[Dijkstra] Shortest paths from '{source}' (all weights ≥ 0)");

        const int Inf = int.MaxValue / 2; // halved to avoid overflow on addition
        var dist = new Dictionary<T, int>();
        var prev = new Dictionary<T, T>();  // predecessor on the shortest path (no entry = source or unreachable)
        var done = new HashSet<T>();

        foreach (var v in _adj.Keys) dist[v] = Inf;
        dist[source] = 0;

        Console.WriteLine($"  Init: '{source}'=0, all others=∞\n");

        while (done.Count < _adj.Count)
        {
            // Pick the unvisited vertex with the smallest known distance (greedy choice)
            T? u = default;
            int best = Inf + 1;
            foreach (var v in _adj.Keys)
            {
                if (!done.Contains(v) && dist[v] < best) { best = dist[v]; u = v; }
            }
            if (u is null || dist[u] >= Inf) break; // remaining vertices are unreachable

            done.Add(u);
            Console.WriteLine($"  Lock '{u}' (dist={dist[u]}) — nearest unvisited vertex");

            foreach (var (nb, w) in _adj[u])
            {
                if (done.Contains(nb)) continue;
                int candidate = dist[u] + w;
                string curStr = dist[nb] >= Inf ? "∞" : dist[nb].ToString();
                Console.Write($"    Relax '{u}'→'{nb}' (w={w}): {dist[u]}+{w}={candidate} vs {curStr}  →  ");
                if (candidate < dist[nb])
                {
                    dist[nb] = candidate;
                    prev[nb] = u;
                    Console.WriteLine($"improved to {candidate}");
                }
                else
                {
                    Console.WriteLine("no improvement");
                }
            }
        }

        Console.WriteLine("\n  Shortest distances and paths:");
        foreach (var v in _adj.Keys.Order())
        {
            if (dist[v] >= Inf)
                Console.WriteLine($"    '{source}' → '{v}':  unreachable");
            else
            {
                var path = ReconstructPath(prev, source, v);
                Console.WriteLine($"    '{source}' → '{v}':  dist={dist[v]}   path: {string.Join(" → ", path)}");
            }
        }
        return dist;
    }

    private List<T> ReconstructPath(Dictionary<T, T> prev, T source, T target)
    {
        var path = new List<T>();
        var cur  = target;
        while (true)
        {
            path.Insert(0, cur);
            if (cur.Equals(source)) break;
            if (!prev.TryGetValue(cur, out var p)) return [target]; // disconnected
            cur = p;
        }
        return path;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string QStr(Queue<T> q) => string.Join(", ", q);
    private static string SStr(Stack<T> s) => string.Join(", ", s);
}
