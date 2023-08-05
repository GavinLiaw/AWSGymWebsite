using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AWSGymWebsite.Data;
using AWSGymWebsite.Models;
using AWSGymWebsite.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using System.Drawing;

namespace AWSGymWebsite.Controllers
{
    public class GymOwnerController : Controller
    {
        private readonly AWSGymWebsiteContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AWSGymWebsiteUser> _userManager;
        //Create variable for connection to DB
        public GymOwnerController(AWSGymWebsiteContext context, UserManager<AWSGymWebsiteUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private const string s3BucketName = "group40aws";

        //function extra: connection string to the AWS Account
        private List<string> getValues()
        {
            List<string> values = new List<string>();

            //1. link to appsettings.json and get back the values
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build(); //build the json file

            values.Add(configure["Values:Key1"]);
            values.Add(configure["Values:Key2"]);
            values.Add(configure["Values:Key3"]);

            return values;
        }
        

        // GET: GymOwner
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            return _context.GymPage != null ? 
                          View(await _context.GymPage.Where(g => g.OwnerID == userId).ToListAsync()) :
                          Problem("Entity set 'AWSGymWebsiteContext.GymPage'  is null.");
        }

        // GET: GymOwner/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.GymPage == null)
            {
                return NotFound();
            }

            var gymPage = await _context.GymPage
                .FirstOrDefaultAsync(m => m.ID == id);
            if (gymPage == null)
            {
                return NotFound();
            }

            return View(gymPage);
        }

        // GET: GymOwner/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GymOwner/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,OwnerID,GymName,GymLocation,ClosingTime,OpeningTime,ContactNumber,Details,ImgURL,S3Key,viewer")] GymPage gymPage, IFormFile imagefile)
        {  
            
            if (ModelState.IsValid)
            {

                if (imagefile == null || imagefile.Length <= 0)
                {
                    ModelState.AddModelError("imagefile", "Please select an image file.");
                    return View(gymPage);
                }

                // Get Credential Key
                List<string> getKeys = getValues();
                // Open S3
                var awsS3client = new AmazonS3Client(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);
                var newfilename = Guid.NewGuid() + "_" + imagefile.FileName;

                try
                {
                    //upload to S3
                    PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                    {
                        InputStream = imagefile.OpenReadStream(),
                        BucketName = s3BucketName,
                        Key = "img/" + newfilename,
                        CannedACL = S3CannedACL.PublicRead
                    };
                    await awsS3client.PutObjectAsync(uploadRequest);
                }
                catch (AmazonS3Exception ex)
                {
                    return BadRequest("Error: " + ex.Message);
                }

                gymPage.ImgURL = "https://" + s3BucketName + ".s3.amazonaws.com/img/" + newfilename;
                gymPage.S3Key = newfilename;
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;
                gymPage.OwnerID = userId;


                     _context.Add(gymPage);
                    await _context.SaveChangesAsync();


                try {
                    //Open SNS
                   var snsClient = new AmazonSimpleNotificationServiceClient(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);
                    
                    //Create New SNS Topic
                    string currentDate = DateTime.Now.ToString("yyyyMMdd");
                    string currentTime = DateTime.Now.ToString("HHmmss");
                    string newGymName = gymPage.ID.ToString().Replace(" ", "_");
                    string formattedGymName = $"{newGymName}_{currentDate}_{currentTime}";

                    var topicarn = await snsClient.CreateTopicAsync(formattedGymName);

                    //Save SNS to GymID
                    SNSTopic newtopic = new SNSTopic();
                    newtopic.TopicARN = topicarn.TopicArn;
                    int id = gymPage.ID;
                    newtopic.GymID = id;

                    _context.Add(newtopic);
                    await _context.SaveChangesAsync();
                } catch (AmazonSimpleNotificationServiceException snsEx)
                {
                    return BadRequest("SNS Error: " + snsEx.Message);
                }
                    return RedirectToAction(nameof(Index));
            }

            return View(gymPage);

            
            
        }

        // GET: GymOwner/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GymPage == null)
            {
                return NotFound();
            }

            var gymPage = await _context.GymPage.FindAsync(id);
            if (gymPage == null)
            {
                return NotFound();
            }
            return View(gymPage);
        }

        // POST: GymOwner/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,OwnerID,GymName,GymLocation,ClosingTime,OpeningTime,ContactNumber,Details,ImgURL,S3Key,viewer")] GymPage gymPage, IFormFile imagefile)
        {
            if (ModelState.IsValid)
            {
                if (id != gymPage.ID)
                {
                    return NotFound();
                }

                if (imagefile != null)
                {
                    List<string> getKeys = getValues();
                    var awsS3client = new AmazonS3Client(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

                    var newfilename = Guid.NewGuid() + "_" + imagefile.FileName;
                    try
                    {

                        //upload to S3
                        PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                        {
                            InputStream = imagefile.OpenReadStream(),
                            BucketName = s3BucketName,
                            Key = "img/" + newfilename,
                            CannedACL = S3CannedACL.PublicRead
                        };

                        await awsS3client.PutObjectAsync(uploadRequest);

                        //create a delete request 
                        DeleteObjectRequest deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = s3BucketName,
                            //Old Image File Name
                            Key = "img/" + gymPage.S3Key
                        };
                        await awsS3client.DeleteObjectAsync(deleteRequest);
                    }
                    catch (AmazonS3Exception ex)
                    {
                        return BadRequest("Error: " + ex.Message);
                    }

                    gymPage.ImgURL = "https://" + s3BucketName + ".s3.amazonaws.com/img/" + newfilename;
                    gymPage.S3Key = newfilename;
                }

                try
                {
                    _context.Update(gymPage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymPageExists(gymPage.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gymPage);
        }

        // GET: GymOwner/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {


            if (id == null || _context.GymPage == null)
            {
                return NotFound();
            }

            var gymPage = await _context.GymPage
                .FirstOrDefaultAsync(m => m.ID == id);
            if (gymPage == null)
            {
                return NotFound();
            }

            return View(gymPage);
        }

        // POST: GymOwner/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            List<string> getKeys = getValues();
            var awsS3client = new AmazonS3Client(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);
            var snsClient = new AmazonSimpleNotificationServiceClient(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

            if (_context.GymPage == null)
            {
                return Problem("Entity set 'AWSGymWebsiteContext.GymPage'  is null.");
            }
            var gymPage = await _context.GymPage.FindAsync(id);
            if (gymPage != null)
            {
                _context.GymPage.Remove(gymPage);
            }
            
            await _context.SaveChangesAsync();

            //Delete Image File
            //create a delete request 
            DeleteObjectRequest deleteRequest = new DeleteObjectRequest
            {
                BucketName = s3BucketName,
                //Old Image File Name
                Key = "img/" + gymPage.S3Key
            };
            await awsS3client.DeleteObjectAsync(deleteRequest);

            //Delete SNS Topic
            //Find the SNS topic based on GymID
            List<SNSTopic> topicresult = await _context.snstopic.Where(gymPage => gymPage.GymID == id).ToListAsync();
            SNSTopic topic = topicresult.First();
            await snsClient.DeleteTopicAsync(topic.TopicARN);

            //delete SNS in data
            _context.snstopic.Remove(topic);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool GymPageExists(int id)
        {
          return (_context.GymPage?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
