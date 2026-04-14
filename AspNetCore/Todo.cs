using System;

namespace AspNetCore
{
    public class Todo
    {
        public string Id { get; set; }
        
        public string Title { get; set; }
        
        public bool IsDone { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string UserId { get; set; }
    }
}
