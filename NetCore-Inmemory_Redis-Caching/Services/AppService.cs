using Microsoft.Extensions.Caching.Memory;
using NetCore_Inmemory_Redis_Caching.Models;

namespace NetCore_Inmemory_Redis_Caching.Services
{
    public class AppService : IAppService
	{
        private readonly HttpClient _client;
        private readonly IMemoryCache _memCache;

        private readonly string TODOS_KEY = "TODOS";
        private readonly string TODO_DETAIL_KEY = "TODO_{0}";

        public AppService(IMemoryCache memCache)
        {
            _client = new HttpClient();
            _memCache = memCache;
        }

        public Todo GetTodo(int id)
        {
            //manuel getting and checking is data exist in cache
            var cacheData = _memCache.Get<Todo>(string.Format(TODO_DETAIL_KEY, id));
            if (cacheData != null)
                return cacheData;

            var todo = _client.GetFromJsonAsync<Todo>(string.Concat(Urls.BaseUrl, string.Format(Urls.GetById, id)), default).Result;
            if (todo == null)
                return new();
            
            //if not exist in cache it adds
            _memCache.Set(string.Format(TODO_DETAIL_KEY, id), todo, DateTime.Now.AddMinutes(2));

            return todo;
        }

        public async Task<List<Todo>> GetTodos()
        {
            #region old-version
            //if (_memCache.TryGetValue(TODOS_KEY, out List<Todo>? cachedata))
            //    return cachedata ?? new();

            //var todos = await _client.GetFromJsonAsync<List<Todo>>(string.Concat(Urls.BaseUrl, Urls.GetAll), default);
            //if (todos == null || !todos.Any())
            //    return new();

            //_memCache.Set(TODOS_KEY, todos, DateTime.Now.AddMinutes(2));
            #endregion

            //tries the get cache value and if it doesnt exist sets to the cache then return data
            var todos = await _memCache.GetOrCreateAsync(TODOS_KEY, async factory =>
            {
                //this section is works if requested data doesnt exist in cache

                //for testing set the breakpoint any line in this section...
                //...it doesnt work if cache data already exist

                //sliding and absolute expiration should be use together for prevent the old data loop
                factory.SlidingExpiration = TimeSpan.FromSeconds(20);
                factory.AbsoluteExpiration = DateTime.Now.AddMinutes(2);

                //when cache fully loaded it has to remove something...
                //...if you dont want to remove your important or commonly used data you should declare priority
                factory.Priority = CacheItemPriority.High;

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
            _memCache.Remove(key);
        }
    }
}