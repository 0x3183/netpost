<template>
    <div class="Post" v-if="!Sent && Properties.Id != null && Data != null">
        <UserPreview :Id="Data.Author.Id" />
        <div class="Content">
            <pre v-for="line in Data.Content.split('\n')">{{line}}</pre> 
        </div>
        <span class="Time">{{RelativeTimePast(Date.now() - Date.parse(Data.Created + "Z"))}}</span>
        <b class="Like" @click="Like()">+</b>
        <span class="Likes">{{Data.Likes.Length}}</span>
    </div>
    <div class="Post" v-if="!Sent && Properties.Id == null">
        <textarea class="Content" style="height: 150px;" spellcheck="false" autocomplete="false" placeholder="Post something new..." maxlength="200mjcd" @input="Data.Content = $event.target.value"></textarea>
        <i v-if="Error != null">{{Error}}</i>
        <button class="Submit" @click="Post()">Post</button>
    </div>
</template>

<script setup>
    import { defineProps, ref } from "vue";
    import Api from "./../Api.js"
    import UserPreview from "./UserPreview.vue"
    var Properties = defineProps(["Id"]);
    var Sent = ref(false);
    var Data = ref(null);
    var CanPost = ref(true);
    var Error = ref(null);
    var Emit = defineEmits(["submit"]);
    async function FetchData(){
        Data.value = await Api.Get(Properties.Id);
    }
    function RelativeTimePast(time){
        time /= 10e2;
        var units = ["ms", "seconds", "minutes", "hours", "days", "weeks", "months", "years", "decades"];
        if (time < 1) return `${Math.floor(time * 1000)} ms ago`;
        if (time < 2) return `${Math.floor(time)} second ago`;
        if (time < 60) return `${Math.floor(time)} seconds ago`;
        if (time < 120) return `${Math.floor(time / 60)} minute ago`;
        if (time < 3600) return `${Math.floor(time / 60)} minutes ago`;
        if (time < 7200) return `${Math.floor(time / 3600)} hour ago`;
        if (time < 86400) return `${Math.floor(time / 3600)} hours ago`;
        if (time < 172800) return `${Math.floor(time / 86400)} day ago`;
        if (time < 604800) return `${Math.floor(time / 86400)} days ago`;
        if (time < 1209600) return `${Math.floor(time / 604800)} week ago`;
        return `${Math.floor(time / 604800)} weeks ago`;
    }
    async function Post(){
        if (!CanPost.value) return;
        CanPost.value = false;
        var response = await Api.Post(Data.value.Content, "");
        CanPost.value = true;
        if (typeof(response) == "string") Error.value = response;
        else Emit("submit", response);
    }
    async function Like(){
        if (!Api.IsAuthenticated()) return;
        Data.value.Likes.Length += await Api.Like(Data.value.Id) ? 1 : -1;
    }
    if (Properties.Id != null) FetchData();
    else Data.value = {Content:""}
</script>

<style>
    .Post .Like{
        font-size: 120%;

        color: #00FF00;
    }
    .Post .Like:hover{
        color: #00C000;
    }
    .Post .Likes{
        font-size: 120%;

        color: #00A000;
        
        margin-left: 5px;
    }
    .Post{
        cursor: default;
        overflow: hidden;
        width: 400px;
        
        border-color: #303030;
        border-style: solid;
        border-radius: 10px;
        background-color: #484848;

        padding: 10px;
        padding-right: 20px;
    }
    .Post .Content *{
        user-select: text;
        white-space: pre-wrap;
    }
    .Post .Content{
        display: inline-block;
        width: 100%;
        
        resize: none;

        border-width: 0px;
        background-color: #505050;

        margin-top: 8px;
        padding: 4px;
        padding-top: 10px;
        padding-bottom: 10px;

        box-shadow: inset 0px 0px 10px 2px #404040;
    }
    .Post .Content:hover, .Post .Content:focus{
        outline: none;
    }
    .Post .Submit{
        cursor: pointer;
        float: right;

        font-size: 200%;

        color: #404040;
        border-color: #404040;
        border-radius: 15px;
        background-color: #606060;
    }
    .Post .Submit:hover{
        color: #406040;
        border-color: #406040;
        background-color: #608060;
    }
    .Post .Time{
        float: right;

        font-size: 85%;
        color: #707070;
    }
</style>
