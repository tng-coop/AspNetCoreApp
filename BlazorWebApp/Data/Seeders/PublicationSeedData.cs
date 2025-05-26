using BlazorWebApp.Data;

namespace BlazorWebApp.Data.Seeders
{
    public record PublicationSeedEntry(
        string? CategorySlug,
        string Title,
        string TitleJa,
        string Slug,
        string Html,
        PublicationStatus Status,
        int CreatedOffset,
        int? PublishedOffset,
        bool IncludeFractalImage = true);

    public static class PublicationSeedData
    {
        public static readonly PublicationSeedEntry[] Entries =
        [
            new("about", "Getting Started with Our CMS", "CMSの始め方",        "getting-started-with-our-cms",        "<h1>Welcome</h1><p>This is your first post. Edit me!</p>",          PublicationStatus.Published,   -7, -6),
            new("home", "default", "default",                               "default",                               "<h1>Welcome Homepage</h1><p>Homepage</p>",          PublicationStatus.Published,   -7, -6, true),
            new("ministries", "Annual Ministries Kickoff", "年間ミニストリー開始",          "annual-ministries-kickoff",          "<h2>Ministries Begin</h2><p>Ministries launch details...</p>",   PublicationStatus.Published,   -5, -4),
            new("service", "Service Opportunities Update", "奉仕の機会の更新",       "service-opportunities-update",       "<h2>Volunteer Service</h2><p>Latest service opportunities...</p>", PublicationStatus.Published,   -4, -3),
            new("outreach", "Outreach Team Training", "アウトリーチチームの研修",             "outreach-team-training",             "<h2>Training</h2><p>Upcoming outreach training...</p>",         PublicationStatus.Published,   -3, -2),
            new("food-pantry", "Food Pantry Schedule", "フードパントリーの予定",              "food-pantry-schedule",              "<h2>Food Pantry</h2><p>Monthly schedule for food pantry...</p>", PublicationStatus.Published,   -2, -1),
            new("clothing-drive", "Clothing Drive Recap", "衣料回収のまとめ",               "clothing-drive-recap",               "<h2>Clothing Drive</h2><p>Recap of the clothing drive...</p>",     PublicationStatus.Published,   -1,  0),
            new("mobile-pantry", "Mobile Pantry Route Announced", "移動パントリーのルート発表",      "mobile-pantry-route-announced",      "<h2>Mobile Pantry</h2><p>Route details for this week...</p>",      PublicationStatus.Published,    0, +1),
            new("outreach", "Community Outreach Recap", "地域アウトリーチのまとめ",           "community-outreach-recap",           "<h2>Recap</h2><p>Here's what happened…</p>",                    PublicationStatus.Published,   -2, -1),
            new("outreach", "Volunteer Spotlight", "ボランティアのスポットライト",                "volunteer-spotlight",                "<h2>Meet our volunteer</h2><p>Spotlight on Jane Doe…</p>",          PublicationStatus.Published,   -1,  0),
            new("home", "Draft Post Example", "下書き投稿の例",                 "draft-post-example",                 "<p>This post is still a draft.</p>",                               PublicationStatus.Draft,         0, null, false)
        ];
    }
}
