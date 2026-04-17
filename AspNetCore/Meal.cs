using System;

namespace AspNetCore
{
    public class Meal
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public int Calories { get; set; }
        
        public DateTime Date { get; set; }
        
        public string UserId { get; set; }
    }
}
