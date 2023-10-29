namespace MealPlannerBackend.Models
{
    public partial class Meal{
        
        public DateTime MealDate { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public int Id { get; set; }
    }
}