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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeGymOwner(int ID)
        {

            //Get Gym Detail based on ID
            var gymPage = await _context.GymPage.FindAsync(ID); //if use await, post function async

            await _context.SaveChangesAsync();
            List<string> getKeys = getValues();
            var snsClient = new AmazonSimpleNotificationServiceClient(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

            //Find Topic based on GymID
            List<SNSTopic> topicresult = await _context.snstopic.Where(gymPage => gymPage.GymID == ID).ToListAsync();
            SNSTopic topic = topicresult.First();

            //Subscribe to Topic
            SubscribeRequest subscribeRequest = new SubscribeRequest(topic.TopicARN, "email", User.Identity.Name);
            SubscribeResponse emailSubscribeResponse = await snsClient.SubscribeAsync(subscribeRequest);
            var emailRequestId = emailSubscribeResponse.ResponseMetadata.RequestId;

            //Add User to Subscriber Data
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            Subscriber newsub = new Subscriber();
            newsub.SubDate = DateTime.Now;
            newsub.UserID = user.Id;
            newsub.SubARN = emailSubscribeResponse.SubscriptionArn;
            newsub.GymID = ID;

            _context.subscriber.Add(newsub);
            await _context.SaveChangesAsync();

            // Redirect to a success page or another appropriate action
            ViewData["Subscribe"] = "true";
            return Redirect("/Viewer/ViewGymDetails?Gymid="+ID);
        }


        private async Task NotifyGymOwner(string topicArn, string viewerEmail)
        {
            List<string> getKeys = getValues();
            var snsClient = new AmazonSimpleNotificationServiceClient(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

            // Construct the notification message
            string message = $"New subscriber with email {viewerEmail}";

            // Publish the message to the specified SNS topic
            PublishRequest publishRequest = new PublishRequest
            {
                Message = message,
                TopicArn = topicArn
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

        public async Task<IActionResult> ViewSubscribedGyms()
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            // Retrieve the list of subscribed gyms for the user
  

            var subscriptions = await (from s in _context.subscriber
                            join g in _context.GymPage on s.GymID equals g.ID
                            where s.UserID == user.Id
                            select new GymPage
                            {
                                ID = g.ID,
                                OwnerID = g.OwnerID,
                                GymName = g.GymName,
                                GymLocation = g.GymLocation,
                                ClosingTime = g.ClosingTime,   
                                OpeningTime = g.OpeningTime,    
                                ContactNumber = g.ContactNumber,
                                Details = g.Details,
                                ImgURL = g.ImgURL,  
                                S3Key = g.S3Key,
                                viewer = g.viewer
                            }).ToListAsync();

            return View(subscriptions); // Pass the subscriptions to the view
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
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            List<Subscriber> subresult = await _context.subscriber.Where(gym => gym.GymID == Gymid && gym.UserID == user.Id).ToListAsync();

            gym.viewer = gym.viewer + 1;
            await _context.SaveChangesAsync();

            if (subresult.Count >= 1) { 
                ViewData["Subscribe"] = "true";
            }
            else
            {
                ViewData["Subscribe"] = "false";
            }
        
            return View(gym);
        }
    }
}