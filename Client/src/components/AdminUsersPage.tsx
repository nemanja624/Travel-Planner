import { useEffect, useState } from "react";
import { User, UserRole } from "../models";
import { ApiError, useServices } from "../services";

interface AdminUsersPageProps {
  onBack: () => void;
}

const roleLabels: Record<UserRole, string> = {
  [UserRole.User]: "Korisnik",
  [UserRole.Admin]: "Admin"
};

export function AdminUsersPage({ onBack }: AdminUsersPageProps) {
  const { adminUserService } = useServices();
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [updatingUserId, setUpdatingUserId] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function loadUsers() {
      try {
        const loadedUsers = await adminUserService.getUsers();
        if (isMounted) {
          setUsers(loadedUsers);
        }
      } catch (caughtError) {
        if (isMounted) {
          setError(caughtError instanceof ApiError ? caughtError.message : "Korisnici nisu ucitani.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadUsers();

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

  return (
    <section className="page-panel">
      <header className="page-header">
        <div>
          <p className="eyebrow">Administracija</p>
          <h1>Korisnici</h1>
          <p className="summary">Pregled naloga, uloga i statusa korisnika.</p>
        </div>
        <button className="secondary-button compact" type="button" onClick={onBack}>
          Nazad
        </button>
      </header>

      {isLoading && <p className="state-message">Ucitavanje korisnika...</p>}
      {error && <p className="form-error">{error}</p>}
      {!isLoading && users.length === 0 && <p className="state-message">Nema korisnika.</p>}

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
    </section>
  );
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("sr-Latn-BA").format(new Date(value));
}
