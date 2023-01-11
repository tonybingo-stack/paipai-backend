﻿using Dapper.Contrib.Extensions;

namespace SignalRHubs.Entities
{
    [Table("Message")]
    public class Message : BaseEntity
    {

        [ExplicitKey]
        public override Guid Id { get; set; }
        public string SenderUserName { get; set; }
        public string ReceiverUserName { get; set; }
        public string Content { get; set; }
        public Guid? ChannelId { get; set; }

        /// <summary>
        /// Store the file azure and store your file path when necessary.
        /// </summary>
        public string? FilePath { get; set; }

        //[Write(false)]
        //public virtual Channel myChannel { get; set; }
    }
}