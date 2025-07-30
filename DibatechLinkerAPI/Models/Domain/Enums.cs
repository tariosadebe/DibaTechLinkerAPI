namespace DibatechLinkerAPI.Models.Domain
{
    public enum LinkCategory
    {
        Tech,
        News,
        Education,
        Lifestyle,
        Entertainment,
        Business,
        Shopping,
        Health,
        Politics,
        Science,
        Sports,
        DIY,
        Inspiration,
        Uncategorized
    }

    public enum ContentType
    {
        Video,
        Article,
        Product,
        SocialPost,
        Podcast,
        Image,
        Document,
        Unknown
    }

    public enum ReminderFrequency
    {
        Daily,
        Weekly,
        None
    }

    public enum LinkStatus
    {
        Unread,
        Read,
        Favourite,
        Archived
    }
}
