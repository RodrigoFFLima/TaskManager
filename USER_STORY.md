# User Stories — Task Manager

## Epic 1: Authentication

### US-01 — Register
**As a** new user,
**I want to** create an account with my name, email and password,
**So that** I can access the task management system.

**Acceptance Criteria:**
- Email must be unique per user
- Password must be at least 8 characters with uppercase, lowercase and number
- On success, I receive a JWT token and am redirected to the task list
- Duplicate email returns a 422 with a clear error message

---

### US-02 — Login
**As a** registered user,
**I want to** sign in with my email and password,
**So that** I can access my tasks.

**Acceptance Criteria:**
- Valid credentials return a JWT token (24h expiry)
- Invalid credentials return 401 with a generic message (no user enumeration)
- Token is stored in localStorage and attached to all subsequent requests

---

## Epic 2: Task Management

### US-03 — View Task List
**As an** authenticated user,
**I want to** see all my tasks in a responsive grid,
**So that** I can get an overview of my work.

**Acceptance Criteria:**
- Tasks are displayed sorted by creation date (newest first)
- Each card shows title, status, priority, due date (if set), and action buttons
- Overdue tasks are visually highlighted in red
- Empty state is shown when no tasks exist

---

### US-04 — Filter Tasks
**As an** authenticated user,
**I want to** filter tasks by status and priority,
**So that** I can focus on what matters.

**Acceptance Criteria:**
- Status filter: All / Pending / InProgress / Completed / Cancelled
- Priority filter: All / Low / Medium / High / Critical
- Filters combine (AND logic)
- Task count updates dynamically

---

### US-05 — Create Task
**As an** authenticated user,
**I want to** create a new task with a title, description, priority and optional due date,
**So that** I can track my work.

**Acceptance Criteria:**
- Title is required (max 200 chars)
- Priority is required (Low / Medium / High / Critical)
- Due date must not be in the past
- Created task appears immediately in the list with "Pending" status

---

### US-06 — Edit Task
**As an** authenticated user,
**I want to** edit the title, description, priority and due date of a task,
**So that** I can keep task details accurate.

**Acceptance Criteria:**
- Cannot edit another user's task (returns 404)
- Status is not editable via this form (uses dedicated status transitions)
- Changes are reflected immediately

---

### US-07 — Delete Task
**As an** authenticated user,
**I want to** delete a task after confirmation,
**So that** I can remove tasks I no longer need.

**Acceptance Criteria:**
- Confirmation dialog before deletion
- Cannot delete another user's task
- Deleted task disappears immediately from the list

---

### US-08 — Change Task Status
**As an** authenticated user,
**I want to** advance or cancel a task through its workflow,
**So that** I can track progress.

**Acceptance Criteria:**
- Valid transitions: Pending → InProgress, Pending → Cancelled
- Valid transitions: InProgress → Completed, InProgress → Cancelled
- Invalid transitions return 422 with explanation
- Action buttons are shown only for applicable transitions

---

## Epic 3: Non-Functional

### US-09 — Security
- All task endpoints require a valid JWT Bearer token
- Users can only see and modify their own tasks
- Passwords are hashed with BCrypt (work factor 11)

### US-10 — Responsiveness
- The UI is fully functional on mobile (320px+) and desktop
