﻿namespace NetCore_Inmemory_Redis_Caching.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public bool Completed { get; set; }
    }
}
