using ds.shared;

namespace ds.stack.queue.example;

// ── Node ─────────────────────────────────────────────────────────────────────

/// <summary>Internal node used by the Queue. Holds a value and a link to the next node in line.</summary>
internal class QueueNode<T>
{
    public T Value;
    public QueueNode<T>? Next;   // points toward the back of the queue

    public QueueNode(T value) { Value = value; }
    public override string ToString() => $"[{Value}]";
}

// ── Queue ─────────────────────────────────────────────────────────────────────

/// <summary>
/// FIFO (First-In First-Out) Queue built on a singly-linked node chain
/// with both a head (front) and a tail (back) pointer.
/// Intentionally avoids Queue&lt;T&gt;, LinkedList&lt;T&gt;, or any BCL collection.
///
/// Visual:
///   FRONT (head)                     BACK (tail)
///     [A] → [B] → [C] → [D] → null
///      ↑                        ↑
///   Dequeue                  Enqueue
///   removes here             adds here
///
/// Enqueue at the BACK (tail) and Dequeue from the FRONT (head) — both O(1).
/// This is why we need the tail pointer: without it, Enqueue would cost O(n).
/// </summary>
public class DsQueue<T>
{
    private QueueNode<T>? _head;   // front — next to be dequeued
    private QueueNode<T>? _tail;   // back  — where new items join
    public int Count { get; private set; }
    public bool IsEmpty => _head is null;

    // ── Enqueue ───────────────────────────────────────────────────────────────

    /// <summary>Adds an element to the back of the queue — O(1).</summary>
    public void Enqueue(T value)
    {
        BigO.Print("O(1)",
            "The tail pointer gives direct access to the last node, so the new node is attached " +
            "without walking the queue. This is the key reason we maintain a tail pointer.");

        Console.WriteLine($"\n[Enqueue] Adding '{value}' to the BACK of the queue.");
        var node = new QueueNode<T>(value);

        if (_tail is null)
        {
            _head = _tail = node;
            Console.WriteLine($"          Queue was empty — '{value}' is both front and back.");
        }
        else
        {
            var oldTail = _tail;
            _tail.Next = node;
            _tail = node;
            Console.WriteLine($"          Old back {oldTail} → linked to new back {node}. Tail updated.");
        }
        Count++;
        PrintQueue("After Enqueue");
    }

    // ── Dequeue ───────────────────────────────────────────────────────────────

    /// <summary>Removes and returns the element at the front of the queue — O(1).</summary>
    public T Dequeue()
    {
        if (_head is null)
            throw new InvalidOperationException("Cannot Dequeue from an empty queue.");

        BigO.Print("O(1)",
            "The front pointer is advanced to the next node — a single pointer update with no traversal. " +
            "The removed node becomes unreachable and is collected by the GC.");

        Console.WriteLine($"\n[Dequeue] Removing front element {_head}.");
        var value = _head.Value;
        _head = _head.Next;
        Count--;

        if (_head is null)
        {
            _tail = null;   // queue is now empty — tail must also be cleared
            Console.WriteLine("          Queue is now empty. Tail pointer also cleared.");
        }
        else
        {
            Console.WriteLine($"          New front is {_head}.");
        }

        PrintQueue("After Dequeue");
        return value;
    }

    // ── Peek ─────────────────────────────────────────────────────────────────

    /// <summary>Returns the front element WITHOUT removing it — O(1).</summary>
    public T Peek()
    {
        if (_head is null)
            throw new InvalidOperationException("Cannot Peek into an empty queue.");

        BigO.Print("O(1)",
            "The head pointer gives instant access to the front node. " +
            "No removal or traversal — just read the value.");

        Console.WriteLine($"\n[Peek] Front element is {_head} (not removed).");
        return _head.Value;
    }

    // ── Contains ─────────────────────────────────────────────────────────────

    /// <summary>Returns true if the value exists anywhere in the queue — O(n).</summary>
    public bool Contains(T value)
    {
        BigO.Print("O(n)",
            "The queue must be walked from front to back since there is no index or ordering guarantee. " +
            "In the worst case every node is visited before a match is found or the back is reached.");

        Console.WriteLine($"\n[Contains] Searching queue for '{value}' (front → back)...");
        var current = _head;
        int pos = 0;
        while (current is not null)
        {
            Console.WriteLine($"           Position {pos}: {current}");
            if (EqualityComparer<T>.Default.Equals(current.Value, value))
            {
                Console.WriteLine($"           Found '{value}' at position {pos}. ✓");
                return true;
            }
            current = current.Next;
            pos++;
        }
        Console.WriteLine($"           '{value}' not found in queue.");
        return false;
    }

    // ── Clear ────────────────────────────────────────────────────────────────

    /// <summary>Removes all elements — O(1) in C#.</summary>
    public void Clear()
    {
        BigO.Print("O(1)",
            "Setting both head and tail to null makes the entire node chain unreachable. " +
            "As with the doubly-linked list, BOTH pointers must be cleared or the tail keeps the chain alive.");

        Console.WriteLine($"\n[Clear] Releasing head and tail pointers. GC will collect all {Count} node(s).");
        _head = null;
        _tail = null;
        Count = 0;
        Console.WriteLine("[Clear] Queue is now empty.");
        PrintQueue("After Clear");
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public void PrintQueue(string label = "Queue")
    {
        var parts = new System.Text.StringBuilder();
        var current = _head;
        while (current is not null)
        {
            parts.Append(current);
            if (current.Next is not null) parts.Append(" → ");
            current = current.Next;
        }
        var body = parts.Length > 0 ? parts.ToString() : "(empty)";
        Console.WriteLine($"  [{label}] FRONT: {body} :BACK  (Count={Count})");
    }
}
