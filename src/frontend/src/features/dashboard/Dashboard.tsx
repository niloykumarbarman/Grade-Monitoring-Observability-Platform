import { GraduationCap, TrendingUp, BookOpen } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts'
import { gradesApi } from '../../services/gradesApi'
import { GradeRecord } from '../../types'

function StatCard({ label, value, icon, accent, textColor }: {
  label: string; value: string | number; icon: React.ReactNode; accent: string; textColor: string
}) {
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

const GRADE_COLORS: Record<string, string> = {
  'A+': '#10b981', 'A': '#34d399', 'B': '#3b82f6',
  'C': '#f59e0b', 'D': '#f97316', 'F': '#ef4444'
}

export default function Dashboard() {
  const { data: grades = [], isLoading } = useQuery<GradeRecord[]>({
    queryKey: ['grades', 'STU001'],
    queryFn: () => gradesApi.getByStudent('STU001'),
  })

  const avg = grades.length
    ? (grades.reduce((s, g) => s + g.score, 0) / grades.length).toFixed(1)
    : '-'

  // Bar chart — score per course
  const barData = grades.map(g => ({
    name: g.courseName.length > 10 ? g.courseName.slice(0, 10) + '…' : g.courseName,
    score: g.score,
  }))

  // Pie chart — grade distribution
  const gradeCounts: Record<string, number> = {}
  grades.forEach(g => { gradeCounts[g.grade] = (gradeCounts[g.grade] || 0) + 1 })
  const pieData = Object.entries(gradeCounts).map(([grade, count]) => ({ name: grade, value: count }))

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-gray-800">Dashboard</h2>
        <p className="text-gray-500 text-sm mt-1">Grade Monitoring & Observability Platform</p>
      </div>

      {/* Stat Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        <StatCard
          label="Total Records"
          value={isLoading ? '...' : grades.length}
          icon={<GraduationCap size={18} className="text-blue-600" />}
          accent="bg-blue-500"
          textColor="text-blue-600"
        />
        <StatCard
          label="Average Score"
          value={isLoading ? '...' : avg}
          icon={<TrendingUp size={18} className="text-emerald-600" />}
          accent="bg-emerald-500"
          textColor="text-emerald-600"
        />
        <StatCard
          label="Courses"
          value={isLoading ? '...' : new Set(grades.map(g => g.courseId)).size}
          icon={<BookOpen size={18} className="text-purple-600" />}
          accent="bg-purple-500"
          textColor="text-purple-600"
        />
      </div>

      {/* Charts */}
      {!isLoading && grades.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          {/* Bar Chart */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h3 className="font-semibold text-gray-700 mb-4">Score by Course</h3>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={barData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis domain={[0, 100]} tick={{ fontSize: 12 }} />
                <Tooltip />
                <Bar dataKey="score" fill="#3b82f6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Pie Chart */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h3 className="font-semibold text-gray-700 mb-4">Grade Distribution</h3>
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie data={pieData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={80} label>
                  {pieData.map((entry) => (
                    <Cell key={entry.name} fill={GRADE_COLORS[entry.name] || '#94a3b8'} />
                  ))}
                </Pie>
                <Legend />
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}
    </div>
  )
}
