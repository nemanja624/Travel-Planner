import { ShareAccessLevel } from "./common";
import {
  ChecklistItem,
  Destination,
  Expense,
  Trip,
  TripActivity
} from "./trips";

export interface CreateShareLinkRequest {
  tripId: string;
  accessLevel: ShareAccessLevel;
  expiresAtUtc: string;
}

export interface ShareLink {
  id: string;
  tripId: string;
  accessLevel: ShareAccessLevel;
  token: string;
  shareUrl: string;
  qrCodeUrl: string;
  expiresAtUtc: string;
  createdAtUtc: string;
}

export interface SharedTrip {
  trip: Trip;
  destinations: Destination[];
  activities: TripActivity[];
  expenses: Expense[];
  checklistItems: ChecklistItem[];
  accessLevel: ShareAccessLevel;
}

export interface ValidateShareTokenRequest {
  token: string;
}
