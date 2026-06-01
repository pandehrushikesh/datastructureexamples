using ds.shared;

namespace ds.binaryheap.example;

// ── Heap type ─────────────────────────────────────────────────────────────────

public enum HeapType { Min, Max }

// ── Heap ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Generic Binary Heap — stored entirely in a flat array (no node pointers).
///
/// KEY INSIGHT — array index arithmetic replaces pointers:
///   Parent    of index i  →  (i - 1) / 2
///   Left child of index i →  2 * i + 1
///   Right child of index i→  2 * i + 2
///
/// Example: MinHeap containing [10, 20, 30, 40, 50, 60]
///
///   Array:  [ 10 | 20 | 30 | 40 | 50 | 60 ]
///   Index:    [0]  [1]  [2]  [3]  [4]  [5]
///
///   Tree view (drawn from the array above):
///
///              10          ← index 0  (root = always the minimum)
///            /    \
///          20      30      ← index 1, 2
///         /  \    /
///        40  50  60        ← index 3, 4, 5
///
///   It is a COMPLETE binary tree: every level is filled left-to-right.
///   The array representation wastes no space and has excellent cache locality.
///
/// HEAP PROPERTY:
///   MinHeap → every parent ≤ both children  (root = global minimum)
///   MaxHeap → every parent ≥ both children  (root = global maximum)
///   Note: siblings have NO ordering between them — this is NOT a sorted array.
///
/// Supports both MinHeap and MaxHeap via the HeapType constructor parameter.
/// The only difference between the two is the comparison direction in Dominates().
/// </summary>
public class DsHeap<T> where T : IComparable<T>
{
    private T[]      _data;
    private int      _count;
    private readonly HeapType _type;
    private const    int DefaultCapacity = 8;

    public int      Count    => _count;
    public bool     IsEmpty  => _count == 0;
    public HeapType Type     => _type;

    public DsHeap(HeapType type = HeapType.Min, int initialCapacity = DefaultCapacity)
    {
        _type  = type;
        _data  = new T[initialCapacity];
        _count = 0;

        string dominant = type == HeapType.Min ? "minimum" : "maximum";
        Console.WriteLine($"[DsHeap] Created {type}Heap. Root is always the {dominant}.");
        Console.WriteLine($"         Backed by a flat array — no node pointers.");
        Console.WriteLine($"         Parent(i)=(i-1)/2  |  LeftChild(i)=2i+1  |  RightChild(i)=2i+2");
    }

    // ── Insert ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Appends the value at the end of the array (next available slot),
    /// then bubbles it UP by repeatedly swapping with its parent until
    /// the heap property is restored.  O(log n).
    /// </summary>
    public void Insert(T value)
    {
        BigO.Print("O(log n)",
            "The value is placed at the end (O(1)), then bubbled up toward the root. " +
            "Each swap moves one level up, and the tree has at most log₂(n) levels.");

        EnsureCapacity();
        int index = _count;
        _data[_count++] = value;
        Console.WriteLine($"\n[Insert] '{value}' placed at index {index}.");
        BubbleUp(index);
        PrintHeap("After Insert");
    }

    private void BubbleUp(int index)
    {
        Console.WriteLine($"         Bubble-UP from index {index}:");
        while (index > 0)
        {
            int parent = Parent(index);
            Console.Write($"           index={index} value='{_data[index]}'  parent={parent} value='{_data[parent]}': ");

            if (Dominates(index, parent))
            {
                Console.WriteLine($"swap ('{_data[index]}' dominates '{_data[parent]}')");
                Swap(index, parent);
                index = parent;
            }
            else
            {
                Console.WriteLine($"no swap — heap property satisfied.");
                break;
            }
        }
        if (index == 0)
            Console.WriteLine($"           Reached root at index 0.");
    }

    // ── ExtractTop ────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes and returns the root (the dominant value).
    /// Replaces the root with the LAST element, shrinks the count,
    /// then bubbles DOWN until the heap property is restored.  O(log n).
    /// </summary>
    public T ExtractTop()
    {
        if (_count == 0) throw new InvalidOperationException("Heap is empty.");

        BigO.Print("O(log n)",
            "The root is saved, the last element is moved to the root (O(1)), " +
            "then bubbled down by swapping with the more dominant child. At most log₂(n) swaps.");

        T top = _data[0];
        Console.WriteLine($"\n[ExtractTop] Removing root '{top}'.");
        Console.WriteLine($"             Moving last element '{_data[_count - 1]}' to root (index 0).");

        _data[0] = _data[_count - 1];
        _data[_count - 1] = default!;
        _count--;

        if (_count > 0)
            BubbleDown(0);

        Console.WriteLine($"             Extracted: '{top}'. Count={_count}.");
        PrintHeap("After ExtractTop");
        return top;
    }

    private void BubbleDown(int index)
    {
        Console.WriteLine($"         Bubble-DOWN from index {index}:");
        while (true)
        {
            int dominant = index;
            int left  = LeftChild(index);
            int right = RightChild(index);

            if (left  < _count && Dominates(left,  dominant)) dominant = left;
            if (right < _count && Dominates(right, dominant)) dominant = right;

            if (dominant == index)
            {
                Console.WriteLine($"           index={index} value='{_data[index]}' — no child dominates. Stop.");
                break;
            }

            Console.WriteLine($"           index={index} value='{_data[index]}' — dominant child is index={dominant} value='{_data[dominant]}'. Swap.");
            Swap(index, dominant);
            index = dominant;
        }
    }

    // ── Peek ──────────────────────────────────────────────────────────────────

    /// <summary>Returns the dominant value (min or max) without removing it — O(1).</summary>
    public T Peek()
    {
        if (_count == 0) throw new InvalidOperationException("Heap is empty.");
        BigO.Print("O(1)",
            "The root (index 0) is always the dominant value by the heap property. " +
            "No traversal needed — just read the array at position 0.");

        Console.WriteLine($"\n[Peek] Root value = '{_data[0]}' (not removed).");
        return _data[0];
    }

    // ── Contains ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Linear scan — O(n).
    /// The heap property gives us NO useful ordering between siblings,
    /// so we cannot prune any part of the search.
    /// </summary>
    public bool Contains(T value)
    {
        BigO.Print("O(n)",
            "The heap property only guarantees parent vs children — siblings are unordered. " +
            "There is no way to skip part of the search; every element must be checked.");

        Console.WriteLine($"\n[Contains] Scanning for '{value}'...");
        for (int i = 0; i < _count; i++)
        {
            Console.WriteLine($"           index {i}: '{_data[i]}'");
            if (_data[i].CompareTo(value) == 0)
            {
                Console.WriteLine($"           Found '{value}' at index {i}. ✓");
                return true;
            }
        }
        Console.WriteLine($"           '{value}' not found.");
        return false;
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    /// <summary>Resets count to 0 — O(1). Array is cleared to release references.</summary>
    public void Clear()
    {
        BigO.Print("O(n) to release references  /  O(1) logically",
            "Logically clearing is O(1) — just reset the count. " +
            "We also null out the array slots so the GC can collect any reference-type values.");

        Console.WriteLine($"\n[Clear] Clearing {_count} element(s).");
        Array.Clear(_data, 0, _count);
        _count = 0;
        Console.WriteLine("[Clear] Done.");
    }

    // ── Heap-sort demo ────────────────────────────────────────────────────────

    /// <summary>
    /// Demonstrates heap sort: extract every element in order.
    /// Calling ExtractTop() n times on a MinHeap yields ascending sorted output.
    /// Total cost: O(n log n).
    /// </summary>
    public T[] DrainSorted()
    {
        BigO.Print("O(n log n)",
            "Each of the n ExtractTop calls costs O(log n). " +
            "Draining a MinHeap gives ascending order; a MaxHeap gives descending order.");

        Console.WriteLine($"\n[DrainSorted] Extracting all {_count} elements in heap order...");
        var result = new T[_count];
        for (int i = 0; i < result.Length; i++)
            result[i] = ExtractTop();
        return result;
    }

    // ── Index arithmetic ──────────────────────────────────────────────────────

    private static int Parent(int i)     => (i - 1) / 2;
    private static int LeftChild(int i)  => 2 * i + 1;
    private static int RightChild(int i) => 2 * i + 2;

    /// <summary>Returns true if index a should be ABOVE index b (closer to root).</summary>
    private bool Dominates(int a, int b)
    {
        int cmp = _data[a].CompareTo(_data[b]);
        return _type == HeapType.Min ? cmp < 0 : cmp > 0;
    }

    private void Swap(int i, int j) => (_data[i], _data[j]) = (_data[j], _data[i]);

    private void EnsureCapacity()
    {
        if (_count < _data.Length) return;
        int newCap = _data.Length * 2;
        Console.WriteLine($"[DsHeap] Capacity full — growing {_data.Length} → {newCap}.");
        var newData = new T[newCap];
        Array.Copy(_data, newData, _count);
        _data = newData;
    }

    // ── Display ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Prints the flat array AND draws the tree structure so you can
    /// see exactly how array indices map to tree positions.
    /// </summary>
    public void PrintHeap(string label = "Heap")
    {
        Console.WriteLine($"\n  ── {label} ({_type}Heap, Count={_count}) ──");
        if (_count == 0) { Console.WriteLine("  (empty)"); return; }

        // Raw array
        Console.Write("  Array : [ ");
        for (int i = 0; i < _count; i++)
            Console.Write($"{_data[i]}{(i < _count - 1 ? " | " : "")}");
        Console.WriteLine(" ]");

        Console.Write("  Index : [ ");
        for (int i = 0; i < _count; i++)
            Console.Write($"{i}{(i < _count - 1 ? "   " : "")}");
        Console.WriteLine(" ]");

        // Tree — level by level
        Console.WriteLine("  Tree  :");
        int level = 0;
        int levelStart = 0;
        while (levelStart < _count)
        {
            int levelEnd   = Math.Min(levelStart + (1 << level), _count);
            int totalWidth = 80;
            int nodes      = levelEnd - levelStart;
            int spacing    = totalWidth / (nodes + 1);

            Console.Write("  ");
            for (int i = levelStart; i < levelEnd; i++)
            {
                string cell = $"[{i}]{_data[i]}";
                int pad = Math.Max(1, spacing - cell.Length);
                Console.Write(new string(' ', pad) + cell);
            }
            Console.WriteLine();

            levelStart = levelEnd;
            level++;
        }
        Console.WriteLine();
    }
}
