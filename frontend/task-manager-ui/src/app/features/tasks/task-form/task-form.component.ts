import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TaskService } from '../../../core/services/task.service';
import { Task } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="overlay" (click)="onOverlayClick($event)">
      <div class="modal">
        <div class="modal-header">
          <h2>{{ task ? 'Edit Task' : 'New Task' }}</h2>
          <button class="btn-close" (click)="cancelled.emit()">✕</button>
        </div>

        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Title *</label>
            <input type="text" formControlName="title" placeholder="Task title"
                   [class.invalid]="isInvalid('title')">
            @if (isInvalid('title')) { <span class="error">Title is required (max 200 chars)</span> }
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea formControlName="description" rows="3" placeholder="Optional description"></textarea>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Priority *</label>
              <select formControlName="priority">
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Critical">Critical</option>
              </select>
            </div>
            <div class="form-group">
              <label>Due Date</label>
              <input type="date" formControlName="dueDate">
            </div>
          </div>

          @if (errorMessage) {
            <div class="alert-error">{{ errorMessage }}</div>
          }

          <div class="modal-footer">
            <button type="button" class="btn-secondary" (click)="cancelled.emit()">Cancel</button>
            <button type="submit" class="btn-primary" [disabled]="loading">
              {{ loading ? 'Saving...' : (task ? 'Update' : 'Create') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5);
      display: flex; align-items: center; justify-content: center;
      z-index: 100; padding: 1rem;
    }
    .modal {
      background: white; border-radius: 12px; padding: 2rem;
      width: 100%; max-width: 520px; max-height: 90vh; overflow-y: auto;
      box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    }
    .modal-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;
    }
    .modal-header h2 { margin: 0; color: #2d3748; }
    .btn-close {
      background: none; border: none; font-size: 1.2rem; cursor: pointer;
      color: #888; padding: 0.25rem; border-radius: 4px;
    }
    .btn-close:hover { background: #f0f0f0; color: #333; }

    .form-group { margin-bottom: 1.25rem; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    label { display: block; font-weight: 500; margin-bottom: 0.4rem; color: #444; font-size: 0.9rem; }
    input, select, textarea {
      width: 100%; padding: 0.7rem 0.9rem; border: 2px solid #e2e8f0; border-radius: 8px;
      font-size: 0.95rem; box-sizing: border-box; font-family: inherit;
    }
    input:focus, select:focus, textarea:focus { outline: none; border-color: #667eea; }
    input.invalid { border-color: #e53e3e; }
    textarea { resize: vertical; }
    .error { color: #e53e3e; font-size: 0.8rem; margin-top: 0.25rem; display: block; }
    .alert-error {
      background: #fff5f5; border: 1px solid #fed7d7; color: #c53030;
      padding: 0.75rem; border-radius: 8px; margin-bottom: 1rem; font-size: 0.9rem;
    }
    .modal-footer { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem; }
    .btn-primary {
      padding: 0.7rem 1.5rem; background: #667eea; color: white; border: none;
      border-radius: 8px; font-weight: 600; cursor: pointer; transition: background 0.2s;
    }
    .btn-primary:hover:not(:disabled) { background: #5a67d8; }
    .btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }
    .btn-secondary {
      padding: 0.7rem 1.5rem; background: #e2e8f0; color: #555; border: none;
      border-radius: 8px; font-weight: 600; cursor: pointer;
    }
    .btn-secondary:hover { background: #cbd5e0; }

    @media (max-width: 480px) {
      .form-row { grid-template-columns: 1fr; }
    }
  `]
})
export class TaskFormComponent implements OnInit {
  @Input() task?: Task;
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  private taskService = inject(TaskService);
  private fb = inject(FormBuilder);

  form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    priority: ['Medium'],
    dueDate: [null as string | null]
  });

  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    if (this.task) {
      this.form.patchValue({
        title: this.task.title,
        description: this.task.description ?? '',
        priority: this.task.priority,
        dueDate: this.task.dueDate ? this.task.dueDate.split('T')[0] : null
      });
    }
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.invalid && ctrl.touched);
  }

  onOverlayClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('overlay')) {
      this.cancelled.emit();
    }
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading = true;
    this.errorMessage = '';

    const value = this.form.value;
    const payload = {
      title: value.title!,
      description: value.description || undefined,
      priority: value.priority as any,
      dueDate: value.dueDate || undefined
    };

    const req$ = this.task
      ? this.taskService.update(this.task.id, payload)
      : this.taskService.create(payload);

    req$.subscribe({
      next: () => { this.loading = false; this.saved.emit(); },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message ?? 'Failed to save task.';
      }
    });
  }
}
