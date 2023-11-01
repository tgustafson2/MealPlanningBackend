using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using MealPlannerBackend.Data;
using MealPlannerBackend.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using GemBox.Email;
using GemBox.Email.Smtp;

namespace MealPlannerBackend.Helpers{

    public class AuthHelper{
        
        private readonly IConfiguration _config;
        private readonly DatabaseContext _dapper;

        public AuthHelper(IConfiguration config){
            _dapper = new DatabaseContext(config);
            _config = config;
        }

        public byte[] GetPasswordHash(string password, byte[] salt){
            string  saltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(salt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(saltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 29000,
                numBytesRequested: 256/8
            );
        }

        public string CreateToken(int userId){
            Claim[] claims = new Claim[]{
                new Claim("userId", userId.ToString())
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    tokenKeyString != null ? tokenKeyString : ""
                )
            );

            SigningCredentials credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha512Signature
            );

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor(){
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);

    }

        public bool SetPassword(UserForLoginDto user){

            byte[] salt = new byte[128/8];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()){
                rng.GetNonZeroBytes(salt);
            }

            byte[] hash = GetPasswordHash(user.Password, salt);

            string sql = @"EXEC MealPlanning.spReg_Upsert
                @Email = @EmailParam,
                @PassHash = @hashParam,
                @PassSalt = @saltParam";

            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@EmailParam", user.Email, DbType.String);
            sqlParams.Add("hashParam", hash, DbType.Binary);
            sqlParams.Add("saltParam", salt, DbType.Binary);

            return _dapper.ExecuteSqlWithParameters(sql, sqlParams);
        }
        public bool CheckIfValid(string email){
            string license = _config.GetSection("AppSettings:GemBoxLicense").Value;
            ComponentInfo.SetLicense(license);
            
            MailAddressValidationResult result = MailAddressValidator.Validate(email);
            return result.Status.ToString() =="Ok";
        }
    }
}