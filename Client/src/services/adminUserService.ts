import {
  AdminTripSummary,
  UpdateUserRoleRequest,
  UpdateUserStatusRequest,
  User
} from "../models";
import { apiClient } from "./apiClient";

export const adminUserService = {
  getUsers(): Promise<User[]> {
    return apiClient.get<User[]>("/api/admin/users");
  },

  updateUserRole(userId: string, request: UpdateUserRoleRequest): Promise<User> {
    return apiClient.put<User, UpdateUserRoleRequest>(`/api/admin/users/${userId}/role`, request);
  },

  updateUserStatus(userId: string, request: UpdateUserStatusRequest): Promise<User> {
    return apiClient.put<User, UpdateUserStatusRequest>(`/api/admin/users/${userId}/status`, request);
  },

  getTrips(): Promise<AdminTripSummary[]> {
    return apiClient.get<AdminTripSummary[]>("/api/admin/trips");
  },

  deleteTrip(tripId: string): Promise<void> {
    return apiClient.delete(`/api/admin/trips/${tripId}`);
  }
};
