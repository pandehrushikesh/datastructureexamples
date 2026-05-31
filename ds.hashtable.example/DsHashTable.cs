using ds.shared;

namespace ds.hashtable.example;

// ── Entry (chain node) ────────────────────────────────────────────────────────

/// <summary>
/// One key-value pair stored inside a bucket chain.
/// Each bucket is a singly-linked list of Entry nodes (separate chaining).
/// </summary>
internal class Entry<TKey, TValue>
{
    public TKey   Key;
    public TValue Value;
    public Entry<TKey, TValue>? Next;   // next entry in the same bucket

    public Entry(TKey key, TValue value)
    {
        Key   = key;
        Value = value;
    }

    public override string ToString() => $"({Key}→{Value})";
}

// ── Hash Table ────────────────────────────────────────────────────────────────

/// <summary>
/// Generic hash table using SEPARATE CHAINING for collision resolution.
/// Built from scratch — no Dictionary&lt;TKey,TValue&gt; or any BCL collection.
///
/// Internal layout (capacity = 7, 4 entries placed):
///
///  Bucket 0:  (apple→5) → null
///  Bucket 1:  null
///  Bucket 2:  (fig→9) → (kiwi→3) → null   ← collision! both hash to bucket 2
///  Bucket 3:  null
///  Bucket 4:  (mango→7) → null
///  Bucket 5:  null
///  Bucket 6:  null
///
/// Key concepts demonstrated:
///   • Hash code → bucket index  :  (key.GetHashCode() &amp; 0x7FFFFFFF) % capacity
///   • Collision resolution      :  each bucket is a linked list (separate chaining)
///   • Load factor               :  Count / Capacity  — triggers rehash at > 0.75
///   • Rehashing                 :  new larger prime capacity, every entry re-inserted
/// </summary>
public class DsHashTable<TKey, TValue> where TKey : notnull
{
    private Entry<TKey, TValue>?[] _buckets;
    private int _count;

    private const double LoadFactorThreshold = 0.75;

    // Primes are preferred as capacities because they reduce "clustering" —
    // when capacity shares factors with common hash patterns, many keys land
    // in the same bucket. A prime capacity has no common factors, so the
    // modulo spreads keys more evenly.
    private static readonly int[] Primes =
        [7, 17, 37, 79, 163, 331, 673, 1361, 2729, 5471, 10949, 21911, 43853];

    public int    Count        => _count;
    public int    Capacity     => _buckets.Length;
    public double LoadFactor   => (double)_count / _buckets.Length;
    public bool   IsEmpty      => _count == 0;

    public DsHashTable(int initialCapacity = 7)
    {
        int cap = GetNextPrime(initialCapacity);
        _buckets = new Entry<TKey, TValue>?[cap];
        Console.WriteLine($"[DsHashTable] Created. Capacity={cap} (prime), LoadFactorThreshold={LoadFactorThreshold:P0}.");
        PrintWhy("Prime capacity reduces clustering — a prime has no common factors with typical hash patterns,");
        PrintWhy("so (hashCode % prime) spreads keys more evenly across buckets.");
    }

    // ── Put (insert or update) ────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new key-value pair, or updates the value if the key already exists.
    /// O(1) average — O(n) worst case if all keys collide into one bucket.
    /// </summary>
    public void Put(TKey key, TValue value)
    {
        BigO.Print("O(1) average / O(n) worst case",
            "Hash code maps directly to a bucket index — no traversal of the whole table. " +
            "Worst case O(n) only if every key hashes to the same bucket (extremely rare with a good hash).");

        int    hashCode    = key.GetHashCode();
        int    safeHash    = hashCode & 0x7FFFFFFF;   // strip sign bit (avoids int.MinValue overflow)
        int    bucketIndex = safeHash % Capacity;

        Console.WriteLine($"\n[Put] Key='{key}'  Value='{value}'");
        PrintHashSteps(key, hashCode, safeHash, bucketIndex);

        var existing = FindEntry(bucketIndex, key);
        if (existing is not null)
        {
            Console.WriteLine($"      Key '{key}' already exists in bucket {bucketIndex} — UPDATING value '{existing.Value}' → '{value}'.");
            existing.Value = value;
            PrintBuckets();
            return;
        }

        // Check for collision before inserting
        if (_buckets[bucketIndex] is not null)
        {
            Console.WriteLine($"      ⚡ COLLISION! Bucket {bucketIndex} already has: {ChainString(bucketIndex)}");
            Console.WriteLine($"         Prepending '{key}' to the chain (separate chaining).");
        }
        else
        {
            Console.WriteLine($"      Bucket {bucketIndex} is empty — no collision.");
        }

        // Prepend to chain (O(1) — no need to walk to tail)
        var entry = new Entry<TKey, TValue>(key, value)
        {
            Next = _buckets[bucketIndex]
        };
        _buckets[bucketIndex] = entry;
        _count++;

        Console.WriteLine($"      Inserted. Count={_count}, LoadFactor={LoadFactor:F2} ({_count}/{Capacity}).");
        PrintBuckets();

        if (LoadFactor > LoadFactorThreshold)
            Rehash();
    }

    // ── Get ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the value for <paramref name="key"/>.
    /// Throws KeyNotFoundException if the key is absent.
    /// O(1) average.
    /// </summary>
    public TValue Get(TKey key)
    {
        BigO.Print("O(1) average / O(n) worst case",
            "The key is hashed to a bucket index in constant time, then only that bucket's " +
            "chain is searched — typically 0 or 1 extra nodes with a healthy load factor.");

        int hashCode    = key.GetHashCode();
        int safeHash    = hashCode & 0x7FFFFFFF;
        int bucketIndex = safeHash % Capacity;

        Console.WriteLine($"\n[Get] Key='{key}'");
        PrintHashSteps(key, hashCode, safeHash, bucketIndex);

        var entry = FindEntry(bucketIndex, key);
        if (entry is null)
            throw new KeyNotFoundException($"Key '{key}' not found in the hash table.");

        Console.WriteLine($"      Found '{key}' → '{entry.Value}' in bucket {bucketIndex}. ✓");
        return entry.Value;
    }

    // ── TryGet ────────────────────────────────────────────────────────────────

    /// <summary>Safe Get — returns false instead of throwing. O(1) average.</summary>
    public bool TryGet(TKey key, out TValue value)
    {
        BigO.Print("O(1) average / O(n) worst case",
            "Same as Get but returns false instead of throwing when the key is missing. " +
            "Preferred in code that must handle missing keys gracefully.");

        int bucketIndex = (key.GetHashCode() & 0x7FFFFFFF) % Capacity;
        Console.WriteLine($"\n[TryGet] Key='{key}' → bucket {bucketIndex}.");

        var entry = FindEntry(bucketIndex, key);
        if (entry is null)
        {
            Console.WriteLine($"         '{key}' not found. Returning false.");
            value = default!;
            return false;
        }
        Console.WriteLine($"         Found '{key}' → '{entry.Value}'. Returning true. ✓");
        value = entry.Value;
        return true;
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes the entry with <paramref name="key"/>.
    /// Returns true if found and removed, false if key was absent.
    /// O(1) average.
    /// </summary>
    public bool Remove(TKey key)
    {
        BigO.Print("O(1) average / O(n) worst case",
            "The bucket is located in O(1) via the hash. Within the chain, the entry is unlinked " +
            "using the same prev/current two-pointer technique as a linked list removal.");

        int hashCode    = key.GetHashCode();
        int safeHash    = hashCode & 0x7FFFFFFF;
        int bucketIndex = safeHash % Capacity;

        Console.WriteLine($"\n[Remove] Key='{key}'");
        PrintHashSteps(key, hashCode, safeHash, bucketIndex);

        Entry<TKey, TValue>? prev    = null;
        Entry<TKey, TValue>? current = _buckets[bucketIndex];

        while (current is not null)
        {
            Console.WriteLine($"         Checking chain entry {current}...");
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                if (prev is null)
                {
                    _buckets[bucketIndex] = current.Next;
                    Console.WriteLine($"         Was first in chain — bucket {bucketIndex} head → {current.Next?.ToString() ?? "null"}.");
                }
                else
                {
                    prev.Next = current.Next;
                    Console.WriteLine($"         Unlinked {current} — {prev}.Next → {current.Next?.ToString() ?? "null"}.");
                }
                _count--;
                Console.WriteLine($"         Removed. Count={_count}, LoadFactor={LoadFactor:F2}.");
                PrintBuckets();
                return true;
            }
            prev    = current;
            current = current.Next;
        }
        Console.WriteLine($"         Key '{key}' not found — nothing removed.");
        return false;
    }

    // ── ContainsKey ───────────────────────────────────────────────────────────

    /// <summary>O(1) average — hashes directly to the bucket, then scans its chain.</summary>
    public bool ContainsKey(TKey key)
    {
        BigO.Print("O(1) average / O(n) worst case",
            "The key is hashed to a single bucket; only that bucket's short chain is scanned. " +
            "The rest of the table is never touched.");

        int bucketIndex = (key.GetHashCode() & 0x7FFFFFFF) % Capacity;
        Console.WriteLine($"\n[ContainsKey] Key='{key}' → bucket {bucketIndex}.");

        var found = FindEntry(bucketIndex, key) is not null;
        Console.WriteLine($"              Result: {found}  {(found ? "✓" : "✗")}");
        return found;
    }

    // ── ContainsValue ─────────────────────────────────────────────────────────

    /// <summary>
    /// O(n) — values are not indexed so every bucket and every chain entry must be visited.
    /// This is the fundamental asymmetry: keys are O(1), values are O(n).
    /// </summary>
    public bool ContainsValue(TValue value)
    {
        BigO.Print("O(n)",
            "Values have no index — the entire table (all buckets, all chain entries) must be scanned. " +
            "This is why hash tables are designed around key lookups, not value lookups.");

        Console.WriteLine($"\n[ContainsValue] Scanning all buckets for value '{value}'...");
        for (int i = 0; i < Capacity; i++)
        {
            var current = _buckets[i];
            while (current is not null)
            {
                Console.WriteLine($"                Bucket {i}: checking {current}...");
                if (EqualityComparer<TValue>.Default.Equals(current.Value, value))
                {
                    Console.WriteLine($"                Found value '{value}' at key '{current.Key}' in bucket {i}. ✓");
                    return true;
                }
                current = current.Next;
            }
        }
        Console.WriteLine($"                Value '{value}' not found.");
        return false;
    }

    // ── Keys / Values ─────────────────────────────────────────────────────────

    /// <summary>Returns all keys — O(n) (walks every bucket and chain).</summary>
    public IEnumerable<TKey> Keys()
    {
        BigO.Print("O(n)", "Every bucket and every chain entry must be visited to collect all keys.");
        Console.WriteLine("\n[Keys] Enumerating all keys...");
        for (int i = 0; i < Capacity; i++)
        {
            var current = _buckets[i];
            while (current is not null)
            {
                Console.WriteLine($"       Bucket {i}: '{current.Key}'");
                yield return current.Key;
                current = current.Next;
            }
        }
    }

    /// <summary>Returns all values — O(n) (walks every bucket and chain).</summary>
    public IEnumerable<TValue> Values()
    {
        BigO.Print("O(n)", "Every bucket and every chain entry must be visited to collect all values.");
        Console.WriteLine("\n[Values] Enumerating all values...");
        for (int i = 0; i < Capacity; i++)
        {
            var current = _buckets[i];
            while (current is not null)
            {
                Console.WriteLine($"         Bucket {i}: '{current.Value}'");
                yield return current.Value;
                current = current.Next;
            }
        }
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets all buckets to null — O(n) to walk the bucket array, but each slot is a single
    /// null assignment regardless of how many entries were in its chain.
    /// </summary>
    public void Clear()
    {
        BigO.Print("O(n)",
            "The bucket array itself must be walked to null every slot — O(capacity). " +
            "Unlike a linked list Clear (O(1)), here we reset an array, not a single pointer.");

        Console.WriteLine($"\n[Clear] Nulling all {Capacity} bucket slots. GC collects all entry chains.");
        Array.Clear(_buckets, 0, _buckets.Length);
        _count = 0;
        Console.WriteLine($"[Clear] Done. Count={_count}.");
        PrintBuckets();
    }

    // ── Rehash ────────────────────────────────────────────────────────────────

    private void Rehash()
    {
        int oldCapacity = Capacity;
        int newCapacity = GetNextPrime(oldCapacity * 2);

        Console.WriteLine($"\n  ═══ REHASH ═══");
        Console.WriteLine($"  LoadFactor {LoadFactor:F2} exceeded threshold {LoadFactorThreshold:P0}.");
        Console.WriteLine($"  Growing capacity {oldCapacity} → {newCapacity} (next prime after {oldCapacity * 2}).");
        Console.WriteLine($"  Every entry must be re-hashed because (hashCode % capacity) changes with a new capacity.");

        var oldBuckets = _buckets;
        _buckets = new Entry<TKey, TValue>?[newCapacity];
        _count   = 0;

        for (int i = 0; i < oldBuckets.Length; i++)
        {
            var current = oldBuckets[i];
            while (current is not null)
            {
                int newSafeHash    = current.Key.GetHashCode() & 0x7FFFFFFF;
                int newBucketIndex = newSafeHash % newCapacity;
                Console.WriteLine($"  Re-inserting {current}: hashCode & 0x7FFFFFFF={newSafeHash} % {newCapacity} → bucket {newBucketIndex}.");

                var next  = current.Next;
                current.Next          = _buckets[newBucketIndex];
                _buckets[newBucketIndex] = current;
                _count++;
                current = next;
            }
        }

        Console.WriteLine($"  Rehash complete. New capacity={Capacity}, Count={_count}, LoadFactor={LoadFactor:F2}.");
        Console.WriteLine("  ════════════════");
        PrintBuckets();
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private Entry<TKey, TValue>? FindEntry(int bucketIndex, TKey key)
    {
        var current = _buckets[bucketIndex];
        while (current is not null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
                return current;
            current = current.Next;
        }
        return null;
    }

    // ── Display helpers ───────────────────────────────────────────────────────

    private void PrintHashSteps(TKey key, int hashCode, int safeHash, int bucketIndex)
    {
        Console.WriteLine($"      Step 1 — GetHashCode()          : {hashCode}");
        Console.WriteLine($"      Step 2 — & 0x7FFFFFFF (safe abs): {safeHash}");
        Console.WriteLine($"      Step 3 — % {Capacity,-4} (capacity)     : {safeHash} % {Capacity} = {bucketIndex}  ← bucket index");
    }

    private string ChainString(int bucketIndex)
    {
        var sb = new System.Text.StringBuilder();
        var current = _buckets[bucketIndex];
        while (current is not null)
        {
            sb.Append(current);
            if (current.Next is not null) sb.Append(" → ");
            current = current.Next;
        }
        return sb.ToString();
    }

    public void PrintBuckets()
    {
        Console.WriteLine($"\n  ┌── Bucket State (Capacity={Capacity}, Count={_count}, LoadFactor={LoadFactor:F2}) ──");
        for (int i = 0; i < Capacity; i++)
        {
            if (_buckets[i] is null)
                Console.WriteLine($"  │  [{i:D2}]  (empty)");
            else
                Console.WriteLine($"  │  [{i:D2}]  {ChainString(i)}");
        }
        Console.WriteLine($"  └──────────────────────────────────────────────────────");
    }

    private static void PrintWhy(string msg) => Console.WriteLine($"       ℹ  {msg}");

    private static int GetNextPrime(int min)
    {
        foreach (var p in Primes)
            if (p >= min) return p;
        // fallback for very large tables: find next odd number that is prime
        int candidate = (min % 2 == 0) ? min + 1 : min;
        while (!IsPrime(candidate)) candidate += 2;
        return candidate;
    }

    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (int i = 3; (long)i * i <= n; i += 2)
            if (n % i == 0) return false;
        return true;
    }
}
