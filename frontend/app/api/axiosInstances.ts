import axios from "axios";
import { GetAuthToken } from "./userState";

export const auth_url = "https://localhost:7180";

export const authService = axios.create({
  baseURL: auth_url,
  timeout: 1000,
  headers: {'Content-Type': 'application/json'}
});

export const matchmaking_url = "https://localhost:7034";

export const matchService = axios.create({
  baseURL: matchmaking_url,
  timeout: 1000,
  headers: {'Content-Type': 'application/json'}
});

matchService.interceptors.request.use(
  async (config) => {
    const token = await GetAuthToken();

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);
