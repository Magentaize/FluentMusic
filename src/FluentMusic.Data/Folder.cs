using System;
using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class Folder
    {
        [Key]
        public long Id { get; set; }

        public string Path { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }
    }
}