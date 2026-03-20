import axios from "axios";
import { config } from "../app/config";

const api = axios.create({
    baseURL: config.apiBaseUrl,
    headers: {
        "Content-Type": "application/json"
    },
});

api.interceptors.request.use((request) => {
    const token = localStorage.getItem("token");

    if (token) {
        request.headers.Authorization = `Bearer ${token}`;
    }
    return request;
});

export default api;