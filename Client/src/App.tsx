import { useState } from "react";
import { UserRole } from "./models";
import { AdminUsersPage } from "./components/AdminUsersPage";
import { AuthPanel } from "./components/AuthPanel";
import { SharedTripPage } from "./components/SharedTripPage";
import { TripDetailsPage } from "./components/TripDetailsPage";
import { TripListPage } from "./components/TripListPage";
import { environment } from "./config/environment";
import { useAuth } from "./state";

export function App() {
  const { auth, isAuthenticated, logout } = useAuth();
  const [selectedTripId, setSelectedTripId] = useState<string | null>(null);
  const [isAdminOpen, setIsAdminOpen] = useState(false);
  const sharedToken = getSharedToken();

  function handleLogout() {
    setSelectedTripId(null);
    setIsAdminOpen(false);
    logout();
  }

  return (
    <main className="app-shell">
      <section className="workspace">
        {sharedToken ? (
          <SharedTripPage token={sharedToken} />
        ) : isAuthenticated && auth && isAdminOpen ? (
          <AdminUsersPage onBack={() => setIsAdminOpen(false)} />
        ) : isAuthenticated && auth && selectedTripId ? (
          <TripDetailsPage tripId={selectedTripId} onBack={() => setSelectedTripId(null)} />
        ) : isAuthenticated && auth ? (
          <TripListPage
            canAdminister={auth.role === UserRole.Admin}
            userEmail={auth.email}
            onLogout={handleLogout}
            onOpenAdmin={() => setIsAdminOpen(true)}
            onOpenTrip={setSelectedTripId}
          />
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

function getSharedToken() {
  const [, prefix, token] = window.location.pathname.split("/");
  return prefix === "shared" && token ? decodeURIComponent(token) : null;
}
