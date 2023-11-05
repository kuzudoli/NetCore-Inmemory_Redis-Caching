using Microsoft.Extensions.Caching.Memory;

namespace NetCore_Inmemory_Redis_Caching.Cache
{
    public class AppCustomCache
    {
        public MemoryCache Cache { get; } = new(new MemoryCacheOptions()
        {
            SizeLimit = 1024, //custom cache capacity
            TrackStatistics = true //must be true if track statistics
        });
    }
}

//MemoryCacheStatistics descriptions
/*
    currentEntryCount -> data count inside of cache
    currentEstimatedSize -> data size inside of cache
    totalMisses -> how many time not found data in cache
    totalHits -> how many time getted data from cache count
*/