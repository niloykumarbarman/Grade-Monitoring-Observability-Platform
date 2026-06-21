import axios from 'axios'
import { GradeRecord } from '../types'

const api = axios.create({
  baseURL: 'http://localhost:5050/api',
  headers: { 'Content-Type': 'application/json' },
})

export const gradesApi = {
  getByStudent: async (studentId: string): Promise<GradeRecord[]> => {
    const res = await api.get<GradeRecord[]>(`/grades/student/${studentId}`)
    return res.data
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
