using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AWSGymWebsite.Data;
using AWSGymWebsite.Models;
using AWSGymWebsite.Areas.Identity.Data;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.EntityFrameworkCore;
using AWSGymWebsite.Data;
using AWSGymWebsite.Models;
using AWSGymWebsite.Areas.Identity.Data;


namespace AWSGymWebsite.Controllers;

public class ViewerController : Controller
{
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly string _topicArn = "YOUR_TOPIC_ARN"; // need change the topic_arn

    public ViewerController(AmazonSimpleNotificationServiceClient snsClient)
    {
        _snsClient = snsClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
public IActionResult SubscribeGymOwner(string ownerId, string viewerEmail)
{
    // ... Your existing code ...

    // Send SNS subscription request to the gym owner
    var subscriptionRequest = new SubscribeRequest
    {
        TopicArn = _topicArn,
        Protocol = "email",
        Endpoint = "owner@email.com" // Replace with the actual owner's email address
    };

    try
    {
        var subscriptionResponse = _snsClient.Subscribe(subscriptionRequest);
        // You can handle the subscriptionResponse if needed, e.g., check for SubscriptionArn.
        if (!string.IsNullOrEmpty(subscriptionResponse.SubscriptionArn))
        {
            SendConfirmationEmail(viewerEmail);
            NotifyGymOwner(ownerId, viewerEmail);
            return RedirectToAction("SubscriptionSuccess", "Gym");
        }
        else
        {
            return RedirectToAction("SubscriptionError", "Gym");
        }
    }
    catch (Exception ex)
    {

        return RedirectToAction("SubscriptionError", "Gym");
    }
}

private void SendConfirmationEmail(string email)
{
    // Implement your logic to send a confirmation email to the viewer.
    // You can use libraries like SendGrid, MailKit, or other email services.
    // For brevity, I'll leave the implementation details out.
}

private void NotifyGymOwner(string ownerId, string viewerEmail)
{
    // Implement your logic to notify the gym owner about the new subscriber.
    // You can send an email, SMS, or use any other notification method based on your requirements.
    // For brevity, I'll leave the implementation details out.
}