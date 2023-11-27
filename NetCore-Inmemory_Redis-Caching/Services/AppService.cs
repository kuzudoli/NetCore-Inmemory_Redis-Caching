using Microsoft.Extensions.Caching.Distributed;
using NetCore_Inmemory_Redis_Caching.Models;
using System.Text.Json;

namespace NetCore_Inmemory_Redis_Caching.Services
{
    public class AppService : IAppService
	{
        private readonly HttpClient _client;
        private readonly IDistributedCache _cache;

        private readonly string TODOS_KEY = "TODOS";
        private readonly string TODO_DETAIL_KEY = "TODO_{0}";

        public AppService(IDistributedCache cache)
        {
            _client = new HttpClient();
            _cache = cache;
        }

        public Todo GetTodo(int id)
        {
            var cacheData = _cache.GetString(string.Format(TODO_DETAIL_KEY, id));
            if (!string.IsNullOrEmpty(cacheData))
            {
                var jsonDoc = JsonDocument.Parse(cacheData);
                var todo = JsonSerializer.Deserialize<Todo>(jsonDoc);
                return todo;
            }

            var result = _client.GetFromJsonAsync<Todo>(string.Concat(Urls.BaseUrl, string.Format(Urls.GetById, id)), default).Result;
            if (result == null)
                return new();

            //options verilmezse absexp/sldexp değerlerine -1 verir yani sonsuz
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
                //AbsoluteExpirationRelativeToNow   ????
            };

            var serialized = JsonSerializer.Serialize(result);
            _cache.SetString(string.Format(TODO_DETAIL_KEY, id), serialized, cacheOptions);//hash tipinde kaydeder

            return result;
        }

        public async Task<List<Todo>> GetTodos()
        {
            var cacheData = await _cache.GetStringAsync(TODOS_KEY);
            if (!string.IsNullOrEmpty(cacheData))
            {
                var jsonDoc = JsonDocument.Parse(cacheData);
                var todos = JsonSerializer.Deserialize<List<Todo>>(jsonDoc);
                return todos;
            }

            var result = await _client.GetFromJsonAsync<List<Todo>>(string.Concat(Urls.BaseUrl, Urls.GetAll, default));
            if (result == null)
                return new();

            //options verilmezse absexp/sldexp değerlerine -1 verir yani sonsuz
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
                //AbsoluteExpirationRelativeToNow   ????
            };

            var serialized = JsonSerializer.Serialize(result);
            _cache.SetString(TODOS_KEY, serialized, cacheOptions);//hash tipinde kaydeder
            return result;
        }

        public void RemoveFromCache(string key)
        {
            _cache.Remove(key);
        }

        //Verilen key'deki cache'in sliding time ömrünü yeniler
        public void RefreshKey(string key)
        {
            _cache.Refresh(key);
        }
    }
}