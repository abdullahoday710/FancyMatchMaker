import axios from "axios";

export const auth_url = import.meta.env.VITE_API_AUTH;

export const authService = axios.create({
  baseURL: auth_url,
  timeout: 1000,
  headers: {'Content-Type': 'application/json'}
});

export const matchmaking_url = import.meta.env.VITE_API_MATCHMAKING;

export const matchService = axios.create({
  baseURL: matchmaking_url,
  timeout: 1000,
  headers: {'Content-Type': 'application/json'}
});