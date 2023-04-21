using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace netpost.Content
{
    public sealed class User : ContentObject
    {
        public (string, DateTime)? Punishment;
        public string Name;
        public readonly ContentList Posts;
        public readonly ContentList Likes;
        [JsonIgnore]
        private string PasswordHash;
        private string SessionKey;
        public User(string hash, string name)
        {
            Punishment = null;
            Name = name;
            Posts = new();
            Likes = new();
            PasswordHash = hash;
            SessionKey = RegenerateSessionKey();
        }
        public override void Delete()
        {
            foreach(Post post in Posts.Content) post.Delete();
            foreach (Post post in Likes.Content) post.Likes.Content.Remove(this);
            base.Delete();
        }
        public string RegenerateSessionKey()
        {
            return SessionKey = string.Concat(RandomNumberGenerator.GetBytes(128).Select(i => i.ToString("X2")));
        }
        public bool SessionKeyMatch(string key)
        {
            return SessionKey == key;
        }
        public bool PasswordHashMatch(string hash)
        {
            return PasswordHash == hash;
        }
    }
}
