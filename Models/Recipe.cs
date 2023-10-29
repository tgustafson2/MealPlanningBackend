namespace MealPlannerBackend.Models
{
    public partial class Recipe{
        public int UserId { get; set; }
        public string  Title { get; set; } = "";
        public string Ingredients { get; set; } = "";
        public string Directions { get; set; } = "";
        public int Id { get; set; } 
    }
}