namespace LRU.Helpers;

internal class LinkedNode<TKey, TValue>
{
    public LinkedNode<TKey, TValue> Next { get; set; }
    public LinkedNode<TKey, TValue> Prev { get; set; }
    public TKey Key { get; set; }
    public TValue Value { get; set; }
}