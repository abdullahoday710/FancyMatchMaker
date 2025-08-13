import { type RouteConfig, route } from "@react-router/dev/routes";

export default [
  route("/login", "routes/login.tsx"),
  route("/dashboard", "routes/dashboard.tsx"),
  route("/game", "routes/gameplay.tsx")
] satisfies RouteConfig;
