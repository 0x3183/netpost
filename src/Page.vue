<template>
  <div class="Header">
    <span class="Title">netpost </span>
    <u class="Title" @click="ChangePage(PostsPage - 1);">-</u>
    <u class="Title" @click="ChangePage(``)">{{PostsPage == null ? " " : PostsPage}}</u>
    <u class="Title" @click="ChangePage(PostsPage + 1);">+</u>
    <u v-if="!Authenticated" class="Option" @click="Authenticate=!Authenticate">Authenticate</u>
    <Authentication v-if="Authenticate" @success="Authenticated=true;Authenticate=false" />
    <u v-if="Authenticated" class="Option" @click="Authenticated=false;Api.Deauthenticate();">Deauthenticate</u>
    <UserPreview class="Option" v-if="Authenticated" :Id="Api.GetCurrentUserId()"/>
  </div>
  <div class="PostList">
    <Post v-if="Authenticated" :Id="null" @submit="Reload()"></Post>
    <Post v-for="post in Posts" :Id="post.Id"/>
  </div>
</template>

<script setup>
import { ref } from "vue";
import Api from "./Api.js";
import Post from "./components/Post.vue";
import Authentication from "./components/Authentication.vue"
import UserPreview from "./components/UserPreview.vue"
const Authenticate = ref(false);
const Authenticated = ref(Api.IsAuthenticated());
const Posts = ref([]);
const PostsPage = ref(null);
async function FetchList(page){
  var id = await Api.GetPostListId();
  var list = await Api.Get(id);
  if (page == undefined) page = Math.floor(list.Length / 20);
  if (page < 0) page = 0;
  PostsPage.value = page;
  var index = page * 20;
  if(index >= list.Length) Posts.value = [];
  else if(index + 20 > list.Length) Posts.value = (await Api.GetList(id, index, list.Length - index)).reverse();
  else Posts.value = (await Api.GetList(id, index, 20)).reverse();
  for (var i = 0; i < Posts.value.length; i++){
    Posts.value[i].Index = i;
  }
}
function Reload(){
  document.location.reload();
}
function ChangePage(page){
  document.location.search = page;
}
var rpage = Number.parseInt(document.location.search.replace('?', ""));
FetchList(isNaN(rpage) ? undefined : rpage);
</script>

<style>
.Header{
  position: fixed;
  width: 100%;
  height: 80px;
  overflow: hidden;
  top: 0%;

  color: #00B000;
  background-color: black;
}
.Header .Option{
  float: right;
  font-size: 150%;

  margin-top: 2%;
  margin-right: 1%;
}
.Header .Title{
  font-family: Consolas, monaco, monospace; 
  font-size: 400%;
}
.PostList{
  background-color: #404040;

  padding: 1%;
  margin-left: 10%;
  margin-top: 80px;
}
.PostList .Post{
  width: 40%;
  display: inline-block;
  vertical-align: middle;
  margin: 0.5%;
}

</style>
