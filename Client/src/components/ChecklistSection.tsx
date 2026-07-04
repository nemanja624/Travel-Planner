import { FormEvent, useEffect, useState } from "react";
import { ChecklistItem } from "../models";
import { ApiError, useServices } from "../services";

interface ChecklistSectionProps {
  tripId: string;
}

export function ChecklistSection({ tripId }: ChecklistSectionProps) {
  const { tripService } = useServices();
  const [items, setItems] = useState<ChecklistItem[]>([]);
  const [text, setText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [updatingItemId, setUpdatingItemId] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function loadItems() {
      try {
        const loadedItems = await tripService.getChecklistItems(tripId);
        if (isMounted) {
          setItems(loadedItems);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Checklist nije ucitana.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadItems();

    return () => {
      isMounted = false;
    };
  }, [tripId, tripService]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (!text.trim()) {
      setError("Tekst stavke je obavezan.");
      return;
    }

    setIsSubmitting(true);
    try {
      const item = await tripService.createChecklistItem(tripId, { text: text.trim() });
      setItems((currentItems) => [...currentItems, item]);
      setText("");
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Stavka nije sacuvana.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function toggleItem(item: ChecklistItem) {
    setError(null);
    setUpdatingItemId(item.id);

    try {
      const updatedItem = await tripService.updateChecklistItem(tripId, item.id, {
        text: item.text,
        isCompleted: !item.isCompleted
      });

      setItems((currentItems) =>
        currentItems.map((currentItem) => (currentItem.id === updatedItem.id ? updatedItem : currentItem))
      );
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Stavka nije azurirana.");
    } finally {
      setUpdatingItemId(null);
    }
  }

  async function deleteItem(itemId: string) {
    setError(null);
    setUpdatingItemId(itemId);

    try {
      await tripService.deleteChecklistItem(tripId, itemId);
      setItems((currentItems) => currentItems.filter((item) => item.id !== itemId));
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Stavka nije obrisana.");
    } finally {
      setUpdatingItemId(null);
    }
  }

  return (
    <section className="management-section">
      <div className="section-header">
        <h2>Checklist</h2>
        {isLoading && <span>Ucitavanje...</span>}
      </div>

      {error && <p className="form-error">{error}</p>}

      <form className="inline-form" onSubmit={handleSubmit}>
        <label>
          Nova stavka
          <input value={text} onChange={(event) => setText(event.target.value)} />
        </label>
        <button className="primary-button" disabled={isSubmitting} type="submit">
          {isSubmitting ? "Cuvanje..." : "Dodaj stavku"}
        </button>
      </form>

      {items.length === 0 && !isLoading && <p className="state-message">Nema checklist stavki.</p>}
      <div className="checklist">
        {items.map((item) => (
          <article className="checklist-item" key={item.id}>
            <label className="checkbox-label">
              <input
                checked={item.isCompleted}
                disabled={updatingItemId === item.id}
                type="checkbox"
                onChange={() => toggleItem(item)}
              />
              <span className={item.isCompleted ? "completed-text" : undefined}>{item.text}</span>
            </label>
            <button
              className="danger-button"
              disabled={updatingItemId === item.id}
              type="button"
              onClick={() => deleteItem(item.id)}
            >
              Obrisi
            </button>
          </article>
        ))}
      </div>
    </section>
  );
}
