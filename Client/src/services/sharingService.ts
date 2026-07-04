import { CreateShareLinkRequest, ShareLink, SharedTrip, Trip, TripFormData } from "../models";
import { apiClient } from "./apiClient";

export const sharingService = {
  createShareLink(request: CreateShareLinkRequest): Promise<ShareLink> {
    return apiClient.post<ShareLink, CreateShareLinkRequest>("/api/share-links", request);
  },

  getSharedTrip(token: string): Promise<SharedTrip> {
    return apiClient.get<SharedTrip>(`/api/shared-trips/${encodeURIComponent(token)}`);
  },

  updateSharedTrip(token: string, request: TripFormData): Promise<Trip> {
    return apiClient.put<Trip, TripFormData>(`/api/shared-trips/${encodeURIComponent(token)}/trip`, request);
  }
};
