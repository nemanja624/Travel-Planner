import { FormEvent, useEffect, useMemo, useState } from "react";
import { ActivityStatus } from "../models";
import { ActivityFormData, TripActivity } from "../models/trips";
import { ApiError, useServices } from "../services";

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

const weekDays = ["Pon", "Uto", "Sri", "Cet", "Pet", "Sub", "Ned"];

export function ActivitiesSection({ tripId }: ActivitiesSectionProps) {
  const { tripService } = useServices();
  const [activities, setActivities] = useState<TripActivity[]>([]);
  const [form, setForm] = useState<ActivityFormData>(initialForm);
  const [visibleMonth, setVisibleMonth] = useState(() => getCurrentMonthValue());
  const [editingActivityId, setEditingActivityId] = useState<string | null>(null);
  const [deletingActivityId, setDeletingActivityId] = useState<string | null>(null);
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
  }, [tripId, tripService]);

  const activityGroups = useMemo(() => groupActivitiesByDate(activities), [activities]);
  const calendarDays = useMemo(() => buildCalendarDays(visibleMonth, activities), [activities, visibleMonth]);
  const visibleMonthLabel = useMemo(() => formatMonthLabel(visibleMonth), [visibleMonth]);

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
      if (editingActivityId) {
        const activity = await tripService.updateActivity(tripId, editingActivityId, form);
        setActivities((currentActivities) =>
          currentActivities.map((currentActivity) => (currentActivity.id === activity.id ? activity : currentActivity))
        );
        setEditingActivityId(null);
      } else {
        const activity = await tripService.createActivity(tripId, form);
        setActivities((currentActivities) => [...currentActivities, activity]);
      }

      setVisibleMonth(form.date.slice(0, 7));
      setForm(initialForm);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Aktivnost nije sacuvana.");
    } finally {
      setIsSubmitting(false);
    }
  }

  function startEditing(activity: TripActivity) {
    setEditingActivityId(activity.id);
    setError(null);
    setForm({
      title: activity.title,
      date: toDateInputValue(activity.date),
      time: toTimeInputValue(activity.time),
      location: activity.location,
      description: activity.description,
      estimatedCost: activity.estimatedCost,
      status: activity.status
    });
  }

  function cancelEditing() {
    setEditingActivityId(null);
    setError(null);
    setForm(initialForm);
  }

  async function handleDelete(activityId: string) {
    setError(null);
    setDeletingActivityId(activityId);

    try {
      await tripService.deleteActivity(tripId, activityId);
      setActivities((currentActivities) => currentActivities.filter((activity) => activity.id !== activityId));

      if (editingActivityId === activityId) {
        cancelEditing();
      }
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Aktivnost nije obrisana.");
    } finally {
      setDeletingActivityId(null);
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
            {isSubmitting ? "Cuvanje..." : editingActivityId ? "Sacuvaj izmjene" : "Dodaj aktivnost"}
          </button>
          {editingActivityId && (
            <button className="secondary-button inline" disabled={isSubmitting} type="button" onClick={cancelEditing}>
              Otkazi
            </button>
          )}
        </div>
      </form>

      {activities.length === 0 && !isLoading && <p className="state-message">Nema aktivnosti.</p>}
      <section className="activity-calendar" aria-label="Kalendar aktivnosti">
        <header className="calendar-toolbar">
          <button
            className="secondary-button compact"
            type="button"
            onClick={() => setVisibleMonth(shiftMonth(visibleMonth, -1))}
          >
            Prethodni
          </button>
          <h3>{visibleMonthLabel}</h3>
          <button
            className="secondary-button compact"
            type="button"
            onClick={() => setVisibleMonth(shiftMonth(visibleMonth, 1))}
          >
            Sljedeci
          </button>
        </header>

        <div className="calendar-grid">
          {weekDays.map((day) => (
            <div className="calendar-weekday" key={day}>
              {day}
            </div>
          ))}
          {calendarDays.map((day) => (
            <article
              className={`calendar-cell${day.isCurrentMonth ? "" : " muted"}${day.activities.length > 0 ? " has-activities" : ""}`}
              key={day.date}
            >
              <span className="calendar-date">{day.dayNumber}</span>
              <div className="calendar-events">
                {day.activities.slice(0, 3).map((activity) => (
                  <button
                    className="calendar-event"
                    key={activity.id}
                    title={`${activity.title} - ${activity.location || "Bez lokacije"}`}
                    type="button"
                    onClick={() => startEditing(activity)}
                  >
                    {activity.time ? `${toTimeInputValue(activity.time)} ` : ""}
                    {activity.title}
                  </button>
                ))}
                {day.activities.length > 3 && (
                  <span className="calendar-more">+{day.activities.length - 3}</span>
                )}
              </div>
            </article>
          ))}
        </div>
      </section>

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
                  <div className="item-actions">
                    <button className="secondary-button inline" type="button" onClick={() => startEditing(activity)}>
                      Uredi
                    </button>
                    <button
                      className="danger-button"
                      disabled={deletingActivityId === activity.id}
                      type="button"
                      onClick={() => handleDelete(activity.id)}
                    >
                      {deletingActivityId === activity.id ? "Brisanje..." : "Obrisi"}
                    </button>
                  </div>
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

  if (!form.time) {
    return "Vrijeme aktivnosti je obavezno.";
  }

  if (form.estimatedCost < 0) {
    return "Procijenjeni trosak ne moze biti negativan.";
  }

  return null;
}

function buildCalendarDays(monthValue: string, activities: TripActivity[]) {
  const [year, month] = monthValue.split("-").map(Number);
  const monthIndex = month - 1;
  const firstDay = new Date(year, monthIndex, 1);
  const mondayOffset = (firstDay.getDay() + 6) % 7;
  const cursor = new Date(year, monthIndex, 1 - mondayOffset);
  const activitiesByDate = new Map<string, TripActivity[]>();

  [...activities]
    .sort((first, second) => `${first.date} ${first.time}`.localeCompare(`${second.date} ${second.time}`))
    .forEach((activity) => {
      const date = toDateInputValue(activity.date);
      activitiesByDate.set(date, [...(activitiesByDate.get(date) ?? []), activity]);
    });

  return Array.from({ length: 42 }, (_, index) => {
    const date = new Date(cursor);
    date.setDate(cursor.getDate() + index);
    const dateValue = toDateInputValueFromDate(date);

    return {
      date: dateValue,
      dayNumber: date.getDate(),
      isCurrentMonth: date.getMonth() === monthIndex,
      activities: activitiesByDate.get(dateValue) ?? []
    };
  });
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

function getCurrentMonthValue() {
  const now = new Date();
  return `${now.getFullYear()}-${padDatePart(now.getMonth() + 1)}`;
}

function shiftMonth(monthValue: string, offset: number) {
  const [year, month] = monthValue.split("-").map(Number);
  const date = new Date(year, month - 1 + offset, 1);
  return `${date.getFullYear()}-${padDatePart(date.getMonth() + 1)}`;
}

function formatMonthLabel(monthValue: string) {
  const [year, month] = monthValue.split("-").map(Number);
  return new Intl.DateTimeFormat("sr-Latn-BA", {
    month: "long",
    year: "numeric"
  }).format(new Date(year, month - 1, 1));
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

function toDateInputValueFromDate(value: Date) {
  return `${value.getFullYear()}-${padDatePart(value.getMonth() + 1)}-${padDatePart(value.getDate())}`;
}

function toTimeInputValue(value: string) {
  return value.slice(0, 5);
}

function padDatePart(value: number) {
  return value.toString().padStart(2, "0");
}
