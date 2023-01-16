﻿namespace SignalRHubs.Models
{
    public class PostViewModel
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Contents { get; set; }
        public string Price { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}