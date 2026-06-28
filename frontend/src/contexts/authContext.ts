import { createContext } from 'react'
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserResponse,
} from '../types/auth'

export type AuthContextValue = {
  user: UserResponse | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (request: LoginRequest) => Promise<AuthResponse>
  register: (request: RegisterRequest) => Promise<AuthResponse>
  logout: () => Promise<void>
  refreshUser: () => Promise<UserResponse | null>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
