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
      position: fixed; inset: 0;
      background: rgba(4,7,18,0.8);
      backdrop-filter: blur(6px);
      -webkit-backdrop-filter: blur(6px);
      display: flex; align-items: center; justify-content: center;
      z-index: 100; padding: 1rem;
      animation: overlayIn 0.15s ease;
    }
    @keyframes overlayIn { from { opacity: 0; } to { opacity: 1; } }

    .modal {
      background: rgba(15,22,41,0.95);
      border: 1px solid rgba(255,255,255,0.1);
      border-radius: 16px; padding: 2rem;
      width: 100%; max-width: 520px; max-height: 90vh; overflow-y: auto;
      box-shadow: 0 32px 80px rgba(0,0,0,0.6), 0 0 0 1px rgba(99,102,241,0.15);
      animation: modalIn 0.2s ease;
    }
    @keyframes modalIn {
      from { opacity: 0; transform: scale(0.97) translateY(8px); }
      to   { opacity: 1; transform: scale(1) translateY(0); }
    }

    .modal-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;
      padding-bottom: 1rem; border-bottom: 1px solid rgba(255,255,255,0.07);
    }
    .modal-header h2 { margin: 0; color: #e2e8f0; font-size: 1.1rem; font-weight: 600; }
    .btn-close {
      background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.08);
      font-size: 1rem; cursor: pointer; color: #64748b;
      padding: 0.3rem 0.5rem; border-radius: 6px; transition: all 0.15s;
    }
    .btn-close:hover { background: rgba(244,63,94,0.1); border-color: rgba(244,63,94,0.3); color: #f43f5e; }

    .form-group { margin-bottom: 1.25rem; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    label { display: block; font-weight: 500; margin-bottom: 0.4rem; color: #94a3b8; font-size: 0.85rem; }
    input, select, textarea {
      width: 100%; padding: 0.7rem 0.9rem;
      background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1);
      border-radius: 8px; font-size: 0.95rem; color: #e2e8f0; font-family: inherit;
      transition: border-color 0.2s, box-shadow 0.2s;
    }
    input::placeholder, textarea::placeholder { color: #475569; }
    input:focus, select:focus, textarea:focus {
      outline: none; border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99,102,241,0.12);
    }
    input.invalid { border-color: #f43f5e; }
    textarea { resize: vertical; }
    select option { background: #0f1629; color: #e2e8f0; }
    input[type="date"]::-webkit-calendar-picker-indicator { filter: invert(0.5); }

    .error { color: #f43f5e; font-size: 0.8rem; margin-top: 0.25rem; display: block; }
    .alert-error {
      background: rgba(244,63,94,0.08); border: 1px solid rgba(244,63,94,0.25); color: #fda4af;
      padding: 0.75rem; border-radius: 8px; margin-bottom: 1rem; font-size: 0.9rem;
    }
    .modal-footer {
      display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem;
      padding-top: 1rem; border-top: 1px solid rgba(255,255,255,0.07);
    }
    .btn-primary {
      padding: 0.65rem 1.5rem;
      background: linear-gradient(135deg, #6366f1, #4f46e5);
      color: white; border: none; border-radius: 8px; font-weight: 600; cursor: pointer;
      transition: all 0.2s; box-shadow: 0 4px 12px rgba(99,102,241,0.3);
    }
    .btn-primary:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 18px rgba(99,102,241,0.45); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-secondary {
      padding: 0.65rem 1.5rem;
      background: rgba(255,255,255,0.05); color: #94a3b8;
      border: 1px solid rgba(255,255,255,0.1);
      border-radius: 8px; font-weight: 600; cursor: pointer; transition: all 0.15s;
    }
    .btn-secondary:hover { background: rgba(255,255,255,0.1); color: #e2e8f0; }

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
