import { AuthResponse } from "../models";

const tokenKey = "travelPlanner.accessToken";
const userKey = "travelPlanner.user";

export const tokenStorage = {
  getAccessToken(): string | null {
    return localStorage.getItem(tokenKey);
  },

  setAccessToken(token: string): void {
    localStorage.setItem(tokenKey, token);
  },

  getAuth(): AuthResponse | null {
    const value = localStorage.getItem(userKey);
    return value ? (JSON.parse(value) as AuthResponse) : null;
  },

  setAuth(auth: AuthResponse): void {
    localStorage.setItem(tokenKey, auth.accessToken);
    localStorage.setItem(userKey, JSON.stringify(auth));
  },

  clearAccessToken(): void {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
  }
};
