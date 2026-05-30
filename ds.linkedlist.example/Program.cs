using ds.linkedlist.example;

// ════════════════════════════════════════════════════════════════════════════
//  PART 1 — SINGLY LINKED LIST
// ════════════════════════════════════════════════════════════════════════════
Banner("Singly Linked List — Data Structure Demo");
Console.WriteLine("""
  Structure:  Head → [A] → [B] → [C] → null
  Each node knows only its NEXT neighbour.
  Trade-off : traversal is one-direction only; no tail pointer so AddLast is O(n).
""");

var singly = new SinglyLinkedList<int>();

Section("1. AddLast — build the list");
singly.AddLast(10);
singly.AddLast(20);
singly.AddLast(30);
singly.AddLast(40);

Section("2. AddFirst — prepend at the head O(1)");
singly.AddFirst(5);

Section("3. InsertAfter — insert 25 after 20");
singly.InsertAfter(20, 25);

Section("4. Contains");
singly.Contains(25);
singly.Contains(99);

Section("5. Remove by value — remove 25");
singly.Remove(25);

Section("6. RemoveFirst — remove head O(1)");
singly.RemoveFirst();

Section("7. RemoveLast — walk to tail O(n)");
singly.RemoveLast();

Section("8. Reverse — rewire every Next pointer in-place O(n)");
singly.Reverse();

Section("9. Clear — O(1): just drop the head reference, GC does the rest");
singly.Clear();
Console.WriteLine($"  IsEmpty={singly.IsEmpty}   Count={singly.Count}");

// ════════════════════════════════════════════════════════════════════════════
//  PART 2 — DOUBLY LINKED LIST
// ════════════════════════════════════════════════════════════════════════════
Banner("Doubly Linked List — Data Structure Demo");
Console.WriteLine("""
  Structure:  null ← [A] ⇄ [B] ⇄ [C] → null
                      ↑               ↑
                     Head            Tail
  Each node knows both PREV and NEXT.
  Advantage : AddLast and RemoveLast are O(1); can traverse backwards.
""");

var doubly = new DoublyLinkedList<int>();

Section("1. AddLast — build the list (O(1) — we hold the tail!)");
doubly.AddLast(10);
doubly.AddLast(20);
doubly.AddLast(30);
doubly.AddLast(40);

Section("2. AddFirst — prepend O(1)");
doubly.AddFirst(5);

Section("3. InsertAfter — insert 25 after 20");
doubly.InsertAfter(20, 25);

Section("4. InsertBefore — insert 15 before 20");
doubly.InsertBefore(20, 15);

Section("5. Contains — forward scan");
doubly.Contains(25);
doubly.Contains(99);

Section("6. PrintReverse — walk backwards via Prev pointers (impossible in singly)");
doubly.PrintReverse();

Section("7. Remove by value — remove 25");
doubly.Remove(25);

Section("8. RemoveFirst — O(1) — update head and clear its Prev");
doubly.RemoveFirst();

Section("9. RemoveLast — O(1) — update tail and clear its Next (singly needs O(n) walk!)");
doubly.RemoveLast();

Section("10. Clear — O(1): null BOTH head and tail, or tail keeps the chain alive");
doubly.Clear();
Console.WriteLine($"  IsEmpty={doubly.IsEmpty}   Count={doubly.Count}");
doubly.PrintList("Final state");

Console.WriteLine("\n══════════════════════════════════════════════════════════");
Console.WriteLine("  Demo complete.");
Console.WriteLine("══════════════════════════════════════════════════════════");

// ── Helpers ──────────────────────────────────────────────────────────────────
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
