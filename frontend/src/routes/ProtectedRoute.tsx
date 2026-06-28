import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../contexts/useAuth'
import type { ReactNode } from 'react'

type ProtectedRouteProps = {
  children: ReactNode
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return (
      <main className="mx-auto flex min-h-[calc(100vh-4rem)] w-full max-w-5xl items-center px-6 py-12">
        <div
          aria-busy="true"
          className="rounded border border-gray-200 bg-white px-4 py-3 text-sm text-gray-600"
        >
          Checking your session...
        </div>
      </main>
    )
  }

  if (!isAuthenticated) {
    return <Navigate replace state={{ from: location }} to="/login" />
  }

  return children
}
