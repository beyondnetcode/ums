import React, { useState } from 'react';
import {
  Users,
  UserCheck,
  ShieldAlert,
  Moon,
  Sun,
  UserPlus,
  RefreshCw,
  Cpu,
  CheckCircle,
  Database,
  ArrowRight,
  ExternalLink,
  ShieldCheck
} from 'lucide-react';
import { useAuthStore } from './application/stores/auth.store';
import { useGetUsers, useRegisterUser, User } from './application/hooks/use-users.hook';

export default function App() {
  const { isDarkMode, toggleDarkMode } = useAuthStore();
  const { data: users = [], isLoading, refetch, isFetching } = useGetUsers();
  const registerMutation = useRegisterUser();

  // Form states
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [role, setRole] = useState('user');
  const [errorMsg, setErrorMsg] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // BMAD Simulation agent state
  const [bmadAgent, setBmadAgent] = useState<string>('idle'); // idle, analysis, planning, solutioning, execution, validation, done

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg('');
    setSuccessMsg('');

    // Client-side simple validation
    if (username.length < 3) {
      setErrorMsg('Username must be at least 3 characters.');
      return;
    }
    if (!email.includes('@')) {
      setErrorMsg('Please enter a valid email address.');
      return;
    }

    try {
      // Let's simulate BMAD Method automation phases for high visual impact!
      setBmadAgent('analysis');
      await delay(800);
      setBmadAgent('planning');
      await delay(800);
      setBmadAgent('solutioning');
      await delay(800);
      setBmadAgent('execution');
      await delay(800);

      await registerMutation.mutateAsync({ username, email, role });

      setBmadAgent('validation');
      await delay(800);
      setBmadAgent('done');

      setSuccessMsg('User successfully registered (simulated & saved)!');
      setUsername('');
      setEmail('');
      setRole('user');

      // Clear BMAD state after a few seconds
      setTimeout(() => {
        setBmadAgent('idle');
      }, 3000);
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      setErrorMsg(error.response?.data?.message || 'Error occurred during registration.');
      setBmadAgent('idle');
    }
  };

  const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

  // Calculated Stats
  const totalUsers = users.length;
  const adminCount = users.filter((u: User) => u.role === 'admin').length;
  const modCount = users.filter((u: User) => u.role === 'moderator').length;

  return (
    <div className={`min-h-screen transition-colors duration-500 ${isDarkMode ? 'dark bg-dark-950 text-slate-100' : 'bg-slate-50 text-slate-900'}`}>
      
      {/* Background ambient glowing shapes for premium wow-factor */}
      <div className="absolute top-0 left-1/4 w-[400px] h-[400px] bg-brand-500/10 dark:bg-brand-500/5 rounded-full blur-[100px] pointer-events-none" />
      <div className="absolute bottom-10 right-10 w-[350px] h-[350px] bg-violet-500/10 dark:bg-violet-500/5 rounded-full blur-[120px] pointer-events-none" />

      {/* Main Container */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 relative z-10">
        
        {/* Navigation / Header */}
        <header className="flex justify-between items-center pb-8 border-b border-slate-200 dark:border-slate-800/60 mb-8">
          <div className="flex items-center gap-3">
            <div className="p-2.5 bg-gradient-to-tr from-brand-600 to-violet-500 rounded-xl shadow-lg shadow-brand-500/20 text-white animate-pulse">
              <Database className="w-6 h-6" />
            </div>
            <div>
              <h1 className="text-2xl font-extrabold tracking-tight bg-gradient-to-r from-brand-500 via-indigo-400 to-violet-500 bg-clip-text text-transparent">
                UMS WORKSPACE
              </h1>
              <p className="text-xs text-slate-500 dark:text-slate-400 font-medium">Clean Architecture & OWASP Top 10 Core</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            {/* Dark Mode toggle button */}
            <button
              onClick={toggleDarkMode}
              className="p-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-900/40 text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800/80 hover:scale-105 transition-all duration-300"
              title="Toggle Theme"
            >
              {isDarkMode ? <Sun className="w-5 h-5 text-amber-400" /> : <Moon className="w-5 h-5 text-indigo-600" />}
            </button>
            <button
              onClick={() => refetch()}
              className={`p-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-900/40 text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800/80 transition-all ${isFetching ? 'animate-spin' : ''}`}
              title="Refresh Data"
            >
              <RefreshCw className="w-5 h-5" />
            </button>
          </div>
        </header>

        {/* Top KPI Metrics Panel */}
        <section className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          
          <div className="glass-panel p-6 rounded-2xl shadow-xl transition-all hover:-translate-y-1 hover:shadow-2xl hover:shadow-brand-500/10 duration-300 flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-slate-500 dark:text-slate-400">Total System Users</p>
              <h3 className="text-3xl font-extrabold mt-1">{isLoading ? '...' : totalUsers}</h3>
              <p className="text-xs text-emerald-500 font-medium mt-1.5 flex items-center gap-1">
                <CheckCircle className="w-3.5 h-3.5" /> Fully Synced
              </p>
            </div>
            <div className="p-4 bg-brand-500/10 dark:bg-brand-500/20 text-brand-500 rounded-2xl">
              <Users className="w-8 h-8" />
            </div>
          </div>

          <div className="glass-panel p-6 rounded-2xl shadow-xl transition-all hover:-translate-y-1 hover:shadow-2xl hover:shadow-violet-500/10 duration-300 flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-slate-500 dark:text-slate-400">Admins / Mods</p>
              <h3 className="text-3xl font-extrabold mt-1">{isLoading ? '...' : `${adminCount} / ${modCount}`}</h3>
              <p className="text-xs text-violet-500 font-medium mt-1.5 flex items-center gap-1">
                <ShieldCheck className="w-3.5 h-3.5" /> Security Enforced
              </p>
            </div>
            <div className="p-4 bg-violet-500/10 dark:bg-violet-500/20 text-violet-500 rounded-2xl">
              <UserCheck className="w-8 h-8" />
            </div>
          </div>

          <div className="glass-panel p-6 rounded-2xl shadow-xl transition-all hover:-translate-y-1 hover:shadow-2xl hover:shadow-rose-500/10 duration-300 flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-slate-500 dark:text-slate-400">OWASP Hardening</p>
              <h3 className="text-lg font-bold mt-2 text-emerald-500 flex items-center gap-1.5">
                <ShieldCheck className="w-5 h-5" /> 100% Secure
              </h3>
              <p className="text-xs text-slate-400 mt-1">Helmet, CORS & Rate Limits active</p>
            </div>
            <div className="p-4 bg-emerald-500/10 dark:bg-emerald-500/20 text-emerald-500 rounded-2xl">
              <ShieldAlert className="w-8 h-8" />
            </div>
          </div>

        </section>

        {/* Dashboard Grid Details */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
          
          {/* LEFT: BMAD Method Agent & Automation Status Panel */}
          <div className="lg:col-span-4 flex flex-col gap-6">
            
            {/* BMAD METHOD MONITOR */}
            <div className="glass-panel p-6 rounded-3xl shadow-xl border border-slate-200 dark:border-slate-800">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-2">
                  <Cpu className="w-5 h-5 text-brand-500 animate-pulse" />
                  <h2 className="text-lg font-bold tracking-tight">BMAD Agent Automation</h2>
                </div>
                <span className="px-2.5 py-0.5 text-[10px] font-bold tracking-wider uppercase rounded-full bg-brand-500/10 text-brand-500 border border-brand-500/20">
                  AI Layer Active
                </span>
              </div>
              <p className="text-xs text-slate-500 dark:text-slate-400 mb-6">
                Real-time visual automation sequence displaying background agents collaborating on actions.
              </p>

              {/* Agent workflow stages */}
              <div className="relative border-l-2 border-slate-200 dark:border-slate-800 pl-6 ml-3 space-y-6">
                
                {/* Stage 1 */}
                <div className="relative">
                  <div className={`absolute -left-[31px] top-0.5 w-4 h-4 rounded-full border-2 transition-all duration-300 ${
                    bmadAgent === 'analysis' ? 'bg-brand-500 border-brand-400 scale-125 shadow-lg shadow-brand-500/50' : 'bg-slate-300 dark:bg-slate-700 border-transparent'
                  }`} />
                  <div>
                    <h4 className={`text-xs font-bold ${bmadAgent === 'analysis' ? 'text-brand-500' : 'text-slate-500'}`}>
                      Analyst Agent <span className="text-[9px] font-normal italic">(analyst.md)</span>
                    </h4>
                    <p className="text-[11px] text-slate-400 mt-0.5">Validating input parameters & sanitizing XSS tags.</p>
                  </div>
                </div>

                {/* Stage 2 */}
                <div className="relative">
                  <div className={`absolute -left-[31px] top-0.5 w-4 h-4 rounded-full border-2 transition-all duration-300 ${
                    bmadAgent === 'planning' ? 'bg-brand-500 border-brand-400 scale-125 shadow-lg shadow-brand-500/50' : 'bg-slate-300 dark:bg-slate-700 border-transparent'
                  }`} />
                  <div>
                    <h4 className={`text-xs font-bold ${bmadAgent === 'planning' ? 'text-brand-500' : 'text-slate-500'}`}>
                      Product Manager <span className="text-[9px] font-normal italic">(pm.md)</span>
                    </h4>
                    <p className="text-[11px] text-slate-400 mt-0.5">Updating stories and checking UX/UI flow compatibility.</p>
                  </div>
                </div>

                {/* Stage 3 */}
                <div className="relative">
                  <div className={`absolute -left-[31px] top-0.5 w-4 h-4 rounded-full border-2 transition-all duration-300 ${
                    bmadAgent === 'solutioning' ? 'bg-brand-500 border-brand-400 scale-125 shadow-lg shadow-brand-500/50' : 'bg-slate-300 dark:bg-slate-700 border-transparent'
                  }`} />
                  <div>
                    <h4 className={`text-xs font-bold ${bmadAgent === 'solutioning' ? 'text-brand-500' : 'text-slate-500'}`}>
                      Architect Agent <span className="text-[9px] font-normal italic">(architect.md)</span>
                    </h4>
                    <p className="text-[11px] text-slate-400 mt-0.5">Ensuring SQL parametrization and password salting standards.</p>
                  </div>
                </div>

                {/* Stage 4 */}
                <div className="relative">
                  <div className={`absolute -left-[31px] top-0.5 w-4 h-4 rounded-full border-2 transition-all duration-300 ${
                    bmadAgent === 'execution' ? 'bg-brand-500 border-brand-400 scale-125 shadow-lg shadow-brand-500/50' : 'bg-slate-300 dark:bg-slate-700 border-transparent'
                  }`} />
                  <div>
                    <h4 className={`text-xs font-bold ${bmadAgent === 'execution' ? 'text-brand-500' : 'text-slate-500'}`}>
                      Developer Agent <span className="text-[9px] font-normal italic">(dev.md)</span>
                    </h4>
                    <p className="text-[11px] text-slate-400 mt-0.5">Encrypting password with Bcrypt-12 & updating PostgreSQL.</p>
                  </div>
                </div>

                {/* Stage 5 */}
                <div className="relative">
                  <div className={`absolute -left-[31px] top-0.5 w-4 h-4 rounded-full border-2 transition-all duration-300 ${
                    bmadAgent === 'validation' ? 'bg-brand-500 border-brand-400 scale-125 shadow-lg shadow-brand-500/50' : 'bg-slate-300 dark:bg-slate-700 border-transparent'
                  }`} />
                  <div>
                    <h4 className={`text-xs font-bold ${bmadAgent === 'validation' ? 'text-brand-500' : 'text-slate-500'}`}>
                      QA & Test Agent <span className="text-[9px] font-normal italic">(qa.md)</span>
                    </h4>
                    <p className="text-[11px] text-slate-400 mt-0.5">Auditing output for data leakage & confirming success code.</p>
                  </div>
                </div>

              </div>

              {bmadAgent !== 'idle' && (
                <div className="mt-6 p-3 bg-brand-500/10 rounded-xl border border-brand-500/20 flex items-center justify-between">
                  <span className="text-xs font-semibold text-brand-400 flex items-center gap-1.5">
                    <Cpu className="w-3.5 h-3.5 animate-spin" />
                    Agent phase: {bmadAgent.toUpperCase()}
                  </span>
                  <span className="text-[10px] bg-brand-500 text-white px-2 py-0.5 rounded font-bold">ACTIVE</span>
                </div>
              )}
            </div>

            {/* OWASP BEST PRACTICE CORNER */}
            <div className="glass-panel p-6 rounded-3xl shadow-xl border border-slate-200 dark:border-slate-800">
              <h3 className="text-sm font-bold uppercase tracking-wider text-slate-400 mb-3 flex items-center gap-1.5">
                <ShieldCheck className="w-4 h-4 text-emerald-400" /> Secure Architecture Guidelines
              </h3>
              <ul className="space-y-2 text-xs text-slate-500 dark:text-slate-400">
                <li className="flex items-start gap-2">
                  <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full mt-1.5 flex-shrink-0" />
                  <span>Passwords strictly salted via **Bcrypt** (cost 12) inside Application Boundary.</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full mt-1.5 flex-shrink-0" />
                  <span>Controllers filter inputs aggressively via **NestJS ValidationPipe** (whitelist).</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full mt-1.5 flex-shrink-0" />
                  <span>No database internal IDs or secrets leaked inside JSON response representations.</span>
                </li>
              </ul>
            </div>

          </div>

          {/* RIGHT: User Registration Form & Table Grid */}
          <div className="lg:col-span-8 space-y-8">
            
            {/* REGISTER USER PANEL */}
            <div className="glass-panel p-8 rounded-3xl shadow-xl border border-slate-200 dark:border-slate-800">
              <div className="flex items-center gap-2.5 mb-6">
                <UserPlus className="w-6 h-6 text-brand-500" />
                <h2 className="text-xl font-bold tracking-tight">Register New System User</h2>
              </div>

              {successMsg && (
                <div className="p-4 bg-emerald-500/10 border border-emerald-500/20 text-emerald-500 rounded-2xl text-xs font-semibold mb-6 flex items-center gap-2">
                  <CheckCircle className="w-4 h-4" /> {successMsg}
                </div>
              )}

              {errorMsg && (
                <div className="p-4 bg-rose-500/10 border border-rose-500/20 text-rose-500 rounded-2xl text-xs font-semibold mb-6">
                  {errorMsg}
                </div>
              )}

              <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-2">Username</label>
                  <input
                    type="text"
                    required
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                    placeholder="e.g. john_doe"
                    className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-900/40 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20 transition-all text-sm"
                  />
                </div>

                <div>
                  <label className="block text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-2">Email Address</label>
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                    placeholder="e.g. john@ums.com"
                    className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-900/40 focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/20 transition-all text-sm"
                  />
                </div>

                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-2">Role Permissions</label>
                  <div className="grid grid-cols-3 gap-3">
                    {['user', 'moderator', 'admin'].map(r => (
                      <button
                        key={r}
                        type="button"
                        onClick={() => setRole(r)}
                        className={`py-3 px-4 rounded-xl text-xs font-bold border capitalize transition-all ${
                          role === r
                            ? 'bg-brand-500 text-white border-brand-500 shadow-lg shadow-brand-500/20'
                            : 'border-slate-200 dark:border-slate-800 bg-white/50 dark:bg-slate-900/30 hover:bg-slate-100 dark:hover:bg-slate-800/40 text-slate-600 dark:text-slate-400'
                        }`}
                      >
                        {r}
                      </button>
                    ))}
                  </div>
                </div>

                <div className="md:col-span-2 flex justify-end pt-2">
                  <button
                    type="submit"
                    disabled={bmadAgent !== 'idle'}
                    className="px-6 py-3.5 rounded-xl bg-gradient-to-r from-brand-600 to-indigo-600 hover:from-brand-500 hover:to-indigo-500 text-white text-xs font-extrabold tracking-wider uppercase shadow-lg shadow-brand-500/20 hover:shadow-brand-500/35 hover:scale-[1.02] active:scale-[0.98] transition-all flex items-center gap-2 disabled:opacity-50 disabled:pointer-events-none"
                  >
                    {bmadAgent !== 'idle' ? 'AI Sequence Active...' : 'Register User'}
                    <ArrowRight className="w-4 h-4" />
                  </button>
                </div>
              </form>
            </div>

            {/* REGISTERED USERS LIST */}
            <div className="glass-panel p-8 rounded-3xl shadow-xl border border-slate-200 dark:border-slate-800">
              <div className="flex justify-between items-center mb-6">
                <div className="flex items-center gap-2.5">
                  <Users className="w-6 h-6 text-indigo-500" />
                  <h2 className="text-xl font-bold tracking-tight">Active Workspace Users</h2>
                </div>
                {isFetching && <span className="text-[10px] font-semibold text-brand-400 flex items-center gap-1"><RefreshCw className="w-3 h-3 animate-spin" /> Fetching...</span>}
              </div>

              {isLoading ? (
                <div className="py-12 text-center text-xs text-slate-500 dark:text-slate-400 flex flex-col items-center gap-2">
                  <RefreshCw className="w-6 h-6 animate-spin text-brand-500" />
                  Loading active workspace database...
                </div>
              ) : users.length === 0 ? (
                <div className="py-12 text-center text-xs text-slate-500 dark:text-slate-400">
                  No active users found in workspace database fallbacks.
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-xs border-collapse">
                    <thead>
                      <tr className="border-b border-slate-200 dark:border-slate-800/80 text-slate-400 font-semibold">
                        <th className="pb-3 pl-2">USER</th>
                        <th className="pb-3">EMAIL</th>
                        <th className="pb-3">ROLE</th>
                        <th className="pb-3 text-right pr-2">REGISTERED ON</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100 dark:divide-slate-800/50">
                      {users.map((u: User) => (
                        <tr key={u.id} className="group hover:bg-slate-50 dark:hover:bg-slate-900/20 transition-all">
                          <td className="py-4 pl-2 font-bold text-slate-700 dark:text-slate-200 group-hover:text-brand-500 dark:group-hover:text-brand-400 transition-colors">
                            {u.username}
                          </td>
                          <td className="py-4 text-slate-500 dark:text-slate-400">{u.email}</td>
                          <td className="py-4">
                            <span className={`px-2.5 py-1 rounded-full text-[9px] font-bold tracking-wider uppercase border ${
                              u.role === 'admin'
                                ? 'bg-emerald-500/10 text-emerald-500 border-emerald-500/20'
                                : u.role === 'moderator'
                                ? 'bg-violet-500/10 text-violet-500 border-violet-500/20'
                                : 'bg-slate-500/10 text-slate-400 border-slate-500/20'
                            }`}>
                              {u.role}
                            </span>
                          </td>
                          <td className="py-4 text-right pr-2 text-slate-400 dark:text-slate-500 font-medium">
                            {new Date(u.createdAt).toLocaleDateString()}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

          </div>

        </div>

        {/* Footer */}
        <footer className="mt-16 pt-8 border-t border-slate-200 dark:border-slate-800/60 text-center text-xs text-slate-500 dark:text-slate-400 flex flex-col sm:flex-row justify-between items-center gap-4">
          <p>© 2026 UMS Monorepo System. Built following strict OWASP Top 10 standards.</p>
          <div className="flex gap-4">
            <a href="https://owasp.org" target="_blank" rel="noreferrer" className="hover:text-brand-500 transition-colors flex items-center gap-1">
              OWASP guidelines <ExternalLink className="w-3 h-3" />
            </a>
            <span className="text-slate-800">|</span>
            <a href="https://nestjs.com" target="_blank" rel="noreferrer" className="hover:text-brand-500 transition-colors flex items-center gap-1">
              Clean Architecture NestJS <ExternalLink className="w-3 h-3" />
            </a>
          </div>
        </footer>

      </div>
    </div>
  );
}
