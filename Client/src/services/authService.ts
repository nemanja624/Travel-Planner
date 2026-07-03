import { AuthResponse, LoginRequest, RegisterUserRequest } from "../models";
import { apiClient } from "./apiClient";
import { tokenStorage } from "./tokenStorage";

export const authService = {
  async register(request: RegisterUserRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse, RegisterUserRequest>("/api/auth/register", request);
    tokenStorage.setAuth(response);
    return response;
  },

  async login(request: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse, LoginRequest>("/api/auth/login", request);
    tokenStorage.setAuth(response);
    return response;
  },

  logout(): void {
    tokenStorage.clearAccessToken();
  }
};
