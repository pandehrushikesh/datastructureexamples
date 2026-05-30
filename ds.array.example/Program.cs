using ds.array.example;

Banner("Dynamic Array — Data Structure Demo");

// ── 1. Create and Add ────────────────────────────────────────────────────────
Section("1. Create array and Add elements");
var arr = new DynamicArray<int>();
arr.Add(30);
arr.Add(10);
arr.Add(50);
arr.Add(20);   // triggers capacity growth (DefaultCapacity = 4)
arr.Add(40);
arr.PrintState("After adds");

// ── 2. Insert ────────────────────────────────────────────────────────────────
Section("2. Insert at a specific index");
arr.Insert(0, 5);    // insert at the beginning
arr.Insert(3, 25);   // insert in the middle
arr.PrintState("After inserts");

// ── 3. Contains ──────────────────────────────────────────────────────────────
Section("3. Contains");
arr.Contains(25);
arr.Contains(99);

// ── 4. IsEmpty / Count ───────────────────────────────────────────────────────
Section("4. IsEmpty and Count");
Console.WriteLine($"IsEmpty = {arr.IsEmpty}");
Console.WriteLine($"Count   = {arr.Count}");

// ── 5. Remove ────────────────────────────────────────────────────────────────
Section("5. Remove by value and by index");
arr.Remove(25);
arr.PrintState("After Remove(25)");
arr.RemoveAt(0);
arr.PrintState("After RemoveAt(0)");

// ── 6. Bubble Sort ───────────────────────────────────────────────────────────
Section("6. Bubble Sort");
var bubbleArr = BuildDemo();
bubbleArr.BubbleSort();
bubbleArr.PrintState("Final");

// ── 7. Selection Sort ────────────────────────────────────────────────────────
Section("7. Selection Sort");
var selArr = BuildDemo();
selArr.SelectionSort();
selArr.PrintState("Final");

// ── 8. Merge Sort ────────────────────────────────────────────────────────────
Section("8. Merge Sort");
var mergeArr = BuildDemo();
mergeArr.MergeSort();
mergeArr.PrintState("Final");

Console.WriteLine("\n══════════════════════════════════════════════════════════");
Console.WriteLine("  Demo complete.");
Console.WriteLine("══════════════════════════════════════════════════════════");

// ── Helpers ──────────────────────────────────────────────────────────────────
static DynamicArray<int> BuildDemo()
{
    var a = new DynamicArray<int>();
    foreach (var v in new[] { 64, 25, 12, 22, 11 })
        a.Add(v);
    return a;
}

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
