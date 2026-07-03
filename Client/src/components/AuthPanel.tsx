import { FormEvent, useState } from "react";
import { ApiError } from "../services";
import { useAuth } from "../state";

type AuthMode = "login" | "register";

export function AuthPanel() {
  const { login, register } = useAuth();
  const [mode, setMode] = useState<AuthMode>("login");
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const validationError = validateForm();
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    try {
      if (mode === "login") {
        await login({ email, password });
      } else {
        await register({ name, email, password });
      }
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Operacija nije uspjela.");
    } finally {
      setIsSubmitting(false);
    }
  }

  function validateForm() {
    if (mode === "register" && name.trim().length < 2) {
      return "Ime mora imati najmanje 2 karaktera.";
    }

    if (!email.includes("@")) {
      return "Unesi ispravnu email adresu.";
    }

    if (password.length < 8) {
      return "Lozinka mora imati najmanje 8 karaktera.";
    }

    return null;
  }

  return (
    <form className="auth-panel" onSubmit={handleSubmit}>
      <div className="segmented-control" aria-label="Auth mode">
        <button
          className={mode === "login" ? "active" : ""}
          type="button"
          onClick={() => setMode("login")}
        >
          Login
        </button>
        <button
          className={mode === "register" ? "active" : ""}
          type="button"
          onClick={() => setMode("register")}
        >
          Registracija
        </button>
      </div>

      {mode === "register" && (
        <label>
          Ime
          <input value={name} onChange={(event) => setName(event.target.value)} />
        </label>
      )}

      <label>
        Email
        <input
          autoComplete="email"
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
        />
      </label>

      <label>
        Lozinka
        <input
          autoComplete={mode === "login" ? "current-password" : "new-password"}
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />
      </label>

      {error && <p className="form-error">{error}</p>}

      <button className="primary-button" disabled={isSubmitting} type="submit">
        {isSubmitting ? "Slanje..." : mode === "login" ? "Uloguj se" : "Registruj se"}
      </button>
    </form>
  );
}
