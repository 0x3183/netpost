using System.Collections.Generic;

namespace netpost.Content
{
    public sealed class ContentList : ContentObject
    {
        public HashSet<ContentObject> Content;
        public ContentList()
        {
            Content = new();
        }
    }
}
