import { FormEvent, useEffect, useMemo, useState } from "react";
import { ActivityStatus } from "../models";
import { ActivityFormData, TripActivity } from "../models/trips";
import { ApiError, tripService } from "../services";

interface ActivitiesSectionProps {
  tripId: string;
}

const initialForm: ActivityFormData = {
  title: "",
  date: "",
  time: "",
  location: "",
  description: "",
  estimatedCost: 0,
  status: ActivityStatus.Planned
};

const statusLabels: Record<ActivityStatus, string> = {
  [ActivityStatus.Planned]: "Planirano",
  [ActivityStatus.Reserved]: "Rezervisano",
  [ActivityStatus.Completed]: "Zavrseno",
  [ActivityStatus.Cancelled]: "Otkazano"
};

export function ActivitiesSection({ tripId }: ActivitiesSectionProps) {
  const [activities, setActivities] = useState<TripActivity[]>([]);
  const [form, setForm] = useState<ActivityFormData>(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let isMounted = true;

    async function loadActivities() {
      try {
        const loadedActivities = await tripService.getActivities(tripId);
        if (isMounted) {
          setActivities(loadedActivities);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Aktivnosti nisu ucitane.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadActivities();

    return () => {
      isMounted = false;
    };
  }, [tripId]);

  const activityGroups = useMemo(() => groupActivitiesByDate(activities), [activities]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const validationError = validateActivity(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      const activity = await tripService.createActivity(tripId, form);
      setActivities((currentActivities) => [...currentActivities, activity]);
      setForm(initialForm);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Aktivnost nije sacuvana.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="management-section">
      <div className="section-header">
        <h2>Aktivnosti</h2>
        {isLoading && <span>Ucitavanje...</span>}
      </div>

      {error && <p className="form-error">{error}</p>}

      <form className="trip-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <label>
            Naziv
            <input value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
          </label>
          <label>
            Datum
            <input
              type="date"
              value={form.date}
              onChange={(event) => setForm({ ...form, date: event.target.value })}
            />
          </label>
          <label>
            Vrijeme
            <input
              type="time"
              value={form.time}
              onChange={(event) => setForm({ ...form, time: event.target.value })}
            />
          </label>
          <label>
            Lokacija
            <input value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} />
          </label>
          <label>
            Procijenjeni trosak
            <input
              min="0"
              step="0.01"
              type="number"
              value={form.estimatedCost}
              onChange={(event) => setForm({ ...form, estimatedCost: Number(event.target.value) })}
            />
          </label>
          <label>
            Status
            <select
              value={form.status}
              onChange={(event) => setForm({ ...form, status: Number(event.target.value) as ActivityStatus })}
            >
              {Object.values(ActivityStatus)
                .filter((status): status is ActivityStatus => typeof status === "number")
                .map((status) => (
                  <option key={status} value={status}>
                    {statusLabels[status]}
                  </option>
                ))}
            </select>
          </label>
        </div>
        <label>
          Opis
          <textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
        </label>
        <div className="form-actions">
          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? "Cuvanje..." : "Dodaj aktivnost"}
          </button>
        </div>
      </form>

      {activities.length === 0 && !isLoading && <p className="state-message">Nema aktivnosti.</p>}
      <div className="calendar-list">
        {activityGroups.map((group) => (
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
                    <span className="status-pill">{statusLabels[activity.status]}</span>
                  </div>
                  <p>{activity.description || "Nema opisa."}</p>
                  <p>Procjena: {formatCurrency(activity.estimatedCost)}</p>
                </article>
              ))}
            </div>
          </section>
        ))}
      </div>
    </section>
  );
}

function validateActivity(form: ActivityFormData) {
  if (!form.title.trim()) {
    return "Naziv aktivnosti je obavezan.";
  }

  if (!form.date) {
    return "Datum aktivnosti je obavezan.";
  }

  if (form.estimatedCost < 0) {
    return "Procijenjeni trosak ne moze biti negativan.";
  }

  return null;
}

function groupActivitiesByDate(activities: TripActivity[]) {
  const groups = new Map<string, TripActivity[]>();

  [...activities]
    .sort((first, second) => `${first.date} ${first.time}`.localeCompare(`${second.date} ${second.time}`))
    .forEach((activity) => {
      const date = toDateInputValue(activity.date);
      groups.set(date, [...(groups.get(date) ?? []), activity]);
    });

  return Array.from(groups.entries()).map(([date, groupActivities]) => ({
    date,
    activities: groupActivities
  }));
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("sr-Latn-BA", {
    weekday: "long",
    day: "2-digit",
    month: "2-digit",
    year: "numeric"
  }).format(new Date(value));
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("sr-Latn-BA", {
    style: "currency",
    currency: "EUR"
  }).format(value);
}

function toDateInputValue(value: string) {
  return value.slice(0, 10);
}
