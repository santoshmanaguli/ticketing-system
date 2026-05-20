import type {
  AddTicketCommentRequest,
  CreateTicketRequest,
  DashboardSummary,
  Lookups,
  TicketDetail,
  TicketFilters,
  TicketListItem,
  UpdateTicketStatusRequest
} from "./apiTypes";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5001/api";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    },
    ...init
  });

  if (!response.ok) {
    let message = `Request failed with ${response.status}`;

    try {
      const error = await response.json();
      message = error.message ?? message;
    } catch {
      // Keep the generic HTTP message if the response is not JSON.
    }

    throw new Error(message);
  }

  return response.json() as Promise<T>;
}

function toQueryString(filters: TicketFilters): string {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });

  const query = params.toString();
  return query ? `?${query}` : "";
}

export async function getLookups(): Promise<Lookups> {
  const [modules, customers, users, statuses, priorities] = await Promise.all([
    request<Lookups["modules"]>("/lookups/erp-modules"),
    request<Lookups["customers"]>("/lookups/customers"),
    request<Lookups["users"]>("/lookups/users"),
    request<Lookups["statuses"]>("/lookups/ticket-statuses"),
    request<Lookups["priorities"]>("/lookups/ticket-priorities")
  ]);

  return { modules, customers, users, statuses, priorities };
}

export function getDashboard(): Promise<DashboardSummary> {
  return request<DashboardSummary>("/dashboard/summary");
}

export function getTickets(filters: TicketFilters): Promise<TicketListItem[]> {
  return request<TicketListItem[]>(`/tickets${toQueryString(filters)}`);
}

export function getTicket(id: number): Promise<TicketDetail> {
  return request<TicketDetail>(`/tickets/${id}`);
}

export function createTicket(payload: CreateTicketRequest): Promise<TicketDetail> {
  return request<TicketDetail>("/tickets", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}

export function updateTicketStatus(id: number, payload: UpdateTicketStatusRequest): Promise<TicketDetail> {
  return request<TicketDetail>(`/tickets/${id}/status`, {
    method: "PATCH",
    body: JSON.stringify(payload)
  });
}

export function addTicketComment(id: number, payload: AddTicketCommentRequest): Promise<TicketDetail> {
  return request<TicketDetail>(`/tickets/${id}/comments`, {
    method: "POST",
    body: JSON.stringify(payload)
  });
}
