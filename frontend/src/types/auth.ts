export type UserResponse = {
  id: string
  email: string
  fullName: string
}

export type RegisterRequest = {
  email: string
  password: string
  confirmPassword: string
  fullName: string
}

export type LoginRequest = {
  email: string
  password: string
  rememberMe: boolean
}

export type AuthResponse = {
  success: boolean
  message: string
  user: UserResponse | null
}
