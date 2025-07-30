using DibatechLinkerAPI.Models.Domain;

namespace DibatechLinkerAPI.Services.Interfaces
{
    public interface ILinkParsingService
    {
        Task<ParsedLink> ParseLinkAsync(string url);
        Task<bool> IsValidUrlAsync(string url);
        LinkCategory CategorizeContent(string title, string description, string domain, string siteName);
        ContentType DetermineContentType(string url, string title, string description, string siteName);
    }
}
