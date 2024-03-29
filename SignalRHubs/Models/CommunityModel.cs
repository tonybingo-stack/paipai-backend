﻿using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Models
{
    public class CommunityModel
    {
        [Required]
        public string CommunityName { get; set; }
        public string? CommunityDescription { get; set; }
        public string? CommunityType { get; set; }
        public string? ForegroundImage { get; set; }
    }
}
