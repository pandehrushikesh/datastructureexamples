using ds.hashtable.example;

// ════════════════════════════════════════════════════════════════════════════
//  Hash Table — Data Structure Demo
// ════════════════════════════════════════════════════════════════════════════
Banner("Hash Table — Data Structure Demo");
Console.WriteLine("""
  A hash table maps KEYS to VALUES using a hash function to compute
  a bucket index — giving O(1) average-case lookup, insert, and delete.

  Internal structure (separate chaining):

    _buckets array
    ┌──────┐
    │ [0]  │──► (apple→5) → null
    │ [1]  │──► null
    │ [2]  │──► (fig→9) → (kiwi→3) → null   ← COLLISION: two keys, same bucket
    │ [3]  │──► null
    │ [4]  │──► (mango→7) → null
    │ [5]  │──► null
    │ [6]  │──► null
    └──────┘

  Key concepts you will see in this demo:
    1. How GetHashCode() + modulo maps a key to a bucket index
    2. Collisions handled by prepending to a per-bucket linked list (separate chaining)
    3. Load factor rising as entries are added
    4. Automatic rehash when load factor exceeds 0.75
    5. Why ContainsValue is O(n) while ContainsKey is O(1)
""");

// ────────────────────────────────────────────────────────────────────────────
Section("1. Create table and Put entries (watch bucket distribution)");
// ────────────────────────────────────────────────────────────────────────────
// Starting capacity = 7 (prime). With threshold 0.75, rehash fires at 6th entry.
var table = new DsHashTable<string, int>();

table.Put("apple",  10);
table.Put("banana", 20);
table.Put("cherry", 30);
table.Put("date",   40);
table.Put("elderberry", 50);
// ← 5 entries, LoadFactor ≈ 0.71 — still below threshold
table.Put("fig",    60);
// ← 6th entry pushes LoadFactor above 0.75 — REHASH triggered here

// ────────────────────────────────────────────────────────────────────────────
Section("2. Put — update an existing key (no new entry, no count change)");
// ────────────────────────────────────────────────────────────────────────────
table.Put("apple", 999);   // update, not insert

// ────────────────────────────────────────────────────────────────────────────
Section("3. Get — O(1) average key lookup");
// ────────────────────────────────────────────────────────────────────────────
var val = table.Get("banana");
Console.WriteLine($"  Get(\"banana\") returned: {val}");

// ────────────────────────────────────────────────────────────────────────────
Section("4. TryGet — safe lookup (no exception on missing key)");
// ────────────────────────────────────────────────────────────────────────────
if (table.TryGet("cherry", out int cherryVal))
    Console.WriteLine($"  TryGet(\"cherry\") → {cherryVal}");

table.TryGet("mango", out _);   // key does not exist

// ────────────────────────────────────────────────────────────────────────────
Section("5. ContainsKey — O(1) vs ContainsValue — O(n)");
// ────────────────────────────────────────────────────────────────────────────
table.ContainsKey("date");
table.ContainsKey("mango");      // absent

Console.WriteLine();
table.ContainsValue(30);
table.ContainsValue(999);        // this is 'apple' after update
table.ContainsValue(-1);         // absent

// ────────────────────────────────────────────────────────────────────────────
Section("6. Keys and Values — O(n) full enumeration");
// ────────────────────────────────────────────────────────────────────────────
var keys   = table.Keys().ToList();
var values = table.Values().ToList();
Console.WriteLine($"\n  All keys  : {string.Join(", ", keys)}");
Console.WriteLine($"  All values: {string.Join(", ", values)}");

// ────────────────────────────────────────────────────────────────────────────
Section("7. Remove — unlink an entry from its bucket chain");
// ────────────────────────────────────────────────────────────────────────────
table.Remove("banana");
table.Remove("mango");    // not present — should say so

// ────────────────────────────────────────────────────────────────────────────
Section("8. Collision demo — intentionally force two keys into the same bucket");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  We will create a fresh small table (capacity 7) and add keys whose
  hash codes happen to share the same bucket index, so you can see
  the chain grow and then be walked on lookup.
""");

// ── Why a custom key type? ────────────────────────────────────────────────────
// .NET enables hash randomisation by default (DOTNET_GC_HASHTABLE_SEED) to
// prevent hash-flooding attacks. This means string.GetHashCode() returns a
// DIFFERENT value on every process run — we cannot predict which bucket any
// string will land in.  Anagram strings (cat/act/tac) share the same characters
// but are NOT hashed by summing char codes; the claim in naive demos is wrong.
//
// Solution: a custom key type that overrides GetHashCode() with a fixed,
// deterministic value. We make three different keys all return the same hash
// so a collision is GUARANTEED every run — and you can reason about exactly
// which bucket they occupy.
// ─────────────────────────────────────────────────────────────────────────────
var small = new DsHashTable<CollisionKey, string>();
small.Put(new CollisionKey("Alpha"),  "first");
small.Put(new CollisionKey("Beta"),   "second");  // ← guaranteed collision with Alpha
small.Put(new CollisionKey("Gamma"),  "third");   // ← guaranteed collision with Alpha & Beta
small.Put(new CollisionKey("Delta"),  "fourth");  // lives in a different bucket (hash=99)

Console.WriteLine("\n  All three of Alpha/Beta/Gamma have GetHashCode()=42 — all land in the");
Console.WriteLine("  same bucket, forming a chain of length 3. Delta (hash=99) is elsewhere.");
Console.WriteLine("\n  Retrieve 'Gamma' — it is the THIRD entry in the chain, showing the walk:");
small.Get(new CollisionKey("Gamma"));

// ────────────────────────────────────────────────────────────────────────────
Section("9. Clear — reset the table");
// ────────────────────────────────────────────────────────────────────────────
table.Clear();
Console.WriteLine($"  IsEmpty={table.IsEmpty}   Count={table.Count}");

// ────────────────────────────────────────────────────────────────────────────
Banner("Big O Summary");
// ────────────────────────────────────────────────────────────────────────────
Console.WriteLine("""
  ┌───────────────────┬──────────────┬──────────────┬────────────────────────────────────────┐
  │ Operation         │   Average    │    Worst     │ Why                                    │
  ├───────────────────┼──────────────┼──────────────┼────────────────────────────────────────┤
  │ Put (insert)      │    O(1)      │    O(n)      │ Hash → bucket; prepend to chain        │
  │ Put (update)      │    O(1)      │    O(n)      │ Hash → bucket; scan chain for key      │
  │ Get               │    O(1)      │    O(n)      │ Hash → bucket; scan chain for key      │
  │ TryGet            │    O(1)      │    O(n)      │ Same as Get, no exception on miss      │
  │ Remove            │    O(1)      │    O(n)      │ Hash → bucket; unlink entry from chain │
  │ ContainsKey       │    O(1)      │    O(n)      │ Hash → bucket; scan chain              │
  │ ContainsValue     │    O(n)      │    O(n)      │ No value index — scan ALL buckets      │
  │ Keys / Values     │    O(n)      │    O(n)      │ Walk every bucket and every chain      │
  │ Clear             │  O(capacity) │  O(capacity) │ Null every slot in the bucket array    │
  │ Rehash            │    O(n)      │    O(n)      │ Re-insert every entry into new array   │
  └───────────────────┴──────────────┴──────────────┴────────────────────────────────────────┘

  Worst case O(n) arises only when ALL keys collide into ONE bucket —
  the table degenerates into a linked list. A good hash function and a
  healthy load factor (≤ 0.75) make this astronomically unlikely in practice.

  The "average O(1)" guarantee holds because:
    • GetHashCode() distributes keys evenly across buckets
    • Load factor kept below 0.75 means most buckets have 0 or 1 entries
    • Chain scans on collision are therefore very short on average
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
