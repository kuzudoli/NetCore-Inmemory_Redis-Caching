﻿using NetCore_Inmemory_Redis_Caching.Models;

namespace NetCore_Inmemory_Redis_Caching.Services
{
    public interface IAppService
	{
		Todo GetTodo(int id);
		Task<List<Todo>> GetTodos();

		void RemoveFromCache(string key);
    }
}

