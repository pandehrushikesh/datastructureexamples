namespace ds.hashtable.example;

/// <summary>
/// A key type whose GetHashCode() is fixed at compile time — immune to .NET's
/// per-process hash randomisation (DOTNET_GC_HASHTABLE_SEED).
///
/// Why this is needed for the collision demo:
///   .NET randomises string.GetHashCode() on every process start to prevent
///   hash-flooding attacks. This means we cannot predict which bucket any
///   string key will land in. Anagram strings (cat/act/tac) share characters
///   but are NOT hashed by summing char codes — so the "anagram = same bucket"
///   assumption commonly seen in naive demos is simply wrong.
///
/// Solution: override GetHashCode() with a hard-coded value so three different
/// keys always return 42, guaranteeing they always collide into the same bucket.
/// Equals is still Name-based (via record), so Get/Remove can distinguish them.
/// </summary>
public record CollisionKey(string Name)
{
    public override int GetHashCode() => Name switch
    {
        "Alpha" or "Beta" or "Gamma" => 42,   // same hash → GUARANTEED collision
        "Delta"                       => 99,   // different hash → different bucket
        _                             => Name.GetHashCode()
    };

    // record generates Equals() from Name automatically —
    // two CollisionKeys are equal only if their Names are equal,
    // even if they share a hash code. This is the correct contract:
    //   Two objects that are Equal MUST have the same hash code.
    //   Two objects with the same hash code are NOT necessarily Equal.

    public override string ToString() => Name;
}
