export interface Alert {
  id: string
  title: string
  message: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  isActive: boolean
  createdAt: string
}

export interface User {
  id: string
  email: string
  fullName: string
  role: string
}

export interface ApiResponse<T> {
  data: T
  success: boolean
  message: string
}
