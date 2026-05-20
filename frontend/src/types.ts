export type TicketStatus =
  | "Open"
  | "InProgress"
  | "WaitingForCustomer"
  | "Resolved"
  | "Closed";

export type TicketPriority = "Low" | "Medium" | "High" | "Critical";

export interface DashboardBucket {
  name: string;
  count: number;
}

export interface DashboardSummary {
  totalTickets: number;
  openTickets: number;
  criticalTickets: number;
  unassignedTickets: number;
  resolvedThisWeek: number;
  ticketsByStatus: DashboardBucket[];
  ticketsByPriority: DashboardBucket[];
  recentTickets: TicketListItem[];
}

export interface TicketListItem {
  id: number;
  ticketNumber: string;
  title: string;
  status: TicketStatus;
  priority: TicketPriority;
  customerName: string;
  branchName: string | null;
  moduleName: string;
  assignedTo: string | null;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

export interface TicketDetail extends TicketListItem {
  description: string;
  customerId: number;
  branchId: number | null;
  erpModuleId: number;
  createdByUserId: number;
  assignedToUserId: number | null;
  resolvedAt: string | null;
  closedAt: string | null;
  comments: TicketComment[];
  statusHistory: TicketStatusHistory[];
}

export interface TicketComment {
  id: number;
  author: string;
  body: string;
  isInternal: boolean;
  createdAt: string;
}

export interface TicketStatusHistory {
  id: number;
  fromStatus: TicketStatus | null;
  toStatus: TicketStatus;
  changedBy: string | null;
  note: string | null;
  createdAt: string;
}

export interface Lookup {
  id: number;
  name: string;
}

export interface EnumLookup {
  value: string;
  label: string;
}

export interface UserLookup {
  id: number;
  fullName: string;
  email: string;
  role: string;
}

export interface BranchLookup {
  id: number;
  name: string;
  city: string;
}

export interface CustomerLookup {
  id: number;
  companyName: string;
  contactPerson: string;
  city: string;
  branches: BranchLookup[];
}

export interface Lookups {
  modules: Lookup[];
  customers: CustomerLookup[];
  users: UserLookup[];
  statuses: EnumLookup[];
  priorities: EnumLookup[];
}

export interface TicketFilters {
  search: string;
  status: string;
  priority: string;
  moduleId: string;
  assignedToUserId: string;
}

export interface CreateTicketDraft {
  title: string;
  description: string;
  customerId: string;
  branchId: string;
  erpModuleId: string;
  priority: TicketPriority;
  createdByUserId: string;
  assignedToUserId: string;
}
