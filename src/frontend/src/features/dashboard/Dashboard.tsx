import { useEffect, useState } from 'react'
import { AlertTriangle, Flame, HeartPulse } from 'lucide-react'
import api from '../../services/api'

interface AlertSummary {
  totalAlerts: number
  firingAlerts: number
  systemHealth: string
}

interface StatCardProps {
  label: string
  value: string | number
  icon: React.ReactNode
  accent: string
  textColor: string
}

function StatCard({ label, value, icon, accent, textColor }: StatCardProps) {
  return (
    <div className="relative bg-white rounded-xl shadow-sm border border-gray-100 p-6 overflow-hidden hover:shadow-md transition-shadow">
      <div className={`absolute top-0 left-0 h-1 w-full ${accent}`} />
      <div className="flex items-center justify-between mb-3">
        <p className="text-gray-500 text-sm font-medium">{label}</p>
        <div className={`p-2 rounded-lg ${accent} bg-opacity-10`}>{icon}</div>
      </div>
      <p className={`text-3xl font-bold ${textColor}`}>{value}</p>
    </div>
  )
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
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-800">Dashboard</h2>
        <p className="text-gray-500 text-sm mt-1">
          Real-time system overview, powered by the live API
        </p>
      </div>

      {error && (
        <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 text-red-600 text-sm border border-red-100">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        <StatCard
          label="Total Alerts"
          value={loading ? '...' : summary?.totalAlerts ?? '-'}
          icon={<AlertTriangle size={18} className="text-blue-600" />}
          accent="bg-blue-500"
          textColor="text-blue-600"
        />
        <StatCard
          label="Firing Alerts"
          value={loading ? '...' : summary?.firingAlerts ?? '-'}
          icon={<Flame size={18} className="text-orange-600" />}
          accent="bg-orange-500"
          textColor="text-orange-600"
        />
        <StatCard
          label="System Health"
          value={loading ? '...' : summary?.systemHealth ?? '-'}
          icon={<HeartPulse size={18} className="text-emerald-600" />}
          accent="bg-emerald-500"
          textColor="text-emerald-600"
        />
      </div>
    </div>
  )
}
