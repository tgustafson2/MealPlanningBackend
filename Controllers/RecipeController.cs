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
    public class RecipeController : ControllerBase{
        private readonly DatabaseContext _dapper;
        public RecipeController(IConfiguration config){
            _dapper = new DatabaseContext(config);
        }

        [HttpPut("UpsertRecipe")]
        public IActionResult UpsertRecipe(Recipe recipe){
            string sql = @"EXEC MealPlanning.spRecipeUpsert
                @UserId = @UserIdParam,
                @Title = @TitleParma,
                @Ingredients = @IngredientsParam,
                @Directions = @DirectionsParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", this.User.FindFirst("Id")?.Value, DbType.Int32);
            sqlParams.Add("@TitleParam", recipe.Title, DbType.String);
            sqlParams.Add("@IngredientsParam", recipe.Ingredients, DbType.String);
            sqlParams.Add("@DirectionsParam", recipe.Directions, DbType.String);

            if(recipe.Id>0){
                sql += ", @Id = @IdParam";
                sqlParams.Add("@IdParam", recipe.Id, DbType.Int32);
            }
            if(_dapper.ExecuteSqlWithParameters(sql,sqlParams)){
                return Ok();
            }
            throw new Exception("Failed to upsert recipe");
        }
    }
}