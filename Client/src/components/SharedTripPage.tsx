import { useEffect, useMemo, useState } from "react";
import { ActivityStatus, ExpenseCategory, ShareAccessLevel, SharedTrip, Trip, TripFormData } from "../models";
import { ApiError, useServices } from "../services";
import { TripForm } from "./TripForm";

interface SharedTripPageProps {
  token: string;
}

const activityStatusLabels: Record<ActivityStatus, string> = {
  [ActivityStatus.Planned]: "Planirano",
  [ActivityStatus.Reserved]: "Rezervisano",
  [ActivityStatus.Completed]: "Zavrseno",
  [ActivityStatus.Cancelled]: "Otkazano"
};

const expenseCategoryLabels: Record<ExpenseCategory, string> = {
  [ExpenseCategory.Transport]: "Prevoz",
  [ExpenseCategory.Accommodation]: "Smjestaj",
  [ExpenseCategory.Food]: "Hrana",
  [ExpenseCategory.Tickets]: "Ulaznice",
  [ExpenseCategory.Shopping]: "Kupovina",
  [ExpenseCategory.Other]: "Ostalo"
};

const accessLabels: Record<ShareAccessLevel, string> = {
  [ShareAccessLevel.View]: "Samo pregled",
  [ShareAccessLevel.Edit]: "Pregled i izmjena"
};

export function SharedTripPage({ token }: SharedTripPageProps) {
  const { sharingService } = useServices();
  const [sharedTrip, setSharedTrip] = useState<SharedTrip | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;

    async function loadSharedTrip() {
      try {
        const loadedTrip = await sharingService.getSharedTrip(token);
        if (isMounted) {
          setSharedTrip(loadedTrip);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Podijeljeni plan nije ucitan.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadSharedTrip();

    return () => {
      isMounted = false;
    };
  }, [token, sharingService]);

  const activitiesByDate = useMemo(
    () => groupActivitiesByDate(sharedTrip?.activities ?? []),
    [sharedTrip?.activities]
  );

  function handleTripSaved(trip: Trip) {
    setSharedTrip((currentSharedTrip) => (currentSharedTrip ? { ...currentSharedTrip, trip } : currentSharedTrip));
    setSaveMessage("Plan je sacuvan.");
  }

  if (isLoading) {
    return <p className="state-message">Ucitavanje podijeljenog plana...</p>;
  }

  if (error) {
    return <p className="form-error">{error}</p>;
  }

  if (!sharedTrip) {
    return <p className="state-message">Plan nije pronadjen.</p>;
  }

  return (
    <article className="details-panel shared-panel">
      <header className="details-header">
        <p className="eyebrow">Podijeljeni plan</p>
        <h1>{sharedTrip.trip.title}</h1>
        <p className="summary">
          {formatDate(sharedTrip.trip.startDate)} - {formatDate(sharedTrip.trip.endDate)}
        </p>
        <span className="status-pill">{accessLabels[sharedTrip.accessLevel]}</span>
      </header>

      <div className="details-grid">
        <section>
          <h2>Opis</h2>
          <p>{sharedTrip.trip.description || "Nema opisa."}</p>
        </section>
        <section>
          <h2>Napomene</h2>
          <p>{sharedTrip.trip.notes || "Nema napomena."}</p>
        </section>
        <section>
          <h2>Budzet</h2>
          <dl>
            <div>
              <dt>Planirano</dt>
              <dd>{formatCurrency(sharedTrip.trip.plannedBudget)}</dd>
            </div>
            <div>
              <dt>Potroseno</dt>
              <dd>{formatCurrency(sharedTrip.trip.totalExpenses)}</dd>
            </div>
            <div>
              <dt>Preostalo</dt>
              <dd>{formatCurrency(sharedTrip.trip.remainingBudget)}</dd>
            </div>
          </dl>
        </section>
      </div>

      {sharedTrip.accessLevel === ShareAccessLevel.Edit && (
        <section className="management-section">
          <div className="section-header">
            <h2>Izmjena plana</h2>
          </div>
          {saveMessage && <p className="success-message">{saveMessage}</p>}
          <TripForm
            initialData={toTripFormData(sharedTrip.trip)}
            submitLabel="Sacuvaj izmjene"
            onSubmitTrip={(form) => sharingService.updateSharedTrip(token, form)}
            onSaved={handleTripSaved}
          />
        </section>
      )}

      <ReadOnlySection title="Destinacije" emptyText="Nema destinacija.">
        {sharedTrip.destinations.map((destination) => (
          <article className="list-item" key={destination.id}>
            <h3>{destination.name}</h3>
            <p>{destination.location}</p>
            <p>
              {formatDate(destination.arrivalDate)} - {formatDate(destination.departureDate)}
            </p>
            <p>{destination.description || "Nema opisa."}</p>
          </article>
        ))}
      </ReadOnlySection>

      <ReadOnlySection title="Aktivnosti" emptyText="Nema aktivnosti.">
        {activitiesByDate.map((group) => (
          <section className="calendar-day" key={group.date}>
            <h3>{formatDate(group.date)}</h3>
            <div className="item-list">
              {group.activities.map((activity) => (
                <article className="list-item" key={activity.id}>
                  <div className="activity-heading">
                    <div>
                      <h4>{activity.title}</h4>
                      <p>
                        {activity.time || "Bez vremena"} - {activity.location || "Bez lokacije"}
                      </p>
                    </div>
                    <span className="status-pill">{activityStatusLabels[activity.status]}</span>
                  </div>
                  <p>{activity.description || "Nema opisa."}</p>
                  <p>Procjena: {formatCurrency(activity.estimatedCost)}</p>
                </article>
              ))}
            </div>
          </section>
        ))}
      </ReadOnlySection>

      <ReadOnlySection title="Troskovi" emptyText="Nema troskova.">
        {sharedTrip.expenses.map((expense) => (
          <article className="list-item" key={expense.id}>
            <div className="activity-heading">
              <div>
                <h3>{expense.name}</h3>
                <p>
                  {expenseCategoryLabels[expense.category]} - {formatDate(expense.date)}
                </p>
              </div>
              <strong>{formatCurrency(expense.amount)}</strong>
            </div>
            <p>{expense.description || "Nema opisa."}</p>
          </article>
        ))}
      </ReadOnlySection>

      <ReadOnlySection
        title="Checklist"
        emptyText="Nema checklist stavki."
        isEmpty={sharedTrip.checklistItems.length === 0}
      >
        <div className="checklist">
          {sharedTrip.checklistItems.map((item) => (
            <article className="checklist-item" key={item.id}>
              <span className={item.isCompleted ? "completed-text" : undefined}>{item.text}</span>
              <span className="status-pill">{item.isCompleted ? "Zavrseno" : "Otvoreno"}</span>
            </article>
          ))}
        </div>
      </ReadOnlySection>
    </article>
  );
}

function ReadOnlySection({
  title,
  emptyText,
  isEmpty,
  children
}: {
  title: string;
  emptyText: string;
  isEmpty?: boolean;
  children: React.ReactNode;
}) {
  const childArray = Array.isArray(children) ? children : [children];
  const hasContent = isEmpty === undefined ? childArray.some(Boolean) : !isEmpty;

  return (
    <section className="management-section">
      <div className="section-header">
        <h2>{title}</h2>
      </div>
      {hasContent ? <div className="item-list">{children}</div> : <p className="state-message">{emptyText}</p>}
    </section>
  );
}

function groupActivitiesByDate(activities: SharedTrip["activities"]) {
  const groups = new Map<string, SharedTrip["activities"]>();

  [...activities]
    .sort((first, second) => `${first.date} ${first.time}`.localeCompare(`${second.date} ${second.time}`))
    .forEach((activity) => {
      const date = activity.date.slice(0, 10);
      groups.set(date, [...(groups.get(date) ?? []), activity]);
    });

  return Array.from(groups.entries()).map(([date, groupActivities]) => ({
    date,
    activities: groupActivities
  }));
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

function toTripFormData(trip: Trip): TripFormData {
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
