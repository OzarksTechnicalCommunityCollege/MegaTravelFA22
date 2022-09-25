using Microsoft.AspNetCore.Mvc;
using MegaTravelClient.Models;
using MegaTravelClient.Pages.Home;
using MegaTravelClient.Utility;
using Newtonsoft.Json;

namespace MegaTravelClient.Controllers
{
    //[Authorize]
    public class UserController : Controller
    {
        public IActionResult Index(LoginResponseModel userData)
        {

            return View();
        }

        // tie our view to the controller
        public async Task<ActionResult> Edit(int id)
        {
            // call the API and get all of the user so we can get the object for the one user
            // that has this user ID
            User currentUser = null;
            GetUsersResponseModel responseModel = null;
            var serializedData = string.Empty;
            ServiceHelper service = new ServiceHelper();
            string response = await service.GetRequest(serializedData, ConstantValues.GetAllUsersAPI, false, string.Empty).ConfigureAwait(true);
            responseModel = JsonConvert.DeserializeObject<GetUsersResponseModel>(response);

            // loop through the list of users returned to find the one we want
            foreach (User user in responseModel.userList)
            {
                if (user.UserId == id)
                {
                    // this is the one we are looking for
                    currentUser = user;
                    break;
                }
            }
            // resturn the currentUser data to the view
            return View(currentUser);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(User updatedUser)
        {
            // Update the data in the database
            try
            {
                LoginResponseModel responseModel = null;
                // cannot send a string - must send a json object, so serialize as a json object
                var serializedData = JsonConvert.SerializeObject(updatedUser);
                // service helper class that understands how to format a get or post
                ServiceHelper service = new ServiceHelper();
                // connecting to the api via the endpoint, sending the json data, then receiving our response
                string response = await service.PostRequest(serializedData, ConstantValues.UpdateUser, false, string.Empty).ConfigureAwait(true);
                responseModel = JsonConvert.DeserializeObject<LoginResponseModel>(response);
                // did we get a rsponse?
                if (responseModel == null)
                {
                    // error with the API that caused a null return
                }
                else if (responseModel.Status = false)
                {
                    // an error occurred
                }
                else
                {
                    // all good- take the user back to the user dashboard and show the updated data
                    return View("Views/User/Index.cshtml", responseModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Edit API {ex.Message}");

            }
            return View();
        }
    }
}
