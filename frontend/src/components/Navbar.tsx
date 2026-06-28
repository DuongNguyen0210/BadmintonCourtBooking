import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export function Navbar() {
  const { isAuthenticated, isLoading, logout, user } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <header className="border-b border-gray-200 bg-white">
      <nav className="mx-auto flex min-h-16 w-full max-w-5xl flex-col gap-3 px-6 py-4 sm:flex-row sm:items-center sm:justify-between">
        <Link className="text-base font-semibold text-gray-950" to="/">
          BadmintonCourtBooking
        </Link>

        <div className="flex flex-wrap items-center gap-3 text-sm">
          <NavLink
            className={({ isActive }) =>
              isActive ? 'font-medium text-emerald-700' : 'text-gray-600 hover:text-gray-950'
            }
            to="/"
          >
            Home
          </NavLink>

          {isLoading ? (
            <span className="text-gray-500">Checking session...</span>
          ) : isAuthenticated ? (
            <>
              <NavLink
                className={({ isActive }) =>
                  isActive
                    ? 'font-medium text-emerald-700'
                    : 'text-gray-600 hover:text-gray-950'
                }
                to="/feed"
              >
                Feed
              </NavLink>
              <span className="max-w-48 truncate text-gray-600">
                {user?.fullName || user?.email}
              </span>
              <button
                className="rounded border border-gray-300 bg-white px-3 py-1.5 font-medium text-gray-800 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
                onClick={handleLogout}
                type="button"
              >
                Logout
              </button>
            </>
          ) : (
            <>
              <NavLink
                className={({ isActive }) =>
                  isActive
                    ? 'font-medium text-emerald-700'
                    : 'text-gray-600 hover:text-gray-950'
                }
                to="/login"
              >
                Login
              </NavLink>
              <Link
                className="rounded bg-emerald-700 px-3 py-1.5 font-medium text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2"
                to="/register"
              >
                Register
              </Link>
            </>
          )}
        </div>
      </nav>
    </header>
  )
}
