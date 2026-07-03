import { FormEvent, useEffect, useState } from "react";
import { Destination, DestinationFormData } from "../models";
import { ApiError, tripService } from "../services";

interface DestinationsSectionProps {
  tripId: string;
}

const initialForm: DestinationFormData = {
  name: "",
  location: "",
  arrivalDate: "",
  departureDate: "",
  description: ""
};

export function DestinationsSection({ tripId }: DestinationsSectionProps) {
  const [destinations, setDestinations] = useState<Destination[]>([]);
  const [form, setForm] = useState<DestinationFormData>(initialForm);
  const [editingDestinationId, setEditingDestinationId] = useState<string | null>(null);
  const [deletingDestinationId, setDeletingDestinationId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let isMounted = true;

    async function loadDestinations() {
      try {
        const loadedDestinations = await tripService.getDestinations(tripId);
        if (isMounted) {
          setDestinations(loadedDestinations);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Destinacije nisu ucitane.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadDestinations();

    return () => {
      isMounted = false;
    };
  }, [tripId]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const validationError = validateDestination(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      if (editingDestinationId) {
        const destination = await tripService.updateDestination(tripId, editingDestinationId, form);
        setDestinations((currentDestinations) =>
          currentDestinations.map((currentDestination) =>
            currentDestination.id === destination.id ? destination : currentDestination
          )
        );
        setEditingDestinationId(null);
      } else {
        const destination = await tripService.createDestination(tripId, form);
        setDestinations((currentDestinations) => [...currentDestinations, destination]);
      }

      setForm(initialForm);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Destinacija nije sacuvana.");
    } finally {
      setIsSubmitting(false);
    }
  }

  function startEditing(destination: Destination) {
    setEditingDestinationId(destination.id);
    setError(null);
    setForm({
      name: destination.name,
      location: destination.location,
      arrivalDate: toDateInputValue(destination.arrivalDate),
      departureDate: toDateInputValue(destination.departureDate),
      description: destination.description ?? ""
    });
  }

  function cancelEditing() {
    setEditingDestinationId(null);
    setError(null);
    setForm(initialForm);
  }

  async function handleDelete(destinationId: string) {
    setError(null);
    setDeletingDestinationId(destinationId);

    try {
      await tripService.deleteDestination(tripId, destinationId);
      setDestinations((currentDestinations) =>
        currentDestinations.filter((destination) => destination.id !== destinationId)
      );

      if (editingDestinationId === destinationId) {
        cancelEditing();
      }
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Destinacija nije obrisana.");
    } finally {
      setDeletingDestinationId(null);
    }
  }

  return (
    <section className="management-section">
      <div className="section-header">
        <h2>Destinacije</h2>
        {isLoading && <span>Ucitavanje...</span>}
      </div>

      {error && <p className="form-error">{error}</p>}

      <form className="trip-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <label>
            Naziv
            <input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          </label>
          <label>
            Lokacija
            <input value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} />
          </label>
          <label>
            Dolazak
            <input
              type="date"
              value={form.arrivalDate}
              onChange={(event) => setForm({ ...form, arrivalDate: event.target.value })}
            />
          </label>
          <label>
            Odlazak
            <input
              type="date"
              value={form.departureDate}
              onChange={(event) => setForm({ ...form, departureDate: event.target.value })}
            />
          </label>
        </div>
        <label>
          Opis ili napomena
          <textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
        </label>
        <div className="form-actions">
          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? "Cuvanje..." : editingDestinationId ? "Sacuvaj izmjene" : "Dodaj destinaciju"}
          </button>
          {editingDestinationId && (
            <button className="secondary-button inline" disabled={isSubmitting} type="button" onClick={cancelEditing}>
              Otkazi
            </button>
          )}
        </div>
      </form>

      {destinations.length === 0 && !isLoading && <p className="state-message">Nema destinacija.</p>}
      <div className="item-list">
        {destinations.map((destination) => (
          <article className="list-item" key={destination.id}>
            <div>
              <h3>{destination.name}</h3>
              <p>{destination.location}</p>
              <p>
                {formatDate(destination.arrivalDate)} - {formatDate(destination.departureDate)}
              </p>
            </div>
            <p>{destination.description}</p>
            <div className="item-actions">
              <button className="secondary-button inline" type="button" onClick={() => startEditing(destination)}>
                Uredi
              </button>
              <button
                className="danger-button"
                disabled={deletingDestinationId === destination.id}
                type="button"
                onClick={() => handleDelete(destination.id)}
              >
                {deletingDestinationId === destination.id ? "Brisanje..." : "Obrisi"}
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}

function validateDestination(form: DestinationFormData) {
  if (!form.name.trim() || !form.location.trim()) {
    return "Naziv i lokacija su obavezni.";
  }

  if (!form.arrivalDate || !form.departureDate) {
    return "Datumi dolaska i odlaska su obavezni.";
  }

  if (form.departureDate < form.arrivalDate) {
    return "Datum odlaska ne moze biti prije datuma dolaska.";
  }

  return null;
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("sr-Latn-BA").format(new Date(value));
}

function toDateInputValue(value: string) {
  return value.slice(0, 10);
}
