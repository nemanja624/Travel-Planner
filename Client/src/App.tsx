import { AuthPanel } from "./components/AuthPanel";
import { environment } from "./config/environment";
import { useAuth } from "./state";

export function App() {
  const { auth, isAuthenticated, logout } = useAuth();

  return (
    <main className="app-shell">
      <section className="workspace">
        <div className="intro-panel">
          <p className="eyebrow">Travel Planner</p>
          <h1>Planovi putovanja</h1>
          <p className="summary">
            Frontend aplikacija je spremna za povezivanje sa backend servisima.
          </p>
          <p className="api-url">Backend: {environment.apiBaseUrl}</p>
          <p className="api-url">
            Status: {isAuthenticated && auth ? `ulogovan kao ${auth.email}` : "nije ulogovan"}
          </p>
          {isAuthenticated ? (
            <button className="secondary-button" type="button" onClick={logout}>
              Odjavi se
            </button>
          ) : (
            <AuthPanel />
          )}
        </div>
      </section>
    </main>
  );
}
