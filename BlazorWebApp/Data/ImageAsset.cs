// Data/ImageAsset.cs
using System;

namespace BlazorWebApp.Data
{
    public class ImageAsset
    {
        public Guid Id { get; set; }
        public byte[] Content { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public DateTimeOffset UploadedAt { get; set; }
    }
}