using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AWSGymWebsite.Data;
using AWSGymWebsite.Models;
using AWSGymWebsite.Areas.Identity.Data;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AWSGymWebsite.Controllers
{
    public class ViewerController : Controller
    {
        private readonly AWSGymWebsiteContext _context;
        private readonly AmazonSimpleNotificationServiceClient _snsClient;
        private readonly UserManager<AWSGymWebsiteUser> _userManager;
        private readonly string _topicArn; // Update with your SNS topic ARN

        public ViewerController(AWSGymWebsiteContext context, UserManager<AWSGymWebsiteUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;

            var awsAccessKeyId = configuration["AWS:AccessKeyId"];
            var awsSecretAccessKey = configuration["AWS:SecretAccessKey"];
            _topicArn = configuration["AWS:SnsTopicArn"];

            _snsClient = new AmazonSimpleNotificationServiceClient(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.USEast1);
        }

        private async Task<GymPage> GetGymPageFromDatabase()
        {
            try
            {
                // Replace the logic here with how you want to retrieve the GymPage from the database
                // For example, you might want to retrieve a specific GymPage based on some criteria.

                // For demonstration purposes, let's fetch the first GymPage from the database:
                List<GymPage> gymPage = await _context.GymPage.ToListAsync();

                return gymPage;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during database retrieval
                // For example, log the exception or return an error message to the user.
                // You might want to use a logger like Serilog, NLog, or log the exception to the database.

                // For simplicity, we will just return null in case of an error.
                // In a real application, you should handle the exception more gracefully.
                return null;
            }
        }

        public async Task<IActionResult> Index() //viewer can view all the gym 
        {
            List<GymPage> gymPage = await _context.GymPage.ToListAsync();

            return View(gymPage);
        }

        private List<string> getValues()
        {
            List<string> values = new List<string>();

            //1. link to appsettings.json and get back the values
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build(); //build the json file

            //2. read the info from json using configure instance
            values.Add(configure["Values:Key1"]);
            values.Add(configure["Values:Key2"]);
            values.Add(configure["Values:Key3"]);

            return values;
        }

        public Task<IActionResult> SubscribeGymOwner(int ownerId, int gymID)
        {
            return SubscribeGymOwner(ownerId, gymID, _context);
        }

        [HttpPost]
        public async Task<IActionResult> SubscribeGymOwner(int OwnerID, int ID, AWSGymWebsiteContext _context)
        {

            GymPage gymPage = await _context.GymPage.FindAsync(OwnerID); //if use await, post function async

            List<string> getKeys = getValues();

            //Find Topic based on GymID
            List<SNSTopic> topicresult = await _context.snstopic.Where(gymPage => gymPage.GymID == ID).ToListAsync();
            SNSTopic topic = topicresult.First();

            //Subscribe to Topic
            SubscribeRequest subscribeRequest = new SubscribeRequest(topic.TopicARN, "email", User.Identity.Name);
            SubscribeResponse emailSubscribeResponse = await _snsClient.SubscribeAsync(subscribeRequest);
            var emailRequestId = emailSubscribeResponse.ResponseMetadata.RequestId;

            //Add User to Subscriber Data
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            Subscriber newsub = new Subscriber();
            newsub.SubDate = DateTime.Now;
            newsub.UserID = int.Parse(user.Id);
            newsub.ID = ID;

            _context.subscriber.Add(newsub);
            await _context.SaveChangesAsync();

            // Redirect to a success page or another appropriate action
            return RedirectToAction("Index");
        }

        private void SendConfirmationEmail(string email)
        {
            // Implement email sending logic here
        }

        private async Task NotifyGymOwner(string ownerId, string viewerEmail)
        {
            List<string> getKeys = getValues();
            var snsClient = new AmazonSimpleNotificationServiceClient(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

            // Construct the notification message
            string message = $"New subscriber with email {viewerEmail}";

            // Publish the message to the SNS topic
            PublishRequest publishRequest = new PublishRequest
            {
                Message = message,
                TopicArn = "arn:aws:sns:us-east-1:YOUR_ACCOUNT_ID:YOUR_TOPIC_NAME"
            };

            try
            {
                PublishResponse response = await snsClient.PublishAsync(publishRequest);
                Console.WriteLine("Message published. MessageId: " + response.MessageId);
            }
            catch (Exception ex)
            {
                // Handle any exception that might occur during publishing
                Console.WriteLine("Error publishing message: " + ex.Message);
            }
        }

        public async Task<IActionResult> ViewGymDetails(int Gymid)
        {
            // Fetch the gym details using the 'id' parameter from the database
            var gym = await _context.GymPage.FirstOrDefaultAsync(g => g.ID == Gymid);

            if (gym == null)
            {
                // Handle the case where the gym with the specified ID is not found.
                return NotFound();
            }

            // Check if the viewer is subscribed to the gym
            var isSubscribed = HttpContext.Session.GetString("Subscribe") == "true";

            // Pass the gym details and subscription status to the view
            ViewBag.IsSubscribed = isSubscribed;
            return View(gym);
        }
    }
}