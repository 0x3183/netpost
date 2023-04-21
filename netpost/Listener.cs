using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.Threading;
using netpost.Content;

namespace netpost
{
    public static class Listener
    {
        private static readonly HttpListener Http;
        static Listener()
        {
            Http = new();
        }
        private static async Task Main()
        {
            try
            {
                User system = new User("cgt3478wtync8weaygdsj8itxwe4c78wrct", "System");
                _ = new Post(system, @"
Willkommen auf ""netpost"", dem fake Twitter.

Folgende Features welche urspruenglich geplant waren wurden nicht implementiert:
- Profilbilder
- Profile
- Tags
- Kommentare
- Teilen

Diese Platform hat viele unoptimierte stellen im Code wobei
viele Informationen nie mitgegeben werden z.B. bei Posts welche
jedoch immer benoetigt werden und viele stellen im Code
an welchen Fehler nie behandelt werden, da nicht alles wie geplant
implementiert wurde und dieses Projekt aus Test ausreichen sollte.
Das User Interface ist auch verbesserungswuerdig.

Passwoerter werden Client-Side und dann nochmal Server-Side gehasht mit dem Secure Hashing Algorithm 512
");
                Http.Prefixes.Add("http://localhost:1234/");
                Http.Start();
                Log.Info("Listening for connections.");
                while (true)
                {
                    HttpListenerContext context = await Http.GetContextAsync();
                    Task.Run(() => Handle(context)).Forget();
                }
            }
            catch(Exception ex)
            {
                Log.Warning("Error while listening for connections!");
                Log.Warning(ex.ToString());
                Environment.Exit(1);
            }
            finally
            {
                Http.Close();
            }
        }
        private static void Handle(HttpListenerContext context)
        {
            try
            {
                (string response, context.Response.StatusCode) = Process(context.Request.Url?.AbsolutePath ?? "/", context.Request)();
                context.Response.AppendHeader("Access-Control-Allow-Origin", "http://localhost:8080");
                context.Response.ContentType = "application/json";
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response));
            }
            catch(Exception ex)
            {
                Log.Warning("Error while handling request!");
                Log.Warning(ex.ToString());
                context.Response.StatusCode = 500;
            }
            finally
            {
                context.Response.Close();
            }
        }
        private static Func<(string, int)> Process(string path, HttpListenerRequest request) => path switch
        {
            "/get" => () =>
            {
                string? id = request.QueryString.Get("Id");
                string? rawindex = request.QueryString.Get("Index");
                string? rawamount = request.QueryString.Get("Amount");
                if (id is null) return (HttpUtility.JavaScriptStringEncode("No Id specified", true), 400);
                if (!Guid.TryParse(id, out Guid guid)) return (HttpUtility.JavaScriptStringEncode("Invalid Id specified.", true), 400);
                ContentObject? content = ContentObject.Get(guid);
                if (content is null) return (HttpUtility.JavaScriptStringEncode("Content not found.", true), 404);
                if (content is not ContentList || rawindex is null) return (Serialize(content, false), 200);
                if (!int.TryParse(rawindex, out int index)) return (HttpUtility.JavaScriptStringEncode("Invalid Index specified.", true), 400);
                if (!int.TryParse(rawamount ?? string.Empty, out int amount)) amount = 1;
                HashSet<ContentObject> list = ((ContentList)content).Content;
                if(index < 0 || index + amount > list.Count) return (HttpUtility.JavaScriptStringEncode("Index out of range.", true), 400);
                return ($"[{string.Join(',', list.TakeLast(list.Count - index).Take(amount).Select(i => Serialize(i, false)))}]", 200);
            },
            "/create" => () =>
            {
                string? name = request.QueryString.Get("Name");
                if (name is null) return (HttpUtility.JavaScriptStringEncode("No Name specified.", true), 400);
                if (name.Length > 32) return (HttpUtility.JavaScriptStringEncode("Name too long.", true), 400);
                string? password = request.QueryString.Get("PasswordHash");
                if (password!.Length > 512) return (HttpUtility.JavaScriptStringEncode("Password too long.", true), 400);
                if (password is null) return (HttpUtility.JavaScriptStringEncode("No PasswordHash specified.", true), 400);
                if (ContentObject.List.Where(i => i.Value is User).Select(i => (User)i.Value).Any(i => i.Name == name)) return (HttpUtility.JavaScriptStringEncode("Username already taken.", true), 404);
                User user = new(string.Concat(SHA512.HashData(Encoding.ASCII.GetBytes(password)).Select(i => i.ToString("X2"))), name);
                return (Serialize(user, true), 200);
            },
            "/authenticate" => () =>
            {
                string? name = request.QueryString.Get("Name");
                if (name is null) return (HttpUtility.JavaScriptStringEncode("No Name specified.", true), 400);
                string? password = request.QueryString.Get("PasswordHash");
                if (password is null) return (HttpUtility.JavaScriptStringEncode("No PasswordHash specified.", true), 400);
                User? user = ContentObject.List.Where(i => i.Value is User).Select(i => (User)i.Value).FirstOrDefault(i => i!.Name == name, null);
                if (user is null) return (HttpUtility.JavaScriptStringEncode("User not found.", true), 404);
                if (!user?.PasswordHashMatch(string.Concat(SHA512.HashData(Encoding.ASCII.GetBytes(password)).Select(i => i.ToString("X2")))) ?? true) return (HttpUtility.JavaScriptStringEncode("PasswordHash mismatch.", true), 403);
                user!.RegenerateSessionKey();
                return (Serialize(user, true), 200);
            },
            "/post" => () =>
            {
                string? userid = request.QueryString.Get("User");
                if (userid is null) return (HttpUtility.JavaScriptStringEncode("No User specified.", true), 400);
                string? key = request.QueryString.Get("SessionKey");
                if (key is null) return (HttpUtility.JavaScriptStringEncode("No SessionKey specified.", true), 400);
                if (!Guid.TryParse(userid, out Guid userguid)) return (HttpUtility.JavaScriptStringEncode("Invalid Id specified.", true), 400);
                ContentObject? usercon = ContentObject.Get(userguid);
                if (usercon is not User user) return (HttpUtility.JavaScriptStringEncode("User not found.", true), 404);
                if (!user?.SessionKeyMatch(key) ?? true) return (HttpUtility.JavaScriptStringEncode("SessionKey mismatch.", true), 403);
                string? content = request.QueryString.Get("Content");
                if (content is null) return (HttpUtility.JavaScriptStringEncode("No Content specified.", true), 400);
                Post post = new(user!, content);
                return (Serialize(post, false), 200);
            } ,
            "/list" => () =>
            {
                return (HttpUtility.JavaScriptStringEncode(Post.Posts.Id.ToString(), true), 200);
            },
            "/like" => () =>
            {
                string? userid = request.QueryString.Get("User");
                if (userid is null) return (HttpUtility.JavaScriptStringEncode("No User specified.", true), 400);
                string? key = request.QueryString.Get("SessionKey");
                if (key is null) return (HttpUtility.JavaScriptStringEncode("No SessionKey specified.", true), 400);
                if (!Guid.TryParse(userid, out Guid userguid)) return (HttpUtility.JavaScriptStringEncode("Invalid Id specified.", true), 400);
                ContentObject? usercon = ContentObject.Get(userguid);
                if (usercon is not User user) return (HttpUtility.JavaScriptStringEncode("User not found.", true), 404);
                if (!user?.SessionKeyMatch(key) ?? true) return (HttpUtility.JavaScriptStringEncode("SessionKey mismatch.", true), 403);
                string? postid = request.QueryString.Get("Post");
                if (postid is null) return (HttpUtility.JavaScriptStringEncode("No Post specified.", true), 400);
                if (!Guid.TryParse(postid, out Guid postguid)) return (HttpUtility.JavaScriptStringEncode("Invalid Post specified.", true), 400);
                ContentObject? postobj = ContentObject.Get(postguid);
                if (postobj is not Post post) return (HttpUtility.JavaScriptStringEncode("Post not found.", true), 404);
                if (post.Likes.Content.Contains(user!))
                {
                    post.Likes.Content.Remove(user!);
                    user!.Likes.Content.Remove(post);
                    return ("false", 200);
                }
                else
                {
                    post.Likes.Content.Add(user!);
                    user!.Likes.Content.Add(post);
                    return ("true", 200);
                }
            },
            _ => () =>
            {
                return ("Endpoint not found.", 404);
            }
        };
        private static string Serialize(ContentObject obj, bool includePrivate) //IMPORTANT: Doesn't serialize Members, only Fields!
        {
            if (obj is ContentList) return $"{{\"Id\":\"{obj.Id}\",\"Length\":{((ContentList)obj).Content.Count}}}";
            return "{" + string.Join(',', obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | (includePrivate ? BindingFlags.NonPublic : BindingFlags.Default)).Where(i => !i.CustomAttributes.Any(e => e.AttributeType == typeof(JsonIgnoreAttribute))).Select(i => new KeyValuePair<string, object?>(i.Name, i.GetValue(obj))).Where(i => i.Value is not null).Select(i =>
            {
                string? final = null;
                if (i.Value is string or Guid) final = HttpUtility.JavaScriptStringEncode(i.Value.ToString()!, true);
                else if (i.Value is double or float or decimal or long or ulong) final = ((float)i.Value).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                else if (i.Value is byte or sbyte or ushort or short or uint or int or bool) final = i.Value.ToString()!.ToLowerInvariant();
                else if (i.Value is DateTime time) final = HttpUtility.JavaScriptStringEncode(time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), true);
                else if (i.Value is IEnumerable<KeyValuePair<object, ContentObject>> map) final = $"{{{string.Join(',', map.Where(kvp => kvp.Key is not null).Select(kvp => new KeyValuePair<string, ContentObject>(kvp.Key.ToString()!, kvp.Value)).Select(kvp => $"{HttpUtility.JavaScriptStringEncode(kvp.Key.ToString(), true)}:{{\"Id\":\"{kvp.Value.Id}\"}}"))}}}";
                else if (i.Value is IEnumerable<ContentObject> ie) final = $"[{string.Join(',', ie.Select(e => $"{{\"Id\":\"{e.Id}\"}}"))}]"; 
                else if (i.Value is ContentList list) final = Serialize(list, false);
                else  if (i.Value is ContentObject content) final = $"{{\"Id\":\"{content.Id}\"}}";
                return $"\"{HttpUtility.JavaScriptStringEncode(i.Key)}\":{final ?? "null"}";
            })) + "}";
        }
    }
}