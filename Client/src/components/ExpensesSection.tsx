import { FormEvent, useEffect, useState } from "react";
import { ExpenseCategory } from "../models";
import { BudgetSummary, Expense, ExpenseFormData } from "../models/trips";
import { ApiError, tripService } from "../services";

interface ExpensesSectionProps {
  tripId: string;
}

const initialForm: ExpenseFormData = {
  name: "",
  category: ExpenseCategory.Other,
  amount: 0,
  date: "",
  description: ""
};

const categoryLabels: Record<ExpenseCategory, string> = {
  [ExpenseCategory.Transport]: "Prevoz",
  [ExpenseCategory.Accommodation]: "Smjestaj",
  [ExpenseCategory.Food]: "Hrana",
  [ExpenseCategory.Tickets]: "Ulaznice",
  [ExpenseCategory.Shopping]: "Kupovina",
  [ExpenseCategory.Other]: "Ostalo"
};

export function ExpensesSection({ tripId }: ExpensesSectionProps) {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [budget, setBudget] = useState<BudgetSummary | null>(null);
  const [form, setForm] = useState<ExpenseFormData>(initialForm);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let isMounted = true;

    async function loadExpenses() {
      try {
        const [loadedExpenses, loadedBudget] = await Promise.all([
          tripService.getExpenses(tripId),
          tripService.getBudget(tripId)
        ]);

        if (isMounted) {
          setExpenses(loadedExpenses);
          setBudget(loadedBudget);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Troskovi nisu ucitani.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadExpenses();

    return () => {
      isMounted = false;
    };
  }, [tripId]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const validationError = validateExpense(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      const expense = await tripService.createExpense(tripId, form);
      const updatedBudget = await tripService.getBudget(tripId);
      setExpenses((currentExpenses) => [...currentExpenses, expense]);
      setBudget(updatedBudget);
      setForm(initialForm);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Trosak nije sacuvan.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="management-section">
      <div className="section-header">
        <h2>Troskovi i budzet</h2>
        {isLoading && <span>Ucitavanje...</span>}
      </div>

      {error && <p className="form-error">{error}</p>}

      {budget && (
        <div className="budget-summary">
          <div>
            <span>Planirano</span>
            <strong>{formatCurrency(budget.plannedBudget)}</strong>
          </div>
          <div>
            <span>Potroseno</span>
            <strong>{formatCurrency(budget.totalExpenses)}</strong>
          </div>
          <div>
            <span>Preostalo</span>
            <strong>{formatCurrency(budget.remainingBudget)}</strong>
          </div>
        </div>
      )}

      <form className="trip-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <label>
            Naziv
            <input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          </label>
          <label>
            Kategorija
            <select
              value={form.category}
              onChange={(event) => setForm({ ...form, category: Number(event.target.value) as ExpenseCategory })}
            >
              {Object.values(ExpenseCategory)
                .filter((category): category is ExpenseCategory => typeof category === "number")
                .map((category) => (
                  <option key={category} value={category}>
                    {categoryLabels[category]}
                  </option>
                ))}
            </select>
          </label>
          <label>
            Iznos
            <input
              min="0"
              step="0.01"
              type="number"
              value={form.amount}
              onChange={(event) => setForm({ ...form, amount: Number(event.target.value) })}
            />
          </label>
          <label>
            Datum
            <input
              type="date"
              value={form.date}
              onChange={(event) => setForm({ ...form, date: event.target.value })}
            />
          </label>
        </div>
        <label>
          Opis
          <textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
        </label>
        <div className="form-actions">
          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? "Cuvanje..." : "Dodaj trosak"}
          </button>
        </div>
      </form>

      {expenses.length === 0 && !isLoading && <p className="state-message">Nema troskova.</p>}
      <div className="item-list">
        {expenses.map((expense) => (
          <article className="list-item" key={expense.id}>
            <div className="activity-heading">
              <div>
                <h3>{expense.name}</h3>
                <p>
                  {categoryLabels[expense.category]} - {formatDate(expense.date)}
                </p>
              </div>
              <strong>{formatCurrency(expense.amount)}</strong>
            </div>
            <p>{expense.description || "Nema opisa."}</p>
          </article>
        ))}
      </div>
    </section>
  );
}

function validateExpense(form: ExpenseFormData) {
  if (!form.name.trim()) {
    return "Naziv troska je obavezan.";
  }

  if (!form.date) {
    return "Datum troska je obavezan.";
  }

  if (form.amount < 0) {
    return "Iznos troska ne moze biti negativan.";
  }

  return null;
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
