import axios from 'axios'
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import * as authApi from '../api/auth'
import { AuthContext, type AuthContextValue } from './authContext'
import type { LoginRequest, RegisterRequest, UserResponse } from '../types/auth'
import type { ReactNode } from 'react'

type AuthProviderProps = {
  children: ReactNode
}

function isUnauthorized(error: unknown) {
  return axios.isAxiosError(error) && error.response?.status === 401
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const refreshUser = useCallback(async () => {
    try {
      const currentUser = await authApi.getCurrentUser()
      setUser(currentUser)
      return currentUser
    } catch (error) {
      if (isUnauthorized(error)) {
        setUser(null)
        return null
      }

      throw error
    }
  }, [])

  useEffect(() => {
    let isMounted = true

    async function loadUser() {
      try {
        const currentUser = await authApi.getCurrentUser()
        if (isMounted) {
          setUser(currentUser)
        }
      } catch (error) {
        if (isMounted && isUnauthorized(error)) {
          setUser(null)
          return
        }

        if (isMounted) {
          console.error(error)
          setUser(null)
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    void loadUser()

    return () => {
      isMounted = false
    }
  }, [])

  const login = useCallback(async (request: LoginRequest) => {
    const response = await authApi.login(request)
    setUser(response.user)
    return response
  }, [])

  const register = useCallback(async (request: RegisterRequest) => {
    const response = await authApi.register(request)
    return response
  }, [])

  const logout = useCallback(async () => {
    await authApi.logout()
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      isLoading,
      login,
      register,
      logout,
      refreshUser,
    }),
    [isLoading, login, logout, refreshUser, register, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
