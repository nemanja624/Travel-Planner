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
  const [editingTrip, setEditingTrip] = useState<Trip | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [editingTripId, setEditingTripId] = useState<string | null>(null);
  const [deletingTripId, setDeletingTripId] = useState<string | null>(null);

  function handleTripSaved(trip: Trip) {
    const summary = toTripSummary(trip);

    setTrips((currentTrips) =>
      editingTrip
        ? currentTrips.map((currentTrip) => (currentTrip.id === summary.id ? summary : currentTrip))
        : [summary, ...currentTrips]
    );
    setEditingTrip(null);
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

  async function startEditing(tripId: string) {
    setError(null);
    setEditingTripId(tripId);

    try {
      const trip = await tripService.getTrip(tripId);
      setEditingTrip(trip);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Plan nije ucitan za izmjenu.");
    } finally {
      setEditingTripId(null);
    }
  }

  async function deleteTrip(tripId: string) {
    setError(null);
    setDeletingTripId(tripId);

    try {
      await tripService.deleteTrip(tripId);
      setTrips((currentTrips) => currentTrips.filter((trip) => trip.id !== tripId));
      if (editingTrip?.id === tripId) {
        setEditingTrip(null);
      }
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Plan nije obrisan.");
    } finally {
      setDeletingTripId(null);
    }
  }

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
        <h2>{editingTrip ? "Izmjena plana putovanja" : "Novi plan putovanja"}</h2>
        <TripForm
          initialData={editingTrip ? toTripFormData(editingTrip) : undefined}
          submitLabel={editingTrip ? "Sacuvaj izmjene" : "Dodaj plan"}
          tripId={editingTrip?.id}
          onCancel={editingTrip ? () => setEditingTrip(null) : undefined}
          onSaved={handleTripSaved}
        />
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
            <div className="item-actions">
              <button className="secondary-button compact" type="button" onClick={() => onOpenTrip(trip.id)}>
                Otvori detalje
              </button>
              <button
                className="secondary-button compact"
                disabled={editingTripId === trip.id}
                type="button"
                onClick={() => startEditing(trip.id)}
              >
                {editingTripId === trip.id ? "Ucitavanje..." : "Uredi"}
              </button>
              <button
                className="danger-button"
                disabled={deletingTripId === trip.id}
                type="button"
                onClick={() => deleteTrip(trip.id)}
              >
                {deletingTripId === trip.id ? "Brisanje..." : "Obrisi"}
              </button>
            </div>
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

function toTripSummary(trip: Trip): TripSummary {
  return {
    id: trip.id,
    title: trip.title,
    startDate: trip.startDate,
    endDate: trip.endDate,
    plannedBudget: trip.plannedBudget,
    totalExpenses: trip.totalExpenses,
    remainingBudget: trip.remainingBudget
  };
}

function toTripFormData(trip: Trip) {
  return {
    title: trip.title,
    description: trip.description,
    startDate: toDateInputValue(trip.startDate),
    endDate: toDateInputValue(trip.endDate),
    plannedBudget: trip.plannedBudget,
    notes: trip.notes
  };
}

function toDateInputValue(value: string) {
  return value.slice(0, 10);
}
