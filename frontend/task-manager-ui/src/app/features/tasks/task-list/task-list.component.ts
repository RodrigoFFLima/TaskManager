import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaskService } from '../../../core/services/task.service';
import { AuthService } from '../../../core/services/auth.service';
import { Task, TaskStatus, TaskPriority } from '../../../core/models/task.model';
import { TaskFormComponent } from '../task-form/task-form.component';
import { TaskCardComponent } from '../task-card/task-card.component';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TaskFormComponent, TaskCardComponent],
  template: `
    <div class="layout">
      <header class="header">
        <div class="header-left">
          <h1>Task Manager</h1>
          <span class="task-count">{{ filteredTasks().length }} {{ filteredTasks().length === 1 ? 'task' : 'tasks' }}</span>
        </div>
        <div class="header-right">
          <span class="user-name">{{ user?.name }}</span>
          <button class="btn-logout" (click)="logout()">Logout</button>
        </div>
      </header>

      <div class="toolbar">
        <div class="filters">
          <select [(ngModel)]="statusFilter" (change)="applyFilter()">
            <option value="">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="InProgress">In Progress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
          <select [(ngModel)]="priorityFilter" (change)="applyFilter()">
            <option value="">All Priorities</option>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
            <option value="Critical">Critical</option>
          </select>
        </div>
        <button class="btn-primary" (click)="openCreate()">+ New Task</button>
      </div>

      @if (loading()) {
        <div class="loading">
          <div class="spinner"></div>
          <span>Loading tasks...</span>
        </div>
      } @else if (filteredTasks().length === 0) {
        <div class="empty-state">
          <div class="empty-icon">◫</div>
          <p class="empty-title">No tasks found</p>
          <p class="empty-sub">{{ statusFilter || priorityFilter ? 'Try clearing the filters.' : 'Create your first task to get started.' }}</p>
        </div>
      } @else {
        <div class="task-grid">
          @for (task of filteredTasks(); track task.id) {
            <app-task-card
              [task]="task"
              (edit)="openEdit($event)"
              (delete)="deleteTask($event)"
              (statusChange)="changeStatus($event.task, $event.status)">
            </app-task-card>
          }
        </div>
      }

      @if (showForm()) {
        <app-task-form
          [task]="editingTask()"
          (saved)="onSaved()"
          (cancelled)="closeForm()">
        </app-task-form>
      }
    </div>
  `,
  styles: [`
    .layout { min-height: 100vh; background: #080d1a; }

    .header {
      background: rgba(8,13,26,0.85);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border-bottom: 1px solid rgba(255,255,255,0.06);
      padding: 0.875rem 2rem;
      display: flex; align-items: center; justify-content: space-between;
      position: sticky; top: 0; z-index: 10;
    }
    .header-left { display: flex; align-items: center; gap: 1rem; }
    .header-left h1 {
      margin: 0; font-size: 1.25rem; font-weight: 700;
      background: linear-gradient(135deg, #a5b4fc, #22d3ee);
      -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
    }
    .task-count {
      background: rgba(99,102,241,0.15); color: #a5b4fc;
      border: 1px solid rgba(99,102,241,0.25);
      padding: 0.15rem 0.6rem; border-radius: 20px; font-size: 0.75rem;
      font-family: 'JetBrains Mono', monospace; font-weight: 600;
    }
    .header-right { display: flex; align-items: center; gap: 1rem; }
    .user-name { color: #64748b; font-size: 0.9rem; }
    .btn-logout {
      padding: 0.4rem 0.9rem;
      border: 1px solid rgba(244,63,94,0.3); color: #f43f5e;
      border-radius: 6px; background: rgba(244,63,94,0.05); cursor: pointer;
      font-size: 0.85rem; font-weight: 600; transition: all 0.2s;
    }
    .btn-logout:hover { background: rgba(244,63,94,0.15); border-color: rgba(244,63,94,0.5); }

    .toolbar {
      padding: 1.25rem 2rem; display: flex; align-items: center;
      justify-content: space-between; gap: 1rem; flex-wrap: wrap;
      border-bottom: 1px solid rgba(255,255,255,0.04);
    }
    .filters { display: flex; gap: 0.75rem; flex-wrap: wrap; }
    select {
      padding: 0.45rem 0.75rem;
      background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1);
      border-radius: 8px; font-size: 0.875rem; color: #94a3b8; cursor: pointer;
      transition: border-color 0.2s;
    }
    select:focus { outline: none; border-color: #6366f1; }
    select option { background: #0f1629; color: #e2e8f0; }

    .btn-primary {
      padding: 0.5rem 1.25rem;
      background: linear-gradient(135deg, #6366f1, #4f46e5);
      color: white; border: none; border-radius: 8px; font-size: 0.9rem; font-weight: 600;
      cursor: pointer; transition: all 0.2s; white-space: nowrap;
      box-shadow: 0 4px 12px rgba(99,102,241,0.3);
    }
    .btn-primary:hover { transform: translateY(-1px); box-shadow: 0 6px 18px rgba(99,102,241,0.45); }

    .task-grid {
      padding: 1.5rem 2rem 2rem;
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.25rem;
    }

    .loading {
      display: flex; flex-direction: column; align-items: center;
      justify-content: center; gap: 1rem; padding: 5rem; color: #475569;
    }
    .spinner {
      width: 36px; height: 36px;
      border: 2px solid rgba(99,102,241,0.15);
      border-top-color: #6366f1;
      border-radius: 50%;
      animation: spin 0.7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

    .empty-state {
      text-align: center; padding: 5rem 2rem; color: #475569;
    }
    .empty-icon { font-size: 3rem; margin-bottom: 1rem; opacity: 0.3; }
    .empty-title { font-size: 1.1rem; font-weight: 600; color: #64748b; margin: 0 0 0.5rem; }
    .empty-sub { font-size: 0.875rem; color: #475569; margin: 0; }

    @media (max-width: 600px) {
      .header { padding: 0.875rem 1rem; }
      .toolbar { padding: 1rem; flex-direction: column; align-items: stretch; }
      .btn-primary { width: 100%; text-align: center; }
      .task-grid { padding: 1rem; grid-template-columns: 1fr; }
    }
  `]
})
export class TaskListComponent implements OnInit {
  private taskService = inject(TaskService);
  private authService = inject(AuthService);

  tasks = signal<Task[]>([]);
  filteredTasks = signal<Task[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingTask = signal<Task | undefined>(undefined);

  statusFilter = '';
  priorityFilter = '';
  user = this.authService.getUser();

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading.set(true);
    this.taskService.getAll().subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.applyFilter();
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  applyFilter(): void {
    let result = this.tasks();
    if (this.statusFilter) result = result.filter(t => t.status === this.statusFilter);
    if (this.priorityFilter) result = result.filter(t => t.priority === this.priorityFilter);
    this.filteredTasks.set(result);
  }

  openCreate(): void {
    this.editingTask.set(undefined);
    this.showForm.set(true);
  }

  openEdit(task: Task): void {
    this.editingTask.set(task);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
  }

  onSaved(): void {
    this.closeForm();
    this.loadTasks();
  }

  deleteTask(id: string): void {
    if (!confirm('Delete this task?')) return;
    this.taskService.delete(id).subscribe(() => this.loadTasks());
  }

  changeStatus(task: Task, status: TaskStatus): void {
    this.taskService.changeStatus(task.id, { status }).subscribe(() => this.loadTasks());
  }

  logout(): void {
    this.authService.logout();
  }
}
