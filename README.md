# LRU

![Build status](https://github.com/ricardotondello/LRU/actions/workflows/dotnet.yml/badge.svg?branch=main)

This is a thread safe caching strategy implementation of Least Recently Used (LRU). It defines the policy to evict elements from the cache to make room for new elements when the cache is full, meaning it discards the least recently used items first.

A Least Recently Used (LRU) Cache organizes items in order of use, allowing you to quickly identify which item hasn't been used for the longest amount of time.


## How to use it

```cs
    var capacity = 3; //It will keep 3 most used keys.
    var cache = new LeastRecentlyUsedCache<int, Client>(capacity); //Key and Value are generics

    var client1 = new Client() { Id = 1, Name = "client 1", Address = "Street 1" };
    cache.Add(1, client1);

    var client2 = new Client() { Id = 2, Name = "client 2", Address = "Street 2" };
    cache.Add(2, client2);

    var client3 = new Client() { Id = 3, Name = "client 3", Address = "Street 3" };
    cache.Add(3, client3);

    var client4 = new Client() { Id = 4, Name = "client 4", Address = "Street 4" };
    cache.Add(4, client4);

    var clientSearchId = 2;
    if (cache.TryGetValue(clientSearchId, out var clientSearch2))
    {
        Console.WriteLine($"{clientSearch2}"); //{ Id = 2, Name = "client 2", Address = "Street 2" }
    }
    
    var clientSearchIdNotOnCache = 1;
    if (!cache.TryGetValue(clientSearchIdNotOnCache, out var clientSearchNotOnCache))
    {
        Console.WriteLine($"Client with Id: {clientSearchIdNotOnCache} not found cache."); //Client with Id: 1 not found cache.
    }
```
