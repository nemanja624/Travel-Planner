import { environment } from "./config/environment";

export function App() {
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
        </div>
      </section>
    </main>
  );
}
