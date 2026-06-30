import axios from 'axios'
import { GradeRecord } from '../types'

const api = axios.create({
  baseURL: import.meta.env.VITE_GRADE_API_URL
    ? `${import.meta.env.VITE_GRADE_API_URL}/api`
    : 'http://localhost:5050/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 3000,
})

export const DEMO_GRADES: GradeRecord[] = [
  { id: '1', studentId: 'STU001', courseId: 'CS101', courseName: 'Algorithms', score: 92, grade: 'A+', recordedAt: '2026-06-01', recordedBy: 'admin' },
  { id: '2', studentId: 'STU001', courseId: 'MATH201', courseName: 'Calculus', score: 78, grade: 'B', recordedAt: '2026-06-02', recordedBy: 'admin' },
  { id: '3', studentId: 'STU001', courseId: 'ENG301', courseName: 'English', score: 85, grade: 'A', recordedAt: '2026-06-03', recordedBy: 'admin' },
  { id: '4', studentId: 'STU001', courseId: 'PHY101', courseName: 'Physics', score: 60, grade: 'C', recordedAt: '2026-06-04', recordedBy: 'admin' },
  { id: '5', studentId: 'STU001', courseId: 'CS202', courseName: 'Databases', score: 95, grade: 'A+', recordedAt: '2026-06-05', recordedBy: 'admin' },
]

export const gradesApi = {
  getByStudent: async (studentId: string): Promise<GradeRecord[]> => {
    try {
      const res = await api.get<GradeRecord[]>(`/grades/student/${studentId}`)
      return res.data
    } catch {
      return DEMO_GRADES
    }
  },
  create: async (data: {
    studentId: string
    courseId: string
    courseName: string
    score: number
    recordedBy: string
  }): Promise<{ id: string }> => {
    const res = await api.post<{ id: string }>('/grades', data)
    return res.data
  },
}
