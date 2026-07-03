import { ActivityStatus, ExpenseCategory } from "./common";

export interface Trip {
  id: string;
  ownerId: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  plannedBudget: number;
  totalExpenses: number;
  remainingBudget: number;
  notes: string;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface TripSummary {
  id: string;
  title: string;
  startDate: string;
  endDate: string;
  plannedBudget: number;
  totalExpenses: number;
  remainingBudget: number;
}

export interface TripFormData {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  plannedBudget: number;
  notes: string;
}

export interface Destination {
  id: string;
  tripId: string;
  name: string;
  location: string;
  arrivalDate: string;
  departureDate: string;
  description: string;
}

export interface DestinationFormData {
  name: string;
  location: string;
  arrivalDate: string;
  departureDate: string;
  description: string;
}

export interface TripActivity {
  id: string;
  tripId: string;
  title: string;
  date: string;
  time: string;
  location: string;
  description: string;
  estimatedCost: number;
  status: ActivityStatus;
}

export interface ActivityFormData {
  title: string;
  date: string;
  time: string;
  location: string;
  description: string;
  estimatedCost: number;
  status: ActivityStatus;
}

export interface Expense {
  id: string;
  tripId: string;
  name: string;
  category: ExpenseCategory;
  amount: number;
  date: string;
  description: string;
}

export interface ExpenseFormData {
  name: string;
  category: ExpenseCategory;
  amount: number;
  date: string;
  description: string;
}

export interface BudgetSummary {
  tripId: string;
  plannedBudget: number;
  totalExpenses: number;
  remainingBudget: number;
}

export interface ChecklistItem {
  id: string;
  tripId: string;
  text: string;
  isCompleted: boolean;
}

export interface ChecklistItemFormData {
  text: string;
  isCompleted?: boolean;
}
