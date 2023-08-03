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
using System;

namespace AWSGymWebsite.Controllers;

public class ViewerController : Controller
{
    private readonly AWSGymWebsiteContext _context;
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly UserManager<AWSGymWebsiteUser> _userManager;
    private readonly string _topicArn = "YOUR_TOPIC_ARN"; // need change the topic_arn
    private string emailRequest;

    public ViewerController(AmazonSimpleNotificationServiceClient snsClient, AWSGymWebsiteContext context, UserManager<AWSGymWebsiteUser> userManager)
    {
        _snsClient = snsClient;
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
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
    public async Task<IActionResult> SubscribeGymOwner(int ownerId, int gymID)
    {
        GymPage gymPage = await _context.GymPage.FindAsync(gymID); //if use await, post function async

        List<string> getKeys = getValues();
        var snsClient = new AmazonSimpleNotificationServiceClient(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

        //Find Topic based on GymID
        List<SNSTopic> topicresult = await _context.snstopic.Where(gymPage => gymPage.GymID == gymID).ToListAsync();
        SNSTopic topic = topicresult.First();

        //Subscribe to Topic
        SubscribeRequest subscribeRequest = new SubscribeRequest(topic.TopicARN, "email", User.Identity.Name);
        SubscribeResponse emailSubscribeResponse = await snsClient.SubscribeAsync(subscribeRequest);
        var emailRequestId = emailSubscribeResponse.ResponseMetadata.RequestId;

        //Add User to Subscriber Data
        var user = await _userManager.FindByEmailAsync(User.Identity.Name);
        Subscriber newsub = new Subscriber();
        newsub.SubDate = DateTime.Now;
        newsub.UserID = int.Parse(user.Id);
        newsub.GymID = gymID;

        _context.subscriber.Add(newsub);
        await _context.SaveChangesAsync();

        return Index();
    }
    private void SendConfirmationEmail(string email)
    {

    }

    private void NotifyGymOwner(string ownerId, string viewerEmail)
    {
    }   

    public async Task<IActionResult> ViewGymDetails(int id)
    {
    // Fetch the gym details using the 'id' parameter from the database
    var gym = _context.GymPage.FirstOrDefault(g => g.ID == id);

        //find gym inside subscriber data
        var gymsub = _context.subscriber.FirstOrDefault(gymall => gymall.GymID == id);
        // find if user exist in the gym subscription
        var user = await _userManager.FindByEmailAsync(User.Identity.Name);
        var sub = _context.subscriber.FirstOrDefault(gymall => gymall.UserID == int.Parse(user.Id));

        HttpContext.Session.SetString("Subscribe", "true");
        if (gym == null)
    {

        // Handle the case where the gym with the specified ID is not found.
        return NotFound();
    }

    // Pass the gym details to the view to display the information
    return View(gym);
    }
}
