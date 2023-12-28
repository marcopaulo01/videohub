using _301236921_Garcia_Lab3.Data;
using _301236921_Garcia_Lab3.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Table = Amazon.DynamoDBv2.DocumentModel.Table;

namespace _301236921_Garcia_Lab3.Controllers
{
    public class VideoController : Controller
    {
        private readonly UserDbContext _userDbContext;
        
        // Specify your AWS credentials and region
        AmazonDynamoDBClient client = new AmazonDynamoDBClient("AKIAZEBKIIPTU5YKI47H", "++gHqI0JfJmeV86FyigKynNOeRMYpexSU+tNA0e0", Amazon.RegionEndpoint.CACentral1);
        AmazonS3Client s3client = new AmazonS3Client("AKIAZEBKIIPTU5YKI47H", "++gHqI0JfJmeV86FyigKynNOeRMYpexSU+tNA0e0", Amazon.RegionEndpoint.CACentral1);

        public VideoController(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public IActionResult Index(Guid userId)
        {

            ViewData["UserId"] = userId;
            var user = _userDbContext.Users.FirstOrDefault(u => u.Id == userId);
            string username = user.Username;

            ViewData["Username"] = username;

            // Specify the table name
            Table table = Table.LoadTable(client, "lab3");

            // Use Scan with ScanOperationConfig for filtering
            ScanOperationConfig scanConfig = new ScanOperationConfig
            {
                Filter = new ScanFilter(),
                // Add other configurations as needed
            };

            Search search = table.Scan(scanConfig);

            List<Document> resultsList = new List<Document>();
            do
            {
                List<Document> results = search.GetNextSetAsync().Result;
                resultsList.AddRange(results);
            } while (!search.IsDone);

            return View(resultsList);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(Video video, Guid userId, IFormFile videoFile)
        {
            // Specify the table name
            var tableName = "lab3";

            // Convert userId to string
            string stringUserId = userId.ToString();

            // Set the current time
            string now = DateTime.Now.ToString();

            // Generate a new GUID for MovieID
            video.MovieID = Guid.NewGuid();

            // Create a Dictionary to hold the item attributes
            var item = new Dictionary<string, AttributeValue>
            {
                {"MovieID", new AttributeValue {S = video.MovieID.ToString() ?? ""}},
                {"Title", new AttributeValue {S = video.Title ?? ""}},
                {"Director", new AttributeValue {S = video.Director ?? ""}},
                {"ReleaseTime", new AttributeValue {S = now}},
                {"UserID", new AttributeValue {S = stringUserId ?? ""}},
                {"Genre", new AttributeValue {S = video.Genre ?? ""}}
            };

            // Create a PutItemRequest
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = item
            };

            // Execute the request asynchronously
            await client.PutItemAsync(request);

            using var memoryStream = new MemoryStream();
            await videoFile.CopyToAsync(memoryStream);

            using var transferUtility = new TransferUtility(s3client);
            string bucketName = "marcogarcialab3";
            string keyName = video.MovieID.ToString() + ".mp4";

            memoryStream.Position = 0;

            await transferUtility.UploadAsync(memoryStream, bucketName, keyName);

            // Redirect to the Index action with the user ID
            return RedirectToAction("Index", "Video", new { userId = stringUserId });
        }

        public async Task<IActionResult> DeleteAsync(Guid movieID, Guid userId)
        {
            string tableName = "lab3";

            // Create a Table object
            Table table = Table.LoadTable(client, tableName);

            // Convert Guid to string
            string movieIdToDelete = movieID.ToString();

            // Delete the item from the table
            await table.DeleteItemAsync(movieIdToDelete);

            // Redirect to the Index action in the Video controller
            return RedirectToAction("Index", "Video", new { userId = userId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid movieId, Guid userId)
        {
            Table table = Table.LoadTable(client, "lab3");

            var search = await table.GetItemAsync(movieId.ToString());

            // Map the DynamoDB Document to your Video model
            Video video = new Video
            {
                MovieID = Guid.Parse(search["MovieID"]),
                Title = search["Title"],
                Director = search["Director"],
                Genre = search["Genre"],
                ReleaseTime = search["ReleaseTime"],
                UserID = search["UserID"]
            };

            return View("Edit", video);
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(Video video)
        {

            // Specify the table name
            string tableName = "lab3";

            // Specify the key of the item you want to update
            string movieIdToUpdate = video.MovieID.ToString();

            // Set the current time
            string now = DateTime.Now.ToString();

            // Specify the update expression and attribute values
            var item = new Dictionary<string, AttributeValue>
        {
            {"MovieID", new AttributeValue {S = movieIdToUpdate ?? ""}},
                {"Title", new AttributeValue {S = video.Title ?? ""}},
                {"Director", new AttributeValue {S = video.Director ?? ""}},
                {"ReleaseTime", new AttributeValue {S = now}},
                {"UserID", new AttributeValue {S = video.UserID ?? ""}},
                {"Genre", new AttributeValue {S = video.Genre ?? ""}}
        };

            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = item
            };

            // Execute the request asynchronously
            await client.PutItemAsync(request);

            return RedirectToAction("Index", "Video", new { userId = video.UserID });
        }

        public IActionResult Download(Guid movieId)
        {
            // Set the name of your S3 bucket
            var bucketName = "marcogarcialab3";

            // Set the key (object key) of the object you want to download
            var objectKey = movieId.ToString()+".mp4";

            // Set the local path where you want to save the downloaded file
            var downloadDirectory = "C:\\Users\\63927\\Downloads";

            // Create a TransferUtility instance
            var transferUtility = new TransferUtility(s3client);

            var sanitizedFileName = Path.GetFileName(objectKey);

            // Combine sanitized file name with the download directory
            var localFilePath = Path.Combine(downloadDirectory, sanitizedFileName);

            // Initiate the download
            transferUtility.DownloadAsync(localFilePath, bucketName, objectKey);

            return NoContent();
        }
    }
}
