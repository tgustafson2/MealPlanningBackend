using System.Data;
using MealPlannerBackend.Data;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPlannerBackend.Controllers{
    
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MealController : ControllerBase{
        
    }
}