import { Outlet, Link } from 'react-router-dom'

export default function Layout() {
  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 py-3 flex gap-6">
          <h1 className="font-bold text-blue-600">Grade Monitor</h1>
          <Link to="/" className="text-gray-600 hover:text-blue-600">Dashboard</Link>
          <Link to="/alerts" className="text-gray-600 hover:text-blue-600">Alerts</Link>
        </div>
      </nav>
      <main className="max-w-7xl mx-auto px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
