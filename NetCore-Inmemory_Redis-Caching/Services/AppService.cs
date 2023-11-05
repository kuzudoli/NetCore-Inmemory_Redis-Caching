using Microsoft.Extensions.Caching.Memory;
using NetCore_Inmemory_Redis_Caching.Cache;
using NetCore_Inmemory_Redis_Caching.Models;

namespace NetCore_Inmemory_Redis_Caching.Services
{
    public class AppService : IAppService
	{
        private readonly HttpClient _client;
        private readonly AppCustomCache _memCache;

        private readonly string TODOS_KEY = "TODOS";
        private readonly string TODO_DETAIL_KEY = "TODO_{0}";

        public AppService(AppCustomCache memCache)
        {
            _client = new HttpClient();
            _memCache = memCache;
        }

        public Todo GetTodo(int id)
        {
            //manuel getting and checking is data exist in cache
            var cacheData = _memCache.Cache.Get<Todo>(string.Format(TODO_DETAIL_KEY, id));
            if (cacheData != null)
                return cacheData;

            var todo = _client.GetFromJsonAsync<Todo>(string.Concat(Urls.BaseUrl, string.Format(Urls.GetById, id)), default).Result;
            if (todo == null)
                return new();

            //it saves in cache the data, absolute expiration time is 1 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(1),
                Size = 1 //must be declared entry size when using custom cache
            };
            _memCache.Cache.Set(string.Format(TODO_DETAIL_KEY, id), todo, cacheOptions);

            return todo;
        }

        public async Task<List<Todo>> GetTodos()
        {
            #region old-version
            //if (_memCache.Cache.TryGetValue(TODOS_KEY, out List<Todo>? cacheData))
            //    return cacheData ?? new();

            //var todos = await _client.GetFromJsonAsync<List<Todo>>(string.Concat(Urls.BaseUrl, Urls.GetAll), default);
            //if (todos == null || !todos.Any())
            //    return new();

            //var opt = new MemoryCacheEntryOptions()
            //{
            //    //cache expiration sliding 1 minute each request
            //    SlidingExpiration = TimeSpan.FromMinutes(1),
            //    //cache maximum lifetime is five minute cause cached data could be changed at db
            //    //so cached data should be update with new version
            //    AbsoluteExpiration = DateTime.Now.AddMinutes(5)
            //};
            //_memCache.Cache.Set(TODOS_KEY, todos, opt);
            #endregion

            //tries the get cache value and if it doesnt exist sets to the cache then return data
            var todos = await _memCache.Cache.GetOrCreateAsync(TODOS_KEY, async opt =>
            {
                //this section is works if requested data doesnt exist in cache

                //for testing set the breakpoint any line in this section...
                //...it doesnt work if cache data already exist

                //sliding and absolute expiration should be use together for prevent the old data loop
                opt.SlidingExpiration = TimeSpan.FromSeconds(20);
                opt.AbsoluteExpiration = DateTime.Now.AddMinutes(2);
                opt.Size = 1;
                //when cache fully loaded it has to remove something...
                //...if you dont want to remove your important or commonly used data you should declare priority
                opt.Priority = CacheItemPriority.High;

                var data = await _client.GetFromJsonAsync<List<Todo>>(string.Concat(Urls.BaseUrl, Urls.GetAll), default);
                if (data == null || !data.Any())
                    return new();

                return data;
            });

            if (todos == null || !todos.Any())
                return new();

            return todos;
        }

        public void RemoveFromCache(string key)
        {
            //removes key-value pair from cache
            _memCache.Cache.Remove(key);
        }

        //cleans cache (but not statistics)
        public void ClearCache()
        {
            _memCache.Cache.Clear();
        }

        //cleans a part of cache's follow these order:
        // 1. Remove all expired items.
        // 2. Bucket by CacheItemPriority.
        // 3. Least recently used objects.
        // ?. Items with the soonest absolute expiration.
        // ?. Items with the soonest sliding expiration.
        // ?. Larger objects - estimated by object graph size, inaccurate.
        public void CompactCache(double percantage)
        {
            _memCache.Cache.Compact(percantage);
        }

        //check cache stats with this method
        public MemoryCacheStatistics GetCacheStatistics()
        {
            return _memCache.Cache.GetCurrentStatistics()!;
        }

        
    }
}