import { FormEvent, useEffect, useState } from "react";
import { Trip, TripFormData } from "../models";
import { ApiError, useServices } from "../services";

interface TripFormProps {
  tripId?: string;
  initialData?: TripFormData;
  submitLabel?: string;
  onCancel?: () => void;
  onSubmitTrip?: (form: TripFormData) => Promise<Trip>;
  onSaved: (trip: Trip) => void;
}

const initialForm: TripFormData = {
  title: "",
  description: "",
  startDate: "",
  endDate: "",
  plannedBudget: 0,
  notes: ""
};

export function TripForm({ tripId, initialData, submitLabel, onCancel, onSubmitTrip, onSaved }: TripFormProps) {
  const { tripService } = useServices();
  const [form, setForm] = useState<TripFormData>(initialData ?? initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    setForm(initialData ?? initialForm);
    setError(null);
  }, [initialData]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const validationError = validateTripForm(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      const savedTrip = onSubmitTrip
        ? await onSubmitTrip(form)
        : tripId
          ? await tripService.updateTrip(tripId, form)
          : await tripService.createTrip(form);
      onSaved(savedTrip);
      if (!tripId && !onSubmitTrip) {
        setForm(initialForm);
      }
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Plan nije sacuvan.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="trip-form" onSubmit={handleSubmit}>
      <div className="form-grid">
        <label>
          Naziv putovanja
          <input
            value={form.title}
            onChange={(event) => setForm({ ...form, title: event.target.value })}
          />
        </label>
        <label>
          Planirani budzet
          <input
            min="0"
            step="0.01"
            type="number"
            value={form.plannedBudget}
            onChange={(event) => setForm({ ...form, plannedBudget: Number(event.target.value) })}
          />
        </label>
        <label>
          Pocetni datum
          <input
            type="date"
            value={form.startDate}
            onChange={(event) => setForm({ ...form, startDate: event.target.value })}
          />
        </label>
        <label>
          Krajnji datum
          <input
            type="date"
            value={form.endDate}
            onChange={(event) => setForm({ ...form, endDate: event.target.value })}
          />
        </label>
      </div>

      <label>
        Kratak opis
        <textarea
          value={form.description}
          onChange={(event) => setForm({ ...form, description: event.target.value })}
        />
      </label>

      <label>
        Napomene
        <textarea
          value={form.notes}
          onChange={(event) => setForm({ ...form, notes: event.target.value })}
        />
      </label>

      {error && <p className="form-error">{error}</p>}

      <div className="form-actions">
        <button className="primary-button" disabled={isSubmitting} type="submit">
          {isSubmitting ? "Cuvanje..." : submitLabel ?? "Dodaj plan"}
        </button>
        {onCancel && (
          <button className="secondary-button inline" disabled={isSubmitting} type="button" onClick={onCancel}>
            Otkazi
          </button>
        )}
      </div>
    </form>
  );
}

function validateTripForm(form: TripFormData) {
  if (!form.title.trim()) {
    return "Naziv putovanja je obavezan.";
  }

  if (!form.startDate || !form.endDate) {
    return "Pocetni i krajnji datum su obavezni.";
  }

  if (form.endDate < form.startDate) {
    return "Krajnji datum ne moze biti prije pocetnog datuma.";
  }

  if (form.plannedBudget < 0) {
    return "Budzet ne moze biti negativan.";
  }

  return null;
}
