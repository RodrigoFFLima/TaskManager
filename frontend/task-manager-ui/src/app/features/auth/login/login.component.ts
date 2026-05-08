import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h1 class="auth-title">Task Manager</h1>
        <h2 class="auth-subtitle">Sign In</h2>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="email">Email</label>
            <input id="email" type="email" formControlName="email"
                   placeholder="demo@taskmanager.com"
                   [class.invalid]="form.get('email')?.invalid && form.get('email')?.touched">
            @if (form.get('email')?.invalid && form.get('email')?.touched) {
              <span class="error">Valid email is required</span>
            }
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input id="password" type="password" formControlName="password"
                   placeholder="Password"
                   [class.invalid]="form.get('password')?.invalid && form.get('password')?.touched">
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <span class="error">Password is required</span>
            }
          </div>

          @if (errorMessage) {
            <div class="alert-error">{{ errorMessage }}</div>
          }

          <button type="submit" class="btn-primary" [disabled]="loading">
            {{ loading ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>

        <p class="auth-link">
          Don't have an account? <a routerLink="/register">Register</a>
        </p>
        <p class="demo-hint">Demo: demo&#64;taskmanager.com / Demo&#64;1234</p>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 1rem;
    }
    .auth-card {
      background: white;
      border-radius: 12px;
      padding: 2.5rem;
      width: 100%;
      max-width: 420px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.2);
    }
    .auth-title { color: #667eea; text-align: center; font-size: 1.8rem; margin: 0 0 0.25rem; }
    .auth-subtitle { text-align: center; color: #666; font-weight: 400; margin: 0 0 2rem; }
    .form-group { margin-bottom: 1.25rem; }
    label { display: block; font-weight: 500; margin-bottom: 0.4rem; color: #444; }
    input {
      width: 100%; padding: 0.75rem 1rem; border: 2px solid #e2e8f0; border-radius: 8px;
      font-size: 1rem; box-sizing: border-box; transition: border-color 0.2s;
    }
    input:focus { outline: none; border-color: #667eea; }
    input.invalid { border-color: #e53e3e; }
    .error { color: #e53e3e; font-size: 0.8rem; margin-top: 0.25rem; display: block; }
    .alert-error {
      background: #fff5f5; border: 1px solid #fed7d7; color: #c53030;
      padding: 0.75rem; border-radius: 8px; margin-bottom: 1rem; font-size: 0.9rem;
    }
    .btn-primary {
      width: 100%; padding: 0.875rem; background: #667eea; color: white;
      border: none; border-radius: 8px; font-size: 1rem; font-weight: 600;
      cursor: pointer; transition: background 0.2s;
    }
    .btn-primary:hover:not(:disabled) { background: #5a67d8; }
    .btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }
    .auth-link { text-align: center; margin-top: 1.25rem; color: #666; }
    .auth-link a { color: #667eea; font-weight: 600; text-decoration: none; }
    .demo-hint { text-align: center; font-size: 0.78rem; color: #999; margin-top: 0.5rem; }
  `]
})
export class LoginComponent {
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  loading = false;
  errorMessage = '';

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.loading = true;
    this.errorMessage = '';

    this.auth.login(this.form.value as any).subscribe({
      next: () => this.loading = false,
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message ?? 'Login failed. Please try again.';
      }
    });
  }
}
