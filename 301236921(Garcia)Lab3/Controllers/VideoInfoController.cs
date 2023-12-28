using _301236921_Garcia_Lab3.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Table = Amazon.DynamoDBv2.DocumentModel.Table;

namespace _301236921_Garcia_Lab3.Controllers
{
    public class VideoInfoController : Controller
    {
        AmazonDynamoDBClient client = new AmazonDynamoDBClient("AKIAZEBKIIPTU5YKI47H", "++gHqI0JfJmeV86FyigKynNOeRMYpexSU+tNA0e0", Amazon.RegionEndpoint.CACentral1);

        public IActionResult Index(string title, string movieId, string userId)
        {
            ViewData["Title"] = title;
            ViewData["MovieId"] = movieId;
            ViewData["UserId"] = userId;

            // Specify the table name
            Table table = Table.LoadTable(client, "lab3Comment");

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

        [HttpPost]
        public IActionResult Post(string title, string movieId, string userId, string comment)
        {
            if(comment != null)
            {
                // Specify the table name
                var tableName = "lab3Comment";

                var item = new Dictionary<string, AttributeValue>
            {
                {"MovieID", new AttributeValue {S = movieId }},
                {"UserID", new AttributeValue {S = userId}},
                {"Comment", new AttributeValue {S = comment}}
            };

                // Create a PutItemRequest
                var request = new PutItemRequest
                {
                    TableName = tableName,
                    Item = item
                };

                // Execute the request 
                _ = client.PutItemAsync(request);
            }

            return RedirectToAction("Index", "VideoInfo", new { userId = userId, movieId = movieId, title = title });
        }

        [HttpGet]
        public IActionResult Rating(int rating, string movieId, string userId)
        {
            // Specify the table name
            var tableName = "lab3Rating";

            var item = new Dictionary<string, AttributeValue>
            {
                {"MovieID", new AttributeValue {S = movieId }},
                {"UserID", new AttributeValue {S = userId}},
                {"Rating", new AttributeValue {N = rating.ToString()}}
            };

            // Create a PutItemRequest
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = item
            };

            // Execute the request 
            _ = client.PutItemAsync(request);
            return NoContent();
        }
    }
}
