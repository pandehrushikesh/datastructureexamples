namespace ds.array.example;

/// <summary>
/// A generic dynamic array backed by a raw fixed-size array that grows automatically.
/// Intentionally avoids List&lt;T&gt; or any BCL collection — the goal is to understand
/// how a resizable array works under the hood.
/// </summary>
public class DynamicArray<T> where T : IComparable<T>
{
    private T[] _data;
    private int _count;
    private const int DefaultCapacity = 4;

    public int Count => _count;
    public int Capacity => _data.Length;
    public bool IsEmpty => _count == 0;

    public DynamicArray(int initialCapacity = DefaultCapacity)
    {
        _data = new T[initialCapacity];
        _count = 0;
        Console.WriteLine($"[DynamicArray] Created with initial capacity {initialCapacity}.");
    }

    // ── Indexer ──────────────────────────────────────────────────────────────

    public T this[int index]
    {
        get
        {
            GuardIndex(index);
            return _data[index];
        }
        set
        {
            GuardIndex(index);
            _data[index] = value;
        }
    }

    // ── Add / Insert ─────────────────────────────────────────────────────────

    /// <summary>Appends an element at the end — O(1) amortised.</summary>
    public void Add(T item)
    {
        Console.WriteLine($"\n[Add] Adding '{item}' to the end. Count={_count}, Capacity={Capacity}.");
        EnsureCapacity();
        _data[_count] = item;
        _count++;
        Console.WriteLine($"[Add] '{item}' placed at index {_count - 1}. Count is now {_count}.");
    }

    /// <summary>Inserts an element at <paramref name="index"/>, shifting elements right — O(n).</summary>
    public void Insert(int index, T item)
    {
        if (index < 0 || index > _count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range [0..{_count}].");

        Console.WriteLine($"\n[Insert] Inserting '{item}' at index {index}. Count={_count}, Capacity={Capacity}.");
        EnsureCapacity();

        // Shift elements right to make room
        Console.WriteLine($"[Insert] Shifting {_count - index} element(s) one position to the right...");
        for (int i = _count; i > index; i--)
        {
            Console.WriteLine($"         _data[{i}] = _data[{i - 1}]  ('{_data[i - 1]}')");
            _data[i] = _data[i - 1];
        }

        _data[index] = item;
        _count++;
        Console.WriteLine($"[Insert] '{item}' placed at index {index}. Count is now {_count}.");
    }

    // ── Remove ───────────────────────────────────────────────────────────────

    /// <summary>Removes the first occurrence of <paramref name="item"/> — O(n).</summary>
    public bool Remove(T item)
    {
        Console.WriteLine($"\n[Remove] Searching for '{item}'...");
        for (int i = 0; i < _count; i++)
        {
            if (_data[i].CompareTo(item) == 0)
            {
                Console.WriteLine($"[Remove] Found '{item}' at index {i}. Shifting elements left.");
                RemoveAt(i, printBanner: false);
                return true;
            }
            Console.WriteLine($"         Index {i}: '{_data[i]}' — no match.");
        }
        Console.WriteLine($"[Remove] '{item}' not found.");
        return false;
    }

    /// <summary>Removes the element at <paramref name="index"/>, shifting elements left — O(n).</summary>
    public void RemoveAt(int index, bool printBanner = true)
    {
        GuardIndex(index);
        if (printBanner)
            Console.WriteLine($"\n[RemoveAt] Removing element at index {index} ('{_data[index]}').");

        for (int i = index; i < _count - 1; i++)
        {
            Console.WriteLine($"         _data[{i}] = _data[{i + 1}]  ('{_data[i + 1]}')");
            _data[i] = _data[i + 1];
        }
        _data[_count - 1] = default!;   // release reference / clear value
        _count--;
        Console.WriteLine($"[RemoveAt] Done. Count is now {_count}.");
    }

    // ── Query ────────────────────────────────────────────────────────────────

    /// <summary>Returns true if <paramref name="item"/> is present — O(n) linear scan.</summary>
    public bool Contains(T item)
    {
        Console.WriteLine($"\n[Contains] Searching for '{item}'...");
        for (int i = 0; i < _count; i++)
        {
            Console.WriteLine($"           Index {i}: '{_data[i]}'");
            if (_data[i].CompareTo(item) == 0)
            {
                Console.WriteLine($"[Contains] Found '{item}' at index {i}. ✓");
                return true;
            }
        }
        Console.WriteLine($"[Contains] '{item}' not found.");
        return false;
    }

    // ── Sorting ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Bubble Sort — repeatedly steps through the list, compares adjacent elements
    /// and swaps them if they are in the wrong order. O(n²) time.
    /// </summary>
    public void BubbleSort()
    {
        Console.WriteLine("\n[BubbleSort] Starting...");
        PrintState("Before");

        for (int pass = 0; pass < _count - 1; pass++)
        {
            Console.WriteLine($"\n  Pass {pass + 1}:");
            bool swapped = false;
            for (int j = 0; j < _count - 1 - pass; j++)
            {
                Console.Write($"    Compare _data[{j}]='{_data[j]}' vs _data[{j + 1}]='{_data[j + 1]}': ");
                if (_data[j].CompareTo(_data[j + 1]) > 0)
                {
                    Console.WriteLine($"swap → '{_data[j + 1]}' before '{_data[j]}'");
                    Swap(j, j + 1);
                    swapped = true;
                }
                else
                {
                    Console.WriteLine("no swap");
                }
            }
            PrintState($"  After pass {pass + 1}");
            if (!swapped)
            {
                Console.WriteLine("  No swaps in this pass — array is sorted. Early exit.");
                break;
            }
        }
        Console.WriteLine("[BubbleSort] Done.");
    }

    /// <summary>
    /// Selection Sort — finds the minimum element in the unsorted portion and places it
    /// at the beginning, one by one. O(n²) time.
    /// </summary>
    public void SelectionSort()
    {
        Console.WriteLine("\n[SelectionSort] Starting...");
        PrintState("Before");

        for (int i = 0; i < _count - 1; i++)
        {
            int minIdx = i;
            Console.WriteLine($"\n  Round {i + 1}: Finding minimum in indices [{i}..{_count - 1}]");

            for (int j = i + 1; j < _count; j++)
            {
                Console.Write($"    _data[{j}]='{_data[j]}' vs current min _data[{minIdx}]='{_data[minIdx]}': ");
                if (_data[j].CompareTo(_data[minIdx]) < 0)
                {
                    minIdx = j;
                    Console.WriteLine($"new min → index {minIdx}");
                }
                else
                {
                    Console.WriteLine("no change");
                }
            }

            if (minIdx != i)
            {
                Console.WriteLine($"  Swap _data[{i}]='{_data[i]}' with _data[{minIdx}]='{_data[minIdx]}'");
                Swap(i, minIdx);
            }
            else
            {
                Console.WriteLine($"  _data[{i}] is already the minimum — no swap.");
            }
            PrintState($"  After round {i + 1}");
        }
        Console.WriteLine("[SelectionSort] Done.");
    }

    /// <summary>
    /// Merge Sort — divides the array in half, recursively sorts each half,
    /// then merges them. O(n log n) time, O(n) auxiliary space.
    /// </summary>
    public void MergeSort()
    {
        Console.WriteLine("\n[MergeSort] Starting...");
        PrintState("Before");
        MergeSortRecursive(0, _count - 1, depth: 0);
        Console.WriteLine("[MergeSort] Done.");
    }

    private void MergeSortRecursive(int left, int right, int depth)
    {
        string indent = new(' ', depth * 2);
        if (left >= right) return;

        int mid = (left + right) / 2;
        Console.WriteLine($"{indent}[MergeSort] Split [{left}..{right}] → [{left}..{mid}] and [{mid + 1}..{right}]");

        MergeSortRecursive(left, mid, depth + 1);
        MergeSortRecursive(mid + 1, right, depth + 1);
        Merge(left, mid, right, indent);
    }

    private void Merge(int left, int mid, int right, string indent)
    {
        int leftLen = mid - left + 1;
        int rightLen = right - mid;

        T[] leftArr = new T[leftLen];
        T[] rightArr = new T[rightLen];

        for (int i = 0; i < leftLen; i++) leftArr[i] = _data[left + i];
        for (int i = 0; i < rightLen; i++) rightArr[i] = _data[mid + 1 + i];

        Console.WriteLine($"{indent}[Merge] Merging left=[{string.Join(", ", leftArr)}] and right=[{string.Join(", ", rightArr)}]");

        int li = 0, ri = 0, k = left;
        while (li < leftLen && ri < rightLen)
        {
            if (leftArr[li].CompareTo(rightArr[ri]) <= 0)
            {
                Console.WriteLine($"{indent}  Pick '{leftArr[li]}' from left");
                _data[k++] = leftArr[li++];
            }
            else
            {
                Console.WriteLine($"{indent}  Pick '{rightArr[ri]}' from right");
                _data[k++] = rightArr[ri++];
            }
        }
        while (li < leftLen) { Console.WriteLine($"{indent}  Remaining from left: '{leftArr[li]}'"); _data[k++] = leftArr[li++]; }
        while (ri < rightLen) { Console.WriteLine($"{indent}  Remaining from right: '{rightArr[ri]}'"); _data[k++] = rightArr[ri++]; }

        Console.WriteLine($"{indent}[Merge] Result segment: [{string.Join(", ", _data[left..(right + 1)])}]");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    public void PrintState(string label = "State")
    {
        var elements = string.Join(", ", _data[.._count].Select(x => $"'{x}'"));
        Console.WriteLine($"  [{label}] [{elements}]  (Count={_count}, Capacity={Capacity})");
    }

    private void EnsureCapacity()
    {
        if (_count < _data.Length) return;

        int newCapacity = _data.Length * 2;
        Console.WriteLine($"[DynamicArray] Capacity full ({_data.Length}). Growing to {newCapacity}...");
        T[] newData = new T[newCapacity];
        for (int i = 0; i < _count; i++)
            newData[i] = _data[i];
        _data = newData;
        Console.WriteLine($"[DynamicArray] Internal array replaced. New capacity: {newCapacity}.");
    }

    private void Swap(int i, int j)
    {
        (_data[i], _data[j]) = (_data[j], _data[i]);
    }

    private void GuardIndex(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Count={_count}.");
    }
}
