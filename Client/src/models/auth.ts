import { UserRole } from "./common";

export interface RegisterUserRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  name: string;
  email: string;
  role: UserRole;
  accessToken: string;
  expiresAtUtc: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAtUtc: string;
}

export interface UpdateUserRoleRequest {
  role: UserRole;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}
