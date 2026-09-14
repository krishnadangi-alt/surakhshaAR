import React, { useState } from 'react';
import { Shield, Lock, Mail, Building, ArrowRight, ShieldCheck } from 'lucide-react';

interface LoginScreenProps {
  onLogin: (domain?: string) => void;
}

export const LoginScreen: React.FC<LoginScreenProps> = ({ onLogin }) => {
  const [email, setEmail] = useState('admin.safety@jharkhand.gov.in');
  const [password, setPassword] = useState('••••••••••••');
  const [sector, setSector] = useState('Jharkhand Industrial Safety Command');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setTimeout(() => {
      setIsLoading(false);
      onLogin(sector);
    }, 500);
  };

  return (
    <div className="min-h-screen w-screen bg-[#08101D] text-[#F4F7FA] flex flex-col justify-between items-center p-6 relative overflow-hidden font-sans">
      {/* Background Subtle Geometric Grid Pattern */}
      <div className="absolute inset-0 bg-[linear-gradient(to_right,#1E3A5F15_1px,transparent_1px),linear-gradient(to_bottom,#1E3A5F15_1px,transparent_1px)] bg-[size:32px_32px] pointer-events-none" />

      {/* Top Institutional Header */}
      <header className="w-full max-w-md pt-6 text-center z-10">
        <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-[#101C2E] border border-[#1E3A5F] text-xs font-semibold text-[#94A3B8] shadow-sm">
          <ShieldCheck className="w-4 h-4 text-[#F59E0B]" />
          <span>Government of Jharkhand — Ministry of Labour & Employment</span>
        </div>
      </header>

      {/* Centered Executive Login Console */}
      <div className="w-full max-w-md my-auto z-10">
        <div className="rounded-2xl border border-[#1E3A5F] bg-[#101C2E] p-8 shadow-2xl relative">
          {/* Subtle Top Safety Accent Line */}
          <div className="absolute top-0 left-8 right-8 h-[2px] bg-gradient-to-r from-transparent via-[#F59E0B] to-transparent opacity-80" />

          {/* Institutional Mark & Title */}
          <div className="text-center mb-8">
            <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-[#14243A] border border-[#1E3A5F] text-[#F59E0B] mb-4 shadow-md">
              <Shield className="w-7 h-7 fill-[#F59E0B]/15" />
            </div>
            <h1 className="text-2xl font-extrabold tracking-wider text-white uppercase">SURAKSHAAR</h1>
            <p className="text-xs font-bold text-[#F59E0B] uppercase tracking-widest mt-1">
              Industrial Safety Training & Compliance
            </p>
            <p className="text-xs text-[#94A3B8] mt-1.5">
              Directorate of Industrial Safety & Health (DISH)
            </p>
          </div>

          {/* Login Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-[11px] font-bold uppercase tracking-wider text-[#94A3B8] mb-1.5">
                Official Government Email / Admin ID
              </label>
              <div className="relative">
                <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-[#94A3B8]" />
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="admin.safety@jharkhand.gov.in"
                  className="w-full rounded-xl border border-[#1E3A5F] bg-[#14243A] pl-10 pr-4 py-2.5 text-xs font-semibold text-white placeholder-[#64748B] focus:border-[#1D6BF3] focus:outline-none focus:ring-1 focus:ring-[#1D6BF3]"
                />
              </div>
            </div>

            <div>
              <label className="block text-[11px] font-bold uppercase tracking-wider text-[#94A3B8] mb-1.5">
                Security Passcode
              </label>
              <div className="relative">
                <Lock className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-[#94A3B8]" />
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••••••"
                  className="w-full rounded-xl border border-[#1E3A5F] bg-[#14243A] pl-10 pr-4 py-2.5 text-xs font-semibold text-white focus:border-[#1D6BF3] focus:outline-none focus:ring-1 focus:ring-[#1D6BF3]"
                />
              </div>
            </div>

            <div>
              <label className="block text-[11px] font-bold uppercase tracking-wider text-[#94A3B8] mb-1.5">
                Jurisdiction Zone
              </label>
              <div className="relative">
                <Building className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-[#94A3B8]" />
                <select
                  value={sector}
                  onChange={(e) => setSector(e.target.value)}
                  className="w-full rounded-xl border border-[#1E3A5F] bg-[#14243A] pl-10 pr-4 py-2.5 text-xs font-semibold text-white focus:border-[#1D6BF3] focus:outline-none"
                >
                  <option value="Jharkhand Industrial Safety Command">State Command HQ (Ranchi)</option>
                  <option value="Dhanbad Mining Circle">Dhanbad Mining Circle</option>
                  <option value="Bokaro Steel Zone">Bokaro Steel Industrial Zone</option>
                  <option value="Jamshedpur Metallurgy Belt">Jamshedpur Metallurgy Belt</option>
                </select>
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full flex items-center justify-center gap-2 rounded-xl bg-[#1D6BF3] py-3 text-xs font-bold text-white hover:bg-[#1556C7] transition shadow-md mt-6 disabled:opacity-50"
            >
              {isLoading ? (
                <span>Verifying Credentials...</span>
              ) : (
                <>
                  <span>Sign In to Safety Console</span>
                  <ArrowRight className="w-4 h-4" />
                </>
              )}
            </button>
          </form>

          {/* Quick Demo Access Trigger */}
          <div className="mt-5 pt-4 border-t border-[#1E3A5F]/60 text-center">
            <button
              type="button"
              onClick={() => onLogin('Jharkhand Industrial Safety Command')}
              className="text-xs font-semibold text-[#F59E0B] hover:underline"
            >
              ⚡ Instant Demo Access (Bypass Auth)
            </button>
          </div>
        </div>

        <p className="text-[11px] text-center text-[#64748B] mt-5">
          Authorized personnel only. All access attempts are logged under the Jharkhand Industrial Safety Act.
        </p>
      </div>

      {/* Footer */}
      <footer className="w-full text-center pb-3 text-[11px] text-[#64748B] z-10">
        © 2026 SURAKSHAAR Portal — Directorate of Industrial Safety & Health (DISH), Jharkhand.
      </footer>
    </div>
  );
};
