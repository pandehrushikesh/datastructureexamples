using ds.stack.queue.example;

// ════════════════════════════════════════════════════════════════════════════
//  PART 1 — STACK
// ════════════════════════════════════════════════════════════════════════════
Banner("Stack (LIFO) — Data Structure Demo");
Console.WriteLine("""
  LIFO = Last-In, First-Out.
  Think of a stack of plates — you always add and remove from the TOP.

  Backed by a singly-linked node chain.
  TOP of stack = head of chain (Push/Pop never need to walk the list).

    TOP  →  [C]
              ↓
             [B]
              ↓
             [A]  ← BOTTOM
              ↓
            null
""");

var stack = new DsStack<int>();

Section("1. Push — add elements onto the stack");
stack.Push(10);
stack.Push(20);
stack.Push(30);
stack.Push(40);

Section("2. Peek — look at the top without removing");
var top = stack.Peek();
Console.WriteLine($"  Peek returned: {top}");
stack.PrintStack("After Peek (unchanged)");

Section("3. Pop — remove elements one by one (LIFO order)");
Console.WriteLine($"  Popped: {stack.Pop()}");
Console.WriteLine($"  Popped: {stack.Pop()}");

Section("4. Contains");
stack.Contains(10);
stack.Contains(99);

Section("5. Push more, then Clear");
stack.Push(50);
stack.Push(60);
stack.PrintStack("Before Clear");
stack.Clear();
Console.WriteLine($"  IsEmpty={stack.IsEmpty}   Count={stack.Count}");

Section("6. Real-world analogy: Undo history");
Console.WriteLine("""
  Every time you type a character, it is Pushed onto the undo stack.
  Ctrl+Z Pops the last action — always undoing the MOST RECENT change first.
  This is exactly LIFO behaviour.
""");

// ════════════════════════════════════════════════════════════════════════════
//  PART 2 — QUEUE
// ════════════════════════════════════════════════════════════════════════════
Banner("Queue (FIFO) — Data Structure Demo");
Console.WriteLine("""
  FIFO = First-In, First-Out.
  Think of a checkout queue — the person who joined first is served first.

  Backed by a singly-linked node chain with HEAD (front) and TAIL (back) pointers.
  Enqueue at the BACK (tail), Dequeue from the FRONT (head) — both O(1).

  FRONT (head)                     BACK (tail)
    [A] → [B] → [C] → [D] → null
     ↑                        ↑
  Dequeue                  Enqueue
""");

var queue = new DsQueue<int>();

Section("1. Enqueue — add elements to the back");
queue.Enqueue(10);
queue.Enqueue(20);
queue.Enqueue(30);
queue.Enqueue(40);

Section("2. Peek — look at the front without removing");
var front = queue.Peek();
Console.WriteLine($"  Peek returned: {front}");
queue.PrintQueue("After Peek (unchanged)");

Section("3. Dequeue — remove elements one by one (FIFO order)");
Console.WriteLine($"  Dequeued: {queue.Dequeue()}");
Console.WriteLine($"  Dequeued: {queue.Dequeue()}");

Section("4. Contains");
queue.Contains(30);
queue.Contains(99);

Section("5. Enqueue more, then Clear");
queue.Enqueue(50);
queue.Enqueue(60);
queue.PrintQueue("Before Clear");
queue.Clear();
Console.WriteLine($"  IsEmpty={queue.IsEmpty}   Count={queue.Count}");

Section("6. Real-world analogy: Print spooler / task scheduler");
Console.WriteLine("""
  When you send documents to a printer, each job is Enqueued.
  The printer processes them in the order they arrived — FIFO.
  A task scheduler works the same way: the oldest waiting task runs next.
""");

// ════════════════════════════════════════════════════════════════════════════
//  SUMMARY — Stack vs Queue
// ════════════════════════════════════════════════════════════════════════════
Banner("Stack vs Queue — Big O Summary");
Console.WriteLine("""
  ┌─────────────────┬────────────┬───────────────────────────────────────────────┐
  │ Operation       │ Complexity │ Notes                                         │
  ├─────────────────┼────────────┼───────────────────────────────────────────────┤
  │ Stack.Push      │   O(1)     │ New node at head — no traversal               │
  │ Stack.Pop       │   O(1)     │ Head advanced one step — no traversal         │
  │ Stack.Peek      │   O(1)     │ Read head value — no removal                  │
  │ Stack.Contains  │   O(n)     │ Linear scan top → bottom                      │
  │ Stack.Clear     │   O(1)     │ Drop head reference — GC collects chain       │
  ├─────────────────┼────────────┼───────────────────────────────────────────────┤
  │ Queue.Enqueue   │   O(1)     │ Append at tail — tail pointer avoids O(n)     │
  │ Queue.Dequeue   │   O(1)     │ Head advanced one step — no traversal         │
  │ Queue.Peek      │   O(1)     │ Read head value — no removal                  │
  │ Queue.Contains  │   O(n)     │ Linear scan front → back                      │
  │ Queue.Clear     │   O(1)     │ Drop head + tail — GC collects chain          │
  └─────────────────┴────────────┴───────────────────────────────────────────────┘

  Key difference: ordering of removal.
    Stack → most recently added item leaves first  (LIFO)
    Queue → oldest item leaves first               (FIFO)

  Both use a linked node chain internally.
  The Queue needs a tail pointer so Enqueue stays O(1).
  The Stack only needs a head pointer — Push/Pop always happen at the top.
""");

Console.WriteLine("══════════════════════════════════════════════════════════");
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
