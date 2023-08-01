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

namespace AWSGymWebsite.Controllers
{
    public class GymOwnerController : Controller
    {
        private readonly AWSGymWebsiteContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AWSGymWebsiteUser> _userManager;

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

            //2. read the info from json using configure instance
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
            try
            {
                List<string> getKeys = getValues();
                var awsS3client = new AmazonS3Client(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

                //upload to S3
                PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                {
                    InputStream = imagefile.OpenReadStream(),
                    BucketName = s3BucketName,
                    Key = "img/"+imagefile.FileName,
                    CannedACL = S3CannedACL.PublicRead
                };

                await awsS3client.PutObjectAsync(uploadRequest);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
            gymPage.ImgURL = "https://" + s3BucketName + ".s3.amazonaws.com/img/" + imagefile.FileName;
            gymPage.S3Key = imagefile.FileName;
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            gymPage.OwnerID = userId;

            if (ModelState.IsValid)
            {
                _context.Add(gymPage);
                await _context.SaveChangesAsync();
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
            if (id != gymPage.ID)
            {
                return NotFound();
            }

            if (imagefile != null)
            {
                try
                {
                    List<string> getKeys = getValues();
                    var awsS3client = new AmazonS3Client(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

                    //upload to S3
                    PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                    {
                        InputStream = imagefile.OpenReadStream(),
                        BucketName = s3BucketName,
                        Key = "img/" + imagefile.FileName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    await awsS3client.PutObjectAsync(uploadRequest);
                }
                catch (AmazonS3Exception ex)
                {
                    return BadRequest("Error: " + ex.Message);
                }
                gymPage.ImgURL = "https://" + s3BucketName + ".s3.amazonaws.com/img/" + imagefile.FileName;
                gymPage.S3Key = imagefile.FileName;
            }

            if (ModelState.IsValid)
            {
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
            return RedirectToAction(nameof(Index));
        }

        private bool GymPageExists(int id)
        {
          return (_context.GymPage?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
