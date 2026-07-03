import { useEffect, useState } from "react";
import { Trip } from "../models";
import { ApiError, tripService } from "../services";

interface TripDetailsPageProps {
  tripId: string;
  onBack: () => void;
}

export function TripDetailsPage({ tripId, onBack }: TripDetailsPageProps) {
  const [trip, setTrip] = useState<Trip | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;

    async function loadTrip() {
      try {
        const loadedTrip = await tripService.getTrip(tripId);
        if (isMounted) {
          setTrip(loadedTrip);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Plan nije ucitan.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadTrip();

    return () => {
      isMounted = false;
    };
  }, [tripId]);

  return (
    <section className="page-panel">
      <button className="secondary-button compact" type="button" onClick={onBack}>
        Nazad na planove
      </button>

      {isLoading && <p className="state-message">Ucitavanje plana...</p>}
      {error && <p className="form-error">{error}</p>}

      {trip && (
        <article className="details-panel">
          <header className="details-header">
            <div>
              <p className="eyebrow">Detalji putovanja</p>
              <h1>{trip.title}</h1>
              <p className="summary">
                {formatDate(trip.startDate)} - {formatDate(trip.endDate)}
              </p>
            </div>
          </header>

          <div className="details-grid">
            <section>
              <h2>Opis</h2>
              <p>{trip.description || "Nema opisa."}</p>
            </section>
            <section>
              <h2>Napomene</h2>
              <p>{trip.notes || "Nema napomena."}</p>
            </section>
            <section>
              <h2>Budzet</h2>
              <dl>
                <div>
                  <dt>Planirano</dt>
                  <dd>{formatCurrency(trip.plannedBudget)}</dd>
                </div>
                <div>
                  <dt>Potroseno</dt>
                  <dd>{formatCurrency(trip.totalExpenses)}</dd>
                </div>
                <div>
                  <dt>Preostalo</dt>
                  <dd>{formatCurrency(trip.remainingBudget)}</dd>
                </div>
              </dl>
            </section>
          </div>
        </article>
      )}
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
