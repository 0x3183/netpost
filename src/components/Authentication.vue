<template>
    <div class="Authentication Form">
        <span>Name</span>
        <input type="text" autocapitalize="false" autocomplete="true" autocorrect="false" maxlength="64" @input="Credentials.Name = $event.target.value">
        <span>Password</span>
        <input type="password" autocapitalize="false" autocomplete="true" autocorrect="false" @input="Credentials.Password = $event.target.value">
        <u @click="Authenticate">Authenticate</u>
        <u @click="CreateAccount">Create</u>
        <i v-if="Error != null">{{Error}}</i>
    </div>
</template>

<script setup>
    import { ref, defineEmits } from "vue";
    import Api from "./../Api.js";
    const Credentials = ref({Name:"",Password:""})
    const Error = ref(null);
    var emit = defineEmits(["success"]);
    async function Authenticate(){
        var response = await Api.Authenticate(Credentials.value.Name, Credentials.value.Password);
        if (response != null) Error.value = response;
        else emit("success");
    }
    async function CreateAccount(){
        var response = await Api.CreateAccount(Credentials.value.Name, Credentials.value.Password);
        if (response != null) Error.value = response;
        else emit("success");
    }
</script>

<style>
    .Authentication{
        position: fixed;
        top: 15%;
        left: 20%;
        width: 60%;
        height: 60%;

        border-style: solid;
        border-radius: 5px;
        border-width: 2px;
    }
</style>