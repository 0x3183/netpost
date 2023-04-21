using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace netpost.Content
{
    public sealed class Post : ContentObject
    {
        public static readonly ContentList Posts = new();
        public readonly User Author;
        public readonly string Content;
        public readonly ContentList Likes;
        public Post(User author, string content)
        {
            Author = author;
            Content = content;
            Likes = new();
            Posts.Content.Add(this);
        }
        public override void Delete()
        {
            foreach (User user in Likes.Content) user.Likes.Content.Remove(this);
            Author.Posts.Content.Remove(this);
            Posts.Content.Remove(this);
            base.Delete();
        }
    }
}
