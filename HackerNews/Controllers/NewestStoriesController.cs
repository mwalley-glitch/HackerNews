using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HackerNews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace HackerNews.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class NewestStoriesController : ControllerBase
    {
    
    const string NewestStoriesApi = "https://hacker-news.firebaseio.com/v0/newstories.json";
    const string StoryApiTemplate = "https://hacker-news.firebaseio.com/v0/item/{0}.json";

    private static HttpClient client = new HttpClient();
    private IMemoryCache cache;

    public NewestStoriesController(IMemoryCache memoryCache)
    {
        cache = memoryCache;
    }

    [HttpGet]
    [Route("GetNewestStories")]
    public async Task<List<HackerNewsStory>> GetAsync(string searchString)
    {
        List<HackerNewsStory> stories = new List<HackerNewsStory>();

        var response = await client.GetAsync(NewestStoriesApi);

        if (response.IsSuccessStatusCode)
        {
            var storiesResponse = response.Content.ReadAsStringAsync().Result;
            var newestIDs = JsonConvert.DeserializeObject<List<int>>(storiesResponse);
            var tasks = newestIDs.Select(GetStoryAsync);

            stories = (await Task.WhenAll(tasks)).ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                var search = searchString.ToLower();

                stories = stories.Where(s =>

                s.Title.ToLower().IndexOf(search) > -1 || s.By.ToLower().IndexOf(search) > -1)
                                    .ToList();
            }
        }
            return stories;
    }

    private async Task<HackerNewsStory> GetStoryAsync(int storyId)
    {
        return await cache.GetOrCreateAsync<HackerNewsStory>(storyId,
            async cacheEntry =>
            {
                HackerNewsStory story = new HackerNewsStory();

                var response = await client.GetAsync(string.Format(StoryApiTemplate, storyId));

                if (response.IsSuccessStatusCode)
                {
                    var storyResponse = response.Content.ReadAsStringAsync().Result;
                    story = JsonConvert.DeserializeObject<HackerNewsStory>(storyResponse);
                }
                else
                {
                    story.Title = string.Format("***Error (Not a title): (ID {0})", storyId);
                }
                return story;
            });
        }
    }
}
