import type { TicketPriority, TicketStatus } from "./types";

export interface CreateTicketRequest {
  title: string;
  description: string;
  customerId: number;
  branchId: number | null;
  erpModuleId: number;
  priority: TicketPriority;
  createdByUserId: number;
  assignedToUserId: number | null;
}

export interface UpdateTicketStatusRequest {
  status: TicketStatus;
  changedByUserId: number;
  note: string | null;
}

export interface AddTicketCommentRequest {
  authorUserId: number;
  body: string;
  isInternal: boolean;
}

export type {
  DashboardSummary,
  Lookups,
  TicketDetail,
  TicketFilters,
  TicketListItem
} from "./types";
