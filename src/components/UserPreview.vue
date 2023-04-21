<template>
    <b class="UserPreview">{{Data.Name}}</b>
</template>

<script setup>
    import { defineProps, ref } from "vue";
    import Api from "../Api.js"
    var Properties = defineProps(["Id"]);
    var Data = ref(
        {
            Name:"Loading",
        }
    );
    async function FetchData(){
        Data.value = await Api.Get(Properties.Id);
    }
    FetchData();
</script>

<style>
    .UserPreview{
        font-size: 100%;
        user-select: text;
    }
</style>