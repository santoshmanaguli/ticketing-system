import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  addTicketComment,
  createTicket,
  getDashboard,
  getLookups,
  getTicket,
  getTickets,
  updateTicketStatus
} from "./api";
import type {
  CreateTicketDraft,
  DashboardSummary,
  Lookups,
  TicketDetail,
  TicketFilters,
  TicketListItem,
  TicketStatus
} from "./types";

const emptyLookups: Lookups = {
  modules: [],
  customers: [],
  users: [],
  statuses: [],
  priorities: []
};

const defaultFilters: TicketFilters = {
  search: "",
  status: "",
  priority: "",
  moduleId: "",
  assignedToUserId: ""
};

const defaultDraft: CreateTicketDraft = {
  title: "",
  description: "",
  customerId: "",
  branchId: "",
  erpModuleId: "",
  priority: "Medium",
  createdByUserId: "",
  assignedToUserId: ""
};

export default function App() {
  const [dashboard, setDashboard] = useState<DashboardSummary | null>(null);
  const [lookups, setLookups] = useState<Lookups>(emptyLookups);
  const [tickets, setTickets] = useState<TicketListItem[]>([]);
  const [selectedTicket, setSelectedTicket] = useState<TicketDetail | null>(null);
  const [filters, setFilters] = useState<TicketFilters>(defaultFilters);
  const [draft, setDraft] = useState<CreateTicketDraft>(defaultDraft);
  const [statusDraft, setStatusDraft] = useState<TicketStatus>("Open");
  const [statusNote, setStatusNote] = useState("");
  const [commentDraft, setCommentDraft] = useState("");
  const [isInternalComment, setIsInternalComment] = useState(true);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const selectedCustomer = useMemo(
    () => lookups.customers.find((customer) => String(customer.id) === draft.customerId),
    [draft.customerId, lookups.customers]
  );

  async function loadWorkspace(nextFilters = filters) {
    setError(null);

    const [dashboardResult, ticketsResult] = await Promise.all([
      getDashboard(),
      getTickets(nextFilters)
    ]);

    setDashboard(dashboardResult);
    setTickets(ticketsResult);
  }

  useEffect(() => {
    async function loadInitialData() {
      try {
        setLoading(true);
        const lookupResult = await getLookups();
        setLookups(lookupResult);

        setDraft((current) => ({
          ...current,
          customerId: current.customerId || String(lookupResult.customers[0]?.id ?? ""),
          branchId: current.branchId || String(lookupResult.customers[0]?.branches[0]?.id ?? ""),
          erpModuleId: current.erpModuleId || String(lookupResult.modules[0]?.id ?? ""),
          createdByUserId: current.createdByUserId || String(lookupResult.users[0]?.id ?? ""),
          assignedToUserId: current.assignedToUserId || String(lookupResult.users[0]?.id ?? "")
        }));

        await loadWorkspace(defaultFilters);
      } catch (caught) {
        setError(caught instanceof Error ? caught.message : "Unable to load workspace.");
      } finally {
        setLoading(false);
      }
    }

    void loadInitialData();
  }, []);

  async function applyFilters(nextFilters: TicketFilters) {
    setFilters(nextFilters);
    try {
      setError(null);
      setTickets(await getTickets(nextFilters));
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Unable to load tickets.");
    }
  }

  async function openTicket(ticketId: number) {
    try {
      setError(null);
      const detail = await getTicket(ticketId);
      setSelectedTicket(detail);
      setStatusDraft(detail.status);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Unable to open ticket.");
    }
  }

  async function submitTicket(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setError(null);
      const created = await createTicket({
        title: draft.title,
        description: draft.description,
        customerId: Number(draft.customerId),
        branchId: draft.branchId ? Number(draft.branchId) : null,
        erpModuleId: Number(draft.erpModuleId),
        priority: draft.priority,
        createdByUserId: Number(draft.createdByUserId),
        assignedToUserId: draft.assignedToUserId ? Number(draft.assignedToUserId) : null
      });

      setSelectedTicket(created);
      setStatusDraft(created.status);
      setDraft((current) => ({
        ...current,
        title: "",
        description: "",
        priority: "Medium"
      }));
      await loadWorkspace();
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Unable to create ticket.");
    }
  }

  async function submitStatus(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedTicket) {
      return;
    }

    try {
      setError(null);
      const changedByUserId = Number(draft.createdByUserId || lookups.users[0]?.id);
      const updated = await updateTicketStatus(selectedTicket.id, {
        status: statusDraft,
        changedByUserId,
        note: statusNote || null
      });

      setSelectedTicket(updated);
      setStatusNote("");
      await loadWorkspace();
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Unable to update status.");
    }
  }

  async function submitComment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedTicket || !commentDraft.trim()) {
      return;
    }

    try {
      setError(null);
      const authorUserId = Number(draft.createdByUserId || lookups.users[0]?.id);
      const updated = await addTicketComment(selectedTicket.id, {
        authorUserId,
        body: commentDraft,
        isInternal: isInternalComment
      });

      setSelectedTicket(updated);
      setCommentDraft("");
      await loadWorkspace();
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Unable to add comment.");
    }
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">Sparkle ERP Support</p>
          <h1>Ticketing Workspace</h1>
        </div>
        <div className="connection-pill">API: localhost:5001</div>
      </header>

      {error && <div className="alert">{error}</div>}

      <main className="workspace">
        <section className="dashboard-band" aria-label="Dashboard summary">
          <Metric label="Total tickets" value={dashboard?.totalTickets ?? 0} />
          <Metric label="Open work" value={dashboard?.openTickets ?? 0} tone="blue" />
          <Metric label="Critical" value={dashboard?.criticalTickets ?? 0} tone="red" />
          <Metric label="Unassigned" value={dashboard?.unassignedTickets ?? 0} tone="amber" />
          <Metric label="Resolved 7d" value={dashboard?.resolvedThisWeek ?? 0} tone="green" />
        </section>

        <section className="content-grid">
          <div className="main-column">
            <section className="toolbar" aria-label="Ticket filters">
              <input
                placeholder="Search ticket, customer, title"
                value={filters.search}
                onChange={(event) => applyFilters({ ...filters, search: event.target.value })}
              />
              <select
                value={filters.status}
                onChange={(event) => applyFilters({ ...filters, status: event.target.value })}
              >
                <option value="">All statuses</option>
                {lookups.statuses.map((status) => (
                  <option key={status.value} value={status.value}>
                    {status.label}
                  </option>
                ))}
              </select>
              <select
                value={filters.priority}
                onChange={(event) => applyFilters({ ...filters, priority: event.target.value })}
              >
                <option value="">All priorities</option>
                {lookups.priorities.map((priority) => (
                  <option key={priority.value} value={priority.value}>
                    {priority.label}
                  </option>
                ))}
              </select>
              <select
                value={filters.moduleId}
                onChange={(event) => applyFilters({ ...filters, moduleId: event.target.value })}
              >
                <option value="">All modules</option>
                {lookups.modules.map((module) => (
                  <option key={module.id} value={module.id}>
                    {module.name}
                  </option>
                ))}
              </select>
            </section>

            <section className="table-panel" aria-label="Tickets">
              <div className="panel-heading">
                <h2>Tickets</h2>
                <span>{loading ? "Loading..." : `${tickets.length} shown`}</span>
              </div>
              <div className="ticket-table">
                <div className="ticket-row ticket-row-head">
                  <span>Ticket</span>
                  <span>Customer</span>
                  <span>Module</span>
                  <span>Owner</span>
                  <span>Status</span>
                </div>
                {tickets.map((ticket) => (
                  <button
                    className="ticket-row"
                    key={ticket.id}
                    type="button"
                    onClick={() => openTicket(ticket.id)}
                  >
                    <span>
                      <strong>{ticket.ticketNumber}</strong>
                      <small>{ticket.title}</small>
                    </span>
                    <span>
                      {ticket.customerName}
                      <small>{ticket.branchName ?? "No branch"}</small>
                    </span>
                    <span>{ticket.moduleName}</span>
                    <span>{ticket.assignedTo ?? "Unassigned"}</span>
                    <span className={`badge ${ticket.status}`}>{labelize(ticket.status)}</span>
                  </button>
                ))}
              </div>
            </section>
          </div>

          <aside className="side-column" aria-label="Create ticket">
            <form className="form-panel" onSubmit={submitTicket}>
              <div className="panel-heading">
                <h2>New Ticket</h2>
              </div>

              <label>
                Title
                <input
                  required
                  value={draft.title}
                  onChange={(event) => setDraft({ ...draft, title: event.target.value })}
                  placeholder="Short issue summary"
                />
              </label>

              <label>
                Customer
                <select
                  required
                  value={draft.customerId}
                  onChange={(event) => {
                    const nextCustomer = lookups.customers.find(
                      (customer) => String(customer.id) === event.target.value
                    );

                    setDraft({
                      ...draft,
                      customerId: event.target.value,
                      branchId: String(nextCustomer?.branches[0]?.id ?? "")
                    });
                  }}
                >
                  {lookups.customers.map((customer) => (
                    <option key={customer.id} value={customer.id}>
                      {customer.companyName}
                    </option>
                  ))}
                </select>
              </label>

              <label>
                Branch
                <select
                  value={draft.branchId}
                  onChange={(event) => setDraft({ ...draft, branchId: event.target.value })}
                >
                  <option value="">No branch</option>
                  {selectedCustomer?.branches.map((branch) => (
                    <option key={branch.id} value={branch.id}>
                      {branch.name}
                    </option>
                  ))}
                </select>
              </label>

              <label>
                ERP Module
                <select
                  required
                  value={draft.erpModuleId}
                  onChange={(event) => setDraft({ ...draft, erpModuleId: event.target.value })}
                >
                  {lookups.modules.map((module) => (
                    <option key={module.id} value={module.id}>
                      {module.name}
                    </option>
                  ))}
                </select>
              </label>

              <div className="form-split">
                <label>
                  Priority
                  <select
                    value={draft.priority}
                    onChange={(event) =>
                      setDraft({ ...draft, priority: event.target.value as CreateTicketDraft["priority"] })
                    }
                  >
                    {lookups.priorities.map((priority) => (
                      <option key={priority.value} value={priority.value}>
                        {priority.label}
                      </option>
                    ))}
                  </select>
                </label>

                <label>
                  Assign
                  <select
                    value={draft.assignedToUserId}
                    onChange={(event) => setDraft({ ...draft, assignedToUserId: event.target.value })}
                  >
                    <option value="">Unassigned</option>
                    {lookups.users.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.fullName}
                      </option>
                    ))}
                  </select>
                </label>
              </div>

              <label>
                Description
                <textarea
                  required
                  rows={5}
                  value={draft.description}
                  onChange={(event) => setDraft({ ...draft, description: event.target.value })}
                  placeholder="What happened, which screen, expected behavior, customer impact"
                />
              </label>

              <button className="primary-button" type="submit">
                Create ticket
              </button>
            </form>
          </aside>
        </section>

        <section className="detail-band" aria-label="Ticket details">
          {selectedTicket ? (
            <>
              <div className="detail-header">
                <div>
                  <p className="eyebrow">{selectedTicket.ticketNumber}</p>
                  <h2>{selectedTicket.title}</h2>
                  <p>{selectedTicket.description}</p>
                </div>
                <div className="detail-meta">
                  <span className={`badge ${selectedTicket.priority}`}>{selectedTicket.priority}</span>
                  <span className={`badge ${selectedTicket.status}`}>{labelize(selectedTicket.status)}</span>
                </div>
              </div>

              <div className="detail-grid">
                <form className="compact-panel" onSubmit={submitStatus}>
                  <h3>Status</h3>
                  <select value={statusDraft} onChange={(event) => setStatusDraft(event.target.value as TicketStatus)}>
                    {lookups.statuses.map((status) => (
                      <option key={status.value} value={status.value}>
                        {status.label}
                      </option>
                    ))}
                  </select>
                  <input
                    value={statusNote}
                    onChange={(event) => setStatusNote(event.target.value)}
                    placeholder="Optional status note"
                  />
                  <button type="submit">Update status</button>
                </form>

                <form className="compact-panel" onSubmit={submitComment}>
                  <h3>Add Comment</h3>
                  <textarea
                    rows={3}
                    value={commentDraft}
                    onChange={(event) => setCommentDraft(event.target.value)}
                    placeholder="Add support note or customer reply"
                  />
                  <label className="checkbox-line">
                    <input
                      type="checkbox"
                      checked={isInternalComment}
                      onChange={(event) => setIsInternalComment(event.target.checked)}
                    />
                    Internal note
                  </label>
                  <button type="submit">Add comment</button>
                </form>

                <div className="compact-panel">
                  <h3>Activity</h3>
                  <div className="activity-list">
                    {selectedTicket.statusHistory.map((history) => (
                      <p key={history.id}>
                        <strong>{labelize(history.toStatus)}</strong>
                        <span>
                          {history.changedBy ?? "System"} · {formatDate(history.createdAt)}
                        </span>
                        {history.note && <small>{history.note}</small>}
                      </p>
                    ))}
                  </div>
                </div>

                <div className="compact-panel">
                  <h3>Comments</h3>
                  <div className="activity-list">
                    {selectedTicket.comments.map((comment) => (
                      <p key={comment.id}>
                        <strong>{comment.author}</strong>
                        <span>
                          {comment.isInternal ? "Internal" : "Customer visible"} · {formatDate(comment.createdAt)}
                        </span>
                        <small>{comment.body}</small>
                      </p>
                    ))}
                  </div>
                </div>
              </div>
            </>
          ) : (
            <div className="empty-detail">
              <h2>Select a ticket</h2>
              <p>Open any ticket from the list to update status, add comments, and review activity.</p>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

function Metric({ label, value, tone }: { label: string; value: number; tone?: string }) {
  return (
    <div className={`metric ${tone ?? ""}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function labelize(value: string) {
  return value.replace(/([A-Z])/g, " $1").trim();
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-IN", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}
