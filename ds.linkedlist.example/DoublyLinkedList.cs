using ds.shared;

namespace ds.linkedlist.example;

// ── Node ─────────────────────────────────────────────────────────────────────

/// <summary>A single link in a doubly-linked chain — knows both its neighbours.</summary>
public class DoublyNode<T>
{
    public T Value;
    public DoublyNode<T>? Prev;
    public DoublyNode<T>? Next;

    public DoublyNode(T value) { Value = value; }
    public override string ToString() => $"[{Value}]";
}

// ── List ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Doubly-linked list built from scratch.
/// Each node holds a value, a pointer to the NEXT node, and a pointer to the PREV node.
/// null ← [A] ⇄ [B] ⇄ [C] → null
///         ↑                   ↑
///        Head                Tail
/// </summary>
public class DoublyLinkedList<T> where T : IComparable<T>
{
    private DoublyNode<T>? _head;
    private DoublyNode<T>? _tail;
    public int Count { get; private set; }
    public bool IsEmpty => _head is null;

    // ── Add ──────────────────────────────────────────────────────────────────

    /// <summary>Prepends a node — O(1).</summary>
    public void AddFirst(T value)
    {
        BigO.Print("O(1)",
            "The new node is wired before the current head and becomes the new head. " +
            "No traversal is needed — both the head pointer and the old head's Prev are updated directly.");

        Console.WriteLine($"\n[AddFirst] Adding '{value}' at the HEAD.");
        var node = new DoublyNode<T>(value);

        if (_head is null)
        {
            _head = _tail = node;
            Console.WriteLine($"           List was empty — '{value}' is both head and tail.");
        }
        else
        {
            node.Next = _head;
            _head.Prev = node;
            _head = node;
            Console.WriteLine($"           {node} ← linked to old head {node.Next}. New head = {_head}.");
        }
        Count++;
        PrintList("After AddFirst");
    }

    /// <summary>Appends a node — O(1) because we hold a tail pointer.</summary>
    public void AddLast(T value)
    {
        BigO.Print("O(1)",
            "The tail pointer gives direct access to the last node — no traversal required. " +
            "This is the key advantage over a singly-linked list, where AddLast costs O(n).");

        Console.WriteLine($"\n[AddLast] Adding '{value}' at the TAIL.");
        var node = new DoublyNode<T>(value);

        if (_tail is null)
        {
            _head = _tail = node;
            Console.WriteLine($"          List was empty — '{value}' is both head and tail.");
        }
        else
        {
            node.Prev = _tail;
            _tail.Next = node;
            _tail = node;
            Console.WriteLine($"          Old tail {node.Prev} → linked to {node}. New tail = {_tail}.");
        }
        Count++;
        PrintList("After AddLast");
    }

    /// <summary>Inserts a new node immediately BEFORE the first node matching <paramref name="beforeValue"/> — O(n).</summary>
    public void InsertBefore(T beforeValue, T value)
    {
        BigO.Print("O(n)",
            "A linear scan is needed to find the target node. " +
            "The actual insertion is O(1) — four pointer updates using both Prev and Next links.");

        Console.WriteLine($"\n[InsertBefore] Looking for '{beforeValue}' to insert '{value}' before it...");
        var current = _head;
        while (current is not null)
        {
            Console.WriteLine($"               Visiting {current}");
            if (current.Value.CompareTo(beforeValue) == 0)
            {
                var node = new DoublyNode<T>(value);
                node.Next = current;
                node.Prev = current.Prev;

                if (current.Prev is not null)
                    current.Prev.Next = node;
                else
                    _head = node;

                current.Prev = node;
                Count++;
                Console.WriteLine($"               Inserted {node} before {current}.");
                PrintList("After InsertBefore");
                return;
            }
            current = current.Next;
        }
        Console.WriteLine($"               '{beforeValue}' not found — nothing inserted.");
    }

    /// <summary>Inserts a new node immediately AFTER the first node matching <paramref name="afterValue"/> — O(n).</summary>
    public void InsertAfter(T afterValue, T value)
    {
        BigO.Print("O(n)",
            "A linear scan is needed to find the target node. " +
            "The actual insertion is O(1) — four pointer updates using both Prev and Next links.");

        Console.WriteLine($"\n[InsertAfter] Looking for '{afterValue}' to insert '{value}' after it...");
        var current = _head;
        while (current is not null)
        {
            Console.WriteLine($"              Visiting {current}");
            if (current.Value.CompareTo(afterValue) == 0)
            {
                var node = new DoublyNode<T>(value);
                node.Prev = current;
                node.Next = current.Next;

                if (current.Next is not null)
                    current.Next.Prev = node;
                else
                    _tail = node;

                current.Next = node;
                Count++;
                Console.WriteLine($"              Inserted {node} after {current}.");
                PrintList("After InsertAfter");
                return;
            }
            current = current.Next;
        }
        Console.WriteLine($"              '{afterValue}' not found — nothing inserted.");
    }

    // ── Remove ───────────────────────────────────────────────────────────────

    /// <summary>Removes the head — O(1).</summary>
    public void RemoveFirst()
    {
        if (_head is null) { Console.WriteLine("\n[RemoveFirst] List is empty."); return; }

        BigO.Print("O(1)",
            "The head pointer is advanced to the next node and its Prev is cleared. " +
            "No traversal needed — direct pointer update.");

        Console.WriteLine($"\n[RemoveFirst] Removing head {_head}.");

        if (_head == _tail)
        {
            _head = _tail = null;
            Console.WriteLine("              Only node — list is now empty.");
        }
        else
        {
            _head = _head.Next!;
            _head.Prev = null;
            Console.WriteLine($"              New head = {_head}, its Prev pointer cleared.");
        }
        Count--;
        PrintList("After RemoveFirst");
    }

    /// <summary>Removes the tail — O(1) because we hold the tail pointer.</summary>
    public void RemoveLast()
    {
        if (_tail is null) { Console.WriteLine("\n[RemoveLast] List is empty."); return; }

        BigO.Print("O(1)",
            "The tail pointer gives direct access to the last node. Its Prev pointer leads immediately " +
            "to the new tail — no traversal required. Singly-linked needs O(n) to do the same.");

        Console.WriteLine($"\n[RemoveLast] Removing tail {_tail}.");

        if (_head == _tail)
        {
            _head = _tail = null;
            Console.WriteLine("             Only node — list is now empty.");
        }
        else
        {
            _tail = _tail.Prev!;
            _tail.Next = null;
            Console.WriteLine($"             New tail = {_tail}, its Next pointer cleared.");
        }
        Count--;
        PrintList("After RemoveLast");
    }

    /// <summary>Removes the first node whose value equals <paramref name="value"/> — O(n).</summary>
    public bool Remove(T value)
    {
        BigO.Print("O(n)",
            "A linear scan from the head is needed to locate the node. " +
            "Once found, unlinking it is O(1) — both Prev and Next neighbours are updated directly.");

        Console.WriteLine($"\n[Remove] Searching for '{value}'...");
        var current = _head;
        while (current is not null)
        {
            Console.WriteLine($"         Visiting {current}  (Prev={current.Prev?.ToString() ?? "null"}, Next={current.Next?.ToString() ?? "null"})");
            if (current.Value.CompareTo(value) == 0)
            {
                Console.WriteLine($"         Match! Unlinking {current}.");

                if (current.Prev is not null)
                {
                    current.Prev.Next = current.Next;
                    Console.WriteLine($"         {current.Prev}.Next → {current.Next?.ToString() ?? "null"}");
                }
                else
                {
                    _head = current.Next;
                    Console.WriteLine($"         Was head — new head = {_head?.ToString() ?? "null"}");
                }

                if (current.Next is not null)
                {
                    current.Next.Prev = current.Prev;
                    Console.WriteLine($"         {current.Next}.Prev → {current.Prev?.ToString() ?? "null"}");
                }
                else
                {
                    _tail = current.Prev;
                    Console.WriteLine($"         Was tail — new tail = {_tail?.ToString() ?? "null"}");
                }

                Count--;
                PrintList("After Remove");
                return true;
            }
            current = current.Next;
        }
        Console.WriteLine($"         '{value}' not found.");
        return false;
    }

    // ── Query ────────────────────────────────────────────────────────────────

    /// <summary>Forward scan — O(n).</summary>
    public bool Contains(T value)
    {
        BigO.Print("O(n)",
            "Each node is visited one by one from the head. " +
            "Best case O(1) if the match is at the head; worst case O(n) if absent or at the tail.");

        Console.WriteLine($"\n[Contains] Searching forward for '{value}'...");
        var current = _head;
        int idx = 0;
        while (current is not null)
        {
            Console.WriteLine($"           [{idx}] {current}");
            if (current.Value.CompareTo(value) == 0)
            {
                Console.WriteLine($"           Found '{value}' at index {idx}. ✓");
                return true;
            }
            current = current.Next;
            idx++;
        }
        Console.WriteLine($"           '{value}' not found.");
        return false;
    }

    // ── Clear ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Clears the list — O(1) in C#.
    /// Both _head AND _tail must be nulled out — if only _head were cleared,
    /// _tail would still hold a reference to the last node, keeping the entire
    /// chain alive in memory (a leak).
    /// </summary>
    public void Clear()
    {
        BigO.Print("O(1)",
            "Both head and tail are set to null, making the entire chain unreachable for the GC. " +
            "Unlike a singly-linked list, BOTH pointers must be cleared or _tail keeps the chain alive.");

        Console.WriteLine($"\n[Clear] Nulling head and tail. GC will collect all {Count} node(s).");
        Console.WriteLine("        (Doubly-linked: BOTH _head and _tail must be cleared —");
        Console.WriteLine("         leaving _tail set would keep the whole chain alive in memory.)");
        _head = null;
        _tail = null;
        Count = 0;
        Console.WriteLine("[Clear] Done. List is now empty.");
        PrintList("After Clear");
    }

    // ── Traversal ────────────────────────────────────────────────────────────

    /// <summary>Walks the list BACKWARDS from the tail using Prev pointers.</summary>
    public void PrintReverse()
    {
        Console.WriteLine("\n[PrintReverse] Walking BACKWARDS from tail using Prev pointers...");
        var parts = new System.Text.StringBuilder("null");
        var current = _tail;
        while (current is not null)
        {
            parts.Append($" ← {current}");
            current = current.Prev;
        }
        Console.WriteLine($"  {parts}  (Count={Count})");
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public void PrintList(string label = "List")
    {
        var parts = new System.Text.StringBuilder("null ⇄ ");
        var current = _head;
        while (current is not null)
        {
            parts.Append(current);
            if (current.Next is not null) parts.Append(" ⇄ ");
            current = current.Next;
        }
        parts.Append(" ⇄ null");
        Console.WriteLine($"  [{label}] {parts}  (Count={Count}, Head={_head?.ToString() ?? "null"}, Tail={_tail?.ToString() ?? "null"})");
    }
}
