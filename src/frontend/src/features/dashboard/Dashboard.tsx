import { useEffect, useState } from 'react'
import api from '../../services/api'

interface AlertSummary {
  totalAlerts: number
  firingAlerts: number
  systemHealth: string
}

export default function Dashboard() {
  const [summary, setSummary] = useState<AlertSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .get<AlertSummary>('/alerts/summary')
      .then((res) => setSummary(res.data))
      .catch(() => setError('Could not load data from API'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-800 mb-4">Dashboard</h2>

      {error && (
        <p className="text-red-600 mb-4">{error}</p>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-500 text-sm">Total Alerts</p>
          <p className="text-3xl font-bold text-blue-600">
            {loading ? '...' : summary?.totalAlerts ?? '-'}
          </p>
        </div>
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-500 text-sm">Firing Alerts</p>
          <p className="text-3xl font-bold text-green-600">
            {loading ? '...' : summary?.firingAlerts ?? '-'}
          </p>
        </div>
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-500 text-sm">System Health</p>
          <p className="text-3xl font-bold text-emerald-600">
            {loading ? '...' : summary?.systemHealth ?? '-'}
          </p>
        </div>
      </div>
    </div>
  )
}
