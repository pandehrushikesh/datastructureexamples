using ds.shared;

namespace ds.linkedlist.example;

// ── Node ─────────────────────────────────────────────────────────────────────

/// <summary>A single link in a singly-linked chain.</summary>
public class SinglyNode<T>
{
    public T Value;
    public SinglyNode<T>? Next;

    public SinglyNode(T value) { Value = value; }
    public override string ToString() => $"[{Value}]";
}

// ── List ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Singly-linked list built from scratch.
/// Each node holds a value and a pointer to the NEXT node only.
/// Head → [A] → [B] → [C] → null
/// </summary>
public class SinglyLinkedList<T> where T : IComparable<T>
{
    private SinglyNode<T>? _head;
    public int Count { get; private set; }
    public bool IsEmpty => _head is null;

    // ── Add ──────────────────────────────────────────────────────────────────

    /// <summary>Prepends a node — O(1).</summary>
    public void AddFirst(T value)
    {
        BigO.Print("O(1)",
            "The new node is wired directly to the current head and becomes the new head. " +
            "No traversal is needed regardless of list size.");

        Console.WriteLine($"\n[AddFirst] Adding '{value}' at the HEAD.");
        var node = new SinglyNode<T>(value);
        node.Next = _head;

        if (_head is null)
            Console.WriteLine($"           List was empty — '{value}' becomes the head.");
        else
            Console.WriteLine($"           New node points to old head {_head}. New head = {node}.");

        _head = node;
        Count++;
        PrintList("After AddFirst");
    }

    /// <summary>Appends a node — O(n) (must walk to the tail).</summary>
    public void AddLast(T value)
    {
        BigO.Print("O(n)",
            "There is no tail pointer, so the list must be walked node by node until the last one is reached. " +
            "A doubly-linked list with a tail pointer makes this O(1).");

        Console.WriteLine($"\n[AddLast] Adding '{value}' at the TAIL.");
        var node = new SinglyNode<T>(value);

        if (_head is null)
        {
            _head = node;
            Console.WriteLine($"          List was empty — '{value}' becomes head and tail.");
        }
        else
        {
            Console.WriteLine("          Walking to the tail...");
            var current = _head;
            while (current.Next is not null)
            {
                Console.WriteLine($"          → {current}  (Next={current.Next})");
                current = current.Next;
            }
            Console.WriteLine($"          Reached tail {current}. Attaching new node.");
            current.Next = node;
        }
        Count++;
        PrintList("After AddLast");
    }

    /// <summary>Inserts after the first node whose value equals <paramref name="afterValue"/> — O(n).</summary>
    public void InsertAfter(T afterValue, T value)
    {
        BigO.Print("O(n)",
            "A linear scan is needed to find the target node. " +
            "Once found, the actual insertion is O(1) — just pointer rewiring.");

        Console.WriteLine($"\n[InsertAfter] Looking for '{afterValue}' to insert '{value}' after it...");
        var current = _head;
        while (current is not null)
        {
            Console.WriteLine($"              Visiting {current}");
            if (current.Value.CompareTo(afterValue) == 0)
            {
                var node = new SinglyNode<T>(value);
                node.Next = current.Next;
                current.Next = node;
                Count++;
                Console.WriteLine($"              Found! Inserted {node} after {current}.");
                PrintList("After InsertAfter");
                return;
            }
            current = current.Next;
        }
        Console.WriteLine($"              '{afterValue}' not found — nothing inserted.");
    }

    // ── Remove ───────────────────────────────────────────────────────────────

    /// <summary>Removes the head node — O(1).</summary>
    public void RemoveFirst()
    {
        if (_head is null) { Console.WriteLine("\n[RemoveFirst] List is empty."); return; }

        BigO.Print("O(1)",
            "The head pointer is simply advanced to the next node. " +
            "No traversal is required.");

        Console.WriteLine($"\n[RemoveFirst] Removing head {_head}.");
        _head = _head.Next;
        Count--;

        Console.WriteLine(_head is null
            ? "              List is now empty."
            : $"              New head is {_head}.");
        PrintList("After RemoveFirst");
    }

    /// <summary>Removes the tail node — O(n).</summary>
    public void RemoveLast()
    {
        if (_head is null) { Console.WriteLine("\n[RemoveLast] List is empty."); return; }

        BigO.Print("O(n)",
            "With no tail pointer, the entire list must be walked to find the second-to-last node " +
            "so its Next can be set to null. A doubly-linked list makes this O(1).");

        Console.WriteLine("\n[RemoveLast] Removing the TAIL node.");

        if (_head.Next is null)
        {
            Console.WriteLine($"             Only one node {_head} — list becomes empty.");
            _head = null;
            Count--;
            PrintList("After RemoveLast");
            return;
        }

        Console.WriteLine("             Walking to find the node before the tail...");
        var prev = _head;
        while (prev.Next!.Next is not null)
        {
            Console.WriteLine($"             → {prev}");
            prev = prev.Next;
        }
        Console.WriteLine($"             Node before tail: {prev}. Tail: {prev.Next}. Unlinking tail.");
        prev.Next = null;
        Count--;
        PrintList("After RemoveLast");
    }

    /// <summary>Removes the first node whose value equals <paramref name="value"/> — O(n).</summary>
    public bool Remove(T value)
    {
        BigO.Print("O(n)",
            "The list is scanned from the head until a matching node is found. " +
            "Once found, unlinking it is O(1) — just update the previous node's Next pointer.");

        Console.WriteLine($"\n[Remove] Searching for '{value}'...");
        if (_head is null) { Console.WriteLine("         List is empty."); return false; }

        if (_head.Value.CompareTo(value) == 0)
        {
            Console.WriteLine($"         Head {_head} matches — removing head.");
            _head = _head.Next;
            Count--;
            PrintList("After Remove");
            return true;
        }

        var prev = _head;
        var current = _head.Next;
        while (current is not null)
        {
            Console.WriteLine($"         Visiting {current} (prev={prev})");
            if (current.Value.CompareTo(value) == 0)
            {
                Console.WriteLine($"         Match! Unlinking {current}. prev.Next → {current.Next}.");
                prev.Next = current.Next;
                Count--;
                PrintList("After Remove");
                return true;
            }
            prev = current;
            current = current.Next;
        }
        Console.WriteLine($"         '{value}' not found.");
        return false;
    }

    // ── Query ────────────────────────────────────────────────────────────────

    /// <summary>Linear scan — O(n).</summary>
    public bool Contains(T value)
    {
        BigO.Print("O(n)",
            "Each node is visited one by one from the head. " +
            "Best case O(1) if the match is at the head; worst case O(n) if absent or at the tail.");

        Console.WriteLine($"\n[Contains] Searching for '{value}'...");
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
    /// Setting _head to null makes the entire chain unreachable; the GC collects
    /// every node automatically. No need to walk and null out each Next pointer
    /// (that would be required in C++ where you must free() each node manually).
    /// </summary>
    public void Clear()
    {
        BigO.Print("O(1)",
            "Dropping the head reference makes the entire node chain unreachable. " +
            "The garbage collector reclaims all nodes automatically — no manual walk needed (unlike C++).");

        Console.WriteLine($"\n[Clear] Dropping reference to head. GC will collect all {Count} node(s).");
        _head = null;
        Count = 0;
        Console.WriteLine("[Clear] Done. List is now empty.");
        PrintList("After Clear");
    }

    // ── Reversal ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Reverses the list in-place by re-wiring every Next pointer — O(n).
    /// Classic three-pointer technique: prev, current, next.
    /// </summary>
    public void Reverse()
    {
        BigO.Print("O(n)",
            "Every node's Next pointer must be re-wired exactly once using the three-pointer " +
            "(prev / current / next) technique. One pass through the list suffices.");

        Console.WriteLine("\n[Reverse] Reversing the list in-place...");
        PrintList("Before");

        SinglyNode<T>? prev = null;
        var current = _head;

        while (current is not null)
        {
            var next = current.Next;
            Console.WriteLine($"          {current}.Next → {(prev is null ? "null" : prev.ToString())}  (saved next={next?.ToString() ?? "null"})");
            current.Next = prev;
            prev = current;
            current = next;
        }

        _head = prev;
        Console.WriteLine("[Reverse] Done.");
        PrintList("After");
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public void PrintList(string label = "List")
    {
        var parts = new System.Text.StringBuilder();
        var current = _head;
        while (current is not null)
        {
            parts.Append(current);
            if (current.Next is not null) parts.Append(" → ");
            current = current.Next;
        }
        parts.Append(" → null");
        Console.WriteLine($"  [{label}] {parts}  (Count={Count})");
    }
}
