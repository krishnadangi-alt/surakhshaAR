import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [focusedField, setFocusedField] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    if (!username.trim() || !password.trim()) {
      setError('Please enter both username and password.');
      return;
    }
    setLoading(true);
    const ok = await login(username.trim(), password);
    setLoading(false);
    if (ok) {
      navigate('/', { replace: true });
    } else {
      setError('Invalid credentials. Please try again.');
    }
  }

    return (
    <div className="login-shell">
        <div className="login-bg-pattern" />
        <div className="login-bg-overlay" />

      <div className="login-container">
        <div className="login-brand-panel animate-slide-left">
          <div className="brand-content">
            <div className="brand-emblem animate-float">
              <div className="emblem-icon">🏛</div>
            </div>
            <h1 className="brand-title">SURAKSHAAR</h1>
            <div className="brand-tagline">Industrial Safety Training<br />&amp; Competency Management System</div>
            <div className="brand-divider">
              <span className="divider-line" />
              <span className="divider-icon">🇮🇳</span>
              <span className="divider-line" />
            </div>
            <div className="brand-org">Government of India</div>
            <div className="brand-features">
              <div className="feature-item">
                <span className="feature-dot success" />
                <span>Worker Safety Monitoring</span>
              </div>
              <div className="feature-item">
                <span className="feature-dot warning" />
                <span>Competency Assessment</span>
              </div>
              <div className="feature-item">
                <span className="feature-dot info" />
                <span>Training Management</span>
              </div>
            </div>
          </div>
        </div>

        <div className="login-form-panel animate-slide-right">
          <div className="login-card animate-bounce-in">
            <div className="login-header">
              <div className="login-header-icon">🔒</div>
              <h2>Welcome Back</h2>
              <p>Sign in to access the admin portal</p>
            </div>

            <div className="login-body">
              {error && (
                <div className="login-error animate-shake">
                  <span className="error-icon">⚠</span>
                  <span>{error}</span>
                </div>
              )}

              <form onSubmit={handleSubmit} className="login-form">
                <div className="form-group">
                  <label htmlFor="username" className="form-label">Username / Email</label>
                  <input
                    id="username"
                    type="text"
                    className={`form-input ${focusedField === 'username' ? 'focused' : ''}`}
                    placeholder="Enter your username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    onFocus={() => setFocusedField('username')}
                    onBlur={() => setFocusedField(null)}
                    autoComplete="username"
                    required
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="password" className="form-label">Password</label>
                  <input
                    id="password"
                    type="password"
                    className={`form-input ${focusedField === 'password' ? 'focused' : ''}`}
                    placeholder="Enter your password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    onFocus={() => setFocusedField('password')}
                    onBlur={() => setFocusedField(null)}
                    autoComplete="current-password"
                    required
                  />
                </div>

                <button
                  type="submit"
                  className={`btn btn-primary btn-block ${loading ? 'loading' : ''}`}
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <span className="spinner" />
                      <span>Signing in...</span>
                    </>
                  ) : (
                    'Sign In'
                  )}
                </button>
              </form>

              <div className="login-footer">
                <div className="demo-creds">
                  <small>Demo: admin / admin123</small>
                  <small>Demo: trainer / trainer123</small>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
