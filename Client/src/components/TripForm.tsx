import { FormEvent, useState } from "react";
import { Trip, TripFormData } from "../models";
import { ApiError, tripService } from "../services";

interface TripFormProps {
  onCreated: (trip: Trip) => void;
}

const initialForm: TripFormData = {
  title: "",
  description: "",
  startDate: "",
  endDate: "",
  plannedBudget: 0,
  notes: ""
};

export function TripForm({ onCreated }: TripFormProps) {
  const [form, setForm] = useState<TripFormData>(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

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
      const createdTrip = await tripService.createTrip(form);
      onCreated(createdTrip);
      setForm(initialForm);
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

      <button className="primary-button" disabled={isSubmitting} type="submit">
        {isSubmitting ? "Cuvanje..." : "Dodaj plan"}
      </button>
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
