var Content = {};
var Lists = {};
var PostList = null;
var User = localStorage.getItem("User");
var SessionKey = localStorage.getItem("SessionKey");
var ApiUrl = "http://localhost:1234";
async function SHA512(str) {
return await crypto.subtle.digest("SHA-512", new TextEncoder("utf-8").encode(str)).then(buf => {
    return Array.prototype.map.call(new Uint8Array(buf), x=>(('00'+x.toString(16)).slice(-2))).join('');
});
}
const Api = {
    Get: async function(id){
        var content = Content[id];
        if(content != undefined) return content;
        var response = await fetch(`${ApiUrl}/get?Id=${id}`);
        content = await response.json();
        Content[id] = content;
        return content;
    },
    GetList: async function(id, index, amount){
        var list = Lists[id];
        if(list != undefined) return list;
        var response = await fetch(`${ApiUrl}/get?Id=${id}&Index=${encodeURIComponent(index)}&Amount=${encodeURIComponent(amount)}`);
        list = await response.json();
        Lists[id] = list;
        return list;
    },
    GetPostListId: async function(){
        if (PostList != null) return PostList;
        var response = await fetch(`${ApiUrl}/list`);
        PostList = await response.json();
        return PostList;
    },
    Authenticate: async function(name, password){
        var response = await fetch(`${ApiUrl}/authenticate?Name=${encodeURIComponent(name)}&PasswordHash=${await SHA512(password)}`);
        var data = await response.json();
        if (typeof(data) == "string") return data;
        User = data.Id;
        SessionKey = data.SessionKey;
        localStorage.setItem("User", data.Id);
        localStorage.setItem("SessionKey", data.SessionKey);
        return null;
    },
    Deauthenticate: async function(){
        localStorage.removeItem("User");
        localStorage.removeItem("SessionKey");
        User = null;
        SessionKey = null;
    },
    CreateAccount: async function(name, password){
        var response = await fetch(`${ApiUrl}/create?Name=${encodeURIComponent(name)}&PasswordHash=${await SHA512(password)}`);
        var data = await response.json();
        if (typeof(data) == "string") return data;
        User = data.Id;
        SessionKey = data.SessionKey;
        localStorage.setItem("User", data.Id);
        localStorage.setItem("SessionKey", data.SessionKey);
        return null;
    },
    IsAuthenticated: function(){
        return User != null && SessionKey != null;
    },
    GetCurrentUserId: function(){
        return User;
    },
    Post: async function(content, tags){
        var response = await fetch(`${ApiUrl}/post?Content=${encodeURIComponent(content)}&User=${User}&SessionKey=${SessionKey}`);
        var json = await response.json();
        if (typeof(json) != "string") Content[json.Id] = json;
        return json;
    },
    Like: async function(id){
        var response = await fetch(`${ApiUrl}/like?Post=${id}&User=${User}&SessionKey=${SessionKey}`);
        return await response.json();
    }
};

export default Api;