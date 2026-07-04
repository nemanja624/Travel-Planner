import {
  createContext,
  PropsWithChildren,
  useContext,
  useMemo,
  useState
} from "react";
import {
  AuthResponse,
  LoginRequest,
  RegisterUserRequest
} from "../models";
import { useServices } from "../services";

interface AuthContextValue {
  auth: AuthResponse | null;
  isAuthenticated: boolean;
  login(request: LoginRequest): Promise<void>;
  register(request: RegisterUserRequest): Promise<void>;
  logout(): void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const { authService, tokenStorage } = useServices();
  const [auth, setAuth] = useState<AuthResponse | null>(() => tokenStorage.getAuth());

  const value = useMemo<AuthContextValue>(
    () => ({
      auth,
      isAuthenticated: auth !== null,
      async login(request) {
        setAuth(await authService.login(request));
      },
      async register(request) {
        setAuth(await authService.register(request));
      },
      logout() {
        authService.logout();
        setAuth(null);
      }
    }),
    [auth, authService, tokenStorage]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
