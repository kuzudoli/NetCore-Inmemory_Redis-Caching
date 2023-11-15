using Microsoft.AspNetCore.Mvc;
using NetCore_Inmemory_Redis_Caching.Models;
using NetCore_Inmemory_Redis_Caching.Services;

namespace NetCore_Inmemory_Redis_Caching.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MemoryCacheController : Controller
    {
        private readonly IAppService _appService;

        public MemoryCacheController(IAppService appService)
        {
            _appService = appService;
        }

        [HttpGet("[action]/{id}")]
        public Todo GetTodo(int id)
        {
            return _appService.GetTodo(id);
        }

        [HttpGet("[action]")]
        public async Task<List<Todo>> GetTodos()
        {
            return await _appService.GetTodos();
        }

        [HttpGet("[action]")]
        public void RemoveFromCache(string key)
        {
            _appService.RemoveFromCache(key);
        }
    }
}
