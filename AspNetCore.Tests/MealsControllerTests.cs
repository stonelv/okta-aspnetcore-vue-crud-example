using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore;
using AspNetCore.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetCore.Tests
{
    public class MealsControllerTests
    {
        private readonly string _testUserId = "test-user-123";
        private readonly string _otherUserId = "other-user-456";

        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private MealsController CreateControllerWithUser(ApplicationDbContext context, string userId)
        {
            var controller = new MealsController(context);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            
            return controller;
        }

        #region Get Tests

        [Fact]
        public async Task Get_ShouldReturnOnlyUserOwnMeals()
        {
            var options = CreateNewContextOptions();
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.AddRange(new List<Meal>
                {
                    new Meal
                    {
                        Id = "meal-1",
                        Name = "User's Breakfast",
                        Calories = 350,
                        Date = DateTime.UtcNow,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "meal-2",
                        Name = "Other User's Lunch",
                        Calories = 500,
                        Date = DateTime.UtcNow,
                        UserId = _otherUserId
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get();
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<PagedResult<Meal>>>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Equal(1, apiResponse.Data.TotalCount);
                Assert.Single(apiResponse.Data.Items);
                Assert.Equal("User's Breakfast", apiResponse.Data.Items[0].Name);
            }
        }

        [Fact]
        public async Task Get_WithKeywordFilter_ShouldReturnFilteredMeals()
        {
            var options = CreateNewContextOptions();
            var today = DateTime.UtcNow.Date;
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.AddRange(new List<Meal>
                {
                    new Meal
                    {
                        Id = "meal-1",
                        Name = "早餐 - 燕麦粥",
                        Calories = 350,
                        Date = today,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "meal-2",
                        Name = "午餐 - 沙拉",
                        Calories = 450,
                        Date = today,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "meal-3",
                        Name = "晚餐 - 牛排",
                        Calories = 600,
                        Date = today,
                        UserId = _testUserId
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(keyword: "早餐");
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<PagedResult<Meal>>>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Equal(1, apiResponse.Data.TotalCount);
                Assert.Single(apiResponse.Data.Items);
                Assert.Equal("早餐 - 燕麦粥", apiResponse.Data.Items[0].Name);
            }
        }

        [Fact]
        public async Task Get_WithDateRangeFilter_ShouldReturnFilteredMeals()
        {
            var options = CreateNewContextOptions();
            var jan1 = new DateTime(2026, 1, 1);
            var jan15 = new DateTime(2026, 1, 15);
            var feb1 = new DateTime(2026, 2, 1);
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.AddRange(new List<Meal>
                {
                    new Meal
                    {
                        Id = "meal-1",
                        Name = "Jan 1 Meal",
                        Calories = 350,
                        Date = jan1,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "meal-2",
                        Name = "Jan 15 Meal",
                        Calories = 450,
                        Date = jan15,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "meal-3",
                        Name = "Feb 1 Meal",
                        Calories = 600,
                        Date = feb1,
                        UserId = _testUserId
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(startDate: jan1, endDate: jan15);
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<PagedResult<Meal>>>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Equal(2, apiResponse.Data.TotalCount);
                Assert.Equal(2, apiResponse.Data.Items.Count);
            }
        }

        [Fact]
        public async Task Get_WithPagination_ShouldReturnPaginatedResults()
        {
            var options = CreateNewContextOptions();
            var today = DateTime.UtcNow.Date;
            
            using (var context = new ApplicationDbContext(options))
            {
                for (int i = 1; i <= 25; i++)
                {
                    context.Meals.Add(new Meal
                    {
                        Id = $"meal-{i}",
                        Name = $"Meal {i}",
                        Calories = 100 * i,
                        Date = today.AddDays(-i),
                        UserId = _testUserId
                    });
                }
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(page: 2, pageSize: 10);
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<PagedResult<Meal>>>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Equal(25, apiResponse.Data.TotalCount);
                Assert.Equal(3, apiResponse.Data.TotalPages);
                Assert.Equal(2, apiResponse.Data.Page);
                Assert.Equal(10, apiResponse.Data.PageSize);
                Assert.Equal(10, apiResponse.Data.Items.Count);
            }
        }

        [Fact]
        public async Task Get_WithSortByCalories_ShouldReturnSortedResults()
        {
            var options = CreateNewContextOptions();
            var today = DateTime.UtcNow.Date;
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.AddRange(new List<Meal>
                {
                    new Meal
                    {
                        Id = "meal-1",
                        Name = "Low Calories",
                        Calories = 200,
                        Date = today,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "meal-2",
                        Name = "Medium Calories",
                        Calories = 500,
                        Date = today,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "meal-3",
                        Name = "High Calories",
                        Calories = 800,
                        Date = today,
                        UserId = _testUserId
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(sortBy: "Calories", sortDesc: false);
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<PagedResult<Meal>>>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Equal(3, apiResponse.Data.Items.Count);
                Assert.Equal(200, apiResponse.Data.Items[0].Calories);
                Assert.Equal(500, apiResponse.Data.Items[1].Calories);
                Assert.Equal(800, apiResponse.Data.Items[2].Calories);
            }
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ExistingMeal_ShouldReturnMeal()
        {
            var options = CreateNewContextOptions();
            const string mealId = "test-meal-id";
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.Add(new Meal
                {
                    Id = mealId,
                    Name = "Test Meal",
                    Calories = 450,
                    Date = DateTime.UtcNow,
                    UserId = _testUserId
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(mealId);
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<Meal>>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Equal(mealId, apiResponse.Data.Id);
                Assert.Equal("Test Meal", apiResponse.Data.Name);
                Assert.Equal(450, apiResponse.Data.Calories);
            }
        }

        [Fact]
        public async Task GetById_OtherUsersMeal_ShouldReturnNotFound()
        {
            var options = CreateNewContextOptions();
            const string otherMealId = "other-meal-id";
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.Add(new Meal
                {
                    Id = otherMealId,
                    Name = "Other User's Meal",
                    Calories = 500,
                    Date = DateTime.UtcNow,
                    UserId = _otherUserId
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get(otherMealId);
                
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<Meal>>(notFoundResult.Value);
                
                Assert.False(apiResponse.Success);
                Assert.Contains("Meal not found", apiResponse.Errors);
            }
        }

        [Fact]
        public async Task GetById_NonExistingMeal_ShouldReturnNotFound()
        {
            var options = CreateNewContextOptions();

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Get("non-existing-id");
                
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<Meal>>(notFoundResult.Value);
                
                Assert.False(apiResponse.Success);
                Assert.Contains("Meal not found", apiResponse.Errors);
            }
        }

        #endregion

        #region Post Tests

        [Fact]
        public async Task Post_ValidData_ShouldCreateMeal()
        {
            var options = CreateNewContextOptions();
            
            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var newMeal = new MealDto
                {
                    Name = "New Meal",
                    Calories = 350,
                    Date = DateTime.UtcNow
                };
                
                var result = await controller.Post(newMeal);
                
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<Meal>>(createdAtActionResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Equal("New Meal", apiResponse.Data.Name);
                Assert.Equal(350, apiResponse.Data.Calories);
                Assert.Equal(_testUserId, apiResponse.Data.UserId);
                Assert.NotNull(apiResponse.Data.Id);
            }

            using (var context = new ApplicationDbContext(options))
            {
                Assert.Equal(1, await context.Meals.CountAsync());
            }
        }

        [Fact]
        public async Task Post_EmptyName_ShouldReturnValidationError()
        {
            var options = CreateNewContextOptions();

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var invalidMeal = new MealDto
                {
                    Name = "",
                    Calories = 350,
                    Date = DateTime.UtcNow
                };
                
                controller.ModelState.AddModelError("Name", "Name is required");
                
                var result = await controller.Post(invalidMeal);
                
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<Meal>>(badRequestResult.Value);
                
                Assert.False(apiResponse.Success);
                Assert.Contains("Validation failed", apiResponse.Message);
            }
        }

        [Fact]
        public async Task Post_InvalidCalories_ShouldReturnValidationError()
        {
            var options = CreateNewContextOptions();

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var invalidMeal = new MealDto
                {
                    Name = "Test Meal",
                    Calories = 0,
                    Date = DateTime.UtcNow
                };
                
                controller.ModelState.AddModelError("Calories", "Calories must be between 1 and 10000");
                
                var result = await controller.Post(invalidMeal);
                
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse<Meal>>(badRequestResult.Value);
                
                Assert.False(apiResponse.Success);
            }
        }

        #endregion

        #region Put Tests

        [Fact]
        public async Task Put_ExistingMeal_ShouldUpdateMeal()
        {
            var options = CreateNewContextOptions();
            const string mealId = "meal-to-update";
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.Add(new Meal
                {
                    Id = mealId,
                    Name = "Original Name",
                    Calories = 300,
                    Date = new DateTime(2026, 1, 1),
                    UserId = _testUserId
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var updatedMeal = new MealDto
                {
                    Name = "Updated Name",
                    Calories = 500,
                    Date = new DateTime(2026, 1, 15)
                };
                
                var result = await controller.Put(mealId, updatedMeal);
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Contains("updated", apiResponse.Message);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var meal = await context.Meals.FindAsync(mealId);
                Assert.Equal("Updated Name", meal.Name);
                Assert.Equal(500, meal.Calories);
                Assert.Equal(new DateTime(2026, 1, 15), meal.Date);
            }
        }

        [Fact]
        public async Task Put_OtherUsersMeal_ShouldReturnNotFound()
        {
            var options = CreateNewContextOptions();
            const string otherMealId = "other-meal-id";
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.Add(new Meal
                {
                    Id = otherMealId,
                    Name = "Other User's Meal",
                    Calories = 500,
                    Date = DateTime.UtcNow,
                    UserId = _otherUserId
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var updatedMeal = new MealDto
                {
                    Name = "Updated",
                    Calories = 600,
                    Date = DateTime.UtcNow
                };
                
                var result = await controller.Put(otherMealId, updatedMeal);
                
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
                
                Assert.False(apiResponse.Success);
                Assert.Contains("Meal not found", apiResponse.Errors);
            }
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ExistingMeal_ShouldDeleteMeal()
        {
            var options = CreateNewContextOptions();
            const string mealId = "meal-to-delete";
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.Add(new Meal
                {
                    Id = mealId,
                    Name = "Meal to Delete",
                    Calories = 350,
                    Date = DateTime.UtcNow,
                    UserId = _testUserId
                });
                await context.SaveChangesAsync();
                Assert.Equal(1, await context.Meals.CountAsync());
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Delete(mealId);
                
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
                
                Assert.True(apiResponse.Success);
                Assert.Contains("deleted", apiResponse.Message);
            }

            using (var context = new ApplicationDbContext(options))
            {
                Assert.Equal(0, await context.Meals.CountAsync());
            }
        }

        [Fact]
        public async Task Delete_OtherUsersMeal_ShouldReturnNotFound()
        {
            var options = CreateNewContextOptions();
            const string otherMealId = "other-meal-id";
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.Add(new Meal
                {
                    Id = otherMealId,
                    Name = "Other User's Meal",
                    Calories = 500,
                    Date = DateTime.UtcNow,
                    UserId = _otherUserId
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Delete(otherMealId);
                
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
                
                Assert.False(apiResponse.Success);
                Assert.Contains("Meal not found", apiResponse.Errors);
            }

            using (var context = new ApplicationDbContext(options))
            {
                Assert.Equal(1, await context.Meals.CountAsync());
            }
        }

        [Fact]
        public async Task Delete_NonExistingMeal_ShouldReturnNotFound()
        {
            var options = CreateNewContextOptions();

            using (var context = new ApplicationDbContext(options))
            {
                var controller = CreateControllerWithUser(context, _testUserId);
                
                var result = await controller.Delete("non-existing-id");
                
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
                var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
                
                Assert.False(apiResponse.Success);
                Assert.Contains("Meal not found", apiResponse.Errors);
            }
        }

        #endregion

        #region Security Tests

        [Fact]
        public async Task AllOperations_ShouldIsolateUsers()
        {
            var options = CreateNewContextOptions();
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Meals.AddRange(new List<Meal>
                {
                    new Meal
                    {
                        Id = "user1-meal",
                        Name = "User 1's Meal",
                        Calories = 300,
                        Date = DateTime.UtcNow,
                        UserId = _testUserId
                    },
                    new Meal
                    {
                        Id = "user2-meal",
                        Name = "User 2's Meal",
                        Calories = 400,
                        Date = DateTime.UtcNow,
                        UserId = _otherUserId
                    }
                });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var user1Controller = CreateControllerWithUser(context, _testUserId);
                var user2Controller = CreateControllerWithUser(context, _otherUserId);

                var user1Result = await user1Controller.Get();
                var user1ApiResponse = Assert.IsType<ApiResponse<PagedResult<Meal>>>(
                    Assert.IsType<OkObjectResult>(user1Result.Result).Value);
                Assert.Equal(1, user1ApiResponse.Data.TotalCount);
                Assert.Equal("User 1's Meal", user1ApiResponse.Data.Items[0].Name);

                var user2Result = await user2Controller.Get();
                var user2ApiResponse = Assert.IsType<ApiResponse<PagedResult<Meal>>>(
                    Assert.IsType<OkObjectResult>(user2Result.Result).Value);
                Assert.Equal(1, user2ApiResponse.Data.TotalCount);
                Assert.Equal("User 2's Meal", user2ApiResponse.Data.Items[0].Name);
            }
        }

        #endregion
    }
}
