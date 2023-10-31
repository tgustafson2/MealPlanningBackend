using System.Data;
using Dapper;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPlannerBackend.Controllers{
    
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MealController : ControllerBase{
        
        private readonly DatabaseContext _dapper;
        public MealController(IConfiguration config){
            _dapper = new DatabaseContext(config);
        }



        [HttpPut("UpsertMeal")]
        public IActionResult UpsertMeal(Meal meal){
            string sql = @"EXEC MealPlanning.spMealChoiceUpsert
                @MealDate = @MealDateParam,
                @UserId = @UserIdParam,
                @RecipeId = @RecipeIdParam";//@Id
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@MealDateParam", meal.MealDate, DbType.DateTime);
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@RecipeIdParam", meal.RecipeId, DbType.Int32);
            if(meal.Id>0){
                sql+=", @Id = @IdParam";
                sqlParams.Add("IdParam", meal.Id, DbType.Int32);
            }
            if(_dapper.ExecuteSqlWithParameters(sql,sqlParams)){
                return Ok();
            }
            throw new Exception("Failed to upsert meal");
        }
        [HttpDelete("DeleteMeal/{Id}")]
        public IActionResult DeleteMeal(int Id){
            string sql = @"EXEC MealPlanning.spMealChoiceDelete
                @Id = @IdParam,
                @UserId = @UserIdParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@IdParam", Id, DbType.Int32);
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);

            if(_dapper.ExecuteSqlWithParameters(sql,sqlParams)){
                return Ok();
            }
            throw new Exception("Failed to delete meal");
        }

        [HttpGet("GetMeals/{StartDate}/{EndDate}")]
        public IEnumerable<Meal> GetMeals(DateTime StartDate, DateTime EndDate){
            string sql = @"EXEC MealPlanning.spMealChoiceGet
                @UserId = @UserIdParam,
                @Start = @StartParam,
                @End = @EndParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("StartParam", StartDate, DbType.DateTime);
            sqlParams.Add("EndParam", EndDate, DbType.DateTime);
            return _dapper.LoadDataWithParameters<Meal>(sql,sqlParams);
        }
    }
}