import { useState } from "react";
import { AuthPanel } from "./components/AuthPanel";
import { TripDetailsPage } from "./components/TripDetailsPage";
import { TripListPage } from "./components/TripListPage";
import { environment } from "./config/environment";
import { useAuth } from "./state";

export function App() {
  const { auth, isAuthenticated, logout } = useAuth();
  const [selectedTripId, setSelectedTripId] = useState<string | null>(null);

  function handleLogout() {
    setSelectedTripId(null);
    logout();
  }

  return (
    <main className="app-shell">
      <section className="workspace">
        {isAuthenticated && auth && selectedTripId ? (
          <TripDetailsPage tripId={selectedTripId} onBack={() => setSelectedTripId(null)} />
        ) : isAuthenticated && auth ? (
          <TripListPage userEmail={auth.email} onLogout={handleLogout} onOpenTrip={setSelectedTripId} />
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
