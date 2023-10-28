using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using MealPlannerBackend.Data;
using MealPlannerBackend.Dtos;
using MealPlannerBackend.Helpers;
using MealPlannerBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;


namespace MealPlannerBackend.Controllers{

    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class AuthController : ControllerBase{
        
        private readonly DatabaseContext _dapper;
        private readonly AuthHelper _authHelper;

        
        public AuthController(IConfiguration config){
            _dapper = new DatabaseContext(config);
            _authHelper = new AuthHelper(config);

        }

        [AllowAnonymous]
        [HttpPost("TestConnection")]
        public DateTime TestConnection(){
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegDto user){
            if (user.Password == user.PasswordConfirm){
                string sql = "SELECT Email from MealPlanning.UserAuth WHERE Email = '"+
                    user.Email+"'";
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sql);
                if(existingUsers.Count()==0){
                    UserForLoginDto setPassword = new UserForLoginDto(){
                        Email = user.Email,
                        Password = user.Password
                    };
                    if(_authHelper.SetPassword(setPassword)){

                        sql = @"EXEC MealPlanning.spUserInfoUpsert
                            @Email = @EmailParam,
                            @FirstName = @FirstNameParam,
                            @LastName = @LastNameParam";
                        DynamicParameters sqlParams = new DynamicParameters();

                        sqlParams.Add("@EmailParam", user.Email, DbType.String);
                        sqlParams.Add("@FirstNameParam", user.FirstName, DbType.String);
                        sqlParams.Add("@LastNameParam", user.LastName, DbType.String);
                        if(_dapper.ExecuteSqlWithParameters(sql,sqlParams)){
                            return Ok();
                        }
                        throw new Exception("Failed to add user");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User Email already exists");
            }
            throw new Exception("Passwords do not match");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto user){

            if(_authHelper.SetPassword(user)){
                return Ok();
            }
            throw new Exception("Failed to updated password");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto user){
            string sql = @"EXEC MealPlanning.spLoginConfirmGet
                @Email = @EmailParam";
            DynamicParameters sqlParams = new DynamicParameters();

            sqlParams.Add("@EmailParam", user.Email, DbType.String);

            UserForLoginConfirmDto userForConfirm = _dapper.LoadDataSingleWithParameters<UserForLoginConfirmDto>(sql,sqlParams);

            byte[] passHash = _authHelper.GetPasswordHash(user.Password,userForConfirm.PasswordSalt);

            for(int i=0; i<passHash.Length; i++){
                if(passHash[i]!= userForConfirm.PasswordHash[i]){
                    return StatusCode(401, "Incorrect password");
                }
            }

            string IdSql = @"SELECT Id from MealPlanning.UserInfo WHERE Email = '"+user.Email+"'";

            int userId = _dapper.LoadDataSingle<int>(IdSql);

            return Ok(new Dictionary<string,string> {
                {"token", _authHelper.CreateToken(userId)}
            });

        }

        [HttpGet("RefreshToken")]
        public string RefreshToken(){
            string sql = @"SELECT Id FROM MealPlanning.UserInfo WHERE Id = '"+ User.FindFirst("Id")?.Value + "'";

            int userId = _dapper.LoadDataSingle<int>(sql);
            return _authHelper.CreateToken(userId);
            
        }

    }
}
