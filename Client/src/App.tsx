import { environment } from "./config/environment";
import { useAuth } from "./state";

export function App() {
  const { auth, isAuthenticated } = useAuth();

  return (
    <main className="app-shell">
      <section className="workspace">
        <div>
          <p className="eyebrow">Travel Planner</p>
          <h1>Planovi putovanja</h1>
          <p className="summary">
            Frontend aplikacija je spremna za povezivanje sa backend servisima.
          </p>
          <p className="api-url">Backend: {environment.apiBaseUrl}</p>
          <p className="api-url">
            Status: {isAuthenticated && auth ? `ulogovan kao ${auth.email}` : "nije ulogovan"}
          </p>
        </div>
      </section>
    </main>
  );
}
