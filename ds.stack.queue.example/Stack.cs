using ds.shared;

namespace ds.stack.queue.example;

// ── Node ─────────────────────────────────────────────────────────────────────

/// <summary>Internal node used by the Stack. Holds a value and a link to the node below it.</summary>
internal class StackNode<T>
{
    public T Value;
    public StackNode<T>? Below;   // points toward the bottom of the stack

    public StackNode(T value) { Value = value; }
    public override string ToString() => $"[{Value}]";
}

// ── Stack ─────────────────────────────────────────────────────────────────────

/// <summary>
/// LIFO (Last-In First-Out) Stack built on a singly-linked node chain.
/// The top of the stack maps to the head of the chain.
/// Intentionally avoids Stack&lt;T&gt;, LinkedList&lt;T&gt;, or any BCL collection.
///
/// Visual:
///   TOP  →  [C]
///            ↓
///           [B]
///            ↓
///           [A]  ← BOTTOM
///            ↓
///           null
///
/// Push and Pop always happen at the TOP — making both O(1).
/// </summary>
public class DsStack<T>
{
    private StackNode<T>? _top;
    public int Count { get; private set; }
    public bool IsEmpty => _top is null;

    // ── Push ─────────────────────────────────────────────────────────────────

    /// <summary>Places a new element on top of the stack — O(1).</summary>
    public void Push(T value)
    {
        BigO.Print("O(1)",
            "A new node is created and linked to the current top, then the top pointer is updated. " +
            "No traversal of existing nodes is needed, regardless of stack size.");

        Console.WriteLine($"\n[Push] Pushing '{value}' onto the stack.");
        var node = new StackNode<T>(value) { Below = _top };

        if (_top is null)
            Console.WriteLine($"       Stack was empty — '{value}' is now the only element.");
        else
            Console.WriteLine($"       New node {node} sits on top of {_top}.");

        _top = node;
        Count++;
        PrintStack("After Push");
    }

    // ── Pop ──────────────────────────────────────────────────────────────────

    /// <summary>Removes and returns the top element — O(1).</summary>
    public T Pop()
    {
        if (_top is null)
            throw new InvalidOperationException("Cannot Pop from an empty stack.");

        BigO.Print("O(1)",
            "The top pointer is simply advanced to the node below. " +
            "The removed node becomes unreachable and is collected by the GC.");

        Console.WriteLine($"\n[Pop] Popping top element {_top}.");
        var value = _top.Value;
        _top = _top.Below;
        Count--;

        Console.WriteLine(_top is null
            ? "      Stack is now empty."
            : $"      New top is {_top}.");

        PrintStack("After Pop");
        return value;
    }

    // ── Peek ─────────────────────────────────────────────────────────────────

    /// <summary>Returns the top element WITHOUT removing it — O(1).</summary>
    public T Peek()
    {
        if (_top is null)
            throw new InvalidOperationException("Cannot Peek into an empty stack.");

        BigO.Print("O(1)",
            "The top pointer gives instant access to the topmost node. " +
            "No removal or traversal — just read the value.");

        Console.WriteLine($"\n[Peek] Top element is {_top} (not removed).");
        return _top.Value;
    }

    // ── Contains ─────────────────────────────────────────────────────────────

    /// <summary>Returns true if the value exists anywhere in the stack — O(n).</summary>
    public bool Contains(T value)
    {
        BigO.Print("O(n)",
            "The stack must be walked from top to bottom since there is no index or ordering. " +
            "In the worst case every node is visited before finding a match or reaching the bottom.");

        Console.WriteLine($"\n[Contains] Searching stack for '{value}' (top → bottom)...");
        var current = _top;
        int depth = 0;
        while (current is not null)
        {
            Console.WriteLine($"           Depth {depth}: {current}");
            if (EqualityComparer<T>.Default.Equals(current.Value, value))
            {
                Console.WriteLine($"           Found '{value}' at depth {depth}. ✓");
                return true;
            }
            current = current.Below;
            depth++;
        }
        Console.WriteLine($"           '{value}' not found in stack.");
        return false;
    }

    // ── Clear ────────────────────────────────────────────────────────────────

    /// <summary>Removes all elements — O(1) in C#.</summary>
    public void Clear()
    {
        BigO.Print("O(1)",
            "Setting the top pointer to null makes the entire node chain unreachable. " +
            "The GC reclaims all nodes — no manual walk required.");

        Console.WriteLine($"\n[Clear] Releasing top pointer. GC will collect all {Count} node(s).");
        _top = null;
        Count = 0;
        Console.WriteLine("[Clear] Stack is now empty.");
        PrintStack("After Clear");
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public void PrintStack(string label = "Stack")
    {
        var parts = new System.Text.StringBuilder();
        var current = _top;
        bool first = true;
        while (current is not null)
        {
            if (!first) parts.Append(" → ");
            parts.Append(current);
            first = false;
            current = current.Below;
        }
        var body = parts.Length > 0 ? parts.ToString() : "(empty)";
        Console.WriteLine($"  [{label}] TOP: {body} :BOTTOM  (Count={Count})");
    }
}
