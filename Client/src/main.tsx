import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";
import { ServiceProvider } from "./services";
import { AuthProvider } from "./state";
import "./styles.css";

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  <React.StrictMode>
    <ServiceProvider>
      <AuthProvider>
        <App />
      </AuthProvider>
    </ServiceProvider>
  </React.StrictMode>
);
