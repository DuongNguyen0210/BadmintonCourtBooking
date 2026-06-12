import { apiClient } from './client'
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserResponse,
} from '../types/auth'

export async function register(request: RegisterRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/register', request)
  return response.data
}

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/login', request)
  return response.data
}

export async function logout(): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/api/auth/logout')
  return response.data
}

export async function getCurrentUser(): Promise<UserResponse> {
  const response = await apiClient.get<UserResponse>('/api/auth/me')
  return response.data
}
