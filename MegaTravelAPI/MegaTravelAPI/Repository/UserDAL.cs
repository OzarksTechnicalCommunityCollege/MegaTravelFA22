﻿using MegaTravelAPI.IRepository;
using MegaTravelAPI.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using System.Text;

namespace MegaTravelAPI.Data
{
    public class UserDAL : IUser
    {
        //context for the database connection
        private readonly MegaTravelContext context;

        private readonly IConfiguration _config;

        public UserDAL(MegaTravelContext Context, IConfiguration config)
        {
            context = Context;
            _config = config;
        }

        #region Get All Users Method
        /// <summary>
        /// Method that retrieves all users in the database
        /// </summary>
        /// <returns></returns>
        public List<User> GetAllUsers()
        {
            List<User> userList = new List<User>();

            try
            {
                //query the database to get all of the users
                var users = context.Users.ToList();

                foreach (var user in users)
                {
                    userList.Add(new User()
                    {
                        UserId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Street1 = user.Street1,
                        Street2 = user.Street2,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.ZipCode,
                        Phone = user.Phone

                    });
                }

                if(userList.Count == 0)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetAllUsers --- " + ex.Message);
                throw;
            }

            return userList;
        }

        #endregion

        #region Save User Record Method
        /// <summary>
        /// save the user registration record into database
        /// </summary>
        /// <param name="usermodel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<SaveUserResponse> SaveUserRecord(RegistrationModel usermodel)
        {
            //set up a new object to hold our user information
            User objApplicationUser;

            //set up a response status object to hold the response
            SaveUserResponse res = new SaveUserResponse();

            try
            {
                //use the incoming information to populate our new user
                objApplicationUser = new User
                {
                    FirstName = usermodel.FirstName,
                    LastName = usermodel.LastName,
                    Email = usermodel.Email,
                    Street1 = usermodel.Street1,
                    Street2 = usermodel.Street2,
                    City = usermodel.City,
                    State = usermodel.State,
                    ZipCode = usermodel.ZipCode,
                    Phone = usermodel.Phone


                };

                //populate the Login information
                objApplicationUser.LoginInfo = new Login
                {
                    UserName = usermodel.Username,
                    Password = usermodel.Password,
                    UserType = "User"
                };

                //get the ID of the most recent user added to the DB
                var highestID = context.Users.Max(x => x.UserId);

                //set the user ID to be the next ID value
                objApplicationUser.UserId = highestID + 1;

                //save this user in the database
                using(var db = new MegaTravelContext(_config))
                {
                    db.Users.Add(objApplicationUser);
                    db.SaveChanges();
                }

                //update the login table with the username and password
                context.Database.ExecuteSqlRaw("[dbo].[AddLogin] @ID, @USERNAME, @PASSWORD, @TYPE",
                            new SqlParameter("@ID", objApplicationUser.UserId),
                            new SqlParameter("@USERNAME", objApplicationUser.LoginInfo.UserName),
                            new SqlParameter("@PASSWORD", objApplicationUser.LoginInfo.Password),
                            new SqlParameter("@TYPE", objApplicationUser.LoginInfo.UserType));


                //set the success to pass the data back
                res.StatusCode = 200;
                res.Message = "Save Successful";
                res.Status = true;
                res.Data = objApplicationUser;
                res.UserId = objApplicationUser.UserId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveUserRecord --- " + ex.Message);
                res.Status = false;
                res.StatusCode = 0;
            }
            return res;
        }
        #endregion

        #region Login User Method
        public async Task<LoginResponse> LoginUser(LoginModel tokenData)
        {
            LoginResponse res = new LoginResponse();
            try
            {
                if(tokenData != null)
                {
                    //look for the user in the database
                    var query = context.Logins
                    .Where(x => x.UserName == tokenData.Username && x.Password == tokenData.Password)
                    .FirstOrDefault<Login>();

                    //if query has a result then we have a match
                    if(query != null)
                    {
                        res.Status = true;
                        res.StatusCode = 200;
                        res.Message = "Login Success";

                        //get the user data so we can send it back with
                        UserData user = await FindByName(query.UserName);
                        res.Data = new UserData
                        {
                            UserId = user.UserId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Street1 = user.Street1,
                            Street2 = user.Street2,
                            City = user.City,
                            State = user.State,
                            ZipCode = user.ZipCode,
                            Phone = user.Phone
                        };
                        return res;
                    }
                    else
                    {
                        //the user wasn't found or wasn't a match
                        res.Status = false;
                        res.StatusCode = 500;
                        res.Message = "Login Failed";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoginUser --- " + ex.Message);
                res.Status = false;
                res.StatusCode = 500;
            }

            return res;
        }
        #endregion

        #region Find User By Name Method

        public async Task<UserData> FindByName(string username)
        {
            UserData user = null;

            try
            {
                if (username != null)
                {
                    //query the database to find the user who has this username
                    var query = context.Users
                        .Where(x => x.LoginInfo.UserName == username)
                        .FirstOrDefault<User>();

                    if (query != null)
                    {
                        //set up the object so we can return it
                        user = new UserData
                        {
                            UserId = query.UserId,
                            FirstName = query.FirstName,
                            LastName = query.LastName,
                            Email = query.Email,
                            Street1 = query.Street1,
                            Street2 = query.Street2,
                            City = query.City,
                            State = query.State,
                            ZipCode = query.ZipCode,
                            Phone = query.Phone
                        };
                    }
                    
                }

                return user;


            }
            catch (Exception ex)
            {
                Console.WriteLine("FindByName --- " + ex.Message);
            }

            return user;
        }
        #endregion
        #region Update User Record Method
        /// <summary>
        /// Update the user database info
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public async Task<SaveUserResponse> UpdateUserRecord(UserData userInfo)
        {
            // create an instance of our sresponse object
            SaveUserResponse response = new SaveUserResponse();

            try
            {
                using (var db = new MegaTravelContext(_config))  // gives access to all tables
                {
                    // find the current user in the database
                    // entity framework
                    // linq query to find the user
                    var result = db.Users.SingleOrDefault(u => u.UserId == userInfo.UserId);
                    // check to see if we actually got a user
                    if (result != null)
                    {
                        // update the user data
                        result.FirstName = userInfo.FirstName;
                        result.LastName = userInfo.LastName;
                        result.Email = userInfo.Email;
                        result.Street1 = userInfo.Street1;
                        result.Street2 = userInfo.Street2;
                        result.City = userInfo.City;
                        result.State = userInfo.State;
                        result.ZipCode = userInfo.ZipCode;
                        result.Phone = userInfo.Phone;

                        // save this user in the database
                        db.SaveChanges();
                        // set the result and pass the data back
                        response.StatusCode = 200;
                        response.Message = "Update Successful";
                        response.Status = true;
                        response.Data = result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateUserRecord -- {ex.Message}");
                response.Status = false;
                response.StatusCode = 500;
            }
            return response;
        }
        #endregion




    }
}
