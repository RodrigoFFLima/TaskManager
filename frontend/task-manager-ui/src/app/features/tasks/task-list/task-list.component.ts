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
          <span class="task-count">{{ filteredTasks().length }} tasks</span>
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
        <div class="loading">Loading tasks...</div>
      } @else if (filteredTasks().length === 0) {
        <div class="empty-state">
          <p>No tasks found. Create your first task!</p>
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
    .layout { min-height: 100vh; background: #f7f8fc; }
    .header {
      background: white; padding: 1rem 2rem;
      display: flex; align-items: center; justify-content: space-between;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1); position: sticky; top: 0; z-index: 10;
    }
    .header-left { display: flex; align-items: center; gap: 1rem; }
    .header-left h1 { margin: 0; color: #667eea; font-size: 1.5rem; }
    .task-count {
      background: #667eea; color: white; padding: 0.2rem 0.6rem;
      border-radius: 20px; font-size: 0.8rem;
    }
    .header-right { display: flex; align-items: center; gap: 1rem; }
    .user-name { color: #555; font-weight: 500; }
    .btn-logout {
      padding: 0.5rem 1rem; border: 2px solid #e53e3e; color: #e53e3e;
      border-radius: 6px; background: none; cursor: pointer; font-weight: 600;
      transition: all 0.2s;
    }
    .btn-logout:hover { background: #e53e3e; color: white; }

    .toolbar {
      padding: 1.5rem 2rem; display: flex; align-items: center;
      justify-content: space-between; gap: 1rem; flex-wrap: wrap;
    }
    .filters { display: flex; gap: 0.75rem; flex-wrap: wrap; }
    select {
      padding: 0.5rem 0.75rem; border: 2px solid #e2e8f0; border-radius: 8px;
      font-size: 0.9rem; color: #444; background: white; cursor: pointer;
    }
    select:focus { outline: none; border-color: #667eea; }

    .btn-primary {
      padding: 0.6rem 1.5rem; background: #667eea; color: white;
      border: none; border-radius: 8px; font-size: 0.95rem; font-weight: 600;
      cursor: pointer; transition: background 0.2s; white-space: nowrap;
    }
    .btn-primary:hover { background: #5a67d8; }

    .task-grid {
      padding: 0 2rem 2rem;
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.25rem;
    }
    .loading, .empty-state {
      text-align: center; padding: 4rem; color: #888; font-size: 1.1rem;
    }

    @media (max-width: 600px) {
      .toolbar { padding: 1rem; flex-direction: column; align-items: stretch; }
      .btn-primary { width: 100%; text-align: center; }
      .task-grid { padding: 0 1rem 1rem; grid-template-columns: 1fr; }
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
