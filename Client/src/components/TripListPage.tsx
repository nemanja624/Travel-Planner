import { useEffect, useState } from "react";
import { Trip, TripSummary } from "../models";
import { ApiError, tripService } from "../services";
import { TripForm } from "./TripForm";

interface TripListPageProps {
  userEmail: string;
  onLogout: () => void;
  onOpenTrip: (tripId: string) => void;
}

export function TripListPage({ userEmail, onLogout, onOpenTrip }: TripListPageProps) {
  const [trips, setTrips] = useState<TripSummary[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  function handleTripCreated(trip: Trip) {
    setTrips((currentTrips) => [
      {
        id: trip.id,
        title: trip.title,
        startDate: trip.startDate,
        endDate: trip.endDate,
        plannedBudget: trip.plannedBudget,
        totalExpenses: trip.totalExpenses,
        remainingBudget: trip.remainingBudget
      },
      ...currentTrips
    ]);
  }

  useEffect(() => {
    let isMounted = true;

    async function loadTrips() {
      try {
        const loadedTrips = await tripService.getTrips();
        if (isMounted) {
          setTrips(loadedTrips);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Planovi nisu ucitani.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadTrips();

    return () => {
      isMounted = false;
    };
  }, []);

  return (
    <section className="page-panel">
      <header className="page-header">
        <div>
          <p className="eyebrow">Travel Planner</p>
          <h1>Moji planovi</h1>
          <p className="summary">Ulogovan kao {userEmail}</p>
        </div>
        <button className="secondary-button compact" type="button" onClick={onLogout}>
          Odjavi se
        </button>
      </header>

      <section className="form-section">
        <h2>Novi plan putovanja</h2>
        <TripForm onCreated={handleTripCreated} />
      </section>

      {isLoading && <p className="state-message">Ucitavanje planova...</p>}
      {error && <p className="form-error">{error}</p>}
      {!isLoading && !error && trips.length === 0 && (
        <p className="state-message">Jos nema kreiranih planova putovanja.</p>
      )}

      <div className="trip-grid">
        {trips.map((trip) => (
          <article className="trip-card" key={trip.id}>
            <h2>{trip.title}</h2>
            <p>
              {formatDate(trip.startDate)} - {formatDate(trip.endDate)}
            </p>
            <dl>
              <div>
                <dt>Budzet</dt>
                <dd>{formatCurrency(trip.plannedBudget)}</dd>
              </div>
              <div>
                <dt>Troskovi</dt>
                <dd>{formatCurrency(trip.totalExpenses)}</dd>
              </div>
              <div>
                <dt>Preostalo</dt>
                <dd>{formatCurrency(trip.remainingBudget)}</dd>
              </div>
            </dl>
            <button className="secondary-button compact" type="button" onClick={() => onOpenTrip(trip.id)}>
              Otvori detalje
            </button>
          </article>
        ))}
      </div>
    </section>
  );
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("sr-Latn-BA").format(new Date(value));
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("sr-Latn-BA", {
    style: "currency",
    currency: "EUR"
  }).format(value);
}
