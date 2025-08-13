import axios from "axios";
import { GetAuthToken } from "./userState";

export const auth_url = "https://localhost:7180";
export const matchmaking_url = "https://localhost:7034";
export const gameservice_url = "https://localhost:7067"

export const authService = axios.create({
  baseURL: auth_url,
  timeout: 1000,
  headers: {'Content-Type': 'application/json'}
});


export const matchService = axios.create({
  baseURL: matchmaking_url,
  timeout: 1000,
  headers: {'Content-Type': 'application/json'}
});

export const gameService = axios.create({
    baseURL: gameservice_url,
  timeout: 1000,
  headers: {'Content-Type': 'application/json'}
})

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

gameService.interceptors.request.use(
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
