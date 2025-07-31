using HtmlAgilityPack;
using DibatechLinkerAPI.Models.Domain;
using DibatechLinkerAPI.Services.Interfaces;
using System.Text.RegularExpressions;

namespace DibatechLinkerAPI.Services.Implementations
{
    public class LinkParsingService : ILinkParsingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LinkParsingService> _logger;

        public LinkParsingService(HttpClient httpClient, ILogger<LinkParsingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Configure HttpClient
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<ParsedLink> ParseLinkAsync(string url)
        {
            var parsedLink = new ParsedLink
            {
                OriginalUrl = url,
                ParsedAt = DateTime.UtcNow
            };

            try
            {
                if (!await IsValidUrlAsync(url))
                {
                    parsedLink.IsValidUrl = false;
                    parsedLink.ErrorMessage = "Invalid URL format";
                    return parsedLink;
                }

                var uri = new Uri(url);
                parsedLink.Domain = uri.Host;

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    parsedLink.IsValidUrl = false;
                    parsedLink.ErrorMessage = $"Failed to fetch content: {response.StatusCode}";
                    return parsedLink;
                }

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Extract metadata
                parsedLink.Title = ExtractTitle(doc, url);
                parsedLink.Description = ExtractDescription(doc);
                parsedLink.ImageUrl = ExtractImageUrl(doc, uri);
                parsedLink.Author = ExtractAuthor(doc);
                parsedLink.SiteName = ExtractSiteName(doc, uri);

                // Determine content type and category
                var contentType = DetermineContentType(
                    parsedLink.OriginalUrl,  // Fixed property name
                    parsedLink.Title ?? "", 
                    parsedLink.Description ?? "", 
                    parsedLink.SiteName ?? ""
                );
                parsedLink.ContentType = contentType;
                parsedLink.Category = CategorizeContent(
                    parsedLink.Title ?? "", 
                    parsedLink.Description ?? "", 
                    parsedLink.Domain ?? "", 
                    parsedLink.SiteName ?? ""
                );

                parsedLink.IsValidUrl = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing link: {Url}", url);
                parsedLink.IsValidUrl = false;
                parsedLink.ErrorMessage = "Failed to parse link content";
            }

            return parsedLink;
        }

        // Fix the IsValidUrlAsync method
        public Task<bool> IsValidUrlAsync(string url)
        {
            try
            {
                var uri = new Uri(url);
                var isValid = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
                return Task.FromResult(isValid);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public LinkCategory CategorizeContent(string title, string description, string domain, string siteName)
        {
            var content = $"{title} {description} {domain} {siteName}".ToLowerInvariant();

            // Tech keywords
            if (ContainsKeywords(content, new[] { "tech", "technology", "programming", "coding", "software", "developer", "github", "stackoverflow", "dev.to", "medium", "api", "framework", "javascript", "python", "react", "angular", "vue", "nodejs", "css", "html" }))
                return LinkCategory.Tech;

            // News keywords
            if (ContainsKeywords(content, new[] { "news", "breaking", "report", "reuters", "bbc", "cnn", "guardian", "times", "post", "herald", "daily", "weekly" }) || IsNewsWebsite(domain))
                return LinkCategory.News;

            // Education keywords
            if (ContainsKeywords(content, new[] { "education", "course", "tutorial", "learn", "study", "university", "college", "khan academy", "coursera", "udemy", "edx", "lesson", "training", "certification" }))
                return LinkCategory.Education;

            // Entertainment keywords
            if (ContainsKeywords(content, new[] { "entertainment", "movie", "film", "music", "game", "gaming", "netflix", "spotify", "youtube", "tiktok", "instagram", "funny", "meme", "viral" }))
                return LinkCategory.Entertainment;

            // Shopping keywords
            if (ContainsKeywords(content, new[] { "shop", "buy", "purchase", "price", "amazon", "ebay", "jumia", "konga", "store", "cart", "product", "deal", "discount", "sale" }))
                return LinkCategory.Shopping;

            // Health keywords
            if (ContainsKeywords(content, new[] { "health", "medical", "doctor", "medicine", "fitness", "nutrition", "wellness", "diet", "exercise", "hospital", "clinic" }))
                return LinkCategory.Health;

            // Business keywords
            if (ContainsKeywords(content, new[] { "business", "entrepreneur", "startup", "company", "corporate", "finance", "investment", "market", "economy", "trading", "stock" }))
                return LinkCategory.Business;

            // Sports keywords
            if (ContainsKeywords(content, new[] { "sport", "football", "basketball", "tennis", "soccer", "olympics", "fifa", "nba", "premier league", "champions league" }))
                return LinkCategory.Sports;

            // Lifestyle keywords
            if (ContainsKeywords(content, new[] { "lifestyle", "fashion", "travel", "food", "recipe", "cooking", "home", "garden", "beauty", "style" }))
                return LinkCategory.Lifestyle;

            // Science keywords
            if (ContainsKeywords(content, new[] { "science", "research", "study", "physics", "chemistry", "biology", "astronomy", "nature", "scientific", "journal" }))
                return LinkCategory.Science;

            // Politics keywords
            if (ContainsKeywords(content, new[] { "politics", "government", "election", "vote", "president", "minister", "congress", "parliament", "policy", "law" }))
                return LinkCategory.Politics;

            // DIY keywords
            if (ContainsKeywords(content, new[] { "diy", "tutorial", "how to", "guide", "repair", "fix", "build", "make", "craft", "project" }))
                return LinkCategory.DIY;

            // Inspiration keywords
            if (ContainsKeywords(content, new[] { "inspiration", "motivational", "quote", "success", "achievement", "goal", "dream", "inspire", "motivation" }))
                return LinkCategory.Inspiration;

            return LinkCategory.Uncategorized;
        }

        // Fix the DetermineContentType method with correct enum values
        public ContentType DetermineContentType(string url, string title, string description, string siteName)
        {
            title = title ?? "";
            description = description ?? "";
            siteName = siteName ?? "";
            
            var urlLower = url.ToLower();
            var titleLower = title.ToLower();
            var descriptionLower = description.ToLower();
            var siteNameLower = siteName.ToLower();

            // Video content detection
            if (urlLower.Contains("youtube.com") || urlLower.Contains("youtu.be") ||
                urlLower.Contains("vimeo.com") || urlLower.Contains("twitch.tv") ||
                titleLower.Contains("video") || descriptionLower.Contains("watch"))
            {
                return ContentType.Video;
            }

            // Article/Blog content detection
            if (urlLower.Contains("blog") || urlLower.Contains("article") ||
                urlLower.Contains("medium.com") || urlLower.Contains("dev.to") ||
                siteNameLower.Contains("blog") || titleLower.Contains("tutorial") ||
                descriptionLower.Contains("article") ||
                urlLower.Contains("twitter.com") || urlLower.Contains("x.com") ||
                urlLower.Contains("facebook.com") || urlLower.Contains("instagram.com") ||
                urlLower.Contains("linkedin.com") || urlLower.Contains("tiktok.com") ||
                siteNameLower.Contains("news") || urlLower.Contains("news") ||
                titleLower.Contains("breaking") || descriptionLower.Contains("reported") ||
                urlLower.Contains("docs") || urlLower.Contains("documentation") ||
                urlLower.Contains("reference") || urlLower.Contains("api") ||
                titleLower.Contains("documentation") || titleLower.Contains("guide"))
            {
                return ContentType.Article;
            }

            // E-commerce/Shopping sites - use Website
            if (urlLower.Contains("shop") || urlLower.Contains("store") ||
                urlLower.Contains("amazon.com") || urlLower.Contains("ebay.com") ||
                titleLower.Contains("buy") || descriptionLower.Contains("price"))
            {
                return ContentType.Article; // Instead of ContentType.Website
            }

            // Default to Website
            return ContentType.Website;
        }

        private string ExtractTitle(HtmlDocument doc, string url)
        {
            // Try OpenGraph title first
            var ogTitle = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(ogTitle))
                return ogTitle.Trim();

            // Try Twitter title
            var twitterTitle = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(twitterTitle))
                return twitterTitle.Trim();

            // Try regular title tag
            var titleTag = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;
            if (!string.IsNullOrWhiteSpace(titleTag))
                return titleTag.Trim();

            // Fallback to URL
            return new Uri(url).Host;
        }

        private string? ExtractDescription(HtmlDocument doc)
        {
            // Try OpenGraph description
            var ogDescription = doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(ogDescription))
                return ogDescription.Trim();

            // Try Twitter description
            var twitterDescription = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(twitterDescription))
                return twitterDescription.Trim();

            // Try meta description
            var metaDescription = doc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(metaDescription))
                return metaDescription.Trim();

            return null;
        }

        private string? ExtractImageUrl(HtmlDocument doc, Uri baseUri)
        {
            // Try OpenGraph image
            var ogImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(ogImage))
                return MakeAbsoluteUrl(ogImage, baseUri);

            // Try Twitter image
            var twitterImage = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(twitterImage))
                return MakeAbsoluteUrl(twitterImage, baseUri);

            // Try link rel icon
            var icon = doc.DocumentNode.SelectSingleNode("//link[@rel='icon']")?.GetAttributeValue("href", "");
            if (!string.IsNullOrWhiteSpace(icon))
                return MakeAbsoluteUrl(icon, baseUri);

            return null;
        }

        private string? ExtractAuthor(HtmlDocument doc)
        {
            // Try OpenGraph author
            var ogAuthor = doc.DocumentNode.SelectSingleNode("//meta[@property='og:author']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(ogAuthor))
                return ogAuthor.Trim();

            // Try article author
            var articleAuthor = doc.DocumentNode.SelectSingleNode("//meta[@name='author']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(articleAuthor))
                return articleAuthor.Trim();

            return null;
        }

        private string? ExtractSiteName(HtmlDocument doc, Uri uri)
        {
            // Try OpenGraph site name
            var ogSiteName = doc.DocumentNode.SelectSingleNode("//meta[@property='og:site_name']")?.GetAttributeValue("content", "");
            if (!string.IsNullOrWhiteSpace(ogSiteName))
                return ogSiteName.Trim();

            // Fallback to domain
            return uri.Host;
        }

        private string MakeAbsoluteUrl(string url, Uri baseUri)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;

            if (Uri.TryCreate(baseUri, url, out var absoluteUri))
                return absoluteUri.ToString();

            return url;
        }

        private bool ContainsKeywords(string content, string[] keywords)
        {
            return keywords.Any(keyword => content.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsNewsWebsite(string domain)
        {
            var newsWebsites = new[] 
            { 
                "bbc.com", "cnn.com", "reuters.com", "guardian.co.uk", "nytimes.com", 
                "washingtonpost.com", "theguardian.com", "bloomberg.com", "npr.org",
                "punchng.com", "vanguardngr.com", "premiumtimesng.com", "thecable.ng"
            };
            
            return newsWebsites.Any(site => domain.Contains(site, StringComparison.OrdinalIgnoreCase));
        }
    }
}
