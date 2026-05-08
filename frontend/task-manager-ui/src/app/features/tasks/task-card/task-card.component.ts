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
              ▶ Start
            </button>
          }
          @if (task.status === 'InProgress') {
            <button class="btn-action btn-done" (click)="statusChange.emit({ task, status: 'Completed' })">
              ✓ Complete
            </button>
          }
        </div>
        <div class="crud-actions">
          <button class="btn-icon" (click)="edit.emit(task)" title="Edit task">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
            </svg>
          </button>
          <button class="btn-icon btn-icon-danger" (click)="delete.emit(task.id)" title="Delete task">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"/>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      background: rgba(255,255,255,0.04);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border: 1px solid rgba(255,255,255,0.07);
      border-left: 3px solid rgba(255,255,255,0.2);
      border-radius: 12px; padding: 1.25rem;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      animation: fadeIn 0.3s ease;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(8px); }
      to   { opacity: 1; transform: translateY(0); }
    }
    .card:hover { transform: translateY(-3px); }

    .priority-critical {
      border-left-color: #f43f5e;
      box-shadow: -2px 0 20px rgba(244,63,94,0.2), 0 4px 24px rgba(0,0,0,0.3);
    }
    .priority-critical:hover { box-shadow: -2px 0 28px rgba(244,63,94,0.35), 0 8px 32px rgba(0,0,0,0.4); }
    .priority-high {
      border-left-color: #fb923c;
      box-shadow: -2px 0 20px rgba(251,146,60,0.2), 0 4px 24px rgba(0,0,0,0.3);
    }
    .priority-high:hover { box-shadow: -2px 0 28px rgba(251,146,60,0.35), 0 8px 32px rgba(0,0,0,0.4); }
    .priority-medium {
      border-left-color: #6366f1;
      box-shadow: -2px 0 20px rgba(99,102,241,0.2), 0 4px 24px rgba(0,0,0,0.3);
    }
    .priority-medium:hover { box-shadow: -2px 0 28px rgba(99,102,241,0.35), 0 8px 32px rgba(0,0,0,0.4); }
    .priority-low {
      border-left-color: #34d399;
      box-shadow: -2px 0 20px rgba(52,211,153,0.2), 0 4px 24px rgba(0,0,0,0.3);
    }
    .priority-low:hover { box-shadow: -2px 0 28px rgba(52,211,153,0.35), 0 8px 32px rgba(0,0,0,0.4); }

    .card-header { display: flex; gap: 0.5rem; margin-bottom: 0.75rem; flex-wrap: wrap; }
    .priority-badge, .status-badge {
      padding: 0.2rem 0.6rem; border-radius: 4px; font-size: 0.7rem; font-weight: 600;
      font-family: 'JetBrains Mono', monospace; letter-spacing: 0.03em; text-transform: uppercase;
    }
    .p-critical { background: rgba(244,63,94,0.15); color: #fb7185; border: 1px solid rgba(244,63,94,0.25); }
    .p-high     { background: rgba(251,146,60,0.15); color: #fdba74; border: 1px solid rgba(251,146,60,0.25); }
    .p-medium   { background: rgba(99,102,241,0.15); color: #a5b4fc; border: 1px solid rgba(99,102,241,0.25); }
    .p-low      { background: rgba(52,211,153,0.15); color: #6ee7b7; border: 1px solid rgba(52,211,153,0.25); }

    .s-pending    { background: rgba(251,191,36,0.1);  color: #fcd34d; border: 1px solid rgba(251,191,36,0.2); }
    .s-inprogress { background: rgba(56,189,248,0.1);  color: #7dd3fc; border: 1px solid rgba(56,189,248,0.2); }
    .s-completed  { background: rgba(52,211,153,0.1);  color: #6ee7b7; border: 1px solid rgba(52,211,153,0.2); }
    .s-cancelled  { background: rgba(100,116,139,0.1); color: #94a3b8; border: 1px solid rgba(100,116,139,0.2); }

    .card-title { margin: 0 0 0.5rem; font-size: 1rem; font-weight: 600; color: #e2e8f0; }
    .card-description { margin: 0 0 0.75rem; color: #64748b; font-size: 0.875rem; line-height: 1.5; }
    .card-due { font-size: 0.8rem; color: #64748b; margin: 0 0 0.75rem; display: flex; align-items: center; gap: 0.3rem; }
    .card-due.overdue { color: #f43f5e; font-weight: 600; }

    .card-actions { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.5rem; margin-top: 1rem; padding-top: 0.75rem; border-top: 1px solid rgba(255,255,255,0.06); }
    .status-actions, .crud-actions { display: flex; gap: 0.5rem; }

    .btn-action {
      padding: 0.3rem 0.7rem; border: 1px solid transparent; border-radius: 6px;
      font-size: 0.75rem; font-weight: 600; cursor: pointer; transition: all 0.15s;
    }
    .btn-start { background: rgba(99,102,241,0.15); color: #a5b4fc; border-color: rgba(99,102,241,0.3); }
    .btn-start:hover { background: rgba(99,102,241,0.3); }
    .btn-done  { background: rgba(52,211,153,0.15); color: #6ee7b7; border-color: rgba(52,211,153,0.3); }
    .btn-done:hover  { background: rgba(52,211,153,0.3); }

    .btn-icon {
      background: none; border: 1px solid transparent; cursor: pointer;
      padding: 0.35rem; border-radius: 6px; transition: all 0.15s;
      color: #475569; display: flex; align-items: center; justify-content: center;
    }
    .btn-icon:hover { background: rgba(255,255,255,0.07); border-color: rgba(255,255,255,0.1); color: #94a3b8; }
    .btn-icon-danger:hover { background: rgba(244,63,94,0.1); border-color: rgba(244,63,94,0.25); color: #f43f5e; }
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
