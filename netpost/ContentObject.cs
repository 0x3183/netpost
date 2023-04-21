using System;
using System.Collections.Generic;

namespace netpost
{
    public abstract class ContentObject
    {
        public static readonly Dictionary<Guid, ContentObject> List = new();
        public readonly Guid Id;
        public readonly DateTime Created;
        public ContentObject()
        {
            Id = Guid.NewGuid();
            Created = DateTime.Now;
            List.Add(Id, this);
        }
        public virtual void Delete()
        {
            List.Remove(Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static ContentObject? Get(Guid id)
        {
            if (List.TryGetValue(id, out ContentObject? value)) return value;
            return null;
        }
    }
}
