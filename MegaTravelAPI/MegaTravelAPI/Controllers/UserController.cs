using Microsoft.AspNetCore.Mvc;
using MegaTravelAPI.Data;
using MegaTravelAPI.IRepository;
using MegaTravelAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace MegaTravelAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class UserController : ControllerBase
    {
        // private readonly IUser repository;
        private readonly IUser repository;

        //context for the database connection
        private readonly MegaTravelContext context;

        //variable for holding the configuration data for login authentication
        private IConfiguration config;

        public UserController(IConfiguration Config)
        {
            config = Config;
            context = new MegaTravelContext(config);
            repository = new UserDAL(context, config);
            

        }

        [HttpGet("GetUsers", Name = "GetUsers")]
        [AllowAnonymous]
        public async Task<GetUsersResponseModel> GetAllUsers()
        {
            GetUsersResponseModel response = new GetUsersResponseModel();

            //set up a list to hold the incoming users we will get from the db
            List<User> userList = new List<User>();
            try
            {
                userList = repository.GetAllUsers();

                //check the list isn't empty
                if(userList.Count != 0)
                {
                    response.Status = true;
                    response.StatusCode = 200;
                    response.userList = userList;
                }
                else
                {
                    //there has been an error
                    response.Status = false;
                    response.Message = "Get Failed";
                    response.StatusCode = 0;
                }

            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Get Failed";
                response.StatusCode = 0;
                //there has been an error
                Console.WriteLine(ex.Message);
            }
            return response;
        }


        

        [HttpPost("RegisterUser", Name = "RegisterUser")]
        [AllowAnonymous]
        public async Task<CreateUserResponseModel> RegisterUser(RegistrationModel userData)
        {
            CreateUserResponseModel response = new CreateUserResponseModel();

            if (userData != null)
            {
                try
                {
                    //call the method that will save the user record
                    var user = await repository.SaveUserRecord(userData).ConfigureAwait(true);

                    if (user.StatusCode == 200) //status success
                    {
                        //user has been added
                        
                        response.Status = true;
                        response.Message = "Registration Successful";
                        response.StatusCode = 200;

                        // send back the user information we just added
                        response.Data = new UserData();

                        response.Data.UserId = user.Data.UserId;
                        response.Data.FirstName = user.Data.FirstName;
                        response.Data.LastName = user.Data.LastName;
                        response.Data.Email = user.Data.Email;
                        response.Data.Phone = user.Data.Phone;
                        response.Data.Street1 = user.Data.Street1;
                        response.Data.Street2 = user.Data.Street2;
                        response.Data.City = user.Data.City;
                        response.Data.State = user.Data.State;
                        response.Data.ZipCode = user.Data.ZipCode;
                        response.Data.Phone = user.Data.Phone;
                        
                    }
                    else
                    {
                        //there has been an error
                        response.Status = false;
                        response.Message = "Registration Failed";
                        response.StatusCode = 0;
                        
                    }
                }
                catch (Exception ex)
                {
                    //there has been an error
                    response.Status = false;
                    response.Message = "Registration Failed";
                    response.StatusCode = 0;
                    Console.WriteLine(ex.Message);
                }
               

            }

            return response;
        }

        [HttpPost("LoginUser", Name = "LoginUser")]
        [AllowAnonymous]
        public async Task<LoginResponse> LoginUser(LoginModel tokenData)
        {
            LoginResponse response = new LoginResponse();
            try
            {
                if(tokenData != null)
                {
                    //call the method that will check the user credentials
                    var loginResult = await repository.LoginUser(tokenData).ConfigureAwait(true);

                    if(loginResult.StatusCode == 200)
                    {
                        //login check has succeeded

                        //query the database to get the information about our logged in user
                        var user = await repository.FindByName(tokenData.Username).ConfigureAwait(true);

                        if(user != null)
                        {
                            //generate the authentication token
                            var tokenString = GenerateJwtToken(tokenData);

                            response.StatusCode = 200;
                            response.Status = true;
                            response.Message = "Login Successful";
                            response.AccountType = tokenData.UserType;
                            response.Authtoken = tokenString;
                            response.Data = new UserData
                            {
                                UserId = user.UserId,
                                Email = user.Email,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Street1 = user.Street1,
                                Street2 = user.Street2,
                                City = user.City,
                                State = user.State,
                                ZipCode = user.ZipCode,
                                Phone = user.Phone
                            };

                            return response;
                        }                        
                    }
                    else
                    {
                        response.StatusCode = 500;
                        response.Status = false;
                        response.Message = "Login Failed";
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoginUser --- " + ex.Message);
                response.StatusCode = 500;
                response.Status = false;
                response.Message = "Login Failed";
            }

            return response;
        }

        /// <summary>
        /// Accept the request to update a user
        /// </summary>
        /// <param name="incomingUser"></param>
        /// <returns></returns>
        [HttpPost("UpdateUser", Name = "UpdateUser")]
        [AllowAnonymous]
        public async Task<CreateUserResponseModel> UpdateUser(UserData incomingUser)
        {
            // create an instance of our response
            CreateUserResponseModel response = new CreateUserResponseModel();

            // did we actually  get a user from the client?
            if (incomingUser != null)
            {
                try
                {
                    // call the method in the DAl that will update and save the record
                    var user = await repository.UpdateUserRecord(incomingUser).ConfigureAwait(true);

                    if (user.StatusCode == 200)
                    {
                        // user has been updated
                        response.Status = true;
                        response.Message = "Update Successful";
                        response.StatusCode = 200;
                        // send back the user information for the updated object
                        response.Data = new UserData();
                        response.Data.UserId = user.Data.UserId;
                        response.Data.FirstName = user.Data.FirstName;
                        response.Data.LastName = user.Data.LastName;
                        response.Data.Email = user.Data.Email;
                        response.Data.Phone = user.Data.Phone;
                        response.Data.Street1 = user.Data.Street1;
                        response.Data.Street2 = user.Data.Street2;
                        response.Data.City = user.Data.City;
                        response.Data.State = user.Data.State;
                        response.Data.ZipCode = user.Data.ZipCode;

                    }
                    else
                    {
                        // the object is null
                        response.Status = false;
                        response.Message = "Update failed";
                        response.StatusCode = 500;
                    }
                }
                catch (Exception ex)
                {
                    response.Status = false;
                    response.Message = "Update failed";
                    response.StatusCode = 500;
                    Console.WriteLine($"UpdateUser -- {ex.Message}");
                }
            }
            else
            {
                // the object is null
                response.Status = false;
                response.Message = "The Object is invalid";
                response.StatusCode = 500;
            }
            return response;
        }

        /// <summary>
        /// generate the token for registration
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        private string GenerateJwtToken(LoginModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenAuthentication:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.Username),
                //new Claim(JwtRegisteredClaimNames., userInfo.Password),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            DateTime expiredDate = DateTime.UtcNow.AddDays(15);
            var token = new JwtSecurityToken(config["TokenAuthentication:Issuer"],
                config["TokenAuthentication:Issuer"],
                claims,
                expires: expiredDate,
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}