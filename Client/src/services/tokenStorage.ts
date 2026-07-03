const tokenKey = "travelPlanner.accessToken";

export const tokenStorage = {
  getAccessToken(): string | null {
    return localStorage.getItem(tokenKey);
  },

  setAccessToken(token: string): void {
    localStorage.setItem(tokenKey, token);
  },

  clearAccessToken(): void {
    localStorage.removeItem(tokenKey);
  }
};
