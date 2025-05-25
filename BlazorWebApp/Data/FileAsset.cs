// Data/FileAsset.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorWebApp.Data
{
    [Table("Images")]
    public class FileAsset
    {
        public Guid Id { get; set; }
        public byte[] Content { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public DateTimeOffset UploadedAt { get; set; }
    }
}