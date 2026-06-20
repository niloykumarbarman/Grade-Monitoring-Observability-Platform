import { Outlet, Link, useLocation } from 'react-router-dom'
import { GraduationCap, LayoutDashboard, Bell } from 'lucide-react'

export default function Layout() {
  const { pathname } = useLocation()

  const navItem = (to: string, label: string, icon: React.ReactNode) => {
    const active = pathname === to
    return (
      <Link
        to={to}
        className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
          active
            ? 'bg-white/10 text-white'
            : 'text-slate-300 hover:text-white hover:bg-white/5'
        }`}
      >
        {icon}
        {label}
      </Link>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-gradient-to-r from-slate-900 to-slate-800 shadow-md">
        <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <GraduationCap className="text-blue-400" size={24} />
            <h1 className="font-bold text-white text-lg tracking-tight">
              Grade Monitor
            </h1>
            <span className="flex items-center gap-1 ml-2 px-2 py-0.5 rounded-full bg-emerald-500/10 text-emerald-400 text-xs font-medium">
              <span className="relative flex h-2 w-2">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
              </span>
              Live
            </span>
          </div>
          <div className="flex gap-1">
            {navItem('/', 'Dashboard', <LayoutDashboard size={16} />)}
            {navItem('/alerts', 'Alerts', <Bell size={16} />)}
          </div>
        </div>
      </nav>
      <main className="max-w-7xl mx-auto px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
