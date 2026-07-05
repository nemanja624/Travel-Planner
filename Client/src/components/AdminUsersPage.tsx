import { useEffect, useState } from "react";
import { AdminTripSummary, User, UserRole } from "../models";
import { ApiError, useServices } from "../services";

interface AdminUsersPageProps {
  onBack: () => void;
}

const roleLabels: Record<UserRole, string> = {
  [UserRole.User]: "Korisnik",
  [UserRole.Admin]: "Admin"
};

type AdminTab = "users" | "content";

export function AdminUsersPage({ onBack }: AdminUsersPageProps) {
  const { adminUserService } = useServices();
  const [activeTab, setActiveTab] = useState<AdminTab>("users");
  const [users, setUsers] = useState<User[]>([]);
  const [trips, setTrips] = useState<AdminTripSummary[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [updatingUserId, setUpdatingUserId] = useState<string | null>(null);
  const [deletingTripId, setDeletingTripId] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function loadAdministrationData() {
      try {
        const [loadedUsers, loadedTrips] = await Promise.all([
          adminUserService.getUsers(),
          adminUserService.getTrips()
        ]);
        if (isMounted) {
          setUsers(loadedUsers);
          setTrips(loadedTrips);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Administracija nije ucitana.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadAdministrationData();

    return () => {
      isMounted = false;
    };
  }, [adminUserService]);

  async function updateRole(user: User, role: UserRole) {
    setError(null);
    setUpdatingUserId(user.id);

    try {
      const updatedUser = await adminUserService.updateUserRole(user.id, { role });
      replaceUser(updatedUser);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Uloga nije promijenjena.");
    } finally {
      setUpdatingUserId(null);
    }
  }

  async function updateStatus(user: User) {
    setError(null);
    setUpdatingUserId(user.id);

    try {
      const updatedUser = await adminUserService.updateUserStatus(user.id, { isActive: !user.isActive });
      replaceUser(updatedUser);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Status korisnika nije promijenjen.");
    } finally {
      setUpdatingUserId(null);
    }
  }

  function replaceUser(updatedUser: User) {
    setUsers((currentUsers) => currentUsers.map((user) => (user.id === updatedUser.id ? updatedUser : user)));
  }

  async function deleteTrip(tripId: string) {
    if (!window.confirm("Obrisati plan i sve povezane podatke?")) {
      return;
    }

    setError(null);
    setDeletingTripId(tripId);

    try {
      await adminUserService.deleteTrip(tripId);
      setTrips((currentTrips) => currentTrips.filter((trip) => trip.id !== tripId));
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Plan nije obrisan.");
    } finally {
      setDeletingTripId(null);
    }
  }

  return (
    <section className="page-panel">
      <header className="page-header">
        <div>
          <p className="eyebrow">Administracija</p>
          <h1>Sistem</h1>
          <p className="summary">Pregled korisnickih naloga i sadrzaja sistema.</p>
        </div>
        <button className="secondary-button compact" type="button" onClick={onBack}>
          Nazad
        </button>
      </header>

      <div className="admin-tabs" role="tablist" aria-label="Administracija">
        <button
          className={activeTab === "users" ? "active" : ""}
          type="button"
          onClick={() => setActiveTab("users")}
        >
          Korisnici
        </button>
        <button
          className={activeTab === "content" ? "active" : ""}
          type="button"
          onClick={() => setActiveTab("content")}
        >
          Sadrzaj
        </button>
      </div>

      {isLoading && <p className="state-message">Ucitavanje administracije...</p>}
      {error && <p className="form-error">{error}</p>}

      {!isLoading && activeTab === "users" && (
        <>
          {users.length === 0 && <p className="state-message">Nema korisnika.</p>}
          <div className="item-list">
            {users.map((user) => (
              <article className="list-item user-row" key={user.id}>
                <div>
                  <h3>{user.name}</h3>
                  <p>{user.email}</p>
                  <p>Kreiran: {formatDate(user.createdAtUtc)}</p>
                </div>
                <div className="admin-controls">
                  <label>
                    Uloga
                    <select
                      disabled={updatingUserId === user.id}
                      value={user.role}
                      onChange={(event) => updateRole(user, Number(event.target.value) as UserRole)}
                    >
                      {Object.values(UserRole)
                        .filter((role): role is UserRole => typeof role === "number")
                        .map((role) => (
                          <option key={role} value={role}>
                            {roleLabels[role]}
                          </option>
                        ))}
                    </select>
                  </label>
                  <span className="status-pill">{user.isActive ? "Aktivan" : "Neaktivan"}</span>
                  <button
                    className={user.isActive ? "danger-button" : "primary-button"}
                    disabled={updatingUserId === user.id}
                    type="button"
                    onClick={() => updateStatus(user)}
                  >
                    {user.isActive ? "Deaktiviraj" : "Aktiviraj"}
                  </button>
                </div>
              </article>
            ))}
          </div>
        </>
      )}

      {!isLoading && activeTab === "content" && (
        <>
          {trips.length === 0 && <p className="state-message">Nema planova putovanja.</p>}
          <div className="item-list">
            {trips.map((trip) => (
              <article className="list-item admin-trip-row" key={trip.id}>
                <div>
                  <h3>{trip.title}</h3>
                  <p>
                    {formatDate(trip.startDate)} - {formatDate(trip.endDate)}
                  </p>
                  <p>Vlasnik: {getOwnerLabel(trip.ownerId, users)}</p>
                  <p>Kreiran: {formatDate(trip.createdAtUtc)}</p>
                </div>
                <dl className="admin-trip-metrics">
                  <div>
                    <dt>Budzet</dt>
                    <dd>{formatCurrency(trip.plannedBudget)}</dd>
                  </div>
                  <div>
                    <dt>Troskovi</dt>
                    <dd>{formatCurrency(trip.totalExpenses)}</dd>
                  </div>
                  <div>
                    <dt>Preostalo</dt>
                    <dd>{formatCurrency(trip.remainingBudget)}</dd>
                  </div>
                </dl>
                <div className="admin-controls">
                  <button
                    className="danger-button"
                    disabled={deletingTripId === trip.id}
                    type="button"
                    onClick={() => deleteTrip(trip.id)}
                  >
                    {deletingTripId === trip.id ? "Brisanje..." : "Obrisi plan"}
                  </button>
                </div>
              </article>
            ))}
          </div>
        </>
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

function getOwnerLabel(ownerId: string, users: User[]) {
  const owner = users.find((user) => user.id === ownerId);
  return owner ? `${owner.name} (${owner.email})` : ownerId;
}
