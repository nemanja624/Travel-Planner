import { AuthPanel } from "./components/AuthPanel";
import { TripListPage } from "./components/TripListPage";
import { environment } from "./config/environment";
import { useAuth } from "./state";

export function App() {
  const { auth, isAuthenticated, logout } = useAuth();

  return (
    <main className="app-shell">
      <section className="workspace">
        {isAuthenticated && auth ? (
          <TripListPage userEmail={auth.email} onLogout={logout} />
        ) : (
          <div className="intro-panel">
            <p className="eyebrow">Travel Planner</p>
            <h1>Planovi putovanja</h1>
            <p className="summary">
              Frontend aplikacija je spremna za povezivanje sa backend servisima.
            </p>
            <p className="api-url">Backend: {environment.apiBaseUrl}</p>
            <AuthPanel />
          </div>
        )}
      </section>
    </main>
  );
}
