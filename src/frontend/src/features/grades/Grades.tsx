import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { gradesApi } from '../../services/gradesApi'
import { GradeRecord } from '../../types'

const gradeColor: Record<string, string> = {
  'A+': 'bg-emerald-100 text-emerald-700',
  'A':  'bg-green-100 text-green-700',
  'B':  'bg-blue-100 text-blue-700',
  'C':  'bg-yellow-100 text-yellow-700',
  'D':  'bg-orange-100 text-orange-700',
  'F':  'bg-red-100 text-red-700',
}

export default function Grades() {
  const queryClient = useQueryClient()
  const [studentId, setStudentId] = useState('STU001')
  const [search, setSearch] = useState('STU001')
  const [form, setForm] = useState({
    studentId: '', courseId: '', courseName: '', score: '', recordedBy: ''
  })

  const { data: grades = [], isLoading } = useQuery<GradeRecord[]>({
    queryKey: ['grades', search],
    queryFn: () => gradesApi.getByStudent(search),
    enabled: !!search,
  })

  const mutation = useMutation({
    mutationFn: gradesApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['grades'] })
      setForm({ studentId: '', courseId: '', courseName: '', score: '', recordedBy: '' })
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    mutation.mutate({ ...form, score: parseFloat(form.score) })
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-gray-800">Grades</h2>
        <p className="text-gray-500 text-sm mt-1">Student grade records</p>
      </div>

      {/* Add Grade Form */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <h3 className="font-semibold text-gray-700 mb-4">Add New Grade</h3>
        <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-3">
          {[
            { key: 'studentId', placeholder: 'Student ID' },
            { key: 'courseId', placeholder: 'Course ID' },
            { key: 'courseName', placeholder: 'Course Name' },
            { key: 'score', placeholder: 'Score (0-100)', type: 'number' },
            { key: 'recordedBy', placeholder: 'Recorded By' },
          ].map(({ key, placeholder, type }) => (
            <input
              key={key}
              type={type || 'text'}
              placeholder={placeholder}
              value={form[key as keyof typeof form]}
              onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))}
              className="border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          ))}
          <button
            type="submit"
            disabled={mutation.isPending}
            className="col-span-2 bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {mutation.isPending ? 'Saving...' : 'Add Grade'}
          </button>
        </form>
      </div>

      {/* Search */}
      <div className="flex gap-2">
        <input
          type="text"
          placeholder="Search by Student ID..."
          value={studentId}
          onChange={e => setStudentId(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-2 text-sm flex-1 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button
          onClick={() => setSearch(studentId)}
          className="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-slate-700"
        >
          Search
        </button>
      </div>

      {/* Grade Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        {isLoading ? (
          <div className="p-8 text-center text-gray-400">Loading...</div>
        ) : grades.length === 0 ? (
          <div className="p-8 text-center text-gray-400">No grades found for "{search}"</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-100">
              <tr>
                {['Student ID', 'Course', 'Score', 'Grade', 'Recorded By', 'Date'].map(h => (
                  <th key={h} className="text-left px-4 py-3 text-gray-500 font-medium">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {grades.map(g => (
                <tr key={g.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-700">{g.studentId}</td>
                  <td className="px-4 py-3 text-gray-600">{g.courseName}</td>
                  <td className="px-4 py-3 text-gray-600">{g.score}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${gradeColor[g.grade] || 'bg-gray-100 text-gray-600'}`}>
                      {g.grade}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500">{g.recordedBy}</td>
                  <td className="px-4 py-3 text-gray-400">{new Date(g.recordedAt).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
