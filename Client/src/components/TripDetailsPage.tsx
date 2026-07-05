import { ReactNode, useEffect, useState } from "react";
import { Trip } from "../models";
import { ApiError, useServices } from "../services";
import { ActivitiesSection } from "./ActivitiesSection";
import { ChecklistSection } from "./ChecklistSection";
import { DestinationsSection } from "./DestinationsSection";
import { ExpensesSection } from "./ExpensesSection";
import { SharingSection } from "./SharingSection";

interface TripDetailsPageProps {
  tripId: string;
  onBack: () => void;
}

export function TripDetailsPage({ tripId, onBack }: TripDetailsPageProps) {
  const { tripService } = useServices();
  const [trip, setTrip] = useState<Trip | null>(null);
  const [activeSection, setActiveSection] = useState<DetailSectionId>("activities");
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
  }, [tripId, tripService]);

  return (
    <section className="page-panel">
      <button className="secondary-button compact" type="button" onClick={onBack}>
        Nazad na planove
      </button>

      {isLoading && <p className="state-message">Ucitavanje plana...</p>}
      {error && <p className="form-error">{error}</p>}

      {trip && (
        <article className="details-panel">
          <header className="details-header trip-hero">
            <div>
              <p className="eyebrow">Detalji putovanja</p>
              <h1>{trip.title}</h1>
              <p className="summary">
                {formatDate(trip.startDate)} - {formatDate(trip.endDate)}
              </p>
            </div>
            <div className="hero-budget">
              <span>Preostalo</span>
              <strong>{formatCurrency(trip.remainingBudget)}</strong>
            </div>
          </header>

          <div className="details-grid compact-overview">
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

          <nav className="detail-tabs" aria-label="Dijelovi plana putovanja">
            {detailSections.map((section) => (
              <button
                className={activeSection === section.id ? "active" : undefined}
                key={section.id}
                type="button"
                onClick={() => setActiveSection(section.id)}
              >
                {section.label}
              </button>
            ))}
          </nav>

          <div className="detail-workspace">{renderActiveSection(activeSection, trip.id)}</div>
        </article>
      )}
    </section>
  );
}

type DetailSectionId = "destinations" | "activities" | "expenses" | "checklist" | "sharing";

const detailSections: Array<{ id: DetailSectionId; label: string }> = [
  { id: "activities", label: "Aktivnosti" },
  { id: "destinations", label: "Destinacije" },
  { id: "expenses", label: "Troskovi" },
  { id: "checklist", label: "Checklist" },
  { id: "sharing", label: "Dijeljenje" }
];

function renderActiveSection(activeSection: DetailSectionId, tripId: string): ReactNode {
  switch (activeSection) {
    case "destinations":
      return <DestinationsSection tripId={tripId} />;
    case "expenses":
      return <ExpensesSection tripId={tripId} />;
    case "checklist":
      return <ChecklistSection tripId={tripId} />;
    case "sharing":
      return <SharingSection tripId={tripId} />;
    case "activities":
    default:
      return <ActivitiesSection tripId={tripId} />;
  }
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
