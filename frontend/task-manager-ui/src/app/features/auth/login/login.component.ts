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
      background: radial-gradient(ellipse at 60% 20%, rgba(99,102,241,0.15) 0%, transparent 60%),
                  radial-gradient(ellipse at 20% 80%, rgba(34,211,238,0.08) 0%, transparent 50%),
                  #080d1a;
      padding: 1rem;
    }
    .auth-card {
      background: rgba(255,255,255,0.04);
      backdrop-filter: blur(16px);
      -webkit-backdrop-filter: blur(16px);
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 16px;
      padding: 2.5rem;
      width: 100%;
      max-width: 420px;
      box-shadow: 0 24px 64px rgba(0,0,0,0.5), 0 0 0 1px rgba(99,102,241,0.1);
    }
    .auth-title {
      background: linear-gradient(135deg, #a5b4fc, #22d3ee);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      text-align: center; font-size: 1.8rem; margin: 0 0 0.25rem; font-weight: 700;
    }
    .auth-subtitle { text-align: center; color: #64748b; font-weight: 400; margin: 0 0 2rem; }
    .form-group { margin-bottom: 1.25rem; }
    label { display: block; font-weight: 500; margin-bottom: 0.4rem; color: #94a3b8; font-size: 0.9rem; }
    input {
      width: 100%; padding: 0.75rem 1rem;
      background: rgba(255,255,255,0.05);
      border: 1px solid rgba(255,255,255,0.1);
      border-radius: 8px; font-size: 1rem; color: #e2e8f0;
      transition: border-color 0.2s, box-shadow 0.2s;
    }
    input::placeholder { color: #475569; }
    input:focus { outline: none; border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99,102,241,0.15); }
    input.invalid { border-color: #f43f5e; }
    .error { color: #f43f5e; font-size: 0.8rem; margin-top: 0.25rem; display: block; }
    .alert-error {
      background: rgba(244,63,94,0.1); border: 1px solid rgba(244,63,94,0.3); color: #fda4af;
      padding: 0.75rem; border-radius: 8px; margin-bottom: 1rem; font-size: 0.9rem;
    }
    .btn-primary {
      width: 100%; padding: 0.875rem;
      background: linear-gradient(135deg, #6366f1, #4f46e5);
      color: white; border: none; border-radius: 8px; font-size: 1rem; font-weight: 600;
      cursor: pointer; transition: all 0.2s;
      box-shadow: 0 4px 15px rgba(99,102,241,0.3);
    }
    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 6px 20px rgba(99,102,241,0.45);
    }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .auth-link { text-align: center; margin-top: 1.25rem; color: #64748b; }
    .auth-link a { color: #818cf8; font-weight: 600; text-decoration: none; }
    .auth-link a:hover { color: #a5b4fc; }
    .demo-hint { text-align: center; font-size: 0.78rem; color: #475569; margin-top: 0.5rem; font-family: 'JetBrains Mono', monospace; }
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
