import { Link, Navigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export function HomePage() {
  const { isAuthenticated } = useAuth()

  if (isAuthenticated) {
    return <Navigate replace to="/feed" />
  }

  return (
    <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col justify-center px-6 py-12">
      <div className="max-w-2xl">
        <p className="text-sm font-medium uppercase tracking-wide text-emerald-700">
          BadmintonCourtBooking
        </p>
        <h1 className="mt-4 text-4xl font-semibold text-gray-950 sm:text-5xl">
          Authentication module
        </h1>
        <p className="mt-5 text-base leading-7 text-gray-600">
          Phase 1 focuses on account registration, login, logout, and current
          user state through ASP.NET Core Identity cookie authentication.
        </p>

        <div className="mt-8 flex flex-col gap-3 sm:flex-row">
          <Link
            className="inline-flex items-center justify-center rounded bg-emerald-700 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
            to="/register"
          >
            Create account
          </Link>
          <Link
            className="inline-flex items-center justify-center rounded border border-gray-300 bg-white px-4 py-2.5 text-sm font-medium text-gray-800 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
            to="/login"
          >
            Sign in
          </Link>
        </div>
      </div>
    </main>
  )
}
