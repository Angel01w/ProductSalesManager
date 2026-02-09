import { createRouter, createWebHistory } from "vue-router";
import Login from "@/components/Login.vue";
import Register from "@/components/Register.vue";
import TheWelcome from "@/components/TheWelcome.vue";

export default createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", redirect: "/login" },          
        { path: "/login", component: Login },
        { path: "/register", component: Register },
        { path: "/app", component: TheWelcome },  
    ],
});
