using System.Data;
using Dapper;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
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
                @Title = @TitleParam,
                @Ingredients = @IngredientsParam,
                @Directions = @DirectionsParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
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

        [HttpGet("GetRecipes")]
        public IEnumerable<Recipe> GetRecipes(){
            string sql = @"EXEC MealPlanning.spRecipeGet 
                    @UserId = @UserIdParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
            IEnumerable<Recipe> recipes = _dapper.LoadDataWithParameters<Recipe>(sql,sqlParams);
            return recipes;
        }
        [HttpGet("GetRecipes/{title}/{ingrd}/{dir}")]
        public IEnumerable<Recipe> GetRecipes(string title = "none", string ingrd = "none", string dir = "none"){
            string sql = @"EXEC MealPlanning.spRecipeGet 
                    @UserId = @UserIdParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
            if(title.ToLower() != "none"){
                sql += ", @TitleSearch = @TitleParam";
                sqlParams.Add("@TitleParam", title, DbType.String);
            } 
            if(ingrd.ToLower() != "none"){
                sql += ", @IngredientsSearch = @IngrdParam";
                sqlParams.Add("@IngrdParam", ingrd, DbType.String);
            } 
            if(dir.ToLower() != "none"){
                sql += ", @DirectionsSearch = @DirParam";
                sqlParams.Add("@DirParam", dir, DbType.String);
            } 

            return _dapper.LoadDataWithParameters<Recipe>(sql, sqlParams);
        }
        [HttpGet("GetRecipes/{RecipeId}")]
        public Recipe GetRecipe(int id){
            string sql = @"EXEC MealPlanning.spRecipeGet
                        @UserId = @UserIdParam,
                        @Id = @IdParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@IdParam", id, DbType.Int32);
            return _dapper.LoadDataSingleWithParameters<Recipe>(sql,sqlParams);
        }
        [HttpDelete("Recipe/{recipeId}")]
        public IActionResult DeleteRecipe(int recipeId){
            string sql = @"EXEC MealPlanning.spRecipeDelete
                        @Id = @IdParam,
                        @UserId = @UserIdParam";
            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParams.Add("@IdParam", recipeId, DbType.Int32);

            if(_dapper.ExecuteSqlWithParameters(sql,sqlParams)){
                return Ok();
            }
            throw new Exception("Failed to delete recipe");
        }
    }
}