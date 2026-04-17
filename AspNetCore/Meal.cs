using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore
{
    public class Meal
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "餐食名称不能为空")]
        [MaxLength(100, ErrorMessage = "餐食名称不能超过100个字符")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "卡路里不能为空")]
        [Range(1, 10000, ErrorMessage = "卡路里必须在1-10000之间")]
        public decimal Calories { get; set; }
        
        [Required(ErrorMessage = "日期不能为空")]
        public DateTime Date { get; set; }
    }
}
