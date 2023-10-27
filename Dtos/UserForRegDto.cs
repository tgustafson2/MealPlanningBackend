namespace MealPlannerBackend.Dtos
{
    public partial class UserForRegDto{
        public string Email { get; set; }  = "";
        public string Password {get; set;} = "";
        public string PasswordConfirm { get; set; } = "";
        public string FirstName { get; set; }  = "";
        public string LastName { get; set; } = "";
    }
}