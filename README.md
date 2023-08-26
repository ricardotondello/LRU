# LRU

![Build status](https://github.com/ricardotondello/LRU/actions/workflows/dotnet.yml/badge.svg?branch=main)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=awesome-ric_lru&metric=alert_status)](https://sonarcloud.io/dashboard?id=awesome-ric_lru)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=awesome-ric_lru&metric=coverage)](https://sonarcloud.io/component_measures?id=awesome-ric_lru&metric=coverage)
[![NuGet latest version](https://badgen.net/nuget/v/LRU/latest)](https://nuget.org/packages/LRU)
[![NuGet downloads](https://img.shields.io/nuget/dt/LRU)](https://www.nuget.org/packages/LRU)

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

## Contributing

Contributions are welcome! If you find a bug or have a feature request, please open an issue on GitHub.
If you would like to contribute code, please fork the repository and submit a pull request.

## License

This Library is licensed under the MIT License. 
See [LICENSE](https://github.com/ricardotondello/LRU/blob/main/LICENSE) for more information.

## Support

<a href="https://www.buymeacoffee.com/ricardotondello" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>
