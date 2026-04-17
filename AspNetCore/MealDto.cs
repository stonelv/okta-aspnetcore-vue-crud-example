using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore
{
    public class MealDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be between 1 and 100 characters", MinimumLength = 1)]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Calories is required")]
        [Range(1, 10000, ErrorMessage = "Calories must be between 1 and 10000")]
        public int Calories { get; set; }
        
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }
    }
}
