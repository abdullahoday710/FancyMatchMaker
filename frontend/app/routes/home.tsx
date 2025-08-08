import type { Route } from "./+types/home";
import { Welcome } from "../welcome/welcome";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "Matchmaking frontend" },
    { name: "description", content: "Welcome to Matchmaking frontend !" },
  ];
}

export default function Home() {
  return <Welcome />;
}
