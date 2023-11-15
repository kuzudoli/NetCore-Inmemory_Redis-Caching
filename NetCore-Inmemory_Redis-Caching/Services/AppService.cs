using NetCore_Inmemory_Redis_Caching.Models;

namespace NetCore_Inmemory_Redis_Caching.Services
{
    public class AppService : IAppService
	{
        private readonly HttpClient _client;

        private readonly string TODOS_KEY = "TODOS";
        private readonly string TODO_DETAIL_KEY = "TODO_{0}";

        public AppService()
        {
            _client = new HttpClient();
        }

        public Todo GetTodo(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Todo>> GetTodos()
        {
            throw new NotImplementedException();
        }

        public void RemoveFromCache(string key)
        {
            throw new NotImplementedException();
        }
    }
}