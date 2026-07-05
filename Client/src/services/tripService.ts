import {
  ActivityFormData,
  BudgetSummary,
  ChecklistItem,
  ChecklistItemFormData,
  Destination,
  DestinationFormData,
  Expense,
  ExpenseFormData,
  Trip,
  TripActivity,
  TripFormData,
  TripSummary
} from "../models";
import { apiClient } from "./apiClient";

export const tripService = {
  getTrips(): Promise<TripSummary[]> {
    return apiClient.get<TripSummary[]>("/api/trips");
  },

  getTrip(tripId: string): Promise<Trip> {
    return apiClient.get<Trip>(`/api/trips/${tripId}`);
  },

  createTrip(request: TripFormData): Promise<Trip> {
    return apiClient.post<Trip, TripFormData>("/api/trips", request);
  },

  updateTrip(tripId: string, request: TripFormData): Promise<Trip> {
    return apiClient.put<Trip, TripFormData>(`/api/trips/${tripId}`, request);
  },

  deleteTrip(tripId: string): Promise<void> {
    return apiClient.delete(`/api/trips/${tripId}`);
  },

  getDestinations(tripId: string): Promise<Destination[]> {
    return apiClient.get<Destination[]>(`/api/trips/${tripId}/destinations`);
  },

  createDestination(tripId: string, request: DestinationFormData): Promise<Destination> {
    return apiClient.post<Destination, DestinationFormData>(`/api/trips/${tripId}/destinations`, request);
  },

  updateDestination(tripId: string, destinationId: string, request: DestinationFormData): Promise<Destination> {
    return apiClient.put<Destination, DestinationFormData>(`/api/trips/${tripId}/destinations/${destinationId}`, request);
  },

  deleteDestination(tripId: string, destinationId: string): Promise<void> {
    return apiClient.delete(`/api/trips/${tripId}/destinations/${destinationId}`);
  },

  getActivities(tripId: string): Promise<TripActivity[]> {
    return apiClient.get<TripActivity[]>(`/api/trips/${tripId}/activities`);
  },

  createActivity(tripId: string, request: ActivityFormData): Promise<TripActivity> {
    return apiClient.post<TripActivity, ActivityFormData>(
      `/api/trips/${tripId}/activities`,
      normalizeActivityRequest(request)
    );
  },

  updateActivity(tripId: string, activityId: string, request: ActivityFormData): Promise<TripActivity> {
    return apiClient.put<TripActivity, ActivityFormData>(
      `/api/trips/${tripId}/activities/${activityId}`,
      normalizeActivityRequest(request)
    );
  },

  deleteActivity(tripId: string, activityId: string): Promise<void> {
    return apiClient.delete(`/api/trips/${tripId}/activities/${activityId}`);
  },

  getExpenses(tripId: string): Promise<Expense[]> {
    return apiClient.get<Expense[]>(`/api/trips/${tripId}/expenses`);
  },

  createExpense(tripId: string, request: ExpenseFormData): Promise<Expense> {
    return apiClient.post<Expense, ExpenseFormData>(`/api/trips/${tripId}/expenses`, request);
  },

  updateExpense(tripId: string, expenseId: string, request: ExpenseFormData): Promise<Expense> {
    return apiClient.put<Expense, ExpenseFormData>(`/api/trips/${tripId}/expenses/${expenseId}`, request);
  },

  deleteExpense(tripId: string, expenseId: string): Promise<void> {
    return apiClient.delete(`/api/trips/${tripId}/expenses/${expenseId}`);
  },

  getBudget(tripId: string): Promise<BudgetSummary> {
    return apiClient.get<BudgetSummary>(`/api/trips/${tripId}/budget`);
  },

  getChecklistItems(tripId: string): Promise<ChecklistItem[]> {
    return apiClient.get<ChecklistItem[]>(`/api/trips/${tripId}/checklist-items`);
  },

  createChecklistItem(tripId: string, request: ChecklistItemFormData): Promise<ChecklistItem> {
    return apiClient.post<ChecklistItem, Pick<ChecklistItemFormData, "text">>(
      `/api/trips/${tripId}/checklist-items`,
      { text: request.text }
    );
  },

  updateChecklistItem(tripId: string, itemId: string, request: Required<ChecklistItemFormData>): Promise<ChecklistItem> {
    return apiClient.put<ChecklistItem, Required<ChecklistItemFormData>>(
      `/api/trips/${tripId}/checklist-items/${itemId}`,
      request
    );
  },

  deleteChecklistItem(tripId: string, itemId: string): Promise<void> {
    return apiClient.delete(`/api/trips/${tripId}/checklist-items/${itemId}`);
  }
};

function normalizeActivityRequest(request: ActivityFormData): ActivityFormData {
  return {
    ...request,
    time: request.time.length === 5 ? `${request.time}:00` : request.time
  };
}
