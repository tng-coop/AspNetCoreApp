using BlazorWebApp.Data;

namespace BlazorWebApp.Data.Seeders
{
    public record PublicationSeedEntry(
        string? CategorySlug,
        string Title,
        string Html,
        PublicationStatus Status,
        int CreatedOffset,
        int? PublishedOffset,
        bool IncludeFractalImage = true);

    public static class PublicationSeedData
    {
        public static readonly PublicationSeedEntry[] Entries =
        {
            new("about",          "Getting Started with Our CMS",        "<h1>Welcome</h1><p>This is your first post. Edit me!</p>",          PublicationStatus.Published,   -7, -6),
            new("home",          "defraul",        "<h1>Welcome Homepage</h1><p>Homepage</p>",          PublicationStatus.Published,   -7, -6),
            new("ministries",     "Annual Ministries Kickoff",          "<h2>Ministries Begin</h2><p>Ministries launch details...</p>",   PublicationStatus.Published,   -5, -4),
            new("service",        "Service Opportunities Update",       "<h2>Volunteer Service</h2><p>Latest service opportunities...</p>", PublicationStatus.Published,   -4, -3),
            new("outreach",       "Outreach Team Training",             "<h2>Training</h2><p>Upcoming outreach training...</p>",         PublicationStatus.Published,   -3, -2),
            new("food-pantry",    "Food Pantry Schedule",              "<h2>Food Pantry</h2><p>Monthly schedule for food pantry...</p>", PublicationStatus.Published,   -2, -1),
            new("clothing-drive", "Clothing Drive Recap",               "<h2>Clothing Drive</h2><p>Recap of the clothing drive...</p>",     PublicationStatus.Published,   -1,  0),
            new("mobile-pantry",  "Mobile Pantry Route Announced",      "<h2>Mobile Pantry</h2><p>Route details for this week...</p>",      PublicationStatus.Published,    0, +1),
            new("outreach",       "Community Outreach Recap",           "<h2>Recap</h2><p>Here's what happened…</p>",                    PublicationStatus.Published,   -2, -1),
            new("outreach",       "Volunteer Spotlight",                "<h2>Meet our volunteer</h2><p>Spotlight on Jane Doe…</p>",          PublicationStatus.Published,   -1,  0),
            new(null,              "Draft Post Example",                 "<p>This post is still a draft.</p>",                               PublicationStatus.Draft,         0, null, false)
        };
    }
}
