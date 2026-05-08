import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Task, TaskStatus } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card" [class]="'priority-' + task.priority.toLowerCase()">
      <div class="card-header">
        <span class="priority-badge" [class]="'p-' + task.priority.toLowerCase()">
          {{ task.priority }}
        </span>
        <span class="status-badge" [class]="'s-' + task.status.toLowerCase()">
          {{ formatStatus(task.status) }}
        </span>
      </div>

      <h3 class="card-title">{{ task.title }}</h3>

      @if (task.description) {
        <p class="card-description">{{ task.description }}</p>
      }

      @if (task.dueDate) {
        <p class="card-due" [class.overdue]="isOverdue(task.dueDate)">
          Due: {{ task.dueDate | date:'mediumDate' }}
          @if (isOverdue(task.dueDate)) { <span>(Overdue)</span> }
        </p>
      }

      <div class="card-actions">
        <div class="status-actions">
          @if (task.status === 'Pending') {
            <button class="btn-action btn-start" (click)="statusChange.emit({ task, status: 'InProgress' })">
              Start
            </button>
            <button class="btn-action btn-cancel" (click)="statusChange.emit({ task, status: 'Cancelled' })">
              Cancel
            </button>
          }
          @if (task.status === 'InProgress') {
            <button class="btn-action btn-done" (click)="statusChange.emit({ task, status: 'Completed' })">
              Complete
            </button>
            <button class="btn-action btn-cancel" (click)="statusChange.emit({ task, status: 'Cancelled' })">
              Cancel
            </button>
          }
        </div>
        <div class="crud-actions">
          <button class="btn-icon btn-edit" (click)="edit.emit(task)" title="Edit">✏️</button>
          <button class="btn-icon btn-delete" (click)="delete.emit(task.id)" title="Delete">🗑️</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      background: white; border-radius: 12px; padding: 1.25rem;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08); border-left: 4px solid #ccc;
      transition: transform 0.15s, box-shadow 0.15s;
    }
    .card:hover { transform: translateY(-2px); box-shadow: 0 4px 16px rgba(0,0,0,0.12); }
    .priority-critical { border-left-color: #e53e3e; }
    .priority-high { border-left-color: #ed8936; }
    .priority-medium { border-left-color: #667eea; }
    .priority-low { border-left-color: #48bb78; }

    .card-header { display: flex; gap: 0.5rem; margin-bottom: 0.75rem; flex-wrap: wrap; }
    .priority-badge, .status-badge {
      padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600;
    }
    .p-critical { background: #fff5f5; color: #c53030; }
    .p-high { background: #fffaf0; color: #c05621; }
    .p-medium { background: #ebf4ff; color: #3c366b; }
    .p-low { background: #f0fff4; color: #276749; }

    .s-pending { background: #fef5e7; color: #b7791f; }
    .s-inprogress { background: #ebf4ff; color: #2b6cb0; }
    .s-completed { background: #f0fff4; color: #276749; }
    .s-cancelled { background: #f7fafc; color: #718096; }

    .card-title { margin: 0 0 0.5rem; font-size: 1rem; font-weight: 600; color: #2d3748; }
    .card-description { margin: 0 0 0.75rem; color: #718096; font-size: 0.875rem; line-height: 1.4; }
    .card-due { font-size: 0.8rem; color: #888; margin: 0 0 0.75rem; }
    .card-due.overdue { color: #e53e3e; font-weight: 600; }

    .card-actions { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; }
    .status-actions, .crud-actions { display: flex; gap: 0.5rem; }

    .btn-action {
      padding: 0.35rem 0.75rem; border: none; border-radius: 6px;
      font-size: 0.8rem; font-weight: 600; cursor: pointer; transition: opacity 0.2s;
    }
    .btn-action:hover { opacity: 0.85; }
    .btn-start { background: #667eea; color: white; }
    .btn-done { background: #48bb78; color: white; }
    .btn-cancel { background: #e2e8f0; color: #666; }
    .btn-icon {
      background: none; border: none; cursor: pointer; font-size: 1rem;
      padding: 0.25rem 0.4rem; border-radius: 4px; transition: background 0.15s;
    }
    .btn-icon:hover { background: #f0f0f0; }
  `]
})
export class TaskCardComponent {
  @Input({ required: true }) task!: Task;
  @Output() edit = new EventEmitter<Task>();
  @Output() delete = new EventEmitter<string>();
  @Output() statusChange = new EventEmitter<{ task: Task; status: TaskStatus }>();

  formatStatus(status: TaskStatus): string {
    return status === 'InProgress' ? 'In Progress' : status;
  }

  isOverdue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }
}
